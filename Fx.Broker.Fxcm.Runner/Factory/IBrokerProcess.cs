using Fx.Broker.Fxcm;

namespace Fx.Broker.Fxcm.Runner
{
    public interface IBrokerProcess
    {
        void Run(Session session, SampleParams sampleParams);
    }
}