using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;


namespace Archimedes.Broker.Fxcm.Runner
{
    public class MessageBrokerConsumer : IMessageBrokerConsumer
    {

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICandleSubscriber _subscriber;
        private readonly IPriceSubscriber _priceSubscriber;

        public MessageBrokerConsumer(ICandleSubscriber subscriber, IPriceSubscriber priceSubscriber)
        {
            _subscriber = subscriber;
            _priceSubscriber = priceSubscriber;
        }

        public void Run()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var url = ConfigurationManager.AppSettings["URL"];
            var accessToken = ConfigurationManager.AppSettings["AccessToken"];

            try
            {
                _logger.Info($"Get Session Token:{accessToken} URL:{url}");

                var session = BrokerSession.GetInstance();

                //session.Connect();

                _logger.Info($"Connected to URL:{url}");

                // move into tasks

                _subscriber.SubscribeCandleMessage(session);
                _priceSubscriber.SubscribePriceMessage(session);

                //_subscriber.SubscribeCandleMessage(session,_sampleParams);
                //_subscriber.SubscribeCandleMessage(session,_sampleParams);

            }
            catch (Exception e)
            {
                _logger.Error($"Error message:{e.Message} StackTrade:{e.StackTrace}");
            }
            finally
            {
                _logger.Info($"Disconnected:{url} - Elapsed: {stopWatch.Elapsed:dd\\.hh\\:mm\\:ss}");
                stopWatch.Stop();
            }
        }
    }
}