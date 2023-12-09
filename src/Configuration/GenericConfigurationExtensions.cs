using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet8.Configuration;

public interface IGenericConfigurationProvider<TOptions>
{
    static abstract string SectionName { get; }

    public void Initialize(TOptions? configuration, Action<IDictionary<string,string?>> onReload);
    public IDictionary<string,string?> Load();
}

public static class GenericConfigurationExtensions
{
    public static IConfigurationBuilder AddGenericConfiguration<TProvider,TOptions>(
        this IConfigurationBuilder builder) where TProvider : IGenericConfigurationProvider<TOptions>, new()
    {
        var tempConfig = builder.Build();
        var options = tempConfig.GetSection(TProvider.SectionName).Get<TOptions>();

        return builder.Add(new GenericConfigurationSource<TProvider,TOptions>(options));
    }
}

