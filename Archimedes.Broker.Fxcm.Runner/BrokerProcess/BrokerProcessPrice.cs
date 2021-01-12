using System;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Logger;
using Archimedes.Library.RabbitMq;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcessPrice
    {
        private readonly IProducerFanout<PriceMessage> _fanoutProducer;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public BrokerProcessPrice(IProducerFanout<PriceMessage> fanoutProducer)
        {
            _fanoutProducer = fanoutProducer;
        }

        public void Run(PriceMessage request)
        {
            try
            {
                SubscribeToPrice(request);
            }
            catch (Exception e)
            {
                _logger.Error($"Error returned from BrokerProcesPrice \n\n{e.Message}\n\n{e.StackTrace}");
            }
        }

        private void SubscribeToPrice(PriceMessage request)
        {
            Task.Run(() =>
            {
                _logId = _batchLog.Start();
                _batchLog.Update(_logId, $"Subscribing to {request.Market} for Prices");

                var session = BrokerSession.GetInstance();

                _batchLog.Update(_logId, $"Instance {request.Market} for Prices");

                if (session.State == SessionState.Disconnected)
                {
                    _batchLog.Update(_logId, $"Reconnecting {session.State}");
                    session.Connect();
                }

                _batchLog.Update(_logId, $"Connection status: {session.State}");

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error(_batchLog.Print(_logId, "Unable to connect to FXCM"));
                    return;
                }

                if (SubscribedMarkets.IsSubscribed(request.Market))
                {
                    _logger.Info(_batchLog.Print(_logId, $"ALREADY SUBSCRIBED  to {request.Market}"));
                    return;
                }

                session.SubscribeSymbol(request.Market);

                SubscribedMarkets.Add(request.Market);

                _logger.Info(_batchLog.Print(_logId, $"SUBSCRIBED {request.Market} - NO logs are published"));

                request.Prices = new List<PriceDto>();

                session.PriceUpdate += priceUpdate =>
                {
                    if (SubscribedMarkets.IsSubscribed(request.Market))
                    {
                        ProcessMessage(request, priceUpdate);
                    }
                    else
                    {
                        _logger.Info($"UNSUBSCRIBED {request.Market}");
                        session.UnsubscribeSymbol(request.Market);
                    }
                };

                while (true)
                {
                    Thread.Sleep(3000);
                }
            }).ConfigureAwait(false);
        }


        private void ProcessMessage(PriceMessage request, PriceUpdate priceUpdate)
        {
            request.Prices.Add(new PriceDto()
            {
                Market = request.Market,
                Bid = decimal.Parse(priceUpdate.Bid.ToString(CultureInfo.InvariantCulture)),
                Ask = decimal.Parse(priceUpdate.Ask.ToString(CultureInfo.InvariantCulture)),
                TimeStamp = priceUpdate.Updated,
                Granularity = "0Min",
            });

            _fanoutProducer.PublishMessage(request,"Archimedes_Price");
            
            //clears down the list
            request.Prices = new List<PriceDto>();
        }
    }
}