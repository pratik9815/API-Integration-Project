namespace ApiTest.HMACService;
public interface IHmacService
{
    string GenerateSecretKey(int length = 64);
    (string Signature, string Timestamp) GenerateSignature(string data, string secretKey);
    bool VerifySignature(string data, string secretKey, string signature, string timestamp, TimeSpan tolerance);
}
