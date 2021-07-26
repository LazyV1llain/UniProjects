using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Warehouse
{
    /// <summary>
    /// Класс администраторов-пользователей программы.
    /// </summary>
    [DataContract]
    class Administrator : User
    {
        /// <summary>
        /// Конструктор объекта класса Administrator.
        /// </summary>
        public Administrator(string eMailAddress, string password, string lastName, string name, string patronymic = "") : base(eMailAddress, password, lastName, name, patronymic) { }
    }
}
