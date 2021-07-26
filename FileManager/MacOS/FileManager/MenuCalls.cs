// MenuCalls.cs - вызовы методов выполнения выбранных в разлчиных меню операций.

using System;
using System.IO;
using System.Net;
using System.Text;

namespace FileManager
{
    partial class Program
    {
        /// <summary>
        /// Метод вызова операций при выборе соответствующих пунктов каталога логических дисков.
        /// </summary>
        /// <param name="selectedItem"> Выбранный элемент меню.</param>
        static void DrivesMenuOptions(int selectedItem)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            // При выборе не последней опции.
            if (selectedItem < drives.Length)
            {
                Console.Clear();
                DrawDir(drives[selectedItem].RootDirectory);
            }
            else if (selectedItem == drives.Length)
            {
                Console.Clear();
                DrawHelp();
                DrawDrives();
            }
            // Выход из программы в случае выбора пункта "ВЫХОД".
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("До встречи!");
                Console.ResetColor();
                Environment.Exit(0);
            }
        }


        /// <summary>
        /// Метод вызова операций при выборе соответствующих пунктов каталога директории.
        /// </summary>
        /// <param name="selectedItem"> Выбранный элемент меню.</param>
        /// <param name="copyingOrMovingFinished"> Метка окончания процесса выбора места для копирования или перемещения файлов.</param>
        /// <param name="dir"> Директория, из которой вызван метод.</param>
        /// <param name="backToParent"> Метка возвращения в "родительскую" директорию.</param>
        /// <param name="files"> Массив файлов директории.</param>
        /// <param name="subdirs"> Массив субиректорий (папок) директории.</param>
        static void DirMenuOptions(int selectedItem, bool copyingOrMovingFinished, DirectoryInfo dir, bool backToParent, FileInfo[] files, DirectoryInfo[] subdirs)
        {
            // Выбор директории для копирования или перемещения файла.
            if (copyingOrMovingFinished && copying)
            {
                CopyTxt(dir);
                copying = false;
                Console.Clear();
                DrawDir(dir);
            }
            else if (copyingOrMovingFinished && moving)
            {
                MoveTxt(targetFile, dir);
                moving = false;
                Console.Clear();
                DrawDir(dir);
            }

            // Возврат в родительскую директорию.
            if ((selectedItem == 0 || backToParent) && dir.Parent == null)
            {
                Console.Clear();
                DrawDrives();
            }
            else if ((selectedItem == 0 || backToParent) && dir.Parent != null)
            {
                Console.Clear();
                DrawDir(dir.Parent);
            }

            // Выход из программы.
            if (selectedItem == files.Length + subdirs.Length + 1)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("До встречи!");
                Console.ResetColor();
                Environment.Exit(0);
            }

            // Переход в субдиректорию (папку).
            if (selectedItem > 0 && selectedItem <= subdirs.Length)
            {
                Console.Clear();
                DrawDir(subdirs[selectedItem - 1]);
            }

            // Выбор файла.
            if (selectedItem > subdirs.Length && selectedItem <= subdirs.Length + files.Length)
            {
                Console.Clear();

                // Выбор файла для конкатенации.
                if (concatenation)
                {
                    concatenation = false;
                    ConcatenateFiles(targetFile, files[selectedItem - subdirs.Length - 1]);
                }

                // Вывод меню действий над файлом.
                else
                {
                    // Если файл является текстовым файлом поддерживаемого расширения.
                    if (files[selectedItem - subdirs.Length - 1].Extension == ".txt"
                        || files[selectedItem - subdirs.Length - 1].Extension == ".csv"
                        || files[selectedItem - subdirs.Length - 1].Extension == ".log"
                        || files[selectedItem - subdirs.Length - 1].Extension == ".ini")
                    {
                        TxtMenu(files[selectedItem - subdirs.Length - 1]);
                    }

                    // Если файл не является текстовым файлом поддерживаемого расписания.
                    else
                    {
                        NonTxtMenu(files[selectedItem - subdirs.Length - 1]);
                    }
                }
            }
        }


        /// <summary>
        /// Метод вызова операций при выборе соответствующих пунктов меню выбора кодировки.
        /// </summary>
        /// <param name="file"> Файл-субъект операций.</param>
        /// <param name="encodingItem"> Выбранная кодировка.</param>
        /// <param name="read"> Метка режима чтения файла.</param>
        /// <param name="write"> Метка режима записи в файл.</param>
        /// <param name="encInfo"> Информация о кодировке.</param>
        static void EncodingMenuOptions(FileInfo file, int encodingItem, bool read, bool write, Encoding encInfo)
        {
            // Путь к файлу.
            string sourcePath = Path.Combine(file.DirectoryName, file.Name);
            switch (encodingItem)
            {
                // Выбрана кодировка UTF8.
                case 0:
                    Console.Clear();
                    if (read)
                    {
                        PrintTxt(file, Encoding.UTF8);
                    }
                    if (write)
                    {
                        File.Delete(sourcePath);
                        File.WriteAllText(sourcePath, string.Empty, Encoding.UTF8);
                        encInfo = Encoding.UTF8;
                    }
                    break;

                // Выбрана кодировка UTF32.
                case 1:
                    Console.Clear();
                    if (read)
                    {
                        PrintTxt(file, Encoding.UTF32);
                    }
                    if (write)
                    {
                        File.Delete(sourcePath);
                        File.WriteAllText(sourcePath, string.Empty, Encoding.UTF32);
                        encInfo = Encoding.UTF32;
                    }
                    break;

                // Выбрана кодировка Unicode.
                case 2:
                    Console.Clear();
                    if (read)
                    {
                        PrintTxt(file, Encoding.Unicode);
                    }
                    if (write)
                    {
                        File.Delete(sourcePath);
                        File.WriteAllText(sourcePath, string.Empty, Encoding.Unicode);
                        encInfo = Encoding.Unicode;
                    }
                    break;

                // Выбрана кодировка ASCII.
                case 3:
                    Console.Clear();
                    if (read)
                    {
                        PrintTxt(file, Encoding.ASCII);
                    }
                    if (write)
                    {
                        File.Delete(sourcePath);
                        File.WriteAllText(sourcePath, string.Empty, Encoding.ASCII);
                        encInfo = Encoding.ASCII;
                    }
                    break;

                // Выбрана кодировка по умолчанию.
                case 4:
                    Console.Clear();
                    if (read)
                    {
                        PrintTxt(file, Encoding.Default);
                    }
                    if (write)
                    {
                        File.Delete(sourcePath);
                        File.WriteAllText(sourcePath, string.Empty, Encoding.Default);
                    }
                    break;
            }
        }


        /// <summary>
        /// Метод вызова операций при выборе соответствующих пунктов меню действий с нетекстовым файлом.
        /// </summary>
        /// <param name="activeOption"> Выбранный элемент меню.</param>
        /// <param name="file"> Файл-субъект операций.</param>
        static void NonTxtFileOptions(int activeOption, FileInfo file)
        {
            switch (activeOption)
            {
                // Выбрана опция "Переименовать".
                case 0:
                    Console.SetCursorPosition(0, 19);
                    Console.CursorVisible = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Введите новое название: ");
                    Console.ResetColor();
                    string newName = Console.ReadLine();
                    RenameFile(file, newName);
                    Console.Clear();
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Копировать в...".
                case 1:
                    Console.Clear();
                    copying = true;
                    buffer.Add(file);
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Переместить в...".
                case 2:
                    Console.Clear();
                    moving = true;
                    targetFile = file;
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Удалить".
                case 3:
                    Console.Clear();
                    DirectoryInfo path = file.Directory;
                    DeleteFile(file);
                    DrawDir(path);
                    break;

                // Выбрана опция "Вернуться в директорию".
                case 4:
                    Console.Clear();
                    DrawDir(file.Directory);
                    break;
            }
        }


        /// <summary>
        /// Метод вызова операций при выборе соответствующих пунктов меню действий с текстовым файлом.
        /// </summary>
        /// <param name="activeOption"> Выбранный элемент меню.</param>
        /// <param name="file"> Файл-субъект операций.</param>
        static void TxtFileOptions(int activeOption, FileInfo file)
        {
            switch (activeOption)
            {
                // Выбрана опция "Открыть содержимое".
                case 0:
                    Console.Clear();
                    PrintTxt(file, Encoding.UTF8);
                    break;

                // Выбрана опция "Открыть содержимое (с указанной кодировкой)".
                case 1:
                    Console.Clear();
                    Encoding enc = Encoding.Default;
                    DrawEncodingMenu(file, true, false, ref enc);
                    break;

                // Выбрана опция "Переименовать".
                case 2:
                    Console.SetCursorPosition(0, 19);
                    Console.CursorVisible = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Введите новое название: ");
                    Console.ResetColor();
                    string newName = Console.ReadLine();
                    RenameFile(file, newName);
                    Console.Clear();
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Копировать в...".
                case 3:
                    Console.Clear();
                    copying = true;
                    buffer.Add(file);
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Переместить в...".
                case 4:
                    Console.Clear();
                    moving = true;
                    targetFile = file;
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Удалить".
                case 5:
                    Console.Clear();
                    DirectoryInfo path = file.Directory;
                    DeleteFile(file);
                    DrawDir(path);
                    break;

                // Выбрана опция "Конкатенировать с...".
                case 6:
                    Console.Clear();
                    concatenation = true;
                    targetFile = file;
                    DrawDir(file.Directory);
                    break;

                // Выбрана опция "Добавить текст в файл...".
                case 7:
                    Console.Clear();
                    WriteToFile(file, Encoding.Default);
                    TxtMenu(file);
                    break;

                // Выбрана опция "Вернуться в директорию".
                case 8:
                    Console.Clear();
                    DrawDir(file.Directory);
                    break;
            }
        }
    }
}