using System;
using System.Collections.Generic;
using System.Text;
using ClassLibrary;

namespace ConsoleApp
{
    /// <summary>
    /// Статический класс, отвечающий за взаимодействие с задачами в консольном приложении.
    /// </summary>
    static class TaskBase_Console
    {
        /// <summary>
        /// Метод сериализации задачи.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <param name="callFromEpic">Флаг, истинный при вызове метода для подзадачи эпика при сериализации эпика.</param>
        /// <returns>Строка с данными о задаче.</returns>
        public static string SerializeTask(TaskBase task, bool callFromEpic)
        {
            string type, name, creationDate, status;

            // Заполнение базовых данных о задаче.
            name = $"Name={task.Name}";
            creationDate = $"CreationDate={task.CreationDate}";
            status = $"Status={task.Status}";

            // Если задача типа Epic_Console.
            if (task is Epic_Console)
            {
                // Сохранение типа задачи.
                type = "Type=Epic";

                // Получение списка сериализованных подзадач.
                List<string> serializedSubTasks = new List<string>();
                foreach (var subtask in (task as Epic_Console).subTasks) serializedSubTasks.Add(SerializeTask(subtask, true));

                // Получение строки с данными о подзадачах.
                string subtasks = serializedSubTasks.Count == 0 ? "Subtasks:~" : $"Subtasks:{string.Join('-',serializedSubTasks)}";

                // Склейка данных и их возврат.
                return $"{type};{name};{creationDate};{status};{subtasks}";
            } 
            // Если задача иного типа.
            else
            {
                // Определение типа задачи и сохранение данных о нём.
                type = task is Task_Console ? "Type=Task" : task is Story_Console ? "Type=Story" : "Type=Bug";

                // Сериализация подзадач эпика в качестве независимых задач не проводится.
                if (task.HasParentEpic && !callFromEpic) return "";

                // Получение списка имён исполнителей.
                List<string> executorsList = new List<string>();
                Array.ForEach((task as IAssignable).UsersAssigned.ToArray(), delegate (User x) { executorsList.Add(x.Name); });

                // Получение строки с данными об исполнителях.
                string executorString;
                if (executorsList.Count == 0) executorString = "~";
                else executorString = string.Join(',', executorsList);
                string executors = $"Executors={executorString}";

                // Склейка данных и их возврат.
                return $"{type};{name};{creationDate};{status};{executors}";
            }
        }

