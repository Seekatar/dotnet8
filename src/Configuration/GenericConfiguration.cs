namespace dotnet8.Configuration;

// this file has the standard classes for creating a custom configuration provider

/// <summary>
/// Standard IConfiguration source for the generic configuration provider
/// </summary>
/// <typeparam name="TProvider"></typeparam>
internal class GenericConfigurationSource<TProvider> : IConfigurationSource where TProvider : IGenericConfigurationProvider, new()
{
    private IConfiguration _configuration;

    public GenericConfigurationSource(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new GenericConfigurationProvider<TProvider>(_configuration);
    }
}

/* ConfigurationProvider source
 https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration/src/ConfigurationProvider.cs

 Looking at the https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationProvider.cs
 and its base https://github.com/dotnet/runtime/blob/437611885c1211a3240497a93ea85f57d54bfea2/src/libraries/Microsoft.Extensions.Configuration.FileExtensions/src/FileConfigurationProvider.cs
 they never lock anything. On reload, they create a new dictionary (ConfigurationProvider.Data) and then swap it in.
*/
/// <summary>
/// Standard ConfigurationProvider implementation the uses a generic class to load data
/// </summary>
/// <typeparam name="TProvider"></typeparam>
internal class GenericConfigurationProvider<TProvider> : ConfigurationProvider where TProvider : IGenericConfigurationProvider, new()
{
    private readonly TProvider _provider;
    public GenericConfigurationProvider(IConfiguration configuration)
    {
        _provider = new TProvider();

        _provider.Initialize(configuration, Reload);
    }

    private void Reload(IDictionary<string, string?>? dictionary)
    {
        if (dictionary == null)
        {
            return;
        }
        Data = dictionary;
        OnReload();
    }

    public override void Load()
    {
        var newData = _provider.Load();
        if (newData != null)
        {
            Data = newData;
        }
    }
}
