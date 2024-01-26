namespace CloudflareCaptchaSolver;

/// <summary>
/// Статус выполнения команды
/// </summary>
public enum CommandExecutionStatus
{
    /// <summary>
    /// Добавлена в очередь
    /// </summary>
    Queued = 1,

    /// <summary>
    /// В работе
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Успешно выполнена
    /// </summary>
    Success = 3,

    /// <summary>
    /// Не успешно выполнена
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Внутренняя ошибка при обработке
    /// </summary>
    Error = 5
}
