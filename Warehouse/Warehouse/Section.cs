using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace Warehouse
{
    /// <summary>
    /// Класс разделов.
    /// </summary>
    [Serializable]
    public class Section : INotifyPropertyChanged
    {
        /// <summary>
        /// Название раздела (поле).
        /// </summary>
        private string name;

        /// <summary>
        /// Название раздела (свойство).
        /// </summary>
        public string Name { 
            get { return name; }
            set
            {
                name = value;
                // Уведомление при измненении названия.
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Коллекция подразделов.
        /// </summary>
        public ObservableCollection<Section> Subsections { get; set; }

        /// <summary>
        /// Коллекция товаров.
        /// </summary>
        public ObservableCollection<Product> Products { get; set; }

        /// <summary>
        /// Родительский раздел.
        /// </summary>
        [XmlIgnore]
        public Section parentSection;

        /// <summary>
        /// Беспараметрический конструктор типа.
        /// </summary>
        public Section() : this("_placeholder_") { }

        /// <summary>
        /// Конструктор раздела.
        /// </summary>
        /// <param name="name">Название раздела.</param>
        public Section(string name)
        {
            Name = name;
            Subsections = new ObservableCollection<Section>();
            Products = new ObservableCollection<Product>();
        }

        /// <summary>
        /// Метод добавления подраздела в раздел.
        /// </summary>
        /// <param name="section">Подраздел.</param>
        public void AddSubsection(Section section) => Subsections.Add(section);

        /// <summary>
        /// Метод добавления товара в раздел.
        /// </summary>
        /// <param name="product">Товар.</param>
        public void AddProduct(Product product) => Products.Add(product);

        /// <summary>
        /// Обработчик события изменения названия раздела.
        /// </summary>
        [field: XmlIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Обработчик события изменения названия.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Метод получения списка недостающих товаров в разделе.
        /// </summary>
        /// <param name="lackingProducts">Список недостающих разделов.</param>
        public void GetLackingProducts(ref List<Product> lackingProducts)
        {
            // Поиск недостающих товаров в собственных товарах.
            foreach (var product in Products)
            {
                if (product.Amount < product.MinimalAmount) lackingProducts.Add(product);
            }

            // Поиск недостающих товаров в подразделах.
            foreach (var subsec in Subsections)
            {
                subsec.GetLackingProducts(ref lackingProducts);
            }
        }

        /// <summary>
        /// Метод установки родительского раздела всем "детям" раздела.
        /// </summary>
        public void SetChildrenParentSection()
        {
            // Установка родительского раздела всем товарам раздела.
            foreach (var product in Products) product.ParentSection = this;

            // Установка родительского раздела всем подразделам раздела.
            foreach (var subsec in Subsections)
            {
                subsec.parentSection = this;
                subsec.SetChildrenParentSection();
            }
        }

        /// <summary>
        /// Метод удаления подраздела раздела.
        /// </summary>
        /// <param name="subsection">Подраздел.</param>
        public void RemoveSubsection(Section subsection)
        {
            // Поиск и удаление подраздела из списка подразделов раздела.
            if (Subsections.Contains(subsection))
            {
                Subsections.Remove(subsection);
            }
            // Поиск и удаление подраздела из подразделов подразделов раздела.
            else
            {
                foreach (var sec in Subsections)
                {
                    sec.RemoveSubsection(subsection);
                }
            }
        }
    }
}
