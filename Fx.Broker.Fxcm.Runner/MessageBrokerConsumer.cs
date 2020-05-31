using System;
using System.Diagnostics;
using Archimedes.Library.Message;
using EasyNetQ;
using NLog;


namespace Fx.Broker.Fxcm.Runner
{
    public class MessageBrokerConsumer
    {

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                _logger.Info($"Get Session Token:{_sampleParams.AccessToken} URL:{_sampleParams.Url}");
                _session = _brokerSession.GetSession(_sampleParams.AccessToken, _sampleParams.Url);

                _session.Connect();

                _logger.Info($"Connected to URL:{_sampleParams.Url}");
                Console.WriteLine("Connected");

                SubscribeCandleMessage();

                _logger.Info($"Disconnected:{_sampleParams.Url} - Elapsed: {stopWatch.Elapsed:dd\\.hh\\:mm\\:ss}");

                Console.WriteLine("Disconnected");
                Console.WriteLine();
                stopWatch.Stop();
            }
            catch (Exception e)
            {
                _logger.Info($"Disconnected:{_sampleParams.Url} - Elapsed: {stopWatch.Elapsed:dd\\.hh\\:mm\\:ss}");
                _logger.Error($"Error message:{e.Message} StackTrade:{e.StackTrace}");
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
                _logger.Info("Listening for Candle messages. Hit <return> to quit");
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