namespace CloudflareCaptchaSolver;

public static class CommandManagerInstaller
{
    /// <summary>
    /// Добавляет менеджер команд в коллекцию сервисов
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configure">Делегат конфигурации</param>
    /// <returns></returns>
    public static IServiceCollection AddCommandManager(this IServiceCollection services, Action<CommandManagerConfiguration>? configure = null)
    {
        var configuration = new CommandManagerConfiguration();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        services.AddSingleton<CommandManager>();
        return services;
    }
}
