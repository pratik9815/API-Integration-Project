using ApiTest.HMACService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiTest.Endpoints;
public static class SecureEndpoint
{
    public static void MapSecureEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/secure/secret", () =>
        {
            return Results.Ok("Hello World");
        }).RequireAuthorization();
    }
}