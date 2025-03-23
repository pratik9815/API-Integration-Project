using SOAPApiTest.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<RequestHandler>();
builder.Services.AddScoped<Serializers>();
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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
