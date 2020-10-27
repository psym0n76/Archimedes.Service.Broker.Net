using System.Collections.Generic;
using System.Threading;
using Archimedes.Library.Message;
using Fx.Broker.Fxcm;
using NLog;
using Archimedes.Library.RabbitMq;
using Newtonsoft.Json;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class PriceSubscriber : IPriceSubscriber
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IBrokerProcessPrice _brokerProcessPrice;
        private readonly IPriceConsumer _consumer;

        public PriceSubscriber(IBrokerProcessPrice brokerProcessPrice, IPriceConsumer consumer)
        {
            _brokerProcessPrice = brokerProcessPrice;
            _consumer = consumer;
            _consumer.HandleMessage += Consumer_HandleMessage;
        }

        private void Consumer_HandleMessage(object sender, MessageHandlerEventArgs args)
        {
            _logger.Info($"Receievd Price Request {args.Message}");
            var requestPrice = JsonConvert.DeserializeObject<PriceMessage>(args.Message);

            requestPrice.Logs = new List<string>(){"Message received from PriceRequestQueue"};
            _brokerProcessPrice.Run(requestPrice);
        }

        public void SubscribePriceMessage(Session session, CancellationToken cancellationToken)
        {
            _logger.Info($"Subscribed to PriceRequestQueue");
            _consumer.Subscribe();
        }
    }
}