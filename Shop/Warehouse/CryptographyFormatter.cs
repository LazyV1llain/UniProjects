using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Warehouse
{
    /// <summary>
    /// Класс-форматтер для шифрования пароля.
    /// </summary>
    class CryptographyFormatter
    {
        /// <summary>
        /// Метод расчёта хэш-функции с солью.
        /// </summary>
        /// <param name="bytesToHash">Пароль в виде массива байтов.</param>
        /// <param name="salt">Соль.</param>
        /// <returns>Хэш-функция.</returns>
        public static string ComputeHash(byte[] bytesToHash, byte[] salt)
        {
            // Расчёт хэш-функции Rfc2898.
            var byteResult = new Rfc2898DeriveBytes(bytesToHash, salt, 10000);
            return Convert.ToBase64String(byteResult.GetBytes(24));
        }

        /// <summary>
        /// Метод генерации случайной соли.
        /// </summary>
        /// <returns>Случайная соль.</returns>
        public static string GenerateSalt()
        {
            var bytes = new byte[128 / 8];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
