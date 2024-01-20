using Microsoft.AspNetCore.Mvc;

namespace CloudflareCaptchaSolver;

public static class CaptchaEndpointInstaller
{
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
        public string Key { get; set; } = string.Empty;
        public Guid CommandId { get; set; }
    }

    internal record GetCaptchaResultModel
    {
        public CommandExecutionStatus Status { get; set; }
        public string? Data { get; set; }
    }

    internal record PostCaptchaModel
    {
        public string Key { get; set; } = string.Empty;
        public CaptchaType Type { get; set; } = CaptchaType.Unknown;
        public string SiteKey { get; set; } = string.Empty;
        public Uri Url { get; set; } = default!;
    }

    internal record PostCaptchaResultModel
    {
        public CommandExecutionStatus Status { get; set; }
        public Guid CommandId { get; set; }
    }
}

