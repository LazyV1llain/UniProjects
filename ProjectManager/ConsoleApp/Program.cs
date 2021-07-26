using System;
using ClassLibrary;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ConsoleApp
{
    class Program
    {
        /// <summary>
        /// Путь к файлу сохранения открытых проектов.
        /// </summary>
        public static string SavePath = @"Projects\projects.txt";

        /// <summary>
        /// Метод для очистки буфера нажатых клавиш для избежания эффекта залипания в меню.
        /// </summary>
        public static void ClearKeyBuffer()
        {
            while (Console.KeyAvailable) Console.ReadKey(false);
        }

        /// <summary>
        /// Список проектов.
        /// </summary>
        public static List<Project_Console> projectList = new List<Project_Console>();

        /// <summary>
        /// Обработчик события закрытия консольного приложения.
        /// </summary>
        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            // Сериализация и сохранение открытых проектов.
            Project_Console.SerializeOpenProjects();
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // Десериализация ранее открытых проектов.
            Project_Console.DeserializeProjects();
            // Вывод списка проектов.
            Project_Console.PrintProjectsList(false);
        }
    }
}
