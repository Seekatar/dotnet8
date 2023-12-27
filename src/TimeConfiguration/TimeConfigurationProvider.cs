using System.Globalization;
using dotnet8.Configuration;

namespace dotnet8.TimeConfiguration;
/// <summary>
/// Sample implementation of the IGenericConfigurationProvider that retrieves current time
/// </summary>
public class TimeConfigurationProvider : IGenericConfigurationProvider
{
    /// <summary>
    /// Initializes the configuration provider.
    /// </summary>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="onReload">The action to be executed when the configuration is reloaded.</param>
    public void Initialize(IConfiguration configuration, Action<IDictionary<string, string?>?> onReload)
    {
        var options = IGenericConfigurationProvider.GetOptions<TimeConfigurationOptions>(configuration, TimeConfigurationOptions.SectionName);

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(options.IntervalSeconds).TotalMilliseconds);
        timer.Elapsed += (sender, args) =>
        {
            onReload(Load());
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    /// <summary>
    /// Loads the WhatTimeIsIt value into the configuration from system time
    /// </summary>
    /// <returns>A dictionary containing the time configuration.</returns>
    public IDictionary<string, string?>? Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeIsIt"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        return data;
    }
}
