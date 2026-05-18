using System;
using _02.CLIMOV.Servicio;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;

namespace _02.CLIMOV.Vista
{
    public partial class TransferenciaPage : ContentPage
    {
        private readonly IRestService _restService;

        public TransferenciaPage()
        {
            InitializeComponent();
            _restService = new RestService();
            LblUsuario.Text = Preferences.Get("username", "Usuario");
        }

        private async void OnTransferirClicked(object sender, EventArgs e)
        {
            string cuentaOrigen = EntryCuentaOrigen.Text?.Trim();
            string cuentaDestino = EntryCuentaDestino.Text?.Trim();
            string importeStr = EntryImporte.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cuentaOrigen) || string.IsNullOrWhiteSpace(cuentaDestino) || string.IsNullOrWhiteSpace(importeStr))
            {
                await MostrarToast("Complete todos los campos");
                return;
            }

            if (!double.TryParse(importeStr, out double importe) || importe <= 0)
            {
                await MostrarToast("Ingrese un importe válido");
                return;
            }

            if (cuentaOrigen == cuentaDestino)
            {
                await MostrarToast("No puede transferir a la misma cuenta");
                return;
            }

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            BtnTransferir.IsEnabled = false;

            try
            {
                var resultado = await _restService.RegistrarTransferenciaAsync(cuentaOrigen, cuentaDestino, importe);

                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                BtnTransferir.IsEnabled = true;

                if (resultado.Exito)
                {
                    string mensajeSaldo = resultado.SaldoActual > 0 
                        ? $"\n\nNuevo saldo disponible\nCuenta {cuentaOrigen}: $ {resultado.SaldoActual:F2}"
                        : "\n\nConsulte movimientos para ver el saldo actualizado";
                    
                    await DisplayAlert("Operación Exitosa", 
                        $"Transferencia realizada correctamente por ${importe:F2}\n\n" +
                        $"Desde cuenta: {cuentaOrigen}\n" +
                        $"Hacia cuenta: {cuentaDestino}{mensajeSaldo}", 
                        "Aceptar");
                    EntryCuentaOrigen.Text = string.Empty;
                    EntryCuentaDestino.Text = string.Empty;
                    EntryImporte.Text = string.Empty;
                }
                else
                {
                    await DisplayAlert("Error en la Operación", 
                        $"Error al realizar la transferencia.\n\n{resultado.Mensaje}", 
                        "Aceptar");
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                BtnTransferir.IsEnabled = true;
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
