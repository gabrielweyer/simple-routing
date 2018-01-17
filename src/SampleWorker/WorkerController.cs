using BeanstalkWorker.SimpleRouting.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BeanstalkWorker.SimpleRouting.SampleWorker
{
    [Route("")]
    public class WorkerController : Controller
    {
        private readonly ILogger<WorkerController> _logger;

        public WorkerController(ILogger<WorkerController> logger)
        {
            _logger = logger;
        }

        [HttpPost(WorkerConstants.DoNothingTaskName)]
        public IActionResult DoNothing(DoNothingMessage model)
        {
            _logger.LogDebug("Received message {@Message}", model);

            return Ok();
        }

        [HttpPost(WorkerConstants.DoWorkTaskName)]
        public IActionResult DoWork(DoWorkMessage model)
        {
            _logger.LogDebug("Received message {@Message}", model);

            return Ok();
        }
    }
}
