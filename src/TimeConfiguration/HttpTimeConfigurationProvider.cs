using System.Globalization;
using dotnet8.Configuration;

namespace dotnet8.TimeConfiguration;

/// <summary>
/// Sample implementation of the IGenericConfigurationProvider that retrieves time configuration from an HTTP endpoint.
/// </summary>
public class HttpTimeConfigurationProvider : IGenericConfigurationProvider
{
    static readonly HttpClient _client = new();

    /// <summary>
    /// Initializes the configuration provider.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="onReload">The action to be executed when the configuration is reloaded.</param>
    public void Initialize(IConfiguration configuration, Action<IDictionary<string, string?>> onReload)
    {
        var options = IGenericConfigurationProvider.GetOptions<TimeConfigurationOptions>(configuration, TimeConfigurationOptions.SectionName);

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(options.HttpIntervalSeconds).TotalMilliseconds);
        timer.Elapsed += (sender, args) =>
        {
            onReload(Reload());
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    /// <summary>
    /// Initial load, grab current time.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string,string?>? Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeWasIt"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        return data;
    }

    /// <summary>
    /// On timer, loads the WhatTimeWasIt value into the configuration from an HTTP endpoint.
    /// </summary>
    /// <returns>A dictionary containing the time configuration.</returns>
    public IDictionary<string, string?> Reload()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeWasIt"] = _client.GetFromJsonAsync<TimeResponse>("http://localhost:5000/time")?.Result?.WhatTimeIsIt.ToString() ?? new DateTime().ToString();
        return data;
    }
}
