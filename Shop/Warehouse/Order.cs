using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CsvHelper.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    /// <summary>
    /// Статусы заказа.
    /// </summary>
    [Flags]
    public enum Status
    {
        None = 0,
        Processed = 1,
        Paid = 2,
        Shipped = 4,
        Executed = 8
    }

    /// <summary>
    /// Класс заказов пользователей.
    /// </summary>
    [DataContract(IsReference = true)]
    public class Order : INotifyPropertyChanged
    {
        /// <summary>
        /// Номер заказа.
        /// </summary>
        [DataMember(Name = "OrderNumber")]
        public int OrderNumber { get; private set; }

        /// <summary>
        /// Время, когда был сделан заказ.
        /// </summary>
        [DataMember(Name = "OrderTime")]
        public DateTime OrderTime { get; private set; }

        /// <summary>
        /// Товары в заказе.
        /// </summary>
        [DataMember(Name = "Products")]
        public ObservableCollection<OrderProduct> Products { get; set; }

        /// <summary>
        /// Клиент, сделавший заказ.
        /// </summary>
        [DataMember(Name = "Client")]
        public Client Client { get; set; }

        private Status status;

        /// <summary>
        /// Текущий статус заказа.
        /// </summary>
        [DataMember(Name = "Status")]
        public Status Status
        {
            get { return status; }
            set
            {
                status = value;
                // Уведомление при измненении статуса.
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Событие изменения статуса.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Обработчик события изменения статуса.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Конструктор объекта типа Order.
        /// </summary>
        /// <param name="orderNumber">Номер заказа.</param>
        /// <param name="orderTime">Время, когда был сделан заказ.</param>
        /// <param name="products">Товары в заказе.</param>
        /// <param name="client">Клиент, сделавший заказ.</param>
        public Order(int orderNumber, DateTime orderTime, List<OrderProduct> products, Client client)
        {
            OrderNumber = orderNumber;
            OrderTime = orderTime;
            Products = new ObservableCollection<OrderProduct>(products);
            Status = 0;
            Client = client;
        }

        /// <summary>
        /// Метод добавления статуса заказу.
        /// </summary>
        /// <param name="targetStatus">Статус, который нужно добавить.</param>
        public void AddStatus(Status targetStatus) => Status |= targetStatus;

        /// <summary>
        /// Метод удаления статуса из заказа.
        /// </summary>
        /// <param name="targetStatus">Статус, который нужно удалить.</param>
        public void RemoveStatus(Status targetStatus) => Status &= ~targetStatus;
    }

    /// <summary>
    /// Класс-маппер класса заказов для записи в CSV.
    /// </summary>
    public sealed class DefectOrderReportMap : ClassMap<Order>
    {
        public DefectOrderReportMap()
        {
            // Обозначение свойств класса, которые нужно записать в CSV.
            Map(m => m.Client.FullName);
            Map(m => m.OrderNumber);
            Map(m => m.OrderTime);
            Map(m => m.Status);
        }
    }
}
