# Elastic Beanstalk Worker simple routing

Allows to route a SQS message to a specific endpoint on the Worker instead of having a single endpoint handling all the messages.

Relies on the [SQS message attributes][sqs-message-attributes]. This is distributed via a `NuGet` package but the implementation is so simple that you can just copy the classes into your own solution if that works better for you.

## How it works

### Set the attribute on the message

### Add the middleware to the router

### Use a matching route on a Controller Action

## Limitations

- Does not support [periodic tasks][periodic-tasks]
  - It could be added fairly easily if required
- `netstandard2.0` and above only
  - .NET is a [second][no-worker-tier] class [citizen][no-environment-variables] on Elastic Beanstalk, for this reason I recommend creating Docker images so that you can take full advantage of the platform.

[sqs-message-attributes]: https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-message-attributes.html
[periodic-tasks]: https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/using-features-managing-env-tiers.html#worker-periodictasks
[no-worker-tier]: https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/concepts.platforms.html#concepts.platforms.net
[no-environment-variables]: https://stackoverflow.com/questions/40127703/aws-elastic-beanstalk-environment-variables-in-asp-net-core-1-0
