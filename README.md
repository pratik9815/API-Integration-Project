# API Integration Project

## Project Overview
This project is designed to consume and integrate with third-party APIs. It demonstrates how to send requests, handle responses, and manage errors effectively in an ASP.NET Core application.

---

## Features
- **API Integration:** Connects with external services to fetch and post data.
- **Dependency Injection:** Uses `HttpClientFactory` for efficient HTTP calls.
- **Error Handling:** Implements try-catch blocks and logging for fault tolerance.
- **Configuration Management:** Uses `appsettings.json` for API keys and URLs.

---

## Technologies Used
- **ASP.NET Core 6/7**
- **C#**
- **HttpClientFactory**
- **Newtonsoft.Json / System.Text.Json**
- **Swagger (NSwag)**

---

## Prerequisites
- .NET 6 or higher
- Visual Studio 2022 or any compatible IDE
- Postman (for API testing)

---

## Installation
1. **Clone the Repository:**
```bash
git clone https://github.com/yourusername/api-integration-project.git
```

2. **Navigate to the Project Directory:**
```bash
cd api-integration-project
```

3. **Install Dependencies:**
```bash
dotnet restore
```

---

## Configuration
1. **Add API Settings** in `appsettings.json`:
```json
{
  "ThirdPartyApi": {
    "BaseUrl": "https://api.thirdparty.com",
    "ApiKey": "your_api_key_here"
  }
}
```

2. **Inject Configuration in Program.cs:**
```csharp
builder.Services.AddHttpClient("ThirdPartyApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ThirdPartyApi:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {builder.Configuration["ThirdPartyApi:ApiKey"]}");
});
```

---

## Usage
### Example API Service
```csharp
public class ThirdPartyService
{
    private readonly HttpClient _httpClient;

    public ThirdPartyService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ThirdPartyApi");
    }

    public async Task<string> GetDataAsync()
    {
        var response = await _httpClient.GetAsync("/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### Example Controller
```csharp
[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    private readonly ThirdPartyService _service;

    public DataController(ThirdPartyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetDataAsync();
        return Ok(result);
    }
}
```

---

## Running the Project
```bash
dotnet run
```

Navigate to `https://localhost:5001/swagger` to test the APIs.

---

## Error Handling
- Uses try-catch blocks to log exceptions.
- Ensures HTTP response validation with `EnsureSuccessStatusCode()`.

---

## Contributing
Feel free to open issues or submit pull requests to improve the project.

---

## License
This project is licensed under the **MIT License**.

