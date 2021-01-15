using NLog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Archimedes.Library.Logger;
using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public class MessageBrokerConsumer : IMessageBrokerConsumer
    {

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ICandleSubscriber _subscriber;
        private readonly IPriceSubscriber _priceSubscriber;
        private readonly BatchLog _batchLog = new BatchLog();
        private string _logId;

        public MessageBrokerConsumer(ICandleSubscriber subscriber, IPriceSubscriber priceSubscriber)
        {
            _subscriber = subscriber;
            _priceSubscriber = priceSubscriber;
        }

        public void Run(CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            _logId = _batchLog.Start(nameof(MessageBrokerConsumer));

            var url = ConfigurationManager.AppSettings["URL"];
            var accessToken = ConfigurationManager.AppSettings["AccessToken"];
            
            _batchLog.Update(_logId,$"Attempting to connect {url} {accessToken}");

            try
            {
                var session = BrokerSession.GetInstance();

                session.Connect();

                if (session.State == SessionState.Disconnected)
                {
                    _logger.Error(_batchLog.Print(_logId,"Unable to connect to FXCM"));
                    return;
                }

                _batchLog.Update(_logId, $"FXCM Connected: { url}");

                _batchLog.Update(_logId,"Subscribe to Candle Queue");
                Task.Run(() => { _subscriber.SubscribeCandleMessage(session, cancellationToken); }, cancellationToken);

                _batchLog.Update(_logId, "Subscribe to Price Queue");
                Task.Run(() => { _priceSubscriber.SubscribePriceMessage(session, cancellationToken); }, cancellationToken);

                _logger.Info(_batchLog.Print(_logId));

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
                
                _logger.Error(_batchLog.Print(_logId,"",e));
            }

            finally
            {
                _logger.Info(_batchLog.Print(_logId,$"Disconnected:{url} - Elapsed: {stopWatch.Elapsed:dd\\.hh\\:mm\\:ss}"));
                stopWatch.Stop();
            }
        }
    }
}