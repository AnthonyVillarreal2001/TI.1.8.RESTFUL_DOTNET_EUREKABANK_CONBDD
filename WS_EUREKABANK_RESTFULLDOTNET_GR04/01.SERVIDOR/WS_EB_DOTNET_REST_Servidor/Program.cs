using Microsoft.OpenApi.Models;
using WS_EB_DOTNET_REST_Servidor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar URLs para escuchar en todas las interfaces de red
builder.WebHost.UseUrls("https://0.0.0.0:7043", "http://0.0.0.0:5043");

// Configurar CORS para permitir acceso desde cualquier origen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EurekaBank REST API", Version = "v1" });
});
builder.Services.AddScoped<EurekaService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
