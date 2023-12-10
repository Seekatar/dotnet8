using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet8.Configuration;

public interface IGenericConfigurationProvider
{
    public static T GetOptions<T>(IConfiguration configuration) where T : class, new() => configuration.GetSection(typeof(T).Name).Get<T>() ?? new T();

    public void Initialize(IConfiguration configuration, Action<IDictionary<string,string?>> onReload);

    public IDictionary<string,string?> Load() => throw new NotImplementedException();
}

public static class GenericConfigurationExtensions
{
    public static IConfigurationBuilder AddGenericConfiguration<TProvider>(
        this IConfigurationBuilder builder) where TProvider : IGenericConfigurationProvider, new()
    {
        var tempConfig = builder.Build();

        return builder.Add(new GenericConfigurationSource<TProvider>(tempConfig));
    }
}

