namespace CatatAja.Api.Middlewares;

public class TelegramSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secretToken;
    private readonly ILogger<TelegramSecurityMiddleware> _logger;

    public TelegramSecurityMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<TelegramSecurityMiddleware> logger)
    {
        _next = next;
        _secretToken = configuration["Telegram:SecretToken"] ?? throw new InvalidOperationException("Secret token for Telegram is not configured.");
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/webhook/telegram", StringComparison.OrdinalIgnoreCase))
        {
            var requestToken = context.Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();

            if (!string.Equals(requestToken, _secretToken, StringComparison.Ordinal))
            {
                _logger.LogWarning("Unauthorized access attempt with token: {Token}", requestToken);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }
}