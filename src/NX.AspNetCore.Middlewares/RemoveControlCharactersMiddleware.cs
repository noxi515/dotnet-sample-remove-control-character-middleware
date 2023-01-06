using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NX.AspNetCore.Middlewares;

/// <summary>
/// リクエストボディ（JSON）に含まれる制御文字を削除するミドルウェア
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
    }

    /// <summary>
    /// ミドルウェアの処理を実装します。
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
        // リクエストのContentTypeがJSON以外の場合は何もしない
        if (!context.Request.HasJsonContentType())
        {
            await _next(context);
            return;
        }

        try
        {
            // 特殊文字を削除したJSONデータを作成してリクエストボディのStreamを差し替える
            await RemoveControlCharactersInRequestBodyAsync(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "RemoveControlCharacter failed.");
            throw;
        }

        // 次のミドルウェアの処理を実行します。
        await _next(context);
    }

    /// <summary>
    /// リクエストボディを全て読み込んで制御文字を削除したJSONバイト配列とし、中身を差し替える
    /// </summary>
    private static async Task RemoveControlCharactersInRequestBodyAsync(HttpContext context)
    {
        using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms, context.RequestAborted);

        var bytes = RemoveControlCharactersInJsonString(ms.ToArray());
        context.Request.Body = new MemoryStream(bytes);
    }

    /// <summary>
    /// JSONバイト配列から制御文字を削除したJSONバイト配列を返す
    /// </summary>
    internal static byte[] RemoveControlCharactersInJsonString(byte[] source)
    {
        var reader = new Utf8JsonReader(source, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 0
        });
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    // Do nothing
                    break;
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;
                case JsonTokenType.PropertyName:
                    writer.WritePropertyName(reader.ValueSpan);
                    break;
                case JsonTokenType.Comment:
                    // Do nothing
                    break;
                case JsonTokenType.String:
                    writer.WriteStringValue(RemoveControlCharacters(reader.GetString()));
                    break;
                case JsonTokenType.Number:
                    writer.WriteNumberValue(reader.GetDecimal());
                    break;
                case JsonTokenType.True:
                case JsonTokenType.False:
                    writer.WriteBooleanValue(reader.GetBoolean());
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                default:
                    throw new InvalidOperationException("Unknown JsonTokenType");
            }
        }

        writer.Flush();
        return ms.ToArray();
    }

    // \r, \n, \t 以外の制御文字
    private static readonly Regex ControlCharactersRegex =
        new ("[\u0000-\u0008\u000b\u000c\u000e-\u001f\u007f\u0080-\u009f]", RegexOptions.Multiline);

    /// <summary>
    /// 文字列から改行と横タブを除いた制御文字を削除した文字列にする
    /// </summary>
    internal static string? RemoveControlCharacters(string? value)
    {
        return value == null ? null : ControlCharactersRegex.Replace(value, "");
    }
}
