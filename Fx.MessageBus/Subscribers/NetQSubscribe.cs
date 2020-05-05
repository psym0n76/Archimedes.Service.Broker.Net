using EasyNetQ;
using System;
using Archimedes.Library.Message;

namespace Fx.MessageBus.Subscribers
{
    public class NetQSubscribe : INetQSubscriber
    {
        private const string Host = "host=localhost";

        public void SubscribeCandleMessage()
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {
                bus.Subscribe<RequestCandle>("Candle", @interface =>
                {
                    if (@interface is RequestCandle candle)
                    {
                        HandleTextMessage(candle);
                    }
                });
                Console.WriteLine("Listening for Candle messages. Hit <return> to quit.");
                Console.ReadLine();
            }
        }

        public void SubscribeTradeMessage()
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {
                bus.Subscribe<RequestTrade>("Trade", @interface =>
                {
                    if (@interface is RequestTrade candle)
                    {
                        HandleTextMessage(candle);
                    }
                });
                Console.WriteLine("Listening for Trade messages. Hit <return> to quit.");
                Console.ReadLine();
            }
        }

        private static void HandleTextMessage<T>(T message)
        {
            if (message == null) return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", message);
            Console.ResetColor();
        }
    }
}