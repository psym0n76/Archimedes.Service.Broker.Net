using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Fx.Broker.Fxcm.Runner;
using NLog;

namespace Archimedes.Service.BrokerDotNet
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected void Application_Start()
        {

            try
            {
                _logger.Info("Started configuration:");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                Task.Run(Program.GetHistPricesRunner);
            }
            catch (Exception e)
            {
                _logger.Error($"Termination Error: {e.StackTrace}");
            }
        }
    }
}
