﻿using Archimedes.Broker.Fxcm.Runner;
using Archimedes.Service.Broker.Net.DependencyResolution;
using NLog;
using StructureMap;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Archimedes.Broker.Fxcm.Runner.Http;
using Archimedes.Library.Domain;
using Microsoft.Extensions.Options;

namespace Archimedes.Service.Broker.Net
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly IMarketClient _client = new MarketClient();

        protected void Application_Start()
        {
            var test = _client.GetMarketAsync();
            ApplicationRunner();

        }

        private void ApplicationRunner()
        {
            try
            {
                _logger.Info("Application Start");

                AreaRegistration.RegisterAllAreas();
                GlobalConfiguration.Configure(WebApiConfig.Register);

                var container = Container.For<DefaultRegistry>();
                var runner = container.GetInstance<MessageBrokerConsumer>();

                _logger.Info("FXCM Validating Connection");

                if (!BrokerSession.ValidateConnection())
                {
                    throw new UnauthorizedAccessException();
                }

                _logger.Info("FXCM Validating Connection - CONNNECTED");

                Task.Run(()=>
                {
                    runner.Run(_cancellationToken.Token);
                });
            }

            catch (UnauthorizedAccessException e)
            {
                _logger.Error(BrokerSessionExceptionLogs.Print("Unable to connect to FXCM URL:"));
            }

            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }

        protected void Application_End()
        {
            try
            {
                _logger.Info("Application End:");
                _client.UpdateMarketStatusAsync();
                _cancellationToken.Cancel();
            }
            catch (Exception e)
            {
                _logger.Error($"Termination Error: Message:{e.Message} StackTrade: {e.StackTrace}");
            }
        }
    }
}
