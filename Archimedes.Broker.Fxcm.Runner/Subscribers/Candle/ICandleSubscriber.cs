using System.Threading;
using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface ICandleSubscriber
    {
        void SubscribeCandleMessage(Session session, CancellationToken cancellationToken);
    }
}