using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NX.AspNetCore.Middlewares;

/// <summary>
/// リクエストボディ（JSON）に含まれる特殊文字を削除するミドルウェア
/// </summary>
public class RemoveControlCharactersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RemoveControlCharactersMiddleware> _logger;

    public RemoveControlCharactersMiddleware(
        RequestDelegate next,
        ILogger<RemoveControlCharactersMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("Initialized");
    }

    /// <summary>
    /// ミドルウェアの処理を実装します。
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        // このミドルウェアの処理を記述します（前処理）。
        _logger.LogInformation("Before");

        // 次のミドルウェアの処理を実行します。
        await _next(context);

        // このミドルウェアの処理を記述します（後処理）。
        _logger.LogInformation("After");
    }
}
