using ApiTest.Endpoints;
using ApiTest.HelperExtensions;
using ApiTest.HMAC;
using ApiTest.HMACService;
using ApiTest.RSA;
using Microsoft.OpenApi.Models;
using SOAPApiTest.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<RequestHandler>();
builder.Services.AddScoped<Serializers>();

builder.Services.AddSingleton<IHmacService, HmacService>();

// Register the HttpClientFactory
builder.Services.AddHttpClient();
//for named clients 
builder.Services.AddHttpClient("REST", httpClient => {
    //httpClient.BaseAddress()
    httpClient.DefaultRequestHeaders.Add("Accept", "application/json"); 
});

builder.Services.AddHttpClient("SOAP", httpClient =>
{
    httpClient.DefaultRequestHeaders.Add("Accept", "text/xml");
    // Set default timeout
    httpClient.Timeout = TimeSpan.FromSeconds(60);
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "HMAC API", Version = "v1" });

    // Add HMAC authentication to Swagger
    options.AddSecurityDefinition("HMAC", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "HMAC",
        BearerFormat = "HMAC",
        Description = "HMAC authentication using the format: hmac `<your signature>`"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "HMAC"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddAuthentication("HMAC")
    .AddScheme<HmacAuthenticationOptions, HmacAuthenticationHandler>("HMAC", options =>
    {
        options.SecretKey = builder.Configuration["Hmac:SecretKey"] ?? "";
        options.HeaderName = "Authorization";

        // Optional: use default payload builder
        options.PayloadBuilder = async (context,clientId, nonce, timestamp) =>
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
            //var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            //return $"{request.Method}\n{request.Path}\n{body}\n{timestamp}";
            return $"{clientId}\n{request.Method}\n{request.Path}\n{body}\n{nonce}\n{timestamp}";
        };
    });

builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()));

//Adding In-Memory Caching
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<RSAEncryptionService>();
builder.Services.AddSingleton<KeyStorage>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapSecureEndpoints();
//app.MapHmac();
app.RSAEndpointFunc();

app.Run();
