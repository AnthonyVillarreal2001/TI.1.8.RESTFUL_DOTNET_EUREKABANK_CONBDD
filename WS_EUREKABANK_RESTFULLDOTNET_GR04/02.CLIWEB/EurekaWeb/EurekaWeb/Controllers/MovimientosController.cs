using Microsoft.AspNetCore.Mvc;
using EurekaWeb.Models;
using EurekaWeb.Models.Rest;
using System.Linq;

public class MovimientosController : Controller
{
    private readonly IRestEurekaClient _api;
    public MovimientosController(IRestEurekaClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index(string cuenta)
    {
        if (string.IsNullOrWhiteSpace(cuenta))
            return RedirectToAction("Menu", "Home");

        var datos = await _api.TraerMovimientos(cuenta);
        System.Diagnostics.Debug.WriteLine($"[REST Movimientos] Cuenta={cuenta} Cantidad={datos?.Length ?? 0}");

        var vm = new MovimientosVm
        {
            Cuenta = cuenta,
            Movs = (datos ?? Array.Empty<Movimiento>())
               .Select(m => MovimientosVm.Item.FromDto(m))
               .ToList()
        };
        return View(vm);
    }
}
