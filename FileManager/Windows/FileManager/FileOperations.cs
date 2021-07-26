// FileOperations.cs - методы выполнения операций с файлами.

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManager
{
    partial class Program
    {
        /// <summary>
        /// Метод, выводящий содержимое текстового файла в консоль.
        /// </summary>
        /// <param name="file"> Файл-субъект операции чтения.</param>
        /// <param name="encoding"> Кодировка (null если не указана)</param>
        static void PrintTxt(FileInfo file, Encoding encoding)
        {
            // Вывод пути к файлу.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ПУТЬ К ФАЙЛУ: " + Path.Combine(file.DirectoryName, file.Name));
            Console.ResetColor();

            // Вывод метки начала файла.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n~~ НАЧАЛО ФАЙЛА ~~\n");
            Console.ResetColor();

            // Попытка считать файл построчно и вывести его содержимое.
            try
            {
                string[] content;

                // Если кодировка не указана.
                if (encoding == null)
                {
                    content = File.ReadAllLines(Path.Combine(file.DirectoryName, file.Name));
                }

                // Если кодировка указана.
                else
                {
                    content = File.ReadAllLines(Path.Combine(file.DirectoryName, file.Name), encoding);
                }

                // Вывод содержимого файла.
                foreach (var line in content)
                {
                    Console.WriteLine(line);
                }

                // Вывод метки конца содержимого файла.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n~~ КОНЕЦ ФАЙЛА ~~\n");
                Console.ResetColor();
            }

            // Ловим исключения.
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
            }

            // Кнопка возврата в меню опций.
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Вернуться в меню опций");
            Console.ResetColor();
            Console.WriteLine();

            // Цикл ожидания нажатия кнопки Enter.
            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey();

                if (input.Key == ConsoleKey.Enter) break;
            }

            Console.Clear();

            // Возврат в меню опций.
            TxtMenu(file);
        }


        /// <summary>
        /// Метод, копирующий файлы в буфере копирования в указанную директорию.
        /// </summary>
        /// <param name="targetDir"> Целевая директория.</param>
        static void CopyTxt(DirectoryInfo targetDir)
        {

            // Попытка скопировать файлы.
            try
            {
                foreach (var file in buffer)
                {
                    // Формирование пути источника и целевого пути.
                    string sourcePath = Path.Combine(file.DirectoryName, file.Name);
                    string targetPath = Path.Combine(targetDir.FullName, file.Name);

                    // Копирование.
                    File.Copy(sourcePath, targetPath);
                }

                buffer.Clear();
            }

            // Ловим исключения.
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Метод, перемещающий файл в указанную директорию.
        /// </summary>
        /// <param name="sourceFile"> Файл-субъект операции.</param>
        /// <param name="targetDir"> Целевая директория.</param>
        static void MoveTxt(FileInfo sourceFile, DirectoryInfo targetDir)
        {
            // Формирование пути источника и целевого пути.
            string sourcePath = Path.Combine(sourceFile.DirectoryName, sourceFile.Name);
            string targetPath = Path.Combine(targetDir.FullName, sourceFile.Name);

            // Попытка переместить файл.
            try
            {
                // Перемещение файла.
                File.Move(sourcePath, targetPath);
            }

            // Ловим исключения.
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Метод, переименовывающий указанный файл.
        /// </summary>
        /// <param name="file"> Файл-субъект операции.</param>
        /// <param name="newName"> Новое имя файла.</param>
        static void RenameFile(FileInfo file, string newName)
        {
            // Формирование пути источника и его пути после переименования.
            string sourcePath = Path.Combine(file.DirectoryName, file.Name);
            string newPath = Path.Combine(file.DirectoryName, newName + file.Extension);

            // Попытка переименовать файл.
            try
            {
                // Переименование файла.
                File.Move(sourcePath, newPath);
            }

            // Ловим исключения.
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// Метод, выполняющий конкатенацию двух переданных в него файлов.
        /// </summary>
        /// <param name="firstFile"> Первый файл-субъект операции (к которому ведется конкатенация).</param>
        /// <param name="secondFile"> Второй файл-субъект операции (который конкатенируется к первому).</param>
        static void ConcatenateFiles(FileInfo firstFile, FileInfo secondFile)
        {
            // Вывод пути к файлам.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ПУТЬ К ПЕРВОМУ ФАЙЛУ: " + Path.Combine(firstFile.DirectoryName, firstFile.Name));
            Console.WriteLine("ПУТЬ КО ВТОРОМУ ФАЙЛУ: " + Path.Combine(secondFile.DirectoryName, secondFile.Name));
            Console.ResetColor();
            Console.WriteLine();

            // Массивы содержмиого файлов.
            string[] firstFileContent;
            string[] secondFileContent;

            // Массив содержимого итогового файла.
            string[] finalContent = null;

            // Попытка считать и сконкатенировать содержимое файлов.
            try
            {
                // Считывание и конкатенация содержимого файлов.
                firstFileContent = File.ReadAllLines(Path.Combine(firstFile.DirectoryName, firstFile.Name));
                secondFileContent = File.ReadAllLines(Path.Combine(secondFile.DirectoryName, secondFile.Name));
                finalContent = firstFileContent.Concat(secondFileContent).ToArray();

                // Вывод метки начала файла.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n~~ НАЧАЛО ФАЙЛА ~~\n");
                Console.ResetColor();

                // Вывод сконкатенированного содержимого двух файлов.
                foreach (var line in finalContent)
                {
                    Console.WriteLine(line);
                }

                // Вывод метки конца файла.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n~~ КОНЕЦ ФАЙЛА ~~\n");
                Console.ResetColor();
            }

            // Ловим исключения.
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }

            // Метка сохранения файла.
            bool isSaving = false;

            // Вывод меню с вопросом о сохранении файла.
            QuestionMenu(ref isSaving, "Сохранить результат конкатенации в данной папке?");

            // Если файл должен быть сохранён.
            if (isSaving)
            {
                // Ввод названия файла.
                Console.CursorVisible = true;
                Console.Write("Введите название файла: ");
                string fileName = Console.ReadLine();

                // Попытка записать содержимое в новый файл.
                try
                {
                    // Сохранение нового файла.
                    File.WriteAllLines(Path.Combine(firstFile.DirectoryName, fileName + firstFile.Extension), finalContent);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Файл {fileName + firstFile.Extension} успешно сохранён.");
                    Console.ResetColor();
                }

                // Ловим исключения.
                catch (Exception ex)
                {
                    // Вывод сообщения об ошибке.
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + ex.Message);
                    Console.ResetColor();
                }
                Console.WriteLine();
                Console.CursorVisible = false;
            }

            // Кнопка возврата в меню опций.
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Вернуться в меню опций");
            Console.ResetColor();
            Console.WriteLine();

            // Цикл ожидания нажатия клавиши Enter.
            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey();

                if (input.Key == ConsoleKey.Enter) break;
            }

            Console.Clear();

            // Вывод меню опций для первого файла.
            TxtMenu(firstFile);
        }


        /// <summary>
        /// Метод, удаляющий переданный в него файл.
        /// </summary>
        /// <param name="file"> Файл-субъект операции.</param>
        static void DeleteFile(FileInfo file)
        {
            // Метка желания пользователя удалить файл.
            bool isWilling = false;

            // Вывод на экран вопрос об удалении файла.
            QuestionMenu(ref isWilling, "Вы уверены, что хотите удалить файл?");

            // Если пользователь не желает удалить файл - выход из метода.
            if (!isWilling) return;

            // Формирование пути файла-субъекта (источника).
            string sourcePath = Path.Combine(file.DirectoryName, file.Name);

            // Попытка удаления файла.
            try
            {
                // Удаление файла.
                File.Delete(sourcePath);
            }

            // Ловим исключения.
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }
            Console.Clear();
        }


        /// <summary>
        /// Метод, выполняющий операцию создания файла.
        /// </summary>
        /// <param name="dir"> Директория, в которой создаётся файл.</param>
        static void CreateFile(DirectoryInfo dir)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Введите имя нового файла (имя.расширение): ");

            // Введённое пользователем имя и расширение.
            string input = Console.ReadLine();

            // Попытка создания файла.
            try {

                // Создание файла и его закрытие.
                var newFile = File.Create(Path.Combine(dir.FullName +  "/" + input));
                newFile.Close();

                Console.WriteLine();

                // Вывод сообщения об успешном создании файла.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Файл {input} успешно создан!");

                // Созданный файл.
                FileInfo createdFile = new FileInfo(Path.Combine(dir.FullName + "/" + input));

                Console.ResetColor();

                // Выполняется, если созданный файл - текстовый поддерживаемого расширения.
                if (input.Split('.')[1] == "txt" || input.Split('.')[1] == "log" || input.Split('.')[1] == "ini" || input.Split('.')[1] == "csv")
                {
                    // Выбор кодировки.
                    Encoding fileEncoding = Encoding.UTF8;
                    bool chooseEncoding = false;
                    QuestionMenu(ref chooseEncoding, "Желаете изменить кодировку файла со стандартной на иную?");

                    if (chooseEncoding)
                    {
                        DrawEncodingMenu(createdFile, false, true, ref fileEncoding);
                    }

                    // Запись текста при создании файла.
                    bool write = false;
                    QuestionMenu(ref write, "Желаете произвести запись в файл?");

                    if (write)
                    {
                        Console.Clear();
                        WriteToFile(createdFile, fileEncoding);
                    }
                }
            } catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nНажмите Enter, чтобы продолжить...");
                Console.ReadKey();
            }

            Console.Clear();
            DrawDir(dir);
        }


        /// <summary>
        /// Метод, выполняющий операцию записи в файл.
        /// </summary>
        /// <param name="targetFile"> Файл-субъект операции.</param>
        /// <param name="encoding"> Кодировка, в которой ведётся запись.</param>
        static void WriteToFile (FileInfo targetFile, Encoding encoding)
        {
            // Путь файла-субъека операции.
            string targetPath = Path.Combine(targetFile.DirectoryName, targetFile.Name);

            // Вывод подсказки на экран.
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Введите текст и напишите stop_input, чтобы окончить запись: ");
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.WriteLine();

            // Массив введённого текста.
            string[] content = new string[0];

            // Наполнение массива вводом.
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "stop_input") break;
                Array.Resize(ref content, content.Length + 1);
                content[content.Length - 1] = input;
            }

            // Попытка записать текст.
            try
            {
                // Запись.
                File.AppendAllLines(targetPath, content, encoding);

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Текст успешно записан в файл!");
            }
            // Ловим исключения.
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка: " + ex.Message);
                Console.ResetColor();
            }

            // Кнопка продолжения.
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Продолжить");
            Console.ResetColor();
            Console.WriteLine();

            // Цикл ожидания нажатия клавиши Enter.
            while (true)
            {
                ConsoleKeyInfo inputKey = Console.ReadKey();

                if (inputKey.Key == ConsoleKey.Enter) break;
            }

            Console.CursorVisible = false;
            Console.Clear();
        }
    }
}