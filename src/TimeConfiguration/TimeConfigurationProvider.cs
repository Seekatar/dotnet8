using System.Globalization;
using dotnet8.Configuration;

namespace dotnet8.TimeConfiguration;

public class TimeConfigurationProvider : IGenericConfigurationProvider<TimeConfigurationOptions>
{
    public static string SectionName => TimeConfigurationOptions.SectionName;

    public void Initialize(TimeConfigurationOptions? options, Action<IDictionary<string, string?>> onReload)
    {
        options ??= new TimeConfigurationOptions();

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
