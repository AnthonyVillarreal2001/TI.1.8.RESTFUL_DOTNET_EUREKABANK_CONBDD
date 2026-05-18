using System.Globalization;
using System.Net.Http.Json;
using System.Text;

// ============================================================================
// Modelos (idénticos)
// ============================================================================
public record Movimiento(int Numero, DateTime Fecha, string TipoCodigo, decimal Importe, string? ReferenciaCuenta);
public record OperacionCuentaResponse(int Estado, decimal Saldo);
public record LoginRequest(string usuario, string password);

// ============================================================================
// Cliente API (modificado: LoginAsync recibe parámetros)
// ============================================================================
static class Api
{
    private static readonly HttpClient http = new()
    {
        BaseAddress = new Uri(Environment.GetEnvironmentVariable("EB_API_BASE") ?? "https://localhost:7043/")
    };

    public static async Task<bool> LoginAsync(string usuario, string password)
    {
        var resp = await http.PostAsJsonAsync("api/CoreBancario/validarIngreso", new LoginRequest(usuario, password));
        var texto = (await resp.Content.ReadAsStringAsync()).Trim();
        return texto.Equals("Exitoso", StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<List<Movimiento>?> ListarMovimientosAsync(string cuenta)
    {
        try
        {
            return await http.GetFromJsonAsync<List<Movimiento>>($"api/CoreBancario/cuentas/{cuenta}/movimientos");
        }
        catch { return null; }
    }

    public static async Task<OperacionCuentaResponse?> DepositoAsync(string cuenta, decimal importe)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/deposito", new { cuenta, importe });
            return await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();
        }
        catch { return null; }
    }

    public static async Task<OperacionCuentaResponse?> RetiroAsync(string cuenta, decimal importe)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/retiro", new { cuenta, importe });
            return await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();
        }
        catch { return null; }
    }

    public static async Task<OperacionCuentaResponse?> TransferenciaAsync(string origen, string destino, decimal importe)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("api/CoreBancario/transferencia", new { cuentaOrigen = origen, cuentaDestino = destino, importe });
            return await resp.Content.ReadFromJsonAsync<OperacionCuentaResponse>();
        }
        catch { return null; }
    }
}

// ============================================================================
// Clase principal con interfaz de consola estilizada
// ============================================================================
class Program
{
    // Paleta de colores
    static readonly ConsoleColor PRIMARY = ConsoleColor.DarkBlue;
    static readonly ConsoleColor SECONDARY = ConsoleColor.DarkYellow;
    static readonly ConsoleColor SUCCESS = ConsoleColor.Green;
    static readonly ConsoleColor ERROR = ConsoleColor.Red;
    static readonly ConsoleColor ACCENT = ConsoleColor.Cyan;
    static readonly ConsoleColor BG_DEFAULT = ConsoleColor.Black;
    static readonly ConsoleColor TEXT_DEFAULT = ConsoleColor.Gray;

    // Estado global
    static string loggedUser = "";
    static int activeCard = -1;
    static bool exitToLogin = false;

    static async Task Main()
    {
        // Configurar cultura y codificación
        var ec = CultureInfo.CreateSpecificCulture("es-EC");
        ec.NumberFormat.CurrencySymbol = "$";
        CultureInfo.DefaultThreadCurrentCulture = ec;
        CultureInfo.DefaultThreadCurrentUICulture = ec;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "EurekaBank - Cliente REST";

        bool loggedIn = false;
        while (!loggedIn)
        {
            Console.BackgroundColor = BG_DEFAULT;
            Console.ForegroundColor = TEXT_DEFAULT;
            Console.Clear();

            DibujarBordeTitulo("BIENVENIDO A EUREKABANK", PRIMARY);
            Console.WriteLine();

            Console.Write("Usuario: ");
            string? user = Console.ReadLine()?.Trim();
            Console.Write("Contraseña: ");
            string? pass = LeerPassword();  // oculta con asteriscos

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                Console.ForegroundColor = ERROR;
                Console.WriteLine("\n✘ Usuario y contraseña son requeridos.");
                Console.WriteLine("\nPresione cualquier tecla para reintentar...");
                Console.ReadKey();
                continue;
            }

            if (await Api.LoginAsync(user, pass))
            {
                loggedUser = user.ToUpper();
                loggedIn = true;
            }
            else
            {
                Console.ForegroundColor = ERROR;
                Console.WriteLine("\n✘ Acceso denegado.");
                Console.WriteLine("\nPresione cualquier tecla para reintentar...");
                Console.ReadKey();
            }
        }

