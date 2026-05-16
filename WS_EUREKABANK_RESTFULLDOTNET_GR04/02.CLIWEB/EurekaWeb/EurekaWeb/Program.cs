using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// HttpClient tipado
builder.Services.AddHttpClient<IRestEurekaClient, RestEurekaClient>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Api:BaseUrl"] ?? "https://localhost:7043";
    http.BaseAddress = new Uri(baseUrl);
    // JSON por defecto camelCase y fechas ISO
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Solo en desarrollo, si usas certificado dev:
    ServerCertificateCustomValidationCallback = (m, c, ch, e) => true
});

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
