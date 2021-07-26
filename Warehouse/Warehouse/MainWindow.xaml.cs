using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using System.Windows.Media.Animation;
using System.Xml.Serialization;
using Microsoft.Win32;
using CsvHelper;
using System.Globalization;
using System.Windows.Threading;
using System.Windows.Media;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        static Random random = new Random();

        /// <summary>
        /// Флаг активности экрана создания раздела\товара.
        /// </summary>
        bool creationScreenIsActive = false;
        /// <summary>
        /// Флаг добалвения подраздела в раздел.
        /// </summary>
        bool addingSubsection = false;
        /// <summary>
        /// Флаг режима редактирования раздела\товара.
        /// </summary>
        bool editingMode = false;
        /// <summary>
        /// Таймер начала анимации.
        /// </summary>
        DispatcherTimer timerAnim = new DispatcherTimer();
        /// <summary>
        /// Таймер автосохранения.
        /// </summary>
        DispatcherTimer timerSave = new DispatcherTimer();
        /// <summary>
        /// Товар-субъект (цель) редактирования.
        /// </summary>
        Product editingSubject;

        /// <summary>
        /// Флаги заполненности полей в экране создания продукта.
        /// </summary>
        bool[] productFieldsFilled = new bool[] { false, false, false, false, false };
        /// <summary>
        /// Массив артикулов товаров на складе.
        /// </summary>
        private List<string> codes = new List<string>();
        /// <summary>
        /// Коллекция разделов склада - данные для mainTree.
        /// </summary>
        private ObservableCollection<Section> sections = new ObservableCollection<Section>();
        /// <summary>
        /// Коллекция отображаемых товаров - данные для dataGrid.
        /// </summary>
        private ObservableCollection<Product> products = new ObservableCollection<Product>();

        /// <summary>
        /// Конструктор окна.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Установка источников данных.
            mainTree.ItemsSource = sections;
            dataGrid.ItemsSource = products;

            // Установка минимального размера окна.
            MinHeight = SystemParameters.PrimaryScreenHeight / 4;
            MinWidth = SystemParameters.PrimaryScreenWidth / 4;

            // Запуск таймера начала анимации.
            timerAnim.Tick += new EventHandler(timerAnim_Tick);
            timerAnim.Interval = TimeSpan.FromSeconds(1.5);
            timerAnim.Start();

            // Запуск таймера автосохранения.
            timerSave.Tick += new EventHandler(timerSave_Tick);
            timerSave.Interval = TimeSpan.FromMinutes(3);
            timerSave.Start();
        }

        #region Общие методы работы с разделами.
        /// <summary>
        /// Метод инициации удаления раздела.
        /// </summary>
        /// <param name="target">Удаляемый раздел.</param>
        private void DeleteSection(Section target)
        {
            // Проверка на то, является ли удаляемый раздел непустым.
            if (target.Products.Count != 0 || target.Subsections.Count != 0)
            {
                // Вывод сообщения об ошибке при удалении непустого раздела.
                ToggleErrorScreen(true, "The section is not empty and therefore cannot be deleted!");
            }
            else
            {
                // Вывод экрана подтверждения удаления раздела.
                ToggleConfirmationScreen(true);
            }
        }

        /// <summary>
        /// Метод добавления товаров из раздела в список товаров для dataGrid.
        /// </summary>
        /// <param name="section">Раздел с товарами.</param>
        private void AddProductsToList(Section section)
        {
            // Итерирование по товарам раздела.
            foreach (var product in section.Products)
            {
                // Определение, является ли товар товаром подраздела выбранного раздела (для отображения метки в dataGrid).
                if (section != (mainTree.SelectedItem as Section)) product.IsInSubsection = true;
                else product.IsInSubsection = false;

                // Добавление товара в список.
                products.Add(product);
            }

            // Рекурсивный вызов метода для подразделов раздела.
            foreach (var subsection in section.Subsections)
            {
                AddProductsToList(subsection);
            }
        }
        #endregion

        #region Обработка событий в TreeView и Datagrid.

        #region Обработка событий в TreeView и его контекстном меню.
        /// <summary>
        /// Метод обработки события изменения выбора раздела в mainTree.
        /// </summary>
        private void mainTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Отображение dataGrid'a в случае, если он скрыт.
            if (dataGrid.Visibility == Visibility.Hidden)
            {
                dataGrid.Visibility = Visibility.Visible;
                DataGridOpacityInAnimation();
            }

            // Обновление содержания dataGrid'a.
            UpdateDataGrid();
        }

        /// <summary>
        /// Метод обработки нажатия кнопки Add Subsection в контекстном меню TreeView.
        /// </summary>
        private void AddSubsectionContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Закрепление фокуса на выбранном элементе.
            (sender as MenuItem).Focus();

            // Проверка на случай, если не выбран никакой раздел.
            if (mainTree.SelectedItem == null)
            {
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса добавления подраздела.
                addingSubsection = true;
                mainTree.IsEnabled = false;
                ToggleSectionNamingScreen(true);
            }
        }

        /// <summary>
        /// Метод обработки нажатия кнопки Edit в контекстном меню TreeView.
        /// </summary>
        private void EditSectionContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Закрепление фокуса на выбранном элементе.
            (sender as MenuItem).Focus();

            // Инициация процесса изменения раздела.
            editingMode = true;
            mainTree.IsEnabled = false;
            ToggleSectionNamingScreen(true);
        }

        /// <summary>
        /// Метод обработки нажатия кнопки Delete в контекстном меню TreeView.
        /// </summary>
        private void DeleteSectionContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Определение удаляемого раздела.
            Section target = ((sender as MenuItem).DataContext) as Section;

            // Инициация процесса удаления раздела.
            if (target != null) DeleteSection(target);
        }

        /// <summary>
        /// Метод обработки события нажатия правой кнопкой мыши по элементу TreeView (активации контекстного меню).
        /// </summary>
        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Определение элемента, на котором было вызвано событие.
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                // Перемещение фокуса на элемент.
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Метод поиска элемента в TreeView по соответствующему DependencyObject.
        /// </summary>
        /// <param name="source">Объект типа DependencyObject.</param>
        /// <returns>Элемент в TreeView.</returns>
        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            // Поиск элемента.
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        #endregion

        #region Обработка событий в DataGrid и метод его заполнения.

        /// <summary>
        /// Метод обновления данных dataGrid'a.
        /// </summary>
        private void UpdateDataGrid()
        {
            if (mainTree.SelectedItem != null)
            {
                // Перезаполнение списка товаров.
                products.Clear();
                AddProductsToList(mainTree.SelectedItem as Section);
                dataGrid.ItemsSource = products;
            }
            else
            {
                // Скрытие dataGrid'a в случае отсутствия выбранного раздела.
                dataGrid.ItemsSource = null;
                dataGrid.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Delete в контекстном меню dataGrid'a.
        /// </summary>
        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Получения menuItem'a, из которого было вызвано событие.
            MenuItem menuItem = (MenuItem)sender;

            // Получение контекстного меню, к которому принадлежит MenuItem.
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            // Получение dataGrid'а, из которого было вызвано меню.
            DataGrid item = (DataGrid)contextMenu.PlacementTarget;

            if (item.SelectedCells.Count == 0) return;

            // Получение контекста данных выбранного элемента dataGrid'a (товара).
            Product target = (Product)item.SelectedCells[0].Item;

            // Нахождение и удаление товара в двух циклах - для выбранного раздела и его подразделов.

            foreach (var product in (mainTree.SelectedItem as Section).Products)
            {
                if (product == target)
                {
                    (mainTree.SelectedItem as Section).Products.Remove(product);
                    UpdateDataGrid();
                    return;
                }
            }

            foreach (var section in (mainTree.SelectedItem as Section).Subsections)
            {
                foreach (var product in section.Products)
                {
                    if (product == target)
                    {
                        (mainTree.SelectedItem as Section).Products.Remove(product);
                        UpdateDataGrid();
                        return;
                    }
                }
            }
        }

        private void EditRow_Click(object sender, RoutedEventArgs e)
        {
            // Получения menuItem'a, из которого было вызвано событие.
            var menuItem = (MenuItem)sender;

            // Получение контекстного меню, к которому принадлежит MenuItem.
            var contextMenu = (ContextMenu)menuItem.Parent;

            // Получение dataGrid'а, из которого было вызвано меню.
            DataGrid item = (DataGrid)contextMenu.PlacementTarget;

            if (item.SelectedCells.Count == 0) return;

            // Получение контекста данных выбранного элемента dataGrid'a (товара).
            editingSubject = (Product)item.SelectedCells[0].Item;

            // Инициация процесса редактирования товара.
            mainTree.IsEnabled = false;
            editingMode = true;
            ToggleProductCreationScreen(true);
        }

        /// <summary>
        /// Обработчик нажатия на кнопку отображения расширенной информации о товаре.
        /// </summary>
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    // Переключение видимости области с деталями о строке.
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
            }
        }

        #endregion

        #endregion

        #region Методы обработки событий в меню программы.

        #region Методы обработки событий в меню File.
        /// <summary>
        /// Обработчик события нажатия кнопки New warehouse в меню File.
        /// </summary>
        private void NewWarehouse_Click(object sender, RoutedEventArgs e)
        {
            // Очистка источников данных.
            products.Clear();
            sections.Clear();

            // Запуск анимаций скрытия элементов интерфейса.
            TreeViewSlidingOutAnimation();
            DataGridOpacityOutAnimation();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Save current state в меню File.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Инициация окна выбора пути, в котором будет сохранён файл состояния склада.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML file (*.xml)|*.xml";
            saveFileDialog.Title = "Save current state...";

            // Сохранение файла.
            if (saveFileDialog.ShowDialog() == true) {
                try
                {
                    Save(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Load state в меню File.
        /// </summary>
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            // Инициация окна выбора файла, в котором сохранено состояние склада.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML file (*.xml)|*.xml";
            openFileDialog.Title = "Load state...";

            // Открытие файла и загрузка состояния склада.
            if (openFileDialog.ShowDialog() == true) {
                try
                {
                    Load(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Form CSV report в меню File.
        /// </summary>
        private void FormCSV_Click(object sender, RoutedEventArgs e)
        {
            // Инициация окна выбора пути, в котором будет сохранён файл-отчёт.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            saveFileDialog.Title = "Save a CSV report...";

            // Сохранение файла.
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveCSV(saveFileDialog.FileName);
                } catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Generate warehouse в меню File.
        /// </summary>
        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            // Инициация экрана и процесса генерации склада.
            ToggleGenerationScreen(true);
        }
        #endregion

        #region Методы обработки событий в меню Section.
        /// <summary>
        /// Обработчик события нажатия на кнопку Add separate section в меню Section.
        /// </summary>
        private void AddSection_Click(object sender, RoutedEventArgs e)
        {
            // Инициаиця процесса создания раздела.
            ToggleSectionNamingScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Add subsection в меню Section.
        /// </summary>
        private void AddSubsection_Click(object sender, RoutedEventArgs e)
        {
            if (mainTree.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса создания раздела.
                addingSubsection = true;
                mainTree.IsEnabled = false;
                ToggleSectionNamingScreen(true);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Edit section в меню Section.
        /// </summary>
        private void EditSection_Click(object sender, RoutedEventArgs e)
        {
            if (mainTree.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
                return;
            }

            // Инициация процесса редактирования раздела.
            editingMode = true;
            mainTree.IsEnabled = false;
            ToggleSectionNamingScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Delete section в меню Section.
        /// </summary>
        private void DeleteSection_Click(object sender, RoutedEventArgs e)
        {
            if (mainTree.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
                return;
            }

            // Определение удаляемого раздела.
            Section target = mainTree.SelectedItem as Section;

            // Инициация процесса удаления.
            if (target != null) DeleteSection(target);
        }
        #endregion

        #region Методы обработки событий в меню Product.
        /// <summary>
        /// Обработчик события нажатия на кнопку Delete product в меню Product.
        /// </summary>
        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            // Определение удаляемого товара.
            Product target = dataGrid.SelectedItem as Product;

            if (target == null)
            {
                // Уведомление об отсутствии выбранного товара.
                ToggleErrorScreen(true, "No product is selected!");
            }
            else
            {
                // Цикл поиска и удаления товара среди товаров выбранного раздела.
                foreach (var product in (mainTree.SelectedItem as Section).Products)
                {
                    if (product == target)
                    {
                        (mainTree.SelectedItem as Section).Products.Remove(product);

                        // Обновление dataGrid'a.
                        UpdateDataGrid();
                        return;
                    }
                }

                // Цикл поиска и удаления товара среди товаров подразделов выбранного раздела.
                foreach (var section in (mainTree.SelectedItem as Section).Subsections)
                {
                    foreach (var product in section.Products)
                    {
                        if (product == target)
                        {
                            (mainTree.SelectedItem as Section).Products.Remove(product);

                            // Обновление dataGrid'a.
                            UpdateDataGrid();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Edit product в меню Product.
        /// </summary>
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            // Определение редактируемого товара.
            Product target = dataGrid.SelectedItem as Product;

            if (target == null)
            {
                // Уведомление об отсутствии выбранного товара.
                ToggleErrorScreen(true, "No product is selected!");
            }
            else
            {
                // Установка субъекта редактирования.
                editingSubject = target;

                // Инициация процесса редактирования товара.
                mainTree.IsEnabled = false;
                editingMode = true;
                ToggleProductCreationScreen(true);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Add product в меню Product.
        /// </summary>
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (mainTree.SelectedItem == null)
            {
                // Уведомление об отсутствии выбранного раздела.
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса создания товара.
                mainTree.IsEnabled = false;
                ToggleProductCreationScreen(true);
            }
        }
        #endregion

        #endregion

        #region Методы различных экранов программы.

        #region Методы экрана создания и редактирования товаров.
        /// <summary>
        /// Метод активации экрана создания и редактирования товара.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleProductCreationScreen(bool toggleOn)
        {
            // Избежание повторной активации экрана.
            if (creationScreenIsActive && toggleOn) return;
            // Установка размера экрана.
            productCreationGrid.Margin = new Thickness(mainTree.Width, 30, 0, 0);
            // Обнуленние флагов заполненности полей.
            productFieldsFilled = new bool[] { false, false, false, false, false };
            // Изменение видимости элементов экрана и самого экрана.
            productApplyButton.Visibility = Visibility.Hidden;
            productCreationGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            dropLabel.Visibility = Visibility.Visible;
            productImageBox.Visibility = Visibility.Hidden;
            productInputLabelError.Visibility = Visibility.Hidden;
            // Установка заголовка экрана.
            productInputLabel.Content = editingMode ? "PRODUCT EDITING" : "PRODUCT CREATION";

            // Установка значений по умолчанию - пустые строки при создании товара.
            if (!editingMode)
            {
                productNameTextbox.Text = string.Empty;
                productCodeTextbox.Text = string.Empty;
                productAmountTextbox.Text = string.Empty;
                productMinAmountTextbox.Text = string.Empty;
                productPriceTextbox.Text = string.Empty;
                productDescTextbox.Text = string.Empty;
                productImageBox.Source = null;
            }
            // Установка значений по умолчанию - свойства товара при его редактировании.
            else
            {
                productNameTextbox.Text = editingSubject.Name;
                productCodeTextbox.Text = editingSubject.Code;
                productAmountTextbox.Text = editingSubject.Amount.ToString();
                productMinAmountTextbox.Text = editingSubject.MinimalAmount.ToString();
                productPriceTextbox.Text = editingSubject.Price.ToString();
                productDescTextbox.Text = editingSubject.Description;
                productImageBox.Source = editingSubject.Image;

                if (productImageBox.Source != null)
                {
                    dropLabel.Visibility = Visibility.Hidden;
                    productImageBox.Visibility = Visibility.Visible;
                }
            }

            // Установка значения флага.
            creationScreenIsActive = toggleOn ? true : false;
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для артикула товара.
        /// </summary>
        private void productCodeTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterTextboxChanges(0);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для цены товара.
        /// </summary>
        private void productPriceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterTextboxChanges(1);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для названия товара.
        /// </summary>
        private void productNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterTextboxChanges(2);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для количества товара.
        /// </summary>
        private void productAmountTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterTextboxChanges(3);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для минимального количества товара.
        /// </summary>
        private void productMinAmountTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterTextboxChanges(4);
        }

        /// <summary>
        /// Метод регистрации изменений в обязательном для заполнения поле.
        /// </summary>
        /// <param name="i">Индекс поля.</param>
        private void RegisterTextboxChanges(int i)
        {
            // Регистрация изменения в массиве флагов наличия изменений в полях.
            productFieldsFilled[i] = true;
            if (productFieldsFilled[0] && productFieldsFilled[1] && productFieldsFilled[2] && productFieldsFilled[3])
                productApplyButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку создания товара\применения изменений в нём.
        /// </summary>
        private void productApplyButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Пустые переменные для методов TryParse.
            int dummyInt;
            decimal dummyDec;

            // Если все обязательные поля заполнены корректно.
            if (productFieldsFilled[0] && productFieldsFilled[1] && productFieldsFilled[2] && productFieldsFilled[3] && productFieldsFilled[4]
                && int.TryParse(productAmountTextbox.Text, out dummyInt) && int.TryParse(productAmountTextbox.Text, out dummyInt)
                && decimal.TryParse(productPriceTextbox.Text, out dummyDec))
            {
                // Вывод сообщения об ошибке в случае, если товар с данным артикулом существует.
                if (codes.Contains(productCodeTextbox.Text) && productCodeTextbox.Text != editingSubject.Code)
                {
                    productInputLabelError.Content = "Error: product with such code already exists, please try again";
                    productInputLabelError.Visibility = Visibility.Visible;
                    return;
                }

                // Создание товара на основе значений в полях.
                if (!editingMode)
                {
                    // Создание и добавление товара.
                    (mainTree.SelectedItem as Section).AddProduct(new Product(productNameTextbox.Text, productCodeTextbox.Text,
                    decimal.Parse(productPriceTextbox.Text), int.Parse(productAmountTextbox.Text), int.Parse(productMinAmountTextbox.Text),
                    productDescTextbox.Text, productImageBox.Source) { ParentSection = mainTree.SelectedItem as Section });
                    // Регистрация артикула.
                    codes.Add(productCodeTextbox.Text);
                }
                // Редактирование товара на основе значений в полях.
                else
                {
                    // Регистрация изменения артикула.
                    for (int i = 0; i < codes.Count; i++)
                    {
                        if (codes[i] == editingSubject.Code) codes[i] = productCodeTextbox.Text;
                    }

                    // Изменение свойств товара.
                    editingSubject.Name = productNameTextbox.Text;
                    editingSubject.Code = productCodeTextbox.Text;
                    editingSubject.Price = decimal.Parse(productPriceTextbox.Text);
                    editingSubject.Amount = int.Parse(productAmountTextbox.Text);
                    editingSubject.MinimalAmount = int.Parse(productMinAmountTextbox.Text);
                    editingSubject.Description = productDescTextbox.Text;
                    editingSubject.Image = productImageBox.Source;

                    // Отключение режима редактирования.
                    editingMode = false;
                    editingSubject = null;
                    mainTree.IsEnabled = true;
                }

                ToggleProductCreationScreen(false);
                mainTree.IsEnabled = true;
            }
            // Вывод сообщения об ошибке при обнаружении неверных значений полей.
            else
            {
                productInputLabelError.Content = "Error: invalid values detected, please try again";
                productInputLabelError.Visibility = Visibility.Visible;
            }

            // Обновление dataGrid'a.
            UpdateDataGrid();
        }

        /// <summary>
        /// Обработчик события "перетаскивания" изображения в соответствующую область экрана создания товара.
        /// </summary>
        private void Image_Drop(object sender, DragEventArgs e)
        {
            // Проверка на наличие файлов.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Получение путей к перетаскиваемым файлам.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Сохранение и отображение изображения в случае, если первый перетаскиваемый файл - изображение.
                if (System.IO.Path.GetExtension(files[0]) == ".jpg" || System.IO.Path.GetExtension(files[0]) == ".png" || System.IO.Path.GetExtension(files[0]) == ".bmp")
                {
                    productImageBox.Source = new BitmapImage(new Uri(files[0]));
                    productImageBox.Visibility = Visibility.Visible;
                }
                else return;

                // Отключение лейбла с подсказками.
                dropLabel.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Обработчик события курсора над областью для изображения при перемещении файлов.
        /// </summary>
        private void borderRect_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            // Изменение курсора.
            e.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Обработчик события нажатия кнопки отмены.
        /// </summary>
        private void productCancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана создания товара.
            ToggleProductCreationScreen(false);
        }
        #endregion

        #region Методы экрана создания и редактирования разделов.
        /// <summary>
        /// Метод активации экрана создания и редактирования раздела.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleSectionNamingScreen(bool toggleOn)
        {
            // Избежание повторной активации экрана.
            if (creationScreenIsActive && toggleOn) return;
            // Установка размера экрана.
            sectionCreationGrid.Margin = new Thickness(mainTree.Width, 30, 0, 0);
            // Изменение видимости элементов экрана и самого экрана.
            sectionNameApplyButton.Visibility = Visibility.Hidden;
            sectionCreationGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            sectionInputLabelError.Visibility = Visibility.Hidden;
            // Установка заголовка экрана.
            sectionInputLabel.Content = !editingMode ? "Enter section name:" : "Enter new section name:";
            // Установка заголовка экрана.
            sectionNameTextbox.Text = string.Empty;
            // Установка значения флага активности экрана.
            creationScreenIsActive = toggleOn ? true : false;
        }

        /// <summary>
        /// Обработчик события изменения текста в поле ввода названия раздела.
        /// </summary>
        private void sectionNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sectionNameApplyButton != null)
            {
                // Отображение либо скрытие кнопки создания раздела на основе текста в поле.
                if (sectionNameTextbox.Text == "") sectionNameApplyButton.Visibility = Visibility.Hidden;
                else if (sectionNameTextbox.Text != "") sectionNameApplyButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку создания раздела\применения внесенных измненений.
        /// </summary>
        private void sectionNameApplyButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если не осуществлялось создание подраздела.
            if (!addingSubsection)
            {
                // Редактирование раздела (внесение изменений).
                if (editingMode)
                {
                    // Итерирование по разделам для нахождения субъекта редактирования и внесения изменений.
                    foreach (var sec in sections)
                    {
                        if (sec == mainTree.SelectedItem as Section)
                        {
                            // Внесение изменений.
                            sec.Name = sectionNameTextbox.Text;
                            break;
                        }

                        foreach (var subsec in sec.Subsections)
                        {
                            if (subsec == mainTree.SelectedItem as Section)
                            {
                                // Внесение изменений.
                                subsec.Name = sectionNameTextbox.Text;
                                break;
                            }
                        }
                    }

                    // Отключение режима редактирования.
                    editingMode = false;
                    mainTree.IsEnabled = true;
                }
                // Создание раздела.
                else
                {
                    // Проверка на наличие раздела с тем же названием с выводом сообщения об ошибке.
                    foreach (var section in sections)
                    {
                        if (section.Name == sectionNameTextbox.Text)
                        {
                            sectionInputLabelError.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    // Создание раздела.
                    sections.Add(new Section(sectionNameTextbox.Text));

                    // Запуск анимации TreeView если до того не было разделов.
                    if (sections.Count == 1) TreeViewSlidingInAnimation();
                }
            }
            // Создание подраздела.
            else
            {
                // Проверка на наличие раздела с тем же названием с выводом сообщения об ошибке.
                foreach (var subsec in (mainTree.SelectedItem as Section).Subsections)
                {
                    if (subsec.Name == sectionNameTextbox.Text)
                    {
                        sectionInputLabelError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                // Создание подраздела.
                (mainTree.SelectedItem as Section).AddSubsection(new Section(sectionNameTextbox.Text) { parentSection = mainTree.SelectedItem as Section });

                addingSubsection = false;
                mainTree.IsEnabled = true;
            }

            // Деактивация экрана.
            ToggleSectionNamingScreen(false);
        }

        /// <summary>
        /// Обработчик события нажатия кнопки отмены.
        /// </summary>
        private void sectionNameCancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleSectionNamingScreen(false);
        }
        #endregion

        #region Методы экрана подтверждения удаления раздела.
        /// <summary>
        /// Метод активации экрана подтверждения удаления раздела.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleConfirmationScreen(bool toggleOn)
        {
            // Установка размера экрана и его видимости.
            confirmationScreenGrid.Margin = new Thickness(mainTree.Width, 30, 0, 0);
            confirmationScreenGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку подтверждения удаления раздела.
        /// </summary>
        private void confimationButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Определение удаляемого раздела и его удаление.
            Section target = mainTree.SelectedItem as Section;
            if (target.parentSection != null)
            {
                foreach (Section section in sections)
                {
                    section.RemoveSubsection(target);
                }
            }
            else
            {
                sections.Remove(mainTree.SelectedItem as Section);
            }

            // Запуск анимации скрытия TreeView, если разделов не осталось.
            if (sections.Count == 0) TreeViewSlidingOutAnimation();

            // Обновление dataGrid'a.
            UpdateDataGrid();
            ToggleConfirmationScreen(false);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку отмены удаления раздела.
        /// </summary>
        private void cancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleConfirmationScreen(false);
        }
        #endregion

        #region Методы экрана уведомления об ошибке.
        /// <summary>
        /// Метод активации экрана уведомления об ошибке.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        /// <param name="message">Сообщение об ошибке.</param>
        private void ToggleErrorScreen(bool toggleOn, string message)
        {
            // Установка видимости и размеров экрана.
            errorScreenGrid.Margin = new Thickness(mainTree.Width, 30, 0, 0);
            errorScreenGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            // Отображение сообщения об ошибке.
            errorLabel.Content = $"ERROR: {message}";
        }

        /// <summary>
        /// Обработчик события нажатия кнопки подтверждения.
        /// </summary>
        private void errorConfimationButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана уведомления об ошибке.
            ToggleErrorScreen(false, "");
        }
        #endregion

        #region Методы экрана генерации склада.
        /// <summary>
        /// Метод активации экрана генерации.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleGenerationScreen(bool toggleOn)
        {
            // Установка размеров и видимости экрана.
            warehouseGenerationGrid.Margin = new Thickness(mainTree.Width, 30, 0, 0);
            warehouseGenerationGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            generationLabelError.Visibility = Visibility.Hidden;
            // Установка значений по умолчанию.
            sectionGenerationTextbox.Text = string.Empty;
            productGenerationTextbox.Text = string.Empty;
        }

        /// <summary>
        /// Обработчик нажатия кнопки генерации.
        /// </summary>
        private void generationApplyButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            int dummy;
            // Проверка введённых данных на корректность и вывод сообщения об ошибке.
            if (!int.TryParse(sectionGenerationTextbox.Text, out dummy) || !int.TryParse(productGenerationTextbox.Text, out dummy) 
                || int.Parse(sectionGenerationTextbox.Text) < 0 || int.Parse(productGenerationTextbox.Text) < 0)
            {
                generationLabelError.Content = "ERROR: Invalid values detected in textboxes, please try again";
                generationLabelError.Visibility = Visibility.Visible;
                return;
            }

            // Предварительная очистка источников данных.
            sections.Clear();
            products.Clear();

            // Число генерируемых разделов.
            int sectionsNum = int.Parse(sectionGenerationTextbox.Text);
            // Случайное число независимых разделов.
            int independentSectionsNum = random.Next(1, sectionsNum + 1);
            // Случайное число подразделов.
            int dependentSectionsNum = sectionsNum - independentSectionsNum;
            // Число генерируемых товаров.
            int productNum = int.Parse(productGenerationTextbox.Text);

            // Цикл генерации независимых разделов с подразделами и товарами.
            for (int i = 0; i < independentSectionsNum; i++)
            {
                sections.Add(Generation.GenerateSection(ref dependentSectionsNum, ref productNum, null, codes));
            }

            // Дополнительная генерация подразделов в случае, если их число меньше заданного пользователем.
            while (dependentSectionsNum > 0)
            {
                int randomIndex = random.Next(0, independentSectionsNum);
                dependentSectionsNum--;
                sections[randomIndex].AddSubsection(Generation.GenerateSection(ref dependentSectionsNum, ref productNum, sections[randomIndex], codes));
            }

            // Дополнительная генерация твоаров в случае, если их число меньше заданного пользователем.
            while (productNum > 0)
            {
                int randomIndex = random.Next(0, independentSectionsNum);
                sections[randomIndex].AddProduct(Generation.GenerateProduct(codes, sections[randomIndex]));
                productNum--;
            }

            // Деактивация экрана генерации.
            ToggleGenerationScreen(false);
            // Запуск анимации TreeView.
            if (sections.Count != 0) TreeViewSlidingInAnimation();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку отмены.
        /// </summary>
        private void generationCancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана генерации.
            ToggleGenerationScreen(false);
        }
        #endregion

        #endregion

        #region Методы сохранения в файл и загрузки файлов.
        /// <summary>
        /// Обработчик события "тика" таймера автосохранения.
        /// </summary>
        private void timerSave_Tick(object sender, EventArgs e)
        {
            try
            {
                // Автосохранение состояния склада.
                if (!Directory.Exists("Autosaves")) Directory.CreateDirectory("Autosaves");
                Save($"Autosaves{Path.DirectorySeparatorChar}warehouseAutosave_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{DateTime.Now.ToString("HH-mm-ss")}.xml");
            }
            catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод сохранения состояния склада в файл.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        private void Save(string filename)
        {
            // Инициализация сериалайзера.
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section[]), new Type[] { typeof(Product) });

            // Сериализация и сохранение состояния склада в файле формата XML.
            using (Stream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                xmlSerializer.Serialize(fs, sections.ToArray());
            }
        }

        /// <summary>
        /// Метод загрузки состояния склада из файла.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        private void Load(string filename)
        {
            // Предварительная очистка источников данных.
            sections.Clear();
            products.Clear();

            // Инициализация сериалайзера.
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Section[]), new Type[] { typeof(Product) });

            // Чтение файла через поток.
            using (Stream fs = File.OpenRead(filename))
            {
                // Десериализация массива разделов с товарами.
                Section[] sectionsArr = (Section[])xmlSerializer.Deserialize(fs);

                // Заполнение источников данных.
                if (sectionsArr.Length != 0)
                {
                    foreach (var section in sectionsArr) sections.Add(section);

                    // Установка родительских разделов для всех разделов склада.
                    foreach (var section in sections) section.SetChildrenParentSection();

                    // Активация анимации раскрытия TreeView.
                    TreeViewSlidingInAnimation();
                }
            }
        }

        /// <summary>
        /// Метод сохранения списка недостающих товаров в CSV-файл.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        private void SaveCSV(string filename)
        {
            // Получение списка недостающих товаров.
            List<Product> lackingProducts = new List<Product>();
            foreach (var section in sections) section.GetLackingProducts(ref lackingProducts);

            // Запись списка в CSV-файл при помощи CSVHelper.
            using (var writer = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ProductMap>();
                csv.WriteRecords(lackingProducts);
            }
        }

        #endregion

        #region Анимации.
        /// <summary>
        /// Обработчик события "тика" таймера начала анимации.
        /// </summary>
        private void timerAnim_Tick(object sender, EventArgs e)
        {
            // Запуск анимации.
            LogoAnimation();
            timerAnim.Stop();
        }

        /// <summary>
        /// Анимация логотипа при запуске программы.
        /// </summary>
        private void LogoAnimation()
        {
            // Анимация буквы W.
            ThicknessAnimation WThicknessAnimation = new ThicknessAnimation();
            WThicknessAnimation.From = new Thickness(0, 0, 0, 52);
            WThicknessAnimation.To = new Thickness(0, 0, 300, 52);
            WThicknessAnimation.Duration = TimeSpan.FromSeconds(1);
            WImage.BeginAnimation(Image.MarginProperty, WThicknessAnimation);

            // Анимация буквы M.
            ThicknessAnimation MThicknessAnimation = new ThicknessAnimation();
            MThicknessAnimation.From = new Thickness(0, 52, 0, 0);
            MThicknessAnimation.To = new Thickness(0, 52, 140, 0);
            MThicknessAnimation.Duration = TimeSpan.FromSeconds(1);

            // Анимация постепенного появления.
            DoubleAnimation fadeinAnimation = new DoubleAnimation();
            fadeinAnimation.From = 0;
            fadeinAnimation.To = 1;
            fadeinAnimation.Duration = TimeSpan.FromSeconds(1.5);

            // Включение анимации постепенного появления текста по окончании анимации букв W и M.
            MThicknessAnimation.Completed += (s, e) =>
            {
                arehouseImage.Visibility = Visibility.Visible;
                anagerImage.Visibility = Visibility.Visible;

                arehouseImage.Opacity = 0;
                anagerImage.Opacity = 0;
                // Запуск анимации.
                arehouseImage.BeginAnimation(Image.OpacityProperty, fadeinAnimation);
                anagerImage.BeginAnimation(Image.OpacityProperty, fadeinAnimation);
            };

            // Включение анимации постепенного исчезновения логотипа по окончании анимации появления текста.
            fadeinAnimation.Completed += (s, e) =>
            {
                // Анимация постепенного исчезновения.
                DoubleAnimation fadeoutAnimation = new DoubleAnimation();
                fadeoutAnimation.From = 1;
                fadeoutAnimation.To = 0;
                fadeoutAnimation.Duration = TimeSpan.FromSeconds(1);
                // Запуск анимации для элементов логотипа.
                WImage.BeginAnimation(Image.OpacityProperty, fadeoutAnimation);
                MImage.BeginAnimation(Image.OpacityProperty, fadeoutAnimation);
                anagerImage.BeginAnimation(Image.OpacityProperty, fadeoutAnimation);
                arehouseImage.BeginAnimation(Image.OpacityProperty, fadeoutAnimation);

                nameLabel.Visibility = Visibility.Visible;
                tipLabel.Visibility = Visibility.Visible;

                nameLabel.Opacity = 0;
                tipLabel.Opacity = 0;

                // Анимация постепенного появления лейблов.
                DoubleAnimation labelFadeinAnimation = new DoubleAnimation();
                labelFadeinAnimation.From = 0;
                labelFadeinAnimation.To = 1;
                labelFadeinAnimation.Duration = TimeSpan.FromSeconds(1.5);
                // Запуск постепенного появления для лейблов-подсказок.
                nameLabel.BeginAnimation(Label.OpacityProperty, labelFadeinAnimation);
                tipLabel.BeginAnimation(Label.OpacityProperty, labelFadeinAnimation);
            };

            // Запуск анимации логотипа.
            MImage.BeginAnimation(Image.MarginProperty, MThicknessAnimation);
        }

        /// <summary>
        /// Метод запуска анимации постепенного появления dataGrid'a.
        /// </summary>
        private void DataGridOpacityInAnimation()
        {
            // Инициализация анимации.
            DoubleAnimation dataGridOpacityAnimation = new DoubleAnimation();
            dataGridOpacityAnimation.From = 0;
            dataGridOpacityAnimation.To = 1;
            dataGridOpacityAnimation.Duration = TimeSpan.FromSeconds(0.5);
            // Запуск анимации.
            dataGrid.BeginAnimation(DataGrid.OpacityProperty, dataGridOpacityAnimation);
        }

        /// <summary>
        /// Метод запуска анимации постепенного исчезания dataGrid'a.
        /// </summary>
        private void DataGridOpacityOutAnimation()
        {
            // Инициализация анимации.
            DoubleAnimation dataGridOpacityAnimation = new DoubleAnimation();
            dataGridOpacityAnimation.From = 1;
            dataGridOpacityAnimation.To = 0;
            dataGridOpacityAnimation.Duration = TimeSpan.FromSeconds(0.5);
            // Запуск анимации.
            dataGrid.BeginAnimation(DataGrid.OpacityProperty, dataGridOpacityAnimation);
        }

        /// <summary>
        /// Метод запуска анимации постепенного появления treeView.
        /// </summary>
        private void TreeViewSlidingInAnimation()
        {
            // Анимация mainTree.
            DoubleAnimation treeAnimation = new DoubleAnimation();
            treeAnimation.From = 0;
            treeAnimation.To = 193;
            treeAnimation.Duration = TimeSpan.FromSeconds(0.5);
            mainTree.BeginAnimation(TreeView.WidthProperty, treeAnimation);

            // Анимация лейбла с названием программы.
            ThicknessAnimation nameThicknessAnimation = new ThicknessAnimation();
            nameThicknessAnimation.From = new Thickness(0, 0, 0, 30);
            nameThicknessAnimation.To = new Thickness(193, 0, 0, 30);
            nameThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            nameLabel.BeginAnimation(Label.MarginProperty, nameThicknessAnimation);

            // Анимация лейбла с подсказкой.
            ThicknessAnimation tipThicknessAnimation = new ThicknessAnimation();
            tipThicknessAnimation.From = new Thickness(0, 30, 0, 0);
            tipThicknessAnimation.To = new Thickness(193, 30, 0, 0);
            tipThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            tipLabel.BeginAnimation(Label.MarginProperty, tipThicknessAnimation);
        }

        /// <summary>
        /// Метод запуска анимации постепенного исчезновения treeView.
        /// </summary>
        private void TreeViewSlidingOutAnimation()
        {
            // Анимация mainTree.
            DoubleAnimation treeAnimation = new DoubleAnimation();
            treeAnimation.From = 193;
            treeAnimation.To = 0;
            treeAnimation.Duration = TimeSpan.FromSeconds(0.5);
            mainTree.BeginAnimation(TreeView.WidthProperty, treeAnimation);

            // Анимация лейбла с названием программы.
            ThicknessAnimation nameThicknessAnimation = new ThicknessAnimation();
            nameThicknessAnimation.From = new Thickness(193, 0, 0, 30);
            nameThicknessAnimation.To = new Thickness(0, 0, 0, 30);
            nameThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            nameLabel.BeginAnimation(Label.MarginProperty, nameThicknessAnimation);

            // Анимация лейбла с подсказкой.
            ThicknessAnimation tipThicknessAnimation = new ThicknessAnimation();
            tipThicknessAnimation.From = new Thickness(193, 30, 0, 0);
            tipThicknessAnimation.To = new Thickness(0, 30, 0, 0);
            tipThicknessAnimation.Duration = TimeSpan.FromSeconds(0.5);
            tipLabel.BeginAnimation(Label.MarginProperty, tipThicknessAnimation);
        }
        #endregion
    }
}
