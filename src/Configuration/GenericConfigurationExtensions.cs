namespace dotnet8.Configuration;

/// <summary>
/// Provides extension methods for adding generic configuration to an <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class GenericConfigurationExtensions
{
    /// <summary>
    /// Adds generic configuration using the specified provider to the configuration builder.
    /// </summary>
    /// <typeparam name="TProvider">The type of the generic configuration provider.</typeparam>
    /// <param name="builder">The configuration builder.</param>
    /// <returns>The configuration builder with the generic configuration added.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static IConfigurationBuilder AddGenericConfiguration<TProvider>(
        this IConfigurationBuilder builder) where TProvider : IGenericConfigurationProvider, new()
    {
        var tempConfig = builder.Build();

        return builder.Add(new GenericConfigurationSource<TProvider>(tempConfig));
    }
}

