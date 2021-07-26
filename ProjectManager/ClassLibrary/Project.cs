using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ClassLibrary
{
    /// <summary>
    /// Класс проектов.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Кол-во вмещаемых проектом задач.
        /// </summary>
        public int ProjectTaskCapacity { get; set; }

        /// <summary>
        /// Названия проекта.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Список задач проекта.
        /// </summary>
        public List<TaskBase> ProjectTasks { get; set; }

        /// <summary>
        /// Список исполнителей проекта.
        /// </summary>
        public List<User> Executors { get; set; }

        /// <summary>
        /// Метод добавления задачи в проект.
        /// </summary>
        /// <param name="task">Добавляемая задача.</param>
        public void AddTask(TaskBase task)
        {
            // Проверка на вместимость задачи и её добавление.
            if (ProjectTasks.Count < ProjectTaskCapacity) ProjectTasks.Add(task);
            else throw new InvalidOperationException("Maximum capacity reached!");
        }

        /// <summary>
        /// Метод удаления задачи из проекта.
        /// </summary>
        /// <param name="task">Удаляемая задача.</param>
        public void RemoveTask(Task task) => ProjectTasks.Remove(task);

        /// <summary>
        /// Конструктор объекта типа Project.
        /// </summary>
        /// <param name="name">Название проекта.</param>
        /// <param name="taskCapacity">Кол-во вмещаемых проектом задач.</param>
        public Project(string name, int taskCapacity)
        {
            Name = name;
            ProjectTaskCapacity = taskCapacity;
            ProjectTasks = new List<TaskBase>();
            Executors = new List<User>();
        }
    }
}
