using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ec.edu.monster.services;
using ec.edu.monster.models;
using ec.edu.monster.utils;
using ec.edu.monster.views;

namespace ec.edu.monster.controllers
{
    public class HomeController
    {
        private readonly HomeWindow _view;
        private readonly IEurekaApi _api;

        public HomeController(HomeWindow view, IEurekaApi api)
        {
            _view = view;
            _api = api;

            _view.SetUser(LoginController.UsuarioActual ?? "USUARIO");

            _view.ConsultarMov += async cta =>
            {
                if (!ValCuenta(cta, out var msg)) { _view.ToastError(msg); return; }
                try
                {
                    var list = await _api.TraerMovimientos(cta);
                    var dlg = new MovimientosDialog(cta, list);
                    dlg.Owner = _view;
                    dlg.ShowDialog();
                }
                catch (Exception ex) { _view.ToastError("No se pudo obtener movimientos: " + ex.Message); }
            };

            _view.Depositar += async (cta, impTxt) =>
            {
                if (!ValCuenta(cta, out var m1)) { _view.ToastError(m1); return; }
                if (!ValImporte(impTxt, out var imp, out var m2)) { _view.ToastError(m2); return; }

                await DoOp(async () => await _api.RegDeposito(cta, imp),
                    ok => _view.ToastOK($"Depósito {Money.Fmt(imp)} realizado. Saldo: {Money.Fmt(ok.Saldo)}"),
                    "depósito");
            };

            _view.Retirar += async (cta, impTxt) =>
            {
                if (!ValCuenta(cta, out var m1)) { _view.ToastError(m1); return; }
                if (!ValImporte(impTxt, out var imp, out var m2)) { _view.ToastError(m2); return; }

                await DoOp(async () => await _api.RegRetiro(cta, imp),
                    ok => _view.ToastOK($"Retiro {Money.Fmt(imp)} realizado. Saldo: {Money.Fmt(ok.Saldo)}"),
                    "retiro");
            };

            _view.Transferir += async (o, d, impTxt) =>
            {
                if (!ValCuenta(o, out var mo)) { _view.ToastError("Origen: " + mo); return; }
                if (!ValCuenta(d, out var md)) { _view.ToastError("Destino: " + md); return; }
                if (o == d) { _view.ToastError("La cuenta origen y destino no pueden ser iguales."); return; }
                if (!ValImporte(impTxt, out var imp, out var mi)) { _view.ToastError(mi); return; }

                await DoOp(async () => await _api.RegTransferencia(o, d, imp),
                    ok => _view.ToastOK($"Transferencia realizada. Saldo origen: {Money.Fmt(ok.Saldo)}"),
                    "transferencia");
            };
        }

        private async Task DoOp(Func<Task<OperacionCuentaResponse>> call,
                                Action<OperacionCuentaResponse> onOk,
                                string nombre)
        {
            _view.SetBusy(true);
            try
            {
                var r = await call();
                if (r == null || r.Estado != 1)
                {
                    _view.ToastError(
                        "La operación fue rechazada por el servidor.\n\n" +
                        "Pistas:\n- Verifica la cuenta (existe y ACTIVA)\n" +
                        "- Empleados 0001/0004 y tipos 003/004/008/009\n" +
                        "- Conexión del servidor a la BD correcta.");
                    return;
                }
                onOk(r);
            }
            catch (Exception ex)
            {
                _view.ToastError($"Error al realizar {nombre}: {ex.Message}");
            }
            finally { _view.SetBusy(false); }
        }

        private static bool ValCuenta(string cuenta, out string msg)
        {
            msg = null;
            if (string.IsNullOrWhiteSpace(cuenta)) { msg = "Ingrese el número de cuenta."; return false; }
            if (!Regex.IsMatch(cuenta.Trim(), @"^\d{8}$")) { msg = "La cuenta debe tener 8 dígitos."; return false; }
            return true;
        }

        private static bool ValImporte(string txt, out decimal valor, out string msg)
        {
            msg = null;
            if (!decimal.TryParse(txt, out valor)) { msg = "Importe inválido."; return false; }
            if (valor <= 0) { msg = "El importe debe ser positivo."; return false; }
            return true;
        }
    }
}
