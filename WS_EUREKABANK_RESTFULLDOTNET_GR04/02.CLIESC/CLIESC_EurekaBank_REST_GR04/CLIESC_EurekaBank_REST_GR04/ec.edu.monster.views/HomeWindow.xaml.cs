using System;
using System.Windows;

namespace ec.edu.monster.views
{
    public partial class HomeWindow : Window
    {
        public event Action<string> ConsultarMov;
        public event Action<string, string> Depositar;
        public event Action<string, string> Retirar;
        public event Action<string, string, string> Transferir;

        public HomeWindow()
        {
            InitializeComponent();
            BtnMov.Click += (s, e) => ConsultarMov?.Invoke(TxtMovCuenta.Text.Trim());
            BtnDep.Click += (s, e) => Depositar?.Invoke(TxtDepCuenta.Text.Trim(), TxtDepImporte.Text.Trim());
            BtnRet.Click += (s, e) => Retirar?.Invoke(TxtRetCuenta.Text.Trim(), TxtRetImporte.Text.Trim());
            BtnTrf.Click += (s, e) => Transferir?.Invoke(TxtTrfOrigen.Text.Trim(), TxtTrfDestino.Text.Trim(), TxtTrfImporte.Text.Trim());
            BtnLogout.Click += (s, e) => Close();
        }

        public void SetUser(string name) => BtnUser.Content = name;

        public void SetBusy(bool busy)
        {
            IsEnabled = !busy;
        }

        public void ToastOK(string msg) =>
            MessageBox.Show(this, msg, "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);

        public void ToastError(string msg) =>
            MessageBox.Show(this, msg, "Error en la operación", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
