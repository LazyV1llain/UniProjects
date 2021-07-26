// DrawMenus.cs - методы отрисовки различных меню программы - меню действий с файлами, меню помощи, меню-вопрос и т.д.

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManager
{
    partial class Program
    {

        /// <summary>
        /// Метод, выводящий на экран меню выбора кодировки для прочтения файла.
        /// </summary>
        /// <param name="file"> Файл-субъект операции.</param>
        /// <param name="read"> Метка режима чтения файла.</param>
        /// <param name="write"> Метка режима записи в файл.</param>
        /// <param name="encInfo"> Переменная для сохранения выбранной кодировки.</param>
        static void DrawEncodingMenu(FileInfo file, bool read, bool write, ref Encoding encInfo)
        {
            // Активный элемент меню - инициализация.
            int encodingItem = 0;

            Console.Write("Выберите кодировку: ");
            Console.WriteLine();

            // Массив элементов меню.
            string[] encodingOptions = { "UTF8", "UTF32", "Unicode", "ASCII", "Кодировка по умолчанию (ОС)" };

            // Цикл повторения решения для меню.
            while (true)
            {
                // Перемещение курсора.
                Console.SetCursorPosition(0, 3);

                // Вывод элеменов меню в консоль.
                for (int i = 0; i < encodingOptions.Length; i++)
                {
                    if (encodingItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(encodingOptions[i]);
                        Console.ResetColor();
                        Console.Write(" \t");
                    }
                    else
                    {
                        Console.Write(encodingOptions[i]);
                        Console.Write(" \t");
                    }
                }

                // Булева переменная - метка выхода из цикла выбора элемента меню и перехода к другим методам.
                bool exitEncMenu = false;

                EncodingMenuKeyHandler(ref encodingItem, out exitEncMenu, encodingOptions);

                // Выход из цикла выбора элемента меню и переход к другим методам.
                if (exitEncMenu)
                {
                    break;
                }
            }

            EncodingMenuOptions(file, encodingItem, read, write, encInfo);
        }


        /// <summary>
        /// Метод, выводящий в консоль меню действий над переданным в него нетекстовым файлом.
        /// </summary>
        /// <param name="file"> Файл-субъект действий.</param>
        static void NonTxtMenu(FileInfo file)
        {
            // Вывод пути к файлу-субъекту и информации о нём.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ПУТЬ К ФАЙЛУ: " + file.DirectoryName + "\\" + file.Name);

            Console.WriteLine("\nРасширение файла: " + file.Extension);
            Console.WriteLine("Размер файла: " + file.Length + " байт");
            Console.WriteLine("Последнее время записи: " + file.LastWriteTime);
            Console.WriteLine("Только для чтения: " + file.IsReadOnly.ToString());
            Console.ResetColor();

            // Активный элемент меню - инициализация.
            int activeOption = 0;

            // Булева переменная - метка выхода из цикла выбора элемента меню.
            bool exitMenu = false;

            Console.WriteLine();
            Console.Write("\rВыберите опцию: ");

            // Массив элементов меню.
            string[] options =
            {
                "Переименовать", "Копировать в...", "Переместить в...", "Удалить", "\nВернуться в директорию"
            };

            // Цикл повторения решения для меню.
            while (true)
            {
                // Перемещение курсора.
                Console.SetCursorPosition(0, 9);

                // Вывод элементов меню.
                for (int i = 0; i < options.Length; i++)
                {
                    if (activeOption == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(options[i]);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.Write(options[i]);
                        Console.WriteLine();
                    }
                }

                // Булева переменная - метка выхода из цикла выбора элемента меню.
                exitMenu = false;

                FileOptionsMenuKeyHandler(ref activeOption, out exitMenu, options);

                // Выход из цикла выбора элемента меню и переход в другие методы.
                if (exitMenu)
                {
                    break;
                }
            }

            NonTxtFileOptions(activeOption, file);
        }


        /// <summary>
        /// Метод, выводящий в консоль меню действий над переданным в него файлом.
        /// </summary>
        /// <param name="file">Файл-субъект действий</param>
        static void TxtMenu(FileInfo file)
        {
            // Вывод пути к файлу-субъекту и информации о нём.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ПУТЬ К ФАЙЛУ: " + file.DirectoryName + "\\" + file.Name);

            Console.WriteLine("\nРасширение файла: " + file.Extension);
            Console.WriteLine("Размер файла: " + file.Length + " байт");
            Console.WriteLine("Последнее время записи: " + file.LastWriteTime);
            Console.WriteLine("Только для чтения: " + file.IsReadOnly.ToString());
            Console.ResetColor();

            // Активный элемент меню - инициализация.
            int activeOption = 0;

            bool exitMenu = false;

            Console.WriteLine();
            Console.Write("\rВыберите опцию: ");

            // Массив элементов меню.
            string[] options =
            {
                "Открыть содержимое", "Открыть содержимое (с указанной кодировкой)", "Переименовать", "Копировать в...", "Переместить в...", "Удалить",
                "Конкатенировать с...", "Добавить текст в файл...", "\nВернуться в директорию"
            };

            // Цикл повторения решения для меню.
            while (true)
            {
                // Перемещение курсора.
                Console.SetCursorPosition(0, 9);

                // Вывод элементов меню.
                for (int i = 0; i < options.Length; i++)
                {
                    if (activeOption == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(options[i]);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.Write(options[i]);
                        Console.WriteLine();
                    }
                }

                // Булева переменная - метка выхода из цикла выбора элемента меню.
                exitMenu = false;

                FileOptionsMenuKeyHandler(ref activeOption, out exitMenu, options);

                // Выход из цикла выбора элемента меню и переход в другие методы.
                if (exitMenu)
                {
                    break;
                }
            }

            TxtFileOptions(activeOption, file);
        }


        /// <summary>
        /// Метод, выводящий на экран переданный в параметрах вопрос с опциями ответа "Да", "Нет".
        /// </summary>
        /// <param name="userResponse"> Переменная, хранящая ответ пользователя.</param>
        /// <param name="question"> Строка с вопросом.</param>
        static void QuestionMenu(ref bool userResponse, string question)
        {
            // Вывод в консоль вопроса.
            Console.WriteLine(question + "\n");

            // Опции - варианты ответа.
            string[] options = { "Да", "Нет" };

            // Активная опция.
            int selectedItem = 0;

            // Цикл повторения отрисовки опций.
            while (true)
            {
                Console.Write("\r");
                
                // Вывод опций в консоль.
                for (int i = 0; i < options.Length; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(options[i]);
                        Console.ResetColor();
                        Console.Write(" \t");
                    }
                    else
                    {
                        Console.Write(options[i]);
                        Console.Write(" \t");
                    }
                }

                // Нажатая пользователем клавиша.
                ConsoleKeyInfo input = Console.ReadKey();

                // Булева переменная - метка выхода из цикла выбора опции.
                bool exitMenu = false;

                // Интерпретация нажатой клавиши.
                switch (input.Key)
                {
                    // Если нажата стрелка вправо.
                    case ConsoleKey.RightArrow:
                        ClearKeyBuffer();
                        if (selectedItem == options.Length - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }

                        break;

                    // Если нажата стрелка влево.
                    case ConsoleKey.LeftArrow:
                        ClearKeyBuffer();
                        if (selectedItem == 0)
                        {
                            selectedItem = options.Length - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                        break;

                    // Если нажата клавиша Enter.
                    case ConsoleKey.Enter:
                        exitMenu = true;
                        break;
                }

                // Выход из цикла выбора опции.
                if (exitMenu)
                {
                    Console.Clear();
                    switch (selectedItem)
                    {
                        case 0:
                            userResponse = true;
                            break;
                        case 1:
                            userResponse = false;
                            break;
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// Метод, выводящий в консоль справку о программе ("помощь").
        /// </summary>
        /// <param name="dir"> Директория, из которой был вызван метод (null если вызван из списка дисков).</param>
        static void DrawHelp()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ CONSOLE FILE MANAGER ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n\n");


            Console.WriteLine("Данная программа предназначена для работы с файлами расширений .txt, .ini, .log и .csv.");
            Console.WriteLine("Пользовательский интерфейс преставен в виде набора меню и каталогов с директориями и файлами.");
            Console.WriteLine("Навигация и горячие клавиши: \n\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Общие клавиши:\n");
            Console.WriteLine("Стрелки вверх\\вниз \t\t - навигация вверх\\вниз по вертикальному меню.");
            Console.WriteLine("Стрелки влево\\вправо \t\t - навигация влево\\вправо по горизонтальному меню.");
            Console.WriteLine("Клавиша ENTER \t\t\t - выбор пункта меню\\переход в выделенную папку\\открытие меню действий с выделенным файлом.");
            Console.WriteLine("Клавиша C \t\t\t - (в директории) создать новый файл.");

            Console.WriteLine("\n\nГорячие клавиши:\n");
            Console.WriteLine("Клавиша F5 \t\t\t - возврат в предыдущую директорию (из каталога директории).");
            Console.WriteLine("Клавиша F6 \t\t\t - показать помощь.");
            Console.WriteLine("Клавиша F10 \t\t\t - выход из программы.");

            Console.WriteLine("\n\nРежимы копирования и перемещения файла:\n");
            Console.WriteLine("Клавиша Y \t\t\t - выбор текущей открытой директории в качестве целевой директории для копирования\\перемещения файла.");

            Console.WriteLine("\n\nРежим поиска второго файла для конкатенации:\n");
            Console.WriteLine("Клавиша ENTER \t\t\t - выбор выделенного файла в качестве второго файла для конкатенации.");


            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Продолжить");
            Console.ResetColor();
            Console.WriteLine();

            // Ожидание нажатия пользователем кнопки.
            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey();

                if (input.Key == ConsoleKey.Enter) break;
            }

            Console.Clear();
        }
    }
}