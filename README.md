# .NET 8 Playground <!-- omit in toc -->

- [.NET 8 Features Explored](#net-8-features-explored)
- [Endpoints](#endpoints)
  - [GET /weatherforecast](#get-weatherforecast)
  - [GET /time](#get-time)
  - [GET /resilient](#get-resilient)
  - [GET /not-resilient](#get-not-resilient)
  - [GET /get-it](#get-get-it)
  - [GET /log-options](#get-log-options)
  - [GET /throw](#get-throw)
- [Testing Chiseled Containers](#testing-chiseled-containers)

This repo explores some of the more interesting .NET 8 and C# 12 features. It was originally created with this command after installing the .NET 8 preview SDK in a GitHub Codespaces container.

```powershell
dotnet new webapi -o src
```

Then the `Program.cs` was edited to add additional endpoints.

To run this locally, you can do `./run.ps1 run` or in the src folder `dotnet run`. `./run.ps1` also has `buildDocker` and `runDocker` commands for testing the Dockerfile.

> The [logging configuration](src/appsettings.json#L23) logs to [Seq](https://datalust.co/seq) running locally to be able to see the structured logging output. It's easiest to run it in a container like this `docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest`. Doc is [here](https://docs.datalust.co/v5/docs/getting-started-with-docker#running-seq-in-a-docker-container)

## .NET 8 Features Explored

- [Primary Constructors for classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors) (doc)
  - [Widget.cs](src/Models/Widget.cs#L8)
- [Spread Operator for collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions) (doc)
  - [Program.cs](src/Program.cs#L126)
- [Default parameters for lambdas](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression) (doc)
  - [Program.cs](src/Program.cs#L65)
- [Alias of any type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/using-alias-types) (doc)
  - [Program.cs](src/Program.cs#L6)
- [HttpClientFactory Default Settings for all HttpClients](https://devblogs.microsoft.com/dotnet/dotnet-8-networking-improvements/#set-up-defaults-for-all-clients) (doc)
  - [Program.cs](src/Program.cs#L21)
- [HttpClientFactory Resilience](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/#standard-resilience-pipeline) (doc)
  - [Program.cs](src/Program.cs#L23)
- [Chiseled Containers](https://devblogs.microsoft.com/dotnet/announcing-dotnet-chiseled-containers/) (doc)
  - [Dockerfile](DevOps/Docker/Dockerfile#L2)
  - [run.ps1](run.ps1#L27) can override to not use chiseled containers.
- [Logging Objects with LogProperties](https://andrewlock.net/customising-the-new-telemetry-logging-source-generator) (Andrew Lock's Blog)
  - [TestOptions.cs](src/Models/TestOptions.cs#L26)
  - [TestOptionTagProvider.cs](src/Models/TestOptionTagProvider.cs)
  - [Program.cs](src/Program.cs#L145)
- [IExceptionHandler](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#iexceptionhandler) (doc)
  - [Program.cs](src/Program.cs#L55)
  - [GlobalExceptionHandler.cs](src/ExceptionHandlers/GlobalExceptionHandler.cs)
- [Custom IConfiguration Providers](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-configuration-provider) (doc)
  - Not a .NET 8 feature, but added to this sample as a bonus!
  - [GenericConfiguration.cs](src/Configuration/GenericConfiguration.cs#L28)
  - [HttpTimeConfigurationProvider.cs](src/TimeConfiguration/HttpTimeConfigurationProvider.cs)
  - [TimeConfigurationProvider.cs](src/TimeConfiguration/TimeConfigurationProvider.cs)
  - [TimeConfigurationOptions.cs](src/TimeConfiguration/TimeConfigurationOptions.cs)

## Endpoints

### GET /weatherforecast

Classic example from the template, with default parameters for the lambda.

### GET /time

Gets time from two custom `IConfiguration` providers. By default `WhatTimeIsIt` is refreshed every 10 seconds and `WhatTimeWasIt` is refreshed every 60 seconds, by calling this endpoint on a timer.

### GET /resilient

Test of the new HttpClientFactory resilience features. This will call /get-it which fails two times, but then succeed.

### GET /not-resilient

Negative test of the new HttpClientFactory resilience features. This will call /get-it without retry, so it will return a 500.

### GET /get-it

For the resiliency endpoints testing. This succeeds on every third call. Adding `?reset=true` will reset the counter.

### GET /log-options

Test the `LogProperties` feature by logging a `TestOptions` object with various methods.

### GET /throw

Test the `IExceptionHandler` implementation by throwing an exception.

## Testing Chiseled Containers

Chiseled containers are used for runtime and are smaller, and have less attack surface. For this app, the size was about half.

```text
dotnet8   aspnet    ec4cee180262   19 hours ago    228MB
dotnet8   chiseled  5f50b98f51f4   25 hours ago    115MB
```

To test it, first build the images

```powershell
 ./run.ps1 buildDocker -AspNetVersion aspNetVersion=8.0.0-jammy -DockerTag aspnet
 ./run.ps1 buildDocker -DockerTag chiseled # defaults to chiseled
 ```

 Run the non-chiseled container.

 ```powershell
 ./run.ps1 runDocker -DockerTag aspnet

```

In another terminal try to attach. This should work. You can run usual Linux commands

```powershell
 docker exec -it dotnet8 bash
 docker exec -it -u root dotnet8 bash
 ```

Run the chiseled container.

 ```powershell
 ./run.ps1 runDocker -DockerTag chiseled
```

In another terminal try to attach. This should get an error like `OCI runtime exec failed: exec failed: unable to start container process: exec: "bash": executable file not found in $PATH: unknown`

```powershell
 docker exec -it dotnet8 bash
 docker exec -it -u root dotnet8 bash
```

One thing that will work is running the `dotnet` command.

```powershell
docker exec -it dotnet8 dotnet --info
```
