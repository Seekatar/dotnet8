using System.Globalization;
using dotnet8.Configuration;

namespace dotnet8.TimeConfiguration;

public class TimeProvider : IGenericConfigurationProvider<TimeOptions>
{
    public static string SectionName => "TimeProvider";

    public void Initialize(TimeOptions? options, Action<IDictionary<string, string?>> onReload)
    {
        options ??= new TimeOptions();

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
