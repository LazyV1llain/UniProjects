using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    /// <summary>
    /// Класс объектов-пользователей (исполнителей).
    /// </summary>
    public class User
    {
        /// <summary>
        /// Имя исполнителя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Конструктор объекта типа User.
        /// </summary>
        /// <param name="name">Имя исполнителя.</param>
        public User(string name)
        {
            Name = name;
        }
    }
}
