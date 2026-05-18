namespace _02.CLIMOV
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var appShell = new AppShell();
            
            // Limpiar preferencias de sesión al iniciar (para testing)
            // Comentar estas líneas si quieres mantener la sesión
            Preferences.Remove("isLoggedIn");
            Preferences.Remove("username");
            
            // Verificar si hay sesión activa
            bool isLoggedIn = Preferences.Get("isLoggedIn", false);
            
            if (isLoggedIn)
            {
                // Si hay sesión, ir a página de conversión
                appShell.CurrentItem = appShell.Items[1]; // ConversionPage
            }
            else
            {
                // Si no hay sesión, ir a login
                appShell.CurrentItem = appShell.Items[0]; // LoginPage
            }

            return new Window(appShell);
        }
    }
}