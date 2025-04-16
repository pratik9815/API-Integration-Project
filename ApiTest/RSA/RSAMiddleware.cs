using System.Security.Cryptography;
using System.Text;

namespace ApiTest.RSA;
public class RSAMiddleware
{

    //public string GetPublicKey()
    //{
        
    //}

    public string GetHMACHash(string data, string key)
    {
        using var secretKeyByte = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = secretKeyByte.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
