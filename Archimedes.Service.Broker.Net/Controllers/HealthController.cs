using System;
using System.Web.Http;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Archimedes.Library.Message.Dto;
using NLog;

namespace Archimedes.Service.Candle.Controllers
{
    [System.Web.Mvc.Route("api/[controller]")]
    public class HealthController : ApiController
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // this is not picke dup

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
                return Ok(health);
            }
            catch (Exception e)
            {
                _logger.Error($"Error {e.Message} {e.StackTrace}");
                return BadRequest("Error");
            }
        }
    }
}