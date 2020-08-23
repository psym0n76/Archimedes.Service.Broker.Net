using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using System.Collections.Generic;
using System.Threading.Tasks;
using Archimedes.Library.RabbitMq;

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

                _logger.Info("Process Price Update");

                if (session.State == SessionState.Disconnected)
                {
                    session.Connect();
                }

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error("Unalbe to connect to FCXM");
                    return;
                }

                session.SubscribeSymbol(request.Market);
                //session.PriceUpdate += Session_PriceUpdate;

                session.PriceUpdate += priceUpdate =>
                {
                    var priceResponse = new PriceMessage()
                    {
                        Prices = new List<PriceDto>()
                        {
                            new PriceDto()
                            {
                                BidOpen = priceUpdate.Bid,
                                BidClose = priceUpdate.Bid,

                                AskOpen = priceUpdate.Ask,
                                AskClose = priceUpdate.Ask,

                                Market = request.Market
                            }
                        }
                    };

                    _producer.PublishMessage(priceResponse, nameof(priceResponse));
                };

                while (true)
                {
                    //loop tpo keep session open
                }

                //session.PriceUpdate -= Session_PriceUpdate;
                //session.UnsubscribeSymbol(request.Market);
            }).ConfigureAwait(false);
        }
    }
}