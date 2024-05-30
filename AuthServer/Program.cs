using DittoSDK;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<DittoAuthConfig>(builder.Configuration.GetSection("DittoAuthConfig"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<Ditto>(serviceProvider =>
{
    // Configuration from appsettings.json
    var dittoAuthConfig = serviceProvider.GetRequiredService<IOptions<DittoAuthConfig>>().Value;
    return DittoInitializer.InitializeAndStartDitto(dittoAuthConfig);
});

var app = builder.Build();

// Force Ditto to be instantiated 
var dittoService = app.Services.GetRequiredService<Ditto>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
