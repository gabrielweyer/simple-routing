# Elastic Beanstalk Worker Simple Routing

| Package | Release | Pre-release |
| --- | --- | --- |
| `BeanstalkWorker.SimpleRouting` | [![NuGet][nuget-badge]][nuget] | [![MyGet][myget-badge]][myget] |

| CI | Status | Platform(s) | Framework(s) | Test Framework(s) |
| --- | --- | --- | --- | --- |
| [AppVeyor][app-veyor] | [![Build Status][app-veyor-shield]][app-veyor] | `Windows` | `nestandard2.0` | `netcoreapp2.1.0` |

Allows to route a `SQS` message to a specific endpoint on the `Worker` instead of having a single endpoint handling all the messages.

Relies on the [SQS message attributes][sqs-message-attributes]. This is distributed via a `NuGet` package but the implementation is so simple that you can just copy the classes into your own solution if that works better for you.

## How it works

### Web Tier - Set the attribute on the message

```csharp
// Instantiate your model
StronglyTypedMessage model = new StronglyTypedMessage();

var sendMessageRequest = new SendMessageRequest
{
  // Serialize your model as JSON
  MessageBody = JsonConvert.SerializeObject(model)
  // Set the QueueUrl
};

// AddRoutingAttribute is an extension method
sendMessageRequest.MessageAttributes.AddRoutingAttribute("task-name");
```

#### Sample project

A sample `Web` app is provided in [samples/SampleWeb](samples/SampleWeb).

You'll need to configure those two settings, either in `appsettings.json` or via environment variables:

- `Aws:RegionSystemName` - [region code][available-regions], for example `ap-southeast-2`
- `Aws:Queue:WorkerQueueUrl` - `URL` of the `SQS` queue, for example `https://sqs.ap-southeast-2.amazonaws.com/375985941080/dev-gabriel`

Create a `iAM` user (if you don't have one already) which has access to `SQS`. Then create two environment variables:

- `AWS_ACCESS_KEY_ID` - this is the `Access key ID`
- `AWS_SECRET_ACCESS_KEY` - this is the `Secret access key`

You can then send two distinct types of messages by hiting two different endpoints:

- `GET /send/work`
- `GET /send/nothing`

### Worker Tier

A sample `Worker` app is provided in [samples/SampleWorker](samples/SampleWorker).

If you wish to run the `Worker` without deploying to `AWS Beanstalk` you can leverage my [Beanstalk Seeder][beanstalk-seeder] project.

#### Add the middleware to the Worker

In the `Configure` method of your `Startup` class:

```csharp
public void Configure(IApplicationBuilder app)
{
    app.UseHeaderRouting();

    // Abbreviated for clarity
}
```

#### Use a matching route on a Controller Action

```csharp
// This is important, we do not want a prefix in front of the action's route
[Route("")]
public class SomeController : Controller
{
  // The route has to match the argument given to AddRoutingAttribute
  [HttpPost("task-name")]
  public async Task<IActionResult> SomeMethod(StronglyTypedMessage model)
  {
      // Abbreviated for clarity
  }
}
```

## Limitations

- Does not support [periodic tasks][periodic-tasks]
  - It could be added fairly easily if required
- `netstandard2.0` and above only
  - `.NET` is a [second][no-worker-tier] class [citizen][no-environment-variables] on `Elastic Beanstalk`, for this reason I recommend creating `Docker` images so that you can take full advantage of the platform.

[sqs-message-attributes]: https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-message-attributes.html
[periodic-tasks]: https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/using-features-managing-env-tiers.html#worker-periodictasks
[no-worker-tier]: https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/concepts.platforms.html#concepts.platforms.net
[no-environment-variables]: https://stackoverflow.com/questions/40127703/aws-elastic-beanstalk-environment-variables-in-asp-net-core-1-0
[available-regions]: http://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html#concepts-available-regions
[beanstalk-seeder]: https://github.com/gabrielweyer/beanstalk-seeder
[nuget-badge]: https://img.shields.io/nuget/v/BeanstalkWorker.SimpleRouting.svg?label=NuGet
[nuget]: https://www.nuget.org/packages/BeanstalkWorker.SimpleRouting/
[myget-badge]: https://img.shields.io/myget/gabrielweyer-pre-release/v/BeanstalkWorker.SimpleRouting.svg?label=MyGet
[myget]: https://www.myget.org/feed/gabrielweyer-pre-release/package/nuget/BeanstalkWorker.SimpleRouting
[app-veyor]: https://ci.appveyor.com/project/GabrielWeyer/simple-routing
[app-veyor-shield]: https://ci.appveyor.com/api/projects/status/github/gabrielweyer/simple-routing?branch=master&svg=true
