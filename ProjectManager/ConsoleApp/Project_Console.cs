using System;
using System.Collections.Generic;
using System.IO;
using ClassLibrary;

namespace ConsoleApp
{
    /// <summary>
    /// Класс проектов с расширенным для консоли функционалом.
    /// </summary>
    class Project_Console : Project
    {
        /// <summary>
        /// Конструктор проекта с расширенным для консоли функционалом.
        /// </summary>
        /// <param name="name">Название проекта.</param>
        /// <param name="taskCapacity">Кол-во вмещаемых проектом задач.</param>
        public Project_Console(string name, int taskCapacity) : base(name, taskCapacity) { }

        /// <summary>
        /// Список индексов задач, отображаемых в списке задач проекта.
        /// </summary>
        private List<int> shownTaskIndicesList = new List<int>();

        /// <summary>
        /// Фильтр списка задач.
        /// </summary>
        private string taskListFilter = "All";

        /// <summary>
        /// Метод сериализации проекта.
        /// </summary>
        /// <returns>Строка с данными о проекте в виде текста.</returns>
        string SerializeProject()
        {
            // Запись данных о названии и вместимости проекта.
            string name = $"Name={Name}";
            string capacity = $"Capacity={ProjectTaskCapacity}";

            // Запись данных об исполнителях из списка исполнителей проекта.
            List<string> executorsString = new List<string>();
            foreach (var executor in Executors) executorsString.Add(executor.Name);
            string executorsLine = $"Executors={string.Join(',',executorsString.ToArray())}";

            // Сериализация задач проекта.
            List<string> serializedTasks = new List<string>();
            foreach (var task in ProjectTasks) 
                if (TaskBase_Console.SerializeTask(task, false) != "") 
                    serializedTasks.Add(TaskBase_Console.SerializeTask(task, false));

            // Формирование выходных данных.
            List<string> outputList = new List<string>();
            outputList.AddRange(new string[] { name, capacity, executorsLine, Environment.NewLine });
            outputList.AddRange(serializedTasks);

            // Возврат строки с данными о проекте.
            return string.Join(Environment.NewLine, outputList);
        }

