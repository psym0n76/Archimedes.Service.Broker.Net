﻿using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.RabbitMq;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcessPrice
    {
        private readonly IProducerFanout<PriceMessage> _fanoutProducer;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BrokerProcessPrice(IProducerFanout<PriceMessage> fanoutProducer)
        {
            _fanoutProducer = fanoutProducer;
        }

        public void Run(PriceMessage request)
        {
            Task.Run(() =>
            {
                var session = BrokerSession.GetInstance();

                _logger.Info($"Process Price Update: {request}");

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error("Unable to connect to FCXM");
                    return;
                }

                if (SubscribedMarkets.IsSubscribed(request.Market))
                {
                    _logger.Info($"Process Price Request: ALREADY SUBSCRIBED  {request.Market}");
                    return;
                }

                session.SubscribeSymbol(request.Market);

                SubscribedMarkets.Add(request.Market);

                _logger.Info($"Process Price Request: SUBSCRIBED {request.Market} - no logs are published");

                request.Prices = new List<PriceDto>();

                session.PriceUpdate += priceUpdate =>
                {
                    if (SubscribedMarkets.IsSubscribed(request.Market))
                    {
                        ProcessMessage(request, priceUpdate);
                    }
                    else
                    {
                        _logger.Info($"Process Price Request: UNSUBSCRIBED {request.Market}");
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

            //_producer.PublishMessage(request, "PriceResponseQueue");
            _fanoutProducer.PublishMessage(request,"Archimedes_Price");
            request.Prices = new List<PriceDto>();
        }
    }
}