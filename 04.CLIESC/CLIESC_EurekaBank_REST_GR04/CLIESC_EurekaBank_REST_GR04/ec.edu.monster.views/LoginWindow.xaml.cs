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
        }

        public void SetBusy(bool busy) => BtnLogin.IsEnabled = !busy;

        public void ShowError(string msg)
        {
            ErrorMsg.Text = msg;
            ErrorMsg.Visibility = Visibility.Visible;
        }
    }
}