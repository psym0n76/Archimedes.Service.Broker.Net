using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Service.Broker.Net.DependencyResolution;
using NLog;
using StructureMap;
using System;
using System.Threading;
using System.Threading.Tasks;
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
                _logger.Info("Started configuration: Waiting 10 Secs for Rabbit");
                Thread.Sleep(10000);
                _logger.Info("Started configuration: Finished waiting for Rabbit");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                var container = Container.For<DefaultRegistry>();
                var runner = container.GetInstance<MessageBrokerConsumer>();

                Task.Run(() =>
                {
                    _logger.Info("Started running:");
                    runner.Run(_cancellationToken.Token);
                });

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
                _logger.Info("CancelationToken Raised:");
                _cancellationToken.Cancel();
            }
            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }
    }
}
