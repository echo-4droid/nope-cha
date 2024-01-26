namespace CloudflareCaptchaSolver;

/// <summary>
/// Тип решаемой капчи
/// </summary>
public enum CaptchaType
{
    /// <summary>
    /// Неизвестно
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Cloudflare turnstile
    /// </summary>
    Turnstile = 1
}
