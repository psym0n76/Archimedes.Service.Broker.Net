using Archimedes.Library.Message;
using EasyNetQ;
using System;

namespace Fx.MessageBus.Subscribers
{
    public class NetQSubscribe : INetQSubscriber
    {
        private readonly string _host;

        public NetQSubscribe(string host)
        {
            _host = host;
        }

        public void SubscribeCandleMessage()
        {
            using (var bus = RabbitHutch.CreateBus(_host))
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
            using (var bus = RabbitHutch.CreateBus(_host))
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