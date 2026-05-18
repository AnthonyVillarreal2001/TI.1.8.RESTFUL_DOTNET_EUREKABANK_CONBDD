using System;
using Microsoft.Maui.Controls;

namespace _02.CLIMOV.Vista
{
    public partial class MenuPage : ContentPage
    {
        private StackLayout _currentOpenForm;

        public MenuPage()
        {
            InitializeComponent();

            string username = Preferences.Get("username", "Usuario");
            LblUsuario.Text = username;
        }

        // Eventos de toggle para abrir/cerrar formularios (acordeón)
        private void OnConsultarMovimientosClicked(object sender, TappedEventArgs e)
        {
            ToggleForm(FormMovimientos);
        }

        private void OnDepositoClicked(object sender, TappedEventArgs e)
        {
            ToggleForm(FormDeposito);
        }

        private void OnRetiroClicked(object sender, TappedEventArgs e)
        {
            ToggleForm(FormRetiro);
        }

        private void OnTransferenciaClicked(object sender, TappedEventArgs e)
        {
            ToggleForm(FormTransferencia);
        }

        private void ToggleForm(StackLayout form)
        {
            if (_currentOpenForm != null && _currentOpenForm != form)
            {
                _currentOpenForm.IsVisible = false;
            }

            form.IsVisible = !form.IsVisible;
            _currentOpenForm = form.IsVisible ? form : null;
        }

        // Eventos de submit para los botones
        private async void OnConsultarMovimientosSubmit(object sender, EventArgs e)
        {
            string cuenta = CuentaMovimientos.Text;
            if (string.IsNullOrWhiteSpace(cuenta))
            {
                await DisplayAlert("Error", "Ingrese un número de cuenta", "OK");
                return;
            }

            // Guardar cuenta en preferences para uso posterior
            Preferences.Set("cuenta_consultada", cuenta);
            await Shell.Current.GoToAsync("//MovimientosPage");
        }

        private async void OnDepositoSubmit(object sender, EventArgs e)
        {
            string cuenta = CuentaDeposito.Text;
            string importe = ImporteDeposito.Text;

            if (string.IsNullOrWhiteSpace(cuenta) || string.IsNullOrWhiteSpace(importe))
            {
                await DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            Preferences.Set("deposito_cuenta", cuenta);
            Preferences.Set("deposito_importe", importe);
            await Shell.Current.GoToAsync("//DepositoPage");
        }

        private async void OnRetiroSubmit(object sender, EventArgs e)
        {
            string cuenta = CuentaRetiro.Text;
            string importe = ImporteRetiro.Text;

            if (string.IsNullOrWhiteSpace(cuenta) || string.IsNullOrWhiteSpace(importe))
            {
                await DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            Preferences.Set("retiro_cuenta", cuenta);
            Preferences.Set("retiro_importe", importe);
            await Shell.Current.GoToAsync("//RetiroPage");
        }

        private async void OnTransferenciaSubmit(object sender, EventArgs e)
        {
            string origen = CuentaOrigen.Text;
            string destino = CuentaDestino.Text;
            string importe = ImporteTransferencia.Text;

            if (string.IsNullOrWhiteSpace(origen) || string.IsNullOrWhiteSpace(destino) || string.IsNullOrWhiteSpace(importe))
            {
                await DisplayAlert("Error", "Complete todos los campos", "OK");
                return;
            }

            Preferences.Set("transferencia_origen", origen);
            Preferences.Set("transferencia_destino", destino);
            Preferences.Set("transferencia_importe", importe);
            await Shell.Current.GoToAsync("//TransferenciaPage");
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
    }
}
