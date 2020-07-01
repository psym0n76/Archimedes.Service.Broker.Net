using System.Collections.Generic;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.MessageBus.Publishers;
using NLog;

namespace Fx.Broker.Fxcm.Runner
{
    public class QueueTesting
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void TestQueue(string host)
        {
            _logger.Info("Test Queue");

            var price = new ResponsePrice()
            {
                Status = "Test",
                Payload = new List<PriceDto>(){new PriceDto(){AskClose = 1.2}},
                Text = "Test Message"
            };

            _logger.Info(price.ToString);

            var pub = new NetQPublish(host);
            pub.PublishPriceMessage(price);
        }
    }
}