using dotnet8.Configuration;
using dotnet8.TimeConfiguration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables()
    .AddGenericConfiguration<TimeConfigurationProvider,TimeConfigurationOptions>(); // must be last since it reads the config from prev values

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
    return new TimeResponse(config.GetValue<DateTime>("WhatTimeIsIt"), options.Value.IntervalSeconds);
})
.WithName("WhatTimeIsIt")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary, string? Zip = null)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string Zip { get; } = Zip ?? "";
}

record TimeResponse(DateTime WhatTimeIsIt, int IntervalSeconds);