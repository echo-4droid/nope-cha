namespace CloudflareCaptchaSolver;

/// <summary>
/// Отчёт о выполнении команды
/// </summary>
public record CommandExecutionReport
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="command"></param>
    public CommandExecutionReport(Command command)
    {
        Command = command;
        Status = CommandExecutionStatus.Queued;
    }

    /// <summary>
    /// Команда
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// Статус выполнения команды
    /// </summary>
    public CommandExecutionStatus Status { get; internal set; }

    /// <summary>
    /// Результат выполнения команды
    /// </summary>
    public object? Result { get; internal set; }
}
