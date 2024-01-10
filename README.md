# .NET 8 Playground <!-- omit in toc -->

- [.NET 8 and C# 12 Features Explored](#net-8-and-c-12-features-explored)
- [Endpoints](#endpoints)
  - [GET /weatherforecast](#get-weatherforecast)
  - [GET /time](#get-time)
  - [GET /resilient](#get-resilient)
  - [GET /not-resilient](#get-not-resilient)
  - [GET /get-it](#get-get-it)
  - [GET /log-options](#get-log-options)
  - [GET /throw](#get-throw)
  - [GET /random](#get-random)
- [Testing Chiseled Containers](#testing-chiseled-containers)
- [Links](#links)

> .NET 7 playground is [here](https://github.com/seekatar/dotnet7)

This repo explores some of the more interesting .NET 8 and C# 12 features. It was originally created with this command after installing the .NET 8 preview SDK in a GitHub Codespaces container.

```powershell
dotnet new webapi -o src
```

Then the `Program.cs` was edited to add additional endpoints.

To run this locally, you can do `./run.ps1 run` or in the src folder `dotnet run`. `./run.ps1` also has `buildDocker` and `runDocker` commands for testing the Dockerfile.

> The [logging configuration](src/appsettings.json#L23) logs to [Seq](https://datalust.co/seq) running locally to be able to see the structured logging output. It's easiest to run it in a container like this `docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest`. Doc is [here](https://docs.datalust.co/v5/docs/getting-started-with-docker#running-seq-in-a-docker-container)

## .NET 8 and C# 12 Features Explored

- [Primary Constructors for classes](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors) (doc)
  - [Widget.cs](src/Models/Widget.cs#L8)
- [Spread Operator for collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions) (doc)
  - [Program.cs](src/Program.cs#L154)
- [Default parameters for lambdas](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression) (doc)
  - [Program.cs](src/Program.cs#L90)
- [Alias of any type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/using-alias-types) (doc)
  - [Program.cs](src/Program.cs#L11)
- [HttpClientFactory Default Settings for all HttpClients](https://devblogs.microsoft.com/dotnet/dotnet-8-networking-improvements/#set-up-defaults-for-all-clients) (doc)
  - [Program.cs](src/Program.cs#L26)
- [HttpClientFactory Resilience](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/#standard-resilience-pipeline) (doc)
  - [Program.cs](src/Program.cs#L28)
  - [Program.cs /resilient](src/Program.cs#L115)
  - [Program.cs /not-resilient](src/Program.cs#L126)
- [Logging Objects with LogProperties](https://andrewlock.net/customising-the-new-telemetry-logging-source-generator) (Andrew Lock's Blog)
  - [TestOptions.cs](src/Models/TestOptions.cs#L26)
  - [TestOptionTagProvider.cs](src/Models/TestOptionTagProvider.cs#L3)
  - [Program.cs](src/Program.cs#L159)
- [IExceptionHandler](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0#iexceptionhandler) (doc)
  - [Program.cs](src/Program.cs#L59)
  - [GlobalExceptionHandler.cs](src/ExceptionHandlers/GlobalExceptionHandler.cs#L10)
- [Keyed Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-8.0#keyed-services) (doc)
  - [Program.cs Registered](src/Program.cs#62)
  - [Program.cs Used](src/Program.cs#191)
- [New RandomNumberGenerator methods](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=net-8.0) (doc)
  - I stumbled on this from the blog in the links below. The MS announcement doesn't mention all the new methods.
  - [Program.cs](src/Program.cs#L183)
- [Chiseled Containers](https://devblogs.microsoft.com/dotnet/announcing-dotnet-chiseled-containers/) (doc)
  - [Dockerfile](DevOps/Docker/Dockerfile#L2)
  - [run.ps1](run.ps1#L27) can override to not use chiseled containers.
- [Custom IConfiguration Providers](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-configuration-provider) (doc)
  - Not a .NET 8 feature, but added to this sample as a bonus!
  - [GenericConfiguration.cs](src/Configuration/GenericConfiguration.cs#L35)
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

### GET /random

Returns values from the new RandomNumberGenerator methods.

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

## Links

- [What's new in .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [What's new in C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)
- [What's new in System.Text.Json in .NET 8](https://devblogs.microsoft.com/dotnet/system-text-json-in-dotnet-8/) mostly around source generators
- [.NET 2023 Conf Video Playlist](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU)
  - [In .NET 8, ASP.NET Ate](https://www.youtube.com/watch?v=eWjtKwRIc54&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=16) shows SignalR stateful reconnect
  - [What's new in C# 12](https://www.youtube.com/watch?v=by-GL-SjHdc&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=6&pp=iAQB) Mads & Dustin
  - [Building resilient cloud services with .NET 8](https://www.youtube.com/watch?v=BDZpuFI8mMM&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=14) shows HttpClientFactory resilience
  - [Let’s catch up with C#! Exciting new features in C# 9, 10, 11 and 12!](https://www.youtube.com/watch?v=Ci-FTnC-j84&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=35)
  - [ASP.NET Basics for Experts](https://www.youtube.com/watch?v=GDCMiBu_2gI&list=PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU&index=23) quick review of new ASP.NET features
- [New RandomNumberGenerator methods](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator?view=net-8.0) (doc)
  - Henrique Siebert Domareski's [Blog on them](https://henriquesd.medium.com/net-8-new-randomness-methods-f2422f55320f)
  - [GetHexString](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.gethexstring?view=net-8.0)
  - [GetItems](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.getitems?view=net-8.0)
  - [GetString](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.getstring?view=net-8.0)
  - [Shuffle](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator.shuffle?view=net-8.0)

- Custom Configuration Providers
  - Andrew Lock [Creating a custom ConfigurationProvider in ASP.NET Core to parse YAML](https://andrewlock.net/creating-a-custom-iconfigurationprovider-in-asp-net-core-to-parse-yaml/)
  - MS Sample [Implement a custom configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-configuration-provider)