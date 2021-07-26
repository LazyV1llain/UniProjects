using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TextEditor
{
    public partial class MainWindow : Form
    {
        #region Объявление путей к файлам и папкам, используемым программой.
        // Путь к файлу настроек программы.
        const string settingsPath = @"Data\settings.txt";
        // Путь к компилятору.
        string compilerPath = @"";
        // Путь к папке с временно используемыми файлами.
        string temporaryFolder = @"Tmp";
        #endregion

        #region Объявление списков кортежей - информации о файлах и вкладках.
        // Список кортежей с информацией об открытых файлах. Item1 - путь к файлу, Item2 - наличие несохранённых изменений.
        public List<Tuple<string, bool>> fileChangeList = new List<Tuple<string, bool>>();
        /* Список кортежей с информацией об открытых вкладках.
         * Item1 - Панель-"граница" вкладки на панели вкладок (является корнем вкладки).
         * Item2 - Панель-"тело" вкладки на панели вкладок (содержит заголовок вкладки).
         * Item3 - RichTextBox, ассоциированный с вкладкой и содержащий содержимое соответствующего файла, если он не является файлом .cs.
         * Item4 - булево значение, флаг выбранной вкладки (true, если вкладка активна).
         * Item5 - FastColoredTextBox, ассоциированный с вкладкой и содержащий содержимое соответствующего файла, если он является файлом .cs. 
         * Индекс кортежа с информацией о конкретной вкладке совпадает с индексом кортежа с информацией об открытом в ней файле. */
        public List<Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>> tabInfo 
            = new List<Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>>();
        #endregion

        #region Объявление стилей подсветки синтаксиса (для FastColoredTextBox).
        // Цвет подсветки имён переменных.
        static Color variableColor = Color.OrangeRed;
        // Цвет подсветки имён методов.
        static Color methodColor = Color.DeepSkyBlue;
        // Цвет подсветки имён классов.
        static Color classColor = Color.Orange;
        // Цвет подсветки ключевых слов.
        static Color keywordColor = Color.Blue;
        // Цвет подсветки строк.
        static Color stringColor = Color.Brown;

        // Кисть подсветки имён переменных.
        static Brush variableBrush = new SolidBrush(variableColor);
        // Кисть подсветки имён методов.
        static Brush methodBrush = new SolidBrush(methodColor);
        // Кисть подсветки имён классов.
        static Brush classBrush = new SolidBrush(classColor);
        // Кисть подсветки ключевых слов.
        static Brush keywordBrush = new SolidBrush(keywordColor);
        // Кисть подсветки строк.
        static Brush stringBrush = new SolidBrush(stringColor);

        // Стиль подсветки имён переменных.
        FastColoredTextBoxNS.Style VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);
        // Стиль подсветки имён методов.
        FastColoredTextBoxNS.Style MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);
        // Стиль подсветки имён классов.
        FastColoredTextBoxNS.Style ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);
        // Стиль подсветки ключевых слов.
        FastColoredTextBoxNS.Style KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);
        // Стиль подсветки строк.
        FastColoredTextBoxNS.Style StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Italic);
        #endregion

        #region Объявление текстовых областей.
        // Активный RichTextBox (ассоциированный с выбранной вкладкой).
        RichTextBox activeTextBox;
        // Текущий шрифт активной области.
        Font currentFont;
        // Активный RichTextBox сообщений компилятора (ассоциированный с выбранной вкладкой и возникающий при компиляции файла).
        RichTextBox activeMessageTextBox;
        // Активный FastColoredTextBox (ассоциированный с выбранной вкладкой).
        FastColoredTextBoxNS.FastColoredTextBox activeCodeTextBox;
        #endregion

        #region Объявление флагов.
        // Флаг режима загрузки открытых ранее файлов при открытии формы.
        bool loadPrevFiles = true;
        // Флаг наличия открытой RichTextBox сообщений компилятора.
        bool messageTextBoxShown = false;
        #endregion

        #region Объявление цветов элементов для различных тем.
        // Словарь цветов элементов для светлой темы.
        Dictionary<string, Color> lightThemeColors = new Dictionary<string, Color>()
        {
            ["MenuBackColor"] = Color.White,
            ["MenuForeColor"] = SystemColors.ControlText,
            ["ToolPanelBackColor"] = Color.FromArgb(234, 234, 236),
            ["TabBorderBackColor_Active"] = Color.FromArgb(80, 112, 231),
            ["TabBorderBackColor_Inactive"] = Color.FromArgb(217, 217, 219),
            ["TabBaseBackColor_Active"] = Color.White,
            ["TabBaseBackColor_Inactive"] = Color.FromArgb(234, 234, 236),
            ["TabLabelForeColor_Active"] = SystemColors.ControlText,
            ["TabLabelForeColor_Inactive"] = Color.FromArgb(165, 165, 167),
            ["RichTextBoxBackColor"] = Color.White
        };

        // Словарь цветов элементов для тёмной темы.
        Dictionary<string, Color> darkThemeColors = new Dictionary<string, Color>()
        {
            ["MenuBackColor"] = Color.FromArgb(33, 36, 43),
            ["MenuForeColor"] = Color.FromArgb(216, 221, 227),
            ["ToolPanelBackColor"] = Color.FromArgb(33, 36, 43),
            ["TabBorderBackColor_Active"] = Color.FromArgb(80, 112, 231),
            ["TabBorderBackColor_Inactive"] = Color.FromArgb(23, 26, 33),
            ["TabBaseBackColor_Active"] = Color.FromArgb(40, 44, 53),
            ["TabBaseBackColor_Inactive"] = Color.FromArgb(33, 36, 43),
            ["TabLabelForeColor_Active"] = Color.White,
            ["TabLabelForeColor_Inactive"] = Color.FromArgb(118, 123, 129),
            ["RichTextBoxBackColor"] = Color.FromArgb(40, 44, 53)
        };
        #endregion

        #region Методы функционала формы.

        /// <summary>
        /// Конструктор формы.
        /// </summary>
        public MainWindow()
        {
            if (Application.OpenForms.Count == 0) Application.Run(new Splash());

            InitializeComponent();

            // Активация элементов управления шрифтом.
            SwitchFontControls(true);

            // Установка минимального размера окна.
            MinimumSize = new Size(Screen.PrimaryScreen.Bounds.Width / 3, Screen.PrimaryScreen.Bounds.Height / 3);

            // Установка значений по умолчанию.
            loggingToolStripComboBox.SelectedIndex = 0;
            intervalComboBox.SelectedIndex = 0;
            themeComboBox.SelectedIndex = 0;
            autosaveTimer.Interval = 120000;
            loggingTimer.Interval = 120000;
            compilerPathToolStripTextBox.Text = compilerPath;

            // Загрузка настроек.
            LoadSettings(loadPrevFiles);
        }

        /// <summary>
        /// Метод, вызывемый при закрытии формы.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                // Проверка открытых файлов на наличией несохранённых изменений.
                if (fileChangeList[i].Item2 == true)
                {
                    // Уведомления пользователя и выбор дальнейших действий.
                    DialogResult result = MessageBox.Show($"There are unsaved changes in {System.IO.Path.GetFileName(fileChangeList[i].Item1)}. Would you like to save them before closing?",
                        "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    switch (result)
                    {
                        // Сохранение изменений.
                        case DialogResult.Yes:
                            Save();
                            break;
                        // Выход без сохранения изменений.
                        case DialogResult.No:
                            break;
                        // Отмена закрытия формы.
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }

            // Сохранение настроек формы и путей открытых в форме файлов.
            SaveSettings();
        }

        /// <summary>
        /// Метод, вызываемый при изменении размера формы.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            // Перерисовка панели вкладок.
            RenderTabs();

            // Перерисовка текстовой области оповещений компилятора.
            if (messageTextBoxShown)
            {
                textBoxPanel.Width = (this.Width - textBoxPanel.Location.X) / 2 - 2;
                borderPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width, textBoxPanel.Location.Y);
                messageTextPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width + 2, textBoxPanel.Location.Y);
                messageTextPanel.Size = new Size(Width - (textBoxPanel.Location.X + textBoxPanel.Width + 2), textBoxPanel.Height);
                borderPanel.Size = new Size(2, textBoxPanel.Height);
            }
        }

        #endregion

        #region Обработчики событий нажатия на кнопки тулбара.
        /// <summary>
        /// Обработчик события нажатия на кнопку создания нового файла на тулбаре.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // Вызов метода создания нового файла.
            NewFile();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку открытия файла на тулбаре.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            // Вызов метода открытия файла.
            OpenFile();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку сохранения файла на тулбаре.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            // Вызов метода сохранения файла.
            Save();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку закрытия текущей закладки на тулбаре.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void closeToolStripButton_Click(object sender, EventArgs e)
        {
            // Выход из метода, если нет открытых вкладок.
            if (tabInfo.Count == 0) return;

            // Определения индекса активной вкладки.
            int index = int.MinValue;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item4) index = i;
            }
            if (index == int.MinValue) return;

            // Проверка на наличие несохранённых сохранений.
            if (fileChangeList[index].Item2 == true)
            {
                // Вывод сообщения о наличии несохраненных изменений с предложением их сохранить.
                DialogResult result = MessageBox.Show($"There are unsaved changes in {Path.GetFileName(fileChangeList[index].Item1)}. Would you like to save them before closing?",
                    "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);


                switch (result)
                {
                    // Сохранение файла и закрытие вкладки.
                    case DialogResult.Yes:
                        // Переход к методу "Сохранить как...", если файл был создан во время исполнения и еще не сохранён.
                        if (fileChangeList[index].Item1.StartsWith("unnamed"))
                        {
                            SaveAs();
                        }
                        // Сохранение изменений в существующем файле.
                        else
                        {
                            // Сохранение rtf-файла.
                            if (Path.GetExtension(fileChangeList[index].Item1) == ".rtf")
                            {
                                activeTextBox.SaveFile(fileChangeList[index].Item1, RichTextBoxStreamType.RichText);
                            }
                            // Сохранение файла расширения не .rtf и .cs.
                            else if (Path.GetExtension(fileChangeList[index].Item1) != ".cs")
                            {
                                activeTextBox.SaveFile(fileChangeList[index].Item1, RichTextBoxStreamType.PlainText);
                            }
                            // Сохранение .cs файла.
                            else
                            {
                                activeCodeTextBox.SaveToFile(fileChangeList[index].Item1, System.Text.Encoding.Default);
                            }
                        }

                        // Удаление вкладки и информации о файле, открытой в ней.
                        tabInfo.Remove(tabInfo[index]);
                        fileChangeList.Remove(fileChangeList[index]);
                        textBoxPanel.Controls.Clear();

                        if (tabInfo.Count == 0)
                        {
                            textBoxPanel.Visible = false;
                            tabPanel.Visible = false;
                            break;
                        }

                        // Изменение выбора вкладки и отрисовка панели вкладок.
                        MoveTabSelection(index);
                        RenderTabs();
                        break;
                    // Закрытие вкладки без сохранения.
                    case DialogResult.No:
                        // Удаление вкладки и информации о файле, открытой в ней.
                        tabInfo.Remove(tabInfo[index]);
                        fileChangeList.Remove(fileChangeList[index]);
                        textBoxPanel.Controls.Clear();

                        if (tabInfo.Count == 0)
                        {
                            textBoxPanel.Visible = false;
                            tabPanel.Visible = false;
                            break;
                        }

                        // Изменение выбора вкладки и отрисовка панели вкладок.
                        MoveTabSelection(index);
                        RenderTabs();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            else
            {
                // Удаление вкладки и информации о файле, открытой в ней.
                tabInfo.Remove(tabInfo[index]);
                fileChangeList.Remove(fileChangeList[index]);
                textBoxPanel.Controls.Clear();

                if (tabInfo.Count == 0)
                {
                    textBoxPanel.Visible = false;
                    tabPanel.Visible = false;
                    HideMessageBox();
                    return;
                }

                // Изменение выбора вкладки и отрисовка панели вкладок.
                MoveTabSelection(index);
                RenderTabs();
            }
            HideMessageBox();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку изменения шрифта.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void fontToolStripButton_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора шрифта.
            if (tabInfo.Count != 0) CallFontDialog();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку активации курсивного шрифта.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            // Добавление стиля шрифта.
            ToggleFontStyle(FontStyle.Italic);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку активации жирного шрифта.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void boldToolStripButton_Click(object sender, EventArgs e)
        {
            // Добавление стиля шрифта.
            ToggleFontStyle(FontStyle.Bold);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку переключения подчеркивания шрифта.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void underlineToolStripButton_Click(object sender, EventArgs e)
        {
            // Добавление стиля шрифта.
            ToggleFontStyle(FontStyle.Underline);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку переключения перечеркивания шрифта.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void strikeoutToolStripButton_Click(object sender, EventArgs e)
        {
            // Добавление стиля шрифта.
            ToggleFontStyle(FontStyle.Strikeout);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку сборки.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void BuildClick(object sender, EventArgs e)
        {
            if (compilerPath == "" || !compilerPath.EndsWith(".exe"))
            {
                MessageBox.Show("Please enter the path to compiler in Settings > Compiler path...");
                return;
            }
            // Инициализация области оповещений компилятора.
            AddMessageBox();

            string fileName = "";

            // Получение имени файла с кодом.
            for (int i = 0; i < tabInfo.Count; i++) if (tabInfo[i].Item4) fileName = tabInfo[i].Item2.Controls[0].Text;
            string filePath = $@"{temporaryFolder}\{fileName}";

            // Создание файла формата .cs с кодом.
            try
            {
                File.WriteAllText(filePath, activeCodeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Компиляция файла.
            if (filePath != "") CompileFromText(filePath);
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку исполнения.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void RunClick(object sender, EventArgs e)
        {
            if (compilerPath == "" || !compilerPath.EndsWith(".exe"))
            {
                MessageBox.Show("Please enter the path to compiler in Settings > Compiler path...");
                return;
            }
            // Инициализация области оповещений компилятора.
            AddMessageBox();

            string fileName = "";

            // Получение имени файла с кодом.
            for (int i = 0; i < tabInfo.Count; i++) if (tabInfo[i].Item4) fileName = tabInfo[i].Item2.Controls[0].Text;
            string filePath = $@"{temporaryFolder}\{fileName}";

            // Создание файла формата .cs с кодом.
            try
            {
                File.WriteAllText(filePath, activeCodeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Получение имени для исполняемого файла.
            string processName = fileName.Split('.')[0] + ".exe";

            // Компиляция и исполнение файла.
            if (filePath != "") CompileFromText(filePath);
            RunProgram(processName);
            DeleteFile(processName);
        }
        #endregion

        #region Методы работы со шрифтами.

        /// <summary>
        /// Метод переключения видимости элементов управления шрифтом.
        /// </summary>
        /// <param name="value">Видимость элементов управления шрифтом.</param>
        private void SwitchFontControls(bool value)
        {
            // Скрытие элементов управления шрифтом и активация кнопок сборки и компиляции кода.
            fontToolStripButton.Visible = value;
            italicToolStripButton.Visible = value;
            boldToolStripButton.Visible = value;
            underlineToolStripButton.Visible = value;
            strikeoutToolStripButton.Visible = value;
            fontToolStripMenuItem.Visible = value;
            formatContextMenuItem.Visible = value;
            codeFomattingToolStripMenuItem.Visible = !value;
            buildToolBarButton.Visible = !value;
            runToolBarButton.Visible = !value;
        }

        /// <summary>
        /// Метод вызова диалога выбора шрифта.
        /// </summary>
        private void CallFontDialog()
        {
            // Возврат, если неактивен RichTextBox.
            if (activeTextBox == null) return;

            // Создание нового диалгоа вібора шрифта.
            FontDialog fontDialog = new FontDialog();
            fontDialog.ShowColor = true;

            fontDialog.Font = activeTextBox.SelectionFont;
            fontDialog.Color = activeTextBox.SelectionColor;

            // Установка шрифта.
            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                activeTextBox.SelectionFont = fontDialog.Font;
                activeTextBox.SelectionColor = fontDialog.Color;
                currentFont = fontDialog.Font;
            }
        }

        /// <summary>
        /// Метод переключения стиля шрифта.
        /// </summary>
        /// <param name="fontStyle">Стиль шрифта.</param>
        private void ToggleFontStyle(FontStyle fontStyle)
        {
            bool validation = false;
            if (activeTextBox == null) return;
            if (currentFont == null) currentFont = activeTextBox.SelectionFont;

            // Инициализация буля валидации - true, если выделенный текст уже имеет данный стиль шрифта.
            switch (fontStyle)
            {
                case FontStyle.Italic:
                    validation = currentFont.Italic;
                    break;
                case FontStyle.Bold:
                    validation = currentFont.Bold;
                    break;
                case FontStyle.Underline:
                    validation = currentFont.Underline;
                    break;
                case FontStyle.Strikeout:
                    validation = currentFont.Strikeout;
                    break;
            }

            // Переключение стиля шрифта.
            if (activeTextBox == null) return;

            // Определение выделенной области.
            int selectionStart = activeTextBox.SelectionStart;
            int selectionLength = activeTextBox.SelectionLength;

            // Добавление стиля посимвольно.
            if (!validation)
            {
                currentFont = new Font(currentFont, currentFont.Style | fontStyle);
                for (int i = selectionStart; i < selectionStart + selectionLength; i++)
                {
                    activeTextBox.Select(i, 1);
                    activeTextBox.SelectionFont = new Font(activeTextBox.SelectionFont, activeTextBox.SelectionFont.Style | fontStyle);
                }
            }
            // Удаление стиля посимвольно.
            else
            {
                currentFont = new Font(currentFont, currentFont.Style & ~fontStyle);
                for (int i = selectionStart; i < selectionStart + selectionLength; i++)
                {
                    activeTextBox.Select(i, 1);
                    activeTextBox.SelectionFont = new Font(activeTextBox.SelectionFont, activeTextBox.SelectionFont.Style & ~fontStyle);
                }
            }

            // Восстановление выделения.
            activeTextBox.SelectionStart = selectionStart;
            activeTextBox.SelectionLength = selectionLength;
        }

        #endregion

        #region Обработчики событий нажатия на кнопки контекстного меню.

        /// <summary>
        /// Обработчик нажатия на кнопку "Select all" в контекстном меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void selectAllTextContextMenuItem_Click(object sender, EventArgs e)
        {
            // Выделение всего текста в активном RichTextBox.
            if (activeTextBox != null) activeTextBox.SelectAll();
            // Выделение всего текста в активном FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.SelectAll();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Cut" в контекстном меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void cutContextMenuItem_Click(object sender, EventArgs e)
        {
            // Вырезание текста в активном RichTextBox.
            if (activeTextBox != null) activeTextBox.Cut();
            // Вырезание текста в активном FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Cut();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Copy" в контекстном меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void copyContextMenuItem_Click(object sender, EventArgs e)
        {
            // Копирование текста в активном RichTextBox.
            if (activeTextBox != null) activeTextBox.Copy();
            // Копирование текста в активном FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Copy();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Paste" в контекстном меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void pasteContextMenuItem_Click(object sender, EventArgs e)
        {
            // Вставка текста в активный RichTextBox.
            if (activeTextBox != null) activeTextBox.Paste();
            // Вставка текста в активный FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Paste();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Format" в контекстном меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void formatContextMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора шрифта, если активен RichTextBox.
            if (activeTextBox == null) return;
            else CallFontDialog();
        }

        #endregion

        #region Методы сохранения файлов.
        /// <summary>
        /// Метод сохранения файла в его изначальном рзамещении.
        /// </summary>
        private void Save()
        {
            // Возврат, если нет открытых вкладок.
            if (tabInfo.Count == 0) return;

            // Переход к "Сохранению как", если файл был создан во время исполнения внутри приложения.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    if (tabInfo[i].Item2.Controls[0].Text.StartsWith("unnamed"))
                    {
                        SaveAs();
                        return;
                    }
                }
            }

            // Определение индекса активной вкладки.
            int activeIndex = 0;
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4) activeIndex = i;
            }

            // Сохранение .rtf файлов.
            if (System.IO.Path.GetExtension(fileChangeList[activeIndex].Item1) == ".rtf")
            {
                activeTextBox.SaveFile(fileChangeList[activeIndex].Item1, RichTextBoxStreamType.RichText);
            }
            // Сохранение файлов расширения .txt, .cfg, .ini, .csv.
            else if (System.IO.Path.GetExtension(fileChangeList[activeIndex].Item1) != ".cs")
            {
                activeTextBox.SaveFile(fileChangeList[activeIndex].Item1, RichTextBoxStreamType.PlainText);
            }
            // Сохранение .cs файлов.
            else
            {
                activeCodeTextBox.SaveToFile(fileChangeList[activeIndex].Item1, System.Text.Encoding.Default);
            }

            // Регистрация сохранения изменений в кортеже.
            fileChangeList[activeIndex] = Tuple.Create(fileChangeList[activeIndex].Item1, false);
        }

        /// <summary>
        /// Метод "сохранения как" файла.
        /// </summary>
        private void SaveAs()
        {
            string filename = "";

            // Используется диалог сохранения файла.
            using (SaveFileDialog dlgSave = new SaveFileDialog())
            {
                try
                {
                    // Установка расширения по умолчанию.
                    dlgSave.DefaultExt = "txt";
                    // Установка названия окна.
                    dlgSave.Title = "Save File As...";
                    // Доступные расширения файла (поддерживаемые приложением).
                    dlgSave.Filter = "Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf|C# files (*.cs)|*.cs" +
                        "|Configuration files (*.cfg)|*.cfg|Initialization files (*.ini)|*.ini|CSV files (*.csv)|*.csv";

                    // Действия в случае успешного выбора пути.
                    if (dlgSave.ShowDialog() == DialogResult.OK)
                    {
                        filename = System.IO.Path.GetFileName(dlgSave.FileName);

                        // Сохранение из FastColoredTextBox.
                        if (activeTextBox == null && activeCodeTextBox != null)
                        {
                            string source = activeCodeTextBox.Text;
                            activeCodeTextBox.SaveToFile(dlgSave.FileName, System.Text.Encoding.Default);
                        }
                        // Сохранение из активного RichTextBox.
                        else
                        {
                            // Сохранение файлов расширения .txt, .cfg, .ini, .csv.
                            if (System.IO.Path.GetExtension(dlgSave.FileName) != ".rtf")
                            {
                                activeTextBox.SaveFile(dlgSave.FileName, RichTextBoxStreamType.PlainText);
                            }
                            // Сохранение .cs файлов.
                            else
                            {
                                activeTextBox.SaveFile(dlgSave.FileName, RichTextBoxStreamType.RichText);
                            }
                        }
                    }
                }
                catch (Exception errorMsg)
                {
                    // Вывод сообщения об исключении.
                    MessageBox.Show(errorMsg.Message);
                }
            }

            // Регистрация сохранения.
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // Регистрация сохранения изменений в кортеже.
                    if (!fileChangeList[i].Item1.StartsWith("unnamed"))
                    {
                        fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, false);
                    }
                    // Регистрация сохранения для созданного файла.
                    else
                    {
                        if (String.IsNullOrEmpty(filename)) return;
                        // Регистрация измнеений в кортежах.
                        fileChangeList[i] = Tuple.Create(filename, false);
                        tabInfo[i].Item2.Controls[0].Text = filename;
                        // Смена рабочей области для кода.
                        if (Path.GetExtension(fileChangeList[i].Item1) == ".cs")
                        {
                            SwitchFontControls(false);
                            FastColoredTextBoxNS.FastColoredTextBox textBoxNS = new FastColoredTextBoxNS.FastColoredTextBox();
                            textBoxNS.Text = activeTextBox.Text;
                            textBoxNS.Dock = DockStyle.Fill;
                            textBoxNS.BorderStyle = BorderStyle.None;
                            textBoxNS.MouseUp += richtextbox_MouseUp;
                            textBoxNS.TextChanged += fastrichtextbox_TextChanged;
                            textBoxNS.Language = FastColoredTextBoxNS.Language.CSharp;
                            activeTextBox = null;
                            activeCodeTextBox = textBoxNS;
                            textBoxPanel.Controls.Clear();
                            textBoxPanel.Controls.Add(activeCodeTextBox);
                        }
                        // Смена рабочей области для текста.
                        else SwitchFontControls(true);
                    }
                }
            }
        }

        /// <summary>
        /// Метод сохранения всех открытых файлов в их изначальном размещении.
        /// </summary>
        private void SaveAll()
        {
            // Вызов делегата в соответствующем потоке.
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    // Проход по всем открытым файлам.
                    for (int i = 0; i < fileChangeList.Count; i++)
                    {
                        // Переход к методу "Сохранить как", если файл был создан в программе и не сохранён.
                        if (fileChangeList[i].Item1.StartsWith("unnamed"))
                        {
                            SaveAs();
                        }
                        // Сохранение существующих файлов.
                        else
                        {
                            // Сохранение .rtf файлов.
                            if (System.IO.Path.GetExtension(fileChangeList[i].Item1) == ".rtf")
                            {
                                tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.RichText);
                            }
                            // Сохранение файлов расширения .txt, .cfg, .ini, .csv.
                            else if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".cs")
                            {
                                tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.PlainText);
                            }
                            // Сохранение .cs файлов.
                            else
                            {
                                tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об исключении.
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        #endregion

        #region Методы открытия и создания файлов.
        /// <summary>
        /// Метод открытия файла.
        /// </summary>
        private void OpenFile()
        {
            // Объявление нового диалогового окна открытия файла.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // Заголовок окна.
            openFileDialog.Title = "Open File...";
            // Фильтр выбора расширений.
            openFileDialog.Filter = "Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf|C# files (*.cs)|*.cs" +
                "|Configuration files (*.cfg)|*.cfg|Initialization files (*.ini)|*.ini|CSV files (*.csv)|*.csv";

            // Инициализация RichTextBox для текстовых файлов.
            RichTextBox txtBox = new RichTextBox();
            txtBox.Dock = DockStyle.Fill;
            txtBox.BorderStyle = BorderStyle.None;
            txtBox.MouseUp += richtextbox_MouseUp;

            // Инициализация FastColoredTextBox для .cs файлов.
            FastColoredTextBoxNS.FastColoredTextBox textBoxNS = new FastColoredTextBoxNS.FastColoredTextBox();
            textBoxNS.Font = new Font("Consolas", textBoxNS.Font.Size);
            textBoxNS.Dock = DockStyle.Fill;
            textBoxNS.BorderStyle = BorderStyle.None;
            textBoxNS.MouseUp += richtextbox_MouseUp;

            string filename = "";

            try
            {
                // В случае успешного выбора поддерживаемого файла.
                if (openFileDialog.ShowDialog() == DialogResult.OK &&
               openFileDialog.FileName.Length > 0)
                {
                    // Сохранение пути к файлу.
                    filename = openFileDialog.FileName;

                    // Открытие .rtf файла.
                    if (System.IO.Path.GetExtension(filename) == ".rtf")
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.RichText);
                    }
                    // Открытие .cs файла.
                    else if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        textBoxNS.Language = FastColoredTextBoxNS.Language.CSharp;
                        string sourceCode = File.ReadAllText(filename);
                        textBoxNS.Text = sourceCode;
                    }
                    // Открытие файла иного расширения.
                    else
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.PlainText);
                    }

                    // Отключение или активация элементов управления шрифтом.
                    if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        SwitchFontControls(false);
                    }
                    else
                    {
                        SwitchFontControls(true);
                    }

                    // Проверка на наличие файла среди открытых.
                    if (fileChangeList.Contains(Tuple.Create(filename, false)))
                    {
                        MessageBox.Show("File is already opened!");
                        return;
                    }

                    // Сохранение информации о файле в кортеже.
                    fileChangeList.Add(Tuple.Create(filename, false));
                }
            }
            catch (Exception ex)
            {
                // Вывод сообщения об исключении.
                MessageBox.Show(ex.Message);
            }

            // Инициализация загруженного файла.
            if (!string.IsNullOrEmpty(filename))
            {
                InitializeLoadedFile(filename, txtBox, textBoxNS);
            }
        }

        /// <summary>
        /// Метод инициализации загруженного файла.
        /// </summary>
        /// <param name="filename">Путь к файлу.</param>
        /// <param name="txtBox">Инициализированный для него RichTextBox.</param>
        /// <param name="textBoxNS">Инициализированный для него FastColoredTextBox.</param>
        private void InitializeLoadedFile(string filename, RichTextBox txtBox, FastColoredTextBoxNS.FastColoredTextBox textBoxNS)
        {
            #region Инициализация вкладки.
            // Инициализация "тела" вкладки и установка его размера.
            Panel tabBase = new Panel();
            tabBase.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
            tabBase.Height = 34;
            tabBase.Location = new Point(1, 0);
            tabBase.BackColor = Color.White;
            // Инициализация лейбла (названия) вкладки с именем файла, размещение внутри тела вкладки.
            Label fileLabel = new Label();
            tabBase.Controls.Add(fileLabel);
            fileLabel.Text = Path.GetFileName(filename);
            fileLabel.AutoSize = false;
            fileLabel.TextAlign = ContentAlignment.MiddleCenter;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.Click += PanelClick;
            // Инициализация панели-границы (корня) вкладки, установка размера.
            Panel tabBorder = new Panel();
            tabBorder.Width = tabBase.Width + 2;
            tabBorder.Height = 34;
            tabBorder.Controls.Add(tabBase);
            tabBorder.BackColor = Color.LightGray;
            #endregion

            // Отмена выбора прежде активной вкладки.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                tabInfo[i] = Tuple.Create(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
            }

            // Добавление информации о вкладке в список кортежей.
            if (System.IO.Path.GetExtension(filename) == ".cs")
            {
                tabInfo.Add(Tuple.Create(tabBorder, tabBase, (RichTextBox)null, true, textBoxNS));
            }
            else
            {
                tabInfo.Add(Tuple.Create(tabBorder, tabBase, txtBox, true, (FastColoredTextBoxNS.FastColoredTextBox)null));
            }

            // Отрисовка панели вкладок и скрытие области уведомлений компилятора.
            RenderTabs();
            HideMessageBox();

            // Определение активной текстовой области (код\текст).
            if (System.IO.Path.GetExtension(filename) == ".cs")
            {
                activeCodeTextBox = textBoxNS;
                activeTextBox = null;
            }
            else
            {
                activeCodeTextBox = null;
                activeTextBox = txtBox;
            }

            // Активация текстовой области и размещение её в окне.
            textBoxPanel.Visible = true;
            textBoxPanel.Controls.Clear();
            if (activeCodeTextBox != null) textBoxPanel.Controls.Add(activeCodeTextBox);
            else textBoxPanel.Controls.Add(activeTextBox);

            // Добавление обработчиков событий.
            txtBox.TextChanged += new EventHandler(richtextbox_TextChanged);
            textBoxNS.TextChanged += fastrichtextbox_TextChanged;

            // Активация подсветки синтаксиса.
            if (activeCodeTextBox != null)
            {
                UpdateSyntaxHighlight();
            }

            // Установка заголовка окна и активация таймера автосохранения.
            this.Text = System.IO.Path.GetFileName(filename) + " - Notepad+";
            if (!autosaveTimer.Enabled) autosaveTimer.Enabled = true;
        }

        /// <summary>
        /// Метод создания нового файла.
        /// </summary>
        private void NewFile()
        {
            // Обновление глобального счётчика безымянных (несохранённых новых) файлов.
            Program.unnamedFileCount += 1;

            // Инициализация текстовой области.
            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.BorderStyle = BorderStyle.None;
            rtb.TextChanged += richtextbox_TextChanged;
            rtb.MouseUp += richtextbox_MouseUp;
            activeTextBox = rtb;
            activeCodeTextBox = null;

            #region Инициализация вкладки.
            // Инициализация "тела" вкладки и установка его размера.
            Panel tabBase = new Panel();
            tabBase.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
            tabBase.Height = 34;
            tabBase.Location = new Point(1, 0);
            // Инициализация лейбла (названия) вкладки с именем файла, размещение внутри тела вкладки.
            Label fileLabel = new Label();
            tabBase.Controls.Add(fileLabel);
            fileLabel.Text = "unnamed" + Program.unnamedFileCount;
            fileLabel.AutoSize = false;
            fileLabel.TextAlign = ContentAlignment.MiddleCenter;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.Click += PanelClick;
            // Инициализация панели-границы (корня) вкладки, установка размера.
            Panel tabBorder = new Panel();
            tabBorder.Width = tabBase.Width + 2;
            tabBorder.Height = 34;
            tabBorder.Controls.Add(tabBase);
            tabBorder.BackColor = Color.LightGray;
            #endregion

            // Отмена выбора прежде активной вкладки.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                tabInfo[i] = Tuple.Create(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
            }

            // Добавление информации о вкладке в списки кортежей.
            tabInfo.Add(Tuple.Create(tabBorder, tabBase, rtb, true, (FastColoredTextBoxNS.FastColoredTextBox)null));
            fileChangeList.Add(Tuple.Create("unnamed" + (Program.unnamedFileCount), false));

            // Отрисовка панели вкладок и скрытие области уведомлений компилятора.
            RenderTabs();
            HideMessageBox();

            // Активация текстовой области и размещение её в окне.
            textBoxPanel.Controls.Clear();
            textBoxPanel.Controls.Add(activeTextBox);
            tabPanel.Visible = true;
            textBoxPanel.Visible = true;

            // Отключение или активация элементов управления шрифтом.
            SwitchFontControls(true);

            // Установка заголовка окна.
            this.Text = "unnamed" + Program.unnamedFileCount + " - Notepad+";

            // Активация лейбла на статус-баре.
            statusStripEventTimer.Enabled = true;
            autosavingStatusLabel.Text = "New file created!";
            autosavingStatusLabel.Visible = true;
        }
        #endregion

        #region Методы сохранения, загрузки и применения настроек.
        /// <summary>
        /// Метод сохранения настроек.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // Инициализация и заполнение списка путей к открытым файлам.
                List<string> filePaths = new List<string>();
                foreach (var fileInfo in fileChangeList) if (!fileInfo.Item1.StartsWith("unnamed")) filePaths.Add(fileInfo.Item1);

                // Сохранение глобальной темы.
                int theme = Program.GlobalThemeIndex;

                // Сохранение интервалов автосохранения и журналирования.
                string autosaveInterval = intervalComboBox.SelectedItem.ToString().Split(' ')[0];
                string loggingInterval = loggingToolStripComboBox.SelectedItem.ToString().Split(' ')[0];

                // Сохранение цветов подсветки синтаксиса.
                string keywordColorString = keywordColor.ToArgb().ToString();
                string stringColorString = stringColor.ToArgb().ToString();
                string variableColorString = variableColor.ToArgb().ToString();
                string classColorString = classColor.ToArgb().ToString();
                string methodColorString = methodColor.ToArgb().ToString();

                // Составление списка настроек.
                List<string> settings = new List<string>();
                settings.Add("Theme=" + theme);
                settings.Add("keywordColor=" + keywordColorString);
                settings.Add("stringColor=" + stringColorString);
                settings.Add("variableColor=" + variableColorString);
                settings.Add("methodColor=" + methodColorString);
                settings.Add("classColor=" + classColorString);
                settings.Add("AutosaveInterval=" + autosaveInterval);
                settings.Add("LoggingInterval=" + loggingInterval);
                settings.Add("CompilerPath=" + compilerPath);
                settings.Add("Files:");

                // Добавление путей к файлам.
                foreach (var filePath in filePaths)
                {
                    settings.Add(filePath);
                }

                // Запись настроек в файл.
                File.WriteAllLines(settingsPath, settings);
            }
            catch (Exception ex)
            {
                // Вывод сообщения об исключении.
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Метод загрузки настроек.
        /// </summary>
        /// <param name="loadPrevFiles">Флаг загрузки открытых ранее файлов.</param>
        private void LoadSettings(bool loadPrevFiles)
        {
            // Возврат в случае несуществования файла настроек (при первом запуске).
            if (!File.Exists(settingsPath)) return;

            try
            {
                // Считывание настроек из файла.
                string[] settings = File.ReadAllLines(settingsPath);

                // Установка темы.
                switch (settings[0].Split('=')[1])
                {
                    case "0":
                        themeComboBox.SelectedIndex = 0;
                        break;
                    case "1":
                        themeComboBox.SelectedIndex = 1;
                        break;
                }

                // Установка цветов подсветки.
                keywordColor = Color.FromArgb(int.Parse(settings[1].Split('=')[1]));
                stringColor = Color.FromArgb(int.Parse(settings[2].Split('=')[1]));
                variableColor = Color.FromArgb(int.Parse(settings[3].Split('=')[1]));
                methodColor = Color.FromArgb(int.Parse(settings[4].Split('=')[1]));
                classColor = Color.FromArgb(int.Parse(settings[5].Split('=')[1]));

                // Установка кистей подсветки.
                variableBrush = new SolidBrush(variableColor);
                methodBrush = new SolidBrush(methodColor);
                classBrush = new SolidBrush(classColor);
                keywordBrush = new SolidBrush(keywordColor);
                stringBrush = new SolidBrush(stringColor);

                // Установка стилей подсветки.
                VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);
                MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);
                ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);
                KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);
                StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Italic);

                // Установка интервала автосохранения.
                switch (settings[6].Split('=')[1])
                {
                    case "2":
                        intervalComboBox.SelectedIndex = 0;
                        autosaveTimer.Interval = 120000;
                        break;
                    case "5":
                        intervalComboBox.SelectedIndex = 1;
                        autosaveTimer.Interval = 300000;
                        break;
                    case "10":
                        intervalComboBox.SelectedIndex = 2;
                        autosaveTimer.Interval = 600000;
                        break;
                }

                // Установка интервала журналирования.
                switch (settings[7].Split('=')[1])
                {
                    case "2":
                        loggingToolStripComboBox.SelectedIndex = 0;
                        loggingTimer.Interval = 120000;
                        break;
                    case "5":
                        loggingToolStripComboBox.SelectedIndex = 1;
                        loggingTimer.Interval = 300000;
                        break;
                    case "10":
                        loggingToolStripComboBox.SelectedIndex = 2;
                        loggingTimer.Interval = 600000;
                        break;
                }

                // Установка пути к компилятору.
                compilerPath = settings[8].Split('=')[1];
                compilerPathToolStripTextBox.Text = settings[8].Split('=')[1];

                // Загрузка темы и выход, если не загружаются файлы.
                if (!loadPrevFiles)
                {
                    LoadTheme();
                    return;
                }

                // Загрузка ранее открытых файлов.
                LoadPreviousFiles(settings);
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке.
                MessageBox.Show(ex.Message);
            }

            // Загрузка темы и обновление подсветки.
            LoadTheme();
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Метод загрузки ранее открытых файлов.
        /// </summary>
        /// <param name="settings">Строки файла настроек.</param>
        private void LoadPreviousFiles(string[] settings)
        {
            for (int i = 10; i < settings.Length; i++)
            {
                // Отмена действий в случае несуществования файла.
                if (!File.Exists(settings[i])) continue;

                // Инициализация текстовых областей (для кода и текста).
                RichTextBox txtBox = new RichTextBox();
                txtBox.Dock = DockStyle.Fill;
                txtBox.BorderStyle = BorderStyle.None;
                txtBox.MouseUp += richtextbox_MouseUp;
                FastColoredTextBoxNS.FastColoredTextBox textBoxNS = new FastColoredTextBoxNS.FastColoredTextBox();
                textBoxNS.Font = new Font("Consolas", textBoxNS.Font.Size);
                textBoxNS.Text = "~~";
                textBoxNS.Dock = DockStyle.Fill;
                textBoxNS.BorderStyle = BorderStyle.None;
                textBoxNS.MouseUp += richtextbox_MouseUp;
                string filename = "";

                try
                {
                    filename = settings[i];
                    // Открытие .rtf файла.
                    if (System.IO.Path.GetExtension(filename) == ".rtf")
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.RichText);
                    }
                    // Открытие .cs файла.
                    else if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        textBoxNS.Language = FastColoredTextBoxNS.Language.CSharp;
                        string sourceCode = File.ReadAllText(filename);
                        textBoxNS.Text = sourceCode;
                    }
                    // Открытие файла иного расширения.
                    else
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.PlainText);
                    }

                    // Отключение или активация элементов управления шрифтом.
                    if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        SwitchFontControls(false);
                    }
                    else
                    {
                        SwitchFontControls(true);
                    }

                    // Проверка на наличие файла среди уже открытых.
                    if (fileChangeList.Contains(Tuple.Create(filename, false)))
                    {
                        MessageBox.Show("File is already opened!");
                        return;
                    }

                    // Добавление информации о файле в список кортежей.
                    fileChangeList.Add(Tuple.Create(filename, false));
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об ошибке.
                    MessageBox.Show(ex.Message);
                }

                if (!string.IsNullOrEmpty(filename))
                {
                    // Инициализация загруженного файла.
                    InitializeLoadedFile(filename, txtBox, textBoxNS);
                }
            }
        }
        #endregion

        #region Методы реализации журналирования.
        /// <summary>
        /// Обработчик события "тика" таймера журналирования.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void loggingTimer_Tick(object sender, EventArgs e)
        {
            // Асинхронное выполнение метода журналирования файлов.
            await Task.Run(() => LogFiles());
        }

        /// <summary>
        /// Метод журналирования файлов.
        /// </summary>
        private void LogFiles()
        {
            // Вызов делегата в соответствующем потоке.
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    for (int i = 0; i < tabInfo.Count; i++)
                    {
                        // Исключение из журналирования несохранённых новых файлов.
                        if (tabInfo[i].Item2.Controls[0].Text.StartsWith("unnamed")) continue;
                        RichTextBox rtb = null;

                        // Формирование строки даты сохранения.
                        string date = DateTime.Now.ToString().Replace(" ", "_").Replace(":", "_");
                        // Формирование пути.
                        string filePath = $@"Logs\{Path.GetFileName(fileChangeList[i].Item1) + "_" + date}{Path.GetExtension(fileChangeList[i].Item1)}";

                        rtb = tabInfo[i].Item3;

                        // Сохранение содержимого ассоциированных текстовых областей.
                        if (rtb != null)
                        {
                            // Сохранение не-.rtf файлов.
                            if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".rtf")
                            {
                                rtb.SaveFile(filePath, RichTextBoxStreamType.PlainText);
                            }
                            // Сохранение .rtf файлов.
                            else
                            {
                                rtb.SaveFile(filePath, RichTextBoxStreamType.RichText);
                            }
                        }
                        // Сохранение .cs файлов.
                        else
                        {
                            tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Вывод сообщения об исключении.
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        #endregion

        #region Методы реализации автосохранения.
        /// <summary>
        /// Обработчик события "тика" таймера автосохранения.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void autosaveTimer_Tick(object sender, EventArgs e)
        {
            // Переменная типа Progress для передачи прогресса автосохранения прогресс-бару.
            Progress<int> progress = new Progress<int>(value => { autosavingProgressBar.Value = value; });

            // Активация лейбла и прогресс-бара на статус-баре.
            autosavingStatusLabel.Visible = true;
            autosavingProgressBar.Visible = true;

            // Асинхронное выполнение метода автосохранения файлов.
            await Task.Run(() => AutoSaveFiles(progress));

            // Скрытие лейбла и прогресс-бара на статус-баре.
            autosavingStatusLabel.Visible = false;
            autosavingProgressBar.Visible = false;
        }

        /// <summary>
        /// Метод автосозранения файлов.
        /// </summary>
        /// <param name="progress">Интерфейс для передачи прогресса.</param>
        private void AutoSaveFiles(IProgress<int> progress)
        {
            // Вызов делегата в соответствующем потоке.
            this.Invoke((MethodInvoker)(() =>
            {
                for (int i = 0; i < tabInfo.Count; i++)
                {
                    // Исключение из автосохранения созданных и не сохранённых файлов.
                    if (tabInfo[i].Item2.Controls[0].Text.StartsWith("unnamed")) continue;

                    // Сохранение .rtf файлов.
                    if (System.IO.Path.GetExtension(fileChangeList[i].Item1) == ".rtf")
                    {
                        tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.RichText);
                    }
                    // Сохранение файлов иных расширений.
                    else if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".cs")
                    {
                        tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.PlainText);
                    }
                    // Сохранение .cs файлов.
                    else
                    {
                        tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                    }

                    // Внесение изменений в список кортежей с информацией о файлах.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, false);

                    // Передача прогресса через интерфейс.
                    double completionRate = (double)(i + 1) / tabInfo.Count;
                    progress.Report((int)completionRate);
                }
            }));
        }
        #endregion

        #region Методы работы статус-бара.
        /// <summary>
        /// Обработчик события "тика" таймера отключения лейбла статус-стрипа.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void statusStripEventTimer_Tick(object sender, EventArgs e)
        {
            // Деактивация лейбла и таймера.
            autosavingStatusLabel.Text = "Autosaving...";
            autosavingStatusLabel.Visible = false;
            statusStripEventTimer.Enabled = false;
        }
        #endregion

        #region Методы обработки нажатия на кнопки меню приложения.
        /// <summary>
        /// Обработчик нажатия на кнопку создания нового файла в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода создания нового файла.
            NewFile();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку открытия файла в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода открытия файла.
            OpenFile();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку создания нового файла в новом окне в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Инициализация нового потока и исполняемого им делегата.
            Thread newThread = new Thread(() =>
            {
                // Создание новой формы, запуск её методов отрисовки и создания нового файла.
                MainWindow newWindow = new MainWindow() { loadPrevFiles = false };
                newWindow.FormClosing += Form1_FormClosing;
                newWindow.tabInfo.Clear();
                newWindow.fileChangeList.Clear();
                newWindow.NewFile();
                newWindow.RenderTabs();
                Application.Run(newWindow);
            });
            newThread.TrySetApartmentState(ApartmentState.STA);
            // Запуск потока.
            newThread.Start();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку сохранения всех открытых файлов в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Активация лейбла на статус-баре.
            autosavingStatusLabel.Text = "Saving all documents...";
            autosavingStatusLabel.Visible = true;

            // Асинхронное выполнение метода сохранения всех открытых файлов.
            await Task.Run(() => SaveAll());

            // Скрытие лейбла на статус-баре.
            autosavingStatusLabel.Visible = false;
            autosavingStatusLabel.Text = "Autosaving:";
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "сохранения как" в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода "сохранения как".
            SaveAs();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку настройки шрифта в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода настройки шрифта.
            if (tabInfo.Count != 0) CallFontDialog();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку сохранения файла в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода сохранения файла.
            if (tabInfo.Count != 0 && activeTextBox != null) Save();
        }

        /// <summary>
        /// Обработчик изменения выбранного объекта в меню выбора интервала автосохранения в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void intervalComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Установка интервала автосохранения.
            switch (intervalComboBox.SelectedIndex)
            {
                case 0:
                    autosaveTimer.Interval = 120000;
                    break;
                case 1:
                    autosaveTimer.Interval = 300000;
                    break;
                case 2:
                    autosaveTimer.Interval = 600000;
                    break;
            }
        }

        /// <summary>
        /// Обработчик изменения выбранного объекта в меню выбора темы в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        public void themeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Изменение глобальной темы.
            Program.GlobalThemeIndex = themeComboBox.SelectedIndex;
        }

        /// <summary>
        /// Обработчик изменения пути к компилятору в меню настроек.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void compilerPathToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            // Изменение пути к компилятору.
            compilerPath = compilerPathToolStripTextBox.Text;
        }

        /// <summary>
        /// Обработчик изменения выбранного объекта в меню выбора интервала журналирования в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void loggingToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Установка интервала журналирования.
            switch (loggingToolStripComboBox.SelectedIndex)
            {
                case 0:
                    loggingTimer.Interval = 120000;
                    break;
                case 1:
                    loggingTimer.Interval = 300000;
                    break;
                case 2:
                    loggingTimer.Interval = 600000;
                    break;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку закрытия окна в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Закрытие окна.
            this.Close();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку отмены последнего действия в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода отмены последнего действия.
            Undo();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку повтора последнего действия в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов метода повтора последнего действия.
            Redo();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку выбора цвета подсветки ключевых слов.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void keywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора цвета.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)keywordBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // Применение нового цвета и реинициализация стилей.
            keywordColor = colordlg.Color;
            keywordBrush = new SolidBrush(keywordColor);
            KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);

            // Обновление подсветки синтаксиса.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку выбора цвета подсветки строк.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void stringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора цвета.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)stringBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // Применение нового цвета и реинициализация стилей.
            stringColor = colordlg.Color;
            stringBrush = new SolidBrush(stringColor);
            StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Regular);

            // Обновление подсветки синтаксиса.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку выбора цвета подсветки переменных.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void variablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора цвета.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)variableBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // Применение нового цвета и реинициализация стилей.
            variableColor = colordlg.Color;
            variableBrush = new SolidBrush(variableColor);
            VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);

            // Обновление подсветки синтаксиса.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку выбора цвета подсветки методов.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void methodsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора цвета.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)methodBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // Применение нового цвета и реинициализация стилей.
            methodColor = colordlg.Color;
            methodBrush = new SolidBrush(methodColor);
            MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);

            // Обновление подсветки синтаксиса.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку выбора цвета подсветки классов.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void classesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Вызов диалога выбора цвета.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)classBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // Применение нового цвета и реинициализация стилей.
            classColor = colordlg.Color;
            classBrush = new SolidBrush(classColor);
            ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);

            // Обновление подсветки синтаксиса.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку About.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, Color> themeDict;
            // Загрузка словаря цветов темы в зависимости от выбранной темы.
            if (Program.GlobalThemeIndex == 0) themeDict = lightThemeColors;
            else themeDict = darkThemeColors;

            // Инициализация формы и цветов.
            HelpForm about = new HelpForm();
            about.BackColor = this.BackColor;
            about.headlineLabel.ForeColor = themeDict["TabLabelForeColor_Active"];
            about.textLabel.ForeColor = themeDict["TabLabelForeColor_Active"];

            // Запуск формы.
            about.ShowDialog();
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку автоформатирования кода в меню.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void codeFomattingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChanged = false;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item5 == null || !tabInfo[i].Item4) continue;

                // Сохранение флага наличия несохраненных изменений до изменения текста для обновления подсветки.
                isChanged = fileChangeList[i].Item2;
                string text = tabInfo[i].Item5.Text;
                // Автоформатирование отступов.
                text = CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().ToFullString();
                // Перезагрузка текста для реинициализации подсветки всех строк кода.
                tabInfo[i].Item5.Text = "";
                tabInfo[i].Item5.Text = text;
                // Возврат кортежа к прежнему состоянию.
                fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, isChanged);
            }
        }
        #endregion

        #region Методы подсветки синтаксиса.
        /// <summary>
        /// Метод формирования REGEX-шаблона для поиска имён переменных, объявленных в файле.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <returns>REGEX-шаблон для поиска имён переменных.</returns>
        private string VariableNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // Парсинг текста при помощи Roslyn для формирования синтаксического дерева.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // Формирование массива имён переменных.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<VariableDeclaratorSyntax>().Select(v => v.Identifier.Text)
            .Concat(syntaxTree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().Select(p => p.Identifier.Text))
            .ToArray();

            // Возврат REGEX-шаблона (строки).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// Метод формирования REGEX-шаблона для поиска имён методов, объявленных в файле.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <returns>REGEX-шаблон для поиска имён методов.</returns>
        private string MethodNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // Парсинг текста при помощи Roslyn для формирования синтаксического дерева.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // Формирование массива имён методов.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Select(v => v.Identifier.Text).ToArray();

            // Возврат REGEX-шаблона (строки).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// Метод формирования REGEX-шаблона для поиска имён классов, объявленных в файле.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <returns>REGEX-шаблон для поиска имён классов.</returns>
        private string ClassNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // Парсинг текста при помощи Roslyn для формирования синтаксического дерева.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // Формирование массива имён классов.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Select(v => v.Identifier.Text).ToArray();

            // Возврат REGEX-шаблона (строки).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// Метод обновления подсветки синтаксиса.
        /// </summary>
        private void UpdateSyntaxHighlight()
        {
            bool isChanged = false;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item5 == null) continue;

                // Сохранение флага наличия несохраненных изменений до изменения текста для обновления подсветки.
                isChanged = fileChangeList[i].Item2;
                string text = tabInfo[i].Item5.Text;
                // Автоформатирование отступов.
                text = CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().ToFullString();
                // Перезагрузка текста для реинициализации подсветки всех строк кода.
                tabInfo[i].Item5.Text = "";
                tabInfo[i].Item5.Text = text;
                // Возврат кортежа к прежнему состоянию.
                fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, isChanged);
            }
        }

        #endregion

        #region Методы отрисовки и работы с вкладками.
        /// <summary>
        /// Метод отрисовки закладок на панели закладок.
        /// </summary>
        private void RenderTabs()
        {
            // Общая ширина закладок.
            int totalWidth = 0;
            if (tabInfo.Count == 0) return;
            tabPanel.Controls.Clear();

            foreach (var tab in tabInfo)
            {
                // Добавление закладки на панель.
                tabPanel.Controls.Add(tab.Item1);
                // Расчёт размера последней закладки с учетом общей ширины (компенсация неточности в целочисленных расчётах).
                if (tabInfo.IndexOf(tab) == tabInfo.Count - 1)
                {
                    tab.Item1.Width = tabPanel.Width - totalWidth;
                    tab.Item2.Width = tab.Item1.Width - 2;
                }
                // Расчёт размера для иных закладок по общему числу закладок.
                else
                {
                    tab.Item2.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
                    tab.Item1.Width = tab.Item2.Width + 2;
                }
                // Установка положения закладки на панели закладок.
                tab.Item1.Location = new Point(totalWidth, 0);
                // Оформление активной (выбранной) вкладки.
                if (tab.Item4)
                {
                    tab.Item1.BackColor = Color.MediumTurquoise;
                    tab.Item2.Location = new Point(2, 0);
                    tab.Item2.BackColor = Color.White;
                }
                // Оформление неактивной вкладки.
                else
                {
                    tab.Item1.BackColor = Color.FromArgb(217, 217, 217);
                    tab.Item2.Location = new Point(1, -1);
                    tab.Item2.BackColor = Color.FromArgb(234, 234, 236);
                }
                // Расчёт общей ширины добавленных вкладок.
                totalWidth += tab.Item1.Width;
            }
            tabPanel.Visible = true;
            // Загрузка темы.
            LoadTheme();
        }

        /// <summary>
        /// Обработчик события нажатия на панель (вкладку).
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void PanelClick(object sender, EventArgs e)
        {
            // Деактивация выбранной на данный момент вкладки.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    tabInfo[i] = new Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
                }
            }

            for (int i = 0; i < tabInfo.Count; i++)
            {
                // Переключение на sender (вкладку, отправившую событие).
                if (tabInfo[i].Item2.Controls[0] == sender)
                {
                    // Активация вкладки в кортеже.
                    tabInfo[i] = new Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, true, tabInfo[i].Item5);

                    // Включение элементов работы с кодом для .cs файлов.
                    if (Path.GetExtension(fileChangeList[i].Item1) == ".cs")
                    {
                        SwitchFontControls(false);
                        activeCodeTextBox = tabInfo[i].Item5;
                        activeTextBox = null;
                        textBoxPanel.Controls.Clear();
                        textBoxPanel.Controls.Add(activeCodeTextBox);
                        UpdateSyntaxHighlight();
                    }
                    // Включение элементов работы с текстом для не-.cs файлов.
                    else
                    {
                        SwitchFontControls(true);
                        activeTextBox = tabInfo[i].Item3;
                        activeCodeTextBox = null;
                        textBoxPanel.Controls.Clear();
                        textBoxPanel.Controls.Add(activeTextBox);
                    }

                    // Изменение заголовка окна.
                    this.Text = tabInfo[i].Item2.Controls[0].Text + " - Notepad+";
                }
            }

            // Вызов метода отрисовки вкладок и скрытие области повещений компилятора.
            RenderTabs();
            HideMessageBox();
        }

        /// <summary>
        /// Метод расчёта сдвига выбора вкладки после закрытия вкладки с индексом index.
        /// </summary>
        /// <param name="index">Индекс закрытой вкладки.</param>
        private void MoveTabSelection(int index)
        {
            // Если есть вкладки за закрытой.
            if (tabInfo.Count >= index + 1)
            {
                SelectTab(index);
            }
            // Если закрытая вкладка была последней.
            else
            {
                SelectTab(index - 1);
            }
            // Скрытие области повещений компилятора.
            HideMessageBox();
        }

        /// <summary>
        /// Метод сдвига выбора вкладки.
        /// </summary>
        /// <param name="index">Индекс выбранной после закрытия вкладки.</param>
        private void SelectTab(int index)
        {
            // Изменение флага выбранной вкладки.
            tabInfo[index] = Tuple.Create(tabInfo[index].Item1, tabInfo[index].Item2, tabInfo[index].Item3, true, tabInfo[index].Item5);
            textBoxPanel.Controls.Clear();
            // Активация текстовых областей, ассоциированных с вкладкой.
            if (tabInfo[index].Item3 != null)
            {
                // Активация RichTextBox.
                textBoxPanel.Controls.Add(tabInfo[index].Item3);
                activeTextBox = tabInfo[index].Item3;
                activeCodeTextBox = null;
            }
            else
            {
                // Активация FastColoredTextBox.
                textBoxPanel.Controls.Add(tabInfo[index].Item5);
                activeTextBox = null;
                activeCodeTextBox = tabInfo[index].Item5;
            }
            // Активация\деактивация элементов управления шрифтом\кодом.
            if (Path.GetExtension(fileChangeList[index].Item1) == ".cs") SwitchFontControls(false);
            else SwitchFontControls(true);

            // Изменение заголовка окна.
            this.Text = tabInfo[index].Item2.Controls[0].Text + " - Notepad+";
        }
        #endregion

        #region Методы функционала текстовых областей.
        /// <summary>
        /// Обработчик изменения текста в текстовой области типа RichTextBox.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void richtextbox_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // Установка положительного значения флага наличия несохранённых изменений.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, true);
                }
            }
        }

        /// <summary>
        /// Обработчик изменения текста в текстовой области типа FastColoredTextBox.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void fastrichtextbox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // Установка положительного значения флага наличия несохранённых изменений.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, true);
                }
            }

            // Подсветка имён переменных, методов и классов.
            e.ChangedRange.SetStyle(VariableStyle, VariableNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));
            e.ChangedRange.SetStyle(MethodStyle, MethodNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));
            e.ChangedRange.SetStyle(ClassStyle, ClassNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));

            // Подсветка ключевых слов и строк (применение стилей).
            ((FastColoredTextBoxNS.FastColoredTextBox)sender).SyntaxHighlighter.KeywordStyle = KeywordStyle;
            ((FastColoredTextBoxNS.FastColoredTextBox)sender).SyntaxHighlighter.StringStyle = StringStyle;
        }

        /// <summary>
        /// Обработчик нажатия клавиши мыши над текстовой областью.
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void richtextbox_MouseUp(object sender, MouseEventArgs e)
        {
            // Определение, нажата ли ПКМ.
            if (e.Button != MouseButtons.Right) return;
            // Активация контекстного меню.
            contextMenuStrip1.Show((Control)sender, e.Location);
        }

        /// <summary>
        /// Метод отмены последнего изменения в активной текстовой области.
        /// </summary>
        private void Undo()
        {
            if (activeTextBox != null) activeTextBox.Undo();
            else if (activeCodeTextBox != null) activeCodeTextBox.Undo();
        }

        /// <summary>
        /// Метод повторения последнего отмененного изменения в активной текстовой области.
        /// </summary>
        private void Redo()
        {
            if (activeTextBox != null) activeTextBox.Redo();
            else if (activeCodeTextBox != null) activeCodeTextBox.Redo();
        }

        #endregion

        #region Метод загрузки темы.
        /// <summary>
        /// Метод загрузки выбранной темы.
        /// </summary>
        public void LoadTheme()
        {
            Dictionary<string, Color> themeDict;
            // Загрузка словаря цветов темы в зависимости от выбранной темы.
            if (Program.GlobalThemeIndex == 0) themeDict = lightThemeColors;
            else themeDict = darkThemeColors;

            // Установка цвета меню.
            menuStrip1.BackColor = themeDict["MenuBackColor"];
            // Установка цвета текста в меню.
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.ForeColor = themeDict["MenuForeColor"];
            }
            // Установка цвета тулбара и его границы.
            toolPanel.BackColor = themeDict["ToolPanelBackColor"];
            toolPanelBorderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            // Установка цвета панели закладок.
            borderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            tabPanel.BackColor = themeDict["TabBaseBackColor_Inactive"];
            // Установка цвета фона и текста в области оповещений компилятора.
            if (messageTextPanel.Controls.Count != 0)
            {
                foreach (RichTextBox rtb in messageTextPanel.Controls)
                {
                    rtb.BackColor = themeDict["RichTextBoxBackColor"];
                    rtb.ForeColor = themeDict["MenuForeColor"];
                }
            }
            // Итерирование по закладкам.
            foreach (var tab in tabInfo)
            {
                // Временное отключение обработчиков внесения изменений в текстовые области.
                if (tab.Item3 != null)
                {
                    tab.Item3.TextChanged -= new EventHandler(richtextbox_TextChanged);
                    tab.Item3.TextChanged -= new EventHandler(richtextbox_TextChanged);
                }
                else
                {
                    tab.Item5.TextChanged -= fastrichtextbox_TextChanged;
                }

                // Установка цветов активной закладки.
                if (tab.Item4)
                {
                    tab.Item1.BackColor = themeDict["TabBorderBackColor_Active"];
                    tab.Item2.BackColor = themeDict["TabBaseBackColor_Active"];
                    tab.Item2.Controls[0].ForeColor = themeDict["TabLabelForeColor_Active"];
                }
                // Установка цветов неактивной закладки.
                else
                {
                    tab.Item1.BackColor = themeDict["TabBorderBackColor_Inactive"];
                    tab.Item2.BackColor = themeDict["TabBaseBackColor_Inactive"];
                    tab.Item2.Controls[0].ForeColor = themeDict["TabLabelForeColor_Inactive"];
                }

                // Установка цветов текстовой области для файлов расширений, кроме .rtf и .cs.
                if (Path.GetExtension(fileChangeList[tabInfo.IndexOf(tab)].Item1) != ".rtf" && tab.Item3 != null)
                {
                    tab.Item3.BackColor = themeDict["RichTextBoxBackColor"];
                    tab.Item3.ForeColor = themeDict["MenuForeColor"];
                    tab.Item3.SelectAll();
                    tab.Item3.SelectionBackColor = themeDict["RichTextBoxBackColor"];
                }
                // Установка цветов текстовой области для файлов расширения .cs.
                else if (Path.GetExtension(fileChangeList[tabInfo.IndexOf(tab)].Item1) != ".rtf" && tab.Item5 != null)
                {
                    tab.Item5.BackColor = themeDict["RichTextBoxBackColor"];
                    tab.Item5.ForeColor = themeDict["MenuForeColor"];
                    tab.Item5.LineNumberColor = themeDict["TabLabelForeColor_Inactive"];
                    tab.Item5.ServiceLinesColor = themeDict["RichTextBoxBackColor"];
                    tab.Item5.IndentBackColor = themeDict["RichTextBoxBackColor"];
                }

                // Включение обработчиков внесения изменений.
                if (tab.Item3 != null)
                {
                    tab.Item3.TextChanged += new EventHandler(richtextbox_TextChanged);
                }
                else
                {
                    tab.Item5.TextChanged += fastrichtextbox_TextChanged;
                }
            }
            // Установка цветов статус-бара.
            statusPanel.BackColor = themeDict["ToolPanelBackColor"];
            statusBorderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            // Установка цвета формы.
            this.BackColor = themeDict["RichTextBoxBackColor"];
            // Устаовка цветов лейблов.
            label2.ForeColor = Color.FromArgb(118, 123, 129);
            autosavingStatusLabel.ForeColor = themeDict["TabLabelForeColor_Active"];
        }
        #endregion

        #region Методы инициализации и скрытия текстового поля оповещений компилятора.
        /// <summary>
        /// Метод скрытия текстового поля оповещений компилятора.
        /// </summary>
        private void HideMessageBox()
        {
            // Восстановление размера текстовой панели.
            textBoxPanel.Width = Width - textBoxPanel.Location.X;

            // Скрытие текстового поля оповещений компилятора.
            messageTextPanel.Visible = false;
            borderPanel.Visible = false;
        }

        /// <summary>
        /// Метод активации текстового поля оповещений компилятора.
        /// </summary>
        private void AddMessageBox()
        {
            // Очистка панели для текстового поля.
            messageTextPanel.Controls.Clear();
            // Уменьшение размера поля для редактирования текста.
            textBoxPanel.Width = (this.Width - textBoxPanel.Location.X) / 2 - 2;

            // Установка размера и положения границы между текстовыми областями.
            borderPanel.Size = new Size(2, textBoxPanel.Height);
            borderPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width, textBoxPanel.Location.Y);
            // Установка размера и положения панели для текстового поля.
            messageTextPanel.Size = new Size(Width - (textBoxPanel.Location.X + textBoxPanel.Width + 2), textBoxPanel.Height);
            messageTextPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width + 2, textBoxPanel.Location.Y);
            // Активация панелей.
            borderPanel.Visible = true;
            messageTextPanel.Visible = true;

            // Инициализация текстового поля оповещений компилятора.
            activeMessageTextBox = new RichTextBox();
            activeMessageTextBox.Dock = DockStyle.Fill;
            activeMessageTextBox.BorderStyle = BorderStyle.None;
            activeMessageTextBox.ReadOnly = true;
            activeMessageTextBox.Font = new Font("Consolas", activeCodeTextBox.Font.Size);
            activeMessageTextBox.BackColor = activeCodeTextBox.BackColor;
            activeMessageTextBox.ForeColor = activeCodeTextBox.ForeColor;

            // Добавление текстового поля оповещений компилятора.
            messageTextPanel.Controls.Add(activeMessageTextBox);
            messageTextBoxShown = true;
        }
        #endregion

        #region Методы компиляции и исполнения кода.
        /// <summary>
        /// Метод исполнения программы.
        /// </summary>
        /// <param name="filePath">Путь к исполняемому файлу.</param>
        private void RunProgram(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // Инициализация процесса и его параметров.
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(filePath);
                process.StartInfo.UseShellExecute = false;
                // Запуск процесса.
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                // Оповещение об исключении.
                activeMessageTextBox.Text = ex.Message;
            }
        }

        /// <summary>
        /// Метод удаления файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        private void DeleteFile(string filePath)
        {
            // Удаление файла, если он существует.
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// Метод компиляции кода из файла.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        private void CompileFromText(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Инициализация процесса и его параметров.
                Process compiler = new Process();
                compiler.StartInfo = new ProcessStartInfo(@"cmd.exe");
                compiler.StartInfo.RedirectStandardInput = true;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                // Запуск процесса.
                compiler.Start();

                // Ввод команды в командную строку для доступа к компилятору и запуска компиляции.
                using (StreamWriter sr = compiler.StandardInput)
                {
                    compiler.StandardInput.WriteLine($@"{compilerPath} {filePath}");
                    sr.Close();
                }
                // Считывание строк из командной строки.
                string[] outputLines = compiler.StandardOutput.ReadToEnd().Split("\n");
                List<string> compilerOutput = new List<string>();

                // Очистка возвращенного текста от текста командной строки.
                for (int i = 4; i < outputLines.Length - 2; i++) compilerOutput.Add(outputLines[i]);

                // Вывод оповещений компилятора.
                activeMessageTextBox.Text = string.Join('\n', compilerOutput.ToArray());
                // Закрытие процесса.
                compiler.Close();
            }
            catch (Exception ex)
            {
                // Вывод сообщения об исключении.
                activeMessageTextBox.Text = ex.Message;
            }
        }
        #endregion
    }
}
