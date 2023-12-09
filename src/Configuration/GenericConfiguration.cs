using System.Globalization;

namespace dotnet8.Configuration;

internal class GenericConfigurationSource<TProvider, TOptions> : IConfigurationSource where TProvider : IGenericConfigurationProvider<TOptions>, new()
{
    private TOptions? _options;

    public GenericConfigurationSource(TOptions? options)
    {
        _options = options;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new GenericConfigurationProvider<TProvider,TOptions>(_options);
    }
}

/* ConfigurationProvider source
 https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration/src/ConfigurationProvider.cs

 Looking at the https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationProvider.cs
 and its base https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.FileExtensions/src/FileConfigurationProvider.cs
 they never lock anything. On reload, they create a new dictionary (ConfigurationProvider.Data) and then swap it in.
*/
internal class GenericConfigurationProvider<TProvider,TOptions> : ConfigurationProvider where TProvider : IGenericConfigurationProvider<TOptions>, new()
{
    private readonly TProvider _provider;
    public GenericConfigurationProvider(TOptions? options)
    {
        _provider = new TProvider();
        _provider.Initialize(options, Reload);
    }

    private void Reload(IDictionary<string, string?> dictionary)
    {
        Data = dictionary;
        OnReload();
    }

    public override void Load()
    {
        Data = _provider.Load();
    }
}
