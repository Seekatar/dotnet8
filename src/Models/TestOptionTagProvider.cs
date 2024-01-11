namespace dotnet8.Models;

internal static class TestOptionTagProvider
{
    public static void RecordTags(ITagCollector collector, TestOptions options)
    {
        // could also use .NET 8 Redaction
        collector.Add(nameof(options.MasterConnectionString), options?.MaskedMasterConnectionString);
        collector.Add(nameof(options.NullSecret), options?.NullSecret ?? "<null>");
    }
}
