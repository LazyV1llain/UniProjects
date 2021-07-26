using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using CsvHelper.Configuration;

namespace Warehouse
{
    /// <summary>
    /// Класс клиентов-пользователей программы.
    /// </summary>
    [DataContract]
    public class Client : User
    {
        /// <summary>
        /// Номер телефона клиента.
        /// </summary>
        [DataMember(Name = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Адрес клиента.
        /// </summary>
        [DataMember(Name = "Address")]
        public string Address { get; set; }

        /// <summary>
        /// Заказы клиента.
        /// </summary>
        [DataMember(Name = "Orders")]
        public ObservableCollection<Order> Orders { get; set; }

        /// <summary>
        /// Количество средств, затраченных клиентом на заказы.
        /// </summary>
        public decimal SpentOnOrders
        {
            get
            {
                decimal sum = 0;

                foreach (var order in Orders)
                {
                    if ((order.Status & Status.Paid) == Status.Paid)
                    {
                        foreach (var product in order.Products) sum += product.Price;
                    }
                }

                return sum;
            }
        }

        /// <summary>
        /// Конструктор объекта класса Client.
        /// </summary>
        /// <param name="phoneNumber">Номер телефона клиента.</param>
        /// <param name="address">Адрес клиента.</param>
        public Client(string eMailAddress, string password, string lastName, string name, string phoneNumber, string address, string patronymic = "") 
            : base(eMailAddress, password, lastName, name, patronymic)
        {
            PhoneNumber = phoneNumber;
            Address = address;
            Orders = new ObservableCollection<Order>();
        }
    }

    /// <summary>
    /// Класс-маппер класса клиентов для записи в CSV.
    /// </summary>
    public sealed class ClientExpenditureReportMap : ClassMap<Client>
    {
        public ClientExpenditureReportMap()
        {
            // Обозначение свойств класса, которые нужно записать в CSV.
            Map(m => m.FullName);
            Map(m => m.EMailAddress);
            Map(m => m.Address);
            Map(m => m.PhoneNumber);
            Map(m => m.SpentOnOrders);
        }
    }
}
