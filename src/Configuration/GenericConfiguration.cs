using System.Globalization;

namespace dotnet8.Configuration;

internal class GenericConfigurationSource : IConfigurationSource
{
    private readonly GenericConfigurationOptions _options;

    public GenericConfigurationSource(GenericConfigurationOptions? options)
    {
        _options = options ?? new GenericConfigurationOptions();
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new GenericConfigurationProvider(_options);
    }
}

/* ConfigurationProvider source
 https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration/src/ConfigurationProvider.cs

 Looking at the https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationProvider.cs
 and its base https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.FileExtensions/src/FileConfigurationProvider.cs
 they never lock anything. On reload, they create a new dictionary (ConfigurationProvider.Data) and then swap it in.
*/


internal class GenericConfigurationProvider : ConfigurationProvider // MS helper to do most of IConfigurationProvider impl
{
    private readonly GenericConfigurationOptions _options;

    public GenericConfigurationProvider(GenericConfigurationOptions options)
    {
        _options = options;
    }

    public override void Load()
    {
        Data = Load(_options);

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(2).TotalMilliseconds);
        timer.Elapsed += (sender, args) =>
        {
            Data = Load(_options);
            OnReload(); // this tells IConfiguration to update, otherwise Data is updated, but nothing else
        };
        timer.AutoReset = true;
        timer.Enabled = true;
    }

    private IDictionary<string, string?> Load(GenericConfigurationOptions options)
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        data["WhatTimeIsIt"] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        return data;
    }

    // just for debugging and learning to set breakpoints
    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
    {
        return base.GetChildKeys(earlierKeys, parentPath);
    }

    // just for debugging and learning to set breakpoints
    public override bool TryGet(string key, out string? value)
    {
        bool ret = base.TryGet(key, out value);
        return ret;
    }

}
