using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp
{
    /// <summary>
    /// Делегат для вызова методов активации вызовов из меню.
    /// </summary>
    /// <param name="activeElement">Индекс активного элемента.</param>
    delegate void MenuCalls(int activeElement);

    class InteractiveMenu
    {
        /// <summary>
        /// Цвет выделения фона активного элемента меню.
        /// </summary>
        public static ConsoleColor selectionBackColor = ConsoleColor.Yellow;

        /// <summary>
        /// Цвет выделения текста активного элемента меню.
        /// </summary>
        public static ConsoleColor selectionForeColor = ConsoleColor.Black;

        /// <summary>
        /// Метод интерпретации нажатий клавиш управления в меню.
        /// </summary>
        /// <param name="activeElement">Индекс активного элемента.</param>
        /// <param name="exitMenu">Флаг выхода из меню.</param>
        /// <param name="options">Массив опций меню.</param>
        /// <param name="menuDirection">Метка направления расположения опций меню.</param>
        static void MenuKeyHandler(ref int activeElement, out bool exitMenu, string[] options, char menuDirection)
        {
            // Нажатая пользователем клавиша.
            ConsoleKeyInfo input = Console.ReadKey();

            // Установка предварительных значений параметров (для вертикального меню).
            exitMenu = false;
            ConsoleKey nextOptionKey = ConsoleKey.DownArrow;
            ConsoleKey previousOptionKey = ConsoleKey.UpArrow;

            // Установка значений параметров для горизонтального меню.
            if (menuDirection == 'h')
            {
                nextOptionKey = ConsoleKey.RightArrow;
                previousOptionKey = ConsoleKey.LeftArrow;
            }

            // Если была нажата клавиша выбора следующей опции.
            if (input.Key == nextOptionKey)
            {
                Program.ClearKeyBuffer();

                // Переход к первой опции, если выбранная опция была последней (карусель).
                if (activeElement == options.Length - 1) activeElement = 0;
                else
                {
                    // "Скачок" через сепаратор.
                    if (options[activeElement + 1] == "sep") activeElement += 2;
                    // Выбор следующей опции.
                    else activeElement++;
                }
            // Если была нажата клавиша выбора предыдущей опции.
            } else if (input.Key == previousOptionKey)
            {
                Program.ClearKeyBuffer();

                // Переход к последней опции, если выбранная опция была первой (карусель).
                if (activeElement == 0) activeElement = options.Length - 1;
                else {
                    // "Скачок" через сепаратор.
                    if (options[activeElement - 1] == "sep") activeElement -= 2;
                    // Выбор предыдущей опции.
                    else activeElement--;
                }
            } else if (input.Key == ConsoleKey.Enter)
            {
                // Выход из меню при окончательной выборе опции.
                exitMenu = true;
            }
        }

        /// <summary>
        /// Метод создания интерактивного меню с указанными опциями в выделенных координатах.
        /// </summary>
        /// <param name="options">Опции меню.</param>
        /// <param name="menuCalls">Делегат вызова метода активации вызовов из меню.</param>
        /// <param name="verticalPos">Координата левого верхнего угла меню по вертикали.</param>
        /// <param name="leftPos">Координата левого верхнего угла меню по горизонтали.</param>
        /// <param name="menuDirection">Направление расположения опций меню.</param>
        /// <param name="label">Заголовок меню.</param>
        public static void CreateMenu(string[] options, MenuCalls menuCalls, int verticalPos, int leftPos, char menuDirection, string label)
        {
            // Отрисовка верхнего края рамки и заголовка меню для вертикального меню.
            if (menuDirection == 'v')
            {
                // Установка полодения курсора.
                Console.SetCursorPosition(leftPos, verticalPos);

                // Отрисовка края рамки.
                string sep = "╔" + new string('═', Console.WindowWidth - 2) + "╗";
                Console.Write(sep);
                Console.SetCursorPosition((Console.WindowWidth - label.Length - 2) / 2, Console.CursorTop);

                // Отрисовка заголовка.
                if (label != "") Console.WriteLine(" " + label + " ");
                else Console.WriteLine();
            }

            // Скрытие курсора.
            Console.CursorVisible = false;

            int activeElement = 0;

            // Цикл перерисовки меню.
            while (true)
            {
                // Установка положения курсора для начала отрисовки меню.
                Console.SetCursorPosition(leftPos, verticalPos + 1);

                for (int i = 0; i < options.Length; i++)
                {
                    // Если данная опция выбрана.
                    if (i == activeElement)
                    {
                        // Переключение выбора, если выбран сепаратор.
                        if (options[i] == "sep")
                        {
                            // Отрисовка сепаратора
                            if (menuDirection == 'h') Console.Write(" \t");
                            else
                            {
                                string s = new string('─', Console.WindowWidth - 2);
                                Console.Write("╟" + s + "╢" + Environment.NewLine);
                            }

                            // Переключение выбора.
                            activeElement++;
                            continue;
                        }

                        // Отрисовка границ рамки.
                        string spaces = new string(' ', Console.WindowWidth - 2);
                        string border = "║" + spaces + "║";
                        Console.Write(border);

                        // Отрисовка текста опции.
                        Console.SetCursorPosition(leftPos + 1, Console.CursorTop);
                        Console.ForegroundColor = selectionForeColor;
                        Console.BackgroundColor = selectionBackColor;
                        Console.Write("> " + options[i]);
                        Console.ResetColor();

                        // Переход в область отрисовки новой опции.
                        if (menuDirection == 'h') Console.Write(" \t");
                        else Console.Write(Environment.NewLine);
                    } 
                    // Если данная опция не выбрана.
                    else
                    {
                        // Отрисовка сепаратора.
                        if (options[i] == "sep")
                        {
                            if (menuDirection == 'h') Console.Write(" \t");
                            else
                            {
                                string s = new string('─', Console.WindowWidth - 2);
                                Console.Write("╟" + s + "╢" + Environment.NewLine);
                            }
                            continue;
                        }

                        // Отрисовка границ рамки.
                        string spaces = new string(' ', Console.WindowWidth - 2);
                        string border = "║" + spaces + "║";
                        Console.Write(border);

                        // Отрисовка текста опции.
                        Console.SetCursorPosition(leftPos + 1, Console.CursorTop);
                        Console.Write("  " + options[i]);

                        // Переход в область отрисовки новой опции.
                        if (menuDirection == 'h') Console.Write(" \t");
                        else Console.Write(Environment.NewLine);
                    }
                }

                // Отрисовка нижнего края рамки для вертикального меню.
                if (menuDirection == 'v')
                {
                    string sep = "╚" + new string('═', Console.WindowWidth - 2) + "╝";
                    Console.WriteLine(sep);
                }

                // Вызов обработчика нажатия клавиш.
                bool exitMenu = false;
                MenuKeyHandler(ref activeElement, out exitMenu, options, menuDirection);

                if (exitMenu) break;
            }

            // Вызов методов из меню.
            menuCalls?.Invoke(activeElement);
        }

        /// <summary>
        /// Метод возврата цветов выделения активной опции меню к значениям по умолчанию.
        /// </summary>
        public static void ResetMenuSelectionColor()
        {
            selectionBackColor = ConsoleColor.Yellow;
            selectionForeColor = ConsoleColor.Black;
        }

        /// <summary>
        /// Метод вывода на экран сообщения об ошибке (исключении).
        /// </summary>
        /// <param name="ex">Исключение.</param>
        public static void PrintException(Exception ex)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Вывод сообщения об ошибке.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + ex.Message);

            Console.WriteLine();

            // Вывод опции возврата и ожидание выбора пользователя.
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.Write("Return");
            Console.ResetColor();
            Console.WriteLine();

            // Обработка нажатия клавиши пользователем.
            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey();

                if (input.Key == ConsoleKey.Enter) break;
            }

            Console.Clear();
        }

        /// <summary>
        /// Метод создания меню выбора запрошенного значения (на собственном экране).
        /// </summary>
        /// <param name="label">Заголовок меню.</param>
        /// <param name="message">Сообщение с описанием запроса.</param>
        /// <param name="options">Опции меню.</param>
        /// <param name="menuDirection">Направление расположения опций меню.</param>
        /// <returns>Выбранная опция.</returns>
        public static string ReplyMenu(string label, string message, string[] options, char menuDirection)
        {
            Console.Clear();
            Console.CursorVisible = false;

            // Отрисовка заголовка меню.
            Console.SetCursorPosition((Console.WindowWidth - label.Length) / 2, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(label);
            Console.ResetColor();
            Console.WriteLine();

            // Отрисовка сообщения.
            Console.WriteLine(message);

            int activeElement = 0;

            while (true)
            {
                // Установка положения курсора.
                Console.SetCursorPosition(0, 3);

                // Цикл отрисовки опций меню.
                for (int i = 0; i < options.Length; i++)
                {
                    // Если данная опция выбрана.
                    if (i == activeElement)
                    {
                        // Отрсиовка сепаратора.
                        if (options[i] == "sep")
                        {
                            if (menuDirection == 'h') Console.Write(" \t");
                            else Console.Write(Environment.NewLine);
                            continue;
                        }

                        // Отрисовка текста опции.
                        Console.ForegroundColor = selectionForeColor;
                        Console.BackgroundColor = selectionBackColor;
                        Console.Write("> " + options[i]);
                        Console.ResetColor();

                        // Переход в область отрисовки следующей опции.
                        if (menuDirection == 'h') Console.Write(" \t");
                        else Console.Write(Environment.NewLine);
                    }
                    // Если данная опция не выбрана.
                    else
                    {
                        // Отрисовка сепаратора.
                        if (options[i] == "sep")
                        {
                            if (menuDirection == 'h') Console.Write(" \t");
                            else Console.Write(Environment.NewLine);
                            continue;
                        }

                        // Отрисовка текста опции.
                        Console.Write("  " + options[i]);

                        // Переход в область отрисовки следующей опции.
                        if (menuDirection == 'h') Console.Write(" \t");
                        else Console.Write(Environment.NewLine);
                    }
                }

                // Вызов обработчика нажатия клавиш.
                bool exitMenu = false;
                MenuKeyHandler(ref activeElement, out exitMenu, options, menuDirection);

                if (exitMenu) break;
            }

            // Возврат выбранной опции.
            return options[activeElement];
        }

        /// <summary>
        /// Метод создания горизонтального меню выбора запрошенного значения (в данных координатах).
        /// </summary>
        /// <param name="message">Сообщение с описанием запроса.</param>
        /// <param name="options">Опции меню.</param>
        /// <param name="verticalPos">Координата левого верхнего угла меню по вертикали.</param>
        /// <param name="horizPos">Координата левого верхнего угла меню по горизонтали.</param>
        /// <returns>Выбранная опция.</returns>
        public static string ReplyMenu(string message, string[] options, int verticalPos, int horizPos)
        {
            // Вывод сообщения.
            Console.SetCursorPosition(horizPos, verticalPos);
            Console.Write(message);

            int activeElement = 0;

            while (true)
            {
                // Установка положения курсора за сообщением.
                Console.SetCursorPosition(message.Length + 2, verticalPos);

                // Цикл отрисовки опций.
                for (int i = 0; i < options.Length; i++)
                {
                    // Если данная опция выбрана.
                    if (i == activeElement)
                    {
                        // Отрисовка сепаратора.
                        if (options[i] == "sep")
                        {
                            Console.Write(" \t");
                            continue;
                        }

                        // Отрисовка текста опции.
                        Console.ForegroundColor = selectionForeColor;
                        Console.BackgroundColor = selectionBackColor;
                        Console.Write("> " + options[i]);

                        // Переход в область отрисовки следующей опции.
                        Console.ResetColor();
                        Console.Write(" \t");
                    }
                    // Если данная опция не выбрана.
                    else
                    {
                        // Отрисовка сепаратора.
                        if (options[i] == "sep")
                        {
                            Console.Write(" \t");
                            continue;
                        }

                        // Отрисовка текста опции.
                        Console.Write("  " + options[i]);

                        // Переход в область отрисовки следующей опции.
                        Console.Write(" \t");
                    }
                }

                // Вызов обработчика нажатия клавиш.
                bool exitMenu = false;
                MenuKeyHandler(ref activeElement, out exitMenu, options, 'h');

                if (exitMenu) break;
            }

            // Возврат выбранной опции.
            return options[activeElement];
        }
    }
}
