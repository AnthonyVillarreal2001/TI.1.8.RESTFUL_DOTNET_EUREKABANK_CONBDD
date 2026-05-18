using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _02.CLIMOV.Modelo;
using _02.CLIMOV.Servicio;
using CommunityToolkit.Maui.Alerts;

namespace _02.CLIMOV.Vista
{
    public partial class LoginPage : ContentPage
    {
        private readonly IRestService _restService;

        public LoginPage()
        {
            InitializeComponent();
            _restService = new RestService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Obtener valores
            string username = EntryUsuario.Text?.Trim();
            string password = EntryPassword.Text?.Trim();

            // Debug
            System.Diagnostics.Debug.WriteLine($"🔐 Intentando login con usuario: '{username}', password: '{password}'");

            // Validar campos vacíos
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await MostrarToast("Por favor complete todos los campos");
                return;
            }

            // Mostrar loading
            MostrarLoading(true);

            try
            {
                // Validar credenciales con el servicio
                System.Diagnostics.Debug.WriteLine("📡 Llamando a ValidarIngresoAsync...");
                bool esValido = await _restService.ValidarIngresoAsync(username, password);
                System.Diagnostics.Debug.WriteLine($"✅ Resultado de validación: {esValido}");

                MostrarLoading(false);

                if (esValido)
                {
                    // Login exitoso
                    await MostrarToast("Inicio de sesión exitoso");

                    // Guardar sesión
                    Preferences.Set("isLoggedIn", true);
                    Preferences.Set("username", username);

                    // Pequeño delay para que se vea el toast
                    await Task.Delay(500);

                    // Navegar a página de menú
                    await Shell.Current.GoToAsync("//MenuPage");

                    // Limpiar campos
                    LimpiarCampos();
                }
                else
                {
                    // Login fallido
                    System.Diagnostics.Debug.WriteLine("❌ Credenciales incorrectas");
                    await MostrarToast("Usuario o contraseña incorrectos");

                    // Limpiar contraseña por seguridad
                    EntryPassword.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MostrarLoading(false);
                System.Diagnostics.Debug.WriteLine($"❌ EXCEPCIÓN en login: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                await DisplayAlert("Error de Conexión", 
                    $"No se pudo conectar al servidor.\n\nDetalle: {ex.Message}\n\n¿Está el servidor SOAP ejecutándose?", 
                    "OK");
                EntryPassword.Text = string.Empty;
            }
        }

        private async Task MostrarToast(string mensaje)
        {
            var toast = Toast.Make(mensaje, CommunityToolkit.Maui.Core.ToastDuration.Short, 16);
            await toast.Show();
        }

        private void MostrarLoading(bool mostrar)
        {
            LoadingIndicator.IsRunning = mostrar;
            LoadingIndicator.IsVisible = mostrar;
            BtnLogin.IsEnabled = !mostrar;
            BtnLogin.Text = mostrar ? "INGRESANDO..." : "INGRESAR";
        }

        private void LimpiarCampos()
        {
            EntryUsuario.Text = string.Empty;
            EntryPassword.Text = string.Empty;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LimpiarCampos();
        }
    }
}
