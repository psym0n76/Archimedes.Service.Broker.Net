using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IPriceSubscriber
    {
        void SubscribePriceMessage(Session session);
    }
}