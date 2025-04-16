using ApiTest.HMAC;
using Microsoft.AspNetCore.Authentication;

namespace ApiTest.HelperExtensions;
public static class HmacAuthenticationExtensions
{
    public static AuthenticationBuilder AddHmacAuthentication(
    this AuthenticationBuilder builder,
    Action<HmacAuthenticationOptions> configureOptions)
    {
        return builder.AddScheme<HmacAuthenticationOptions, HmacAuthenticationHandler>(
            "HMAC",
            "HMAC Authentication",
            configureOptions);
    }
}
