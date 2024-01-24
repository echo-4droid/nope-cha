namespace CloudflareCaptchaSolver;

public record CommandExecutionReport
{
    public CommandExecutionReport(Command command)
    {
        Command = command;
        Status = CommandExecutionStatus.Queued;
    }

    public Command Command { get; }
    public CommandExecutionStatus Status { get; internal set; }
    public bool Success => Status == CommandExecutionStatus.Success;
    public object? Result { get; internal set; }
}
