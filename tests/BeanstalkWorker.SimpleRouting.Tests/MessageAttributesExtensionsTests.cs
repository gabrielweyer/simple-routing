using System.Collections.Generic;
using Amazon.SQS.Model;
using Xunit;

namespace BeanstalkWorker.SimpleRouting.Tests
{
    public class MessageAttributesExtensionsTests
    {
        [Fact]
        public void GivenValidTaskName_WhenAddRoutingAttribute_ThenAddAttribute()
        {
            // Arrange

            const string taskName = "DoWork";
            var attributes = new Dictionary<string, MessageAttributeValue>();

            // Act

            attributes.AddRoutingAttribute(taskName);
            
            // Assert

            const string expectedKey = "Task";
            
            Assert.Single(attributes);
            Assert.True(attributes.TryGetValue(expectedKey, out var attribute));
            Assert.Equal("String", attribute.DataType);
            Assert.Equal(taskName, attribute.StringValue);
        }
    }
}