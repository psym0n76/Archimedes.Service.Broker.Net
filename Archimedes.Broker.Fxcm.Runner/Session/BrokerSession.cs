using System.Configuration;
using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public static class BrokerSession
    {
        private static volatile Session _instance;
        private static readonly object _mutex = new object();

        public static Session GetInstance()
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new Session(ConfigurationManager.AppSettings["AccessToken"],ConfigurationManager.AppSettings["URL"]);
                    }
                }
            }

            return _instance;
        }
    }
}