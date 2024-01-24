using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace CloudflareCaptchaSolver;

public class BrowserManager : IAsyncDisposable
{
    public BrowserManager(BrowserManagerConfiguration configuration, ILogger<BrowserManager> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _playwright = Playwright.CreateAsync().Result;
        _contexts = [];
    }

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

    public async ValueTask<string> Solve(Command command)
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

    private async ValueTask<string> Solve(IPage page, SolveCaptcha command)
    {
        await page.GotoAsync(command.Url.ToString());

        var frame = page.FrameByUrl(new Regex(@"\S+cloudflare\S+"));
        if (frame == null) return string.Empty;

        var checkbox = frame.Locator("#challenge-stage input[type=checkbox]");

        await checkbox.WaitForAsync(new LocatorWaitForOptions() { State = WaitForSelectorState.Visible });
        await checkbox.HoverAsync();
        await checkbox.ClickAsync();

        await Task.Delay(1000);

        return "EXAMPLE-TOKEN";
    }

    private async ValueTask<PageContext> GetNextPageContext()
    {
        var browser = _contexts
            .Where(c => !c.Dispose && _contexts.Count(cc => cc.Browser == c.Browser) < _configuration.PagePerBrowserInstance)
            .Select(c => c.Browser)
            .FirstOrDefault() ?? await _playwright.Chromium.LaunchAsync(new() { Headless = false });

        var activePageCount = _contexts.Count(c => c.Browser == browser);

        //todo: add proxy
        //var proxy = new Proxy
        //{
        //    Server = "http://myproxy.com:3128",
        //    Username = "user",
        //    Password = "pwd"
        //};

        var pageContext = new PageContext
        {
            Browser = browser,
            Page = await browser.NewPageAsync(), // new() { Proxy = proxy }
            Dispose = ++activePageCount >= _configuration.PagePerBrowserInstance,
        };

        _contexts.Add(pageContext);

        foreach (var context in _contexts.Where(c => c.Browser == pageContext.Browser))
        {
            context.Dispose = pageContext.Dispose;
        }

        return pageContext;
    }

    private async ValueTask DisposePageContext(PageContext pageContext)
    {
        if (!pageContext.Page.IsClosed) await pageContext.Page.CloseAsync();
        if (pageContext.Dispose && _contexts.Count(c => c.Browser == pageContext.Browser) <= 1)
        {
            await pageContext.Browser.DisposeAsync();
        }

        _contexts.Remove(pageContext);
    }

    private readonly BrowserManagerConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IPlaywright _playwright;
    private readonly List<PageContext> _contexts;
}

public class BrowserManagerConfiguration
{
    public IBrowserType? BrowserType { get; set; }
    public uint PagePerBrowserInstance { get; set; }
    public bool BrowserRestart { get; set; }
}

internal record PageContext
{
    public IBrowser Browser { get; set; } = default!;
    public IPage Page { get; set; } = default!;
    public bool Dispose { get; set; }
}
