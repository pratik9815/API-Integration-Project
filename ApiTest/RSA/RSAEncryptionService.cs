using System.Security.Cryptography;
using System.Text;

namespace ApiTest.RSA;
public class RSAEncryptionService
{
    //Encrypt the data using public key
    public string Encrypt(string data, string publicKey)
    {
        using (var rsa =  System.Security.Cryptography.RSA.Create())
        {
            // Imports the public key, provided in base64 format, to set up the RSA object for encryption.
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            // Converts the string data to a byte array in UTF-8 encoding.
            var dataBytes = Encoding.UTF8.GetBytes(data);
            // Encrypts the data bytes using the public key and OAEP padding with SHA-256.
            var encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            // Converts the encrypted byte array back to a base64 string for easy storage or transmission.
            return Convert.ToBase64String(encryptedData);
        }
    }
    public string Decrypt(string encryptedData, string privateKey)
    {
        using(var rsa = System.Security.Cryptography.RSA.Create())
        {
            // Imports the private key, provided in base64 format, to set up the RSA object for decryption.
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            // Converts the base64 encoded encrypted data back into a byte array.
            var dataBytes = Convert.FromBase64String(encryptedData);
            // Decrypts the data bytes using the private key and OAEP padding with SHA-256.
            var decryptedData = rsa.Decrypt(dataBytes,RSAEncryptionPadding.OaepSHA256);
            // Converts the decrypted byte array back to a UTF-8 encoded string.
            return Encoding.UTF8.GetString(decryptedData);
        }
    }

}
