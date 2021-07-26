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
using System.Data;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using System.Linq;
using System.Windows.Forms;
using ColorPickerWPF;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace WpfApp10
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        /// <summary>
        /// Коллекция значений для графика.
        /// </summary>
        public SeriesCollection Collection { get; set; }

        /// <summary>
        /// Массив лейблов для графика.
        /// </summary>
        public string[] Labels { get; set; }

        /// <summary>
        /// Форматтер для графика.
        /// </summary>
        public Func<double, string> Formatter { get; set; }

        /// <summary>
        /// Выбранная и переданная в окно информация из таблицы.
        /// </summary>
        private IList<DataGridCellInfo> Data { get; set; }

        /// <summary>
        /// Столбцы гистограммы.
        /// </summary>
        private ColumnSeries Columns { get; set; }

        /// <summary>
        /// Словарь окрашенных столбцов гистограммы.
        /// </summary>
        private Dictionary<double, SolidColorBrush> coloredColumns { get; set; }

        /// <summary>
        /// Конструктор окна.
        /// </summary>
        /// <param name="data">Выбранная и переданная в окно информация из таблицы.</param>
        /// <param name="chartType">Тип графика.</param>
        public ChartWindow(IList<DataGridCellInfo> data, string chartType)
        {
            // Установка ограничений на размер окна.
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MinHeight = SystemParameters.MaximizedPrimaryScreenHeight / 4;
            MinWidth = SystemParameters.MaximizedPrimaryScreenWidth / 4;

            // Сохранение табличных данных.
            Data = data;

            InitializeComponent();

            // Отрисовка графика.
            switch (chartType)
            {
                case "HIST_N":
                    BuildNumericalHistogram(data, false);
                    break;
                case "HIST_T":
                    BuildNonnumericalHistogram(data);
                    break;
                case "SCAT":
                    BuildXYChart(data);
                    break;
            }
        }

        /// <summary>
        /// Метод обработки события нажатия на график (гистограмму).
        /// </summary>
        private void ChartDataClick(object sender, ChartPoint chartPoint)
        {
            System.Windows.Media.Color color;

            // Выбор цвета.
            if (ColorPickerWindow.ShowDialog(out color))
            {
                // Регистрация изменения цвета в словаре.
                if (!coloredColumns.ContainsKey(chartPoint.X)) coloredColumns.Add(chartPoint.X, new SolidColorBrush(color));
                else coloredColumns[chartPoint.X] = new SolidColorBrush(color);

                // Обновление маппера графика и самого графика.
                var mapper = new CartesianMapper<int>().X((value, index) => index).Y((value) => value)
                    .Fill((v, i) => coloredColumns.ContainsKey(i) ? coloredColumns[i] : new SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 149, 242)));
                Columns.Configuration = mapper;
            }
        }

        /// <summary>
        /// Метод отрисовки гистограммы для нечисловых значений.
        /// </summary>
        /// <param name="data">Выбранная и переданная в окно информация из таблицы.</param>
        private void BuildNonnumericalHistogram(IList<DataGridCellInfo> data)
        {
            // Подписка делегата события нажатия на график на обработчик.
            chart.DataClick += ChartDataClick;

            // Инициализация коллекции значений.
            Collection = new SeriesCollection();

            // Инициализация словаря значений и их количества в data.
            Dictionary<string, int> values = new Dictionary<string, int>();

            // Заполнение словаря.
            for (int i = 0; i < data.Count; i++)
            {
                string currentData = ((DataRowView)data[i].Item).Row.ItemArray[data[0].Column.DisplayIndex].ToString();
                if (!values.ContainsKey(currentData)) values.Add(currentData, 0);
                values[currentData]++;
            }

            // Получение лейблов из словаря.
            Labels = Array.ConvertAll(values.Keys.ToArray(), x => x.ToString());

            // Инициализация коллекции столбцов гистограммы.
            ColumnSeries columnSeries = new ColumnSeries
            {
                Title = data[0].Column.Header.ToString(),
                Values = new ChartValues<int>(values.Values)
            };
            Columns = columnSeries;
            Collection.Add(columnSeries);

            // Инициализация форматтера.
            Formatter = value => value.ToString();

            // Обновление лейбла окна.
            chartWindowLabel.Content += data[0].Column.Header.ToString().ToUpper();
            Title = chartWindowLabel.Content.ToString();

            DataContext = this;
        }

        /// <summary>
        /// Метод отрисовки гистограммы для нечисловых значений.
        /// </summary>
        /// <param name="data">Выбранная и переданная в окно информация из таблицы.</param>
        /// <param name="redraw">Булево значение - истина, если график отрисовывается не в первый раз.</param>
        private void BuildNumericalHistogram(IList<DataGridCellInfo> data, bool redraw)
        {
            // Инициализация списка окрашенных столбцов.
            coloredColumns = new Dictionary<double, SolidColorBrush>();

            // Нахождение максимального и минимального значения из выбранных ячеек.
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < data.Count; i++)
            {
                double currentData = double.Parse(((DataRowView)data[i].Item).Row.ItemArray[data[0].Column.DisplayIndex].ToString(), CultureInfo.InvariantCulture);

                if (currentData < min) min = currentData;
                if (currentData > max) max = currentData;
            }

            // Активация элементов управления интервалами.
            upDownControl.Visibility = Visibility.Visible;
            intervalLabel.Visibility = Visibility.Visible;

            // Инициализация элементов при первом построении графика.
            if (!redraw)
            {
                upDownControl.Value = (int)((max - min) / 20);
                upDownControl.ValueChanged += UpDownControlValueChanged;
                chart.DataClick += ChartDataClick;
            }

            // Инициализация коллекции элементов графика.
            Collection = new SeriesCollection();

            // Словарь пар 'значение из интервала между минимумом и максимумом - количество ячеек с этим интервалом, соответствующим этому значению'.
            Dictionary<int, int> values = new Dictionary<int, int>();

            // Добавление ключей.
            for (int i = (int)min; i < (int)max; i = i + (int)upDownControl.Value) values.Add(i, 0);

            // Заполнение словаря.
            for (int i = 0; i < data.Count; i++)
            {
                double currentData = double.Parse(((DataRowView)data[i].Item).Row.ItemArray[data[0].Column.DisplayIndex].ToString(), CultureInfo.InvariantCulture);

                for (int index = 0; index < values.Count; index++)
                {
                    var item = values.ElementAt(index);
                    var itemKey = item.Key;

                    // Определение того, какому из интервалов принадлежит значение.
                    if (currentData < itemKey + ((double)((int)upDownControl.Value)) / 2 && currentData >= itemKey - ((double)((int)upDownControl.Value)) / 2) values[itemKey]++;
                }
            }

            // Инициализация лейблов столбцов гистограммы.
            Labels = Array.ConvertAll(values.Keys.ToArray(), x => $"[{x - ((double)((int)upDownControl.Value)) / 2};{x + ((double)((int)upDownControl.Value)) / 2})");

            // Инициализация коллекции столбцев гистограммы.
            ColumnSeries columnSeries = new ColumnSeries
            {
                Title = data[0].Column.Header.ToString(),
                Values = new ChartValues<int>(values.Values)
            };
            Columns = columnSeries;
            if (redraw) Collection.Clear();
            Collection.Add(columnSeries);

            // Инициализация форматтера.
            Formatter = value => value.ToString();

            if (!redraw)
            {
                chartWindowLabel.Content += data[0].Column.Header.ToString().ToUpper();
                Title = chartWindowLabel.Content.ToString();
            }
            if (redraw) chart.Update(true, false);

            DataContext = Labels;
            DataContext = this;
        }

        /// <summary>
        /// Метод отрисовки графика
        /// </summary>
        /// <param name="data">Выбранная и переданная в окно информация из таблицы.</param>
        private void BuildXYChart(IList<DataGridCellInfo> data)
        {
            // Словарь столбцов и списков ячеек в них.
            Dictionary<DataGridColumn, List<DataGridCellInfo>> columnsDict = new Dictionary<DataGridColumn, List<DataGridCellInfo>>();

            // Заполнение словаря.
            for (int i = 0; i < data.Count; i++)
            {
                if (!columnsDict.ContainsKey(data[i].Column)) columnsDict.Add(data[i].Column, new List<DataGridCellInfo>());
                columnsDict[data[i].Column].Add(data[i]);
            }

            // Буль наличия дубликатов в столбце по оси X.
            bool containsDuplicates = false;

            // Словарь 'значение в столбце 1 - значение в столбце 2'.
            Dictionary<double, double> pairDict = new Dictionary<double, double>();
            // Список пар значений - на случай наличия дубликатов.
            List<double[]> pairList = new List<double[]>();

            // Заполнение списка или словаря.
            for (int i = 0; i < columnsDict[columnsDict.First().Key].Count; i++)
            {
                string content1 = ((DataRowView)columnsDict[columnsDict.First().Key][i].Item).Row.ItemArray[columnsDict[columnsDict.First().Key][i].Column.DisplayIndex].ToString();
                string content2 = ((DataRowView)columnsDict[columnsDict.Last().Key][i].Item).Row.ItemArray[columnsDict[columnsDict.Last().Key][i].Column.DisplayIndex].ToString();

                double num1 = double.Parse(content1, CultureInfo.InvariantCulture);
                double num2 = double.Parse(content2, CultureInfo.InvariantCulture);

                // Если обнаружен дубликат - переключение на список.
                if (pairDict.ContainsKey(num1) && !containsDuplicates)
                {
                    containsDuplicates = true;
                    foreach (var pair in pairDict) pairList.Add(new double[] { pair.Key, pair.Value });
                }

                if (containsDuplicates) pairList.Add(new double[] { num1, num2 });
                else pairDict.Add(num1, num2);
            }

            // Если дубликаты присутствуют - построение scatter-графика.
            if (containsDuplicates)
            {
                // Получение массива точек на scatter-графике.
                ChartValues<ScatterPoint> valList = new ChartValues<ScatterPoint>();
                foreach (var pair in pairList) valList.Add(new ScatterPoint(pair[0], pair[1], 1));

                // Инициализация scatter-коллекции.
                Collection = new SeriesCollection
                {
                new ScatterSeries
                {
                    Values = valList,
                    MinPointShapeDiameter = 10,
                    MaxPointShapeDiameter = 10
                }
                };
            }
            // Если дубликаты отстутствуют - построение line-графика.
            else
            {
                // Сортировка словаря по значению ключа.
                pairDict = pairDict.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);

                // Получение массива точек на line-графике.
                ChartValues<double> valList = new ChartValues<double>();
                foreach (var pair in pairDict) valList.Add(pair.Value);

                // Инициализация списка лейблов на графике.
                List<string> labels = new List<string>();
                foreach (var pair in pairDict) labels.Add(pair.Key.ToString());

                // Инициализация line-коллекции.
                Collection = new SeriesCollection
                {
                new LineSeries
                {
                    Title = "Series 1",
                    Values = valList
                }
                };

                Labels = labels.ToArray();
            }

            // Инициализация подписей осей.
            chart.AxisX[0].Title = $"{columnsDict.First().Key.Header}";
            chart.AxisY[0].Title = $"{columnsDict.Last().Key.Header}";

            // Обновление заголовка окна.
            if (containsDuplicates) chartWindowLabel.Content = $"SCATTER PLOT - {columnsDict.First().Key.Header.ToString().ToUpper()} & {columnsDict.Last().Key.Header.ToString().ToUpper()}";
            else chartWindowLabel.Content = $"LINE PLOT - {columnsDict.First().Key.Header.ToString().ToUpper()} & {columnsDict.Last().Key.Header.ToString().ToUpper()}";
            Title = chartWindowLabel.Content.ToString();

            DataContext = this;
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
        /// Обработчик события зажатия ЛКМ на верхней области окна.
        /// </summary>
        private void HeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Перемещение окна.
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        /// <summary>
        /// Обработчик события изменения интервалов на гистограмме.
        /// </summary>
        private void UpDownControlValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Перерисовка графика.
            BuildNumericalHistogram(Data, true);
        }

        /// <summary>
        /// Метод сохранения графика в виде изображения.
        /// </summary>
        private void SaveAsImageButtonClick(object sender, RoutedEventArgs e)
        {
            // Получение изображения из окна.
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)chart.ActualWidth, (int)chart.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(chart);
            PngBitmapEncoder png = new PngBitmapEncoder();
            png.Frames.Add(BitmapFrame.Create(rtb));
            MemoryStream stream = new MemoryStream();
            png.Save(stream);
            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);

            // Выбор директории и сохранение изображения.
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.Description = "Select the folder to save the file to";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    img.Save(folderBrowserDialog.SelectedPath + System.IO.Path.DirectorySeparatorChar + "plot.png");
                } catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

        }
    }
}
