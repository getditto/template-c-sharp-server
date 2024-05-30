using DittoSDK;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<DittoAuthConfig>(builder.Configuration.GetSection("DittoAuthConfig"));

builder.Services.AddSingleton<Ditto>(serviceProvider =>
{
    // Configuration from appsettings.json
    var dittoAuthConfig = serviceProvider.GetRequiredService<IOptions<DittoAuthConfig>>().Value;
    return DittoInitializer.InitializeAndStartDitto(dittoAuthConfig);
});

var app = builder.Build();

// Force Ditto to be instantiated 
var dittoService = app.Services.GetRequiredService<Ditto>();

app.MapGet("/", () => "Hello World!");

app.Run();
