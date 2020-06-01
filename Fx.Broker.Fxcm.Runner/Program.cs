using System;
using System.Collections.Generic;
using System.Configuration;
using Archimedes.Library.Message;
using Archimedes.Library.Message.Dto;
using EasyNetQ;
using NLog;

namespace Fx.Broker.Fxcm.Runner
{
    public class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            try
            {
                _logger.Info("Initialise Main");

                var brokerSession = new BrokerSession();
                var sampleParams = new SampleParams(ConfigurationManager.AppSettings);

                var consumer = new MessageBrokerConsumer(sampleParams, brokerSession);

                consumer.Run();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Stopped program because of exception");
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }

        }

        public static void GetHistPricesRunner()
        {

            try
            {
                _logger.Info("Starting Hist Price Runner.....");

                var brokerSession = new BrokerSession();
                var sampleParams = new SampleParams(ConfigurationManager.AppSettings);
                var consumer = new MessageBrokerConsumer(sampleParams, brokerSession);

                consumer.Run();

                _logger.Info("Finishing.....");
            }

            catch (Exception e)
            {
                _logger.Error(e, "Stopped program because of exception");
            }

        }

        private void TestQueue()
        {
            _logger.Info("Test Queue");

            const string Host = "host=localhost";

            var price = new ResponsePrice()
            {
                Status = "Test",
                Payload = new List<PriceDto>(){new PriceDto(){AskClose = 1.2}},
                Text = "This is working"
            };

            _logger.Info(price);

            try
            {
                using (var bus = RabbitHutch.CreateBus(Host))
                {
                    bus.Publish(price);
                    _logger.Info("Posted PriceResponse messages. Hit <return> to quit");
                }
            }
            catch (Exception e)
            {

                _logger.Error(e,"Failed to post to Queue");
            }
        }
    }
}
