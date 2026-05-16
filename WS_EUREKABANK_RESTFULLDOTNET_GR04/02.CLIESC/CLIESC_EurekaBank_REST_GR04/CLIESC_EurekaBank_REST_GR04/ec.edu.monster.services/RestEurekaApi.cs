using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ec.edu.monster.models;

namespace ec.edu.monster.services
{
    /// <summary>
    /// Cliente REST contra el servidor ASP.NET (01.SERVIDOR).
    /// </summary>
    public class RestEurekaApi : IEurekaApi, IDisposable
    {
        private readonly HttpClient _http;

        private record MovimientoDto(string CuentaCodigo, int Numero, DateTime Fecha,
            string TipoCodigo, decimal Importe, string? ReferenciaCuenta);

        public RestEurekaApi(string baseUrl = null)
        {
            var url = string.IsNullOrWhiteSpace(baseUrl)
                ? "https://localhost:7043/"
                : baseUrl.Trim();
            _http = new HttpClient { BaseAddress = new Uri(url) };
        }

        public async Task<bool> ValidarIngreso(string usuario, string password)
        {
            var resp = await _http.PostAsJsonAsync("api/CoreBancario/validarIngreso",
                new { usuario, password });
            var texto = (await resp.Content.ReadAsStringAsync()).Trim();
            return texto.Equals("Exitoso", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<List<Movimiento>> TraerMovimientos(string cuenta)
        {
            var dto = await _http.GetFromJsonAsync<List<MovimientoDto>>(
                $"api/CoreBancario/cuentas/{cuenta}/movimientos");

            return dto?
                .Select(m =>
                {
                    var (tipo, accion) = MapTipoAccion(m.TipoCodigo);
                    return new Movimiento
                    {
                        Cuenta = m.CuentaCodigo ?? string.Empty,
                        NroMov = m.Numero,
                        Fecha = m.Fecha,
                        Tipo = tipo,
                        Accion = accion,
                        Importe = m.Importe
                    };
                })
                .ToList() ?? new List<Movimiento>();
        }

        public async Task<OperacionCuentaResponse> RegDeposito(string cuenta, decimal importe)
        {
            return await PostOperacion("api/CoreBancario/deposito",
                new { cuenta, importe });
        }

        public async Task<OperacionCuentaResponse> RegRetiro(string cuenta, decimal importe)
        {
            return await PostOperacion("api/CoreBancario/retiro",
                new { cuenta, importe });
        }

        public async Task<OperacionCuentaResponse> RegTransferencia(string origen, string destino, decimal importe)
        {
            return await PostOperacion("api/CoreBancario/transferencia",
                new { cuentaOrigen = origen, cuentaDestino = destino, importe });
        }

        public Task<string> ProbarConexion() => Task.FromResult("OK");

        private async Task<OperacionCuentaResponse> PostOperacion(string path, object payload)
        {
            var resp = await _http.PostAsJsonAsync(path, payload);
            return await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>()
                   ?? new OperacionCuentaResponse { Estado = -1, Saldo = -1 };
        }

        private static (string Tipo, string Accion) MapTipoAccion(string codigo) =>
            codigo switch
            {
                "003" => ("DEPOSITO", "INGRESO"),
                "004" => ("RETIRO", "SALIDA"),
                "008" => ("TRANSFERENCIA", "INGRESO"),
                "009" => ("TRANSFERENCIA", "SALIDA"),
                "001" => ("APERTURA", "INGRESO"),
                _ => ("", "")
            };

        public void Dispose() => _http?.Dispose();
    }
}
