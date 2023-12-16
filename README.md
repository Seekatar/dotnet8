# .NET 8 Playground <!-- omit in toc -->

- [Features](#features)
- [Endpoints](#endpoints)
  - [GET /weatherforecast](#get-weatherforecast)
  - [GET /time](#get-time)
  - [GET /resilient](#get-resilient)
  - [GET /not-resilient](#get-not-resilient)
  - [GET /get-it](#get-get-it)

This was created with this command after installing the .NET 8 preview SDK in the GitHub Codespaces container.

```powershell
dotnet new webapi -o src
```

Then the `Program.cs` was edited to add some additional endpoints.

## Features

- [Primary Constructors](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/instance-constructors#primary-constructors)
- [Spread Operator for collections](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#collection-expressions)
- [Default parameters for lambdas](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions#input-parameters-of-a-lambda-expression)
- [Alias of any type](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/using-alias-types)
- HttpClientFactory
  - [Default Settings for all HttpClients](https://devblogs.microsoft.com/dotnet/dotnet-8-networking-improvements/#set-up-defaults-for-all-clients)
  - [HttpClientFactory Resilience](https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/#standard-resilience-pipeline)

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
