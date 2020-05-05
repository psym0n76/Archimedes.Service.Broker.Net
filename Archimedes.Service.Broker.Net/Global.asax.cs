using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace Archimedes.Service.BrokerDotNet
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);

            Task.Run(() => { Fx.Broker.Fxcm.Runner.Program.GetHistPricesRunner(); });
        }
    }
}
