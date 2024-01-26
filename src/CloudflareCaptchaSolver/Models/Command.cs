namespace CloudflareCaptchaSolver;

/// <summary>
/// Команда
/// </summary>
public abstract record Command
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>
    /// Токен пользователя
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

/// <summary>
/// Команда решения капчи
/// </summary>
public record SolveCaptcha : Command
{
    /// <summary>
    /// Тип капчи
    /// </summary>
    public CaptchaType Type { get; set; } = CaptchaType.Unknown;

    /// <summary>
    /// Ключ сайта
    /// </summary>
    public string SiteKey { get; set; } = string.Empty;

    /// <summary>
    /// Ссылка на страницу с капчей
    /// </summary>
    public Uri Url { get; set; } = default!;
}
