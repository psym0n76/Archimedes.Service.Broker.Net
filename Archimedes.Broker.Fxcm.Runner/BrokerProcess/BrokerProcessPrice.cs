using System;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.RabbitMq;
using NLog;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcessPrice
    {
        private readonly IProducer<PriceMessage> _producer;

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public BrokerProcessPrice(IProducer<PriceMessage> producer)
        {
            _producer = producer;
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

                session.SubscribeSymbol(request.Market);

                _logger.Info($"Process Price Request: SUBSCRIBED {request.Market} - no logs are published");

                request.Prices = new List<PriceDto>();

                session.PriceUpdate += priceUpdate =>
                {
                    ProcessMessage(request, priceUpdate);
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
                BidOpen = priceUpdate.Bid,
                BidClose = priceUpdate.Bid,
                BidHigh = priceUpdate.Bid,
                BidLow = priceUpdate.Bid,

                AskOpen = priceUpdate.Ask,
                AskClose = priceUpdate.Ask,
                AskHigh = priceUpdate.Ask,
                AskLow = priceUpdate.Ask,

                TimeStamp = priceUpdate.Updated,
                LastUpdated = DateTime.Now,
                Granularity = "0Min",
                
            });

            _producer.PublishMessage(request, "PriceResponseQueue");
            request.Prices = new List<PriceDto>();
        }
    }
}