using ec.edu.monster.controllers;
using ec.edu.monster.services;
using ec.edu.monster.views;
using System.Windows;

namespace EurekaBank.WpfClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var api = new RestEurekaApi("https://localhost:7043/");
            var login = new LoginWindow();
            var ctrl = new LoginController(login, api);
            MainWindow = login;
            login.Show();
        }
    }
}
