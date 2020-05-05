using EasyNetQMessageDotNet;

namespace FXCMRestRunner
{
    public interface INetQPublish
    {
        void PublishMessage(string message);
        void PublishMessageClass<T>(T message) where T : IActionMessage;
    }
}