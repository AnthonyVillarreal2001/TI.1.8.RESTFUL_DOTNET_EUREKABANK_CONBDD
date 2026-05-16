using System.Globalization;
using System.Net.Http.Json;
using System.Linq;
using System;
using System.Collections.Generic;

// ===== Modelos que mapea el cliente al servidor REST =====
public record Movimiento(int Numero, DateTime Fecha, string TipoCodigo, decimal Importe, string? ReferenciaCuenta);
public record OperacionCuentaResponse(int Estado, decimal Saldo);
public record LoginRequest(string usuario, string password);

static class Api
{
    private static readonly HttpClient http = new()
    {
        // Cambia con EB_API_BASE si quieres otro host/puerto
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("EB_API_BASE") ?? "https://localhost:7043/")
    };

    // ===== LOGIN =====
    public static async Task<bool> LoginAsync()
    {
        Console.WriteLine("=== Inicio de sesión ===");
        Console.Write("Usuario: ");
        var usuario = Console.ReadLine()?.Trim();
        Console.Write("Password: ");
        var password = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("\nAcceso denegado");
            return false;
        }

        var resp = await http.PostAsJsonAsync("api/CoreBancario/validarIngreso", new LoginRequest(usuario!, password!));
        var texto = (await resp.Content.ReadAsStringAsync()).Trim();
        if (texto.Equals("Exitoso", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\nAcceso concedido");
            return true;
        }

        Console.WriteLine("\nAcceso denegado");
        return false;
    }

    // ===== Helper de presentación: mapea código a Tipo/Acción =====
    static (string Tipo, string Accion) MapTipoAccion(string codigo)
    {
        return codigo switch
        {
            "003" => ("DEPOSITO", "INGRESO"),
            "004" => ("RETIRO", "SALIDA"),
            "008" => ("TRANSFERENCIA", "INGRESO"),
            "009" => ("TRANSFERENCIA", "SALIDA"),
            "001" => ("APERTURA", "INGRESO"),
            _ => ("DESCONOCIDO", "")
        };
    }

    // ===== LISTAR MOVIMIENTOS =====
    public static async Task ListarMovimientosAsync()
    {
        Console.Write("\nCuenta: ");
        var cuenta = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(cuenta))
        {
            Console.WriteLine("Cuenta requerida.");
            return;
        }

        List<Movimiento>? movs;
        try
        {
            movs = await http.GetFromJsonAsync<List<Movimiento>>($"api/CoreBancario/cuentas/{cuenta}/movimientos");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al consultar movimientos: {ex.Message}");
            return;
        }

        Console.WriteLine($"\nMovimientos de la cuenta {cuenta}");
        Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");
        Console.WriteLine($"{"#",-4} {"Fecha",-20} {"Tipo",-15} {"Acción",-10} {"Importe",12} {"Ref.",-10}");
        Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");

        if (movs is { Count: > 0 })
        {
            // Orden descendente por fecha y número, como SOAP
            var ordenados = movs
                .OrderByDescending(m => m.Fecha)
                .ThenByDescending(m => m.Numero)
                .ToList();

            foreach (var m in ordenados)
            {
                var (tipo, accion) = MapTipoAccion(m.TipoCodigo);
                Console.WriteLine($"{m.Numero,-4} {m.Fecha,-20:yyyy-MM-dd HH:mm} {tipo,-15} {accion,-10} {m.Importe,12:C} {m.ReferenciaCuenta,-10}");
            }
        }
        else
        {
            Console.WriteLine("No hay movimientos.");
        }

        Console.WriteLine("───────────────────────────────────────────────────────────────────────────────");
    }

    // ===== DEPÓSITO =====
    public static async Task DepositoAsync()
    {
        Console.Write("\nCuenta: ");
        var cuenta = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(cuenta))
        {
            Console.WriteLine("Cuenta requerida.");
            return;
        }

        Console.Write("Importe: ");
        if (!decimal.TryParse(Console.ReadLine(), out var importe) || importe <= 0)
        {
            Console.WriteLine("Importe inválido.");
            return;
        }

        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/deposito", new { cuenta, importe });
            var data = await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();

            if (data is { Estado: 1 })
                Console.WriteLine($"Operación exitosa. Saldo actual: {data.Saldo:C}");
            else
                Console.WriteLine("Operación fallida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en depósito: {ex.Message}");
        }
    }

    // ===== RETIRO =====
    public static async Task RetiroAsync()
    {
        Console.Write("\nCuenta: ");
        var cuenta = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(cuenta))
        {
            Console.WriteLine("Cuenta requerida.");
            return;
        }

        Console.Write("Importe: ");
        if (!decimal.TryParse(Console.ReadLine(), out var importe) || importe <= 0)
        {
            Console.WriteLine("Importe inválido.");
            return;
        }

        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/retiro", new { cuenta, importe });
            var data = await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();

            if (data is { Estado: 1 })
                Console.WriteLine($"Operación exitosa. Saldo actual: {data.Saldo:C}");
            else
                Console.WriteLine("Operación fallida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en retiro: {ex.Message}");
        }
    }

    // ===== TRANSFERENCIA =====
    public static async Task TransferenciaAsync()
    {
        Console.Write("\nCuenta Origen: ");
        var origen = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(origen))
        {
            Console.WriteLine("Cuenta origen requerida.");
            return;
        }

        Console.Write("Cuenta Destino: ");
        var destino = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(destino))
        {
            Console.WriteLine("Cuenta destino requerida.");
            return;
        }

        Console.Write("Importe: ");
        if (!decimal.TryParse(Console.ReadLine(), out var importe) || importe <= 0)
        {
            Console.WriteLine("Importe inválido.");
            return;
        }

        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/transferencia", new { cuentaOrigen = origen, cuentaDestino = destino, importe });
            var data = await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();

            if (data is { Estado: 1 })
                Console.WriteLine($"Operación exitosa. Saldo cuenta origen: {data.Saldo:C}");
            else
                Console.WriteLine("Operación fallida.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en transferencia: {ex.Message}");
        }
    }
}

class Program
{
    static async Task Main()
    {
        // Fijar cultura a es-EC (símbolo $ y formato local)
        var ec = CultureInfo.CreateSpecificCulture("es-EC");
        ec.NumberFormat.CurrencySymbol = "$";
        CultureInfo.DefaultThreadCurrentCulture = ec;
        CultureInfo.DefaultThreadCurrentUICulture = ec;

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Bienvenido al cliente REST de EurekaBank\n");

        if (!await Api.LoginAsync()) return;

        while (true)
        {
            Console.WriteLine("\n──── Menú Principal ────");
            Console.WriteLine("1) Listar Movimientos");
            Console.WriteLine("2) Depósito");
            Console.WriteLine("3) Retiro");
            Console.WriteLine("4) Transferencia");
            Console.WriteLine("0) Salir");
            Console.Write("Opción: ");
            var k = Console.ReadLine();

            try
            {
                switch (k)
                {
                    case "1": await Api.ListarMovimientosAsync(); break;
                    case "2": await Api.DepositoAsync(); break;
                    case "3": await Api.RetiroAsync(); break;
                    case "4": await Api.TransferenciaAsync(); break;
                    case "0": return;
                    default: Console.WriteLine("Opción inválida"); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
