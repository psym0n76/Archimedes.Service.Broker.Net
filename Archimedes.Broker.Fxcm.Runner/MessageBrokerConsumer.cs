using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Fx.Broker.Fxcm;

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

        public void Run(CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var url = ConfigurationManager.AppSettings["URL"];
            var accessToken = ConfigurationManager.AppSettings["AccessToken"];

            try
            {
                _logger.Info($"FXCM Session Token: {accessToken} URL: {url}");

                var session = BrokerSession.GetInstance();

                session.Connect();

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error("Unable to connect to FXCM");
                    return;
                }

                _logger.Info($"FXCM Connected: {url}");

                Task.Run(() => { _subscriber.SubscribeCandleMessage(session, cancellationToken); }, cancellationToken);

                Task.Run(() => { _priceSubscriber.SubscribePriceMessage(session, cancellationToken); }, cancellationToken);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    Thread.Sleep(3000);
                }

            }
            catch (Exception e)
            {
                _logger.Error($"Error message:{e.Message} StackTrace:{e.StackTrace}");
            }

            finally
            {
                _logger.Info($"Disconnected:{url} - Elapsed: {stopWatch.Elapsed:dd\\.hh\\:mm\\:ss}");
                stopWatch.Stop();
            }
        }
    }
}