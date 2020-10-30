using System.Collections.Generic;

namespace Archimedes.Broker.Fxcm.Runner
{
    public static class SubscribedMarkets
    {
        public static List<string> Markets = new List<string>();
        private static volatile object _locker = new object();

        public static void Add(string market)
        {
            lock (_locker)
            {
                Markets.Add(market);
            }
        }

        public static bool IsSubscribed(string market)
        {
            lock (_locker)
            {
                return Markets.Contains(market);
            }
        }
    }
}