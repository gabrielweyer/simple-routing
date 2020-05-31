using System;

namespace BeanstalkWorker.SimpleRouting.Core.Models
{
    public class DoNothingMessage
    {
        public DateTimeOffset StartAt { get; set; }
    }
}
