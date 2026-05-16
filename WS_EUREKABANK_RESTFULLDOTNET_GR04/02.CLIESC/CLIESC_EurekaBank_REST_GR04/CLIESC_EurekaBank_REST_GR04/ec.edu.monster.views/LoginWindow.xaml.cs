using System;
using System.Windows;

namespace ec.edu.monster.views
{
    public partial class LoginWindow : Window
    {
        public event Action<string, string> LoginClicked;

        public LoginWindow()
        {
            InitializeComponent();
            BtnLogin.Click += (s, e) => LoginClicked?.Invoke(TxtUser.Text.Trim(), TxtPass.Password);
            TxtUser.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Enter) BtnLogin.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent)); };
            TxtPass.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Enter) BtnLogin.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent)); };
        }

        public void SetBusy(bool busy) => BtnLogin.IsEnabled = !busy;

        public void ShowError(string msg) => MessageBox.Show(this, msg, "Error en el ingreso",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
