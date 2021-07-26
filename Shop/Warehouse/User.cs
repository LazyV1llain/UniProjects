using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Warehouse
{
    /// <summary>
    /// Класс пользователей программы.
    /// </summary>
    [DataContract, KnownType(typeof(Client))]
    public class User
    {
        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        [DataMember(Name = "FullName")]
        public string FullName { get; private set; }

        /// <summary>
        /// Адрес электронной почты (логин) пользователя.
        /// </summary>
        [DataMember(Name = "EMailAddress")]
        public string EMailAddress { get; private set; }

        /// <summary>
        /// Пароль пользователя.
        /// </summary>
        [DataMember(Name = "Password")]
        string Password { get; set; }

        /// <summary>
        /// "Соль" в пароле пользователя.
        /// </summary>
        [DataMember(Name = "PasswordSalt")]
        string PasswordSalt { get; set; }

        /// <summary>
        /// Конструктор объекта класса User.
        /// </summary>
        /// <param name="eMailAddress">Адрес электронной почты (логин) пользователя.</param>
        /// <param name="password">Пароль пользователя.</param>
        /// <param name="lastName">Фамилия пользователя.</param>
        /// <param name="name">Имя пользователя.</param>
        /// <param name="patronymic">Отчество пользователя.</param>
        public User(string eMailAddress, string password, string lastName, string name, string patronymic = "")
        {
            FullName = patronymic == "" ? name + " " + lastName : name + " " + patronymic + " " + lastName;
            EMailAddress = eMailAddress;
            SetNewPassword(password);
        }

        /// <summary>
        /// Установка нового пароля пользователя.
        /// </summary>
        /// <param name="password">Пароль в исходном виде.</param>
        public void SetNewPassword(string password)
        {
            // Генерация соли.
            PasswordSalt = CryptographyFormatter.GenerateSalt();
            // Вывод хэш-функции шифрования пароля и соли.
            Password = CryptographyFormatter.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(PasswordSalt));
        }

        /// <summary>
        /// Метод проверки введенных входных данных на корректность.
        /// </summary>
        /// <param name="eMailAddress">Введенный адрес электронной почты (логин).</param>
        /// <param name="password">Введенный пароль.</param>
        /// <returns>True, если данные корректны.</returns>
        public bool CredentialsCorrect(string eMailAddress, string password)
        {
            // Расчёт хэш-функции введенного пароля и соли.
            string encryptedPassword = CryptographyFormatter.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(PasswordSalt));

            // Определение корректности введённых данных.
            if (eMailAddress == EMailAddress && encryptedPassword == Password) return true;
            else return false;
        }
    }
}
