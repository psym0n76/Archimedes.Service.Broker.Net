using System;
using System.Threading.Tasks;
using Fx.Broker.Fxcm;
//using Fx.MessageBus.Publishers;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class BrokerProcessPrice : IBrokerProcess
    {
        public void Run(Session session, SampleParams sampleParams)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Process Price Update");
                session.SubscribeSymbol(sampleParams.Instrument);
                //session.PriceUpdate += Session_PriceUpdate;

                session.PriceUpdate += (PriceUpdate priceUpdate) =>
                {
                    //INetQPublish p = new NetQPublish(sampleParams.RabbitHutchConnection);
                    //p.PublishMessage("");
                    //Console.WriteLine($"Date: {priceUpdate.Updated} Ask: {priceUpdate.Ask} Bid: {priceUpdate.Bid}");
                };

                Console.ReadLine();

                // session.PriceUpdate -= Session_PriceUpdate;
                // session.UnsubscribeSymbol(sampleParams.Instrument);
            }).ConfigureAwait(false);
        }

        //private static void Session_PriceUpdate(PriceUpdate priceUpdate)
        //{
        //    INetQPublish p = new NetQPublish();
        //    p.PublishMessage("");
        //    Console.WriteLine($"Date: {priceUpdate.Updated} Ask: {priceUpdate.Ask} Bid: {priceUpdate.Bid}");
        //}
    }
}