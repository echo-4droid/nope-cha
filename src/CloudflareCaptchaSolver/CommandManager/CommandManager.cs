using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CloudflareCaptchaSolver;

public class CommandManager
{
    public CommandManager(CommandManagerConfiguration configuration, BrowserManager browserManager, AuthenticationManager authManager, ILogger<CommandManager> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _browserManager = browserManager ?? throw new ArgumentNullException(nameof(browserManager));
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reports = [];

        var queueOptions = new BoundedChannelOptions(_configuration.ChannelCapacity)
        {
            FullMode = _configuration.ChannelFullMode
        };
        _queue = Channel.CreateBounded<Command>(queueOptions);
        Task.Run(ProcessQueue);
    }

    public async Task<Guid> Enqueue(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (_reports.TryAdd(command.Id, new CommandExecutionReport(command)))
        {
            _authManager.Bind(command.Key, command.Id);
            await _queue.Writer.WriteAsync(command);

            return command.Id;
        }

        throw new InvalidOperationException();
    }

    public Task<CommandExecutionReport?> GetCommandExecutionReport(Guid commandId)
    {
        _reports.TryGetValue(commandId, out var report);
        return Task.FromResult(report);
    }

    private readonly CommandManagerConfiguration _configuration;
    private readonly BrowserManager _browserManager;
    private readonly AuthenticationManager _authManager;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<Guid, CommandExecutionReport> _reports;
    private readonly Channel<Command> _queue;

    private async Task ProcessQueue()
    {
        while (true)
        {
            var command = await _queue.Reader.ReadAsync();
            if (command is SolveCaptcha captcha)
            {
                try
                {
                    _reports[command.Id].Status = CommandExecutionStatus.Processing;
                    //var result = await _browserManager.Solve(captcha);
                    //_reports[command.Id].Status = !string.IsNullOrEmpty(result) ? CommandExecutionStatus.Success : CommandExecutionStatus.Failed;
                    //_reports[command.Id].Result = result;
                    _ = _browserManager.Solve(captcha).ContinueWith(async task =>
                    {
                        var result = await task;
                        _reports[command.Id].Status = !string.IsNullOrEmpty(result) ? CommandExecutionStatus.Success : CommandExecutionStatus.Failed;
                        _reports[command.Id].Result = result;
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Command '{Command}' processing failed", $"{command.Id}");
                    _reports[command.Id].Status = CommandExecutionStatus.Error;
                }
            }
        }
    }
}

public class CommandManagerConfiguration
{
    public int ChannelCapacity { get; set; } = 10;
    public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;
}
