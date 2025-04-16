using Microsoft.AspNetCore.Authentication;
namespace ApiTest.HMAC;
public class HmacAuthenticationOptions : AuthenticationSchemeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string HeaderName { get; set; } = "Authorization";
    public const string DefaultSchema = "hmac";
    public string Schema => DefaultSchema;
    public TimeSpan Tolerance { get; set; } = TimeSpan.FromMinutes(5);
    public Func<HttpContext, string, string, long , Task<string>>? PayloadBuilder { get; set; }
    public Func<HttpContext, string, Task>? OnAuthenticationFailed { get; set; }
    public Func<HttpContext, Task>? OnAuthenticationSucceeded { get; set; }
}
