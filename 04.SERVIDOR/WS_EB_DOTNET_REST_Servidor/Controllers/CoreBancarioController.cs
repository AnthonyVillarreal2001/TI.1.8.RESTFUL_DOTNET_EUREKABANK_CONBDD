using Microsoft.AspNetCore.Mvc;
using WS_EB_DOTNET_REST_Servidor.Models;
using WS_EB_DOTNET_REST_Servidor.Services;

namespace WS_EB_DOTNET_REST_Servidor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoreBancarioController : ControllerBase
    {
        private readonly EurekaService _service;
        public CoreBancarioController(EurekaService service) => _service = service;

        public record LoginRequest(string usuario, string password);
        public record OperacionRequest(string cuenta, decimal importe, string? empleado = null);
        public record TransferenciaRequest(string cuentaOrigen, string cuentaDestino, decimal importe, string? empleado = null);

        // ===== LOGIN (igual que SOAP: "Exitoso"/"Denegado") =====
        [HttpPost("validarIngreso")]
        public ContentResult ValidarIngreso([FromBody] LoginRequest req)
        {
            bool ok = _service.ValidarIngreso(req.usuario, req.password);
            return Content(ok ? "Exitoso" : "Denegado", "text/plain");
        }

        // ===== MOVIMIENTOS =====
        [HttpGet("cuentas/{cuenta}/movimientos")]
        public ActionResult<IEnumerable<Movimiento>> TraerMovimientos(string cuenta)
            => Ok(_service.ListarMovimientos(cuenta));

        // ===== DEPÓSITO =====
        [HttpPost("deposito")]
        public ActionResult<OperacionCuentaResponse> RegDeposito([FromBody] OperacionRequest req)
        {
            if (req.importe <= 0) return BadRequest("Importe inválido");
            try
            {
                var saldo = _service.RegistrarDeposito(req.cuenta, req.importe, req.empleado ?? "0001");
                return Ok(new OperacionCuentaResponse { Estado = 1, Saldo = saldo });
            }
            catch
            {
                return Ok(new OperacionCuentaResponse { Estado = -1, Saldo = -1 });
            }
        }

        // ===== RETIRO =====
        [HttpPost("retiro")]
        public ActionResult<OperacionCuentaResponse> RegRetiro([FromBody] OperacionRequest req)
        {
            if (req.importe <= 0) return BadRequest("Importe inválido");
            try
            {
                var saldo = _service.RegistrarRetiro(req.cuenta, req.importe, req.empleado ?? "0004");
                return Ok(new OperacionCuentaResponse { Estado = 1, Saldo = saldo });
            }
            catch
            {
                return Ok(new OperacionCuentaResponse { Estado = -1, Saldo = -1 });
            }
        }

        // ===== TRANSFERENCIA (saldo = saldo de ORIGEN) =====
        [HttpPost("transferencia")]
        public ActionResult<OperacionCuentaResponse> RegTransferencia([FromBody] TransferenciaRequest req)
        {
            if (req.importe <= 0) return BadRequest("Importe inválido");
            try
            {
                var saldoOrigen = _service.RegistrarTransferencia(req.cuentaOrigen, req.cuentaDestino, req.importe, req.empleado ?? "0004");
                return Ok(new OperacionCuentaResponse { Estado = 1, Saldo = saldoOrigen });
            }
            catch
            {
                return Ok(new OperacionCuentaResponse { Estado = -1, Saldo = -1 });
            }
        }
    }
}
