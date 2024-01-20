namespace CloudflareCaptchaSolver;

public abstract record Command
{
    public Guid Id { get; internal set; } = Guid.NewGuid();
}

public record SolveCaptcha : Command
{
    public string Key { get; set; } = string.Empty;
    public CaptchaType Type { get; set; } = CaptchaType.Unknown;
    public string SiteKey { get; set; } = string.Empty;
    public Uri Url { get; set; } = default!;
}
