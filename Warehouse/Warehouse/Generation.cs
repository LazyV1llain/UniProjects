using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    /// <summary>
    /// Статический класс генерации объектов.
    /// </summary>
    static class Generation
    {
        static Random random = new Random();

        /// <summary>
        /// Метод генерации случайной строки заданной длины.
        /// </summary>
        /// <param name="length">Длина строки.</param>
        /// <returns>Сгенерированная строка.</returns>
        public static string RandomString(int length)
        {
            // Набор символов, из которых собирается строка.
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            // Генерация и возврат строки.
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Метод генерации случайного раздела по заданным параметрам.
        /// </summary>
        /// <param name="sectionNumber">Максимально возможное число подразделов.</param>
        /// <param name="productNumber">Максимально возможное число продуктов.</param>
        /// <param name="parentSection">Родительский раздел.</param>
        /// <param name="codes">Артикулы существующих товаров.</param>
        /// <returns>Сгенерированный раздел.</returns>
        public static Section GenerateSection(ref int sectionNumber, ref int productNumber, Section parentSection, List<string> codes)
        {
            // Генерация раздела и установка родительского раздела.
            Section newSection = new Section(RandomString(random.Next(4, 9)));
            if (parentSection != null) newSection.parentSection = parentSection;

            // Генерация числа товаров в разделе.
            int sectionProductNumber = random.Next(0, productNumber / 4 + 1);
            productNumber -= sectionProductNumber;

            // Генерация товаров раздела.
            for (int i = 0; i < sectionProductNumber; i++)
            {
                newSection.AddProduct(GenerateProduct(codes, newSection));
            }

            // Генерация числа подразделов раздела.
            int sectionSubsectionNumber = random.Next(0, sectionNumber + 1);
            sectionNumber -= sectionSubsectionNumber;

            // Генерация подразделов раздела.
            for (int i = 0; i < sectionSubsectionNumber; i++)
            {
                newSection.Subsections.Add(GenerateSection(ref sectionNumber, ref productNumber, newSection, codes));
            }

            // Возврат сгенерированного раздела.
            return newSection;
        }

        /// <summary>
        /// Метод генерации случайного товара.
        /// </summary>
        /// <param name="codes">Артикулы существующих товаров.</param>
        /// <param name="parentSection">Родительский раздел.</param>
        /// <returns>Сгенерированный товар.</returns>
        public static Product GenerateProduct(List<string> codes, Section parentSection)
        {
            // Генерация артикула товара.
            string code;
            do code = RandomString(random.Next(4, 8));
            while (codes.Contains(code));

            // Генерация товара.
            Product product = new Product(RandomString(random.Next(4, 9)), code, (decimal)(random.Next(0, 1000) + Math.Round(random.NextDouble(), 2)),
                random.Next(0, 1000), random.Next(10, 1000), RandomString(random.Next(4, 9))) { ParentSection = parentSection };
            // Внесение артикула в список.
            codes.Add(code);

            // Возврат сгенерированного товара.
            return product;
        }
    }
}
