using System.Threading;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IMessageBrokerConsumer
    {
        void Run(CancellationToken cancellationToken);
    }
}