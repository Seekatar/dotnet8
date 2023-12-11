using dotnet8.Configuration;
using dotnet8.TimeConfiguration;
using Microsoft.Extensions.Options;

const string NotResilient = "NotResilient";
const string Resilient = "Resilient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// .NET8 Can set resiliency or other settings for _all_ with this
// builder.Services.ConfigureHttpClientDefaults(b => b.AddStandardResilienceHandler()); https://devblogs.microsoft.com/dotnet/dotnet-8-networking-improvements/#set-up-defaults-for-all-clients
builder.Services.AddHttpClient(NotResilient);
builder.Services.AddHttpClient(Resilient)
    .AddStandardResilienceHandler(options =>
    {
        // .NET8 (but resiliency is NuGet available to all .NET)
        // take the defaults, but can change them here
        // see https://devblogs.microsoft.com/dotnet/building-resilient-cloud-services-with-dotnet-8/#standard-resilience-pipeline
    });

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables()
    .AddGenericConfiguration<TimeConfigurationProvider>() // must be after others it reads the config from prev values
    .AddGenericConfiguration<HttpTimeConfigurationProvider>();

// for testing, normally you probably don't need this since it is used by AddGenericConfiguration
builder.Services.AddOptions<TimeConfigurationOptions>()
    .Bind(builder.Configuration.GetSection(TimeConfigurationOptions.SectionName))
    .ValidateDataAnnotations();

var app = builder.Build();

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

// .NET8 default lambda parameter for zip
app.MapGet("/weatherforecast", (string zip = "30022") =>
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
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/time", (IConfiguration config, IOptions<TimeConfigurationOptions> options) =>
{
    return new TimeResponse(config.GetValue<DateTime>("WhatTimeIsIt"), config.GetValue<DateTime>("WhatTimeWasIt"), options.Value.IntervalSeconds);
})
.WithName("WhatTimeIsIt")
.WithOpenApi();

// this will work since it does retry
app.MapGet("/resilient", async (HttpRequest request, IHttpClientFactory clientFactory) => {
    var client = clientFactory.CreateClient(Resilient);
    var host = request.Host.Value;
    var scheme = request.Scheme;
    await client.GetAsync($"{scheme}://{host}/get-it?reset=true");
    var response = await client.GetAsync($"{scheme}://{host}/get-it");
    return response.StatusCode;
})
.WithName("Resilient")
.WithOpenApi();

// this will fail since it doesn't retry
app.MapGet("/not-resilient", async (HttpRequest request, IHttpClientFactory clientFactory) => {
    var client = clientFactory.CreateClient(NotResilient);
    var host = request.Host.Value;
    var scheme = request.Scheme;
    await client.GetAsync($"{scheme}://{host}/get-it?reset=true");
    var response = await client.GetAsync($"{scheme}://{host}/get-it");
    return response.StatusCode;
})
.WithName("NotResilient")
.WithOpenApi();

// simulate failures for /resilient and /not-resilient
int i = 1;
app.MapGet("/get-it", (bool reset = false) => {
    if (reset) {
        i = 1; return Results.Ok(i);
    }
    var ret = i++ % 3 == 0 ? Results.Ok(i - 1) : Results.StatusCode(500);
    System.Diagnostics.Debug.WriteLine($"Returning {ret} for {i - 1}");
    return ret;
})
.WithName("GetIt")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string? Zip = null)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Zip { get; } = Zip ?? "";
}

record TimeResponse(DateTime WhatTimeIsIt, DateTime WhatTimeWasIt, int IntervalSeconds);