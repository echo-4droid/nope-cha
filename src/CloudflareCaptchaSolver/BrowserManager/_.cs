namespace CloudflareCaptchaSolver;

/// <summary>
/// 
/// </summary>
public static class BrowserManagerInstaller
{
    /// <summary>
    /// Добавляет менеджер аутентификации в коллекцию сервисов
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configure">Делегат конфигурации</param>
    /// <returns></returns>
    public static IServiceCollection AddBrowserManager(this IServiceCollection services, Action<BrowserManagerConfiguration>? configure = null)
    {
        var configuration = new BrowserManagerConfiguration();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        services.AddSingleton<BrowserManager>();
        return services;
    }
}
