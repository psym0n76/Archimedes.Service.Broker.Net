using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Service.Broker.Net.DependencyResolution;
using NLog;
using StructureMap;
using System;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;

namespace Archimedes.Service.Broker.Net
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        protected void Application_Start()
        {
            try
            {
                _logger.Info("Application Start:");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                var container = Container.For<DefaultRegistry>();
                var runner = container.GetInstance<MessageBrokerConsumer>();

                _logger.Info("Started running");
                _logger.Info("Validating FXCM Connection");

                if (!BrokerSession.ValidateConnection())
                {
                    throw new UnauthorizedAccessException();
                }

                _logger.Info("Validating FXCM Connection - CONNNECTED");

                runner.Run(_cancellationToken.Token);

            }

            catch (UnauthorizedAccessException e)
            {
                _logger.Error(BrokerSessionExceptionLogs.Print("Unable to connect to FXCM URL:"));
            }

            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }

        protected void Application_End()
        {
            try
            {
                _logger.Info("Application End:");
                _logger.Info($"CancelationToken Raised {_cancellationToken.IsCancellationRequested}");
                _cancellationToken.Cancel();
                _logger.Info($"CancelationToken Raised {_cancellationToken.IsCancellationRequested}");
                _logger.Info("Application End: Waiting 5 secs to shut down");
                Thread.Sleep(5000);
                _logger.Info("Application End: SHUTDOWN");
            }
            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }
    }
}
