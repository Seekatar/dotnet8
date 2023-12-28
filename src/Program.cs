using dotnet8.Configuration;
using dotnet8.ExceptionHandlers;
using dotnet8.TimeConfiguration;
using Microsoft.Extensions.Options;
using Serilog;

// .NET 8 alias for any type now, before only named types
using WidgetAlias = dotnet8.Models.Widget;

const string NotResilient = "NotResilient";
const string Resilient = "Resilient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// .NET8 Can set resiliency or other settings for _all_ with this
// https://devblogs.microsoft.com/dotnet/dotnet-8-networking-improvements/#set-up-defaults-for-all-clients
#if SetAddStandardResilienceHandlerOnAll
    builder.Services.ConfigureHttpClientDefaults(b => b.AddStandardResilienceHandler());
#else
    builder.Services.AddHttpClient(NotResilient);
    builder.Services.AddHttpClient(Resilient)
        .AddStandardResilienceHandler(options =>
        {
            // .NET8 (but resiliency is NuGet available to all .NET)
            // take the defaults, but can change them here
            // see https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/#standard-resilience-pipeline
        });
#endif

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables()
    .AddGenericConfiguration<TimeConfigurationProvider>() // must be after others it reads the config from prev values
    .AddGenericConfiguration<HttpTimeConfigurationProvider>();

builder.Host.UseSerilog((ctx,loggerConfig) => loggerConfig.ReadFrom.Configuration(builder.Configuration));

// for testing, normally you probably don't need this since it is used by AddGenericConfiguration
builder.Services.AddOptions<TimeConfigurationOptions>()
    .Bind(builder.Configuration.GetSection(TimeConfigurationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<TestOptions>()
    .Bind(builder.Configuration.GetSection(TestOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// register the exception handler and use it below
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// use the exception handlers
app.UseExceptionHandler(opt => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var client = app.MapGroup("/")
                .WithOpenApi();

// .NET8 default lambda parameter for zip
client.MapGet("/weatherforecast", (string zip = "30022") =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)],
            zip
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

client.MapGet("/time", (IConfiguration config, IOptions<TimeConfigurationOptions> options) =>
{
    return new TimeResponse(config.GetValue<DateTime>("WhatTimeIsIt"),
                            config.GetValue<DateTime>("WhatTimeWasIt"),
                            options.Value.IntervalSeconds,
                            options.Value.HttpIntervalSeconds);
})
.WithName("WhatTimeIsIt");

// this will work since it does retry
client.MapGet("/resilient", async (HttpRequest request, IHttpClientFactory clientFactory) => {
    var client = clientFactory.CreateClient(Resilient);
    var host = request.Host.Value;
    var scheme = request.Scheme;
    await client.GetAsync($"{scheme}://{host}/get-it?reset=true");
    var response = await client.GetAsync($"{scheme}://{host}/get-it");
    return response.StatusCode;
})
.WithName("Resilient");

// this will fail since it doesn't retry
client.MapGet("/not-resilient", async (HttpRequest request, IHttpClientFactory clientFactory) => {
    var client = clientFactory.CreateClient(NotResilient);
    var host = request.Host.Value;
    var scheme = request.Scheme;
    await client.GetAsync($"{scheme}://{host}/get-it?reset=true");
    var response = await client.GetAsync($"{scheme}://{host}/get-it");
    return response.StatusCode;
})
.WithName("NotResilient");

// simulate failures for /resilient and /not-resilient
int i = 1;
client.MapGet("/get-it", (bool reset = false) => {
    if (reset) {
        i = 1; return Results.Ok(i);
    }
    var ret = i++ % 3 == 0 ? Results.Ok(i - 1) : Results.StatusCode(500);
    System.Diagnostics.Debug.WriteLine($"Returning {ret} for {i - 1}");
    return ret;
})
.WithName("GetIt");

client.MapGet("/widget", () => {
    // .NET8 construct lists with brackets and it figures out the type
    List<WidgetAlias> widgetList = [new WidgetAlias("My Widget", 1), new WidgetAlias("My Widget", 2)];
    WidgetAlias[] widgetArray = [new WidgetAlias("My Widget", 3), new WidgetAlias("My Widget", 4)];

    // .NET8 spread operator
    WidgetAlias[] ret = [new WidgetAlias("My Widget", 0), .. widgetList, .. widgetArray];
    return ret; // try to return without ret gives ambiguous compiler error
})
.WithName("Widget");

client.MapGet("/log-options", (IOptionsSnapshot<TestOptions> options, ILogger<TestOptions> logger) => {

    // .NET 6 flavor that logs individual properties to the logs
    TestOptions.LogOptions(logger, TestOptions.MaskConnectionString(options.Value.MasterConnectionString),
                                   TestOptions.MaskConnectionString(options.Value.ClientConnectionString));

    // .NET 8 flavor that logs all properties to structured logging
    TestOptions.LogOptionsWithLogProperties(logger, options.Value);
    TestOptions.LogOptionsWithLogPropertiesSkipNull(logger, options.Value);
    TestOptions.LogOptionsWithLogProvider(logger, options.Value);

    return Results.Ok(new { Message = "Check the structured log output for multiple entries"});
})
.WithName("LogOptions");

client.MapGet("/throw", () => {
    throw new Exception("This is an exception");
})
.WithName("Throw");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string? Zip = null)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Zip { get; } = Zip ?? "";
}

record TimeResponse(DateTime WhatTimeIsIt, DateTime WhatTimeWasIt, int IntervalSeconds, int HttpIntervalSeconds);