using System;
using System.Collections.Generic;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using Fx.MessageBus.Publishers;
using NLog;

namespace Fx.Broker.Fxcm.Runner
{
    public class QueueTesting
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void TestQueue(string host)
        {
            try
            {
                _logger.Info("Running Test Queue");

                var price = new ResponsePrice()
                {
                    Status = "Test",
                    Payload = new List<PriceDto>() {new PriceDto() {AskClose = 1.2}},
                    Text = "Test Message"
                };

                _logger.Info(price);
                _logger.Info(host);

                var pub = new NetQPublish(host);
                pub.PublishPriceMessage(price);

            }
            catch (Exception e)
            {
                _logger.Error($"Error found: Message:{e.Message} StackTrace:{e.StackTrace}");
            }
        }
    }
}