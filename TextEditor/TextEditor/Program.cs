using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    static class Program
    {
        // Глобальный счётчик безымянных (новых несохранённых) файлов.
        public static int unnamedFileCount = 0;
        // Индекс глобальной темы (применяется ко всем окнам).
        private static int globalThemeIndex;

        // Индекс глобальной темы (применяется ко всем окнам) (свойство).
        public static int GlobalThemeIndex
        {
            get => globalThemeIndex;
            set
            {
                globalThemeIndex = value;
                ChangeFormThemes();
            }
        }

        /// <summary>
        /// Изменение глобальной темы форм.
        /// </summary>
        private static void ChangeFormThemes()
        {
            // Проход по всем открытым формам.
            foreach (MainWindow form in Application.OpenForms)
            {
                form.Invoke((MethodInvoker)(() =>
                {
                    // Загрузка темы и изменение выбранной опции в ComboBox.
                    form.LoadTheme();
                    form.themeComboBox.SelectedIndexChanged -= form.themeComboBox_SelectedIndexChanged;
                    form.themeComboBox.SelectedIndex = GlobalThemeIndex;
                    form.themeComboBox.SelectedIndexChanged += form.themeComboBox_SelectedIndexChanged;
                }));
            }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
