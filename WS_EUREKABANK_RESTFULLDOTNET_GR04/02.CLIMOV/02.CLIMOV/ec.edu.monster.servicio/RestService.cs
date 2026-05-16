using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using _02.CLIMOV.Modelo;

namespace _02.CLIMOV.Servicio
{
    public class RestService : IRestService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;

        public RestService()
        {
            // 🔧 CONFIGURACIÓN DEL ENDPOINT REST
            // ===================================
            
            // ⚠️ IMPORTANTE: Según tu launchSettings.json el servidor corre en:
            // - HTTPS: puerto 7043 ✅ (FUNCIONA - comprobado en navegador)
            // - HTTP:  puerto 5043
            
            // 📱 Para DISPOSITIVO FÍSICO usa HTTPS con la IP de tu PC:
            _baseUrl = "https://10.40.27.236:7043/api/CoreBancario";
            
            // 📱 Para EMULADOR ANDROID usa 10.0.2.2:
            // _baseUrl = "https://10.0.2.2:7043/api/CoreBancario";
            
            // 💻 Para WINDOWS/Mac Catalyst usa localhost:
            // _baseUrl = "https://localhost:7043/api/CoreBancario";

            // Configurar HttpClient con SSL
            var handler = new HttpClientHandler();
            
            // Bypass de validación SSL (solo para desarrollo)
            handler.ServerCertificateCustomValidationCallback = 
                (message, cert, chain, sslPolicyErrors) => true;
            
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(60); // Timeout de 60 segundos
            
            System.Diagnostics.Debug.WriteLine($"🔧 RestService configurado para: {_baseUrl}");
        }

