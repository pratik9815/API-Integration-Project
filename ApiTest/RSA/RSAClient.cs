using System.Security.Cryptography;
using System.Text;
using ApiTest.Endpoints;
using Newtonsoft.Json;

namespace ApiTest.RSA;
public class RSAClient
{
    // HttpClient instance for sending requests to a server.
    private static readonly HttpClient client = new HttpClient();
    public static async Task GetKeys()
    {
        var clientId = "Client1";
        client.DefaultRequestHeaders.Add("clientId", clientId);
        // Request to generate RSA keys from the server and retrieve them.
        var keysResponse = await client.GetFromJsonAsync<KeysResponse>("https://localhost:7102/api/Employees/generate-keys");
        var publicKey = keysResponse.PublicKey;
        var privateKey = keysResponse.PrivateKey;

        // Create a new employee object.
        var newEmployee = new Employee
        {
            Name = "John Doe",
            Position = "Developer",
            Salary = 60000
        };

        // Encrypt the new employee data using the public key and send to the server.
        var encryptedData = EncryptData(newEmployee, publicKey);
        var createRequest = new EncryptedRequest { Data = encryptedData };


        // Send the encrypted employee data to the server to create a new employee.
        var createResponse = await client.PostAsJsonAsync("https://localhost:7102/api/Employees/create", createRequest);
        var createdEmployeeData = await createResponse.Content.ReadFromJsonAsync<EncryptedDataResponse>();
        var createdEmployeeJson = DecryptData(createdEmployeeData.Data, privateKey);
        var createdEmployee = JsonConvert.DeserializeObject<Employee>(createdEmployeeJson);
        Console.WriteLine($"Created Employee: {JsonConvert.SerializeObject(createdEmployee)}");


    }

    // Encrypts data using an RSA public key.
    private static string EncryptData(object data, string publicKey)
    {
        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
            var dataBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            var encryptedData = rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedData);
        }
    }
    // Decrypts data using an RSA private key.
    private static string DecryptData(string encryptedData, string privateKey)
    {
        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKey), out _);
            var dataBytes = Convert.FromBase64String(encryptedData);
            var decryptedData = rsa.Decrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}
// Response model for receiving keys.
public class KeysResponse
{
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
}
