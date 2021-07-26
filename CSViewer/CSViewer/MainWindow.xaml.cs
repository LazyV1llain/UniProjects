using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Win32;
using CSVLibraryAK;
using System.Globalization;
using System.Threading;

namespace WpfApp10
{
    /// <summary>
    /// Логика взаимодействия с MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструктор окна.
        /// </summary>
        public MainWindow()
        {
            // Установка максимальных и минимальных размеров окна.
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MinHeight = SystemParameters.MaximizedPrimaryScreenHeight / 4;
            MinWidth = SystemParameters.MaximizedPrimaryScreenWidth / 4;

            SplashScreen splash = new SplashScreen("/splash-csv.png");
            splash.Show(false);
            Thread.Sleep(2000);
            splash.Close(TimeSpan.FromSeconds(2));

            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку иморта файла.
        /// </summary>
        private void ImportButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Объявление нового диалогового окна открытия файла.
                OpenFileDialog openFileDialog = new OpenFileDialog();
                // Заголовок окна.
                openFileDialog.Title = "Open File...";
                // Фильтр выбора расширений.
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";

                // Инициализация таблицы в памяти.
                DataTable datatable = new DataTable();

                if (openFileDialog.ShowDialog() == true)
                {  
                    string filePath = openFileDialog.FileName;

                    // Импорт CSV файла.  
                    datatable = CSVLibraryAK.CSVLibraryAK.Import(filePath, true);

                    // Верификация.  
                    if (datatable.Rows.Count <= 0)
                    {
                        // Вывод сообщения об ошибке.  
                        MessageBox.Show("Your file is either corrupt or does not contain any data. Make sure that you are using valid CSV file.", 
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Загрузка CSV в datagrid.  
                    grdLoad.ItemsSource = datatable.DefaultView;

                    // Отображение пути к файлу.
                    filePathLabel.Text = filePath;
                    this.Title = $"CSViewer - {filePath}";

                    // Скрытие плейсхоледра и визуализация таблицы.
                    grdLoad.Visibility = Visibility.Visible;
                    placeholderRect.Visibility = Visibility.Hidden;
                    inactiveLogo.Visibility = Visibility.Hidden;
                    inactiveLabel1.Visibility = Visibility.Hidden;
                    inactiveLabel2.Visibility = Visibility.Hidden;
                    activeLogo.Visibility = Visibility.Visible;
                }
            } catch
            {
                MessageBox.Show("Could not load the file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Обработчик события измненения 
        /// </summary>
        private void GrdLoadSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // Отображение числа выбранных ячеек.
            selectedAmountLabel.Text = $"Selected: {grdLoad.SelectedCells.Count}";

            // Если выбрана одна ячейка.
            if (grdLoad.SelectedCells.Count == 1)
            {
                // Если ячейка пуста.
                if (grdLoad.SelectedCells[0].Column.GetCellContent(grdLoad.SelectedCells[0].Item) == null) {
                    statsLabel.Text = "Value: ";
                    return;
                }

                // Если ячейка непуста.
                statsLabel.Text = $"Value: {((TextBlock)grdLoad.SelectedCells[0].Column.GetCellContent(grdLoad.SelectedCells[0].Item)).Text}";
            } 
            // Если выбраны несколько ячеек.
            else if (grdLoad.SelectedCells.Count > 1)
            {
                // Если среди выбранных ячеек есть текстовые.
                if (DataContainsText())
                {
                    statsLabel.Text = "DATA CONTAINS NONNUMERICAL VALUES";
                    return;
                }

                // Отображение статистических данных.
                statsLabel.Text = $"AVG: {DataAverageValue()}, MEDIAN: {DataMedianValue()}, VARIATION: {DataVariationValue()}, STD: {DataStandardDeviation()}";
                string columnNotification = Convert.ToBoolean(DataColumnAmount() == 1) ? "" : " (DIFFERENT COLUMNS)";
                statsLabel.Text += columnNotification;
            } 
            // Если не выбрана ни одна ячейка.
            else
            {
                selectedAmountLabel.Text = $"No cell selected";
                statsLabel.Text = "Select a cell to see statistics";
            }
        }

        /// <summary>
        /// Метод расчёта среднего арифметического содержимого выбранных ячеек.
        /// </summary>
        /// <returns>Строка с средним арифметическим.</returns>
        private string DataAverageValue()
        {
            double sum = 0;
            int count = 0;

            // Расчёт суммы и кол-ва чисел в ячейках.
            for (int i = 0; i < grdLoad.SelectedCells.Count; i++)
            {
                if (grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item) == null) continue;
                double n = 0;
                if (double.TryParse(((TextBlock)grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item)).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                {
                    sum += n;
                    count++;
                }
            }

            // Расчёт среднего арифметического.
            double avg = sum / count;
            return Math.Round(avg, 3).ToString();
        }

        /// <summary>
        /// Метод расчёта медианы содержимого выбранных ячеек.
        /// </summary>
        /// <returns>Строка с медианой.</returns>
        private string DataMedianValue()
        {
            // Список значений в выбранных ячейках.
            List<double> valList = new List<double>();

            // Добавление значений в список.
            for (int i = 0; i < grdLoad.SelectedCells.Count; i++)
            {
                if (grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item) == null) continue;
                double n = 0;
                if (double.TryParse(((TextBlock)grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item)).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                {
                    valList.Add(n);
                }
            }

            // Сортировка списка.
            double[] valArray = valList.ToArray();
            Array.Sort(valArray);

            // Выбор медианы и возвращение строки.
            if (valArray.Length != 0)
            {
                double median = valArray.Length % 2 == 1 ? valArray[valArray.Length / 2] : (valArray[valArray.Length / 2] + valArray[valArray.Length / 2 - 1]) / 2;
                return Math.Round(median, 3).ToString();
            } else return "NAN";
        }

        /// <summary>
        /// Метод расчёта числа столбцов, из которых были выбраны ячейки.
        /// </summary>
        /// <returns>Число столбцов.</returns>
        private int DataColumnAmount()
        {
            // Инициализация списка столбцов.
            List<DataGridColumn> columns = new List<DataGridColumn>();
            columns.Add(grdLoad.SelectedCells[0].Column);

            // Добавление столбцов в список.
            for (int i = 0; i < grdLoad.SelectedCells.Count; i++)
            {
                if (!columns.Contains(grdLoad.SelectedCells[i].Column))
                {
                    columns.Add(grdLoad.SelectedCells[i].Column);
                }
            }

            // Возврат числа столбцов.
            return columns.Count;
        }

        /// <summary>
        /// Метод определения того, содержится ли в выбранных ячейках нечисловая операция.
        /// </summary>
        /// <returns>Булево значение - истина, если содержится.</returns>
        private bool DataContainsText()
        {
            bool containsText = false;

            // Определение того, есть ли среди ячеек те, чьё содержимое нельзя конвертировать в вещественное число.
            for (int i = 0; i < grdLoad.SelectedCells.Count; i++)
            {
                if (grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item) == null) continue;
                double n = 0;
                if (!double.TryParse(((TextBlock)grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item)).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                {
                    containsText = true;
                    break;
                }
            }

            // Возврат булева значения.
            return containsText;
        }

        /// <summary>
        /// Метод расчёта дисперсии содержимого выбранных ячеек.
        /// </summary>
        /// <returns>Строка с дисперсией.</returns>
        private string DataVariationValue()
        {
            // Получение среднего арфиметического.
            double avg = double.Parse(DataAverageValue());

            // Список с значениями отклонения значения в ячейке от среднего.
            List<double> deviationArr = new List<double>();

            // Заполнение списка.
            for (int i = 0; i < grdLoad.SelectedCells.Count; i++)
            {
                if (grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item) == null) continue;
                double n = 0;
                if (double.TryParse(((TextBlock)grdLoad.SelectedCells[i].Column.GetCellContent(grdLoad.SelectedCells[i].Item)).Text, NumberStyles.Any, CultureInfo.InvariantCulture, out n))
                {
                    deviationArr.Add(n - avg);
                }
            }

            // Получение суммы квадратов отклонений и расчёт дисперсии.
            double sum = 0;
            foreach (var dev in deviationArr) sum += dev * dev;
            if (deviationArr.Count != 0) return Math.Round((sum / deviationArr.Count), 3).ToString();
            else return "NAN";
        }

        /// <summary>
        /// Метод расчёта стандартного отклонения содержимого выбранных ячеек.
        /// </summary>
        /// <returns>Строка с стандартным отклонением.</returns>
        private string DataStandardDeviation()
        {
            // Возврат квадратного корня из дисперсии.
            string variation = DataVariationValue();
            if (variation == "NAN") return "NAN";
            else return Math.Round(Math.Sqrt(double.Parse(variation)), 3).ToString();
        }

        /// <summary>
        /// Метод выбора всех ячеек столбца при нажатии на его заголовок.
        /// </summary>
        private void ColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            // Получение объекта заголовка.
            var columnHeader = sender as System.Windows.Controls.Primitives.DataGridColumnHeader;

            if (columnHeader != null)
            {
                // Временная отписка события от метода обработки изменения выбора ячеек (для избежания зависаний).
                grdLoad.SelectedCellsChanged -= GrdLoadSelectedCellsChanged;

                // Выбор ячеек.
                grdLoad.SelectedCells.Clear();
                foreach (var item in grdLoad.Items)
                {
                    if (item != null) grdLoad.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
                }

                // Переподписка события.
                grdLoad.SelectedCellsChanged += GrdLoadSelectedCellsChanged;

                // Вызов метода обработки изменения выбора ячеек.
                GrdLoadSelectedCellsChanged(grdLoad, new SelectedCellsChangedEventArgs(new List<DataGridCellInfo>(), new List<DataGridCellInfo>()));
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

        /// <summary>
        /// Обработчик события нажатия по кнопке закрытия окна.
        /// </summary>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Обработчик события нажатия по кнопке расширения окна.
        /// </summary>
        private void MaximizeButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
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
        /// Обработчик измненения размера окна.
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
        /// Обработчик нажатия на кнопку построения графика.
        /// </summary>
        private void ChartButtonClick(object sender, RoutedEventArgs e) 
        {
            // Определение вида графика.
            if (grdLoad.SelectedCells.Count > 0 && DataColumnAmount() <= 2)
            {
                ChartWindow newChartWindow = null;

                // Построение гистограммы, если выбраны ячейки в одном столбце.
                if (DataColumnAmount() == 1)
                {
                    // Построение числовой гистограммы (с интервалами) для числовых значений.
                    if (!DataContainsText()) newChartWindow = new ChartWindow(grdLoad.SelectedCells, "HIST_N");
                    // Построение текстовой гистограммы (без интервалов), если среди значений есть текст.
                    else newChartWindow = new ChartWindow(grdLoad.SelectedCells, "HIST_T");
                }
                // Построение графика зависимости, если выбраны ячейки в двух столбцах.
                else if (DataColumnAmount() == 2) {
                    // Построение scatter/line графика для числовых значений.
                    if (!DataContainsText()) newChartWindow = new ChartWindow(grdLoad.SelectedCells, "SCAT");
                    // Построение гистограммы для одного из столбцов, если среди значений есть текст.
                    else newChartWindow = new ChartWindow(grdLoad.SelectedCells, "HIST_T");
                }
                newChartWindow.Show();
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку вызова окна помощи.
        /// </summary>
        private void HelpButtonClick(object sender, MouseButtonEventArgs e)
        {
            HelpWindow help = new HelpWindow();
            help.Show();
        }
    }
}
