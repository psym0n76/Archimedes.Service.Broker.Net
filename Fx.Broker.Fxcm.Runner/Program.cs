using System;
using System.Configuration;
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

                var sampleParams = new SampleParams(ConfigurationManager.AppSettings);
                var test = new QueueTesting();
                test.TestQueue(sampleParams.RabbitHutchConnection);

                //var brokerSession = new BrokerSession();

                //var consumer = new MessageBrokerConsumer(sampleParams, brokerSession);

                //consumer.Run();

                _logger.Info("Finishing.....");
            }

            catch (Exception e)
            {
                _logger.Error(e.StackTrace, "Stopped program because of exception");
            }
        }
    }
}
