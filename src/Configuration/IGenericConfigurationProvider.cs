namespace dotnet8.Configuration;

/// <summary>
/// Represents a provider for IConfiguration data.
/// </summary>
public interface IGenericConfigurationProvider
{
    /// <summary>
    /// Gets the options for IConfiguration.
    /// </summary>
    /// <typeparam name="T">The type of options to retrieve.</typeparam>
    /// <param name="configuration">The configuration object.</param>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <returns>The options object.</returns>
    public static T GetOptions<T>(IConfiguration configuration, string sectionName) where T : class, new() => configuration.GetSection(sectionName).Get<T>() ?? new T();

    /// <summary>
    /// Initializes the configuration provider
    /// </summary>
    /// <param name="configuration">The configuration object of previously configured providers.</param>
    /// <param name="onReload">The action to be executed when the configuration is reloaded.</param>
    public void Initialize(IConfiguration configuration, Action<IDictionary<string,string?>?> onReload);

    /// <summary>
    /// Loads the configuration data.
    /// </summary>
    /// <returns>The loaded configuration data.</returns>
    public IDictionary<string,string?>? Load() => null;
}

