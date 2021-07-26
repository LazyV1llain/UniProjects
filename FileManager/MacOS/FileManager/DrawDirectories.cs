// DrawDirectories.cs - методы отрисовки логических дисков и директорий.

using System;
using System.IO;
using System.Linq;

namespace FileManager
{
    partial class Program
    {
        /// <summary>
        /// Метод, выводящий в консоль интерактивный каталог логических дисков компьютера.
        /// </summary>
        static void DrawDrives()
        {
            // Выключение видимости курсора.
            Console.CursorVisible = false;
            Console.Clear();

            // Инициализация основных переменных - индекса выбранного элемента и массива информации о логических дисках.
            int selectedItem = 0;
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Цикл повторения решения для каталога дисков.
            while (true)
            {
                // Перемещение курсора в начало каталога.
                Console.SetCursorPosition(Console.WindowTop, Console.WindowLeft);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Тома (партиции):");
                Console.ResetColor();
                Console.WriteLine();

                // Вывод метки режима копирования, перемещения и конкатенации файла.
                if (copying || moving || concatenation)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    if (copying)
                    {
                        Console.WriteLine("~ Выбор целевой директории для копирования ~");
                    }
                    else if (moving)
                    {
                        Console.WriteLine("~ Выбор целевой директории для перемещения ~");
                    }
                    else if (concatenation)
                    {
                        Console.WriteLine("~ Выбор второго файла для конкатенации ~");
                    }
                    Console.ResetColor();
                    Console.WriteLine();
                }

                // Вывод каталога логических дисков (активный пункт каталога выделяется цветом).
                for (int i = 0; i < drives.Length; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(drives[i].Name);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(drives[i].Name);
                    }
                }

                // Вывод пункта с помощью.
                if (selectedItem == drives.Length)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("\n\nПОМОЩЬ");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("\n\nПОМОЩЬ");
                }

