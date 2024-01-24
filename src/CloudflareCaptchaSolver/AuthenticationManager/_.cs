namespace CloudflareCaptchaSolver;

public static class AuthenticationManagerInstaller
{
    public static IServiceCollection AddAuthenticationManager(this IServiceCollection services)
    {
        services.AddSingleton<AuthenticationManager>();
        return services;
    }
}
