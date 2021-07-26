// MenuKeyHandlers.cs - методы интерпретации нажатых клавиш для различных меню программы.

using System;
using System.IO;

namespace FileManager
{
    partial class Program
    {
        static void DrivesMenuKeyHandler(out bool exitMenu, ref int selectedItem)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Нажатая пользователем клавиша.
            ConsoleKeyInfo input = Console.ReadKey();

            // Булева переменная - метка выхода из выбора значений каталога.
            exitMenu = false;

            // Проверка нажатой клавиши.
            switch (input.Key)
            {
                // При нажатой стрелке вниз.
                case ConsoleKey.DownArrow:
                    if (selectedItem == drives.Length + 1)
                    {
                        selectedItem = 0;
                    }
                    else
                    {
                        selectedItem++;
                    }

                    break;

                // При нажатой стрелке вверх.
                case ConsoleKey.UpArrow:
                    if (selectedItem == 0)
                    {
                        selectedItem = drives.Length + 1;
                    }
                    else
                    {
                        selectedItem--;
                    }

                    break;

                // При нажатой клавише Enter.
                case ConsoleKey.Enter:
                    exitMenu = true;
                    break;
            }
        }

        static void DirMenuKeyHandler(DirectoryInfo dir, out bool exitMenu, out bool copyingOrMovingFinished, 
            out bool backToParent, ref int selectedItem, FileInfo[] files, DirectoryInfo[] subdirs)
        {
            // Клавиша, нажатая пользователем.
            ConsoleKeyInfo input = Console.ReadKey();

            // Булева переменная - метка выхода из каталога.
            exitMenu = false;

            // Булева переменная - метка окончания выбора точки назначения для копирования или перемещения файла.
            copyingOrMovingFinished = false;

            // Булева переменная - метка возврата к директории-"родителю".
            backToParent = false;

            // Интерпретация нажатия кнопки.

            // Если нажата кнопка Y и идёт процесс выбора места для копирования или перемещения файла.
            if (input.Key == ConsoleKey.Y && (copying || moving))
            {
                copyingOrMovingFinished = true;
                exitMenu = true;
            }

            // Если нажата стрелка вниз.
            else if (input.Key == ConsoleKey.DownArrow)
            {
                ClearKeyBuffer();
                if (selectedItem == files.Length + subdirs.Length + 1)
                {
                    selectedItem = 0;
                }
                else
                {
                    selectedItem++;
                }
            }

            // Если нажата стрелка вверх.
            else if (input.Key == ConsoleKey.UpArrow)
            {
                ClearKeyBuffer();
                if (selectedItem == 0)
                {
                    selectedItem = files.Length + subdirs.Length + 1;
                }
                else
                {
                    selectedItem--;
                }
            }

            // Если нажата клавиша Enter.
            else if (input.Key == ConsoleKey.Enter)
            {
                exitMenu = true;
            }

            // Горячие клавиши F5, F6, F10.
            else if (input.Key == ConsoleKey.F5)
            {
                backToParent = true;
                exitMenu = true;
            }
            else if (input.Key == ConsoleKey.F6)
            {
                Console.Clear();
                DrawHelp();
            }
            else if (input.Key == ConsoleKey.F10)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("До встречи!");
                Console.ResetColor();
                Environment.Exit(0);

                // Нажатие клавиши С - создание файла.
            }
            else if (!copying && !moving && !concatenation && input.Key == ConsoleKey.C)
            {
                Console.Clear();
                CreateFile(dir);
            }
        }

        static void EncodingMenuKeyHandler(ref int encodingItem, out bool exitEncMenu, string[] encodingOptions)
        {
            // Нажатая пользователем клавиша.
            ConsoleKeyInfo inputEnc = Console.ReadKey();

            // Булева переменная - метка выхода из цикла выбора элемента меню и перехода к другим методам.
            exitEncMenu = false;

            // Интерпретация нажатой клавиши.
            switch (inputEnc.Key)
            {
                // Если нажата стрелка вправою
                case ConsoleKey.RightArrow:
                    ClearKeyBuffer();
                    if (encodingItem == encodingOptions.Length - 1)
                    {
                        encodingItem = 0;
                    }
                    else
                    {
                        encodingItem++;
                    }

                    break;

                // Если нажата стрелка влево.
                case ConsoleKey.LeftArrow:
                    ClearKeyBuffer();
                    if (encodingItem == 0)
                    {
                        encodingItem = encodingOptions.Length - 1;
                    }
                    else
                    {
                        encodingItem--;
                    }

                    break;

                // Если нажата клавиша Enter.
                case ConsoleKey.Enter:
                    exitEncMenu = true;
                    break;
            }
        }

        static void FileOptionsMenuKeyHandler(ref int activeOption, out bool exitMenu, string[] options)
        {
            // Нажатая пользователем клавиша.
            ConsoleKeyInfo input = Console.ReadKey();

            // Булева переменная - метка выхода из цикла выбора элемента меню.
            exitMenu = false;

            // Интерпретация нажатой клавиши.
            switch (input.Key)
            {
                // Если нажата стрелка вниз.
                case ConsoleKey.DownArrow:
                    ClearKeyBuffer();
                    if (activeOption == options.Length - 1)
                    {
                        activeOption = 0;
                    }
                    else
                    {
                        activeOption++;
                    }

                    break;

                // Если нажата стрелка вверх.
                case ConsoleKey.UpArrow:
                    ClearKeyBuffer();
                    if (activeOption == 0)
                    {
                        activeOption = options.Length - 1;
                    }
                    else
                    {
                        activeOption--;
                    }

                    break;

                // Если нажата клавиша Enter.
                case ConsoleKey.Enter:
                    exitMenu = true;
                    break;
            }
        }
    }
}