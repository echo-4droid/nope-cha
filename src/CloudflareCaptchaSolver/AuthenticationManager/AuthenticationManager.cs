using System.Collections.Concurrent;

namespace CloudflareCaptchaSolver;

public class AuthenticationManager
{
    public AuthenticationManager(ILogger<CommandManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _access = [];
    }

    public bool Authenticate(string token) => true;

    public bool HasAccess(string token, in Guid commandId)
    {
        if (_access.TryGetValue(token, out var commandIds))
        {
            return commandIds.Contains(commandId);
        }

        return false;
    }

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
