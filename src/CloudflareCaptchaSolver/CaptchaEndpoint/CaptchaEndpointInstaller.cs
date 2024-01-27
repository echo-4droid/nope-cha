using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CloudflareCaptchaSolver;

/// <summary>
/// 
/// </summary>
public static class CaptchaEndpointInstaller
{
    /// <summary>
    /// Добавляет сконфигурированный Swagger
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <returns></returns>
    public static IServiceCollection AddCaptchaSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(cfg =>
        {
            cfg.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Cloudflare Captcha Solver API",
                Version = "v1",
                Description = ""
            });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            cfg.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>
    /// Добавляет эндпоинты для предоставления доступа к сервису
    /// </summary>
    /// <param name="routes"></param>
    public static void MapCaptchaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/captcha").WithTags("Captcha");

        group.MapGet("/", async ([AsParameters] GetCaptchaModel model, [FromServices] CommandManager commandManager, [FromServices] AuthenticationManager authManager) =>
        {
            if (!authManager.HasAccess(model.Key, model.CommandId)) return Results.NotFound($"CommandId '{model.CommandId}' for key '{model.Key}' not found");

            var report = await commandManager.GetCommandExecutionReport(model.CommandId);
            if (report == null) return Results.NotFound($"Task '{model.CommandId}' is not found");

            return TypedResults.Ok(new GetCaptchaResultModel() { Status = report.Status, Data = report.Result?.ToString() });
        })
        .WithName("CaptchaResolve")
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces<GetCaptchaResultModel>(StatusCodes.Status200OK)
        .WithOpenApi();

        group.MapPost("/", async ([FromBody] PostCaptchaModel model, [FromServices] CommandManager commandManager, [FromServices] AuthenticationManager authManager) =>
        {
            if (!authManager.Authenticate(model.Key)) return Results.NotFound($"Key '{model.Key}' not found");

            var commandId = await commandManager.Enqueue(new SolveCaptcha()
            {
                Key = model.Key,
                Type = model.Type,
                SiteKey = model.SiteKey,
                Url = model.Url,
            });

            return TypedResults.Accepted("/captcha", new PostCaptchaResultModel() { Status = CommandExecutionStatus.Queued, CommandId = commandId });
        })
        .WithName("SolveCaptcha")
        .Produces<string>(StatusCodes.Status404NotFound)
        .Produces<PostCaptchaResultModel>(StatusCodes.Status200OK)
        .WithOpenApi();
    }

    internal record GetCaptchaModel
    {
        /// <summary>
        /// User API Key
        /// </summary>
        /// <example>402880824ff933a4014ff9345d7c0002</example>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Command Id received by POST request
        /// </summary>
        /// <example>317c55b1-ae5a-45dc-9297-3a87b8bb2c6b</example>
        public Guid CommandId { get; set; }
    }

    internal record GetCaptchaResultModel
    {
        /// <summary>
        /// Статус выполнения команды
        /// </summary>
        public CommandExecutionStatus Status { get; set; }

        /// <summary>
        /// Результат выполнения команды
        /// </summary>
        public string? Data { get; set; }
    }

    internal record PostCaptchaModel
    {
        /// <summary>
        /// User API Key
        /// </summary>
        /// <example>402880824ff933a4014ff9345d7c0002</example>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Captcha type
        /// </summary>
        /// <example>1</example>
        public CaptchaType Type { get; set; } = CaptchaType.Unknown;

        /// <summary>
        /// Captcha site key
        /// </summary>
        /// <example>0x4AAAAAAAAjq6WYeRDKmebM</example>
        public string SiteKey { get; set; } = string.Empty;

        /// <summary>
        /// Url to page with captcha
        /// </summary>
        /// <example>https://nopecha.com/demo/cloudflare</example>
        public Uri Url { get; set; } = default!;
    }

    internal record PostCaptchaResultModel
    {
        /// <summary>
        /// Статус выполнения команды
        /// </summary>
        public CommandExecutionStatus Status { get; set; }

        /// <summary>
        /// Идентификатор команды
        /// </summary>
        public Guid CommandId { get; set; }
    }
}

