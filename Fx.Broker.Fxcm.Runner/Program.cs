using System;
using System.Configuration;

namespace Fx.Broker.Fxcm.Runner
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting....");

            var brokerSession = new BrokerSession();
            var sampleParams = new SampleParams(ConfigurationManager.AppSettings);

            var consumer = new MessageBrokerConsumer(sampleParams, brokerSession);

            consumer.Run();

            Console.WriteLine("Finishing....");
            Console.ReadLine();
        }

        public static void GetHistPricesRunner()
        {
            //SampleParams sampleParams = new SampleParams(ConfigurationManager.AppSettings);
            //PrintSampleParams("GetHistPrices", sampleParams);

            //var session = new Session(sampleParams.AccessToken, sampleParams.Url);
            //session.Connect();
            //IList<Candle> candleHistory = GetHistory(session, sampleParams);
            //session.Close();
            //return candleHistory.Count;

            Console.WriteLine("Starting....");

            var brokerSession = new BrokerSession();
            var sampleParams = new SampleParams(ConfigurationManager.AppSettings);
            var consumer = new MessageBrokerConsumer(sampleParams, brokerSession);

            consumer.Run();

            Console.WriteLine("Finishing....");
            Console.ReadLine();
        }
    }
}
