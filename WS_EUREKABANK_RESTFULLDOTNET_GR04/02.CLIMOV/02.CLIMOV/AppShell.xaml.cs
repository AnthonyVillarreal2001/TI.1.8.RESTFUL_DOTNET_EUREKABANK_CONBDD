using _02.CLIMOV.Vista;

namespace _02.CLIMOV
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("MenuPage", typeof(MenuPage));
            Routing.RegisterRoute("MovimientosPage", typeof(MovimientosPage));
            Routing.RegisterRoute("DepositoPage", typeof(DepositoPage));
            Routing.RegisterRoute("RetiroPage", typeof(RetiroPage));
            Routing.RegisterRoute("TransferenciaPage", typeof(TransferenciaPage));
        }
    }
}
