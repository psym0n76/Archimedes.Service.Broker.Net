using System;
using System.Collections.Generic;
using System.Linq;

namespace Archimedes.Broker.Fxcm.Runner
{
    public static class BrokerSessionExceptionLogs
    {
        private static readonly List<Exception> Logs = new List<Exception>();

        public static void Add(Exception item)
        {
            Logs.Add(item);
        }

        public static void Clear()
        {
            Logs.Clear();
        }

        public static string Print(string message)
        {
            return Logs.Aggregate(string.Empty, (current, log) => current + $"\n {message}  {log.Message} {log.InnerException} {log.StackTrace}");
        }
    }
}