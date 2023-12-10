namespace dotnet8.TimeConfiguration;
public class TimeConfigurationOptions
{
    public const string SectionName = "TimeProvider";
    public int IntervalSeconds { get; set; } = 2;
}
