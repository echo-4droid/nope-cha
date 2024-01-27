using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace CloudflareCaptchaSolver;

/// <summary>
/// Реализует управление браузерами и страницами, решение капчи
/// </summary>
public partial class BrowserManager : IAsyncDisposable
{
    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="configuration">Конфигурация</param>
    /// <param name="logger">Логгер</param>
    /// <exception cref="ArgumentNullException"></exception>
    public BrowserManager(BrowserManagerConfiguration configuration, ILogger<BrowserManager> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _playwright = Playwright.CreateAsync().Result;
        _contexts = [];
    }

    /// <summary>
    /// Освобождает ресурсы
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        foreach (var contexts in _contexts.GroupBy(c => c.Browser))
        {
            foreach (var context in contexts)
            {
                if (!context.Page.IsClosed) await context.Page.CloseAsync();
            }

            await contexts.Key.DisposeAsync();
        }

        _playwright.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Решает капчу
    /// </summary>
    /// <param name="command">Команда на решение капчи</param>
    /// <returns>Токен решения капчи</returns>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> Solve(Command command)
    {
        var solveCommand = command as SolveCaptcha ?? throw new InvalidCastException(nameof(command));
        var pageContext = await GetNextPageContext();

        try
        {
            return await Solve(pageContext.Page, solveCommand);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
        finally
        {
            await DisposePageContext(pageContext);
        }
    }

    private readonly BrowserManagerConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IPlaywright _playwright;
    private readonly List<PageContext> _contexts;

    private async Task<string> Solve(IPage page, SolveCaptcha command)
    {
        await page.RouteAsync("**/*", async route =>
        {
            if (route.Request.ResourceType is not "document" && !CloudflareEnvironmentPattern().IsMatch(route.Request.Url))
            {
                _logger.LogInformation($"BLOCKED: {route.Request.Method} {route.Request.Url} '{route.Request.ResourceType}'");
                await route.AbortAsync();
            }
            else
            {
                _logger.LogInformation($"LOADED: {route.Request.Method} {route.Request.Url} '{route.Request.ResourceType}'");
                await route.ContinueAsync();
            }
        });

        await page.GotoAsync(command.Url.ToString());

        var frame = page.FrameByUrl(CloudflareEnvironmentPattern());
        if (frame == null) return string.Empty;

        var checkbox = frame.Locator("#challenge-stage input[type=checkbox]");

        await checkbox.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Visible, Timeout = 60000 });
        await checkbox.HoverAsync();
        await checkbox.ClickAsync();

        return "EXAMPLE-TOKEN";
    }

    private async Task<PageContext> GetNextPageContext()
    {
        var browser = _contexts
            .Where(c => !c.Dispose && _contexts.Count(cc => cc.Browser == c.Browser) < _configuration.PagePerBrowserInstance)
            .Select(c => c.Browser)
            .FirstOrDefault();

        if (browser == null)
        {
            var browserOptions = new BrowserTypeLaunchOptions() { Headless = true, Devtools = true };

            if (_configuration.Proxies.Count != 0)
            {
                browserOptions.Proxy = new Proxy
                {
                    Server = "http://overrided-proxy-server"
                };
            }

            browser = await _playwright.Chromium.LaunchAsync(browserOptions);
        }

        var activePageCount = _contexts.Count(c => c.Browser == browser);

        var options = new BrowserNewPageOptions();

        if (_configuration.Proxies.Count != 0)
        {
            var random = (new Random()).Next(_configuration.Proxies.Count);
            options.Proxy = new Proxy
            {
                Server = _configuration.Proxies[random].Server,
                Username = _configuration.Proxies[random].Username,
                Password = _configuration.Proxies[random].Password
            };
        }

        var pageContext = new PageContext
        {
            Browser = browser,
            Page = await browser.NewPageAsync(options),
            Dispose = _configuration.BrowserRestart && ++activePageCount >= _configuration.PagePerBrowserInstance,
        };

        _contexts.Add(pageContext);

        foreach (var context in _contexts.Where(c => c.Browser == pageContext.Browser))
        {
            context.Dispose = pageContext.Dispose;
        }

        _logger.LogInformation($"Run {_contexts.Select(c => c.Browser).Distinct().Count()} with {_contexts.Count} pages");

        return pageContext;
    }

    private async Task DisposePageContext(PageContext pageContext)
    {
        if (!pageContext.Page.IsClosed) await pageContext.Page.CloseAsync();
        if (pageContext.Dispose && _contexts.Count(c => c.Browser == pageContext.Browser) <= 1)
        {
            await pageContext.Browser.DisposeAsync();
        }

        _contexts.Remove(pageContext);

        _logger.LogInformation($"Run {_contexts.Select(c => c.Browser).Distinct().Count()} with {_contexts.Count} pages");
    }

    [GeneratedRegex(@"\S+cloudflare\S+|\S+challenge-platform\S+")]
    private static partial Regex CloudflareEnvironmentPattern();

    [GeneratedRegex(@"\S+challenges.cloudflare.com/turnstile\S+api.js\S*")]
    private static partial Regex CloudflareApiJsPattern();
}

/// <summary>
/// Конфигурация менеджера браузеров
/// </summary>
public record BrowserManagerConfiguration
{
    /// <summary>
    /// Максимальное количество открытых страниц на один браузер
    /// </summary>
    public uint PagePerBrowserInstance { get; set; }

    /// <summary>
    /// Закрывать браузер после достижения максимального количества страниц
    /// </summary>
    public bool BrowserRestart { get; set; }

    /// <summary>
    /// Список прокси серверов
    /// </summary>
    public List<ProxyConfiguration> Proxies { get; set; } = [];
}

/// <summary>
/// Конфигурация прокси сервера
/// </summary>
public record ProxyConfiguration
{
    /// <summary>
    /// Адрес сервера
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Логин (если требуется)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль (если требуется)
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

internal record PageContext
{
    public IBrowser Browser { get; set; } = default!;
    public IPage Page { get; set; } = default!;
    public bool Dispose { get; set; }
}
