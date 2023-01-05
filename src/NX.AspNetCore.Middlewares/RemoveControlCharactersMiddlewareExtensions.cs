using Microsoft.AspNetCore.Builder;

namespace NX.AspNetCore.Middlewares;

public static class RemoveControlCharactersMiddlewareExtensions
{
    /// <summary>
    /// 特殊文字をリクエストボディから削除するミドルウェアを使用します。
    /// </summary>
    public static IApplicationBuilder UseRemoveControlCharacters(this IApplicationBuilder app)
    {
        app.UseMiddleware<RemoveControlCharactersMiddleware>();
        return app;
    }
}
