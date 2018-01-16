using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using BeanstalkWorker.SimpleRouting.Core.Models;
using BeanstalkWorker.SimpleRouting.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BeanstalkWorker.SimpleRouting.Core.Logic
{
    public class SqsClient : ISqsClient
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly QueueOptions _queueOptions;
        private readonly ILogger<SqsClient> _logger;

        public SqsClient(IAmazonSQS sqsClient, IOptions<QueueOptions> queueOptions, ILogger<SqsClient> logger)
        {
            _sqsClient = sqsClient;
            _queueOptions = queueOptions.Value;
            _logger = logger;
        }

        public Task SendDoWorkMessageAsync(DoWorkMessage body)
        {
            return SendMessagesAsync(WorkerConstants.DoWorkTaskName, body);
        }

        public Task SendDoNothingMessageAync(DoNothingMessage body)
        {
            return SendMessagesAsync(WorkerConstants.DoNothingTaskName, body);
        }

        private async Task SendMessagesAsync<T>(string task, T body)
        {
            var request = GenerateRequest();

            _logger.LogDebug("Sending message {MessageBody} with attributes {MessageAttributes} to queue {QueueUrl}",
                request.MessageBody,
                request.MessageAttributes,
                request.QueueUrl);

            await _sqsClient.SendMessageAsync(request);

            SendMessageRequest GenerateRequest()
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    MessageBody = JsonConvert.SerializeObject(body),
                    QueueUrl = _queueOptions.WorkerQueueUrl.ToString()
                };
                
                sendMessageRequest.MessageAttributes.AddRoutingAttribute(task);

                return sendMessageRequest;
            }
        }
    }

    public interface ISqsClient
    {
        Task SendDoWorkMessageAsync(DoWorkMessage body);
        Task SendDoNothingMessageAync(DoNothingMessage body);
    }
}
