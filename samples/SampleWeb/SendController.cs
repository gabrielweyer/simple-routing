using System;
using System.Threading.Tasks;
using BeanstalkWorker.SimpleRouting.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeanstalkWorker.SimpleRouting.SampleWeb
{
    [ApiController]
    [Route("send")]
    public class SendController : ControllerBase
    {
        private readonly ISqsClient _sqsClient;

        public SendController(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;
        }

        [Route("work")]
        public async Task Work()
        {
            var body = new DoWorkMessage
            {
                NatureOfWork = "Not much",
                StartsAt = DateTimeOffset.UtcNow.AddYears(3)
            };

            await _sqsClient.SendDoWorkMessageAsync(body);
        }

        [Route("nothing")]
        public async Task Nothing()
        {
            var body = new DoNothingMessage
            {
                StartAt = DateTimeOffset.UtcNow.AddSeconds(1)
            };

            await _sqsClient.SendDoNothingMessageAsync(body);
        }
    }
}
