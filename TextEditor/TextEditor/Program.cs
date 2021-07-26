using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    static class Program
    {
        // ���������� ������� ���������� (����� ������������) ������.
        public static int unnamedFileCount = 0;
        // ������ ���������� ���� (����������� �� ���� �����).
        private static int globalThemeIndex;

        // ������ ���������� ���� (����������� �� ���� �����) (��������).
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
        /// ��������� ���������� ���� ����.
        /// </summary>
        private static void ChangeFormThemes()
        {
            // ������ �� ���� �������� ������.
            foreach (MainWindow form in Application.OpenForms)
            {
                form.Invoke((MethodInvoker)(() =>
                {
                    // �������� ���� � ��������� ��������� ����� � ComboBox.
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
