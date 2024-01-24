namespace CloudflareCaptchaSolver;

public static class BrowserManagerInstaller
{
    public static IServiceCollection AddBrowserManager(this IServiceCollection services, Action<BrowserManagerConfiguration>? configure = null)
    {
        var configuration = new BrowserManagerConfiguration();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        services.AddSingleton<BrowserManager>();
        return services;
    }
}
