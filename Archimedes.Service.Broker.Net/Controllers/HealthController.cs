using System;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Archimedes.Library.Message.Dto;

namespace Archimedes.Service.Candle.Controllers
{
    [System.Web.Mvc.Route("api/[controller]")]
    public class HealthController : ApiController
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet()]
        public IHttpActionResult Get()
        {
            var appName = ConfigurationManager.AppSettings["ApplicationName"];
            var appVersion = ConfigurationManager.AppSettings["Version"];

            var health = new HealthMonitorDto()
            {
                AppName = appName,
                Version = appVersion,
                LastActiveVersion = appVersion,
                Status = true,
                LastUpdated = DateTime.Now,
                LastActive = DateTime.Now
            };

            try
            {
                _logger.LogInformation($"{appName} Version: {appVersion}");
                return Ok(health);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error {e.Message} {e.StackTrace}");
                return BadRequest("Error");
            }
        }
    }
}