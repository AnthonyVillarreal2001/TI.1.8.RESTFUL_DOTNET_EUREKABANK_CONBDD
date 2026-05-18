using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ec.edu.monster.models;

namespace ec.edu.monster.views
{
    public partial class MovimientosDialog : Window
    {
        public MovimientosDialog(string cuenta, IList<Movimiento> items)
        {
            InitializeComponent();
            CuentaText.Text = cuenta;
            foreach (var m in items)
            {
                m.TipoColor = m.Tipo switch
                {
                    "DEPOSITO" => new SolidColorBrush(Color.FromRgb(46, 204, 113)),
                    "RETIRO" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                    "TRANSFERENCIA" => new SolidColorBrush(Color.FromRgb(0, 168, 255)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            MovimientosGrid.ItemsSource = items;
        }
    }
}