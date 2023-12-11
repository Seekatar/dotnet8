using dotnet8.Configuration;

namespace dotnet8.TimeConfiguration;

public class HttpTimeConfigurationProvider : IGenericConfigurationProvider
{
    static readonly HttpClient _client = new();

    public void Initialize(IConfiguration configuration, Action<IDictionary<string, string?>> onReload)
    {
        var options = IGenericConfigurationProvider.GetOptions<TimeConfigurationOptions>(configuration,"HttpTimeProvider");

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(options.IntervalSeconds).TotalMilliseconds);
        timer.Elapsed += (sender, args) =>
        {
            onReload(Reload());
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    public IDictionary<string, string?> Reload()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeWasIt"] = _client.GetFromJsonAsync<TimeResponse>("http://localhost:5000/time")?.Result?.WhatTimeIsIt.ToString() ?? new DateTime().ToString();
        return data;
    }
}
