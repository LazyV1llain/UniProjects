using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows.Media;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using AutoMapper;

namespace Warehouse
{
    /// <summary>
    /// Класс товаров.
    /// </summary>
    [Serializable]
    public class Product
    {
        /// <summary>
        /// Флаг того, что товар находится в подразделе в иерархии разделов.
        /// </summary>
        [XmlIgnore]
        public bool IsInSubsection { get; set; }

        /// <summary>
        /// Родительский раздел.
        /// </summary>
        [XmlIgnore]
        public Section ParentSection { get; set; }

        /// <summary>
        /// Строка с путём к товару в иерархии разделов.
        /// </summary>
        [XmlIgnore]
        public string ProductPath { 
            get
            {
                // Получение списка родительских разделов.
                List<string> sectionNames = new List<string>();
                Section parentSection = ParentSection;
                while (parentSection != null)
                {
                    sectionNames.Add(parentSection.Name);
                    parentSection = parentSection.parentSection;
                }

                // Поулчение и возврат пути к товару.
                sectionNames.Reverse();
                return string.Join('/', sectionNames.ToArray());
            }
        }

        /// <summary>
        /// Название товара.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Артикул товара.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Цена товара.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество товара на складе.
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// Минимальное количество товара на складе до его нехватки.
        /// </summary>
        public int MinimalAmount { get; set; }

        /// <summary>
        /// Строка вида "количество товара\минимальное количество товара".
        /// </summary>
        [XmlIgnore]
        public string AmountString
        {
            get => $"{Amount}/{MinimalAmount}";
        }

        /// <summary>
        /// Строка с описанием товара.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Изображение товара.
        /// </summary>
        [XmlIgnore]
        public ImageSource Image { get; set; }

        /// <summary>
        /// Беспараметрический конструктор объекта.
        /// </summary>
        public Product() { }

        /// <summary>
        /// Конструктор товара.
        /// </summary>
        /// <param name="name">Название товара.</param>
        /// <param name="code">Артикул товара.</param>
        /// <param name="price">Цена товара.</param>
        /// <param name="amount">Количество товара на складе.</param>
        /// <param name="minamount">Минимальное количество товара на складе до его нехватки.</param>
        /// <param name="desc">Строка с описанием товара.</param>
        /// <param name="source">Изображение товара.</param>
        public Product(string name, string code, decimal price, int amount, int minamount, string desc = "", ImageSource source = null)
        {
            Name = name;
            Code = code;
            Price = price;
            Amount = amount;
            MinimalAmount = minamount;
            Description = desc;
            Image = source;
        }

        /// <summary>
        /// Запись изображения товара в виде последовательности байтов.
        /// </summary>
        [XmlElement("Image")]
        public byte[] ImageBuffer
        {
            get
            {
                byte[] imageBuffer = null;

                if (Image != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder(); // or some other encoder
                        encoder.Frames.Add(BitmapFrame.Create(Image as BitmapSource));
                        encoder.Save(stream);
                        imageBuffer = stream.ToArray();
                    }
                }

                return imageBuffer;
            }
            set
            {
                if (value == null)
                {
                    Image = null;
                }
                else
                {
                    BitmapImage biImg = new BitmapImage();
                    MemoryStream ms = new MemoryStream(value);
                    biImg.BeginInit();
                    biImg.StreamSource = ms;
                    biImg.EndInit();

                    ImageSource imgSrc = biImg as ImageSource;

                    Image = imgSrc;
                }
            }
        }
    }

    /// <summary>
    /// Класс-маппер класса товаров для записи в CSV.
    /// </summary>
    public sealed class ProductMap : ClassMap<Product>
    {
        public ProductMap()
        {
            // Обозначение свойств класса, которые нужно записать в CSV.
            Map(m => m.ProductPath);
            Map(m => m.Code);
            Map(m => m.Name);
            Map(m => m.Price);
            Map(m => m.Amount);
            Map(m => m.MinimalAmount);
        }
    }
}
