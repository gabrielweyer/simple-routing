using BeanstalkWorker.SimpleRouting.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BeanstalkWorker.SimpleRouting.SampleWorker
{
    [Route("")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        private readonly ILogger<WorkerController> _logger;

        public WorkerController(ILogger<WorkerController> logger)
        {
            _logger = logger;
        }

        [HttpPost(WorkerConstants.DoNothingTaskName)]
        public IActionResult DoNothing(DoNothingMessage model)
        {
            _logger.LogDebug("Received order to do nothing starting at {StartAt}", model.StartAt);

            return Ok();
        }

        [HttpPost(WorkerConstants.DoWorkTaskName)]
        public IActionResult DoWork(DoWorkMessage model)
        {
            _logger.LogDebug("Received order to do {WorkType} starting at {StartAt}", model.NatureOfWork, model.StartsAt);

            return Ok();
        }
    }
}
