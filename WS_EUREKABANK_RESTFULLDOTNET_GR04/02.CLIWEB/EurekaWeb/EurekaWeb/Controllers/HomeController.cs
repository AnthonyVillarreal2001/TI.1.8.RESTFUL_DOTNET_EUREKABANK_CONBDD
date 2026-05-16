using Microsoft.AspNetCore.Mvc;
using EurekaWeb.Models;

public class HomeController : Controller
{
    private readonly IRestEurekaClient _api;
    public HomeController(IRestEurekaClient api) => _api = api;

    [HttpGet]
    public IActionResult Menu()
    {
        var usuario = HttpContext.Session.GetString("usuario");
        if (string.IsNullOrEmpty(usuario)) return RedirectToAction("Login", "Account");
        ViewBag.Usuario = usuario;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Operacion(string action, string? cuenta, string? cuentaOrigen, string? cuentaDestino, decimal importe)
    {
        var usuario = HttpContext.Session.GetString("usuario");
        if (string.IsNullOrEmpty(usuario)) return RedirectToAction("Login", "Account");

        var vm = new ResultadoVm();
        try
        {
            switch ((action ?? "").ToLowerInvariant())
            {
                case "deposito":
                    {
                        var r = await _api.RegDeposito(cuenta ?? "", importe);
                        vm.EsExitoso = r.Estado == 1; vm.Saldo = r.Saldo; vm.Cuenta = cuenta;
                        vm.Mensaje = vm.EsExitoso ? "Depósito realizado correctamente." : "Error al realizar el depósito.";
                        break;
                    }
                case "retiro":
                    {
                        var r = await _api.RegRetiro(cuenta ?? "", importe);
                        vm.EsExitoso = r.Estado == 1; vm.Saldo = r.Saldo; vm.Cuenta = cuenta;
                        vm.Mensaje = vm.EsExitoso ? "Retiro realizado correctamente." : "Error al realizar el retiro.";
                        break;
                    }
                case "transferencia":
                    {
                        var r = await _api.RegTransferencia(cuentaOrigen ?? "", cuentaDestino ?? "", importe);
                        vm.EsExitoso = r.Estado == 1; vm.Saldo = r.Saldo; vm.Cuenta = cuentaOrigen;
                        vm.Mensaje = vm.EsExitoso ? "Transferencia realizada correctamente." : "Error al realizar la transferencia.";
                        break;
                    }
                default:
                    vm.EsExitoso = false;
                    vm.Mensaje = "Acción no válida.";
                    break;
            }
        }
        catch (Exception ex)
        {
            vm.EsExitoso = false;
            vm.Mensaje = $"Error: {ex.Message}";
        }

        return View("Resultado", vm);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }
}
