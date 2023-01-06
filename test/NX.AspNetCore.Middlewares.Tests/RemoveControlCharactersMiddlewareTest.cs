using System.Net;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

// ReSharper disable InconsistentNaming

namespace NX.AspNetCore.Middlewares;

public class RemoveControlCharactersMiddlewareTest
{
    public class JSONから制御文字を削除するテスト
    {
        [Fact]
        public void 制御文字が含まれない()
        {
            const string json =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            const string expected =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            var source = Encoding.UTF8.GetBytes(json);
            var result = RemoveControlCharactersMiddleware.RemoveControlCharactersInJsonString(source);
            Encoding.UTF8.GetString(result).Should().Be(expected);
        }

        [Fact]
        public void 制御文字が含まれる()
        {
            const string json =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text\r\n\t\u0000\u0009"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            const string expected =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text\r\n\t\t"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            var source = Encoding.UTF8.GetBytes(json);
            var result = RemoveControlCharactersMiddleware.RemoveControlCharactersInJsonString(source);
            Encoding.UTF8.GetString(result).Should().Be(expected);
        }
    }

    public class 削除対象の制御文字テスト
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData("\u0000", "")]
        [InlineData("\u0001", "")]
        [InlineData("\u0002", "")]
        [InlineData("\u0003", "")]
        [InlineData("\u0004", "")]
        [InlineData("\u0005", "")]
        [InlineData("\u0006", "")]
        [InlineData("\u0007", "")]
        [InlineData("\u0008", "")]
        [InlineData("\u0009", "\t")]
        [InlineData("\u000a", "\n")]
        [InlineData("\u000b", "")]
        [InlineData("\u000c", "")]
        [InlineData("\u000d", "\r")]
        [InlineData("\u000e", "")]
        [InlineData("\u000f", "")]
        [InlineData("\u0010", "")]
        [InlineData("\u0011", "")]
        [InlineData("\u0012", "")]
        [InlineData("\u0013", "")]
        [InlineData("\u0014", "")]
        [InlineData("\u0015", "")]
        [InlineData("\u0016", "")]
        [InlineData("\u0017", "")]
        [InlineData("\u0018", "")]
        [InlineData("\u0019", "")]
        [InlineData("\u001a", "")]
        [InlineData("\u001b", "")]
        [InlineData("\u001c", "")]
        [InlineData("\u001d", "")]
        [InlineData("\u001e", "")]
        [InlineData("\u001f", "")]
        [InlineData("\u007f", "")]
        [InlineData("\u0080", "")]
        [InlineData("\u0081", "")]
        [InlineData("\u0082", "")]
        [InlineData("\u0083", "")]
        [InlineData("\u0084", "")]
        [InlineData("\u0085", "")]
        [InlineData("\u0086", "")]
        [InlineData("\u0087", "")]
        [InlineData("\u0088", "")]
        [InlineData("\u0089", "")]
        [InlineData("\u008a", "")]
        [InlineData("\u008b", "")]
        [InlineData("\u008c", "")]
        [InlineData("\u008d", "")]
        [InlineData("\u008e", "")]
        [InlineData("\u008f", "")]
        [InlineData("\u0090", "")]
        [InlineData("\u0091", "")]
        [InlineData("\u0092", "")]
        [InlineData("\u0093", "")]
        [InlineData("\u0094", "")]
        [InlineData("\u0095", "")]
        [InlineData("\u0096", "")]
        [InlineData("\u0097", "")]
        [InlineData("\u0098", "")]
        [InlineData("\u0099", "")]
        [InlineData("\u009a", "")]
        [InlineData("\u009b", "")]
        [InlineData("\u009c", "")]
        [InlineData("\u009d", "")]
        [InlineData("\u009e", "")]
        [InlineData("\u009f", "")]
        [InlineData(".", ".")]
        [InlineData("a", "a")]
        [InlineData("A", "A")]
        [InlineData("0", "0")]
        [InlineData("あ", "あ")]
        public void 制御文字削除(string? target, string? expected)
        {
            var result = RemoveControlCharactersMiddleware.RemoveControlCharacters(target);
            result.Should().Be(expected);
        }
    }

    public class ミドルウェアのテスト
    {
        [Fact]
        public async Task GET()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/");

            using var host = await CreateHost().StartAsync();
            var response = await host.GetTestClient().SendAsync(req);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Be("");
        }

        [Fact]
        public async Task POST_JSON以外()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new StringContent("\r\n\t\v", Encoding.UTF8, MediaTypeNames.Text.Plain)
            };

            using var host = await CreateHost().StartAsync();
            var response = await host.GetTestClient().SendAsync(req);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Be("\r\n\t\v");
        }

        [Fact]
        public async Task POST_JSON_制御文字が含まれない()
        {
            const string json =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            const string expected =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text"",""number1"":0,""number2"":0.00000000001,""array"":[]}";

            var req = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            using var host = await CreateHost().StartAsync();
            var response = await host.GetTestClient().SendAsync(req);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Be(expected);
        }

        [Fact]
        public async Task POST_JSON_制御文字が含まれる()
        {
            const string json =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text\r\n\t\u0000\u0009"",""number1"":0,""number2"":0.00000000001,""array"":[]}";
            const string expected =
                @"{""null"":null,""false"":false,""true"":true,""text"":""text\r\n\t\t"",""number1"":0,""number2"":0.00000000001,""array"":[]}";

            var req = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            using var host = await CreateHost().StartAsync();
            var response = await host.GetTestClient().SendAsync(req);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Be(expected);
        }

        private static IHostBuilder CreateHost()
        {
            return new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder
                        .UseTestServer()
                        .Configure(app =>
                        {
                            app.UseRemoveControlCharacters();
                            app.Run(async context =>
                            {
                                context.Response.StatusCode = 200;
                                if (context.Request.ContentType != null)
                                {
                                    await context.Request.Body.CopyToAsync(context.Response.Body);
                                }
                            });
                        });
                });
        }
    }
}
