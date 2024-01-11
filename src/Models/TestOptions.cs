using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace dotnet8.Models;

public partial class TestOptions
{
    public static string SectionName = "TestOptions";

    [Required]
    [LogPropertyIgnore] // .NET 8
    public required string MasterConnectionString { get; set; }
    [Required]
    [LogPropertyIgnore] // .NET 8
    public required string ClientConnectionString { get; set; }
    [Required]
    public required string SshKey { get; set; }
    [Required]
    [LogPropertyIgnore] // .NET 8
    public required string Password { get; set; }
    public string? NullSecret { get; set; }

    public string MaskedMasterConnectionString => MaskConnectionString(MasterConnectionString);
    public string MaskedClientConnectionString => MaskConnectionString(ClientConnectionString);

    // .NET 6 added LoggerMessageAttribute
    [LoggerMessage(Level = LogLevel.Information, Message = "Options: {MasterConnectionString} {ClientConnectionString2}")]
    public static partial void LogOptions(ILogger logger, string MasterConnectionString, string ClientConnectionString2);

    // LogProperties is .NET 8. Don't use parameters in the Message string
    [LoggerMessage(Level = LogLevel.Information, Message = nameof(LogOptionsWithLogProperties))]
    public static partial void LogOptionsWithLogProperties(ILogger logger,
                                [LogProperties] // .NET 8
                                TestOptions options);

    [LoggerMessage(Level = LogLevel.Information, Message = nameof(LogOptionsWithLogPropertiesSkipNull))]
    public static partial void LogOptionsWithLogPropertiesSkipNull(ILogger logger,
                                [LogProperties(SkipNullProperties = true, OmitReferenceName = true)] // .NET 8
                                TestOptions options);

    [LoggerMessage(Level = LogLevel.Information, Message = nameof(LogOptionsWithLogProvider))]
    public static partial void LogOptionsWithLogProvider(ILogger logger,
                                [TagProvider(typeof(TestOptionTagProvider), nameof(TestOptionTagProvider.RecordTags))] // .NET 8
                                TestOptions options);

    internal static string MaskConnectionString(string? connectionString)
    {
        return Seekatar.OptionToStringGenerator.OptionsToStringAttribute.Format(connectionString, regex: "Password=(?<password>[^;]+);");
    }
}
