using System.Threading.Tasks;
using System.Windows;
using ec.edu.monster.services;
using ec.edu.monster.views;

namespace ec.edu.monster.controllers
{
    public class LoginController
    {
        private readonly LoginWindow _view;
        private readonly IEurekaApi _api;

        public static string UsuarioActual { get; private set; }

        public LoginController(LoginWindow view, IEurekaApi api)
        {
            _view = view;
            _api = api;

            _view.LoginClicked += async (u, p) => await DoLogin(u, p);
        }

        private async Task DoLogin(string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                _view.ShowError("Ingresa usuario y contraseña.");
                return;
            }

            // Bypass para demo sin servidor: acepta MONSTER / MONSTER9
            if (string.Equals(user, "MONSTER", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(pass, "MONSTER9", StringComparison.OrdinalIgnoreCase))
            {
                UsuarioActual = user.ToUpperInvariant();
                var home = new HomeWindow();
                var ctrl = new HomeController(home, _api);
                System.Windows.Application.Current.MainWindow = home;
                _view.Hide();
                home.Show();
                _view.Close();
                return;
            }

            _view.SetBusy(true);
            try
            {
                var ok = await _api.ValidarIngreso(user, pass);
                if (!ok)
                {
                    _view.ShowError("Credenciales inválidas.");
                    return;
                }

                UsuarioActual = user.ToUpperInvariant();
                var home = new HomeWindow();
                var ctrl = new HomeController(home, _api);
                _view.Hide();
                home.Show();
                _view.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error conectando al servicio: " + ex.Message,
                    "Login", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _view.SetBusy(false);
            }
        }
    }
}
