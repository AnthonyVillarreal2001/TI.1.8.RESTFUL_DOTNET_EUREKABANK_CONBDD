using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace ec.edu.monster.views
{
    public partial class HomeWindow : Window
    {
        public event Action<string> ConsultarMov;
        public event Action<string, string> Depositar;
        public event Action<string, string> Retirar;
        public event Action<string, string, string> Transferir;

        private readonly List<CardControl> _cards = new();

        public HomeWindow()
        {
            InitializeComponent();
            CreateCards();
            BtnLogout.Click += (s, e) => Close();
        }

        private void CreateCards()
        {
            var cardData = new[]
            {
                new { Title = "Consultar Movimientos",
                      Icon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM9 17H7v-7h2v7zm4 0h-2V7h2v10zm4 0h-2v-4h2v4z",
                      Fields = new[] { new Field { Label = "Número de cuenta", Name = "cuenta" } },
                      ButtonText = "Ver movimientos", ActionType = "consultar" },
                new { Title = "Depósito",
                      Icon = "M11 17h2v-1h1c.55 0 1-.45 1-1v-3c0-.55-.45-1-1-1h-3v-1h4V8h-2V7h-2v1h-1c-.55 0-1 .45-1 1v3c0 .55.45 1 1 1h3v1H9v2h2v1zm9-13H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4V6h16v12z",
                      Fields = new[] { new Field { Label = "Cuenta destino", Name = "cuenta" }, new Field { Label = "Importe (USD)", Name = "importe" } },
                      ButtonText = "Depositar", ActionType = "deposito" },
                new { Title = "Retiro",
                      Icon = "M21 7.28V5c0-1.1-.9-2-2-2H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2v-2.28c.59-.35 1-.98 1-1.72V9c0-.74-.41-1.37-1-1.72zM20 9v6h-7V9h7zM5 19V5h14v2h-6c-1.1 0-2 .9-2 2v6c0 1.1.9 2 2 2h6v2H5z",
                      Fields = new[] { new Field { Label = "Cuenta origen", Name = "cuenta" }, new Field { Label = "Importe (USD)", Name = "importe" } },
                      ButtonText = "Retirar", ActionType = "retiro" },
                new { Title = "Transferencia",
                      Icon = "M20 4H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V6c0-1.11-.89-2-2-2zm0 14H4v-6h16v6zm0-10H4V6h16v2z",
                      Fields = new[] { new Field { Label = "Cuenta origen", Name = "origen" }, new Field { Label = "Cuenta destino", Name = "destino" }, new Field { Label = "Importe (USD)", Name = "importe" } },
                      ButtonText = "Transferir", ActionType = "transferencia" }
            };

            foreach (var data in cardData)
            {
                var card = new CardControl(data.Title, data.Icon, data.Fields, data.ButtonText);
                card.ButtonClick += (fields) =>
                {
                    switch (data.ActionType)
                    {
                        case "consultar":
                            if (fields.TryGetValue("cuenta", out var cta)) ConsultarMov?.Invoke(cta);
                            break;
                        case "deposito":
                            if (fields.TryGetValue("cuenta", out var ctaDep) && fields.TryGetValue("importe", out var impDep))
                                Depositar?.Invoke(ctaDep, impDep);
                            break;
                        case "retiro":
                            if (fields.TryGetValue("cuenta", out var ctaRet) && fields.TryGetValue("importe", out var impRet))
                                Retirar?.Invoke(ctaRet, impRet);
                            break;
                        case "transferencia":
                            if (fields.TryGetValue("origen", out var orig) && fields.TryGetValue("destino", out var dest) && fields.TryGetValue("importe", out var impTrf))
                                Transferir?.Invoke(orig, dest, impTrf);
                            break;
                    }
                };
                _cards.Add(card);
                CardsPanel.Children.Add(card);
            }

            foreach (var card in _cards)
                card.CardClicked += (sender) => { foreach (var c in _cards) if (c != sender) c.CloseForm(); };

            this.PreviewMouseDown += (s, e) =>
            {
                if (!(e.OriginalSource is DependencyObject dep && FindParent<CardControl>(dep) != null))
                    foreach (var c in _cards) c.CloseForm();
            };
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T)) child = VisualTreeHelper.GetParent(child);
            return child as T;
        }

        public void SetUser(string name) => UserBadge.Text = name;
        public void SetBusy(bool busy) => this.IsEnabled = !busy;
        public void ToastOK(string msg) => new ResultDialog(true, msg).ShowDialog();
        public void ToastError(string msg) => new ResultDialog(false, msg).ShowDialog();
    }

    public class CardControl : Border
    {
        private readonly StackPanel _formPanel;
        private readonly Dictionary<string, TextBox> _textBoxes = new();
        private bool _isOpen;

        public event Action<CardControl> CardClicked;
        public event Action<Dictionary<string, string>> ButtonClick;

        public CardControl(string title, string iconPath, Field[] fields, string buttonText)
        {
            this.Width = 320;
            this.Margin = new Thickness(10);
            this.CornerRadius = new CornerRadius(20);
            this.Background = Brushes.White;
            this.Effect = new DropShadowEffect { BlurRadius = 15, ShadowDepth = 3, Opacity = 0.2 };
            this.Cursor = Cursors.Hand;

            var mainStack = new StackPanel();

            // Icono
            var iconBorder = new Border
            {
                Background = new LinearGradientBrush(Colors.DarkBlue, Colors.Navy, 90),
                CornerRadius = new CornerRadius(20, 20, 0, 0)
            };
            var icon = new Path { Data = Geometry.Parse(iconPath), Fill = Brushes.White, Width = 60, Height = 60, Stretch = Stretch.Uniform };
            iconBorder.Child = icon;
            iconBorder.Padding = new Thickness(20);
            mainStack.Children.Add(iconBorder);

            // Título
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(12, 45, 72)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 10)
            };
            mainStack.Children.Add(titleBlock);

            // Formulario (colapsable)
            _formPanel = new StackPanel { Visibility = Visibility.Collapsed, Margin = new Thickness(0, 5, 0, 10) };

            foreach (var f in fields)
            {
                // Etiqueta
                var label = new TextBlock
                {
                    Text = f.Label,
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(10, 61, 98)),
                    Margin = new Thickness(0, 8, 0, 4)
                };
                _formPanel.Children.Add(label);

                // TextBox con estilo explícito para garantizar visibilidad
                var txt = new TextBox
                {
                    Tag = f.Name,
                    Height = 42,
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(12, 8, 12, 8),
                    Background = Brushes.White,
                    Foreground = Brushes.Black,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                    BorderThickness = new Thickness(1),
                    FontSize = 13,
                    FontFamily = new FontFamily("Segoe UI"),
                    CaretBrush = Brushes.Black
                };
                // Aplicar esquinas redondeadas directamente
                var border = new Border { CornerRadius = new CornerRadius(20), Child = txt };
                _formPanel.Children.Add(border);
                _textBoxes[f.Name] = txt;
            }

            // Botón
            var btn = new Button
            {
                Content = buttonText,
                Height = 40,
                Margin = new Thickness(0, 5, 0, 5),
                Background = new SolidColorBrush(Color.FromRgb(230, 126, 34)),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Cursor = Cursors.Hand,
                BorderThickness = new Thickness(0)
            };
            btn.Click += (s, e) =>
            {
                var values = new Dictionary<string, string>();
                foreach (var kv in _textBoxes) values[kv.Key] = kv.Value.Text.Trim();
                ButtonClick?.Invoke(values);
            };
            var btnBorder = new Border { CornerRadius = new CornerRadius(20), Background = btn.Background, Child = btn };
            _formPanel.Children.Add(btnBorder);

            mainStack.Children.Add(_formPanel);
            this.Child = mainStack;

            // Evento para abrir/cerrar solo si no se hizo clic en un control interno
            this.MouseLeftButtonUp += (s, e) =>
            {
                var originalSource = e.OriginalSource as DependencyObject;
                if (originalSource != null &&
                    (FindParent<TextBox>(originalSource) != null ||
                     FindParent<PasswordBox>(originalSource) != null ||
                     FindParent<Button>(originalSource) != null))
                {
                    return;
                }
                if (_isOpen) CloseForm(); else OpenForm();
                CardClicked?.Invoke(this);
                e.Handled = true;
            };
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T)) child = VisualTreeHelper.GetParent(child);
            return child as T;
        }

        public void OpenForm() { _formPanel.Visibility = Visibility.Visible; _isOpen = true; }
        public void CloseForm() { _formPanel.Visibility = Visibility.Collapsed; _isOpen = false; }
    }

    public class Field
    {
        public string Label { get; set; }
        public string Name { get; set; }
    }
}