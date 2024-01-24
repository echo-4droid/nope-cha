namespace CloudflareCaptchaSolver;

public static class CommandManagerInstaller
{
    public static IServiceCollection AddCommandManager(this IServiceCollection services, Action<CommandManagerConfiguration>? configure = null)
    {
        var configuration = new CommandManagerConfiguration();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        services.AddSingleton<CommandManager>();
        return services;
    }
}
