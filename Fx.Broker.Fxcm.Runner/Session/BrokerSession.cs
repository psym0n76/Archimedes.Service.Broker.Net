using Fx.Broker.Fxcm;

namespace Fx.Broker.Fxcm.Runner
{
    public class BrokerSession : IBrokerSession
    {
        public Session GetSession(string accessToken, string host)
        {
            return new Session(accessToken, host);
        }
    }
}