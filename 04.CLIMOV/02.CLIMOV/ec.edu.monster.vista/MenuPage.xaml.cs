using System;
using Microsoft.Maui.Controls;

namespace _02.CLIMOV.Vista
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            
            // Cargar nombre de usuario
            string username = Preferences.Get("username", "Usuario");
            LblUsuario.Text = username;
        }

        private async void OnConsultarMovimientosClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MovimientosPage");
        }

        private async void OnDepositoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//DepositoPage");
        }

        private async void OnRetiroClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//RetiroPage");
        }

        private async void OnTransferenciaClicked(object sender, EventArgs e)
        {
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
