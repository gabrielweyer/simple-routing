# Elastic Beanstalk Worker Simple Routing

| Package | Release | Pre-release |
| --- | --- | --- |
| `BeanstalkWorker.SimpleRouting` | [![NuGet][nuget-badge]][nuget] | [![MyGet][myget-badge]][myget] |

| CI | Status | Platform(s) | Framework(s) | Test Framework(s) |
| --- | --- | --- | --- | --- |
| [AppVeyor][app-veyor] | [![Build Status][app-veyor-shield]][app-veyor] | `Windows` | `nestandard2.0` | `netcoreapp3.1` |

Allows to route a `SQS` message to a specific endpoint on the `Worker` instead of having a single endpoint handling all the messages.

Relies on the [SQS message attributes][sqs-message-attributes]. This is distributed via a `NuGet` package but the implementation is so simple that you can just copy the classes into your own solution if that works better for you.

## How it works

### Web Tier - Set the attribute on the message

```csharp
// Instantiate your model
StronglyTypedMessage model = new StronglyTypedMessage();

var sendMessageRequest = new SendMessageRequest
{
  // Serialize your model as JSON (you can use Newtonsoft.Json if you prefer)
  MessageBody = System.Text.Json.JsonSerializer.Serialize(model)
  // Set the QueueUrl and other properties as you see fit
};

// AddRoutingAttribute is an extension method in the "BeanstalkWorker.SimpleRouting" namespace
sendMessageRequest.MessageAttributes.AddRoutingAttribute("task-name");
```

#### Web sample project

A sample `Web` app is provided in [samples/SampleWeb](samples/SampleWeb).

You can send two distinct types of messages by hitting two different endpoints:

- `GET http://localhost:5000/send/work`
- `GET http://localhost:5000/send/nothing`

##### Configuration

Create a `iAM` user (if you don't have one already) which has access to `SQS`.

You'll need to configure four settings using [user secrets][secret-manager]:

- `Aws:RegionSystemName` - [region code][available-regions], for example `ap-southeast-2`
- `Aws:Queue:WorkerQueueUrl` - `URL` of the `SQS` queue, for example `https://sqs.ap-southeast-2.amazonaws.com/375985941080/dev-gabriel`
- `Aws:Queue:AccessKeyId` - this is the `Access key ID` of your `iAM user`
- `Aws:Queue:SecretAccessKey` - this is the `Secret access key` of your `iAM user`

### Worker Tier

#### Add the middleware to the Worker

In the `Configure` method of your `Startup` class:

```csharp
public void Configure(IApplicationBuilder app)
{
    // The simple routing middleware needs to be added **before** configuring endpoint routing
    app.UseHeaderRouting();

    app.UseRouting();

    app.UseEndpoints(endpoint => endpoint.MapControllers());
}
```

#### Use a matching route on a Controller Action

```csharp
// This is important, we do not want a prefix in front of the action's route
[Route("")]
[ApiController]
public class SomeController : ControllerBase
{
  // The route template has to match the argument given to AddRoutingAttribute
  [HttpPost("task-name")]
  public async Task<IActionResult> SomeMethod(StronglyTypedMessage model)
  {
      // Abbreviated for clarity
  }
}
```

#### Worker sample project

A sample `Worker` app is provided in [samples/SampleWorker](samples/SampleWorker).

If you wish to run the `Worker` without deploying to `AWS Beanstalk` you can leverage my [Beanstalk Seeder][beanstalk-seeder] project.

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
[secret-manager]: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.1&tabs=windows#secret-manager
