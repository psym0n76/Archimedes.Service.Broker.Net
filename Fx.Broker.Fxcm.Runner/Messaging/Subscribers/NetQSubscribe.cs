using EasyNetQ;
using EasyNetQMessageDotNet;
using System;

namespace FXCMRestRunner
{
    public class NetQSubscribe : INetQSubscriber
    {
        private const string Host = "host=localhost";

        public void SubscribeCandleMessage()
        {
            using (var bus = RabbitHutch.CreateBus(Host))
            {
                bus.Subscribe<IActionMessage>("Candle", @interface =>
                {
                    if (@interface is CandleMessage candle)
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
                bus.Subscribe<IActionMessage>("Trade", @interface =>
                {
                    if (@interface is TradeMessage candle)
                    {
                        HandleTextMessage(candle);
                    }
                });
                Console.WriteLine("Listening for Trade messages. Hit <return> to quit.");
                Console.ReadLine();
            }
        }

        private static void HandleTextMessage<T>(T message) where T : IActionMessage
        {
            if (message == null) return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", message.Text);
            Console.ResetColor();
        }
    }
}