        public async Task<bool> ValidarIngresoAsync(string usuario, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== VALIDAR INGRESO REST ===");
                System.Diagnostics.Debug.WriteLine($"Usuario enviado: '{usuario}'");
                System.Diagnostics.Debug.WriteLine($"Password enviado: '{password}'");
                
                var requestData = new
                {
                    usuario = usuario,
                    password = password
                };
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/validarIngreso", jsonContent);
                
                System.Diagnostics.Debug.WriteLine($"📥 Status Code: {response.StatusCode}");
                
                var resultado = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Resultado recibido: '{resultado}'");
                
                response.EnsureSuccessStatusCode();
                
                return resultado.Contains("Exitoso");
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Timeout en llamada REST: {ex.Message}");
                throw new Exception($"Timeout: El servidor no respondió a tiempo.", ex);
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {ex.Message}");
                throw new Exception($"No se puede conectar al servidor REST: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al validar ingreso: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Movimiento>> TraerMovimientosAsync(string cuenta)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== TRAER MOVIMIENTOS REST ===");
                System.Diagnostics.Debug.WriteLine($"Cuenta: '{cuenta}'");
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/cuentas/{cuenta}/movimientos");
                
                System.Diagnostics.Debug.WriteLine($"📥 Status Code: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var movimientos = await response.Content.ReadFromJsonAsync<List<MovimientoDto>>();
                
                if (movimientos == null || movimientos.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No se encontraron movimientos");
                    return new List<Movimiento>();
                }
                
                System.Diagnostics.Debug.WriteLine($"Movimientos encontrados: {movimientos.Count}");
                
                // Mapear de MovimientoDto a Movimiento
                return movimientos.Select(m => new Movimiento
                {
                    Cuenta = m.CuentaCodigo,
                    NroMov = m.Numero,
                    Fecha = m.Fecha,
                    Tipo = m.TipoCodigo,
                    Accion = ObtenerAccionPorTipo(m.TipoCodigo),
                    Importe = (double)m.Importe
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al traer movimientos: {ex.Message}");
                throw new Exception("Error al obtener movimientos", ex);
            }
        }

        public async Task<OperacionResponse> RegistrarDepositoAsync(string cuenta, double importe)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== REGISTRAR DEPOSITO REST ===");
                System.Diagnostics.Debug.WriteLine($"Cuenta: '{cuenta}', Importe: {importe}");
                
                var requestData = new
                {
                    cuenta = cuenta,
                    importe = importe,
                    empleado = "0001"
                };
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/deposito", jsonContent);
                
                System.Diagnostics.Debug.WriteLine($"📥 Status Code: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var resultado = await response.Content.ReadFromJsonAsync<OperacionCuentaResponseDto>();
                
                if (resultado == null)
                {
                    return new OperacionResponse
                    {
                        Exito = false,
                        Mensaje = "Error: No se recibió respuesta del servidor",
                        SaldoActual = 0,
                        Cuenta = cuenta
                    };
                }
                
                System.Diagnostics.Debug.WriteLine($"Estado: {resultado.Estado}, Saldo: {resultado.Saldo}");
                
                return new OperacionResponse
                {
                    Exito = resultado.Estado == 1,
                    Mensaje = resultado.Estado == 1 ? "Depósito exitoso" : "Error en el depósito",
                    SaldoActual = (double)resultado.Saldo,
                    Cuenta = cuenta
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al registrar depósito: {ex.Message}");
                return new OperacionResponse
                {
                    Exito = false,
                    Mensaje = $"Error: {ex.Message}",
                    SaldoActual = 0,
                    Cuenta = cuenta
                };
            }
        }

        public async Task<OperacionResponse> RegistrarRetiroAsync(string cuenta, double importe)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== REGISTRAR RETIRO REST ===");
                System.Diagnostics.Debug.WriteLine($"Cuenta: '{cuenta}', Importe: {importe}");
                
                var requestData = new
                {
                    cuenta = cuenta,
                    importe = importe,
                    empleado = "0004"
                };
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/retiro", jsonContent);
                
                System.Diagnostics.Debug.WriteLine($"📥 Status Code: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var resultado = await response.Content.ReadFromJsonAsync<OperacionCuentaResponseDto>();
                
                if (resultado == null)
                {
                    return new OperacionResponse
                    {
                        Exito = false,
                        Mensaje = "Error: No se recibió respuesta del servidor",
                        SaldoActual = 0,
                        Cuenta = cuenta
                    };
                }
                
                System.Diagnostics.Debug.WriteLine($"Estado: {resultado.Estado}, Saldo: {resultado.Saldo}");
                
                return new OperacionResponse
                {
                    Exito = resultado.Estado == 1,
                    Mensaje = resultado.Estado == 1 ? "Retiro exitoso" : "Error en el retiro",
                    SaldoActual = (double)resultado.Saldo,
                    Cuenta = cuenta
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al registrar retiro: {ex.Message}");
                return new OperacionResponse
                {
                    Exito = false,
                    Mensaje = $"Error: {ex.Message}",
                    SaldoActual = 0,
                    Cuenta = cuenta
                };
            }
        }

        public async Task<OperacionResponse> RegistrarTransferenciaAsync(string origen, string destino, double importe)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== REGISTRAR TRANSFERENCIA REST ===");
                System.Diagnostics.Debug.WriteLine($"Origen: '{origen}', Destino: '{destino}', Importe: {importe}");
                
                var requestData = new
                {
                    cuentaOrigen = origen,
                    cuentaDestino = destino,
                    importe = importe,
                    empleado = "0004"
                };
                
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json"
                );
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/transferencia", jsonContent);
                
                System.Diagnostics.Debug.WriteLine($"📥 Status Code: {response.StatusCode}");
                
                response.EnsureSuccessStatusCode();
                
                var resultado = await response.Content.ReadFromJsonAsync<OperacionCuentaResponseDto>();
                
                if (resultado == null)
                {
                    return new OperacionResponse
                    {
                        Exito = false,
                        Mensaje = "Error: No se recibió respuesta del servidor",
                        SaldoActual = 0,
                        Cuenta = origen
                    };
                }
                
                System.Diagnostics.Debug.WriteLine($"Estado: {resultado.Estado}, Saldo: {resultado.Saldo}");
                
                return new OperacionResponse
                {
                    Exito = resultado.Estado == 1,
                    Mensaje = resultado.Estado == 1 ? "Transferencia exitosa" : "Error en la transferencia",
                    SaldoActual = (double)resultado.Saldo,
                    Cuenta = origen
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al registrar transferencia: {ex.Message}");
                return new OperacionResponse
                {
                    Exito = false,
                    Mensaje = $"Error: {ex.Message}",
                    SaldoActual = 0,
                    Cuenta = origen
                };
            }
        }

        private string ObtenerAccionPorTipo(string tipoCodigo)
        {
            // Mapeo de códigos de tipo a acciones (según tu base de datos)
            return tipoCodigo switch
            {
                "001" => "APERTURA",
                "003" => "DEPOSITO",
                "004" => "RETIRO",
                "008" => "TRANSFERENCIA",
                "009" => "TRANSFERENCIA",
                _ => tipoCodigo
            };
        }

        // ===== DTOs para mapeo de respuestas REST =====
        
        private class MovimientoDto
        {
            public string CuentaCodigo { get; set; } = string.Empty;
            public int Numero { get; set; }
            public DateTime Fecha { get; set; }
            public string EmpleadoCodigo { get; set; } = string.Empty;
            public string TipoCodigo { get; set; } = string.Empty;
            public decimal Importe { get; set; }
            public string? ReferenciaCuenta { get; set; }
        }
        
        private class OperacionCuentaResponseDto
        {
            public int Estado { get; set; }
            public decimal Saldo { get; set; }
        }
    }
}
