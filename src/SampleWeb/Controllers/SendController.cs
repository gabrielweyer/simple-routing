using System;
using System.Threading.Tasks;
using BeanstalkWorker.SimpleRouting.Core.Logic;
using BeanstalkWorker.SimpleRouting.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeanstalkWorker.SimpleRouting.SampleWeb.Controllers
{
    public class SendController : Controller
    {
        private readonly ISqsClient _sqsClient;

        public SendController(ISqsClient sqsClient)
        {
            _sqsClient = sqsClient;
        }
        
        public async Task Work()
        {
            var body = new DoWorkMessage
            {
                ExpectedDurationOfWork = TimeSpan.FromSeconds(3),
                NatureOfWork = "Not much"
            };
            
            await _sqsClient.SendDoWorkMessageAsync(body);
        }
        
        public async Task Nothing()
        {
            var body = new DoNothingMessage
            {
                RestDuration = TimeSpan.FromDays(3)
            };
            
            await _sqsClient.SendDoNothingMessageAync(body);
        }
    }
}
