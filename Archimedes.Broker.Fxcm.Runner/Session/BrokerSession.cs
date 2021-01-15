using System;
using Fx.Broker.Fxcm;
using System.Configuration;
using System.Threading;

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
                        _instance = new Session(ConfigurationManager.AppSettings["AccessToken"],
                            ConfigurationManager.AppSettings["URL"]);
                    }
                }
            }

            return _instance;
        }

        //public static Session GetInstance()
        //{
        //    return new Session(ConfigurationManager.AppSettings["AccessToken"],
        //        ConfigurationManager.AppSettings["URL"]);

        //}

        public static bool ValidateConnection()
        {
            var retry = 0;

            while (!ConnectionSuccessful() && retry < 5)
            {
                Thread.Sleep(2000);
                retry++;
            }

            return retry != 5;
        }

        private static bool ConnectionSuccessful()
        {
            try
            {
                var session = GetInstance();
                session.Connect();
                session.Close();

                return true;
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException(exception.Message);
            }
            catch (Exception e)
            {
                BrokerSessionExceptionLogs.Add(e);
                return false;
            }
        }
    }
}