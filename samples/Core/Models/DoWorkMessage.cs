using System;

namespace BeanstalkWorker.SimpleRouting.Core.Models
{
    public class DoWorkMessage
    {
        public string NatureOfWork { get; set; }
        public DateTimeOffset StartsAt { get; set; }
    }
}
