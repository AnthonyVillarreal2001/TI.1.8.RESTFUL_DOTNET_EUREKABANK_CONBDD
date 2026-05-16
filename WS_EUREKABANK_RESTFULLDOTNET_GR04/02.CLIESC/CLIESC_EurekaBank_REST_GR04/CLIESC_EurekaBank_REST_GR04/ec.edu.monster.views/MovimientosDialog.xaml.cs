using System.Collections.Generic;
using System.Windows;
using ec.edu.monster.models;

namespace ec.edu.monster.views
{
    public partial class MovimientosDialog : Window
    {
        public MovimientosDialog(string cuenta, IList<Movimiento> items)
        {
            InitializeComponent();
            LblCuenta.Text = cuenta;
            GridMovs.ItemsSource = items;
        }
    }
}