                // Вывод пункта выхода из программы.
                if (selectedItem == drives.Length + 1)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.WriteLine("ВЫХОД");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("ВЫХОД");
                }

                bool exitMenu;

                DrivesMenuKeyHandler(out exitMenu, ref selectedItem);

                // Выход из выбора значений каталога и переход в иные методы.
                if (exitMenu)
                {
                    break;
                }
            }

            DrivesMenuOptions(selectedItem);
        }


        /// <summary>
        /// Метод, получающий и возвращающий информацию о содержимом директории.
        /// </summary>
        /// <param name="dir"> Директория.</param>
        /// <param name="filesArray"> Массив файлов в директории.</param>
        /// <param name="subdirsArray"> Массив субдиректорий (папок) в директории.</param>
        static void GetDirInfo(DirectoryInfo dir, ref FileInfo[] filesArray, ref DirectoryInfo[] subdirsArray)
        {

            if (textOnly)
            {
                try
                {
                    // Получение полного массива файлов в директории, имеющих расширение .txt, .csv и .ini.
                    filesArray = dir.GetFiles("*.txt").Concat(dir.GetFiles("*.csv")).Concat(dir.GetFiles("*.ini"))
                        .Concat(dir.GetFiles("*.log")).ToArray();
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об ошибке.
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + ex.Message);
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                    Console.ReadKey();
                    Console.Clear();
                    if (dir.Parent != null) DrawDir(dir.Parent);
                    else DrawDrives();
                }
            }
            else
            {
                try
                {
                    filesArray = dir.GetFiles();
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об ошибке.
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + ex.Message);
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                    Console.ReadKey();
                    Console.Clear();
                    if (dir.Parent != null) DrawDir(dir.Parent);
                    else DrawDrives();
                }
            }

            // Получение массива папок, содержащихся в директории.
            subdirsArray = dir.GetDirectories();
        }


        /// <summary>
        /// Метод вывода статуса буфера копирования.
        /// </summary>
        static void PrintBufferStatus()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{buffer.Count} ФАЙЛ(А\\ОВ) В БУФЕРЕ КОПИРОВАНИЯ: ");

            foreach (var file in buffer)
            {
                Console.Write(file.Name + " \t");
            }
            Console.WriteLine();
            Console.ResetColor();
        }


        /// <summary>
        /// Метод вывода подсказок в различных режимах.
        /// </summary>
        static void PrintModeHints()
        {
            if (copying || moving || concatenation)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (copying)
                {
                    Console.Write("~ Нажмите Y в целевой директории для копирования файла в неё ~");
                }
                else if (moving)
                {
                    Console.Write("~ Нажмите Y в целевой директории для перемещения файла в неё ~");
                }
                else if (concatenation)
                {
                    Console.Write("~ Нажмите Enter при выборе файла для конкатенации с ним ~");
                }
                Console.ResetColor();
                Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("~ Нажмите C в директории, чтобы создать в ней новый файл. ~");
                Console.ResetColor();
                Console.WriteLine();
            }
        }


        /// <summary>
        /// Метод, выводящий в консоль интерактивный каталог содержимого директории.
        /// </summary>
        /// <param name="dir"> Директория.</param>
        static void DrawDir(DirectoryInfo dir)
        {
            // По умолчанию первый элемент каталога становится активным.
            int selectedItem = 0;

            // Выключение видимости курсора.
            Console.CursorVisible = false;

            // Инициализация массива файлов директории.
            FileInfo[] files = null;

            // Инициализация массива папок директории.
            DirectoryInfo[] subdirs = null;

            // Получение информации о содержимом директории.
            GetDirInfo(dir, ref files, ref subdirs);

            // Булева переменная - метка выхода из каталога.
            bool exitMenu = false;

            // Булева переменная - метка окончания выбора точки назначения для копирования или перемещения файла.
            bool copyingOrMovingFinished = false;

            // Булева переменная - метка возврата к директории-"родителю".
            bool backToParent = false;

            // Цикл повторения решения для каталога содержимого директории.
            while (true)
            {
                Console.Clear();

                // Вывод пути текущей директории.
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("ПУТЬ ТЕКУЩЕЙ ДИРЕКТОРИИ: " + dir + Environment.NewLine);
                Console.ResetColor();

                PrintModeHints();

                if (copying)
                {
                    PrintBufferStatus();
                }

                // Вывод опции возврата в директорию-"родитель".
                if (selectedItem == 0)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("..\\");
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("..\\");
                }

                // Вывод папок, содержащихся в директории (шапка выводится только в том случае, если в директории есть папки).
                if (subdirs.Length != 0)
                {
                    Console.WriteLine("====================================== ДИРЕКТОРИИ ========================================");
                }
                for (int i = 0; i < subdirs.Length; i++)
                {
                    if (selectedItem == i + 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(subdirs[i].Name);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(subdirs[i].Name);
                    }
                }
                Console.WriteLine(Environment.NewLine + Environment.NewLine);

                // Вывод файлов, содержащихся в директории (шапка выводится только в том случае, если в директории есть файлы).
                if (files.Length != 0)
                {
                    Console.WriteLine("========================================== ФАЙЛЫ ==========================================");
                    Console.WriteLine("Имя файла \tРасширение \tРазмер (байты\\килобайты) \tВремя последнего изменения");
                }
                for (int i = 0; i < files.Length; i++)
                {
                    if (selectedItem == i + subdirs.Length + 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;

                        // Размер файла - отображается в байтах, если его размер меньше 1 КБ, и в килобайтах в ином случае.
                        string fileSize = ((files[i].Length < 1000) ? files[i].Length.ToString() + " B" : ((double)(files[i].Length / 1000)).ToString() + " KB");

                        Console.Write(files[i].Name + "\t" + files[i].Extension + "\t\t" + fileSize + "\t\t\t\t" + files[i].LastWriteTime);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        // Размер файла - отображается в байтах, если его размер меньше 1 КБ, и в килобайтах в ином случае.
                        string fileSize = ((files[i].Length < 1000) ? files[i].Length.ToString() + " B" : ((double)(files[i].Length / 1000)).ToString() + " KB");

                        Console.Write(files[i].Name + "\t" + files[i].Extension + "\t\t" + fileSize + "\t\t\t\t" + files[i].LastWriteTime);
                        Console.WriteLine();
                    }
                }

                // Вывод пункта выхода из программы.
                if (selectedItem == files.Length + subdirs.Length + 1)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("\nВЫХОД");
                    Console.ResetColor();
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("\nВЫХОД");
                }

                Console.SetCursorPosition(0, selectedItem);

                // Булева переменная - метка выхода из каталога.
                exitMenu = false;

                // Булева переменная - метка окончания выбора точки назначения для копирования или перемещения файла.
                copyingOrMovingFinished = false;

                // Булева переменная - метка возврата к директории-"родителю".
                backToParent = false;

                DirMenuKeyHandler(dir, out exitMenu, out copyingOrMovingFinished, out backToParent, ref selectedItem, files, subdirs);

                // Выход из выбора опций каталога и переход в другие методы.
                if (exitMenu)
                {
                    break;
                }
            }

            DirMenuOptions(selectedItem, copyingOrMovingFinished, dir, backToParent, files, subdirs);
        }
    }
}