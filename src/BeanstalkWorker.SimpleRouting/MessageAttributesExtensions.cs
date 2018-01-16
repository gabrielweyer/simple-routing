using System.Collections.Generic;
using Amazon.SQS.Model;

namespace BeanstalkWorker.SimpleRouting
{
    public static class MessageAttributesExtensions
    {
        public static void AddRoutingAttribute(this IDictionary<string, MessageAttributeValue> attributes, string task)
        {
            attributes.Add(
                RoutingConstants.HeaderName,
                new MessageAttributeValue {StringValue = task, DataType = RoutingConstants.HeaderType});
        }
    }
}