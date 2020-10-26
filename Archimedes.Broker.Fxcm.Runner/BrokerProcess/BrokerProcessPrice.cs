using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.Broker.Fxcm;
using System.Collections.Generic;
using System.Threading;
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

                session.PriceUpdate += priceUpdate =>
                {
                    
                    _logger.Info($"Process Price Update: receievd update {priceUpdate.Ask} : {priceUpdate.Bid} : {priceUpdate.High} : {priceUpdate.Low} : {priceUpdate.Symbol} : {priceUpdate.Updated}");
                    var price = new PriceDto()
                    {
                        BidOpen = priceUpdate.Bid,
                        BidClose = priceUpdate.Bid,

                        AskOpen = priceUpdate.Ask,
                        AskClose = priceUpdate.Ask,

                        Market = request.Market
                    };
                    
                    request.Prices.Add(price);

                    _producer.PublishMessage(request, "PriceResponseQueue");
                    _logger.Info($"Published to Queue: {request}");
                };

                while (true)
                {
                    //loop tpo keep session open
                    Thread.Sleep(3000);
                }

                //session.PriceUpdate -= Session_PriceUpdate;
                //session.UnsubscribeSymbol(request.Market);
            }).ConfigureAwait(false);
        }
    }
}