        // Bucle principal de la aplicación (menú)
        bool salirAplicacion = false;
        while (!salirAplicacion)
        {
            Console.Clear();
            activeCard = -1;
            while (true)  // bucle interno para mostrar menú hasta cerrar sesión o salir
            {
                DibujarHeader();
                DibujarCards();
                Console.WriteLine();
                DibujarFooter();

                Console.Write("\n→ Seleccione una opción (1-4, 0=Cerrar sesión, X=Salir): ");
                string input = Console.ReadLine()?.Trim() ?? "";

                if (input.Equals("0", StringComparison.OrdinalIgnoreCase))
                {
                    // Cerrar sesión: volver a login
                    loggedIn = false;
                    salirAplicacion = false; // no sale del programa, solo reinicia login
                    break;
                }
                else if (input.Equals("X", StringComparison.OrdinalIgnoreCase))
                {
                    salirAplicacion = true;
                    break;
                }
                else if (int.TryParse(input, out int op) && op >= 1 && op <= 4)
                {
                    // Acordeón: misma card => cerrar; diferente => abrir
                    if (activeCard == op)
                        activeCard = -1;
                    else
                        activeCard = op;

                    if (activeCard != -1)
                    {
                        Console.Clear();
                        DibujarHeader();
                        Console.WriteLine();
                        await EjecutarCard(activeCard);
                        Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                        Console.ReadKey();
                        Console.Clear();
                        activeCard = -1;
                    }
                }
                else
                {
                    Console.ForegroundColor = ERROR;
                    Console.WriteLine("  Opción inválida. Presione una tecla para continuar.");
                    Console.ReadKey();
                    Console.Clear();
                }
            }

            // Si se cerró sesión, volvemos al bucle externo para pedir login nuevamente
            if (!salirAplicacion && !loggedIn)
            {
                // Limpiar y volver a login
                continue;
            }
        }

