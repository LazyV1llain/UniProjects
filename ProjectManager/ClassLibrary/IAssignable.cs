using System;
using System.Collections.Generic;

namespace ClassLibrary
{
    /// <summary>
    /// Интерфейс, отвечающий за функционал привязки исполнителя к задаче.
    /// </summary>
    public interface IAssignable
    {
        /// <summary>
        /// Список назначенных исполнителей задачи.
        /// </summary>
        List<User> UsersAssigned { get; }

        /// <summary>
        /// Метод добавления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        void AddUser(User user);

        /// <summary>
        /// Метод удаления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        void RemoveUser(User user);
    }
}
