namespace dotnet8.TimeConfiguration;
public class TimeConfigurationOptions
{
    public const string SectionName = "TimeProvider";

    /// <summary>
    /// The interval in seconds to update the time configuration for WhatTimeIsIt.
    /// </summary>
    public int IntervalSeconds { get; set; } = 2;

    /// <summary>
    /// The interval in seconds to update the time configuration for WhatTimeWasIt that calls an HTTP endpoint.
    /// </summary>
    public int HttpIntervalSeconds { get; set; } = 2;
}
