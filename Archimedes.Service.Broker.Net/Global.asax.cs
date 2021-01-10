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
        private string _logId;

        protected void Application_Start()
        {
            _logId = _batchLog.Start();
            ApplicationRunner();
        }

        private void ApplicationRunner()
        {
            try
            {
                _batchLog.Update(_logId, "Application Start");
                
                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                var container = Container.For<DefaultRegistry>();
                var runner = container.GetInstance<MessageBrokerConsumer>();

                _batchLog.Update(_logId, "FXCM Validating Connection");

                if (!BrokerSession.ValidateConnection())
                {
                     _logger.Error(_batchLog.Print(_logId,"FXCM Validating Connection - UNABLE TO CONNECT"));
                    return;
                }

                _batchLog.Update(_logId, "FXCM Validating Connection - CONNECTED");

                Task.Run(()=>
                {
                    runner.Run(_cancellationToken.Token);
                });
                
                _logger.Info(_batchLog.Print(_logId));
            }

            catch (UnauthorizedAccessException e)
            {
                _logger.Error(_batchLog.Print(_logId, BrokerSessionExceptionLogs.Print("Unable to connect to FXCM URL:")));
            }

            catch (Exception e)
            {
                _logger.Error(_batchLog.Print(_logId,"Error returned from Application Runner",e));
            }
        }

        protected void Application_End()
        {
            try
            {
                _logger.Info(_batchLog.Print(_logId, "Application End:"));
                _cancellationToken.Cancel();
            }
            catch (Exception e)
            {
                _logger.Error(_batchLog.Print(_logId, "Error returned from Application Runner", e));
            }
        }
    }
}
