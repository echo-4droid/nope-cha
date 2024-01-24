namespace CloudflareCaptchaSolver;

public abstract record Query
{
    public Guid CommandId { get; set; }
}

public record CommandExecution : Query
{
    public string Key { get; set; } = string.Empty;
}
