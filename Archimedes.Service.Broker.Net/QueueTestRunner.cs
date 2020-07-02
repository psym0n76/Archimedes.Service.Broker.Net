using System.Configuration;
using Archimedes.Broker.Fxcm.Runner;

namespace Archimedes.Service.Broker.Net
{
    public class QueueTestRunner
    {
        private readonly IQueueTesting _testing;

        public QueueTestRunner(IQueueTesting testing)
        {
            _testing = testing;
        }

        public void Run()
        {
            _testing.QueueTest();
        }
        
    }
}