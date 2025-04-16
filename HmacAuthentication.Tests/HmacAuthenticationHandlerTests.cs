//using System.Text;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using Microsoft.AspNetCore.Authentication;
//using System.Text.Encodings.Web;
//using ApiTest.HMAC;
//using System.Security.Cryptography;
//using Microsoft.Extensions.Caching.Memory;

//public class HmacAuthenticationHandlerTests
//{
//    private HmacAuthenticationHandler CreateHandler(HttpContext context, string secretKey, string headerValue)
//    {
//        var scheme = new AuthenticationScheme("HMAC", null, typeof(HmacAuthenticationHandler));
//        var options = new HmacAuthenticationOptions
//        {
//            SecretKey = secretKey,
//            PayloadBuilder = async (ctx, timeStamp) => "GET\n/test\n\n" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
//        };

//        var optionsMonitor = Mock.Of<IOptionsMonitor<HmacAuthenticationOptions>>(opt =>
//            opt.Get(It.IsAny<string>()) == options);

//        var handler = new HmacAuthenticationHandler(
//            optionsMonitor,
//            Mock.Of<ILoggerFactory>(),
//            UrlEncoder.Default,
//            Mock.Of<IMemoryCache>
//        );

//        handler.InitializeAsync(scheme, context).Wait();

//        context.Request.Headers["X-Auth-HMAC"] = headerValue;
//        return handler;
//    }

//    [Fact]
//    public async Task HandleAuthenticateAsync_ShouldFail_WhenHeaderMissing()
//    {
//        var context = new DefaultHttpContext();
//        var handler = CreateHandler(context, "testkey", null);

//        var result = await handler.AuthenticateAsync();

//        Assert.False(result.Succeeded);
//    }

//    [Fact]
//    public async Task HandleAuthenticateAsync_ShouldFail_WhenSignatureIsInvalid()
//    {
//        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//        var fakeSignature = "abcdef123456";
//        var header = $"{fakeSignature}:{timestamp}";

//        var context = new DefaultHttpContext();
//        var handler = CreateHandler(context, "wrongkey", header);

//        var result = await handler.AuthenticateAsync();

//        Assert.False(result.Succeeded);
//    }

//    [Fact]
//    public async Task HandleAuthenticateAsync_ShouldSucceed_WhenSignatureIsValid()
//    {
//        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
//        var method = "GET";
//        var path = "/test";
//        var body = "";
//        var payload = $"{method}\n{path}\n{body}\n{timestamp}";

//        var secretKey = "supersecret";
//        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
//        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
//        var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

//        var header = $"{signature}:{timestamp}";

//        var context = new DefaultHttpContext();
//        context.Request.Method = method;
//        context.Request.Path = path;

//        var handler = CreateHandler(context, secretKey, header);

//        var result = await handler.AuthenticateAsync();

//        Assert.True(result.Succeeded);
//    }
//}
