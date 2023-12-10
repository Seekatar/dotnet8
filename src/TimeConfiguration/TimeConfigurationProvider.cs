using System.Globalization;
using dotnet8.Configuration;
using Microsoft.Extensions.Options;

namespace dotnet8.TimeConfiguration;

public class TimeConfigurationProvider : IGenericConfigurationProvider
{
    public void Initialize(IConfiguration configuration, Action<IDictionary<string, string?>> onReload)
    {
        var options = IGenericConfigurationProvider.GetOptions<TimeConfigurationOptions>(configuration);

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(options.IntervalSeconds).TotalMilliseconds);
        timer.Elapsed += (sender, args) =>
        {
            onReload(Load());
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    public IDictionary<string, string?> Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeIsIt"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        return data;
    }
}
