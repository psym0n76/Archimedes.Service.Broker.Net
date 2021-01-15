using System;
using Fx.Broker.Fxcm;
using System.Configuration;
using System.Threading;
using Archimedes.Library.Logger;

namespace Archimedes.Broker.Fxcm.Runner
{
    public static class BrokerSession
    {
        private static volatile Session _instance;
        public static readonly object Mutex = new object();
        private static readonly BatchLog BatchLog = new BatchLog();
        public static string LogId;
        private const int RetryMax = 30;
        private const int Timeout = 5;

        public static Session GetInstance()
        {
            if (_instance == null)
            {
                lock (Mutex)
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

        public static Tuple<BatchLog, bool> ValidateConnection()
        {
            
            LogId = BatchLog.Start(nameof(BrokerSession));
            BatchLog.Update(LogId, nameof(ValidateConnection));

            try
            {
                var retry = 1;
                var session = GetInstance();

                BatchLog.Update(LogId, "Attempt to Connect to FXCM");

                if (session.State == SessionState.Connected)
                {
                    session.Close();
                    return new Tuple<BatchLog, bool>(BatchLog, true);
                }
                
                session.Connect();

                while (session.State == SessionState.Reconnecting && retry < RetryMax)
                {
                    BatchLog.Update(LogId, $"Waiting to Connect: {session.State} elapsed {retry * Timeout} Sec(s)");
                    Thread.Sleep(Timeout * 1000);
                    retry++;
                }

                if (session.State == SessionState.Disconnected || session.State == SessionState.Reconnecting)
                {
                    BatchLog.Update(LogId, $"Unable to Connect {session.State}");
                    return new Tuple<BatchLog, bool>(BatchLog, false);
                }

                BatchLog.Update(LogId,"Connected");
                
                session.Close();
                return new Tuple<BatchLog, bool>(BatchLog, true);
            }
            catch (InvalidOperationException exception)
            {
                BatchLog.Update(LogId,
                    $"InvalidOperationException {exception.Message}");
                return new Tuple<BatchLog, bool>(BatchLog, false);
            }
            catch (Exception e)
            {
                BatchLog.Update(LogId, $"Error returned from BrokerSession: {e.Message}");
                return new Tuple<BatchLog, bool>(BatchLog, false);
            }
        }
    }
}