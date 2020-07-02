using Fx.Broker.Fxcm;

namespace Archimedes.Broker.Fxcm.Runner
{
    public interface IBrokerSession
    {
        Session GetSession(string accessTokens, string host);
    }
}