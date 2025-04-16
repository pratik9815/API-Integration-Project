using ApiTest.RSA;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ApiTest.Endpoints;
public static class RSAEndpoint
{
    // Static list simulating a database table storing employee records.
    private static List<Employee> Employees = new List<Employee>
        {
            new Employee { Id = 1, Name = "Alice Smith", Position="DBA", Salary = 75000 },
            new Employee { Id = 2, Name = "Bob Johnson", Position="HR", Salary = 60000 },
            new Employee { Id = 3, Name = "Carol White", Position="Developer", Salary = 55000 }
        };
    public static void RSAEndpointFunc(this IEndpointRouteBuilder app)
    {
        // HTTP GET endpoint to generate RSA keys for a client and return them.
        app.MapGet("api/generate-keys", ([FromServices] KeyStorage _keyStorage, [FromHeader] string clientId) =>
        {
            _keyStorage.GenerateKeys(clientId);
            var publicKey = _keyStorage.GetPublicKey(clientId);
            var privateKey = _keyStorage.GetPrivateKey(clientId);
            return Results.Ok(new { PublicKey = publicKey, PrivateKey = privateKey });
        });
        // HTTP POST endpoint to create a new employee from encrypted data.
        app.MapPost("api/create", ([FromHeader] string clientId,
                                            [FromBody] EncryptedRequest request,
                                            [FromServices] RSAEncryptionService _encryptionService, [FromServices] KeyStorage _keyStorage) =>
        {
            var privateKey = _keyStorage.GetPrivateKey(clientId);
            var decryptedData = _encryptionService.Decrypt(request.Data, privateKey);
            var employee = JsonConvert.DeserializeObject<Employee>(decryptedData);
            employee.Id = Employees.Count > 0 ? Employees.Max(e => e.Id) + 1 : 1;
            Employees.Add(employee);
            var encryptedResponse = _encryptionService.Encrypt(JsonConvert.SerializeObject(employee), _keyStorage.GetPublicKey(clientId));
            return Results.Ok(new EncryptedDataResponse { Data = encryptedResponse });
        });

        // HTTP GET endpoint to retrieve an employee's details, encrypt the data before sending it back.
        app.MapGet("api/{id}", (int id, [FromHeader] string clientId,
                                                [FromServices] RSAEncryptionService _encryptionService,
                                                 [FromServices] KeyStorage _keyStorage) =>
        {
            var employees = Employees.FirstOrDefault(e => e.Id == id);
            if (employees == null)
                return Results.NotFound("The result you are looking for not found");

            var data = JsonConvert.SerializeObject(employees);
            var encryptedData = _encryptionService.Encrypt(data, _keyStorage.GetPublicKey(clientId));

            return Results.Ok(new EncryptedDataResponse { Data = encryptedData});
        });
    }
}

public class Employee
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
}

// Class to handle encrypted request data.
public class EncryptedRequest
{
    public string Data { get; set; }
}
// Class to handle encrypted data responses.
public class EncryptedDataResponse
{
    public string Data { get; set; }
}