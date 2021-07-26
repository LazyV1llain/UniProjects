using System;
using System.Collections.Generic;
using System.Text;
using ClassLibrary;
using Newtonsoft.Json;

namespace ConsoleApp
{
    /// <summary>
    /// Класс исполнителей с функционалом, расширенным для консольного интерфейса.
    /// </summary>
    class User_Console : User
    {
        /// <summary>
        /// Родительский проект.
        /// </summary>
        public Project_Console ParentProject { get; set; }

        /// <summary>
        /// Конструктор объекта класса User_Console.
        /// </summary>
        /// <param name="name">Имя исполнителя.</param>
        /// <param name="parentProject">Родительский проект.</param>
        public User_Console(string name, Project_Console parentProject) : base(name) 
        {
            ParentProject = parentProject;
        }

        /// <summary>
        /// Метод вывода меню исполнителя.
        /// </summary>
        public void PrintExecutorMenu()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"EXECUTOR {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Получение списка назначенных исполнителю задач.
            List<string> assignedTasksNames = new List<string>();
            foreach (var task in ParentProject.ProjectTasks)
            {
                if (task is IAssignable && ((IAssignable)task).UsersAssigned.Contains(this)) assignedTasksNames.Add(task.Name);
            }
            string assignedTasksNamesString = assignedTasksNames.Count == 0 ? "None" : string.Join(", ", assignedTasksNames);

            // Вывод информации о назначенных исполнителю задачах.
            Console.WriteLine($"Assigned tasks: {assignedTasksNamesString}");
            Console.WriteLine();

            // Создание меню исполнителя.
            string[] options = { "Assign a task", "sep", "Rename executor", "Delete executor", "sep", "Back to executors list" };
            MenuCalls menuCalls = new MenuCalls(ExecutorMenuCalls);
            InteractiveMenu.CreateMenu(options, menuCalls, 4, 0, 'v', "");
        }

        /// <summary>
        /// Метод вызовов из меню исполнителя.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void ExecutorMenuCalls(int activeElement)
        {
            switch (activeElement)
            {
                // Привязка исполнителя к задаче.
                case 0:
                    AssignExecutorToTask();
                    break;
                // Переименование исполнителя.
                case 2:
                    RenameExecutor();
                    break;
                // Удаление исполнителя из списка.
                case 3:
                    ParentProject.RemoveExecutor(this);
                    ParentProject.PrintExecutorsList(false);
                    break;
                // Вывод списка исполнителей.
                case 5:
                    ParentProject.PrintExecutorsList(false);
                    break;
            }
        }

        /// <summary>
        /// Метод привязки исполнителя к задаче.
        /// </summary>
        void AssignExecutorToTask()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"PROJECT {ParentProject.Name.ToUpper()} - ASSIGNING EXECUTOR {Name.ToUpper()} TO TASK";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод кнопки возврата в меню исполнителя в случае отсутствия задач в проекте.
            if (ParentProject.ProjectTasks.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("Back to the executor menu");
                Console.ResetColor();
                Console.WriteLine();

                while (true)
                {
                    ConsoleKeyInfo input = Console.ReadKey();

                    if (input.Key == ConsoleKey.Enter) break;
                }

                Console.Clear();
                PrintExecutorMenu();
            }
            // Вывод списка-меню задач проекта.
            else
            {
                // Сортировка списка задач проекта.
                ParentProject.SortTasksList();
                Console.WriteLine("Tasks list:");

                // Формирование массива опций мменю.
                string[] taskOptions = new string[ParentProject.ProjectTasks.Count + 2];

                for (int i = 0; i < ParentProject.ProjectTasks.Count; i++)
                {
                    // Получение строки-типа задачи.
                    string taskType = ParentProject.ProjectTasks[i] is Epic_Console ? "Epic" : 
                        ParentProject.ProjectTasks[i] is Story_Console ? "Story" : 
                        ParentProject.ProjectTasks[i] is Task_Console ? "Task" : "Bug";
                    // Префикс подзадачи.
                    string prefix = ParentProject.ProjectTasks[i].HasParentEpic ? "├    " : "";
                    // Получение строки с информацеий о задаче.
                    string taskString = string.Format("{0,-6}{1,-15}{2,-45}{3,-10}", taskType, 
                        ParentProject.ProjectTasks[i].Name,
                        $"Creation date: {ParentProject.ProjectTasks[i].CreationDate}", 
                        $"Status: {ParentProject.ProjectTasks[i].Status}");
                    taskOptions[i] = prefix + taskString;
                }

                taskOptions[ParentProject.ProjectTasks.Count] = "sep";
                taskOptions[ParentProject.ProjectTasks.Count + 1] = "Back to executor menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(AssignListCalls);

                // Создание меню.
                InteractiveMenu.CreateMenu(taskOptions, menuCalls, 5, 0, 'v', "");
            }
        }

        /// <summary>
        /// Метод вызовов из меню назначения задачи исполнителю.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        public void AssignListCalls(int activeElement)
        {
            // Если выбрана задача проекта.
            if (activeElement <= ParentProject.ProjectTasks.Count - 1)
            {
                if (ParentProject.ProjectTasks[activeElement] is IAssignable)
                {
                    // Определение типа задачи.
                    if (!((IAssignable)ParentProject.ProjectTasks[activeElement]).UsersAssigned.Contains(this)) 
                        if (ParentProject.ProjectTasks[activeElement] is Task_Console || 
                            ParentProject.ProjectTasks[activeElement] is Bug_Console)
                        {
                            // Попытка добавления исполнителя.
                            if (((IAssignable)ParentProject.ProjectTasks[activeElement]).UsersAssigned.Count == 0)
                                ((IAssignable)ParentProject.ProjectTasks[activeElement]).UsersAssigned.Add(this);
                            else InteractiveMenu.PrintException(new Exception("TASK EXECUTOR CAPACITY EXCEEDED"));
                        } else ((IAssignable)ParentProject.ProjectTasks[activeElement]).UsersAssigned.Add(this);
                }
                PrintExecutorMenu();
            }
            // Возврат в меню исполнителя.
            else if (activeElement == ParentProject.ProjectTasks.Count + 1)
            {
                PrintExecutorMenu();
            }
        }

        /// <summary>
        /// Метод переименования исполнителя.
        /// </summary>
        void RenameExecutor()
        {
            Console.Clear();
            Console.CursorVisible = true;

            // Отрисовка заголовка.
            string s = "EXECUTOR RENAMING";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Переименование исполнителя.
            Console.WriteLine("Executor name: ");
            Console.SetCursorPosition(14, 2);
            string name = Console.ReadLine();
            Name = name;
        }

        /// <summary>
        /// Переопределенный метод Equals для валидного сравнения исполнителей.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is User_Console console &&
                   Name == console.Name &&
                   EqualityComparer<Project_Console>.Default.Equals(ParentProject, console.ParentProject);
        }
    }
}
