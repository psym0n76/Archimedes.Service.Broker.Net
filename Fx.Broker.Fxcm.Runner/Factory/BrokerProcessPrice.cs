using System;
using System.Threading.Tasks;
using Fx.MessageBus.Publishers;

namespace Fx.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcess
    {
        public void Run(Session session, SampleParams sampleParams)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Process Price Update");
                session.SubscribeSymbol(sampleParams.Instrument);
                session.PriceUpdate += Session_PriceUpdate;
                Console.ReadLine();

                // session.PriceUpdate -= Session_PriceUpdate;
                // session.UnsubscribeSymbol(sampleParams.Instrument);
            }).ConfigureAwait(false);
        }

        private static void Session_PriceUpdate(PriceUpdate priceUpdate)
        {
            INetQPublish p = new NetQPublish();
            p.PublishMessage("");
            Console.WriteLine($"Date: {priceUpdate.Updated} Ask: {priceUpdate.Ask} Bid: {priceUpdate.Bid}");
        }
    }
}