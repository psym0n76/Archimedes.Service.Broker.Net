using System;
using Archimedes.Library.Message;
using EasyNetQ;
    

namespace Fx.Broker.Fxcm.Runner
{
    public class MessageBrokerConsumer
    {
        private readonly SampleParams _sampleParams;
        private readonly IBrokerSession _brokerSession;
        private Session _session;

        public MessageBrokerConsumer(SampleParams sampleParams, IBrokerSession brokerSession)
        {
            _sampleParams = sampleParams;
            _brokerSession = brokerSession;
        }

        private const string Host = "host=localhost";

        public void Run()
        {
            try
            {
                _session = _brokerSession.GetSession(_sampleParams.AccessToken, _sampleParams.Url);
                _session.Connect();

                Console.WriteLine("Connected");

                SubscribeCandleMessage();

                Console.WriteLine("Disconnected");
                Console.WriteLine();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        private void SubscribeCandleMessage()
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

        private void HandleTextMessage(IRequest message)
        {
            if (message == null) return;

            var process = BrokerProcessFactory.Get(message.Text);

            // return a message of some kind
            process.Run(_session, _sampleParams);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Got message: {0}", message.Text);
            Console.ResetColor();
        }
    }
}