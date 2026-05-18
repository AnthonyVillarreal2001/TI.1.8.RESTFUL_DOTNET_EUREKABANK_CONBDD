using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly IRestEurekaClient _api;
    public AccountController(IRestEurekaClient api) => _api = api;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string usuario, string password)
    {
        var r = await _api.ValidarIngreso(usuario, password);
        if (string.Equals(r, "Exitoso", StringComparison.OrdinalIgnoreCase))
        {
            HttpContext.Session.SetString("usuario", usuario ?? "");
            return RedirectToAction("Menu", "Home");
        }
        ViewBag.Error = "Usuario o contraseña incorrectos.";
        return View();
    }
}
