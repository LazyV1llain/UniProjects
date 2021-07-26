// Это основной файл программы, содержащий метод Main() и важные статические поля программы, а так же один служебный метод.
// Иные методы категоризованы и разбиты на следующие файлы:

// DrawDirectories.cs - методы отрисовки логических дисков и директорий.
// DrawMenus.cs - методы отрисовки различных меню программы - меню действий с файлами, меню помощи, меню-вопрос и т.д.
// MenuKeyHandlers.cs - методы интерпретации нажатых клавиш для различных меню программы.
// MenuCalls.cs - вызовы методов выполнения выбранных в разлчиных меню операций.
// FileOperations.cs - методы выполнения операций с файлами.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace FileManager
{
    partial class Program
    {
        // Метка режима копирования файла.
        public static bool copying = false;

        // Метка режима перемещения файла.
        public static bool moving = false;

        // Метка режима конкатенации файла.
        public static bool concatenation = false;

        // Файл-субъект операции.
        public static FileInfo targetFile = null;

        // Метка режима работы только с текстовыми файлами.
        public static bool textOnly = false;

        // Буфер копирования (обмена).
        public static List<FileInfo> buffer = new List<FileInfo>();


        /// <summary>
        /// Метод очистки буфера ввода клавиш.
        /// Используеся в меню и каталогах для избежания бесконечной прокрутки при зажатии клавиши.
        /// </summary>
        public static void ClearKeyBuffer()
        {
            while (Console.KeyAvailable) Console.ReadKey(false);
        }

        static void Main(string[] args)
        {
            // Вызов меню справки (с переходом в каталог дисков).
            DrawHelp();

            QuestionMenu(ref textOnly, "Желаете ли вы работать только с текстовыми файлами? (Иные файлы будут автоматически скрыты)");
            DrawDrives();
        }
    }
}
