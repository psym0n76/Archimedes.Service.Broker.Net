using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerProcess
    {
        void Run(Session session, SampleParams sampleParams);
    }
}