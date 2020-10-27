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

                request.Prices = new List<PriceDto>();

                var counter = 0;

                session.PriceUpdate += priceUpdate => { ProcessMessage(request, counter, priceUpdate); };

                while (true)
                {
                    Thread.Sleep(3000);
                }

            }).ConfigureAwait(false);
        }

        private void ProcessMessage(PriceMessage request, int counter, PriceUpdate priceUpdate)
        {
            counter++;
            if (counter < 5)
            {
                _logger.Info(
                    $"Process Price Update: receievd update {priceUpdate.Ask} : {priceUpdate.Bid} : {priceUpdate.High} : {priceUpdate.Low} : {priceUpdate.Symbol} : {priceUpdate.Updated}");
            }

            else if (counter == 500)
            {
                _logger.Info($"Process Price Update: receievd 500 updates ");
                counter = 0;
            }

            request.Prices.Add(new PriceDto()
            {
                BidOpen = priceUpdate.Bid,
                BidClose = priceUpdate.Bid,

                AskOpen = priceUpdate.Ask,
                AskClose = priceUpdate.Ask,

                Market = request.Market,
                TimeStamp = priceUpdate.Updated,
                LastUpdated = DateTime.Now
            });

            _producer.PublishMessage(request, "PriceResponseQueue");

            if (counter < 5)
            {
                _logger.Info($"Published to Queue: {request}");
            }
        }
    }
}