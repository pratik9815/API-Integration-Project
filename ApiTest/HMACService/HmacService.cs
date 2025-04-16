using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ApiTest.HMACService;
public class HmacService : IHmacService
{
    private readonly string _secretKey;
    private readonly string _baseUrl;

    public HmacService(IConfiguration configuration)
    {
        _secretKey = configuration["Hmac:SecretKey"];
        _baseUrl = "https://localhost:7131";
    }

    public string GenerateSecretKey(int length = 64)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] keyBytes = new byte[length];
        rng.GetBytes(keyBytes);
        return Convert.ToBase64String(keyBytes);
    }

    public (string Signature, string Timestamp) GenerateSignature(string data, string secretKey)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var payload = $"{data}:{timestamp}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        return (signature, timestamp);
    }
    public bool VerifySignature(string data, string secretKey, string signature, string timestamp, TimeSpan tolerance)
    {
        // Check timestamp freshness
        if (!long.TryParse(timestamp, out var timestampMs))
            return false;

        var requestTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        if (Math.Abs((DateTimeOffset.UtcNow - requestTime).TotalMilliseconds) > tolerance.TotalMilliseconds)
            return false;

        // Recompute the signature
        var payload = $"{data}:{timestamp}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLowerInvariant();

        // Constant-time comparison
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature));
    }

    //Hmac client
    public void SendRequestWithHmac()
    {
        string endPoint = "/api/secure/secret";
        HttpRequestMessage message = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
        };
        if (message.Method == HttpMethod.Post)
        {
            message.Content = new StringContent("This is example content");
        }

        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string clientId = "admin";
        // Generate a unique nonce
        var nonce = Guid.NewGuid().ToString();

        var payload = $"{clientId}\n{message.Method}\n{endPoint}\n{message.Content}\n{nonce}\n{timeStamp}";
        // Generate HMAC-SHA256 signature
        var signature = ComputeHmac(payload, _secretKey);
        var hmacHeader = $"{clientId}:{signature}:{nonce}:{timeStamp}";

        message.Headers.Add("Authorization", hmacHeader);

        var fullUrl = $"{_baseUrl}/{endPoint}";
        message.RequestUri = new Uri(fullUrl);
        using var httpClient = new HttpClient();
        var response = httpClient.SendAsync(message).Result;
    }
    private static string ComputeHmac(string data, string secretKey)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

}
