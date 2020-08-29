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

        public static void ValidateConnection()
        {
            var url = ConfigurationManager.AppSettings["URL"];
            var accessToken = ConfigurationManager.AppSettings["AccessToken"];

            var retry = 0;

            while (!ConnectionSuccessful(url, accessToken) && retry < 2)
            {
                Thread.Sleep(2000);
                retry++;
            }

            if (retry == 2)
            {
                throw new UnauthorizedAccessException(
                    $"Unable to connect to FXCM URL:{url} Port:{accessToken} - Max Retries hit 5");
            }
        }

        private static bool ConnectionSuccessful(string url, string accessToken)
        {
            try
            {
                var session = GetInstance();
                session.Connect();

                return session.State == SessionState.Connected;
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