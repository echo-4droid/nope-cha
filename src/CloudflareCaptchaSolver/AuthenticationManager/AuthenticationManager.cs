using System.Collections.Concurrent;

namespace CloudflareCaptchaSolver;

/// <summary>
/// Заглушка аутентификации
/// </summary>
public class AuthenticationManager
{
    /// <summary>
    /// Конструктор класса
    /// </summary>
    /// <param name="logger">Логгер</param>
    /// <exception cref="ArgumentNullException"></exception>
    public AuthenticationManager(ILogger<CommandManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _access = [];
    }

    /// <summary>
    /// Проверяет возможность доступа к сервису по токену
    /// </summary>
    /// <param name="token">Токен</param>
    /// <returns></returns>
    public bool Authenticate(string token) => true;

    /// <summary>
    /// Проверяет возможность доступа к информации о выполнении команды по данному токену
    /// </summary>
    /// <param name="token">Токен</param>
    /// <param name="commandId">Идентификатор команды</param>
    /// <returns></returns>
    public bool HasAccess(string token, in Guid commandId)
    {
        if (_access.TryGetValue(token, out var commandIds))
        {
            return commandIds.Contains(commandId);
        }

        return false;
    }

    /// <summary>
    /// Связывает токен и идентификатор команды
    /// </summary>
    /// <param name="token">Токен</param>
    /// <param name="commandId">Идентификатор команды</param>
    public void Bind(string token, Guid commandId)
    {
        if (!_access.TryGetValue(token, out var commandIds))
        {
            commandIds = _access.AddOrUpdate(token, _ => new List<Guid>(), (_, exists) => exists);
        }

        commandIds.Add(commandId);
    }

    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, List<Guid>> _access;
}
