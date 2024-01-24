using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CloudflareCaptchaSolver;

public static class CaptchaEndpointInstaller
{
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

    public static void MapCaptchaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/captcha").WithTags("Captcha");

        group.MapGet("/", async ([AsParameters] GetCaptchaModel model, [FromServices] CommandManager manager) =>
        {
            var report = await manager.GetCommandExecutionReport(model.CommandId);

            if (report == null) return Results.BadRequest($"Task '{model.CommandId}' is not found");

            return TypedResults.Ok(new GetCaptchaResultModel() { Status = report.Status, Data = report.Result?.ToString() });
        })
        .WithName("CaptchaResolve")
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces<GetCaptchaResultModel>(StatusCodes.Status200OK)
        .WithOpenApi();

        group.MapPost("/", async ([FromBody] PostCaptchaModel model, [FromServices] CommandManager manager) =>
        {
            var commandId = await manager.Enqueue(new SolveCaptcha()
            {
                Key = model.Key,
                Type = model.Type,
                SiteKey = model.SiteKey,
                Url = model.Url,
            });

            return TypedResults.Accepted("/captcha", new PostCaptchaResultModel() { Status = CommandExecutionStatus.Queued, CommandId = commandId });
        })
        .WithName("SolveCaptcha")
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
        public CommandExecutionStatus Status { get; set; }
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
        public CommandExecutionStatus Status { get; set; }
        public Guid CommandId { get; set; }
    }
}

