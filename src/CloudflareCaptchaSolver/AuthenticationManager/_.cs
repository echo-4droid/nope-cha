namespace CloudflareCaptchaSolver;

/// <summary>
/// 
/// </summary>
public static class AuthenticationManagerInstaller
{
    /// <summary>
    /// Добавляет менеджер аутентификации в коллекцию сервисов
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns></returns>
    public static IServiceCollection AddAuthenticationManager(this IServiceCollection services)
    {
        services.AddSingleton<AuthenticationManager>();

        return services;
    }
}
