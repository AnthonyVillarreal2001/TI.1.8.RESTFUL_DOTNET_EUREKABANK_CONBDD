using System;
using _02.CLIMOV.Servicio;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;

namespace _02.CLIMOV.Vista
{
    public partial class DepositoPage : ContentPage
    {
        private readonly IRestService _restService;

        public DepositoPage()
        {
            InitializeComponent();
            _restService = new RestService();
            LblUsuario.Text = Preferences.Get("username", "Usuario");
        }

        private async void OnDepositarClicked(object sender, EventArgs e)
        {
            string cuenta = EntryCuenta.Text?.Trim();
            string importeStr = EntryImporte.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cuenta) || string.IsNullOrWhiteSpace(importeStr))
            {
                await MostrarToast("Complete todos los campos");
                return;
            }

            if (!double.TryParse(importeStr, out double importe) || importe <= 0)
            {
                await MostrarToast("Ingrese un importe válido");
                return;
            }

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            BtnDepositar.IsEnabled = false;

            try
            {
                var resultado = await _restService.RegistrarDepositoAsync(cuenta, importe);

                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                BtnDepositar.IsEnabled = true;

                if (resultado.Exito)
                {
                    string mensajeSaldo = resultado.SaldoActual > 0 
                        ? $"\n\nNuevo saldo disponible\nCuenta {cuenta}: $ {resultado.SaldoActual:F2}"
                        : "\n\nConsulte movimientos para ver el saldo actualizado";
                    
                    await DisplayAlert("Operación Exitosa", 
                        $"Depósito realizado correctamente por ${importe:F2}{mensajeSaldo}", 
                        "Aceptar");
                    EntryCuenta.Text = string.Empty;
                    EntryImporte.Text = string.Empty;
                }
                else
                {
                    await DisplayAlert("Error en la Operación", 
                        $"Error al realizar el depósito.\n\n{resultado.Mensaje}", 
                        "Aceptar");
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                BtnDepositar.IsEnabled = true;
                await DisplayAlert("Error", $"Error: {ex.Message}", "Aceptar");
            }
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmacion = await DisplayAlert("Cerrar Sesión", "¿Está seguro de que desea cerrar sesión?", "Sí", "No");
            if (confirmacion)
            {
                Preferences.Remove("isLoggedIn");
                Preferences.Remove("username");
                await Shell.Current.GoToAsync("//LoginPage");
            }
        }

        private async void OnVolverClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MenuPage");
        }

        private async Task MostrarToast(string mensaje)
        {
            var toast = Toast.Make(mensaje, CommunityToolkit.Maui.Core.ToastDuration.Short, 16);
            await toast.Show();
        }
    }
}