        /// <summary>
        /// Метод десериализации задачи.
        /// </summary>
        /// <param name="parameterString">Строка с данными о задаче.</param>
        /// <param name="project">Проект, в котором находится задача.</param>
        /// <param name="parentEpic">Флаг, отмечающий, есть ли у задачи родительский эпик.</param>
        /// <returns>Десериализованная задача.</returns>
        public static TaskBase DeserializeTask(string parameterString, Project_Console project, bool parentEpic)
        {
            // Разделение данных о задаче.
            string[] parameters = parameterString.Split(';');

            // Получение базовых данных о задаче.
            string type = parameters[0].Split('=')[1];
            string name = parameters[1].Split('=')[1];
            DateTime creationDate = DateTime.Parse(parameters[2].Split('=')[1]);
            string status = parameters[3].Split('=')[1];

            switch (type)
            {
                // Десериализация эпика.
                case "Epic":
                    // Создание задачи.
                    Epic_Console newEpic = new Epic_Console(name, creationDate, project);
                    newEpic.SetStatus(status);

                    // Получение данных о подзадачах.
                    string subtasksString = parameterString.Substring(parameterString.IndexOf("Subtasks:") + "Subtasks:".Length);
                    if (subtasksString == "~") return newEpic;
                    string[] serializedSubtasks = subtasksString.Split('-');

                    // Десериализация подзадач.
                    foreach (var serializedSubtask in serializedSubtasks)
                    {
                        TaskBase subtask = DeserializeTask(serializedSubtask, project, true);
                        if (subtask == null) continue;
                        newEpic.subTasks.Add(subtask);
                    }

                    // Возврат задачи.
                    return newEpic;
                // Десериализация Story.
                case "Story":
                    // Создание задачи.
                    Story_Console newStory = new Story_Console(name, creationDate, project, parentEpic);
                    newStory.SetStatus(status);

                    // Получение строк с именами исполнителей.
                    string[] storyExecsNames = parameters[4].Split('=')[1].Split(',');

                    // Сопоставление исполнителей с исполнителями в проекте.
                    if (storyExecsNames[0] == "~") return newStory;
                    foreach (var execName in storyExecsNames)
                    {
                        User_Console newStoryExec = new User_Console(execName, project);
                        if (project.Executors.Contains(newStoryExec))
                        {
                            newStoryExec = (project.Executors[project.Executors.IndexOf(newStoryExec)] as User_Console);
                            newStory.AddUser(newStoryExec);
                        }
                    }

                    // Возврат задачи.
                    return newStory;
                // Десериализация Task. 
                case "Task":
                    // Создание задачи.
                    Task_Console newTask = new Task_Console(name, creationDate, project, parentEpic);
                    newTask.SetStatus(status);

                    // Получение строки с именем исполнителя.
                    string taskExecName = parameters[4].Split('=')[1].Split(',')[0];

                    // Сопоставления исполнителя с исполнителем в проекте.
                    if (taskExecName == "~") return newTask;
                    User_Console newTaskExec = new User_Console(taskExecName, project);
                    if (project.Executors.Contains(newTaskExec))
                    {
                        newTaskExec = (project.Executors[project.Executors.IndexOf(newTaskExec)] as User_Console);
                        newTask.AddUser(newTaskExec);
                    }

                    // Возврат задачи.
                    return newTask;
                // Десериализация Bug.
                case "Bug":
                    // Создание задачи.
                    Bug_Console newBug = new Bug_Console(name, creationDate, project);
                    newBug.SetStatus(status);

                    // Получение строки с именем исполнителя.
                    string bugExecName = parameters[4].Split('=')[1].Split(',')[0];

                    // Сопоставления исполнителя с исполнителем в проекте.
                    if (bugExecName == "~") return newBug;
                    User_Console newBugExec = new User_Console(bugExecName, project);
                    if (project.Executors.Contains(newBugExec))
                    {
                        newBugExec = (project.Executors[project.Executors.IndexOf(newBugExec)] as User_Console);
                        newBug.AddUser(newBugExec);
                    }

                    // Возврат задачи.
                    return newBug;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Метод создания задачи.
        /// </summary>
        /// <param name="parentProject">Проект, в котором находится задача.</param>
        /// <param name="parentEpic">Флаг, отмечающий, есть ли у задачи родительский эпик.</param>
        /// <returns>Новая задача.</returns>
        public static TaskBase CreateTask(Project_Console parentProject, bool parentEpic)
        {
            Console.Clear();
            Console.CursorVisible = true;

            // Отрисовка заголовка.
            string s = "TASK CREATION";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Считывание имени задачи.
            Console.WriteLine("Task name: ");
            Console.SetCursorPosition(11, 2);
            string name = Console.ReadLine();

            // Выбор типа задачи при помощи меню.
            string[] typeOptions = new string[] { "Epic", "Story", "Task", "Bug"};
            Console.CursorVisible = false;
            string type = InteractiveMenu.ReplyMenu("Task type:", typeOptions, 3, 0);

            Console.Clear();
            switch (type.ToLower())
            {
                // Создание эпика.
                case "epic":
                    Epic_Console newEpic = new Epic_Console(name, DateTime.Now, parentProject);
                    return newEpic;
                // Создание Story.
                case "story":
                    Story_Console newStory = new Story_Console(name, DateTime.Now, parentProject, parentEpic);
                    return newStory;
                // Создание Task.
                case "task":
                    Task_Console newTask = new Task_Console(name, DateTime.Now, parentProject, parentEpic);
                    return newTask;
                // Создание Bug.
                case "bug":
                    Bug_Console newBug = new Bug_Console(name, DateTime.Now, parentProject);
                    return newBug;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Метод переименования задачи.
        /// </summary>
        /// <param name="task">Задача.</param>
        /// <returns>Переименованная задача.</returns>
        public static TaskBase RenameTask(TaskBase task)
        {
            Console.Clear();
            Console.CursorVisible = true;

            // Отрисовка заголовка.
            string s = "Task Renaming";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Переименование задачи.
            Console.WriteLine("Task name: ");
            Console.SetCursorPosition(14, 2);
            string name = Console.ReadLine();
            task.Name = name;

            // Возврат переименованной задачи.
            return task;
        }
    }

    /// <summary>
    /// Класс задач типа Epic с функционалом, расширенным для консольного интерфейса.
    /// </summary>
    class Epic_Console : Epic
    {
        /// <summary>
        /// Конструктор объекта класса Epic_Console.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentProject">Проект, в котором содержится задача.</param>
        public Epic_Console(string name, DateTime creationDate, Project_Console parentProject) : base(name, creationDate) {
            ParentProject = parentProject;
        }

        /// <summary>
        /// Метод вывода меню задач типа Epic.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления подзадач.</param>
        public void PrintEpicMenu(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"TASK {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод информации о задаче.
            Console.WriteLine("Task type: Epic");
            Console.WriteLine($"Subtasks: {subTasks.Count}");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine();

            // Отрисовка меню управления задачей при отсутствии подзадач.
            if (subTasks.Count == 0)
            {
                string[] menuOptions = { "Create new subtask", "sep", "Rename task", "Change status", "sep", "Back to task list" };
                MenuCalls menuCalls = new MenuCalls(EpicMenuCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 6, 0, 'v', "");
            }
            // Отрисовка списка подзадач и меню управления задачей.
            else
            {
                // Инициализация заголовка.
                string header = deletionMode ? "SELECT THE SUBTASK TO DELETE" : "SUBTASKS LIST";

                // Заполнение массива опций списка-меню.
                string[] subtaskOptions = new string[subTasks.Count + 8];

                for (int i = 0; i < subTasks.Count; i++)
                {
                    string taskString = string.Format("{0,-15}{1,-45}{2,-10}", subTasks[i].Name,
                        $"Creation date: {subTasks[i].CreationDate}", $"Status: {subTasks[i].Status}");
                    subtaskOptions[i] = taskString;
                }

                subtaskOptions[subTasks.Count] = "sep";
                subtaskOptions[subTasks.Count + 1] = "Add new subtask";
                subtaskOptions[subTasks.Count + 2] = "Delete subtask";
                subtaskOptions[subTasks.Count + 3] = "sep";
                subtaskOptions[subTasks.Count + 4] = "Rename the task";
                subtaskOptions[subTasks.Count + 5] = "Change status";
                subtaskOptions[subTasks.Count + 6] = "sep";
                subtaskOptions[subTasks.Count + 7] = "Back to project tasks list";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(EpicMenuCalls);

                // Изменение списка в режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref subtaskOptions, subTasks.Count + 2);
                    subtaskOptions[subTasks.Count] = "sep";
                    subtaskOptions[subTasks.Count + 1] = "Back";
                    menuCalls = new MenuCalls(EpicDeletionMenuCalls);
                }

                // Создание меню-списка.
                InteractiveMenu.CreateMenu(subtaskOptions, menuCalls, 7, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из меню эпика.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void EpicMenuCalls(int activeElement)
        {
            if (subTasks.Count == 0)
            {
                switch (activeElement)
                {
                    // Создание подзадачи.
                    case 0:
                        // Создание задачи.
                        TaskBase newTask = TaskBase_Console.CreateTask((ParentProject as Project_Console), true);

                        // Добавление задачи в списки подзадач эпика и задач проекта.
                        if (newTask != null)
                        {
                            if (ParentProject.ProjectTasks.Count == ParentProject.ProjectTaskCapacity) InteractiveMenu.PrintException(new Exception("ERROR: Maximum project task capacity reached"));
                            else
                            {
                                subTasks.Add(newTask);
                                ParentProject.ProjectTasks.Add(newTask);
                            }
                        }

                        // Возврат в меню.
                        PrintEpicMenu(false);
                        break;
                    // Переименование задачи.
                    case 2:
                        TaskBase_Console.RenameTask(this);
                        break;
                    // Смена статуса задачи.
                    case 3:
                        string[] menuOptions = {"Open", "In progress", "Close"};
                        Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                        PrintEpicMenu(false);
                        break;
                    // Возврат в список задач проекта.
                    case 5:
                        (ParentProject as Project_Console).PrintTasksList(false);
                        break;
                }
            }
            else
            {
                // Выбор подзадачи эпика.
                if (activeElement <= subTasks.Count - 1)
                {
                    // Переход в меню подзадачи.
                    if (subTasks[activeElement] is Epic_Console) ((Epic_Console)subTasks[activeElement]).PrintEpicMenu(false);
                    else if (subTasks[activeElement] is Story_Console) ((Story_Console)subTasks[activeElement]).PrintStoryMenu(false);
                    else if (subTasks[activeElement] is Task_Console) ((Task_Console)subTasks[activeElement]).PrintTaskMenu(false);
                    else if (subTasks[activeElement] is Bug_Console) ((Bug_Console)subTasks[activeElement]).PrintBugMenu(false);
                }
                // Создание новой подзадачи.
                else if (activeElement == subTasks.Count + 1)
                {
                    // Создание задачи.
                    TaskBase newTask = TaskBase_Console.CreateTask((ParentProject as Project_Console), true);

                    // Добавление задачи в списки подзадач эпика и задач проекта.
                    if (newTask != null)
                    {
                        if (ParentProject.ProjectTasks.Count == ParentProject.ProjectTaskCapacity) InteractiveMenu.PrintException(new Exception("ERROR: Maximum project task capacity reached"));
                        else
                        {
                            subTasks.Add(newTask);
                            ParentProject.ProjectTasks.Add(newTask);
                        }
                    }

                    // Возврат в меню.
                    PrintEpicMenu(false);
                }
                // Удаление подзадачи.
                else if (activeElement == subTasks.Count + 2)
                {
                    // Возврат в меню в режиме удаления.
                    PrintEpicMenu(true);
                }
                // Переименование задачи.
                else if (activeElement == subTasks.Count + 4)
                {
                    TaskBase_Console.RenameTask(this);
                } 
                // Смена статуса задачи.
                else if (activeElement == subTasks.Count + 5)
                {
                    string[] menuOptions = { "Open", "In progress", "Close" };
                    Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                    PrintEpicMenu(false);
                }
                // Возврат в список задач проекта.
                else if (activeElement == subTasks.Count + 7)
                {
                    (ParentProject as Project_Console).PrintTasksList(false);
                }
            }
        }

        /// <summary>
        /// Метод вызовов из меню эпика в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void EpicDeletionMenuCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Удаление подзадачи из всех списков.
            if (activeElement <= subTasks.Count - 1)
            {
                ParentProject.ProjectTasks.Remove(subTasks[activeElement]);
                subTasks.Remove(subTasks[activeElement]);
                PrintEpicMenu(false);
            }
            // Возврат в меню эпика.
            else if (activeElement == subTasks.Count + 1)
            {
                PrintEpicMenu(false);
            }
        }
    }

    /// <summary>
    /// Класс задач типа Story с функционалом, расширенным для консольного интерфейса.
    /// </summary>
    class Story_Console : Story
    {
        /// <summary>
        /// Конструктор объекта класса Story_Console.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentProject">Проект, в котором содержится задача.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public Story_Console(string name, DateTime creationDate, Project_Console parentProject, bool parentEpic) : base(name, creationDate, parentEpic)
        {
            ParentProject = parentProject;
        }

        /// <summary>
        /// Метод вывода меню задачи типа Story.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления исполнителей.</param>
        public void PrintStoryMenu(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"TASK {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод информации о задаче.
            Console.WriteLine("Task type: Story");
            Console.WriteLine($"Executors: {UsersAssigned.Count}");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine();

            // Отрисовка меню управления задачей при отсутствии исполнителей.
            if (UsersAssigned.Count == 0)
            {
                string[] menuOptions = { "Assign an executor", "sep", "Rename task", "Change status", "sep", "Back to tasks list" };
                MenuCalls menuCalls = new MenuCalls(StoryMenuCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 6, 0, 'v', "");
            }
            // Отрисовка списка исполнителей и меню управления задачей.
            else
            {
                // Инициализация заголовка.
                string header = deletionMode ? "SELECT THE EXECUTOR TO UNASSIGN" : "TASK MENU";

                // Заполнение массива опций списка-меню.
                string[] executorsOptions = new string[UsersAssigned.Count + 8];

                for (int i = 0; i < UsersAssigned.Count; i++)
                {
                    executorsOptions[i] = $"Executor {UsersAssigned[i].Name}";
                }

                executorsOptions[UsersAssigned.Count] = "sep";
                executorsOptions[UsersAssigned.Count + 1] = "Assign an executor";
                executorsOptions[UsersAssigned.Count + 2] = "Unassign an executor";
                executorsOptions[UsersAssigned.Count + 3] = "sep";
                executorsOptions[UsersAssigned.Count + 4] = "Rename the task";
                executorsOptions[UsersAssigned.Count + 5] = "Change status";
                executorsOptions[UsersAssigned.Count + 6] = "sep";
                executorsOptions[UsersAssigned.Count + 7] = "Back to tasks list";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(StoryMenuCalls);

                // Изменение списка в режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref executorsOptions, UsersAssigned.Count + 2);
                    executorsOptions[UsersAssigned.Count] = "sep";
                    executorsOptions[UsersAssigned.Count + 1] = "Back";
                    menuCalls = new MenuCalls(StoryDeletionMenuCalls);
                }

                // Создание меню-списка.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 7, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Story.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void StoryMenuCalls(int activeElement)
        {
            if (UsersAssigned.Count == 0)
            {
                switch (activeElement)
                {
                    // Вызов меню назначения исполнителей.
                    case 0:
                        ExecutorAssignList();
                        break;
                    // Переименование задачи.
                    case 2:
                        TaskBase_Console.RenameTask(this);
                        PrintStoryMenu(false);
                        break;
                    // Смена статуса задачи.
                    case 3:
                        string[] menuOptions = { "Open", "In progress", "Close" };
                        Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                        PrintStoryMenu(false);
                        break;
                    // Возврат в список задач проекта.
                    case 5:
                        (ParentProject as Project_Console).PrintTasksList(false);
                        break;
                }
            }
            else
            {
                // Переход в меню исполнителя при выборе исполнителя.
                if (activeElement <= UsersAssigned.Count - 1)
                {
                    ((User_Console)UsersAssigned[activeElement]).PrintExecutorMenu();
                }
                // Назначение нового исполнителя из списка.
                else if (activeElement == UsersAssigned.Count + 1)
                {
                    ExecutorAssignList();
                }
                // Удаление исполнителя.
                else if (activeElement == UsersAssigned.Count + 2)
                {
                    PrintStoryMenu(true);
                }
                // Переименование задачи.
                else if (activeElement == UsersAssigned.Count + 4)
                {
                    TaskBase_Console.RenameTask(this);
                    PrintStoryMenu(false);
                }
                // Смена статуса задачи.
                else if (activeElement == UsersAssigned.Count + 5)
                {
                    string[] menuOptions = { "Open", "In progress", "Close" };
                    Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                    PrintStoryMenu(false);
                }
                // Вывод списка задач проекта.
                else if (activeElement == UsersAssigned.Count + 7)
                {
                    (ParentProject as Project_Console).PrintTasksList(false);
                }
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Story в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void StoryDeletionMenuCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Если был выбран исполнитель.
            if (activeElement <= UsersAssigned.Count - 1)
            {
                // Удаление исполнителя из списка исполнителей задачи.
                UsersAssigned.Remove((ParentProject as Project_Console).Executors[activeElement]);
                PrintStoryMenu(false);
            }
            else if (activeElement == UsersAssigned.Count + 1)
            {
                // Вовзрат в меню задачи.
                PrintStoryMenu(false);
            }
        }

        /// <summary>
        /// Метод вызова списка исполнителей проекта для назначения.
        /// </summary>
        void ExecutorAssignList()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"PROJECT {Name.ToUpper()} - EXECUTORS LIST";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод оповещения о пустом списке исполнителей.
            if (ParentProject.Executors.Count == 0)
            {
                // Вывод сообщения.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The executors list is empty!");
                Console.ResetColor();

                // Отрисовка кнопки.
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("Back to the task menu");
                Console.ResetColor();
                Console.WriteLine();

                // Ожидание нажатием пользователем кнопки Enter.
                while (true)
                {
                    ConsoleKeyInfo input = Console.ReadKey();

                    if (input.Key == ConsoleKey.Enter) break;
                }

                Console.Clear();
                PrintStoryMenu(false);
            }
            else
            {
                // Формирование массива опций списка-меню.
                string[] executorsOptions = new string[ParentProject.Executors.Count + 2];

                for (int i = 0; i < ParentProject.Executors.Count; i++)
                {
                    executorsOptions[i] = $"Executor {ParentProject.Executors[i].Name}";
                }

                executorsOptions[ParentProject.Executors.Count] = "sep";
                executorsOptions[ParentProject.Executors.Count + 1] = "Back to the task menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(AssignExecutorListCalls);

                // Создание меню.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 4, 0, 'v', "SELECT AN EXECUTOR TO ASSIGN");
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Story.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        public void AssignExecutorListCalls(int activeElement)
        {
            // Назначение выбранного исполнителя.
            if (activeElement <= ParentProject.Executors.Count - 1)
            {
                if (!UsersAssigned.Contains(ParentProject.Executors[activeElement]))
                    UsersAssigned.Add(ParentProject.Executors[activeElement]);
                PrintStoryMenu(false);
            }
            // Возврат в меню задачи.
            else
            {
                PrintStoryMenu(false);
            }
        }
    }

    /// <summary>
    /// Класс задач типа Task с функционалом, расширенным для консольного интерфейса.
    /// </summary>
    class Task_Console : Task
    {
        /// <summary>
        /// Конструктор объекта класса Task_Console.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentProject">Проект, в котором содержится задача.</param>
        /// <param name="parentEpic">Флаг наличия у задачи родительского эпика.</param>
        public Task_Console(string name, DateTime creationDate, Project_Console parentProject, bool parentEpic) : base(name, creationDate, parentEpic)
        {
            ParentProject = parentProject;
        }

        /// <summary>
        /// Метод вывода меню задачи типа Task.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления исполнителей.</param>
        public void PrintTaskMenu(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"TASK {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод информации о задаче.
            Console.WriteLine("Task type: Task");
            Console.WriteLine($"Executors: {UsersAssigned.Count}");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine();

            // Отрисовка меню управления задачей при отсутствии исполнителей.
            if (UsersAssigned.Count == 0)
            {
                string[] menuOptions = { "Assign an executor", "sep", "Rename task", "Change status", "sep", "Back to tasks list" };
                MenuCalls menuCalls = new MenuCalls(TaskMenuCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 6, 0, 'v', "");
            }
            else
            {
                // Инициализация заголовка.
                string header = deletionMode ? "SELECT THE EXECUTOR TO UNASSIGN" : "TASK MENU";

                // Заполнение массива опций списка-меню.
                string[] executorsOptions = new string[UsersAssigned.Count + 7];

                for (int i = 0; i < UsersAssigned.Count; i++)
                {
                    executorsOptions[i] = $"Executor {UsersAssigned[i].Name}";
                }

                executorsOptions[UsersAssigned.Count] = "sep";
                executorsOptions[UsersAssigned.Count + 1] = "Unassign an executor";
                executorsOptions[UsersAssigned.Count + 2] = "sep";
                executorsOptions[UsersAssigned.Count + 3] = "Rename the task";
                executorsOptions[UsersAssigned.Count + 4] = "Change status";
                executorsOptions[UsersAssigned.Count + 5] = "sep";
                executorsOptions[UsersAssigned.Count + 6] = "Back to tasks list";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(TaskMenuCalls);

                // Изменение списка в режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref executorsOptions, UsersAssigned.Count + 2);
                    executorsOptions[UsersAssigned.Count] = "sep";
                    executorsOptions[UsersAssigned.Count + 1] = "Back";
                    menuCalls = new MenuCalls(TaskDeletionMenuCalls);
                }

                // Создание меню-списка.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 7, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Task.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void TaskMenuCalls(int activeElement)
        {
            if (UsersAssigned.Count == 0)
            {
                switch (activeElement)
                {
                    // Вызов меню назначения исполнителей.
                    case 0:
                        ExecutorAssignList();
                        break;
                    // Переименование задачи.
                    case 2:
                        TaskBase_Console.RenameTask(this);
                        PrintTaskMenu(false);
                        break;
                    // Смена статуса задачи.
                    case 3:
                        string[] menuOptions = { "Open", "In progress", "Close" };
                        Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                        PrintTaskMenu(false);
                        break;
                    // Возврат в список задач проекта.
                    case 5:
                        (ParentProject as Project_Console).PrintTasksList(false);
                        break;
                }
            }
            else
            {
                // Переход в меню исполнителя при выборе исполнителя.
                if (activeElement <= UsersAssigned.Count - 1)
                {
                    ((User_Console)UsersAssigned[activeElement]).PrintExecutorMenu();
                }
                // Удаление исполнителя.
                else if (activeElement == UsersAssigned.Count + 1)
                {
                    PrintTaskMenu(true);
                }
                // Переименование задачи.
                else if (activeElement == UsersAssigned.Count + 3)
                {
                    TaskBase_Console.RenameTask(this);
                    PrintTaskMenu(false);
                }
                // Смена статуса задачи.
                else if (activeElement == UsersAssigned.Count + 4)
                {
                    string[] menuOptions = { "Open", "In progress", "Close" };
                    Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                    PrintTaskMenu(false);
                }
                // Вывод списка задач проекта.
                else if (activeElement == UsersAssigned.Count + 6)
                {
                    (ParentProject as Project_Console).PrintTasksList(false);
                }
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Task в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void TaskDeletionMenuCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Если был выбран исполнитель.
            if (activeElement <= UsersAssigned.Count - 1)
            {
                // Удаление исполнителя из списка исполнителей задачи.
                UsersAssigned.Remove(ParentProject.Executors[activeElement]);
                PrintTaskMenu(false);
            }
            else if (activeElement == UsersAssigned.Count + 1)
            {
                // Вовзрат в меню задачи.
                PrintTaskMenu(false);
            }
        }

        /// <summary>
        /// Метод вызова списка исполнителей проекта для назначения.
        /// </summary>
        void ExecutorAssignList()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"PROJECT {Name.ToUpper()} - EXECUTORS LIST";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод оповещения о пустом списке исполнителей.
            if (ParentProject.Executors.Count == 0)
            {
                // Вывод сообщения.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The executors list is empty!");
                Console.ResetColor();

                // Отрисовка кнопки.
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("Back to the task menu");
                Console.ResetColor();
                Console.WriteLine();

                // Ожидание нажатием пользователем кнопки Enter.
                while (true)
                {
                    ConsoleKeyInfo input = Console.ReadKey();

                    if (input.Key == ConsoleKey.Enter) break;
                }

                Console.Clear();
                PrintTaskMenu(false);
            }
            else
            {
                // Формирование массива опций списка-меню.
                string[] executorsOptions = new string[ParentProject.Executors.Count + 2];

                for (int i = 0; i < ParentProject.Executors.Count; i++)
                {
                    executorsOptions[i] = $"Executor {ParentProject.Executors[i].Name}";
                }

                executorsOptions[ParentProject.Executors.Count] = "sep";
                executorsOptions[ParentProject.Executors.Count + 1] = "Back to the task menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(AssignExecutorListCalls);

                // Создание меню.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 4, 0, 'v', "SELECT AN EXECUTOR TO ASSIGN");
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Task.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        public void AssignExecutorListCalls(int activeElement)
        {
            // Назначение выбранного исполнителя.
            if (activeElement <= ParentProject.Executors.Count - 1)
            {
                if (!UsersAssigned.Contains(ParentProject.Executors[activeElement]))
                    UsersAssigned.Add(ParentProject.Executors[activeElement]);
                PrintTaskMenu(false);
            }
            // Возврат в меню задачи.
            else
            {
                PrintTaskMenu(false);
            }
        }
    }

    /// <summary>
    /// Класс задач типа Bug с функционалом, расширенным для консольного интерфейса.
    /// </summary>
    class Bug_Console : Task
    {
        /// <summary>
        /// Конструктор объекта класса Bug_ConsoleBug_Console.
        /// </summary>
        /// <param name="name">Название задачи.</param>
        /// <param name="creationDate">Дата создания задачи.</param>
        /// <param name="parentProject">Проект, в котором содержится задача.</param>
        public Bug_Console(string name, DateTime creationDate, Project_Console parentProject) : base(name, creationDate, false)
        {
            ParentProject = parentProject;
        }

        /// <summary>
        /// Метод вывода меню задачи типа Bug.
        /// </summary>
        /// <param name="deletionMode">Флаг режима удаления исполнителей.</param>
        public void PrintBugMenu(bool deletionMode)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"TASK {Name.ToUpper()}";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод информации о задаче.
            Console.WriteLine("Task type: Bug");
            Console.WriteLine($"Executors: {UsersAssigned.Count}");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine();

            // Отрисовка меню управления задачей при отсутствии исполнителей.
            if (UsersAssigned.Count == 0)
            {
                string[] menuOptions = { "Assign an executor", "sep", "Rename task", "Change status", "sep", "Back to tasks list" };
                MenuCalls menuCalls = new MenuCalls(BugMenuCalls);

                InteractiveMenu.CreateMenu(menuOptions, menuCalls, 6, 0, 'v', "");
            }
            else
            {
                // Инициализация заголовка.
                string header = deletionMode ? "SELECT THE EXECUTOR TO UNASSIGN" : "TASK MENU";

                // Заполнение массива опций списка-меню.
                string[] executorsOptions = new string[UsersAssigned.Count + 7];

                for (int i = 0; i < UsersAssigned.Count; i++)
                {
                    executorsOptions[i] = $"Executor {UsersAssigned[i].Name}";
                }

                executorsOptions[UsersAssigned.Count] = "sep";
                executorsOptions[UsersAssigned.Count + 1] = "Unassign an executor";
                executorsOptions[UsersAssigned.Count + 2] = "sep";
                executorsOptions[UsersAssigned.Count + 3] = "Rename the task";
                executorsOptions[UsersAssigned.Count + 4] = "Change status";
                executorsOptions[UsersAssigned.Count + 5] = "sep";
                executorsOptions[UsersAssigned.Count + 6] = "Back to tasks list";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(BugMenuCalls);

                // Изменение списка в режиме удаления.
                if (deletionMode == true)
                {
                    InteractiveMenu.selectionBackColor = ConsoleColor.Red;

                    Array.Resize(ref executorsOptions, UsersAssigned.Count + 2);
                    executorsOptions[UsersAssigned.Count] = "sep";
                    executorsOptions[UsersAssigned.Count + 1] = "Back";
                    menuCalls = new MenuCalls(BugDeletionMenuCalls);
                }

                // Создание меню-списка.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 7, 0, 'v', header);
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Bug.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void BugMenuCalls(int activeElement)
        {
            if (UsersAssigned.Count == 0)
            {
                switch (activeElement)
                {
                    // Вызов меню назначения исполнителей.
                    case 0:
                        ExecutorAssignList();
                        break;
                    // Переименование задачи.
                    case 2:
                        TaskBase_Console.RenameTask(this);
                        PrintBugMenu(false);
                        break;
                    // Смена статуса задачи.
                    case 3:
                        string[] menuOptions = { "Open", "In progress", "Close" };
                        Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                        PrintBugMenu(false);
                        break;
                    // Возврат в список задач проекта.
                    case 5:
                        (ParentProject as Project_Console).PrintTasksList(false);
                        break;
                }
            }
            else
            {
                // Переход в меню исполнителя при выборе исполнителя.
                if (activeElement <= UsersAssigned.Count - 1)
                {
                    ((User_Console)UsersAssigned[activeElement]).PrintExecutorMenu();
                }
                // Удаление исполнителя.
                else if (activeElement == UsersAssigned.Count + 1)
                {
                    PrintBugMenu(true);
                }
                // Переименование задачи.
                else if (activeElement == UsersAssigned.Count + 3)
                {
                    TaskBase_Console.RenameTask(this);
                    PrintBugMenu(false);
                }
                // Смена статуса задачи.
                else if (activeElement == UsersAssigned.Count + 4)
                {
                    string[] menuOptions = { "Open", "In progress", "Close" };
                    Status = InteractiveMenu.ReplyMenu($"TASK {Name} - STATUS CHANGE", "Choose the status below:", menuOptions, 'h');
                    PrintBugMenu(false);
                }
                // Вывод списка задач проекта.
                else if (activeElement == UsersAssigned.Count + 6)
                {
                    (ParentProject as Project_Console).PrintTasksList(false);
                }
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Task в режиме удаления.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        void BugDeletionMenuCalls(int activeElement)
        {
            InteractiveMenu.ResetMenuSelectionColor();

            // Если был выбран исполнитель.
            if (activeElement <= UsersAssigned.Count - 1)
            {
                // Удаление исполнителя из списка исполнителей задачи.
                UsersAssigned.Remove(ParentProject.Executors[activeElement]);
                PrintBugMenu(false);
            }
            else if (activeElement == UsersAssigned.Count + 1)
            {
                // Вовзрат в меню задачи.
                PrintBugMenu(false);
            }
        }


        /// <summary>
        /// Метод вызова списка исполнителей проекта для назначения.
        /// </summary>
        void ExecutorAssignList()
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка.
            string s = $"PROJECT {Name.ToUpper()} - EXECUTORS LIST";
            Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
            Console.WriteLine();

            // Вывод оповещения о пустом списке исполнителей.
            if (ParentProject.Executors.Count == 0)
            {
                // Вывод сообщения.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("The executors list is empty!");
                Console.ResetColor();

                // Отрисовка кнопки.
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write("Back to the task menu");
                Console.ResetColor();
                Console.WriteLine();

                // Ожидание нажатием пользователем кнопки Enter.
                while (true)
                {
                    ConsoleKeyInfo input = Console.ReadKey();

                    if (input.Key == ConsoleKey.Enter) break;
                }

                Console.Clear();
                PrintBugMenu(false);
            }
            else
            {
                // Формирование массива опций списка-меню.
                string[] executorsOptions = new string[ParentProject.Executors.Count + 2];

                for (int i = 0; i < ParentProject.Executors.Count; i++)
                {
                    executorsOptions[i] = $"Executor {ParentProject.Executors[i].Name}";
                }

                executorsOptions[ParentProject.Executors.Count] = "sep";
                executorsOptions[ParentProject.Executors.Count + 1] = "Back to the task menu";

                // Назначение делегата вызовов из меню.
                MenuCalls menuCalls = new MenuCalls(AssignExecutorListCalls);

                // Создание меню.
                InteractiveMenu.CreateMenu(executorsOptions, menuCalls, 4, 0, 'v', "SELECT AN EXECUTOR TO ASSIGN");
            }
        }

        /// <summary>
        /// Метод вызовов из меню задачи типа Task.
        /// </summary>
        /// <param name="activeElement">Индекс выбранного элемента.</param>
        public void AssignExecutorListCalls(int activeElement)
        {
            // Назначение выбранного исполнителя.
            if (activeElement <= ParentProject.Executors.Count - 1)
            {
                if (!UsersAssigned.Contains(ParentProject.Executors[activeElement]))
                    UsersAssigned.Add(ParentProject.Executors[activeElement]);
                PrintBugMenu(false);
            }
            // Возврат в меню задачи.
            else
            {
                PrintBugMenu(false);
            }
        }
    }
}
