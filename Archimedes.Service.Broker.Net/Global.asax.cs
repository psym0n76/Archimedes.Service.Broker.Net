using System;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Service.Broker.Net.DependencyResolution;
using NLog;
using StructureMap;

namespace Archimedes.Service.Broker.Net
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
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
                runner.Run();

                //Program.GetHistPricesRunner();
                
            }
            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }
    }
}
