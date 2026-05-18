using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using EurekaWeb.Models.Rest;

public class RestEurekaClient : IRestEurekaClient
{
    private readonly HttpClient _http;
    public RestEurekaClient(HttpClient http) => _http = http;

    static string Pad8(string? c)
    {
        var s = (c ?? "").Trim();
        return (s.Length > 0 && s.Length < 8) ? s.PadLeft(8, '0') : s;
    }

    public async Task<string> ValidarIngreso(string usuario, string password)
    {
        // El servidor devuelve text/plain
        var req = new LoginRequest(usuario ?? "", password ?? "");
        using var resp = await _http.PostAsJsonAsync("api/CoreBancario/validarIngreso", req);
        resp.EnsureSuccessStatusCode();
        var txt = await resp.Content.ReadAsStringAsync();
        return txt.Trim(); // "Exitoso" / "Denegado"
    }

    public async Task<Movimiento[]> TraerMovimientos(string cuenta)
    {
        var c = Pad8(cuenta);
        var arr = await _http.GetFromJsonAsync<Movimiento[]>($"api/CoreBancario/cuentas/{c}/movimientos");
        return arr ?? Array.Empty<Movimiento>();
    }

    public async Task<OperacionCuentaResponse> RegDeposito(string cuenta, decimal importe)
    {
        var req = new OperacionRequest(Pad8(cuenta), importe, null);
        var resp = await _http.PostAsJsonAsync("api/CoreBancario/deposito", req);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>())!;
    }

    public async Task<OperacionCuentaResponse> RegRetiro(string cuenta, decimal importe)
    {
        var req = new OperacionRequest(Pad8(cuenta), importe, null);
        var resp = await _http.PostAsJsonAsync("api/CoreBancario/retiro", req);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>())!;
    }

    public async Task<OperacionCuentaResponse> RegTransferencia(string cuentaOrigen, string cuentaDestino, decimal importe)
    {
        var req = new TransferenciaRequest(Pad8(cuentaOrigen), Pad8(cuentaDestino), importe, null);
        var resp = await _http.PostAsJsonAsync("api/CoreBancario/transferencia", req);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>())!;
    }
}
