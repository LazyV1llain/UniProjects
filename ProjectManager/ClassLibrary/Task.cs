using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ClassLibrary
{
    /// <summary>
    /// Базовый класс задач.
    /// </summary>
    public class TaskBase
    {
        /// <summary>
        /// Название задачи.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дата создания задачи.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Статус задачи.
        /// </summary>
        public string Status { get; protected set; }

        /// <summary>
        /// Родительский проект задачи.
        /// </summary>
        public Project ParentProject { get; set; }

        /// <summary>
        /// Флаг наличия у задачи родительского эпика.
        /// </summary>
        public bool HasParentEpic { get; set; }

        /// <summary>
        /// Конструктор объекта типа TaskBase.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public TaskBase(string name, DateTime creationDate, bool parentEpic)
        {
            Name = name;
            CreationDate = creationDate;
            Status = "Open";
            HasParentEpic = parentEpic;
        }

        /// <summary>
        /// Метод установки статуса задачи.
        /// </summary>
        /// <param name="newStatus">Новый статус задачи.</param>
        public void SetStatus(string newStatus)
        {
            if (newStatus == "Open" || newStatus == "Close" || newStatus == "In progress") Status = newStatus;
        }
    }

    /// <summary>
    /// Класс задач типа Epic.
    /// </summary>
    public class Epic : TaskBase
    {
        /// <summary>
        /// Список подзадач.
        /// </summary>
        public List<TaskBase> subTasks { get; private set; }

        /// <summary>
        /// Конструктор объекта типа Epic.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        public Epic(string name, DateTime creationDate) : base(name, creationDate, false) 
        {
            subTasks = new List<TaskBase>();
        }
    }

    /// <summary>
    /// Класс задач типа Story.
    /// </summary>
    public class Story : TaskBase, IAssignable
    {
        /// <summary>
        /// Список назначенных исполнителей.
        /// </summary>
        public List<User> UsersAssigned { get; private set; }

        /// <summary>
        /// Конструктор объекта типа Story.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public Story(string name, DateTime creationDate, bool parentEpic) : base(name, creationDate, parentEpic)
        {
            UsersAssigned = new List<User>();
        }

        /// <summary>
        /// Метод добавления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void AddUser(User user) => UsersAssigned.Add(user);

        /// <summary>
        /// Метод удаления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void RemoveUser(User user) => UsersAssigned.Remove(user);
    }

    /// <summary>
    /// Класс задач типа Task.
    /// </summary>
    public class Task : TaskBase, IAssignable
    {
        /// <summary>
        /// Список назначенных исполнителей.
        /// </summary>
        public List<User> UsersAssigned { get; private set; }

        /// <summary>
        /// Конструктор объекта типа Task.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public Task(string name, DateTime creationDate, bool parentEpic) : base(name, creationDate, parentEpic)
        {
            UsersAssigned = new List<User>();
        }

        /// <summary>
        /// Метод добавления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void AddUser(User user) => UsersAssigned.Add(user);

        /// <summary>
        /// Метод удаления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void RemoveUser(User user) => UsersAssigned.Remove(user);
    }

    /// <summary>
    /// Класс задач типа Bug.
    /// </summary>
    public class Bug : TaskBase, IAssignable
    {
        /// <summary>
        /// Список назначенных исполнителей.
        /// </summary>
        public List<User> UsersAssigned { get; private set; }

        /// <summary>
        /// Конструктор объекта типа Task.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public Bug(string name, DateTime creationDate, bool parentEpic) : base(name, creationDate, parentEpic)
        {
            UsersAssigned = new List<User>();
        }

        /// <summary>
        /// Метод добавления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void AddUser(User user) => UsersAssigned.Add(user);

        /// <summary>
        /// Метод удаления исполнителя.
        /// </summary>
        /// <param name="user">Исполнитель.</param>
        public void RemoveUser(User user) => UsersAssigned.Remove(user);
    }
}
