using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
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
    /// Перечисление, содержащее основные режимы отображения данных в окне программы:
    /// None - отображение заглавного экрана программы;
    /// SectionProductView - отображение иерархии разделов товаров на складе и товаров в них;
    /// ShoppingCartProductView - отображение содержимого корзины пользователя;
    /// OrderProductView - отображение списка заказов и информации о них;
    /// ClientOrderView - отображение списка клиентов и информации о них (включая их заказы);
    /// ActiveOrderView - отображение списка активных заказов.
    /// </summary>
    enum ViewMode
    {
        None,
        SectionProductView,
        ShoppingCartProductView,
        OrderProductView,
        ClientOrderView,
        ActiveOrderView
    }

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
        /// Активный до перехода к текущему режим отображения данных в окне.
        /// </summary>
        ViewMode prevWindowDataViewMode;
        /// <summary>
        /// Текущий режим отображения данных в окне.
        /// </summary>
        ViewMode windowDataViewMode;
        /// <summary>
        /// Активный dataGrid программы.
        /// </summary>
        DataGrid activeDataGrid;
        /// <summary>
        /// Активный treeView программы.
        /// </summary>
        TreeView activeTreeView;
        /// <summary>
        /// Заказ, с которым производится взаимодействие (если таковой есть на данный момент).
        /// </summary>
        Order targetOrder;
        /// <summary>
        /// Активный (текущий) пользователь программы.
        /// </summary>
        User activeUser = null;

        /// <summary>
        /// Флаги заполненности полей в экране создания продукта.
        /// </summary>
        bool[] productFieldsFilled = new bool[] { false, false, false, false, false };
        /// <summary>
        /// Флаги заполненности полей в экране создания продукта.
        /// </summary>
        bool[] signInFieldsFilled = new bool[] { false, false, false, false };
        /// <summary>
        /// Массив артикулов товаров на складе.
        /// </summary>
        private List<string> codes = new List<string>();
        /// <summary>
        /// Коллекция разделов склада - данные для sectionTree.
        /// </summary>
        private ObservableCollection<Section> sections = new ObservableCollection<Section>();
        /// <summary>
        /// Коллекция отображаемых товаров - данные для dataGrid.
        /// </summary>
        private ObservableCollection<Product> products = new ObservableCollection<Product>();
        /// <summary>
        /// Коллекция товаров в корзине - данные для dataGrid.
        /// </summary>
        private ObservableCollection<OrderProduct> shoppingCartProducts = new ObservableCollection<OrderProduct>();
        /// <summary>
        /// Список пользователей программы (загружается из файла).
        /// </summary>
        private List<User> users = new List<User>();

        /// <summary>
        /// Конструктор окна.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Установка источников данных.
            productDataGrid.ItemsSource = products;
            sectionTree.ItemsSource = sections;
            sectionNoMenuTree.ItemsSource = sections;

            // Установка минимального размера окна.
            MinHeight = 450;
            MinWidth = 800;

            // Запуск таймера начала анимации.
            timerAnim.Tick += new EventHandler(timerAnim_Tick);
            timerAnim.Interval = TimeSpan.FromSeconds(1.5);
            timerAnim.Start();

            // Запуск таймера автосохранения.
            timerSave.Tick += new EventHandler(timerSave_Tick);
            timerSave.Interval = TimeSpan.FromMinutes(3);
            timerSave.Start();

            LoadUserList();
            InitializeUserInterface();
        }

        #region Обработчики событий окна.

        /// <summary>
        /// Обработчик события, вызываемого перед закрытием окна.
        /// </summary>
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Запуск сохранения пользователей и их заказов.
            SaveUsers();
        }

        #endregion

        #region Методы получения данных для отображения\экспорта.

        /// <summary>
        /// Метод получения всех заказов, сделанных клиентами.
        /// </summary>
        /// <returns>Коллекция всех заказов, сделанных клиентами.</returns>
        private ObservableCollection<Order> GetAllOrders()
        {
            // Инициализация коллекции.
            ObservableCollection<Order> orders = new ObservableCollection<Order>();

            // Заполнение коллекции.
            foreach (var user in users)
            {
                if (user is Client)
                {
                    foreach (var order in (user as Client).Orders) orders.Add(order);
                }
            }

            return orders;
        }

        /// <summary>
        /// Метод получения всех активных (не исполненных) на данный момент заказов.
        /// </summary>
        /// <returns>Коллекция всех активных заказов.</returns>
        private ObservableCollection<Order> GetAllActiveOrders()
        {
            // Инициализация коллекции.
            ObservableCollection<Order> orders = new ObservableCollection<Order>();

            // Заполнение коллекции.
            foreach (var user in users)
            {
                if (user is Client)
                {
                    foreach (var order in (user as Client).Orders)
                    {
                        if ((order.Status & Status.Executed) != Status.Executed) orders.Add(order);
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// Метод получения всех клиентов среди зарегистрированных пользователей.
        /// </summary>
        /// <returns>Коллекция клиентов.</returns>
        private ObservableCollection<Client> GetAllClients()
        {
            // Инициализация коллекции.
            ObservableCollection<Client> clients = new ObservableCollection<Client>();

            // Заполнение коллекции.
            foreach (var user in users)
            {
                if (user is Client) clients.Add(user as Client);
            }

            return clients;
        }

        #endregion

        #region Методы инициализации и обновления пользовательского интерфейса.

        /// <summary>
        /// Метод инициализации основных элементов пользовательского интерфейса в зависимости от активного пользователя.
        /// </summary>
        private void InitializeUserInterface()
        {
            // Если вход не был выполнен.
            if (activeUser == null)
            {
                ActiveUserNullUIActivation();
            }
            // Если активный пользователь является клиентом.
            else if (activeUser is Client)
            {
                ActiveUserClientUIActivation();
            }
            // Если активный пользователь является администратором.
            else if (activeUser is Administrator)
            {
                ActiveUserAdministratorUIActivation();
            }
        }

        /// <summary>
        /// Метод активации элементов пользовательского интерфейса в случае, если вход не был выполнен.
        /// </summary>
        private void ActiveUserNullUIActivation()
        {
            // Инициализация текста лейблов в меню и на экране подсказки.
            userNameLabel.Content = "Not logged in";
            nameLabel.Content = "WAREHOUSE MANAGER";
            tipLabel.Content = "Please log in to interact with the warehouse";

            // Установка изображения в хэдере меню пользователя.
            userPicture.Source = new BitmapImage(new Uri("pack://application:,,,/Warehouse;component/res/icons8-user-not-found-30.png", UriKind.Absolute));

            // Скрытие всех пунктов меню, кроме File и User.
            foreach (MenuItem menuItem in menuStrip.Items)
            {
                if (menuItem.Name != "fileMenuItem" && menuItem.Name != "userMenuItem")
                {
                    menuItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    menuItem.Visibility = Visibility.Visible;
                }
            }

            // Установка видимости главных элементов интерфейса.
            newWarehouseMenuItem.Visibility = Visibility.Collapsed;
            newWarehouseItemSeparator.Visibility = Visibility.Collapsed;
            saveStateMenuItem.Visibility = Visibility.Collapsed;
            sectionViewMenuItem.Visibility = Visibility.Collapsed;
            sectionViewMenuItemSeparator.Visibility = Visibility.Collapsed;
            productDataGrid_editContextMenuItem.Visibility = Visibility.Collapsed;
            productDataGrid_deleteContextMenuItem.Visibility = Visibility.Collapsed;
            productDataGrid_addToOrderContextMenuItem.Visibility = Visibility.Collapsed;
            logInMenuItem.Visibility = Visibility.Visible;
            signInMenuItem.Visibility = Visibility.Visible;
            logOutMenuItem.Visibility = Visibility.Collapsed;
            amountColumn.Visibility = Visibility.Collapsed;

            // Если не загружен склад, либо загруженный склад пуст.
            if (sections.Count == 0)
            {
                // Установка режима отображения None.
                windowDataViewMode = ViewMode.None;
                prevWindowDataViewMode = ViewMode.None;
            }
            // Если загруженный склад непуст.
            else
            {
                // Установка режима отображения SectionProductView.
                windowDataViewMode = ViewMode.SectionProductView;
                prevWindowDataViewMode = ViewMode.None;
            }

            // Установка состояния интерфейса в соответствии с режимом отображения.
            SetWindowViewState();
        }

        /// <summary>
        /// Метод активации элементов пользовательского интерфейса в случае, если активный пользователь - клиент.
        /// </summary>
        private void ActiveUserClientUIActivation()
        {
            // Инициализация текста лейблов в меню и на экране подсказки.
            userNameLabel.Content = $"Client {activeUser.FullName}";
            nameLabel.Content = $"WELCOME, {activeUser.FullName.ToUpper()}!";
            tipLabel.Content = "Load the warehouse and browse it using the panel on the left";

            // Установка изображения в хэдере меню пользователя.
            userPicture.Source = new BitmapImage(new Uri("pack://application:,,,/Warehouse;component/icons8-user-male-30.png", UriKind.Absolute));

            // Скрытие всех пунктов меню, кроме File, Order и User.
            foreach (MenuItem menuItem in menuStrip.Items)
            {
                if (menuItem.Name != "fileMenuItem" && menuItem.Name != "userMenuItem" && menuItem.Name != "orderMenuItem")
                {
                    menuItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    menuItem.Visibility = Visibility.Visible;
                }
            }

            // Установка видимости главных элементов интерфейса.
            newWarehouseMenuItem.Visibility = Visibility.Collapsed;
            newWarehouseItemSeparator.Visibility = Visibility.Collapsed;
            saveStateMenuItem.Visibility = Visibility.Collapsed;
            sectionViewMenuItem.Visibility = Visibility.Visible;
            sectionViewMenuItemSeparator.Visibility = Visibility.Visible;
            productDataGrid_editContextMenuItem.Visibility = Visibility.Collapsed;
            productDataGrid_deleteContextMenuItem.Visibility = Visibility.Collapsed;
            productDataGrid_addToOrderContextMenuItem.Visibility = Visibility.Visible;
            logInMenuItem.Visibility = Visibility.Collapsed;
            signInMenuItem.Visibility = Visibility.Collapsed;
            logOutMenuItem.Visibility = Visibility.Visible;
            amountColumn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Метод активации элементов пользовательского интерфейса в случае, если активный пользователь - администратор.
        /// </summary>
        private void ActiveUserAdministratorUIActivation()
        {
            // Инициализация текста лейблов в меню и на экране подсказки.
            userNameLabel.Content = $"Administrator {activeUser.FullName}";
            nameLabel.Content = $"WELCOME, {activeUser.FullName.ToUpper()}!";
            tipLabel.Content = "You are given full rights of an administrator";

            // Установка изображения в хэдере меню пользователя.
            userPicture.Source = new BitmapImage(new Uri("pack://application:,,,/Warehouse;component/icons8-user-male-30.png", UriKind.Absolute));

            // Скрытие всех пунктов меню, кроме File, Section, Product, Administrator и User.
            foreach (MenuItem menuItem in menuStrip.Items)
            {
                if (menuItem.Name != "fileMenuItem" && menuItem.Name != "userMenuItem" && menuItem.Name != "sectionMenuItem"
                    && menuItem.Name != "productMenuItem" && menuItem.Name != "administratorMenuItem")
                {
                    menuItem.Visibility = Visibility.Collapsed;
                }
                else
                {
                    menuItem.Visibility = Visibility.Visible;
                }
            }

            // Установка видимости главных элементов интерфейса.
            newWarehouseMenuItem.Visibility = Visibility.Visible;
            newWarehouseItemSeparator.Visibility = Visibility.Visible;
            saveStateMenuItem.Visibility = Visibility.Visible;
            sectionViewMenuItem.Visibility = Visibility.Visible;
            sectionViewMenuItemSeparator.Visibility = Visibility.Visible;
            productDataGrid_editContextMenuItem.Visibility = Visibility.Visible;
            productDataGrid_deleteContextMenuItem.Visibility = Visibility.Visible;
            productDataGrid_addToOrderContextMenuItem.Visibility = Visibility.Collapsed;
            logInMenuItem.Visibility = Visibility.Collapsed;
            signInMenuItem.Visibility = Visibility.Collapsed;
            logOutMenuItem.Visibility = Visibility.Visible;
            amountColumn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Метод установки состояния интерфейса в соответствии с режимом отображения.
        /// </summary>
        /// <param name="targetClient"></param>
        private void SetWindowViewState(Client targetClient = null)
        {
            // Скрытие кнопки возврата к просмотру склада, если он не загружен или пуст.
            if (sections.Count == 0)
            {
                sectionViewMenuItem.Visibility = Visibility.Collapsed;
                sectionViewMenuItemSeparator.Visibility = Visibility.Collapsed;
            }
            // Отображение кнопки возврата к просмотру склада, если он непуст.
            else
            {
                sectionViewMenuItem.Visibility = Visibility.Visible;
                sectionViewMenuItemSeparator.Visibility = Visibility.Visible;
            }

            // Если ранее был установлен иной режим отображения окна.
            if (prevWindowDataViewMode != windowDataViewMode)
            {
                switch (prevWindowDataViewMode)
                {
                    // Скрытие кнопки возврата к предыдущему режиму, если тот являлся None.
                    case ViewMode.None:
                        treeReturnButton.Visibility = Visibility.Hidden;
                        break;
                    // Отображение кнопки возврата к предыдущему режиму с соответствующим лейблом в ином случае.
                    case ViewMode.SectionProductView:
                        treeReturnButton.Visibility = Visibility.Visible;
                        treeReturnLabel.Content = "Return to products";
                        break;
                    case ViewMode.ShoppingCartProductView:
                        treeReturnButton.Visibility = Visibility.Visible;
                        treeReturnLabel.Content = "Return to the shopping cart";
                        break;
                    case ViewMode.OrderProductView:
                        treeReturnButton.Visibility = Visibility.Visible;
                        treeReturnLabel.Content = "Return to orders";
                        break;
                    case ViewMode.ClientOrderView:
                        treeReturnButton.Visibility = Visibility.Visible;
                        treeReturnLabel.Content = "Return to the client list";
                        break;
                    case ViewMode.ActiveOrderView:
                        treeReturnButton.Visibility = Visibility.Visible;
                        treeReturnLabel.Content = "Return to active orders";
                        break;
                }
            }

            // Установка состояния интерфейса в соответствии с активным на данный момент режимом отображения.
            switch (windowDataViewMode)
            {
                // Установка режима отображения None.
                case ViewMode.None:
                    SetNoneViewMode();
                    break;
                // Установка режима отображения SectionProductView.
                case ViewMode.SectionProductView:
                    SetSectionProductViewMode();
                    break;
                // Установка режима отображения ShoppingCartProductView.
                case ViewMode.ShoppingCartProductView:
                    SetShoppingCartProductViewMode();
                    break;
                case ViewMode.OrderProductView:
                    SetOrderProductViewMode(targetClient);
                    break;
                case ViewMode.ClientOrderView:
                    SetClientOrderViewMode();
                    break;
                case ViewMode.ActiveOrderView:
                    SetActiveOrderViewMode();
                    break;
            }
        }

        /// <summary>
        /// Метод установки режима отображения None.
        /// </summary>
        private void SetNoneViewMode()
        {
            // Скрытие элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Hidden;

            // Отключение экранов информации о пользователе и заказе.
            ToggleClientCardScreen(false);
            ToggleOrderDetailsScreen(false);

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();

            // Обнуление ссылок на активные элементы управления.
            activeDataGrid = null;
            activeTreeView = null;
        }

        /// <summary>
        /// Метод установки режима отображения SectionProductView.
        /// </summary>
        private void SetSectionProductViewMode()
        {
            // Скрытие элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Hidden;

            // Отключение экранов информации о пользователе и заказе.
            ToggleClientCardScreen(false);
            ToggleOrderDetailsScreen(false);

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();

            // Вовзрат, если склад пуст.
            if (sections.Count == 0) return;

            // Установка лейбла treeGrid.
            treeLabel.Content = "SECTIONS";

            // Установка активного dataGrid и его источника данных.
            productDataGrid.ItemsSource = products;
            activeDataGrid = productDataGrid;
            activeDataGrid.Margin = new Thickness(activeDataGrid.Margin.Left, activeDataGrid.Margin.Top,
                activeDataGrid.Margin.Right, 0);

            // Установка активного treeView и его отображение.
            if (activeUser is Administrator)
            {
                activeTreeView = sectionTree;
                sectionNoMenuTree.Visibility = Visibility.Hidden;
            }
            else
            {
                activeTreeView = sectionNoMenuTree;
                sectionTree.Visibility = Visibility.Hidden;
            }

            // Запуск анимаций активации элементов.
            TreeViewSlidingInAnimation();
            if (activeTreeView.SelectedItem != null) DataGridOpacityInAnimation();
        }

        /// <summary>
        /// Метод установки режима отображения ShoppingCartProductView.
        /// </summary>
        private void SetShoppingCartProductViewMode()
        {
            // Отображение элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Visible;

            // Отключение экранов информации о пользователе и заказе.
            ToggleClientCardScreen(false);
            ToggleOrderDetailsScreen(false);

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();
            activeTreeView = null;

            // Установка активного dataGrid и его источника данных.
            orderProductDataGrid.ItemsSource = shoppingCartProducts;
            activeDataGrid = orderProductDataGrid;
            activeDataGrid.Margin = new Thickness(activeDataGrid.Margin.Left, activeDataGrid.Margin.Top,
                activeDataGrid.Margin.Right, 40);

            // Запуск анимации активации элементов.
            DataGridOpacityInAnimation();
        }

        /// <summary>
        /// Метод установки режима отображения OrderProductView.
        /// </summary>
        private void SetOrderProductViewMode(Client targetClient)
        {
            // Скрытие элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Hidden;

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();

            // Отключение экрана информации о пользователе.
            ToggleClientCardScreen(false);

            // Установка лейбла treeGrid.
            treeLabel.Content = "ORDERS";

            // Установка активного dataGrid и его источника данных.
            activeDataGrid = orderProductDataGrid;
            activeDataGrid.Margin = new Thickness(activeDataGrid.Margin.Left, activeDataGrid.Margin.Top,
                activeDataGrid.Margin.Right, 0);

            // Установка активного treeView и его отображение.
            activeTreeView = orderTree;
            orderTree.ItemsSource = targetClient == null ? GetAllOrders() : targetClient.Orders;

            // Запуск анимаций активации элементов.
            TreeViewSlidingInAnimation();
            if (activeTreeView.SelectedItem != null)
            {
                targetOrder = activeTreeView.SelectedItem as Order;
                ToggleOrderDetailsScreen(true);
            }
        }

        /// <summary>
        /// Метод установки режима отображения ClientOrderView.
        /// </summary>
        private void SetClientOrderViewMode()
        {
            // Скрытие элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Hidden;

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();

            // Отключение экрана информации о заказе.
            ToggleOrderDetailsScreen(false);

            // Установка лейбла treeGrid.
            treeLabel.Content = "CLIENTS";

            // Обнуление ссылки на активный датагрид.
            activeDataGrid = null;

            // Установка активного treeView и его отображение.
            activeTreeView = clientTree;
            clientTree.ItemsSource = GetAllClients();

            // Запуск анимаций активации элементов.
            TreeViewSlidingInAnimation();
            if (activeTreeView.SelectedItem != null) ToggleClientCardScreen(true, activeTreeView.SelectedItem as Client);
        }

        /// <summary>
        /// Метод установки режима отображения ActiveOrderView.
        /// </summary>
        private void SetActiveOrderViewMode()
        {
            // Скрытие элементов уплавления корзины.
            closeShoppingCartButton.Visibility = Visibility.Hidden;

            // Запуск анимаций деактивации при необходимости.
            if (activeDataGrid != null) DataGridOpacityOutAnimation();
            if (activeTreeView != null) TreeViewSlidingOutAnimation();

            // Отключение экрана информации о пользователе.
            ToggleClientCardScreen(false);

            // Установка лейбла treeGrid.
            treeLabel.Content = "ACTIVE ORDERS";

            // Установка активного dataGrid и его источника данных.
            activeDataGrid = orderProductDataGrid;
            activeDataGrid.Margin = new Thickness(activeDataGrid.Margin.Left, activeDataGrid.Margin.Top,
                activeDataGrid.Margin.Right, 0);

            // Установка активного treeView и его отображение.
            activeTreeView = orderTree;
            orderTree.ItemsSource = GetAllActiveOrders();

            // Запуск анимаций активации элементов.
            TreeViewSlidingInAnimation();
            if (activeTreeView.SelectedItem != null)
            {
                targetOrder = activeTreeView.SelectedItem as Order;
                ToggleOrderDetailsScreen(true);
            }
        }
        #endregion

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
                if (section != (activeTreeView.SelectedItem as Section)) product.IsInSubsection = true;
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

        #region Обработка событий в TreeView и их контекстном меню.

        /// <summary>
        /// Обработчик события нажатия на кнопку возврата к предыдущему режиму отображения окна в treeGrid.
        /// </summary>
        private void treeReturnLabel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Перестановка текущего и прошлого режима отображения местами.
            ViewMode tmp = windowDataViewMode;
            windowDataViewMode = prevWindowDataViewMode;
            prevWindowDataViewMode = tmp;

            // Установка режима отображения окна.
            if (windowDataViewMode == ViewMode.OrderProductView && activeUser is Client) SetWindowViewState(activeUser as Client);
            else SetWindowViewState();
        }

        #region Обработка событий в sectionTree и его контекстном меню.
        /// <summary>
        /// Метод обработки события изменения выбора раздела в sectionTree.
        /// </summary>
        private void sectionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Отображение dataGrid'a в случае, если он скрыт.
            if (activeDataGrid.Visibility == Visibility.Hidden)
            {
                activeDataGrid.Visibility = Visibility.Visible;
                DataGridOpacityInAnimation();
            }

            // Обновление содержания dataGrid'a.
            if (windowDataViewMode == ViewMode.SectionProductView || windowDataViewMode == ViewMode.OrderProductView)
            {
                UpdateProductDataGrid();
            }
        }

        /// <summary>
        /// Метод обработки нажатия кнопки Add Subsection в контекстном меню TreeView.
        /// </summary>
        private void AddSubsectionContextMenu_Click(object sender, RoutedEventArgs e)
        {
            // Закрепление фокуса на выбранном элементе.
            (sender as MenuItem).Focus();

            // Проверка на случай, если не выбран никакой раздел.
            if (activeTreeView.SelectedItem == null)
            {
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса добавления подраздела.
                addingSubsection = true;
                activeTreeView.IsEnabled = false;
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
            activeTreeView.IsEnabled = false;
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

        #region Обработка событий в orderTree.
        /// <summary>
        /// Метод обработки события изменения выбора заказа в orderTree.
        /// </summary>
        private void orderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Определение выбранного заказа.
            Order target = (Order)orderTree.SelectedItem;

            if (target != null)
            {
                // Отображение экрана с информацией о заказе.
                targetOrder = target;
                ToggleOrderDetailsScreen(true);
            }
        }
        #endregion

        #region Обработка событий в clientTree.
        /// <summary>
        /// Метод обработки события изменения выбора клиента в clientTree.
        /// </summary>
        private void clientTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Отображение информации о клиенте.
            if (clientTree.SelectedItem == null)
            {
                ToggleClientCardScreen(false);
            }
            else
            {
                ToggleClientCardScreen(true, clientTree.SelectedItem as Client);
            }
        }
        #endregion

        #endregion

        #region Обработка событий в DataGrid и метод его заполнения.

        /// <summary>
        /// Метод обновления данных dataGrid'a.
        /// </summary>
        private void UpdateProductDataGrid()
        {
            // Обновление productDataGrid'a.
            if (windowDataViewMode == ViewMode.SectionProductView)
            {
                if (activeTreeView.SelectedItem != null)
                {
                    // Перезаполнение списка товаров.
                    products.Clear();
                    AddProductsToList(activeTreeView.SelectedItem as Section);
                    productDataGrid.ItemsSource = products;
                }
                else
                {
                    // Скрытие dataGrid'a в случае отсутствия выбранного раздела.
                    productDataGrid.ItemsSource = null;
                    productDataGrid.Visibility = Visibility.Hidden;
                }
            }
            // Обновление orderProductDataGrid'a для ShoppingCartProductView.
            else if (windowDataViewMode == ViewMode.ShoppingCartProductView)
            {
                // Переинициализация источника данных.
                orderProductDataGrid.ItemsSource = null;
                orderProductDataGrid.ItemsSource = shoppingCartProducts;
            }
            // Обновление orderProductDataGrid'a для OrderProductView.
            else if (windowDataViewMode == ViewMode.OrderProductView)
            {
                if (orderTree.SelectedItem != null)
                {
                    // Инициализация источника данных.
                    orderProductDataGrid.ItemsSource = (orderTree.SelectedItem as Order).Products;
                }
                else
                {
                    // Скрытие dataGrid'a в случае отсутствия выбранного заказа.
                    orderProductDataGrid.ItemsSource = null;
                    orderProductDataGrid.Visibility = Visibility.Hidden;
                }
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

            foreach (var product in (activeTreeView.SelectedItem as Section).Products)
            {
                if (product == target)
                {
                    (activeTreeView.SelectedItem as Section).Products.Remove(product);
                    UpdateProductDataGrid();
                    return;
                }
            }

            foreach (var section in (activeTreeView.SelectedItem as Section).Subsections)
            {
                foreach (var product in section.Products)
                {
                    if (product == target)
                    {
                        (activeTreeView.SelectedItem as Section).Products.Remove(product);
                        UpdateProductDataGrid();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Edit в контекстном меню dataGrid'a.
        /// </summary>
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
            activeTreeView.IsEnabled = false;
            editingMode = true;
            ToggleProductCreationScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Add to order в контекстном меню dataGrid'a.
        /// </summary>
        private void AddToOrder_Click(object sender, RoutedEventArgs e)
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

            // Инициализация нового продукта для заказа на основе данного товара.
            OrderProduct orderProduct = new OrderProduct(target);

            // Увеличение количества товара в корзине, если тот уже в ней лежит.
            foreach (var ordProd in shoppingCartProducts)
            {
                if (ordProd.Code == orderProduct.Code)
                {
                    ordProd.AmountInOrder++;
                    return;
                }
            }

            // Добавление товара в корзину в ином случае.
            shoppingCartProducts.Add(orderProduct);

            UpdateProductDataGrid();
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
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveWarehouseState(saveFileDialog.FileName);
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
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    LoadWarehouseState(openFileDialog.FileName);
                }
                catch (Exception ex)
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

        /// <summary>
        /// Обработчик события нажатия кнопки Go to the products view в меню File.
        /// </summary>
        private void sectionViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Переход к режиму отображения SectionProductView.
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.SectionProductView;
            SetWindowViewState();
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
            if (activeTreeView.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса создания раздела.
                addingSubsection = true;
                activeTreeView.IsEnabled = false;
                ToggleSectionNamingScreen(true);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Edit section в меню Section.
        /// </summary>
        private void EditSection_Click(object sender, RoutedEventArgs e)
        {
            if (activeTreeView.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
                return;
            }

            // Инициация процесса редактирования раздела.
            editingMode = true;
            activeTreeView.IsEnabled = false;
            ToggleSectionNamingScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Delete section в меню Section.
        /// </summary>
        private void DeleteSection_Click(object sender, RoutedEventArgs e)
        {
            if (activeTreeView.SelectedItem == null)
            {
                // Уведомление об ошибке в случае, если не выбран раздел.
                ToggleErrorScreen(true, "No section is selected!");
                return;
            }

            // Определение удаляемого раздела.
            Section target = activeTreeView.SelectedItem as Section;

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
            Product target = productDataGrid.SelectedItem as Product;

            if (target == null)
            {
                // Уведомление об отсутствии выбранного товара.
                ToggleErrorScreen(true, "No product is selected!");
            }
            else
            {
                // Цикл поиска и удаления товара среди товаров выбранного раздела.
                foreach (var product in (activeTreeView.SelectedItem as Section).Products)
                {
                    if (product == target)
                    {
                        (activeTreeView.SelectedItem as Section).Products.Remove(product);

                        // Обновление dataGrid'a.
                        UpdateProductDataGrid();
                        return;
                    }
                }

                // Цикл поиска и удаления товара среди товаров подразделов выбранного раздела.
                foreach (var section in (activeTreeView.SelectedItem as Section).Subsections)
                {
                    foreach (var product in section.Products)
                    {
                        if (product == target)
                        {
                            (activeTreeView.SelectedItem as Section).Products.Remove(product);

                            // Обновление dataGrid'a.
                            UpdateProductDataGrid();
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
            Product target = productDataGrid.SelectedItem as Product;

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
                activeTreeView.IsEnabled = false;
                editingMode = true;
                ToggleProductCreationScreen(true);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Add product в меню Product.
        /// </summary>
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (activeTreeView.SelectedItem == null)
            {
                // Уведомление об отсутствии выбранного раздела.
                ToggleErrorScreen(true, "No section is selected!");
            }
            else
            {
                // Инициация процесса создания товара.
                activeTreeView.IsEnabled = false;
                ToggleProductCreationScreen(true);
            }
        }
        #endregion

        #region Методы обработки событий в меню User.

        /// <summary>
        /// Обработчик события нажатия на кнопку Log in в меню User.
        /// </summary>
        private void logInMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Активация экрана входа.
            ToggleLogInScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Sign in в меню User.
        /// </summary>
        private void signInMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Активация экрана регистрации.
            ToggleSignInScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Log out в меню User.
        /// </summary>
        private void logOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Обнуление активного пользователя и реинициализация интерфейса.
            activeUser = null;
            InitializeUserInterface();
        }

        #endregion

        #region Методы обработки событий в меню Administrator.
        /// <summary>
        /// Обработчик события нажатия на кнопку Client list в меню Administrator.
        /// </summary>
        private void clientListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Установка режима отображения ClientOrderView.
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.ClientOrderView;
            SetWindowViewState();
        }

        /// <summary>
        /// Обработчик события нажатия кнопки Form lacking products report в меню Administrator.
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
                    SaveLackingProductsCSV(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Active orders list в меню Administrator.
        /// </summary>
        private void activeOrdersListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Установка режима отображения ActiveOrderView.
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.ActiveOrderView;
            SetWindowViewState();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Form a client report в меню Administrator.
        /// </summary>
        private void formAClientReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Активация экрана ввода минимальной суммы затрат.
            ToggleCriteriaScreen(true);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Form a defect report в меню Administrator.
        /// </summary>
        private void formADefectReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Активация экрана ввода артикула дефективного товара.
            ToggleDefectScreen(true);
        }
        #endregion

        #region Методы обработки событий в меню Order.

        /// <summary>
        /// Обработчик события нажатия на кнопку Add product to the cart в меню Order.
        /// </summary>
        private void addProductToOrderMenuitem_Click(object sender, RoutedEventArgs e)
        {
            // Определение товара-субъекта.
            Product target = productDataGrid.SelectedItem as Product;

            if (target == null)
            {
                // Уведомление об отсутствии выбранного товара.
                ToggleErrorScreen(true, "No product is selected!");
            }
            else
            {
                // Инициализация нового продукта для заказа на основе данного товара-субъекта.
                OrderProduct orderProduct = new OrderProduct(target);

                // Увеличение количества товара в корзине, если тот уже в ней лежит.
                foreach (var ordProd in shoppingCartProducts)
                {
                    if (ordProd.Code == orderProduct.Code)
                    {
                        ordProd.AmountInOrder++;
                        return;
                    }
                }

                // Добавление товара в корзину в ином случае.
                shoppingCartProducts.Add(orderProduct);
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Shopping cart в меню Order.
        /// </summary>
        private void orderProductListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Установка режима отображения ShoppingCartProductView.
            if (windowDataViewMode == ViewMode.ShoppingCartProductView) return;
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.ShoppingCartProductView;
            SetWindowViewState();
        }

        /// <summary>
        /// Метод проверки на предмет наличия заказа с данным ID.
        /// </summary>
        /// <param name="orderID">ID заказа.</param>
        /// <returns>True, если такой заказ есть, и False в ином случае.</returns>
        private bool OrderIDTaken(int orderID)
        {
            foreach (var order in GetAllOrders()) if (order.OrderNumber == orderID) return true;
            return false;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Form the order в меню Order.
        /// </summary>
        private void formOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Уведомление об ошибке в случае, если корзина пуста.
            if (shoppingCartProducts.Count == 0)
            {
                // Отображение экрана с ошибкой.
                ToggleErrorScreen(true, "The shopping cart is empty!");
                return;
            }

            // Список продуктов для нового заказа.
            List<OrderProduct> orderProducts = new List<OrderProduct>();

            // Копирование продуктов в корзине в список.
            foreach (var prod in shoppingCartProducts)
            {
                Product productCopy = new Product(prod);
                orderProducts.Add(new OrderProduct(productCopy, prod.AmountInOrder));
            }

            // Генерация ID заказа.
            int orderID;
            do orderID = random.Next(1, 2147483647);
            while (OrderIDTaken(orderID));

            // Добавление заказа.
            (activeUser as Client).Orders.Add(new Order(orderID, DateTime.Now, orderProducts, activeUser as Client));

            // Очистка корзины.
            shoppingCartProducts.Clear();

            // Установка режима отображения OrderProductView.
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.OrderProductView;
            SetWindowViewState(activeUser as Client);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Order list в меню Order.
        /// </summary>
        private void orderListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Установка режима отображения OrderProductView.
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.OrderProductView;
            SetWindowViewState(activeUser as Client);
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
            productCreationGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            // Обнуление флагов заполненности полей.
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
            RegisterProductScreenTextboxChanges(0);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для цены товара.
        /// </summary>
        private void productPriceTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterProductScreenTextboxChanges(1);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для названия товара.
        /// </summary>
        private void productNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterProductScreenTextboxChanges(2);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для количества товара.
        /// </summary>
        private void productAmountTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterProductScreenTextboxChanges(3);
        }

        /// <summary>
        /// Обработчик события изменения текста в поле для минимального количества товара.
        /// </summary>
        private void productMinAmountTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация изменений в поле.
            RegisterProductScreenTextboxChanges(4);
        }

        /// <summary>
        /// Метод регистрации изменений в обязательном для заполнения поле.
        /// </summary>
        /// <param name="i">Индекс поля.</param>
        private void RegisterProductScreenTextboxChanges(int i)
        {
            // Регистрация изменения в массиве флагов наличия изменений в полях.
            productFieldsFilled[i] = true;
            if (productFieldsFilled[0] && productFieldsFilled[1] && productFieldsFilled[2] && productFieldsFilled[3] && productFieldsFilled[4])
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
                && int.TryParse(productAmountTextbox.Text, out dummyInt) && int.TryParse(productAmountTextbox.Text, out dummyInt) && int.TryParse(productMinAmountTextbox.Text, out dummyInt)
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
                    (activeTreeView.SelectedItem as Section).AddProduct(new Product(productNameTextbox.Text, productCodeTextbox.Text,
                    decimal.Parse(productPriceTextbox.Text), int.Parse(productAmountTextbox.Text), int.Parse(productMinAmountTextbox.Text),
                    productDescTextbox.Text, productImageBox.Source)
                    { ParentSection = activeTreeView.SelectedItem as Section });
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
                    activeTreeView.IsEnabled = true;
                }

                ToggleProductCreationScreen(false);
                activeTreeView.IsEnabled = true;
            }
            // Вывод сообщения об ошибке при обнаружении неверных значений полей.
            else
            {
                productInputLabelError.Content = "Error: invalid values detected, please try again";
                productInputLabelError.Visibility = Visibility.Visible;
            }

            // Обновление dataGrid'a.
            UpdateProductDataGrid();
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
            sectionCreationGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
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
                        if (sec == activeTreeView.SelectedItem as Section)
                        {
                            // Внесение изменений.
                            sec.Name = sectionNameTextbox.Text;
                            break;
                        }

                        foreach (var subsec in sec.Subsections)
                        {
                            if (subsec == activeTreeView.SelectedItem as Section)
                            {
                                // Внесение изменений.
                                subsec.Name = sectionNameTextbox.Text;
                                break;
                            }
                        }
                    }

                    // Отключение режима редактирования.
                    editingMode = false;
                    activeTreeView.IsEnabled = true;
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
                foreach (var subsec in (activeTreeView.SelectedItem as Section).Subsections)
                {
                    if (subsec.Name == sectionNameTextbox.Text)
                    {
                        sectionInputLabelError.Visibility = Visibility.Visible;
                        return;
                    }
                }

                // Создание подраздела.
                (activeTreeView.SelectedItem as Section).AddSubsection(new Section(sectionNameTextbox.Text) { parentSection = sectionTree.SelectedItem as Section });

                addingSubsection = false;
                activeTreeView.IsEnabled = true;
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
            confirmationScreenGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            confirmationScreenGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку подтверждения удаления раздела.
        /// </summary>
        private void confimationButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Определение удаляемого раздела и его удаление.
            Section target = activeTreeView.SelectedItem as Section;
            if (target.parentSection != null)
            {
                foreach (Section section in sections)
                {
                    section.RemoveSubsection(target);
                }
            }
            else
            {
                sections.Remove(activeTreeView.SelectedItem as Section);
            }

            // Запуск анимации скрытия TreeView, если разделов не осталось.
            if (sections.Count == 0) TreeViewSlidingOutAnimation();

            // Обновление dataGrid'a.
            UpdateProductDataGrid();
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
            errorScreenGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
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
            warehouseGenerationGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
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
            prevWindowDataViewMode = windowDataViewMode;
            windowDataViewMode = ViewMode.SectionProductView;
            SetWindowViewState();
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

        #region Методы экрана авторизации.

        /// <summary>
        /// Метод активации экрана авторизации.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleLogInScreen(bool toggleOn)
        {
            // Установка размеров и видимости экрана.
            logInGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            logInGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            logInErrorLabel.Visibility = Visibility.Hidden;
            logInButton.Visibility = Visibility.Hidden;
            // Установка значений по умолчанию.
            logInErrorLabel.Content = string.Empty;
            eMailTextBox.Text = string.Empty;
            passwordTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Метод проверки строки на предмет того, содержится ли в ней адрес электронной почты.
        /// </summary>
        /// <param name="email">Строка.</param>
        /// <returns>True, если строка представляет собой адрес электронной почты.</returns>
        private bool IsValidEmail(string email)
        {
            // Проверка при помощи System.Net.Mail.
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку входа.
        /// </summary>
        private void logInButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Вывод уведомления об ошибке в случае, если введенный адрес электорнной почты некорректен.
            if (!IsValidEmail(eMailTextBox.Text))
            {
                logInErrorLabel.Visibility = Visibility.Visible;
                logInErrorLabel.Content = "Error: the e-mail address is incorrect, please try again";
                return;
            }

            // Вывод уведомления об ошибке в случае, если не был введён пароль.
            if (passwordTextBox.Text == string.Empty)
            {
                logInErrorLabel.Visibility = Visibility.Visible;
                logInErrorLabel.Content = "Error: please input a password";
                return;
            }

            // Проверка на корректность введённых данных.
            foreach (var user in users)
            {
                // Вход в систему, если данные верны.
                if (user.CredentialsCorrect(eMailTextBox.Text, passwordTextBox.Text))
                {
                    activeUser = user;

                    // Инициализация интерфейса.
                    ToggleLogInScreen(false);
                    InitializeUserInterface();
                    SetWindowViewState();
                    return;
                }
            }

            // Отображение уведомления об ошибке в случае, если пользователь с такими данными не найден.
            logInErrorLabel.Visibility = Visibility.Visible;
            logInErrorLabel.Content = "Error: user with such credentials not found";
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку закрытия экрана авторизации.
        /// </summary>
        private void logInCloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Отключение экрана авторизации.
            ToggleLogInScreen(false);
        }

        /// <summary>
        /// Обработчик события изменения текста в eMailTextBox.
        /// </summary>
        private void eMailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Отображение кнопки входа.
            logInButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработчик события изменения текста в passwordTextBox.
        /// </summary>
        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Отображение кнопки входа.
            logInButton.Visibility = Visibility.Visible;
        }

        #endregion

        #region Методы экрана регистрации клиента.

        /// <summary>
        /// Метод активации экрана регистрации.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleSignInScreen(bool toggleOn)
        {
            // Установка размеров и видимости экрана.
            signInGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            signInGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            signInErrorLabel.Visibility = Visibility.Hidden;
            signInButton.Visibility = Visibility.Hidden;
            // Обнуление флагов заполненности полей.
            signInFieldsFilled = new bool[] { false, false, false, false };
            // Установка значений по умолчанию.
            logInErrorLabel.Content = string.Empty;
            eMailSignInTextBox.Text = string.Empty;
            passwordSignInTextBox.Text = string.Empty;
            lastNameSignInTextBox.Text = string.Empty;
            nameSignInTextBox.Text = string.Empty;
            patronymicSignInTextBox.Text = string.Empty;
            addressSignInTextBox.Text = string.Empty;
            phoneNumberSignInTextBox.Text = string.Empty;
        }

        /// <summary>
        /// Обработчик события изменения текста в lastNameSignInTextBox.
        /// </summary>
        private void lastNameSignInTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация наличия изменений.
            RegisterSignInScreenTextboxChanges(0);
        }

        /// <summary>
        /// Обработчик события изменения текста в nameSignInTextBox.
        /// </summary>
        private void nameSignInTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация наличия изменений.
            RegisterSignInScreenTextboxChanges(1);
        }

        /// <summary>
        /// Обработчик события изменения текста в eMailSignInTextBox.
        /// </summary>
        private void eMailSignInTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация наличия изменений.
            RegisterSignInScreenTextboxChanges(2);
        }

        /// <summary>
        /// Обработчик события изменения текста в passwordSignInTextBox.
        /// </summary>
        private void passwordSignInTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Регистрация наличия изменений.
            RegisterSignInScreenTextboxChanges(3);
        }

        /// <summary>
        /// Метод регистрации изменений в обязательном для заполнения поле.
        /// </summary>
        /// <param name="i">Индекс поля.</param>
        private void RegisterSignInScreenTextboxChanges(int i)
        {
            // Регистрация изменения в массиве флагов наличия изменений в полях.
            signInFieldsFilled[i] = true;
            if (signInFieldsFilled[0] && signInFieldsFilled[1] && signInFieldsFilled[2] && signInFieldsFilled[3])
                signInButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Метод обработки события нажатия на кнопку регистрации.
        /// </summary>
        private void signInButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если все необходимые строки непусты.
            if (lastNameSignInTextBox.Text != string.Empty && nameSignInTextBox.Text != string.Empty &&
                eMailSignInTextBox.Text != string.Empty && passwordSignInTextBox.Text != string.Empty)
            {
                long dummy;
                // Уведомление об ошибке в случае, если номер телефона не является корректным.
                if (phoneNumberSignInTextBox.Text != string.Empty && !long.TryParse(phoneNumberSignInTextBox.Text, out dummy))
                {
                    signInErrorLabel.Visibility = Visibility.Visible;
                    signInErrorLabel.Content = "Error: the phone number is not valid, please try again";
                    return;
                }

                // Уведомление об ошибке в случае, если адрес электронной почты не является корректным.
                if (!IsValidEmail(eMailSignInTextBox.Text))
                {
                    signInErrorLabel.Visibility = Visibility.Visible;
                    signInErrorLabel.Content = "Error: the e-mail address is not valid, please try again";
                    return;
                }

                // Уведомление об ошибке в случае, если адрес электронной почты уже занят.
                foreach (var user in users)
                {
                    if (user.EMailAddress == eMailSignInTextBox.Text)
                    {
                        signInErrorLabel.Visibility = Visibility.Visible;
                        signInErrorLabel.Content = "Error: user with this e-mail address already exists, please try to log in";
                        return;
                    }
                }

                // Регистрация нового клиента.
                Client client = new Client(eMailSignInTextBox.Text, passwordSignInTextBox.Text, lastNameSignInTextBox.Text, nameSignInTextBox.Text,
                    phoneNumberSignInTextBox.Text, addressSignInTextBox.Text, patronymicSignInTextBox.Text);
                users.Add(client);
                activeUser = client;

                // Инициализация интерфейса.
                InitializeUserInterface();
                ToggleSignInScreen(false);
            } else
            {
                // Уведомление об ошибке в случае, если не были заполнены обязательные поля.
                signInErrorLabel.Visibility = Visibility.Visible;
                signInErrorLabel.Content = "Error: some of the required information is missing, please try again";
                return;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку отмены регистрации.
        /// </summary>
        private void signInCloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана регистрации.
            ToggleSignInScreen(false);
        }

        #endregion

        #region Методы экрана информации о клиенте.

        /// <summary>
        /// Метод активации экрана информации о клиенте.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleClientCardScreen(bool toggleOn, Client client = null)
        {
            // Установка размеров и видимости экрана.
            clientCardGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            clientCardGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            // Установка значений в соответствии с информацией о клиенте.
            if (client != null)
            {
                // Заполнение лейблов информацией о клиенте.
                clientNameLabel.Content = client.FullName;
                clientEMailLabel.Content = client.EMailAddress;
                clientHomeAddressLabel.Content = client.Address == string.Empty ? "None" : client.Address;
                clientPhoneNumberLabel.Content = client.PhoneNumber == string.Empty ? "None" : client.PhoneNumber;
                clientOrdersDataGrid.ItemsSource = client.Orders;
            }
            // Установка значений по умолчанию.
            else
            {
                clientNameLabel.Content = string.Empty;
                clientEMailLabel.Content = string.Empty;
                clientHomeAddressLabel.Content = string.Empty;
                clientPhoneNumberLabel.Content = string.Empty;
                clientOrdersDataGrid.ItemsSource = null;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку закрытия экрана с информацией о клиенте.
        /// </summary>
        private void clientCardCloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleClientCardScreen(false);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку Order details в контекстном меню таблицы заказов клиента.
        /// </summary>
        private void orderDetailsContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получения menuItem'a, из которого было вызвано событие.
            MenuItem menuItem = (MenuItem)sender;

            // Получение контекстного меню, к которому принадлежит MenuItem.
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;

            // Получение dataGrid'а, из которого было вызвано меню.
            DataGrid item = (DataGrid)contextMenu.PlacementTarget;

            if (item.SelectedCells.Count == 0) return;

            // Получение контекста данных выбранного элемента dataGrid'a (заказа).
            targetOrder = (Order)item.SelectedCells[0].Item;

            // Отображение информации о заказе.
            ToggleOrderDetailsScreen(true);
        }

        #endregion

        #region Методы экрана взаимодействия с заказом.

        /// <summary>
        /// Метод активации экрана взаимодействия с заказом.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleOrderDetailsScreen(bool toggleOn)
        {
            // Установка размеров и видимости экрана.
            orderDetailsGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            orderDetailsGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;

            // Установка значений в случае, если заказ выбран.
            if (targetOrder != null)
            {
                // Отображение базовой информации о товаре.
                orderNumberLabel.Content = targetOrder.OrderNumber;
                orderTimeLabel.Content = targetOrder.OrderTime;

                // В случае, если активный пользователь - администратор.
                if (activeUser is Administrator)
                {
                    // Установка видимости элементов экрана.
                    orderStatusLabelPanel.Visibility = Visibility.Collapsed;
                    statusLabelPanel.Visibility = Visibility.Collapsed;
                    statusLabelPanel.Visibility = Visibility.Visible;
                    statusPanel.Visibility = Visibility.Visible;
                    orderGridCloseButton.Visibility = windowDataViewMode == ViewMode.ActiveOrderView ? Visibility.Hidden : Visibility.Visible;
                    buttonsPanel.Visibility = Visibility.Collapsed;
                    orderClientLabelPanel.Visibility = Visibility.Visible;
                    orderErrorLabel.BeginAnimation(OpacityProperty, null);

                    // Установка значений для checkBox'ов, отображающих статус заказа.
                    processedCheckBox.IsChecked = (targetOrder.Status & Status.Processed) == Status.Processed;
                    processedCheckBox.IsEnabled = !((targetOrder.Status & Status.Paid) == Status.Paid);
                    paidCheckBox.IsChecked = (targetOrder.Status & Status.Paid) == Status.Paid;
                    shippedCheckBox.IsChecked = (targetOrder.Status & Status.Shipped) == Status.Shipped;
                    shippedCheckBox.IsEnabled = (targetOrder.Status & Status.Paid) == Status.Paid;
                    executedCheckBox.IsChecked = (targetOrder.Status & Status.Executed) == Status.Executed;
                    executedCheckBox.IsEnabled = (targetOrder.Status & Status.Paid) == Status.Paid;

                    // Отображение имени заказчика.
                    orderClientLabel.Content = targetOrder.Client.FullName;

                    if (sections.Count != 0)
                    {
                        try
                        {
                            // Проверка наличия товаров на складе в случае, если склад был загружен.
                            OrderProductCheck();
                            orderErrorLabel.Visibility = Visibility.Collapsed;
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Уведомление в случае нехватки товара на складе.
                            orderErrorLabel.Visibility = Visibility.Visible;
                            orderErrorLabel.Content = ex.Message;
                        }
                    }
                    else orderErrorLabel.Visibility = Visibility.Collapsed;
                }
                // В случае, если активный пользователь - клиент.
                else if (activeUser is Client)
                {
                    // Установка видимости элементов экрана.
                    orderStatusLabelPanel.Visibility = Visibility.Visible;
                    statusLabelPanel.Visibility = Visibility.Collapsed;
                    statusPanel.Visibility = Visibility.Collapsed;
                    orderClientLabelPanel.Visibility = Visibility.Collapsed;
                    orderErrorLabel.Visibility = Visibility.Collapsed;
                    orderGridCloseButton.Visibility = Visibility.Hidden;

                    // Отображение статуса заказа.
                    orderStatusLabel.Content = targetOrder.Status;

                    // Скрытие кнопок оплаты и отмены в случае, если заказ уже был оплачен.
                    if ((targetOrder.Status & Status.Paid) == Status.Paid)
                    {
                        buttonsPanel.Visibility = Visibility.Collapsed;
                    }
                    // Отображение кнопок в обратном случае.
                    else
                    {
                        buttonsPanel.BeginAnimation(OpacityProperty, null);
                        buttonsPanel.Visibility = Visibility.Visible;

                        // Активация кнопки покупки в случае, если заказ был обработан.
                        if ((targetOrder.Status & Status.Processed) == Status.Processed)
                        {
                            purchaseButton.IsEnabled = true;
                            purchaseButtonCover.IsEnabled = true;
                        }
                        // Деактивация в обратном случае.
                        else
                        {
                            purchaseButton.IsEnabled = false;
                            purchaseButtonCover.IsEnabled = false;
                        }
                    }
                }

                // Расчёт и отображение суммы стоимостей товаров.
                decimal sum = 0;
                foreach (var prod in targetOrder.Products) sum += prod.Price;
                totalLabel.Content = Math.Round(sum, 2);
                
                // Скрытие изображений.
                purchaseConfirmedImage.Visibility = Visibility.Hidden;
                purchaseFailedImage.Visibility = Visibility.Hidden;

                // Установка источников данных таблицы продуктов в заказе.
                orderDetailsProductsDataGrid.ItemsSource = targetOrder.Products;
            }
            // Установка значений по умолчанию.
            else
            {
                orderNumberLabel.Content = string.Empty;
                orderTimeLabel.Content = string.Empty;
                orderStatusLabel.Content = string.Empty;

                statusLabelPanel.Visibility = Visibility.Collapsed;
                statusPanel.Visibility = Visibility.Collapsed;
                buttonsPanel.Visibility = Visibility.Collapsed;
                orderErrorLabel.BeginAnimation(OpacityProperty, null);
                orderErrorLabel.Visibility = Visibility.Collapsed;

                processedCheckBox.IsChecked = false;
                paidCheckBox.IsChecked = false;
                shippedCheckBox.IsChecked = false;
                executedCheckBox.IsChecked = false;

                orderDetailsProductsDataGrid.ItemsSource = null;
            }
        }

        /// <summary>
        /// Метод проверки товаров заказа, с которым проводится взаимодействие.
        /// </summary>
        private void OrderProductCheck()
        {
            // Массив флагов - true, если продукт найден и его количества на складе хватает.
            bool[] productFoundFlags = new bool[targetOrder.Products.Count];

            // Окончание, примыкаемое к сообщению об ошибке.
            string suffix = activeUser is Administrator ? "" : ", please contact the administrator";

            // Цикл поиска товаров.
            for (int i = 0; i < targetOrder.Products.Count; i++)
            {
                foreach (var section in sections)
                {
                    foreach (var product in section.Products)
                    {
                        if (product.Code == targetOrder.Products[i].Code)
                        {
                            // Установка флага в положение true, если продукт найден и его количества на складе хватает.
                            if (product.Amount > targetOrder.Products[i].AmountInOrder)
                            {
                                product.Amount--;
                                productFoundFlags[i] = true;
                                break;
                            }
                            // Уведомление об ошибке, связанной с нехваткой товара на складе.
                            else
                            {
                                throw new InvalidOperationException($"Error: insufficient amount of a product {targetOrder.Products[i].Code}{suffix}");
                            }
                        }
                    }
                    if (productFoundFlags[i]) break;
                }

                // Уведомление об ошибке, связанной с отсутствием товара на складе.
                if (!productFoundFlags[i])
                {
                    throw new InvalidOperationException($"Error: a product {targetOrder.Products[i].Code} is missing{suffix}");
                }
            }
        }

        /// <summary>
        /// Обработчик события установки галочки на processedCheckBox.
        /// </summary>
        private void processedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Добавление статуса Processed.
            targetOrder.AddStatus(Status.Processed);
        }

        /// <summary>
        /// Обработчик события установки галочки на shippedCheckBox.
        /// </summary>
        private void shippedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Добавление статуса Shipped.
            targetOrder.AddStatus(Status.Shipped);
        }

        /// <summary>
        /// Обработчик события установки галочки на executedCheckBox.
        /// </summary>
        private void executedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Добавление статуса Executed.
            targetOrder.AddStatus(Status.Executed);
        }

        /// <summary>
        /// Обработчик события снятия галочки с processedCheckBox.
        /// </summary>
        private void processedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Удаление статуса Processed.
            targetOrder.RemoveStatus(Status.Processed);
        }

        /// <summary>
        /// Обработчик события снятия галочки с shippedCheckBox.
        /// </summary>
        private void shippedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Удаление статуса Shipped.
            targetOrder.RemoveStatus(Status.Shipped);
        }

        /// <summary>
        /// Обработчик события снятия галочки с executedCheckBox.
        /// </summary>
        private void executedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Удаление статуса Executed.
            targetOrder.RemoveStatus(Status.Executed);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку закрытия экрана с информацией о заказе.
        /// </summary>
        private void orderGridCloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleOrderDetailsScreen(false);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку оплаты.
        /// </summary>
        private void purchaseButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Если склад непуст.
            if (sections.Count != 0)
            {
                try
                {
                    // Запуск проверки товаров заказа.
                    OrderProductCheck();
                    // Запуск анимации успешной оплаты.
                    PurchaseConfirmedAnimation();

                    // Добавление статуса Paid.
                    targetOrder.AddStatus(Status.Paid);
                    orderStatusLabel.Content = targetOrder.Status;
                }
                // Уведомление об ошибке.
                catch (InvalidOperationException ex)
                {
                    orderErrorLabel.Content = ex.Message;
                    PurchaseFailedAnimation();
                }
            }
            // Если склад пуст (без проверки).
            else
            {
                // Запуск анимации успешной оплаты.
                PurchaseConfirmedAnimation();

                // Добавление статуса Paid.
                targetOrder.AddStatus(Status.Paid);
                orderStatusLabel.Content = targetOrder.Status;
            }
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку отмены заказа.
        /// </summary>
        private void cancelOrderButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Удаление заказа и деактивация экрана.
            (activeUser as Client).Orders.Remove(targetOrder);
            ToggleOrderDetailsScreen(false);
        }

        #endregion

        #region Методы корзины клиента.
        /// <summary>
        /// Обработчик нажатия на кнопку выхода из корзины.
        /// </summary>
        private void closeShoppingCartButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Вовзрат к предыдущему режиму отображения.
            windowDataViewMode = prevWindowDataViewMode;
            prevWindowDataViewMode = ViewMode.ShoppingCartProductView;
            SetWindowViewState();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку Delete from order в контекстном меню таблицы в корзине.
        /// </summary>
        private void DeleteFromOrder_Click(object sender, RoutedEventArgs e)
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

            // Поиск товара по артикулу и его удаление из корзины.
            for (int i = 0; i < shoppingCartProducts.Count; i++)
            {
                if (target.Code == shoppingCartProducts[i].Code)
                {
                    shoppingCartProducts.Remove(shoppingCartProducts[i]);
                    break;
                }
            }
        }

        #endregion

        #region Методы экрана указания наименьшей суммы потраченный средств для CSV отчёта.

        /// <summary>
        /// Метод активации экрана экрана указания наименьшей суммы потраченный средств для CSV отчёта.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleCriteriaScreen(bool toggleOn)
        {
            // Установка размера экрана.
            criteriaInputGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            criteriaInputGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            criteriaLabelError.Visibility = Visibility.Hidden;
            // Установка значения по умолчанию.
            criteriaTextbox.Text = string.Empty;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку применения введенного фильтра и формирования отчёта.
        /// </summary>
        private void criteriaApplyButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            decimal dummy;
            // Уведомление об ошибке в случае, если была введена некорректная стоимость.
            if (!decimal.TryParse(criteriaTextbox.Text, out dummy))
            {
                criteriaLabelError.Visibility = Visibility.Visible;
                criteriaLabelError.Content = "Error: invalid number entered, please try again";
                return;
            }

            // Инициация окна выбора пути, в котором будет сохранён файл-отчёт.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            saveFileDialog.Title = "Save a CSV report...";

            // Сохранение файла.
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveClientsByCriterionCSV(saveFileDialog.FileName, decimal.Parse(criteriaTextbox.Text));
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
                ToggleCriteriaScreen(false);
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку закрытия экрана ввода.
        /// </summary>
        private void criteriaCancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleCriteriaScreen(false);
        }

        #endregion

        #region Методы экрана указания артикула дефективного товара.

        /// <summary>
        /// Метод активации экрана экрана указания артикула дефективного товара.
        /// </summary>
        /// <param name="toggleOn">Булева метка - если true, то экран активируется; если false, то деактивируется.</param>
        private void ToggleDefectScreen(bool toggleOn)
        {
            // Установка размера экрана.
            defectInputGrid.Margin = new Thickness(treeGrid.Width, 30, 0, 0);
            defectInputGrid.Visibility = toggleOn ? Visibility.Visible : Visibility.Hidden;
            defectLabelError.Visibility = Visibility.Hidden;
            // Установка значения по умолчанию.
            defectTextbox.Text = string.Empty;
        }

        /// <summary>
        /// Обработчик нажатия на кнопку применения введенного фильтра и формирования отчёта.
        /// </summary>
        private void defectApplyButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            bool productIsPresent = false;

            // Получение списка товаров в заказах.
            var productsInOrders = from user in users
                                   where user is Client
                                   from order in (user as Client).Orders
                                   from product in order.Products
                                   select product;

            // Проверка на наличие товара с указанным артикулом в заказах.
            foreach (var product in productsInOrders)
            {
                if (product.Code == defectTextbox.Text) productIsPresent = true;
            }

            // Уведомление об отсутствии указанного товара в заказах.
            if (!productIsPresent)
            {
                defectLabelError.Visibility = Visibility.Visible;
                defectLabelError.Content = "Error: product with such a code wasn't found in any order";
                return;
            }

            // Инициация окна выбора пути, в котором будет сохранён файл-отчёт.
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            saveFileDialog.Title = "Save a CSV report...";

            // Сохранение файла.
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    SaveAffectedOrdersCSV(saveFileDialog.FileName, defectTextbox.Text);
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
                ToggleDefectScreen(false);
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку закрытия экрана указания артикула дефективного товара.
        /// </summary>
        private void defectCancelButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Деактивация экрана.
            ToggleDefectScreen(false);
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
                SaveWarehouseState($"Autosaves{Path.DirectorySeparatorChar}warehouseAutosave_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{DateTime.Now.ToString("HH-mm-ss")}.xml");
                SaveUsers();
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
        private void SaveWarehouseState(string filename)
        {
            // Инициализация сериалайзера.
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Section[]), new Type[] { typeof(Product) });

            try
            {
                // Сериализация и сохранение состояния склада в файле формата XML.
                using (Stream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    dataContractSerializer.WriteObject(fs, sections.ToArray());
                }
            } catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод загрузки состояния склада из файла.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        private void LoadWarehouseState(string filename)
        {
            // Предварительная очистка источников данных.
            sections.Clear();
            products.Clear();

            // Инициализация сериалайзера.
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(Section[]), new Type[] { typeof(Product) });

            try
            {
                // Чтение файла через поток.
                using (Stream fs = File.OpenRead(filename))
                {
                    // Десериализация массива разделов с товарами.
                    Section[] sectionsArr = (Section[])dataContractSerializer.ReadObject(fs);

                    // Заполнение источников данных.
                    if (sectionsArr.Length != 0)
                    {
                        foreach (var section in sectionsArr) sections.Add(section);

                        // Установка родительских разделов для всех разделов склада.
                        foreach (var section in sections) section.SetChildrenParentSection();

                        // Установка режима отображения.
                        prevWindowDataViewMode = windowDataViewMode;
                        windowDataViewMode = ViewMode.SectionProductView;
                        SetWindowViewState();
                    }
                }
            } catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод сохранения списка недостающих товаров в CSV-файл.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        private void SaveLackingProductsCSV(string filename)
        {
            // Получение списка недостающих товаров.
            List<Product> lackingProducts = new List<Product>();
            foreach (var section in sections) section.GetLackingProducts(ref lackingProducts);

            try
            {
                // Запись списка в CSV-файл при помощи CSVHelper.
                using (var writer = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ProductMap>();
                    csv.WriteRecords(lackingProducts);
                }
            } catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод сохранения CSV-списка клиентов, потративших на заказы сумму, превышающую данную.
        /// </summary>
        /// <param name="filename">Путь, в котором сохраняется файл.</param>
        /// <param name="minAmount">Минимальная сумма.</param>
        private void SaveClientsByCriterionCSV(string filename, decimal minAmount)
        {
            // Экспортируемый список клиентов.
            List<Client> clients = new List<Client>();

            // Заполнение списка.
            foreach (var user in users)
            {
                if (user is Client)
                {
                    if ((user as Client).SpentOnOrders >= minAmount) clients.Add(user as Client);
                }
            }

            // Сортировка списка по потраченной сумме.
            Client[] clientArray = clients.ToArray();
            Array.Sort(clientArray, delegate (Client x, Client y) { return -x.SpentOnOrders.CompareTo(y.SpentOnOrders); });

            try
            {
                // Запись списка в CSV-файл при помощи CSVHelper.
                using (var writer = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ClientExpenditureReportMap>();
                    csv.WriteRecords(clientArray);
                }
            }
            catch (Exception ex) 
            { 
                ToggleErrorScreen(true, ex.Message); 
            }
        }

        /// <summary>
        /// Метод сохранения CSV-списка заказов, содержащих дефективный товар.
        /// </summary>
        /// <param name="filename">Путь, в котором сохраняется файл.</param>
        /// <param name="productCode">Артикул дефективного товара.</param>
        private void SaveAffectedOrdersCSV(string filename, string productCode)
        {
            // Экспортируемый список клиентов.
            List<Order> affectedOrders = new List<Order>();

            // Заполнение списка.
            foreach (var user in users)
            {
                if (user is Client)
                {
                    foreach (var order in (user as Client).Orders)
                    {
                        foreach (var product in order.Products) if (product.Code == productCode) affectedOrders.Add(order);
                    }
                }
            }

            try
            {
                // Запись списка в CSV-файл при помощи CSVHelper.
                using (var writer = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<DefectOrderReportMap>();
                    csv.WriteRecords(affectedOrders);
                }
            }
            catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод сохранения списка пользователей программы в файл.
        /// </summary>
        private void SaveUsers()
        {
            // Инициализация сериалайзера.
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(User[]), new Type[] { typeof(Client), typeof(Administrator), typeof(Order) });

            try
            {
                // Сохранение списка пользователей.
                if (!Directory.Exists("Users")) Directory.CreateDirectory("Users");
                using (Stream fs = new FileStream($"Users{Path.DirectorySeparatorChar}userList.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    dataContractSerializer.WriteObject(fs, users.ToArray());
                }
            }
            catch (Exception ex)
            {
                ToggleErrorScreen(true, ex.Message);
            }
        }

        /// <summary>
        /// Метод загрузки списка пользователей программы.
        /// </summary>
        private void LoadUserList()
        {
            // Создание директории для файлов с информацией о пользователях если такой не существует.
            if (!Directory.Exists("Users")) Directory.CreateDirectory("Users");

            // Инициализация сериалайзера.
            DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(User[]), new Type[] { typeof(Client), typeof(Administrator), typeof(Order) });

            // Загрузка списка пользователей из файла, если он существует.
            if (File.Exists($"Users{Path.DirectorySeparatorChar}userList.xml"))
            {
                try
                {
                    // Загрузка (десериализация) списка.
                    using (Stream fs = File.OpenRead($"Users{Path.DirectorySeparatorChar}userList.xml"))
                    {
                        users = new List<User>((User[])dataContractSerializer.ReadObject(fs));
                    }
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
            }
            // Создание и сохранение списка пользователей в файл, если он не существует.
            else
            {
                // Создание учётной записи администратора.
                Administrator adminUser = new Administrator("admin@avgshop.co.uk", "admpass", "Wallace", "William");
                users.Add(adminUser);

                try
                {
                    // Сохранение списка пользователей в файл.
                    using (Stream fs = new FileStream($"Users{Path.DirectorySeparatorChar}userList.xml", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        dataContractSerializer.WriteObject(fs, users.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    ToggleErrorScreen(true, ex.Message);
                }
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
            activeDataGrid.Visibility = Visibility.Visible;
            // Инициализация анимации.
            DoubleAnimation dataGridOpacityAnimation = new DoubleAnimation();
            dataGridOpacityAnimation.From = 0;
            dataGridOpacityAnimation.To = 1;
            dataGridOpacityAnimation.Duration = TimeSpan.FromSeconds(0.5);
            // Запуск анимации.
            activeDataGrid.BeginAnimation(DataGrid.OpacityProperty, dataGridOpacityAnimation);
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
            activeDataGrid.BeginAnimation(DataGrid.OpacityProperty, dataGridOpacityAnimation);
            activeDataGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Метод запуска анимации постепенного появления treeView.
        /// </summary>
        private void TreeViewSlidingInAnimation()
        {
            activeTreeView.Visibility = Visibility.Visible;
            // Анимация sectionTree.
            DoubleAnimation treeAnimation = new DoubleAnimation();
            treeAnimation.From = 0;
            treeAnimation.To = 193;
            treeAnimation.Duration = TimeSpan.FromSeconds(0.5);
            treeGrid.BeginAnimation(Grid.WidthProperty, treeAnimation);

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

            if (activeDataGrid != null)
            {
                ThicknessAnimation dataGridAnimation = new ThicknessAnimation();
                dataGridAnimation.From = new Thickness(0, 30, 0, activeDataGrid.Margin.Bottom);
                dataGridAnimation.To = new Thickness(193, 30, 0, activeDataGrid.Margin.Bottom);
                dataGridAnimation.Duration = TimeSpan.FromSeconds(0.5);
                activeDataGrid.BeginAnimation(DataGrid.MarginProperty, dataGridAnimation);
            }
        }

        /// <summary>
        /// Метод запуска анимации постепенного исчезновения treeView.
        /// </summary>
        private void TreeViewSlidingOutAnimation()
        {
            // Анимация sectionTree.
            DoubleAnimation treeAnimation = new DoubleAnimation();
            treeAnimation.From = 193;
            treeAnimation.To = 0;
            treeAnimation.Duration = TimeSpan.FromSeconds(0.5);
            treeGrid.BeginAnimation(Grid.WidthProperty, treeAnimation);

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

            if (activeDataGrid != null)
            {
                ThicknessAnimation dataGridAnimation = new ThicknessAnimation();
                dataGridAnimation.From = new Thickness(193, 30, 0, activeDataGrid.Margin.Bottom);
                dataGridAnimation.To = new Thickness(0, 30, 0, activeDataGrid.Margin.Bottom);
                dataGridAnimation.Duration = TimeSpan.FromSeconds(0.5);
                activeDataGrid.BeginAnimation(DataGrid.MarginProperty, dataGridAnimation);
            }

            activeTreeView.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Анимация, запускаемая при успешной оплате.
        /// </summary>
        private void PurchaseConfirmedAnimation()
        {
            // Анимация постепенного исчезновения панели кнопок.
            DoubleAnimation buttonFadeoutAnimation = new DoubleAnimation();
            buttonFadeoutAnimation.From = 1;
            buttonFadeoutAnimation.To = 0;
            buttonFadeoutAnimation.Duration = TimeSpan.FromSeconds(0.5);

            purchaseConfirmedImage.Visibility = Visibility.Visible;
            purchaseConfirmedImage.Opacity = 0;

            // Анимация постепенного появления изображения.
            DoubleAnimation imageFadeinAnimation = new DoubleAnimation();
            imageFadeinAnimation.From = 0;
            imageFadeinAnimation.To = 1;
            imageFadeinAnimation.Duration = TimeSpan.FromSeconds(0.5);

            // Анимация постепенного исчезновения изображения.
            DoubleAnimation imageFadeoutAnimation = new DoubleAnimation();
            imageFadeoutAnimation.From = 1;
            imageFadeoutAnimation.To = 0;
            imageFadeoutAnimation.Duration = TimeSpan.FromSeconds(0.5);

            buttonFadeoutAnimation.Completed += (s, e) =>
            {
                // Запуск постепенного появления изображения.
                purchaseConfirmedImage.BeginAnimation(Image.OpacityProperty, imageFadeinAnimation);
            };

            imageFadeinAnimation.Completed += (s, e) =>
            {
                // Запуск постепенного исчезновения изображения.
                purchaseConfirmedImage.BeginAnimation(Image.OpacityProperty, imageFadeoutAnimation);
            };

            imageFadeoutAnimation.Completed += (s, e) =>
            {
                // Отключение видимости обоих элементов.
                buttonsPanel.Visibility = Visibility.Collapsed;
                purchaseConfirmedImage.Visibility = Visibility.Hidden;
            };

            // Запуск анимации.
            buttonsPanel.BeginAnimation(OpacityProperty, buttonFadeoutAnimation);
        }

        /// <summary>
        /// Анимация, запускаемая при неуспешной оплате.
        /// </summary>
        private void PurchaseFailedAnimation()
        {
            // Анимация постепенного исчезновения панели кнопок.
            DoubleAnimation buttonFadeoutAnimation = new DoubleAnimation();
            buttonFadeoutAnimation.From = 1;
            buttonFadeoutAnimation.To = 0;
            buttonFadeoutAnimation.Duration = TimeSpan.FromSeconds(0.5);

            purchaseFailedImage.Visibility = Visibility.Visible;
            purchaseFailedImage.Opacity = 0;

            orderErrorLabel.Visibility = Visibility.Visible;
            orderErrorLabel.Opacity = 0;

            // Анимация постепенного появления изображения.
            DoubleAnimation fadeinAnimation = new DoubleAnimation();
            fadeinAnimation.From = 0;
            fadeinAnimation.To = 1;
            fadeinAnimation.Duration = TimeSpan.FromSeconds(0.5);

            // Анимация постепенного исчезновения изображения.
            DoubleAnimation fadeoutAnimation = new DoubleAnimation();
            fadeoutAnimation.From = 1;
            fadeoutAnimation.To = 0;
            fadeoutAnimation.Duration = TimeSpan.FromSeconds(0.5);

            buttonFadeoutAnimation.Completed += (s, e) =>
            {
                // Запуск постепенного появления изображения.
                purchaseFailedImage.BeginAnimation(Image.OpacityProperty, fadeinAnimation);
            };

            fadeinAnimation.Completed += (s, e) =>
            {
                // Запуск постепенного исчезновения изображения.
                purchaseFailedImage.BeginAnimation(Image.OpacityProperty, fadeoutAnimation);
            };

            // Анимация постепенного появления лейбла.
            DoubleAnimation labelFadeinAnimation = new DoubleAnimation();
            labelFadeinAnimation.From = 0;
            labelFadeinAnimation.To = 1;
            labelFadeinAnimation.Duration = TimeSpan.FromSeconds(0.5);

            fadeoutAnimation.Completed += (s, e) =>
            {
                // Отключение видимости обоих элементов.
                buttonsPanel.Visibility = Visibility.Collapsed;
                purchaseFailedImage.Visibility = Visibility.Hidden;
                // Запуск появления лейбла с ошибкой.
                orderErrorLabel.BeginAnimation(OpacityProperty, labelFadeinAnimation);
            };

            // Запуск анимации.
            buttonsPanel.BeginAnimation(OpacityProperty, buttonFadeoutAnimation);
        }
        #endregion
    }
}