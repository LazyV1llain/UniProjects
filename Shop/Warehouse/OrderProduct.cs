using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Warehouse
{
    /// <summary>
    /// Класс товаров в заказе пользователя.
    /// </summary>
    [DataContract]
    public class OrderProduct : Product, INotifyPropertyChanged
    {
        private int amountInOrder;

        /// <summary>
        /// Количество товара в заказе.
        /// </summary>
        [DataMember(Name = "AmountInOrder")]
        public int AmountInOrder
        {
            get { return amountInOrder; }
            set
            {
                amountInOrder = value;
                // Уведомление при измненении количества.
                OnPropertyChanged("AmountInOrder");
            }
        }

        /// <summary>
        /// Обработчик события изменения количества товара.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Обработчик события изменения количества.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Конструктор товара в заказе.
        /// </summary>
        /// <param name="name">Название товара.</param>
        /// <param name="code">Артикул товара.</param>
        /// <param name="price">Цена товара.</param>
        /// <param name="amount">Количество товара на складе.</param>
        /// <param name="minAmount">Минимальное количество товара на складе до его нехватки.</param>
        /// <param name="desc">Строка с описанием товара.</param>
        /// <param name="source">Изображение товара.</param>
        /// <param name="amountInOrder">Количество товара в заказе.</param>
        public OrderProduct(string name, string code, decimal price, int amount, int minAmount, string desc = "", ImageSource source = null, int amountInOrder = 1) 
            : base(name, code, price, amount, minAmount, desc, source)
        {
            AmountInOrder = amountInOrder;
        }

        /// <summary>
        /// Конструктор товара в заказе.
        /// </summary>
        /// <param name="other">Товар-основа.</param>
        /// <param name="amountInOrder">Количество товара в заказе.</param>
        public OrderProduct(Product other, int amountInOrder = 1) : base (other)
        {
            AmountInOrder = amountInOrder;
        }
    }
}
