using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ApiTest.HMAC;
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using Microsoft.Extensions.Caching.Memory;

public class HmacAuthenticationHandler : AuthenticationHandler<HmacAuthenticationOptions>
{
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan NonceExpiry = TimeSpan.FromMinutes(5);
    //List<ClientSecret> secrets = new List<ClientSecret>
    //{
    //     new ClientSecret { Id = 1, ClientId = "WebAppClient", SecretKey = "a1b2c3d4e5f6g7h8i9j0" },
    //     new ClientSecret { Id = 2, ClientId = "MobileAppClient", SecretKey = "z9y8x7w6v5u4t3s2r1q0" },
    //     new ClientSecret { Id = 3, ClientId = "DesktopClient", SecretKey = "m1n2b3v4c5x6z7l8k9j0" }
    //};
    //List<Employee> employees = new List<Employee>
    //{
    //    new Employee { Id = 1, Name = "John Doe", Position = "Software Engineer", Salary = 80000m },
    //    new Employee { Id = 2, Name = "Jane Smith", Position = "Project Manager", Salary = 95000m }
    //};
    public HmacAuthenticationHandler(IOptionsMonitor<HmacAuthenticationOptions> options,
                                     ILoggerFactory logger,
                                     UrlEncoder encoder,
                                     IMemoryCache memoryCache)
        : base(options, logger, encoder)
    {
        _memoryCache = memoryCache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            HttpRequestMessageFeature hreqf = new HttpRequestMessageFeature(Request.HttpContext);
            HttpRequestMessage req = hreqf.HttpRequestMessage;

            if (req.Headers.Authorization == null || !Options.Schema.Equals(req.Headers.Authorization.Scheme, StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("HMAC Schema missing");


            // 1. Get HMAC from header
            if (!Request.Headers.TryGetValue(Options.HeaderName, out var headerValue))
                return AuthenticateResult.Fail("HMAC header missing");

            var receivedHmac = req.Headers.Authorization.Parameter;
            //var receivedHmac = headerValue.FirstOrDefault();
            if (string.IsNullOrEmpty(receivedHmac))
                return AuthenticateResult.Fail("Empty HMAC value");

            //I need to add one more thing here which is clientId for knowing which platform or the user the request is from
            // 2. Parse components (signature:timestamp)
            // 2. Parse components (clientId:signature:nonce:timestamp)
            var parts = receivedHmac.Split(':');
            //if (parts.Length != 2)
            //    return AuthenticateResult.Fail("Invalid HMAC format");
            if (parts.Length != 4)
                return AuthenticateResult.Fail("Invalid HMAC format");

            var clientId = parts[0];    // Extract client ID
            var signature = parts[1];   // Extract signature
            var nonce = parts[2];       // Extract nonce
            if (!long.TryParse(parts[3], out var timestamp))
                return AuthenticateResult.Fail("Invalid timestamp");

            // 3. Check timestamp freshness
            var requestTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
            var now = DateTimeOffset.UtcNow;

            if (Math.Abs((now - requestTime).TotalMilliseconds) > Options.Tolerance.TotalMilliseconds)
                return AuthenticateResult.Fail("Request timestamp is too old or in the future");


            // NEW: 3b. Validate the nonce hasn't been used before
            var nonceKey = $"{clientId}:{nonce}";

            if (_memoryCache.TryGetValue(nonceKey, out _))
                return AuthenticateResult.Fail("Nonce has already been used");

            // Store nonce in cache to prevent reuse
            _memoryCache.Set(nonceKey, true, NonceExpiry);

            // NEW: 3c. Get the client's secret key based on client ID
            //commented for now because no need to do this in my case
            //var secretKey = await GetClientSecretKey(clientId);
            //if (string.IsNullOrEmpty(secretKey))
            //    return AuthenticateResult.Fail("Invalid client ID");


            // 4. Build payload
            var payload = Options.PayloadBuilder != null
                ? await Options.PayloadBuilder(Context,clientId, nonce, timestamp)
                : await BuildDefaultPayload(Context,clientId, nonce, timestamp);

            // 5. Compute HMAC
            var expectedSignature = ComputeHmac(payload, Options.SecretKey);

            // 6. Compare signatures
            if (!CompareSignatures(signature, expectedSignature))
                return AuthenticateResult.Fail("Invalid HMAC signature");

            // 7. Authentication succeeded
            if (Options.OnAuthenticationSucceeded != null)
                await Options.OnAuthenticationSucceeded.Invoke(Context);

            // Add client ID as a claim
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("client_id", clientId)
            };
            var ticket = new AuthenticationTicket(
                new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(claims, "HMAC")), Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            if (Options.OnAuthenticationFailed != null)
                await Options.OnAuthenticationFailed.Invoke(Context, ex.Message);
            return AuthenticateResult.Fail(ex.Message);
        }
    }

    private static async Task<string> BuildDefaultPayload(HttpContext context,string clientId,string nonce, long timestamp)
    {
        var request = context.Request;
        string body = string.Empty;

        if (request.Body.CanRead)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        //return $"{request.Method}\n{request.Path}\n{body}\n{timestamp}";
        // Include clientId and nonce in the payload construction
        return $"{clientId}\n{request.Method}\n{request.Path}\n{body}\n{nonce}\n{timestamp}";
    }

    private static string ComputeHmac(string data, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static bool CompareSignatures(string a, string b)
    {
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
    }
    // New method to get client secret based on client ID
    //do it using db or other methods
    //private async Task<string> GetClientSecretKey(string clientId)
    //{
    //    // Implement your client secret retrieval logic
    //    // This could query a database, secrets vault, etc.

    //    // Example implementation (replace with actual logic):
    //    var clientSecretService = Context.RequestServices.GetRequiredService<IClientSecretService>();
    //    return await clientSecretService.GetSecretKeyAsync(clientId);
    //}

}

public class ClientSecret
{
    public int Id { get; set; }
    public string ClientId { get; set; }
    public string SecretKey { get; set; }   
}
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
}
