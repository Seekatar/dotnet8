using Microsoft.Extensions.DependencyInjection.Extensions;

namespace dotnet8.Configuration;

public static class GenericConfigurationExtensions
{
    public static IConfigurationBuilder AddGenericConfiguration(
        this IConfigurationBuilder builder, string sectionName = "test")
    {
        var tempConfig = builder.Build();
        var options = tempConfig.GetSection(sectionName).Get<GenericConfigurationOptions>();

        return builder.Add(new GenericConfigurationSource(options));
    }
}

