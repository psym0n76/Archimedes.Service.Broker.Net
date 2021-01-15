using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Service.Broker.Net.DependencyResolution;
using NLog;
using StructureMap;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Archimedes.Library.Logger;

namespace Archimedes.Service.Broker.Net
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly BatchLog _batchLog = new BatchLog();

        protected void Application_Start()
        {
            var logId = _batchLog.Start();

            _batchLog.Update(logId, "===============================================================================================================================================");
            _batchLog.Update(logId, "Application Start Application Start Application Start Application Start Application Start Application Start Application Start Application Start");
            _batchLog.Update(logId, "===============================================================================================================================================");
            _logger.Info(_batchLog.Print(logId));
            
            ApplicationRunner();
        }

        private void ApplicationRunner()
        {
            var logId = _batchLog.Start(nameof(ApplicationRunner));

            try
            {
                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                var container = Container.For<DefaultRegistry>();
                var runner = container.GetInstance<MessageBrokerConsumer>();

                _batchLog.Update(logId, "FXCM Validating Connection");

                var (logger, connected) = BrokerSession.ValidateConnection();

                if (!connected)
                {
                    _batchLog.Update(logId,"FXCM Connection Failed - following log from BrokerSession");
                    _logger.Error(_batchLog.Print(logId, logger.Print(BrokerSession.LogId)));
                    Application_End();
                    return;
                }
                
                _batchLog.Update(logId, "FXCM Validating Connection - CONNECTED");

                Task.Run(()=>
                {
                    runner.Run(_cancellationToken.Token);
                });
                
                _logger.Info(_batchLog.Print(logId));
            }

            catch (Exception e)
            {
                _logger.Error(_batchLog.Print(logId,"Error returned from Application Runner",e));
            }
        }

        protected void Application_End()
        {
            try
            {
                var logId = _batchLog.Start();
                _batchLog.Update(logId, "===============================================================================================================================================");
                _batchLog.Update(logId, "Application End Application End Application End Application End Application End Application End Application End Application End Application End");
                _batchLog.Update(logId, "===============================================================================================================================================");
                _logger.Info(_batchLog.Print(logId));
                
                _cancellationToken.Cancel();
            }
            catch (Exception e)
            {
                _logger.Error($"Error returned from Application_End \n\n{e.Message} \n\n{e.InnerException} \n\n{e.StackTrace}");
            }
        }
    }
}
