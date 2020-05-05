using Fx.Broker.Fxcm;

namespace Fx.Broker.Fxcm.Runner
{
    public interface IBrokerSession
    {
        Session GetSession(string accessTokens, string host);
    }
}