        /// <summary>
        /// Метод сериализации открытых проектов.
        /// </summary>
        public static void SerializeOpenProjects()
        {
            // Инициализация списка сериализованных проектов.
            List<string> serializedProjects = new List<string>();

            // Сериализация проектов из списка проектов.
            foreach (var project in Program.projectList) serializedProjects.Add(project.SerializeProject());

            try
            {
                // Запись данных о проектах в файл.
                using (StreamWriter sw = new StreamWriter(Program.SavePath, false, System.Text.Encoding.Default))
                {
                    sw.Write(string.Join(Environment.NewLine + "_NEWPROJECT_" + Environment.NewLine, serializedProjects).TrimEnd('\r', '\n'));
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// Метод десериализации проектов.
        /// </summary>
        public static void DeserializeProjects()
        {
            // Проверка на существование файла с данными.
            if (!File.Exists(Program.SavePath)) return;
            try
            {
                // Чтение содержимого файла.
                string fileContents;
                using (StreamReader sr = new StreamReader(Program.SavePath))
                {
                    fileContents = sr.ReadToEnd();
                }

                // Получение данных о каждом отдельном файле.
                string[] serializedProjects = fileContents.Split(Environment.NewLine + "_NEWPROJECT_" + Environment.NewLine);

                // Десериализация проектов.
                foreach (var serializedProject in serializedProjects)
                    Program.projectList.Add(DeserializeProject(serializedProject));
            } catch (Exception ex)
            {
                // Вывод сообщения об ошибке десериализации.
                InteractiveMenu.PrintException(new Exception("PROJECT LOADING FAILED"));
            }
        }

        /// <summary>
        /// Метод десериализации проекта.
        /// </summary>
        /// <param name="serializedProject">Строка с данными о проекте.</param>
        /// <returns>Проект типа Project_Console.</returns>
        static Project_Console DeserializeProject(string serializedProject)
        {
            // Разделение параметров в строке.
            string[] parameters = serializedProject.Split(Environment.NewLine);

            // Определение имени и вместимости проекта.
            string name = parameters[0].Split('=')[1];
            string capacity = parameters[1].Split('=')[1];

            // Создание проекта.
            Project_Console newProject = new Project_Console(name, int.Parse(capacity));

            // Получение списка исполнителей.
            string[] executorNames = parameters[2].Split('=')[1].Split(',');
            foreach (var execName in executorNames) newProject.Executors.Add(new User_Console(execName, newProject));

            // Десериализация задач.
            for (int i = 5; i < parameters.Length; i++)
            {
                // Инициализация задачи.
                TaskBase task = TaskBase_Console.DeserializeTask(parameters[i], newProject, false);

                if (task != null)
                {
                    // Добавление задачи и её подзадач при наличии.
                    newProject.ProjectTasks.Add(task);
                    if (task is Epic_Console) 
                        foreach (var subtask in (task as Epic_Console).subTasks) 
                            newProject.ProjectTasks.Add(subtask);
                }
            }

            return newProject;
        }

        /// <summary>
        /// Метод вывода меню проекта.
        /// </summary>
        public void PrintProjectMenu()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Вывод заголовка с названием проекта.
            string s = $"PROJECT {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод информации о проекте.
            Console.WriteLine($"Tasks: {ProjectTasks.Count}/{ProjectTaskCapacity}");
            Console.WriteLine($"Executors: {Executors.Count}");
            Console.WriteLine();

            // Создание и вызов меню.
            string[] options = {"Executors list", "Tasks list", "sep", "Rename project", "Delete project", "sep", "Back to projects list"};
            MenuCalls menuCalls = new MenuCalls(ProjectsMenuCalls);
            InteractiveMenu.CreateMenu(options, menuCalls, 5, 0, 'v', "");
        }

        /// <summary>
        /// Метод вызовов из меню проекта.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void ProjectsMenuCalls(int activeElement)
        {
            switch (activeElement)
            {
                case 0:
                    // Вывод списка исполнителей.
                    PrintExecutorsList(false);
                    break;
                case 1:
                    // Вывод списка задач.
                    PrintTasksList(false);
                    break;
                case 3:
                    // Переименование проекта.
                    RenameProject();
                    break;
                case 4:
                    // Удаление проекта.
                    Program.projectList.Remove(this);
                    PrintProjectsList(false);
                    break;
                case 6:
                    // Вывод списка проектов.
                    PrintProjectsList(false);
                    break;
            }
        }

        /// <summary>
        /// Метод переименования проекта.
        /// </summary>
        void RenameProject()
        {
            Console.Clear();
            Console.CursorVisible = true;

            // Отрисовка заголовка.
            string s = "PROJECT RENAMING";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Переименование проекта.
            Console.WriteLine("Project name: ");
            Console.SetCursorPosition(14, 2);
            string name = Console.ReadLine();
            Name = name;

            // Вывод меню проекта.
            PrintProjectMenu();
        }

        /// <summary>
        /// Метод вывода списка исполнителей проекта.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления исполнителей.</param>
        public void PrintExecutorsList(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Вывод заголовка.
            string s = $"PROJECT {Name.ToUpper()} - EXECUTORS LIST";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();


            if (Executors.Count == 0)
            {
                // Вывод меню при отсутствии исполнителей.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The executors list is empty!");
                Console.ResetColor();

                string[] menuOptions = { "Add an executor", "Back to project menu" };
                MenuCalls menuCalls = new MenuCalls(ExecutorsListCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 4, 0, 'v', "");
            }
            else
            {
                // Инициализация заголовка списка.
                string header = deletionMode ? "SELECT THE EXECUTOR TO DELETE:" : "EXECUTORS LIST:";

                // Формирование опций меню.
                string[] executorsOptions = new string[Executors.Count + 5];

                for (int i = 0; i < Executors.Count; i++)
                {
                    executorsOptions[i] = $"Executor {Executors[i].Name}";
                }

                executorsOptions[Executors.Count] = "sep";
                executorsOptions[Executors.Count + 1] = "Add an executor";
                executorsOptions[Executors.Count + 2] = "Delete executor";
                executorsOptions[Executors.Count + 3] = "sep";
                executorsOptions[Executors.Count + 4] = "Back to project menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(ExecutorsListCalls);

                // Изменение опций при режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref executorsOptions, Executors.Count + 2);
                    executorsOptions[Executors.Count] = "sep";
                    executorsOptions[Executors.Count + 1] = "Back to project menu";
                    menuCalls = new MenuCalls(ExecutorsDeletionListCalls);
                }

                // Вывод меню с списком исполнителей.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 4, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из списка исполнителей.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void ExecutorsListCalls(int activeElement)
        {
            if (Executors.Count == 0)
            {
                switch (activeElement)
                {
                    // Создание исполнителя.
                    case 0:
                        CreateExecutor();
                        break;
                    // Вывод меню проекта.
                    case 1:
                        PrintProjectMenu();
                        break;
                }
            }
            else
            {
                // Переход в меню исполнителя.
                if (activeElement <= Executors.Count - 1)
                {
                    ((User_Console)Executors[activeElement]).PrintExecutorMenu();
                }
                // Создание исполнителя.
                else if (activeElement == Executors.Count + 1)
                {
                    CreateExecutor();
                }
                // Переход в режим удаления исполнителя.
                else if (activeElement == Executors.Count + 2)
                {
                    PrintExecutorsList(true);
                }
                // Вывод меню проекта.
                else if (activeElement == Executors.Count + 4)
                {
                    PrintProjectMenu();
                }
            }
        }

        /// <summary>
        /// Метод вызовов из списка исполнителей в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void ExecutorsDeletionListCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Удаление исполнителя.
            if (activeElement <= Executors.Count - 1)
            {
                RemoveExecutor(Executors[activeElement]);
                PrintExecutorsList(false);
            }
            // Вывод списка исполнителей.
            else if (activeElement == Executors.Count + 1)
            {
                PrintExecutorsList(false);
            }
        }

        /// <summary>
        /// Метод создания исполнителя.
        /// </summary>
        void CreateExecutor()
        {
            Console.Clear();

            // Отрисовка заголовка.
            Console.CursorVisible = true;
            string s = "ADDING AN EXECUTOR";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Executor name: ");

            // Считывание введенного имени и создание исполнителя.
            Console.SetCursorPosition(15, 2);
            string name = Console.ReadLine();
            Executors.Add(new User_Console(name, this));

            // Вывод списка исполнителей.
            Console.CursorVisible = false;
            PrintExecutorsList(false);
        }

        /// <summary>
        /// Метод удаления исполнителя.
        /// </summary>
        /// <param name="exec">Исполнитель.</param>
        public void RemoveExecutor(User exec)
        {
            // Удаление исполнителя из всех назначенных ему задач.
            foreach (var task in ProjectTasks)
                if (task is IAssignable && (task as IAssignable).UsersAssigned.Contains(exec))
                    (task as IAssignable).RemoveUser(exec);

            // Удаление исполнителя из списка исполнителей.
            Executors.Remove(exec);
        }

        /// <summary>
        /// Метод вывода списка проектов.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления проектов.</param>
        public static void PrintProjectsList(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Вывод заголовка.
            string s = "PROJECT MANAGER - CONSOLE EDITION";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод меню при отсутствии проектов.
            if (Program.projectList.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The project list is empty!");
                Console.ResetColor();

                string[] menuOptions = { "Create new project", "sep", "Exit" };
                MenuCalls menuCalls = new MenuCalls(ProjectListCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 4, 0, 'v', "");
            }
            // Вывод списка при наличии проектов.
            else
            {
                // Инициализация заголовка списка.
                string header = deletionMode ? "SELECT THE PROJECT TO DELETE" : "PROJECT LIST";

                // Формирование опций меню.
                string[] projectsOptions = new string[Program.projectList.Count + 5];

                for (int i = 0; i < Program.projectList.Count; i++)
                {
                    projectsOptions[i] = $"{Program.projectList[i].Name}                   Tasks: {Program.projectList[i].ProjectTasks.Count}\\{Program.projectList[i].ProjectTaskCapacity}";
                }

                projectsOptions[Program.projectList.Count] = "sep";
                projectsOptions[Program.projectList.Count + 1] = "Create new project";
                projectsOptions[Program.projectList.Count + 2] = "Delete project";
                projectsOptions[Program.projectList.Count + 3] = "sep";
                projectsOptions[Program.projectList.Count + 4] = "Exit";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(ProjectListCalls);

                // Изменение опций при режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref projectsOptions, Program.projectList.Count + 2);
                    projectsOptions[Program.projectList.Count] = "sep";
                    projectsOptions[Program.projectList.Count + 1] = "Back";
                    menuCalls = new MenuCalls(ProjectDeletionListCalls);
                }

                // Вывод меню с списком проектов.
                InteractiveMenu.CreateMenu(projectsOptions, menuCalls, 3, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из списка проектов.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        static void ProjectListCalls(int activeElement)
        {
            if (Program.projectList.Count == 0)
            {
                switch (activeElement)
                {
                    // Создание нового проекта.
                    case 0:
                        CreateProject();
                        break;
                    // Выход из программы.
                    case 2:
                        Environment.Exit(0);
                        break;
                }
            }
            else
            {
                // Вызов меню проекта.
                if (activeElement <= Program.projectList.Count - 1)
                {
                    Program.projectList[activeElement].PrintProjectMenu();
                }
                // Создание нового проекта.
                else if (activeElement == Program.projectList.Count + 1)
                {
                    CreateProject();
                }
                // Вывод списка проектов в режиме удаления.
                else if (activeElement == Program.projectList.Count + 2)
                {
                    PrintProjectsList(true);
                }
                // Выход из программы.
                else if (activeElement == Program.projectList.Count + 4)
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Метод вызовов из списка проектов в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        static void ProjectDeletionListCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();
            // Удаление проекта.
            if (activeElement <= Program.projectList.Count - 1)
            {
                Program.projectList.Remove(Program.projectList[activeElement]);
                PrintProjectsList(false);
            }
            // Вывод списка проектов.
            else if (activeElement == Program.projectList.Count + 1)
            {
                PrintProjectsList(false);
            }
        }

        /// <summary>
        /// Метод создания проекта.
        /// </summary>
        static void CreateProject()
        {
            Console.Clear();
            Console.CursorVisible = true;

            // Отрисовка заголовка.
            string s = "PROJECT CREATION";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Отрисовка полей для заполнения.
            Console.WriteLine("Project name: ");
            Console.WriteLine("Task capacity: ____________");

            Console.SetCursorPosition(14, 2);

            // Считывание имени проекта.
            string name = Console.ReadLine();

            // Считывание вместимости проекта.
            int taskCapacity;
            do
            {
                Console.SetCursorPosition(0, 3);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, 3);
                Console.WriteLine("Task capacity: ");
                Console.SetCursorPosition(15, 3);
            } while (!int.TryParse(Console.ReadLine(), out taskCapacity) || taskCapacity <= 0);

            // Создание проекта и добавление его в список проектов.
            Project_Console newProject = new Project_Console(name, taskCapacity);
            Program.projectList.Add(newProject);

            // Вывод списка проектов.
            Console.CursorVisible = false;
            PrintProjectsList(false);
        }

        /// <summary>
        /// Метод вывода списка задач проекта.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления задач.</param>
        public void PrintTasksList(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Применение фильтра к списку задач.
            FilterList();

            // Отрисовка заголовка.
            string s = $"PROJECT {Name.ToUpper()} - TASKS LIST";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Если после фильтрации в списке нет задач.
            if (shownTaskIndicesList.Count == 0)
            {
                // Создание меню.
                Console.Write($"Tasks list filter: {taskListFilter}");
                string[] menuOptions = { "Create new task", "Change tasks list filter", "sep", "Back to project menu" };
                MenuCalls menuCalls = new MenuCalls(TasksListCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 4, 0, 'v', "");
            }
            // Если после фильтрации в списке есть задачи.
            else
            {
                // Вывод информации о фильтре.
                Console.Write($"Tasks list filter: {taskListFilter}");

                // Инициализация заголовка.
                string header = deletionMode ? "SELECT THE TASK TO DELETE" : "TASKS LIST";

                // Заполнение массива опций списка-меню.
                string[] taskOptions = new string[shownTaskIndicesList.Count + 6];

                for (int i = 0; i < ProjectTasks.Count; i++)
                {
                    if (!shownTaskIndicesList.Contains(i)) continue;
                    string taskType = ProjectTasks[i] is Epic_Console ? "Epic" : ProjectTasks[i] is Story_Console ? "Story" : ProjectTasks[i] is Task_Console ? "Task" : "Bug";
                    string prefix = ProjectTasks[i].HasParentEpic ? "├    " : "";
                    string taskString = string.Format("{0,-6}{1,-15}{2,-45}{3,-10}", taskType, ProjectTasks[i].Name, 
                        $"Creation date: {ProjectTasks[i].CreationDate}", $"Status: {ProjectTasks[i].Status}");
                    taskOptions[shownTaskIndicesList.IndexOf(i)] = prefix + taskString;
                }

                taskOptions[shownTaskIndicesList.Count] = "sep";
                taskOptions[shownTaskIndicesList.Count + 1] = "Add new task";
                taskOptions[shownTaskIndicesList.Count + 2] = "Delete task";
                taskOptions[shownTaskIndicesList.Count + 3] = "Change tasks list filter";
                taskOptions[shownTaskIndicesList.Count + 4] = "sep";
                taskOptions[shownTaskIndicesList.Count + 5] = "Back to project menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(TasksListCalls);

                // Изменение списка в режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref taskOptions, ProjectTasks.Count + 2);

                    for (int i = 0; i < ProjectTasks.Count; i++)
                    {
                        string taskType = ProjectTasks[i] is Epic_Console ? "Epic" : ProjectTasks[i] is Story_Console ? "Story" : ProjectTasks[i] is Task_Console ? "Task" : "Bug";
                        string prefix = ProjectTasks[i].HasParentEpic ? "├    " : "";
                        string taskString = string.Format("{0,-6}{1,-15}{2,-45}{3,-10}", taskType, ProjectTasks[i].Name,
                            $"Creation date: {ProjectTasks[i].CreationDate}", $"Status: {ProjectTasks[i].Status}");
                        taskOptions[i] = prefix + taskString;
                    }

                    taskOptions[ProjectTasks.Count] = "sep";
                    taskOptions[ProjectTasks.Count + 1] = "Back";
                    menuCalls = new MenuCalls(TasksDeletionListCalls);
                }

                // Создание меню-списка.
                InteractiveMenu.CreateMenu(taskOptions, menuCalls, 4, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из списка задач.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void TasksListCalls(int activeElement)
        {
            if (shownTaskIndicesList.Count == 0)
            {
                switch (activeElement)
                {
                    // Создание задачи.
                    case 0:
                        // Создание новой задачи.
                        TaskBase newTask = TaskBase_Console.CreateTask(this, false);

                        // Добавление задачи в список.
                        if (newTask != null)
                        {
                            try { ProjectTasks.Add(newTask); }
                            catch (InvalidOperationException ex)
                            {
                                InteractiveMenu.PrintException(ex);
                            }
                        }

                        // Вывод списка задач.
                        PrintTasksList(false);
                        break;
                    case 1:
                        // Вывод меню выбора фильтра.
                        string label = $"PROJECT {Name.ToUpper()} - TASKS LIST - SWITCHING FILTER";
                        string message = "Choose the status filter:";
                        string[] options = new string[] { "All", "Open", "In progress", "Close" };
                        taskListFilter = InteractiveMenu.ReplyMenu(label, message, options, 'h');

                        // Вывод списка задач.
                        PrintTasksList(false);
                        break;
                    case 3:
                        // Вывод меню проекта.
                        PrintProjectMenu();
                        break;
                }
            }
            else
            {
                // Если выбрана задача из списка.
                if (activeElement <= shownTaskIndicesList.Count - 1)
                {
                    // Определение типа задачи и открытие её меню.
                    if (ProjectTasks[shownTaskIndicesList[activeElement]] is Epic_Console) ((Epic_Console)ProjectTasks[shownTaskIndicesList[activeElement]]).PrintEpicMenu(false);
                    else if (ProjectTasks[shownTaskIndicesList[activeElement]] is Story_Console) ((Story_Console)ProjectTasks[shownTaskIndicesList[activeElement]]).PrintStoryMenu(false);
                    else if (ProjectTasks[shownTaskIndicesList[activeElement]] is Task_Console) ((Task_Console)ProjectTasks[shownTaskIndicesList[activeElement]]).PrintTaskMenu(false);
                    else if (ProjectTasks[shownTaskIndicesList[activeElement]] is Bug_Console) ((Bug_Console)ProjectTasks[shownTaskIndicesList[activeElement]]).PrintBugMenu(false);
                }
                // Создание задачи.
                else if (activeElement == shownTaskIndicesList.Count + 1)
                {
                    // Создание новой задачи.
                    TaskBase newTask = TaskBase_Console.CreateTask(this, false);

                    // Добавление задачи в список.
                    if (newTask != null)
                    {
                        try { ProjectTasks.Add(newTask); }
                        catch (InvalidOperationException ex)
                        {
                            InteractiveMenu.PrintException(ex);
                        }
                    }

                    // Вывод списка задач.
                    PrintTasksList(false);
                }
                // Удаление задачи.
                else if (activeElement == shownTaskIndicesList.Count + 2)
                {
                    PrintTasksList(true);
                }
                // Смена фильтра списка.
                else if (activeElement == shownTaskIndicesList.Count + 3)
                {
                    // Вывод меню выбора фильтра.
                    string label = $"PROJECT {Name.ToUpper()} - TASKS LIST - SWITCHING FILTER";
                    string message = "Choose the status filter:";
                    string[] options = new string[] { "All", "Open", "In progress", "Close" };
                    taskListFilter = InteractiveMenu.ReplyMenu(label, message, options, 'h');

                    // Вывод списка задач.
                    PrintTasksList(false);
                }
                // Возврат в меню проекта.
                else if (activeElement == shownTaskIndicesList.Count + 5)
                {
                    PrintProjectMenu();
                }
            }
        }

        /// <summary>
        /// Метод вызовов из списка задач в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void TasksDeletionListCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Если была выбрана задача из списка.
            if (activeElement <= ProjectTasks.Count - 1)
            {
                // Удаление всех подзадач в случае, если выбранная задача - эпик.
                if (ProjectTasks[activeElement] is Epic_Console)
                {
                    for (int i = activeElement + 1; i < ProjectTasks.Count; i++)
                    {
                        if (ProjectTasks[i].HasParentEpic)
                        {
                            ProjectTasks.Remove(ProjectTasks[i]);
                            i--;
                        }
                        else break;
                    }
                }

                // Удаление задачи из подзадач родительского эпика.
                ProjectTasks[activeElement].ParentProject = null;
                if (ProjectTasks[activeElement].HasParentEpic)
                {
                    foreach(var epic in ProjectTasks)
                    {
                        if (epic is Epic_Console) (epic as Epic_Console).subTasks.Remove(ProjectTasks[activeElement]);
                    }
                }

                // Удаление задачи из списка задач.
                ProjectTasks.Remove(ProjectTasks[activeElement]);

                // Вывод списка задач.
                PrintTasksList(false);
            }
            // Возврат к списку задач.
            else if (activeElement == ProjectTasks.Count + 1)
            {
                PrintTasksList(false);
            }
        }

        /// <summary>
        /// Метод применения установленного фильтра к списку задач.
        /// </summary>
        public void FilterList()
        {
            shownTaskIndicesList.Clear();

            // Предварительная сортировка списка задач.
            SortTasksList();

            // Отображение всех задач.
            if (taskListFilter == "All")
            {
                for (int i = 0; i < ProjectTasks.Count; i++) shownTaskIndicesList.Add(i);
            } 
            // Применение фильтров по статусу.
            else
            {
                for (int i = 0; i < ProjectTasks.Count; i++)
                {
                    if (ProjectTasks[i].Status == taskListFilter) shownTaskIndicesList.Add(i);
                }
            }
        }

        /// <summary>
        /// Метод сортировки списка задач для выстроения иерархий эпик-подзадачи.
        /// </summary>
        public void SortTasksList()
        {
            // Список-иерархия задач.
            List<TaskBase[]> tasks = new List<TaskBase[]>();

            // Заполнение списка задач.
            for (int i = 0; i < ProjectTasks.Count; i++)
            {
                // Добавление иерархии эпика и его подзадач в список.
                if (ProjectTasks[i] is Epic_Console)
                {
                    // Иерархия эпик - подзадачи.
                    List<TaskBase> epicAndSubtasks = new List<TaskBase>();

                    // Добавление эпика в начало списка.
                    epicAndSubtasks.Add(ProjectTasks[i]);

                    // Добавление подзадач.
                    for (int j = 0; j < ProjectTasks.Count; j++)
                    {
                        if (((Epic_Console)ProjectTasks[i]).subTasks.Contains(ProjectTasks[j]))
                        {
                            epicAndSubtasks.Add(ProjectTasks[j]);
                            ProjectTasks.Remove(ProjectTasks[j]);
                            if (j <= i) i--;
                            j--;
                        }
                    }

                    ProjectTasks.Remove(ProjectTasks[i]);
                    i--;

                    // Добавление иерархии в список.
                    tasks.Add(epicAndSubtasks.ToArray());
                }
                else
                {
                    // Добавление независимой задачи.
                    tasks.Add(new TaskBase[] { ProjectTasks[i] });
                }
            }

            // Реконструкция списка задач.
            ProjectTasks.Clear();
            foreach (var taskList in tasks)
            {
                foreach (var task in taskList)
                {
                    ProjectTasks.Add(task);
                }
            }
        }
    }
}
