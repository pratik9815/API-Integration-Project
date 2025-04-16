using System.Security.Cryptography;

namespace ApiTest.RSA;
public class KeyStorage
{
    // A private static dictionary to store the RSA keys for each client.
    // The dictionary keys are client IDs (string), and the values are tuples containing both public and private keys.
    private static readonly Dictionary<string, (string PublicKey, string PrivateKey)> _keyStore = new();
    public void GenerateKeys(string clientId)
    {
        using(var rsa = System.Security.Cryptography.RSA.Create(2048))
        {
            // Export the public key information and convert it to a base64 string for easier storage and transmission.
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            // Export the private key in PKCS#8 format and convert it to a base64 string.
            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            // Store the generated keys in the _keyStore dictionary with the provided clientId as the key.
            _keyStore[clientId] = (publicKey, privateKey);
        }
    }
    //Method to retrieve the public key for a given clientId
    public string GetPublicKey(string clientId)
    {
        // Attempt to retrieve the keys for the specified clientId. If found, return the public key; otherwise, return null.
        return _keyStore.TryGetValue(clientId, out var keys) ? keys.PublicKey : null;
    }
    // Method to retrieve the private key for a given clientId.
    public string GetPrivateKey(string clientId)
    {
        return _keyStore.TryGetValue(clientId, out var keys) ? keys.PrivateKey : null;  
    }

    //To get keys from a file 
    private readonly string _storagePath = "";
    public (string publicKey, string privateKey)? GetKeys(string clientId)
    {
        string pubPath = Path.Combine(_storagePath, $"{clientId}_public.key");
        string privPath = Path.Combine(_storagePath, $"{clientId}_private.key");
        if (File.Exists(pubPath) && File.Exists(privPath))
        {
            var publicKey = File.ReadAllText(pubPath);
            var privateKey = File.ReadAllText(privPath);    
            return (publicKey, privateKey); 
        }
        return null;
    }
    public void SaveKeys(string clientId, string publicKey, string privateKey)
    {
        File.WriteAllText(Path.Combine(_storagePath, $"{clientId}_public.key"), publicKey);
        File.WriteAllText(Path.Combine(_storagePath, $"{clientId}_private.key"), privateKey);
    }

    //DbBased implementation


}
