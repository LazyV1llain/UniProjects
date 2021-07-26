using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp10
{
    /// <summary>
    /// Логика взаимодействия для HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события измненения размеров окна.
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                BorderThickness = new Thickness(7);
            }
            else
            {
                BorderThickness = new Thickness(0);
            }
        }

        /// <summary>
        /// Обработчик события нажатия по кнопке закрытия окна.
        /// </summary>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Обработчик события нажатия по кнопке сворачивания окна.
        /// </summary>
        private void MinimizeButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// Обработчик события зажатия ЛКМ на верхней области окна.
        /// </summary>
        private void HeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Перемещение окна.
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
