using System;
using System.Collections.Generic;
using _02.CLIMOV.Modelo;
using _02.CLIMOV.Servicio;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;

namespace _02.CLIMOV.Vista
{
    public partial class MovimientosPage : ContentPage
    {
        private readonly IRestService _restService;

        public MovimientosPage()
        {
            InitializeComponent();
            _restService = new RestService();
            
            string username = Preferences.Get("username", "Usuario");
            LblUsuario.Text = username;
        }

        private async void OnVerMovimientosClicked(object sender, EventArgs e)
        {
            string cuenta = EntryCuenta.Text?.Trim();

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                await MostrarToast("⚠️ Por favor ingrese un número de cuenta");
                return;
            }

            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            PanelMovimientos.IsVisible = false;

            try
            {
                var movimientos = await _restService.TraerMovimientosAsync(cuenta);

                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                if (movimientos != null && movimientos.Count > 0)
                {
                    ListaMovimientos.ItemsSource = movimientos;
                    LblNumeroCuenta.Text = cuenta;
                    PanelMovimientos.IsVisible = true;
                    await MostrarToast($"Se encontraron {movimientos.Count} movimientos");
                }
                else
                {
                    await MostrarToast("No se encontraron movimientos para esta cuenta");
                }
            }
            catch (Exception ex)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                await MostrarToast($"❌ Error: {ex.Message}");
            }
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmacion = await DisplayAlert(
                "Cerrar Sesión",
                "¿Está seguro de que desea cerrar sesión?",
                "Sí",
                "No");

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

    // Converter para el color del tipo de movimiento
    public class TipoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string tipo)
            {
                return tipo switch
                {
                    "001" => Color.FromArgb("#3B82F6"), // Apertura - Azul
                    "003" => Color.FromArgb("#10B981"), // Depósito - Verde
                    "004" => Color.FromArgb("#EF4444"), // Retiro - Rojo
                    "008" => Color.FromArgb("#F59E0B"), // Transferencia Salida - Naranja
                    "009" => Color.FromArgb("#8B5CF6"), // Transferencia Ingreso - Morado
                    _ => Color.FromArgb("#6B7280")      // Desconocido - Gris
                };
            }
            return Color.FromArgb("#6B7280");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