        Console.ForegroundColor = TEXT_DEFAULT;
        Console.Clear();
        Console.WriteLine("Gracias por usar EurekaBank. ¡Hasta luego!");
    }

    // ========================================================================
    // Funciones auxiliares de UI
    // ========================================================================

    static string LeerPassword()
    {
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return password;
    }

    static void DibujarBordeTitulo(string titulo, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        int ancho = titulo.Length + 4;
        Console.WriteLine(new string('═', ancho));
        Console.WriteLine($"  {titulo}  ");
        Console.WriteLine(new string('═', ancho));
        Console.ForegroundColor = TEXT_DEFAULT;
    }

    static void DibujarHeader()
    {
        Console.BackgroundColor = PRIMARY;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("  EUREKABANK  ");
        Console.BackgroundColor = BG_DEFAULT;
        Console.ForegroundColor = TEXT_DEFAULT;
        Console.Write("   ");
        Console.ForegroundColor = ACCENT;
        Console.Write($"👤 {loggedUser}");
        Console.ForegroundColor = TEXT_DEFAULT;
        Console.WriteLine("   [0=Cerrar sesión | X=Salir]");
        Console.WriteLine(new string('─', Console.WindowWidth - 1));
    }

    static void DibujarCards()
    {
        string[] titulos = { "📋 Consulta de Movimientos", "💰 Depósito", "💸 Retiro", "🔄 Transferencia" };
        string[] iconos = { "📋", "💰", "💸", "🔄" };

        for (int i = 0; i < titulos.Length; i++)
        {
            bool activa = (activeCard == i + 1);
            if (activa)
                Console.BackgroundColor = PRIMARY;

            Console.ForegroundColor = activa ? ConsoleColor.White : SECONDARY;
            Console.Write($"  {i + 1}. {iconos[i]} {titulos[i]}");
            if (activa)
                Console.Write("  [▲]");
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    static async Task EjecutarCard(int cardNumber)
    {
        switch (cardNumber)
        {
            case 1: await ConsultaMovimientos(); break;
            case 2: await Deposito(); break;
            case 3: await Retiro(); break;
            case 4: await Transferencia(); break;
        }
    }

    static async Task ConsultaMovimientos()
    {
        Console.ForegroundColor = ACCENT;
        Console.WriteLine(">> CONSULTA DE MOVIMIENTOS");
        Console.ResetColor();
        Console.Write("Número de cuenta: ");
        string? cuenta = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(cuenta))
        {
            MostrarModal("Error", "La cuenta no puede estar vacía.", false);
            return;
        }

        var movimientos = await Api.ListarMovimientosAsync(cuenta);
        if (movimientos == null || movimientos.Count == 0)
        {
            MostrarModal("Sin movimientos", $"La cuenta {cuenta} no tiene movimientos registrados.", false);
            return;
        }

        var ordenados = movimientos.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Numero).ToList();

        Console.Clear();
        DibujarHeader();
        Console.WriteLine($"\n  Cuenta: {cuenta}");
        Console.WriteLine($"  Total movimientos: {ordenados.Count}\n");

        Console.ForegroundColor = PRIMARY;
        Console.WriteLine("┌─────┬──────────────────────┬───────────────┬──────────┬────────────┬──────────────┐");
        Console.WriteLine("│ Nro │ Fecha                 │ Tipo          │ Acción   │ Importe    │ Referencia   │");
        Console.WriteLine("├─────┼──────────────────────┼───────────────┼──────────┼────────────┼──────────────┤");
        Console.ResetColor();

        foreach (var m in ordenados)
        {
            var (tipo, accion, esIngreso) = MapearTipo(m.TipoCodigo);
            ConsoleColor bgFila = esIngreso ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
            Console.BackgroundColor = bgFila;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"│ {m.Numero,-3} ");
            Console.Write($"│ {m.Fecha:yyyy-MM-dd HH:mm} ");
            Console.Write($"│ {tipo,-13} ");
            Console.Write($"│ {accion,-8} ");
            Console.Write($"│ {m.Importe,10:C} ");
            Console.Write($"│ {(m.ReferenciaCuenta ?? "---"),-12} │");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.ForegroundColor = PRIMARY;
        Console.WriteLine("└─────┴──────────────────────┴───────────────┴──────────┴────────────┴──────────────┘");
        Console.ResetColor();
    }

    static async Task Deposito()
    {
        Console.ForegroundColor = ACCENT;
        Console.WriteLine(">> DEPÓSITO");
        Console.ResetColor();
        Console.Write("Cuenta destino: ");
        string? cuenta = Console.ReadLine()?.Trim();
        Console.Write("Importe (USD): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe) || importe <= 0)
        {
            MostrarModal("Error", "Importe inválido.", false);
            return;
        }

        var resultado = await Api.DepositoAsync(cuenta!, importe);
        if (resultado != null && resultado.Estado == 1)
            MostrarModal("Depósito exitoso", $"Cuenta: {cuenta}\nNuevo saldo: {resultado.Saldo:C}", true);
        else
            MostrarModal("Error en depósito", "No se pudo realizar la operación. Verifique la cuenta.", false);
    }

    static async Task Retiro()
    {
        Console.ForegroundColor = ACCENT;
        Console.WriteLine(">> RETIRO");
        Console.ResetColor();
        Console.Write("Cuenta origen: ");
        string? cuenta = Console.ReadLine()?.Trim();
        Console.Write("Importe (USD): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe) || importe <= 0)
        {
            MostrarModal("Error", "Importe inválido.", false);
            return;
        }

        var resultado = await Api.RetiroAsync(cuenta!, importe);
        if (resultado != null && resultado.Estado == 1)
            MostrarModal("Retiro exitoso", $"Cuenta: {cuenta}\nNuevo saldo: {resultado.Saldo:C}", true);
        else
            MostrarModal("Error en retiro", "Saldo insuficiente o cuenta inválida.", false);
    }

    static async Task Transferencia()
    {
        Console.ForegroundColor = ACCENT;
        Console.WriteLine(">> TRANSFERENCIA");
        Console.ResetColor();
        Console.Write("Cuenta origen: ");
        string? origen = Console.ReadLine()?.Trim();
        Console.Write("Cuenta destino: ");
        string? destino = Console.ReadLine()?.Trim();
        Console.Write("Importe (USD): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe) || importe <= 0)
        {
            MostrarModal("Error", "Importe inválido.", false);
            return;
        }

        var resultado = await Api.TransferenciaAsync(origen!, destino!, importe);
        if (resultado != null && resultado.Estado == 1)
            MostrarModal("Transferencia exitosa", $"Origen: {origen}\nDestino: {destino}\nSaldo origen: {resultado.Saldo:C}", true);
        else
            MostrarModal("Error en transferencia", "Saldo insuficiente o cuentas inválidas.", false);
    }

    static void MostrarModal(string titulo, string mensaje, bool esExito)
    {
        Console.Clear();
        Console.BackgroundColor = BG_DEFAULT;
        int ancho = Math.Min(60, Console.WindowWidth - 4);
        string linea = new string('═', ancho);

        Console.ForegroundColor = esExito ? SUCCESS : ERROR;
        Console.WriteLine($"\n  {linea}");
        Console.WriteLine($"  {(esExito ? "✔" : "✘")} {titulo}");
        Console.WriteLine($"  {linea}");
        Console.ResetColor();
        Console.ForegroundColor = TEXT_DEFAULT;
        Console.WriteLine($"  {mensaje}");
        Console.WriteLine($"  {linea}");

        if (esExito)
        {
            Console.ForegroundColor = SUCCESS;
            Console.WriteLine("  ¡CONFETI! 🎉🎉🎉");
            Console.ResetColor();
        }

        Console.WriteLine("\n  Presione cualquier tecla para continuar...");
        Console.ReadKey();
        Console.Clear();
    }

    static void DibujarFooter()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(new string('─', Console.WindowWidth - 1));
        Console.WriteLine("  Desarrollado por Ariel R. y Anthony V. | EurekaBank © 2025");
        Console.ResetColor();
    }

    static (string tipo, string accion, bool esIngreso) MapearTipo(string codigo)
    {
        return codigo switch
        {
            "003" => ("DEPÓSITO", "INGRESO", true),
            "004" => ("RETIRO", "SALIDA", false),
            "008" => ("TRANSFERENCIA", "INGRESO", true),
            "009" => ("TRANSFERENCIA", "SALIDA", false),
            _ => ("DESCONOCIDO", "", false)
        };
    }
}