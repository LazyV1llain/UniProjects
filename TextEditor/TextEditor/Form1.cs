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
        #region ���������� ����� � ������ � ������, ������������ ����������.
        // ���� � ����� �������� ���������.
        const string settingsPath = @"Data\settings.txt";
        // ���� � �����������.
        string compilerPath = @"";
        // ���� � ����� � �������� ������������� �������.
        string temporaryFolder = @"Tmp";
        #endregion

        #region ���������� ������� �������� - ���������� � ������ � ��������.
        // ������ �������� � ����������� �� �������� ������. Item1 - ���� � �����, Item2 - ������� ������������ ���������.
        public List<Tuple<string, bool>> fileChangeList = new List<Tuple<string, bool>>();
        /* ������ �������� � ����������� �� �������� ��������.
         * Item1 - ������-"�������" ������� �� ������ ������� (�������� ������ �������).
         * Item2 - ������-"����" ������� �� ������ ������� (�������� ��������� �������).
         * Item3 - RichTextBox, ��������������� � �������� � ���������� ���������� ���������������� �����, ���� �� �� �������� ������ .cs.
         * Item4 - ������ ��������, ���� ��������� ������� (true, ���� ������� �������).
         * Item5 - FastColoredTextBox, ��������������� � �������� � ���������� ���������� ���������������� �����, ���� �� �������� ������ .cs. 
         * ������ ������� � ����������� � ���������� ������� ��������� � �������� ������� � ����������� �� �������� � ��� �����. */
        public List<Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>> tabInfo 
            = new List<Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>>();
        #endregion

        #region ���������� ������ ��������� ���������� (��� FastColoredTextBox).
        // ���� ��������� ��� ����������.
        static Color variableColor = Color.OrangeRed;
        // ���� ��������� ��� �������.
        static Color methodColor = Color.DeepSkyBlue;
        // ���� ��������� ��� �������.
        static Color classColor = Color.Orange;
        // ���� ��������� �������� ����.
        static Color keywordColor = Color.Blue;
        // ���� ��������� �����.
        static Color stringColor = Color.Brown;

        // ����� ��������� ��� ����������.
        static Brush variableBrush = new SolidBrush(variableColor);
        // ����� ��������� ��� �������.
        static Brush methodBrush = new SolidBrush(methodColor);
        // ����� ��������� ��� �������.
        static Brush classBrush = new SolidBrush(classColor);
        // ����� ��������� �������� ����.
        static Brush keywordBrush = new SolidBrush(keywordColor);
        // ����� ��������� �����.
        static Brush stringBrush = new SolidBrush(stringColor);

        // ����� ��������� ��� ����������.
        FastColoredTextBoxNS.Style VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);
        // ����� ��������� ��� �������.
        FastColoredTextBoxNS.Style MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);
        // ����� ��������� ��� �������.
        FastColoredTextBoxNS.Style ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);
        // ����� ��������� �������� ����.
        FastColoredTextBoxNS.Style KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);
        // ����� ��������� �����.
        FastColoredTextBoxNS.Style StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Italic);
        #endregion

        #region ���������� ��������� ��������.
        // �������� RichTextBox (��������������� � ��������� ��������).
        RichTextBox activeTextBox;
        // ������� ����� �������� �������.
        Font currentFont;
        // �������� RichTextBox ��������� ����������� (��������������� � ��������� �������� � ����������� ��� ���������� �����).
        RichTextBox activeMessageTextBox;
        // �������� FastColoredTextBox (��������������� � ��������� ��������).
        FastColoredTextBoxNS.FastColoredTextBox activeCodeTextBox;
        #endregion

        #region ���������� ������.
        // ���� ������ �������� �������� ����� ������ ��� �������� �����.
        bool loadPrevFiles = true;
        // ���� ������� �������� RichTextBox ��������� �����������.
        bool messageTextBoxShown = false;
        #endregion

        #region ���������� ������ ��������� ��� ��������� ���.
        // ������� ������ ��������� ��� ������� ����.
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

        // ������� ������ ��������� ��� ����� ����.
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

        #region ������ ����������� �����.

        /// <summary>
        /// ����������� �����.
        /// </summary>
        public MainWindow()
        {
            if (Application.OpenForms.Count == 0) Application.Run(new Splash());

            InitializeComponent();

            // ��������� ��������� ���������� �������.
            SwitchFontControls(true);

            // ��������� ������������ ������� ����.
            MinimumSize = new Size(Screen.PrimaryScreen.Bounds.Width / 3, Screen.PrimaryScreen.Bounds.Height / 3);

            // ��������� �������� �� ���������.
            loggingToolStripComboBox.SelectedIndex = 0;
            intervalComboBox.SelectedIndex = 0;
            themeComboBox.SelectedIndex = 0;
            autosaveTimer.Interval = 120000;
            loggingTimer.Interval = 120000;
            compilerPathToolStripTextBox.Text = compilerPath;

            // �������� ��������.
            LoadSettings(loadPrevFiles);
        }

        /// <summary>
        /// �����, ��������� ��� �������� �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                // �������� �������� ������ �� �������� ������������ ���������.
                if (fileChangeList[i].Item2 == true)
                {
                    // ����������� ������������ � ����� ���������� ��������.
                    DialogResult result = MessageBox.Show($"There are unsaved changes in {System.IO.Path.GetFileName(fileChangeList[i].Item1)}. Would you like to save them before closing?",
                        "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                    switch (result)
                    {
                        // ���������� ���������.
                        case DialogResult.Yes:
                            Save();
                            break;
                        // ����� ��� ���������� ���������.
                        case DialogResult.No:
                            break;
                        // ������ �������� �����.
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                    }
                }
            }

            // ���������� �������� ����� � ����� �������� � ����� ������.
            SaveSettings();
        }

        /// <summary>
        /// �����, ���������� ��� ��������� ������� �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            // ����������� ������ �������.
            RenderTabs();

            // ����������� ��������� ������� ���������� �����������.
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

        #region ����������� ������� ������� �� ������ �������.
        /// <summary>
        /// ���������� ������� ������� �� ������ �������� ������ ����� �� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            // ����� ������ �������� ������ �����.
            NewFile();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ �������� ����� �� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            // ����� ������ �������� �����.
            OpenFile();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ���������� ����� �� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            // ����� ������ ���������� �����.
            Save();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ �������� ������� �������� �� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void closeToolStripButton_Click(object sender, EventArgs e)
        {
            // ����� �� ������, ���� ��� �������� �������.
            if (tabInfo.Count == 0) return;

            // ����������� ������� �������� �������.
            int index = int.MinValue;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item4) index = i;
            }
            if (index == int.MinValue) return;

            // �������� �� ������� ������������ ����������.
            if (fileChangeList[index].Item2 == true)
            {
                // ����� ��������� � ������� ������������� ��������� � ������������ �� ���������.
                DialogResult result = MessageBox.Show($"There are unsaved changes in {Path.GetFileName(fileChangeList[index].Item1)}. Would you like to save them before closing?",
                    "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);


                switch (result)
                {
                    // ���������� ����� � �������� �������.
                    case DialogResult.Yes:
                        // ������� � ������ "��������� ���...", ���� ���� ��� ������ �� ����� ���������� � ��� �� �������.
                        if (fileChangeList[index].Item1.StartsWith("unnamed"))
                        {
                            SaveAs();
                        }
                        // ���������� ��������� � ������������ �����.
                        else
                        {
                            // ���������� rtf-�����.
                            if (Path.GetExtension(fileChangeList[index].Item1) == ".rtf")
                            {
                                activeTextBox.SaveFile(fileChangeList[index].Item1, RichTextBoxStreamType.RichText);
                            }
                            // ���������� ����� ���������� �� .rtf � .cs.
                            else if (Path.GetExtension(fileChangeList[index].Item1) != ".cs")
                            {
                                activeTextBox.SaveFile(fileChangeList[index].Item1, RichTextBoxStreamType.PlainText);
                            }
                            // ���������� .cs �����.
                            else
                            {
                                activeCodeTextBox.SaveToFile(fileChangeList[index].Item1, System.Text.Encoding.Default);
                            }
                        }

                        // �������� ������� � ���������� � �����, �������� � ���.
                        tabInfo.Remove(tabInfo[index]);
                        fileChangeList.Remove(fileChangeList[index]);
                        textBoxPanel.Controls.Clear();

                        if (tabInfo.Count == 0)
                        {
                            textBoxPanel.Visible = false;
                            tabPanel.Visible = false;
                            break;
                        }

                        // ��������� ������ ������� � ��������� ������ �������.
                        MoveTabSelection(index);
                        RenderTabs();
                        break;
                    // �������� ������� ��� ����������.
                    case DialogResult.No:
                        // �������� ������� � ���������� � �����, �������� � ���.
                        tabInfo.Remove(tabInfo[index]);
                        fileChangeList.Remove(fileChangeList[index]);
                        textBoxPanel.Controls.Clear();

                        if (tabInfo.Count == 0)
                        {
                            textBoxPanel.Visible = false;
                            tabPanel.Visible = false;
                            break;
                        }

                        // ��������� ������ ������� � ��������� ������ �������.
                        MoveTabSelection(index);
                        RenderTabs();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
            else
            {
                // �������� ������� � ���������� � �����, �������� � ���.
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

                // ��������� ������ ������� � ��������� ������ �������.
                MoveTabSelection(index);
                RenderTabs();
            }
            HideMessageBox();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ��������� ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void fontToolStripButton_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ ������.
            if (tabInfo.Count != 0) CallFontDialog();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ��������� ���������� ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void italicToolStripButton_Click(object sender, EventArgs e)
        {
            // ���������� ����� ������.
            ToggleFontStyle(FontStyle.Italic);
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ��������� ������� ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void boldToolStripButton_Click(object sender, EventArgs e)
        {
            // ���������� ����� ������.
            ToggleFontStyle(FontStyle.Bold);
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ������������ ������������� ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void underlineToolStripButton_Click(object sender, EventArgs e)
        {
            // ���������� ����� ������.
            ToggleFontStyle(FontStyle.Underline);
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ������������ �������������� ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void strikeoutToolStripButton_Click(object sender, EventArgs e)
        {
            // ���������� ����� ������.
            ToggleFontStyle(FontStyle.Strikeout);
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void BuildClick(object sender, EventArgs e)
        {
            if (compilerPath == "" || !compilerPath.EndsWith(".exe"))
            {
                MessageBox.Show("Please enter the path to compiler in Settings > Compiler path...");
                return;
            }
            // ������������� ������� ���������� �����������.
            AddMessageBox();

            string fileName = "";

            // ��������� ����� ����� � �����.
            for (int i = 0; i < tabInfo.Count; i++) if (tabInfo[i].Item4) fileName = tabInfo[i].Item2.Controls[0].Text;
            string filePath = $@"{temporaryFolder}\{fileName}";

            // �������� ����� ������� .cs � �����.
            try
            {
                File.WriteAllText(filePath, activeCodeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // ���������� �����.
            if (filePath != "") CompileFromText(filePath);
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ����������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void RunClick(object sender, EventArgs e)
        {
            if (compilerPath == "" || !compilerPath.EndsWith(".exe"))
            {
                MessageBox.Show("Please enter the path to compiler in Settings > Compiler path...");
                return;
            }
            // ������������� ������� ���������� �����������.
            AddMessageBox();

            string fileName = "";

            // ��������� ����� ����� � �����.
            for (int i = 0; i < tabInfo.Count; i++) if (tabInfo[i].Item4) fileName = tabInfo[i].Item2.Controls[0].Text;
            string filePath = $@"{temporaryFolder}\{fileName}";

            // �������� ����� ������� .cs � �����.
            try
            {
                File.WriteAllText(filePath, activeCodeTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // ��������� ����� ��� ������������ �����.
            string processName = fileName.Split('.')[0] + ".exe";

            // ���������� � ���������� �����.
            if (filePath != "") CompileFromText(filePath);
            RunProgram(processName);
            DeleteFile(processName);
        }
        #endregion

        #region ������ ������ �� ��������.

        /// <summary>
        /// ����� ������������ ��������� ��������� ���������� �������.
        /// </summary>
        /// <param name="value">��������� ��������� ���������� �������.</param>
        private void SwitchFontControls(bool value)
        {
            // ������� ��������� ���������� ������� � ��������� ������ ������ � ���������� ����.
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
        /// ����� ������ ������� ������ ������.
        /// </summary>
        private void CallFontDialog()
        {
            // �������, ���� ��������� RichTextBox.
            if (activeTextBox == null) return;

            // �������� ������ ������� ����� ������.
            FontDialog fontDialog = new FontDialog();
            fontDialog.ShowColor = true;

            fontDialog.Font = activeTextBox.SelectionFont;
            fontDialog.Color = activeTextBox.SelectionColor;

            // ��������� ������.
            if (fontDialog.ShowDialog() != DialogResult.Cancel)
            {
                activeTextBox.SelectionFont = fontDialog.Font;
                activeTextBox.SelectionColor = fontDialog.Color;
                currentFont = fontDialog.Font;
            }
        }

        /// <summary>
        /// ����� ������������ ����� ������.
        /// </summary>
        /// <param name="fontStyle">����� ������.</param>
        private void ToggleFontStyle(FontStyle fontStyle)
        {
            bool validation = false;
            if (activeTextBox == null) return;
            if (currentFont == null) currentFont = activeTextBox.SelectionFont;

            // ������������� ���� ��������� - true, ���� ���������� ����� ��� ����� ������ ����� ������.
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

            // ������������ ����� ������.
            if (activeTextBox == null) return;

            // ����������� ���������� �������.
            int selectionStart = activeTextBox.SelectionStart;
            int selectionLength = activeTextBox.SelectionLength;

            // ���������� ����� �����������.
            if (!validation)
            {
                currentFont = new Font(currentFont, currentFont.Style | fontStyle);
                for (int i = selectionStart; i < selectionStart + selectionLength; i++)
                {
                    activeTextBox.Select(i, 1);
                    activeTextBox.SelectionFont = new Font(activeTextBox.SelectionFont, activeTextBox.SelectionFont.Style | fontStyle);
                }
            }
            // �������� ����� �����������.
            else
            {
                currentFont = new Font(currentFont, currentFont.Style & ~fontStyle);
                for (int i = selectionStart; i < selectionStart + selectionLength; i++)
                {
                    activeTextBox.Select(i, 1);
                    activeTextBox.SelectionFont = new Font(activeTextBox.SelectionFont, activeTextBox.SelectionFont.Style & ~fontStyle);
                }
            }

            // �������������� ���������.
            activeTextBox.SelectionStart = selectionStart;
            activeTextBox.SelectionLength = selectionLength;
        }

        #endregion

        #region ����������� ������� ������� �� ������ ������������ ����.

        /// <summary>
        /// ���������� ������� �� ������ "Select all" � ����������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void selectAllTextContextMenuItem_Click(object sender, EventArgs e)
        {
            // ��������� ����� ������ � �������� RichTextBox.
            if (activeTextBox != null) activeTextBox.SelectAll();
            // ��������� ����� ������ � �������� FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.SelectAll();
        }

        /// <summary>
        /// ���������� ������� �� ������ "Cut" � ����������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void cutContextMenuItem_Click(object sender, EventArgs e)
        {
            // ��������� ������ � �������� RichTextBox.
            if (activeTextBox != null) activeTextBox.Cut();
            // ��������� ������ � �������� FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Cut();
        }

        /// <summary>
        /// ���������� ������� �� ������ "Copy" � ����������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void copyContextMenuItem_Click(object sender, EventArgs e)
        {
            // ����������� ������ � �������� RichTextBox.
            if (activeTextBox != null) activeTextBox.Copy();
            // ����������� ������ � �������� FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Copy();
        }

        /// <summary>
        /// ���������� ������� �� ������ "Paste" � ����������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void pasteContextMenuItem_Click(object sender, EventArgs e)
        {
            // ������� ������ � �������� RichTextBox.
            if (activeTextBox != null) activeTextBox.Paste();
            // ������� ������ � �������� FastColoredTextBox.
            else if (activeCodeTextBox != null) activeCodeTextBox.Paste();
        }

        /// <summary>
        /// ���������� ������� �� ������ "Format" � ����������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void formatContextMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ ������, ���� ������� RichTextBox.
            if (activeTextBox == null) return;
            else CallFontDialog();
        }

        #endregion

        #region ������ ���������� ������.
        /// <summary>
        /// ����� ���������� ����� � ��� ����������� ����������.
        /// </summary>
        private void Save()
        {
            // �������, ���� ��� �������� �������.
            if (tabInfo.Count == 0) return;

            // ������� � "���������� ���", ���� ���� ��� ������ �� ����� ���������� ������ ����������.
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

            // ����������� ������� �������� �������.
            int activeIndex = 0;
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4) activeIndex = i;
            }

            // ���������� .rtf ������.
            if (System.IO.Path.GetExtension(fileChangeList[activeIndex].Item1) == ".rtf")
            {
                activeTextBox.SaveFile(fileChangeList[activeIndex].Item1, RichTextBoxStreamType.RichText);
            }
            // ���������� ������ ���������� .txt, .cfg, .ini, .csv.
            else if (System.IO.Path.GetExtension(fileChangeList[activeIndex].Item1) != ".cs")
            {
                activeTextBox.SaveFile(fileChangeList[activeIndex].Item1, RichTextBoxStreamType.PlainText);
            }
            // ���������� .cs ������.
            else
            {
                activeCodeTextBox.SaveToFile(fileChangeList[activeIndex].Item1, System.Text.Encoding.Default);
            }

            // ����������� ���������� ��������� � �������.
            fileChangeList[activeIndex] = Tuple.Create(fileChangeList[activeIndex].Item1, false);
        }

        /// <summary>
        /// ����� "���������� ���" �����.
        /// </summary>
        private void SaveAs()
        {
            string filename = "";

            // ������������ ������ ���������� �����.
            using (SaveFileDialog dlgSave = new SaveFileDialog())
            {
                try
                {
                    // ��������� ���������� �� ���������.
                    dlgSave.DefaultExt = "txt";
                    // ��������� �������� ����.
                    dlgSave.Title = "Save File As...";
                    // ��������� ���������� ����� (�������������� �����������).
                    dlgSave.Filter = "Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf|C# files (*.cs)|*.cs" +
                        "|Configuration files (*.cfg)|*.cfg|Initialization files (*.ini)|*.ini|CSV files (*.csv)|*.csv";

                    // �������� � ������ ��������� ������ ����.
                    if (dlgSave.ShowDialog() == DialogResult.OK)
                    {
                        filename = System.IO.Path.GetFileName(dlgSave.FileName);

                        // ���������� �� FastColoredTextBox.
                        if (activeTextBox == null && activeCodeTextBox != null)
                        {
                            string source = activeCodeTextBox.Text;
                            activeCodeTextBox.SaveToFile(dlgSave.FileName, System.Text.Encoding.Default);
                        }
                        // ���������� �� ��������� RichTextBox.
                        else
                        {
                            // ���������� ������ ���������� .txt, .cfg, .ini, .csv.
                            if (System.IO.Path.GetExtension(dlgSave.FileName) != ".rtf")
                            {
                                activeTextBox.SaveFile(dlgSave.FileName, RichTextBoxStreamType.PlainText);
                            }
                            // ���������� .cs ������.
                            else
                            {
                                activeTextBox.SaveFile(dlgSave.FileName, RichTextBoxStreamType.RichText);
                            }
                        }
                    }
                }
                catch (Exception errorMsg)
                {
                    // ����� ��������� �� ����������.
                    MessageBox.Show(errorMsg.Message);
                }
            }

            // ����������� ����������.
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // ����������� ���������� ��������� � �������.
                    if (!fileChangeList[i].Item1.StartsWith("unnamed"))
                    {
                        fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, false);
                    }
                    // ����������� ���������� ��� ���������� �����.
                    else
                    {
                        if (String.IsNullOrEmpty(filename)) return;
                        // ����������� ��������� � ��������.
                        fileChangeList[i] = Tuple.Create(filename, false);
                        tabInfo[i].Item2.Controls[0].Text = filename;
                        // ����� ������� ������� ��� ����.
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
                        // ����� ������� ������� ��� ������.
                        else SwitchFontControls(true);
                    }
                }
            }
        }

        /// <summary>
        /// ����� ���������� ���� �������� ������ � �� ����������� ����������.
        /// </summary>
        private void SaveAll()
        {
            // ����� �������� � ��������������� ������.
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    // ������ �� ���� �������� ������.
                    for (int i = 0; i < fileChangeList.Count; i++)
                    {
                        // ������� � ������ "��������� ���", ���� ���� ��� ������ � ��������� � �� �������.
                        if (fileChangeList[i].Item1.StartsWith("unnamed"))
                        {
                            SaveAs();
                        }
                        // ���������� ������������ ������.
                        else
                        {
                            // ���������� .rtf ������.
                            if (System.IO.Path.GetExtension(fileChangeList[i].Item1) == ".rtf")
                            {
                                tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.RichText);
                            }
                            // ���������� ������ ���������� .txt, .cfg, .ini, .csv.
                            else if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".cs")
                            {
                                tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.PlainText);
                            }
                            // ���������� .cs ������.
                            else
                            {
                                tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ����� ��������� �� ����������.
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        #endregion

        #region ������ �������� � �������� ������.
        /// <summary>
        /// ����� �������� �����.
        /// </summary>
        private void OpenFile()
        {
            // ���������� ������ ����������� ���� �������� �����.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // ��������� ����.
            openFileDialog.Title = "Open File...";
            // ������ ������ ����������.
            openFileDialog.Filter = "Text files (*.txt)|*.txt|Rich Text Format files (*.rtf)|*.rtf|C# files (*.cs)|*.cs" +
                "|Configuration files (*.cfg)|*.cfg|Initialization files (*.ini)|*.ini|CSV files (*.csv)|*.csv";

            // ������������� RichTextBox ��� ��������� ������.
            RichTextBox txtBox = new RichTextBox();
            txtBox.Dock = DockStyle.Fill;
            txtBox.BorderStyle = BorderStyle.None;
            txtBox.MouseUp += richtextbox_MouseUp;

            // ������������� FastColoredTextBox ��� .cs ������.
            FastColoredTextBoxNS.FastColoredTextBox textBoxNS = new FastColoredTextBoxNS.FastColoredTextBox();
            textBoxNS.Font = new Font("Consolas", textBoxNS.Font.Size);
            textBoxNS.Dock = DockStyle.Fill;
            textBoxNS.BorderStyle = BorderStyle.None;
            textBoxNS.MouseUp += richtextbox_MouseUp;

            string filename = "";

            try
            {
                // � ������ ��������� ������ ��������������� �����.
                if (openFileDialog.ShowDialog() == DialogResult.OK &&
               openFileDialog.FileName.Length > 0)
                {
                    // ���������� ���� � �����.
                    filename = openFileDialog.FileName;

                    // �������� .rtf �����.
                    if (System.IO.Path.GetExtension(filename) == ".rtf")
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.RichText);
                    }
                    // �������� .cs �����.
                    else if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        textBoxNS.Language = FastColoredTextBoxNS.Language.CSharp;
                        string sourceCode = File.ReadAllText(filename);
                        textBoxNS.Text = sourceCode;
                    }
                    // �������� ����� ����� ����������.
                    else
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.PlainText);
                    }

                    // ���������� ��� ��������� ��������� ���������� �������.
                    if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        SwitchFontControls(false);
                    }
                    else
                    {
                        SwitchFontControls(true);
                    }

                    // �������� �� ������� ����� ����� ��������.
                    if (fileChangeList.Contains(Tuple.Create(filename, false)))
                    {
                        MessageBox.Show("File is already opened!");
                        return;
                    }

                    // ���������� ���������� � ����� � �������.
                    fileChangeList.Add(Tuple.Create(filename, false));
                }
            }
            catch (Exception ex)
            {
                // ����� ��������� �� ����������.
                MessageBox.Show(ex.Message);
            }

            // ������������� ������������ �����.
            if (!string.IsNullOrEmpty(filename))
            {
                InitializeLoadedFile(filename, txtBox, textBoxNS);
            }
        }

        /// <summary>
        /// ����� ������������� ������������ �����.
        /// </summary>
        /// <param name="filename">���� � �����.</param>
        /// <param name="txtBox">������������������ ��� ���� RichTextBox.</param>
        /// <param name="textBoxNS">������������������ ��� ���� FastColoredTextBox.</param>
        private void InitializeLoadedFile(string filename, RichTextBox txtBox, FastColoredTextBoxNS.FastColoredTextBox textBoxNS)
        {
            #region ������������� �������.
            // ������������� "����" ������� � ��������� ��� �������.
            Panel tabBase = new Panel();
            tabBase.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
            tabBase.Height = 34;
            tabBase.Location = new Point(1, 0);
            tabBase.BackColor = Color.White;
            // ������������� ������ (��������) ������� � ������ �����, ���������� ������ ���� �������.
            Label fileLabel = new Label();
            tabBase.Controls.Add(fileLabel);
            fileLabel.Text = Path.GetFileName(filename);
            fileLabel.AutoSize = false;
            fileLabel.TextAlign = ContentAlignment.MiddleCenter;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.Click += PanelClick;
            // ������������� ������-������� (�����) �������, ��������� �������.
            Panel tabBorder = new Panel();
            tabBorder.Width = tabBase.Width + 2;
            tabBorder.Height = 34;
            tabBorder.Controls.Add(tabBase);
            tabBorder.BackColor = Color.LightGray;
            #endregion

            // ������ ������ ������ �������� �������.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                tabInfo[i] = Tuple.Create(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
            }

            // ���������� ���������� � ������� � ������ ��������.
            if (System.IO.Path.GetExtension(filename) == ".cs")
            {
                tabInfo.Add(Tuple.Create(tabBorder, tabBase, (RichTextBox)null, true, textBoxNS));
            }
            else
            {
                tabInfo.Add(Tuple.Create(tabBorder, tabBase, txtBox, true, (FastColoredTextBoxNS.FastColoredTextBox)null));
            }

            // ��������� ������ ������� � ������� ������� ����������� �����������.
            RenderTabs();
            HideMessageBox();

            // ����������� �������� ��������� ������� (���\�����).
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

            // ��������� ��������� ������� � ���������� � � ����.
            textBoxPanel.Visible = true;
            textBoxPanel.Controls.Clear();
            if (activeCodeTextBox != null) textBoxPanel.Controls.Add(activeCodeTextBox);
            else textBoxPanel.Controls.Add(activeTextBox);

            // ���������� ������������ �������.
            txtBox.TextChanged += new EventHandler(richtextbox_TextChanged);
            textBoxNS.TextChanged += fastrichtextbox_TextChanged;

            // ��������� ��������� ����������.
            if (activeCodeTextBox != null)
            {
                UpdateSyntaxHighlight();
            }

            // ��������� ��������� ���� � ��������� ������� ��������������.
            this.Text = System.IO.Path.GetFileName(filename) + " - Notepad+";
            if (!autosaveTimer.Enabled) autosaveTimer.Enabled = true;
        }

        /// <summary>
        /// ����� �������� ������ �����.
        /// </summary>
        private void NewFile()
        {
            // ���������� ����������� �������� ���������� (������������ �����) ������.
            Program.unnamedFileCount += 1;

            // ������������� ��������� �������.
            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.BorderStyle = BorderStyle.None;
            rtb.TextChanged += richtextbox_TextChanged;
            rtb.MouseUp += richtextbox_MouseUp;
            activeTextBox = rtb;
            activeCodeTextBox = null;

            #region ������������� �������.
            // ������������� "����" ������� � ��������� ��� �������.
            Panel tabBase = new Panel();
            tabBase.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
            tabBase.Height = 34;
            tabBase.Location = new Point(1, 0);
            // ������������� ������ (��������) ������� � ������ �����, ���������� ������ ���� �������.
            Label fileLabel = new Label();
            tabBase.Controls.Add(fileLabel);
            fileLabel.Text = "unnamed" + Program.unnamedFileCount;
            fileLabel.AutoSize = false;
            fileLabel.TextAlign = ContentAlignment.MiddleCenter;
            fileLabel.Dock = DockStyle.Fill;
            fileLabel.Click += PanelClick;
            // ������������� ������-������� (�����) �������, ��������� �������.
            Panel tabBorder = new Panel();
            tabBorder.Width = tabBase.Width + 2;
            tabBorder.Height = 34;
            tabBorder.Controls.Add(tabBase);
            tabBorder.BackColor = Color.LightGray;
            #endregion

            // ������ ������ ������ �������� �������.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                tabInfo[i] = Tuple.Create(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
            }

            // ���������� ���������� � ������� � ������ ��������.
            tabInfo.Add(Tuple.Create(tabBorder, tabBase, rtb, true, (FastColoredTextBoxNS.FastColoredTextBox)null));
            fileChangeList.Add(Tuple.Create("unnamed" + (Program.unnamedFileCount), false));

            // ��������� ������ ������� � ������� ������� ����������� �����������.
            RenderTabs();
            HideMessageBox();

            // ��������� ��������� ������� � ���������� � � ����.
            textBoxPanel.Controls.Clear();
            textBoxPanel.Controls.Add(activeTextBox);
            tabPanel.Visible = true;
            textBoxPanel.Visible = true;

            // ���������� ��� ��������� ��������� ���������� �������.
            SwitchFontControls(true);

            // ��������� ��������� ����.
            this.Text = "unnamed" + Program.unnamedFileCount + " - Notepad+";

            // ��������� ������ �� ������-����.
            statusStripEventTimer.Enabled = true;
            autosavingStatusLabel.Text = "New file created!";
            autosavingStatusLabel.Visible = true;
        }
        #endregion

        #region ������ ����������, �������� � ���������� ��������.
        /// <summary>
        /// ����� ���������� ��������.
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // ������������� � ���������� ������ ����� � �������� ������.
                List<string> filePaths = new List<string>();
                foreach (var fileInfo in fileChangeList) if (!fileInfo.Item1.StartsWith("unnamed")) filePaths.Add(fileInfo.Item1);

                // ���������� ���������� ����.
                int theme = Program.GlobalThemeIndex;

                // ���������� ���������� �������������� � ��������������.
                string autosaveInterval = intervalComboBox.SelectedItem.ToString().Split(' ')[0];
                string loggingInterval = loggingToolStripComboBox.SelectedItem.ToString().Split(' ')[0];

                // ���������� ������ ��������� ����������.
                string keywordColorString = keywordColor.ToArgb().ToString();
                string stringColorString = stringColor.ToArgb().ToString();
                string variableColorString = variableColor.ToArgb().ToString();
                string classColorString = classColor.ToArgb().ToString();
                string methodColorString = methodColor.ToArgb().ToString();

                // ����������� ������ ��������.
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

                // ���������� ����� � ������.
                foreach (var filePath in filePaths)
                {
                    settings.Add(filePath);
                }

                // ������ �������� � ����.
                File.WriteAllLines(settingsPath, settings);
            }
            catch (Exception ex)
            {
                // ����� ��������� �� ����������.
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// ����� �������� ��������.
        /// </summary>
        /// <param name="loadPrevFiles">���� �������� �������� ����� ������.</param>
        private void LoadSettings(bool loadPrevFiles)
        {
            // ������� � ������ ��������������� ����� �������� (��� ������ �������).
            if (!File.Exists(settingsPath)) return;

            try
            {
                // ���������� �������� �� �����.
                string[] settings = File.ReadAllLines(settingsPath);

                // ��������� ����.
                switch (settings[0].Split('=')[1])
                {
                    case "0":
                        themeComboBox.SelectedIndex = 0;
                        break;
                    case "1":
                        themeComboBox.SelectedIndex = 1;
                        break;
                }

                // ��������� ������ ���������.
                keywordColor = Color.FromArgb(int.Parse(settings[1].Split('=')[1]));
                stringColor = Color.FromArgb(int.Parse(settings[2].Split('=')[1]));
                variableColor = Color.FromArgb(int.Parse(settings[3].Split('=')[1]));
                methodColor = Color.FromArgb(int.Parse(settings[4].Split('=')[1]));
                classColor = Color.FromArgb(int.Parse(settings[5].Split('=')[1]));

                // ��������� ������ ���������.
                variableBrush = new SolidBrush(variableColor);
                methodBrush = new SolidBrush(methodColor);
                classBrush = new SolidBrush(classColor);
                keywordBrush = new SolidBrush(keywordColor);
                stringBrush = new SolidBrush(stringColor);

                // ��������� ������ ���������.
                VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);
                MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);
                ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);
                KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);
                StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Italic);

                // ��������� ��������� ��������������.
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

                // ��������� ��������� ��������������.
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

                // ��������� ���� � �����������.
                compilerPath = settings[8].Split('=')[1];
                compilerPathToolStripTextBox.Text = settings[8].Split('=')[1];

                // �������� ���� � �����, ���� �� ����������� �����.
                if (!loadPrevFiles)
                {
                    LoadTheme();
                    return;
                }

                // �������� ����� �������� ������.
                LoadPreviousFiles(settings);
            }
            catch (Exception ex)
            {
                // ����� ��������� �� ������.
                MessageBox.Show(ex.Message);
            }

            // �������� ���� � ���������� ���������.
            LoadTheme();
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ����� �������� ����� �������� ������.
        /// </summary>
        /// <param name="settings">������ ����� ��������.</param>
        private void LoadPreviousFiles(string[] settings)
        {
            for (int i = 10; i < settings.Length; i++)
            {
                // ������ �������� � ������ ��������������� �����.
                if (!File.Exists(settings[i])) continue;

                // ������������� ��������� �������� (��� ���� � ������).
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
                    // �������� .rtf �����.
                    if (System.IO.Path.GetExtension(filename) == ".rtf")
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.RichText);
                    }
                    // �������� .cs �����.
                    else if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        textBoxNS.Language = FastColoredTextBoxNS.Language.CSharp;
                        string sourceCode = File.ReadAllText(filename);
                        textBoxNS.Text = sourceCode;
                    }
                    // �������� ����� ����� ����������.
                    else
                    {
                        txtBox.LoadFile(filename, RichTextBoxStreamType.PlainText);
                    }

                    // ���������� ��� ��������� ��������� ���������� �������.
                    if (System.IO.Path.GetExtension(filename) == ".cs")
                    {
                        SwitchFontControls(false);
                    }
                    else
                    {
                        SwitchFontControls(true);
                    }

                    // �������� �� ������� ����� ����� ��� ��������.
                    if (fileChangeList.Contains(Tuple.Create(filename, false)))
                    {
                        MessageBox.Show("File is already opened!");
                        return;
                    }

                    // ���������� ���������� � ����� � ������ ��������.
                    fileChangeList.Add(Tuple.Create(filename, false));
                }
                catch (Exception ex)
                {
                    // ����� ��������� �� ������.
                    MessageBox.Show(ex.Message);
                }

                if (!string.IsNullOrEmpty(filename))
                {
                    // ������������� ������������ �����.
                    InitializeLoadedFile(filename, txtBox, textBoxNS);
                }
            }
        }
        #endregion

        #region ������ ���������� ��������������.
        /// <summary>
        /// ���������� ������� "����" ������� ��������������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private async void loggingTimer_Tick(object sender, EventArgs e)
        {
            // ����������� ���������� ������ �������������� ������.
            await Task.Run(() => LogFiles());
        }

        /// <summary>
        /// ����� �������������� ������.
        /// </summary>
        private void LogFiles()
        {
            // ����� �������� � ��������������� ������.
            this.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    for (int i = 0; i < tabInfo.Count; i++)
                    {
                        // ���������� �� �������������� ������������ ����� ������.
                        if (tabInfo[i].Item2.Controls[0].Text.StartsWith("unnamed")) continue;
                        RichTextBox rtb = null;

                        // ������������ ������ ���� ����������.
                        string date = DateTime.Now.ToString().Replace(" ", "_").Replace(":", "_");
                        // ������������ ����.
                        string filePath = $@"Logs\{Path.GetFileName(fileChangeList[i].Item1) + "_" + date}{Path.GetExtension(fileChangeList[i].Item1)}";

                        rtb = tabInfo[i].Item3;

                        // ���������� ����������� ��������������� ��������� ��������.
                        if (rtb != null)
                        {
                            // ���������� ��-.rtf ������.
                            if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".rtf")
                            {
                                rtb.SaveFile(filePath, RichTextBoxStreamType.PlainText);
                            }
                            // ���������� .rtf ������.
                            else
                            {
                                rtb.SaveFile(filePath, RichTextBoxStreamType.RichText);
                            }
                        }
                        // ���������� .cs ������.
                        else
                        {
                            tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // ����� ��������� �� ����������.
                    MessageBox.Show(ex.Message);
                }
            }));
        }
        #endregion

        #region ������ ���������� ��������������.
        /// <summary>
        /// ���������� ������� "����" ������� ��������������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private async void autosaveTimer_Tick(object sender, EventArgs e)
        {
            // ���������� ���� Progress ��� �������� ��������� �������������� ��������-����.
            Progress<int> progress = new Progress<int>(value => { autosavingProgressBar.Value = value; });

            // ��������� ������ � ��������-���� �� ������-����.
            autosavingStatusLabel.Visible = true;
            autosavingProgressBar.Visible = true;

            // ����������� ���������� ������ �������������� ������.
            await Task.Run(() => AutoSaveFiles(progress));

            // ������� ������ � ��������-���� �� ������-����.
            autosavingStatusLabel.Visible = false;
            autosavingProgressBar.Visible = false;
        }

        /// <summary>
        /// ����� �������������� ������.
        /// </summary>
        /// <param name="progress">��������� ��� �������� ���������.</param>
        private void AutoSaveFiles(IProgress<int> progress)
        {
            // ����� �������� � ��������������� ������.
            this.Invoke((MethodInvoker)(() =>
            {
                for (int i = 0; i < tabInfo.Count; i++)
                {
                    // ���������� �� �������������� ��������� � �� ���������� ������.
                    if (tabInfo[i].Item2.Controls[0].Text.StartsWith("unnamed")) continue;

                    // ���������� .rtf ������.
                    if (System.IO.Path.GetExtension(fileChangeList[i].Item1) == ".rtf")
                    {
                        tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.RichText);
                    }
                    // ���������� ������ ���� ����������.
                    else if (System.IO.Path.GetExtension(fileChangeList[i].Item1) != ".cs")
                    {
                        tabInfo[i].Item3.SaveFile(fileChangeList[i].Item1, RichTextBoxStreamType.PlainText);
                    }
                    // ���������� .cs ������.
                    else
                    {
                        tabInfo[i].Item5.SaveToFile(fileChangeList[i].Item1, System.Text.Encoding.Default);
                    }

                    // �������� ��������� � ������ �������� � ����������� � ������.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, false);

                    // �������� ��������� ����� ���������.
                    double completionRate = (double)(i + 1) / tabInfo.Count;
                    progress.Report((int)completionRate);
                }
            }));
        }
        #endregion

        #region ������ ������ ������-����.
        /// <summary>
        /// ���������� ������� "����" ������� ���������� ������ ������-������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void statusStripEventTimer_Tick(object sender, EventArgs e)
        {
            // ����������� ������ � �������.
            autosavingStatusLabel.Text = "Autosaving...";
            autosavingStatusLabel.Visible = false;
            statusStripEventTimer.Enabled = false;
        }
        #endregion

        #region ������ ��������� ������� �� ������ ���� ����������.
        /// <summary>
        /// ���������� ������� �� ������ �������� ������ ����� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ �������� ������ �����.
            NewFile();
        }

        /// <summary>
        /// ���������� ������� �� ������ �������� ����� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ �������� �����.
            OpenFile();
        }

        /// <summary>
        /// ���������� ������� �� ������ �������� ������ ����� � ����� ���� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ������������� ������ ������ � ������������ �� ��������.
            Thread newThread = new Thread(() =>
            {
                // �������� ����� �����, ������ � ������� ��������� � �������� ������ �����.
                MainWindow newWindow = new MainWindow() { loadPrevFiles = false };
                newWindow.FormClosing += Form1_FormClosing;
                newWindow.tabInfo.Clear();
                newWindow.fileChangeList.Clear();
                newWindow.NewFile();
                newWindow.RenderTabs();
                Application.Run(newWindow);
            });
            newThread.TrySetApartmentState(ApartmentState.STA);
            // ������ ������.
            newThread.Start();
        }

        /// <summary>
        /// ���������� ������� �� ������ ���������� ���� �������� ������ � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private async void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ��������� ������ �� ������-����.
            autosavingStatusLabel.Text = "Saving all documents...";
            autosavingStatusLabel.Visible = true;

            // ����������� ���������� ������ ���������� ���� �������� ������.
            await Task.Run(() => SaveAll());

            // ������� ������ �� ������-����.
            autosavingStatusLabel.Visible = false;
            autosavingStatusLabel.Text = "Autosaving:";
        }

        /// <summary>
        /// ���������� ������� �� ������ "���������� ���" � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ "���������� ���".
            SaveAs();
        }

        /// <summary>
        /// ���������� ������� �� ������ ��������� ������ � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ ��������� ������.
            if (tabInfo.Count != 0) CallFontDialog();
        }

        /// <summary>
        /// ���������� ������� �� ������ ���������� ����� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ ���������� �����.
            if (tabInfo.Count != 0 && activeTextBox != null) Save();
        }

        /// <summary>
        /// ���������� ��������� ���������� ������� � ���� ������ ��������� �������������� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void intervalComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ��������� ��������� ��������������.
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
        /// ���������� ��������� ���������� ������� � ���� ������ ���� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        public void themeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ��������� ���������� ����.
            Program.GlobalThemeIndex = themeComboBox.SelectedIndex;
        }

        /// <summary>
        /// ���������� ��������� ���� � ����������� � ���� ��������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void compilerPathToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            // ��������� ���� � �����������.
            compilerPath = compilerPathToolStripTextBox.Text;
        }

        /// <summary>
        /// ���������� ��������� ���������� ������� � ���� ������ ��������� �������������� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void loggingToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // ��������� ��������� ��������������.
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
        /// ���������� ������� �� ������ �������� ���� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // �������� ����.
            this.Close();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ���������� �������� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ ������ ���������� ��������.
            Undo();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������� ���������� �������� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������ ������� ���������� ��������.
            Redo();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ����� ��������� �������� ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void keywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ �����.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)keywordBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // ���������� ������ ����� � ��������������� ������.
            keywordColor = colordlg.Color;
            keywordBrush = new SolidBrush(keywordColor);
            KeywordStyle = new FastColoredTextBoxNS.TextStyle(keywordBrush, null, FontStyle.Regular);

            // ���������� ��������� ����������.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ����� ��������� �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void stringsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ �����.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)stringBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // ���������� ������ ����� � ��������������� ������.
            stringColor = colordlg.Color;
            stringBrush = new SolidBrush(stringColor);
            StringStyle = new FastColoredTextBoxNS.TextStyle(stringBrush, null, FontStyle.Regular);

            // ���������� ��������� ����������.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ����� ��������� ����������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void variablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ �����.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)variableBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // ���������� ������ ����� � ��������������� ������.
            variableColor = colordlg.Color;
            variableBrush = new SolidBrush(variableColor);
            VariableStyle = new FastColoredTextBoxNS.TextStyle(variableBrush, null, FontStyle.Regular);

            // ���������� ��������� ����������.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ����� ��������� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void methodsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ �����.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)methodBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // ���������� ������ ����� � ��������������� ������.
            methodColor = colordlg.Color;
            methodBrush = new SolidBrush(methodColor);
            MethodStyle = new FastColoredTextBoxNS.TextStyle(methodBrush, null, FontStyle.Regular);

            // ���������� ��������� ����������.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ���������� ������� �� ������ ������ ����� ��������� �������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void classesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // ����� ������� ������ �����.
            ColorDialog colordlg = new ColorDialog();
            colordlg.Color = ((SolidBrush)classBrush).Color;

            if (colordlg.ShowDialog() == DialogResult.Cancel)
                return;

            // ���������� ������ ����� � ��������������� ������.
            classColor = colordlg.Color;
            classBrush = new SolidBrush(classColor);
            ClassStyle = new FastColoredTextBoxNS.TextStyle(classBrush, null, FontStyle.Regular);

            // ���������� ��������� ����������.
            UpdateSyntaxHighlight();
        }

        /// <summary>
        /// ���������� ������� �� ������ About.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, Color> themeDict;
            // �������� ������� ������ ���� � ����������� �� ��������� ����.
            if (Program.GlobalThemeIndex == 0) themeDict = lightThemeColors;
            else themeDict = darkThemeColors;

            // ������������� ����� � ������.
            HelpForm about = new HelpForm();
            about.BackColor = this.BackColor;
            about.headlineLabel.ForeColor = themeDict["TabLabelForeColor_Active"];
            about.textLabel.ForeColor = themeDict["TabLabelForeColor_Active"];

            // ������ �����.
            about.ShowDialog();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ ������������������ ���� � ����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void codeFomattingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isChanged = false;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item5 == null || !tabInfo[i].Item4) continue;

                // ���������� ����� ������� ������������� ��������� �� ��������� ������ ��� ���������� ���������.
                isChanged = fileChangeList[i].Item2;
                string text = tabInfo[i].Item5.Text;
                // ������������������ ��������.
                text = CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().ToFullString();
                // ������������ ������ ��� ��������������� ��������� ���� ����� ����.
                tabInfo[i].Item5.Text = "";
                tabInfo[i].Item5.Text = text;
                // ������� ������� � �������� ���������.
                fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, isChanged);
            }
        }
        #endregion

        #region ������ ��������� ����������.
        /// <summary>
        /// ����� ������������ REGEX-������� ��� ������ ��� ����������, ����������� � �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <returns>REGEX-������ ��� ������ ��� ����������.</returns>
        private string VariableNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // ������� ������ ��� ������ Roslyn ��� ������������ ��������������� ������.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // ������������ ������� ��� ����������.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<VariableDeclaratorSyntax>().Select(v => v.Identifier.Text)
            .Concat(syntaxTree.GetRoot().DescendantNodes().OfType<ParameterSyntax>().Select(p => p.Identifier.Text))
            .ToArray();

            // ������� REGEX-������� (������).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// ����� ������������ REGEX-������� ��� ������ ��� �������, ����������� � �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <returns>REGEX-������ ��� ������ ��� �������.</returns>
        private string MethodNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // ������� ������ ��� ������ Roslyn ��� ������������ ��������������� ������.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // ������������ ������� ��� �������.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Select(v => v.Identifier.Text).ToArray();

            // ������� REGEX-������� (������).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// ����� ������������ REGEX-������� ��� ������ ��� �������, ����������� � �����.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <returns>REGEX-������ ��� ������ ��� �������.</returns>
        private string ClassNamesPattern(FastColoredTextBoxNS.FastColoredTextBox sender)
        {
            // ������� ������ ��� ������ Roslyn ��� ������������ ��������������� ������.
            var syntaxTree = CSharpSyntaxTree.ParseText(sender.Text);
            // ������������ ������� ��� �������.
            string[] identifierNames = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Select(v => v.Identifier.Text).ToArray();

            // ������� REGEX-������� (������).
            return $@"\b({String.Join("|", identifierNames)})\b";
        }

        /// <summary>
        /// ����� ���������� ��������� ����������.
        /// </summary>
        private void UpdateSyntaxHighlight()
        {
            bool isChanged = false;
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item5 == null) continue;

                // ���������� ����� ������� ������������� ��������� �� ��������� ������ ��� ���������� ���������.
                isChanged = fileChangeList[i].Item2;
                string text = tabInfo[i].Item5.Text;
                // ������������������ ��������.
                text = CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().ToFullString();
                // ������������ ������ ��� ��������������� ��������� ���� ����� ����.
                tabInfo[i].Item5.Text = "";
                tabInfo[i].Item5.Text = text;
                // ������� ������� � �������� ���������.
                fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, isChanged);
            }
        }

        #endregion

        #region ������ ��������� � ������ � ���������.
        /// <summary>
        /// ����� ��������� �������� �� ������ ��������.
        /// </summary>
        private void RenderTabs()
        {
            // ����� ������ ��������.
            int totalWidth = 0;
            if (tabInfo.Count == 0) return;
            tabPanel.Controls.Clear();

            foreach (var tab in tabInfo)
            {
                // ���������� �������� �� ������.
                tabPanel.Controls.Add(tab.Item1);
                // ������ ������� ��������� �������� � ������ ����� ������ (����������� ���������� � ������������� ��������).
                if (tabInfo.IndexOf(tab) == tabInfo.Count - 1)
                {
                    tab.Item1.Width = tabPanel.Width - totalWidth;
                    tab.Item2.Width = tab.Item1.Width - 2;
                }
                // ������ ������� ��� ���� �������� �� ������ ����� ��������.
                else
                {
                    tab.Item2.Width = tabInfo.Count == 0 ? tabPanel.Width - 2 : this.Width / tabInfo.Count - 2;
                    tab.Item1.Width = tab.Item2.Width + 2;
                }
                // ��������� ��������� �������� �� ������ ��������.
                tab.Item1.Location = new Point(totalWidth, 0);
                // ���������� �������� (���������) �������.
                if (tab.Item4)
                {
                    tab.Item1.BackColor = Color.MediumTurquoise;
                    tab.Item2.Location = new Point(2, 0);
                    tab.Item2.BackColor = Color.White;
                }
                // ���������� ���������� �������.
                else
                {
                    tab.Item1.BackColor = Color.FromArgb(217, 217, 217);
                    tab.Item2.Location = new Point(1, -1);
                    tab.Item2.BackColor = Color.FromArgb(234, 234, 236);
                }
                // ������ ����� ������ ����������� �������.
                totalWidth += tab.Item1.Width;
            }
            tabPanel.Visible = true;
            // �������� ����.
            LoadTheme();
        }

        /// <summary>
        /// ���������� ������� ������� �� ������ (�������).
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void PanelClick(object sender, EventArgs e)
        {
            // ����������� ��������� �� ������ ������ �������.
            for (int i = 0; i < tabInfo.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    tabInfo[i] = new Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, false, tabInfo[i].Item5);
                }
            }

            for (int i = 0; i < tabInfo.Count; i++)
            {
                // ������������ �� sender (�������, ����������� �������).
                if (tabInfo[i].Item2.Controls[0] == sender)
                {
                    // ��������� ������� � �������.
                    tabInfo[i] = new Tuple<Panel, Panel, RichTextBox, bool, FastColoredTextBoxNS.FastColoredTextBox>(tabInfo[i].Item1, tabInfo[i].Item2, tabInfo[i].Item3, true, tabInfo[i].Item5);

                    // ��������� ��������� ������ � ����� ��� .cs ������.
                    if (Path.GetExtension(fileChangeList[i].Item1) == ".cs")
                    {
                        SwitchFontControls(false);
                        activeCodeTextBox = tabInfo[i].Item5;
                        activeTextBox = null;
                        textBoxPanel.Controls.Clear();
                        textBoxPanel.Controls.Add(activeCodeTextBox);
                        UpdateSyntaxHighlight();
                    }
                    // ��������� ��������� ������ � ������� ��� ��-.cs ������.
                    else
                    {
                        SwitchFontControls(true);
                        activeTextBox = tabInfo[i].Item3;
                        activeCodeTextBox = null;
                        textBoxPanel.Controls.Clear();
                        textBoxPanel.Controls.Add(activeTextBox);
                    }

                    // ��������� ��������� ����.
                    this.Text = tabInfo[i].Item2.Controls[0].Text + " - Notepad+";
                }
            }

            // ����� ������ ��������� ������� � ������� ������� ��������� �����������.
            RenderTabs();
            HideMessageBox();
        }

        /// <summary>
        /// ����� ������� ������ ������ ������� ����� �������� ������� � �������� index.
        /// </summary>
        /// <param name="index">������ �������� �������.</param>
        private void MoveTabSelection(int index)
        {
            // ���� ���� ������� �� ��������.
            if (tabInfo.Count >= index + 1)
            {
                SelectTab(index);
            }
            // ���� �������� ������� ���� ���������.
            else
            {
                SelectTab(index - 1);
            }
            // ������� ������� ��������� �����������.
            HideMessageBox();
        }

        /// <summary>
        /// ����� ������ ������ �������.
        /// </summary>
        /// <param name="index">������ ��������� ����� �������� �������.</param>
        private void SelectTab(int index)
        {
            // ��������� ����� ��������� �������.
            tabInfo[index] = Tuple.Create(tabInfo[index].Item1, tabInfo[index].Item2, tabInfo[index].Item3, true, tabInfo[index].Item5);
            textBoxPanel.Controls.Clear();
            // ��������� ��������� ��������, ��������������� � ��������.
            if (tabInfo[index].Item3 != null)
            {
                // ��������� RichTextBox.
                textBoxPanel.Controls.Add(tabInfo[index].Item3);
                activeTextBox = tabInfo[index].Item3;
                activeCodeTextBox = null;
            }
            else
            {
                // ��������� FastColoredTextBox.
                textBoxPanel.Controls.Add(tabInfo[index].Item5);
                activeTextBox = null;
                activeCodeTextBox = tabInfo[index].Item5;
            }
            // ���������\����������� ��������� ���������� �������\�����.
            if (Path.GetExtension(fileChangeList[index].Item1) == ".cs") SwitchFontControls(false);
            else SwitchFontControls(true);

            // ��������� ��������� ����.
            this.Text = tabInfo[index].Item2.Controls[0].Text + " - Notepad+";
        }
        #endregion

        #region ������ ����������� ��������� ��������.
        /// <summary>
        /// ���������� ��������� ������ � ��������� ������� ���� RichTextBox.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void richtextbox_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // ��������� �������������� �������� ����� ������� ������������ ���������.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, true);
                }
            }
        }

        /// <summary>
        /// ���������� ��������� ������ � ��������� ������� ���� FastColoredTextBox.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void fastrichtextbox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            for (int i = 0; i < fileChangeList.Count; i++)
            {
                if (tabInfo[i].Item4)
                {
                    // ��������� �������������� �������� ����� ������� ������������ ���������.
                    fileChangeList[i] = Tuple.Create(fileChangeList[i].Item1, true);
                }
            }

            // ��������� ��� ����������, ������� � �������.
            e.ChangedRange.SetStyle(VariableStyle, VariableNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));
            e.ChangedRange.SetStyle(MethodStyle, MethodNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));
            e.ChangedRange.SetStyle(ClassStyle, ClassNamesPattern((FastColoredTextBoxNS.FastColoredTextBox)sender));

            // ��������� �������� ���� � ����� (���������� ������).
            ((FastColoredTextBoxNS.FastColoredTextBox)sender).SyntaxHighlighter.KeywordStyle = KeywordStyle;
            ((FastColoredTextBoxNS.FastColoredTextBox)sender).SyntaxHighlighter.StringStyle = StringStyle;
        }

        /// <summary>
        /// ���������� ������� ������� ���� ��� ��������� ��������.
        /// </summary>
        /// <param name="sender">������, ��������� �������.</param>
        /// <param name="e">��������� �������.</param>
        private void richtextbox_MouseUp(object sender, MouseEventArgs e)
        {
            // �����������, ������ �� ���.
            if (e.Button != MouseButtons.Right) return;
            // ��������� ������������ ����.
            contextMenuStrip1.Show((Control)sender, e.Location);
        }

        /// <summary>
        /// ����� ������ ���������� ��������� � �������� ��������� �������.
        /// </summary>
        private void Undo()
        {
            if (activeTextBox != null) activeTextBox.Undo();
            else if (activeCodeTextBox != null) activeCodeTextBox.Undo();
        }

        /// <summary>
        /// ����� ���������� ���������� ����������� ��������� � �������� ��������� �������.
        /// </summary>
        private void Redo()
        {
            if (activeTextBox != null) activeTextBox.Redo();
            else if (activeCodeTextBox != null) activeCodeTextBox.Redo();
        }

        #endregion

        #region ����� �������� ����.
        /// <summary>
        /// ����� �������� ��������� ����.
        /// </summary>
        public void LoadTheme()
        {
            Dictionary<string, Color> themeDict;
            // �������� ������� ������ ���� � ����������� �� ��������� ����.
            if (Program.GlobalThemeIndex == 0) themeDict = lightThemeColors;
            else themeDict = darkThemeColors;

            // ��������� ����� ����.
            menuStrip1.BackColor = themeDict["MenuBackColor"];
            // ��������� ����� ������ � ����.
            foreach (ToolStripMenuItem item in menuStrip1.Items)
            {
                item.ForeColor = themeDict["MenuForeColor"];
            }
            // ��������� ����� ������� � ��� �������.
            toolPanel.BackColor = themeDict["ToolPanelBackColor"];
            toolPanelBorderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            // ��������� ����� ������ ��������.
            borderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            tabPanel.BackColor = themeDict["TabBaseBackColor_Inactive"];
            // ��������� ����� ���� � ������ � ������� ���������� �����������.
            if (messageTextPanel.Controls.Count != 0)
            {
                foreach (RichTextBox rtb in messageTextPanel.Controls)
                {
                    rtb.BackColor = themeDict["RichTextBoxBackColor"];
                    rtb.ForeColor = themeDict["MenuForeColor"];
                }
            }
            // ������������ �� ���������.
            foreach (var tab in tabInfo)
            {
                // ��������� ���������� ������������ �������� ��������� � ��������� �������.
                if (tab.Item3 != null)
                {
                    tab.Item3.TextChanged -= new EventHandler(richtextbox_TextChanged);
                    tab.Item3.TextChanged -= new EventHandler(richtextbox_TextChanged);
                }
                else
                {
                    tab.Item5.TextChanged -= fastrichtextbox_TextChanged;
                }

                // ��������� ������ �������� ��������.
                if (tab.Item4)
                {
                    tab.Item1.BackColor = themeDict["TabBorderBackColor_Active"];
                    tab.Item2.BackColor = themeDict["TabBaseBackColor_Active"];
                    tab.Item2.Controls[0].ForeColor = themeDict["TabLabelForeColor_Active"];
                }
                // ��������� ������ ���������� ��������.
                else
                {
                    tab.Item1.BackColor = themeDict["TabBorderBackColor_Inactive"];
                    tab.Item2.BackColor = themeDict["TabBaseBackColor_Inactive"];
                    tab.Item2.Controls[0].ForeColor = themeDict["TabLabelForeColor_Inactive"];
                }

                // ��������� ������ ��������� ������� ��� ������ ����������, ����� .rtf � .cs.
                if (Path.GetExtension(fileChangeList[tabInfo.IndexOf(tab)].Item1) != ".rtf" && tab.Item3 != null)
                {
                    tab.Item3.BackColor = themeDict["RichTextBoxBackColor"];
                    tab.Item3.ForeColor = themeDict["MenuForeColor"];
                    tab.Item3.SelectAll();
                    tab.Item3.SelectionBackColor = themeDict["RichTextBoxBackColor"];
                }
                // ��������� ������ ��������� ������� ��� ������ ���������� .cs.
                else if (Path.GetExtension(fileChangeList[tabInfo.IndexOf(tab)].Item1) != ".rtf" && tab.Item5 != null)
                {
                    tab.Item5.BackColor = themeDict["RichTextBoxBackColor"];
                    tab.Item5.ForeColor = themeDict["MenuForeColor"];
                    tab.Item5.LineNumberColor = themeDict["TabLabelForeColor_Inactive"];
                    tab.Item5.ServiceLinesColor = themeDict["RichTextBoxBackColor"];
                    tab.Item5.IndentBackColor = themeDict["RichTextBoxBackColor"];
                }

                // ��������� ������������ �������� ���������.
                if (tab.Item3 != null)
                {
                    tab.Item3.TextChanged += new EventHandler(richtextbox_TextChanged);
                }
                else
                {
                    tab.Item5.TextChanged += fastrichtextbox_TextChanged;
                }
            }
            // ��������� ������ ������-����.
            statusPanel.BackColor = themeDict["ToolPanelBackColor"];
            statusBorderPanel.BackColor = themeDict["TabBorderBackColor_Inactive"];
            // ��������� ����� �����.
            this.BackColor = themeDict["RichTextBoxBackColor"];
            // �������� ������ �������.
            label2.ForeColor = Color.FromArgb(118, 123, 129);
            autosavingStatusLabel.ForeColor = themeDict["TabLabelForeColor_Active"];
        }
        #endregion

        #region ������ ������������� � ������� ���������� ���� ���������� �����������.
        /// <summary>
        /// ����� ������� ���������� ���� ���������� �����������.
        /// </summary>
        private void HideMessageBox()
        {
            // �������������� ������� ��������� ������.
            textBoxPanel.Width = Width - textBoxPanel.Location.X;

            // ������� ���������� ���� ���������� �����������.
            messageTextPanel.Visible = false;
            borderPanel.Visible = false;
        }

        /// <summary>
        /// ����� ��������� ���������� ���� ���������� �����������.
        /// </summary>
        private void AddMessageBox()
        {
            // ������� ������ ��� ���������� ����.
            messageTextPanel.Controls.Clear();
            // ���������� ������� ���� ��� �������������� ������.
            textBoxPanel.Width = (this.Width - textBoxPanel.Location.X) / 2 - 2;

            // ��������� ������� � ��������� ������� ����� ���������� ���������.
            borderPanel.Size = new Size(2, textBoxPanel.Height);
            borderPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width, textBoxPanel.Location.Y);
            // ��������� ������� � ��������� ������ ��� ���������� ����.
            messageTextPanel.Size = new Size(Width - (textBoxPanel.Location.X + textBoxPanel.Width + 2), textBoxPanel.Height);
            messageTextPanel.Location = new Point(textBoxPanel.Location.X + textBoxPanel.Width + 2, textBoxPanel.Location.Y);
            // ��������� �������.
            borderPanel.Visible = true;
            messageTextPanel.Visible = true;

            // ������������� ���������� ���� ���������� �����������.
            activeMessageTextBox = new RichTextBox();
            activeMessageTextBox.Dock = DockStyle.Fill;
            activeMessageTextBox.BorderStyle = BorderStyle.None;
            activeMessageTextBox.ReadOnly = true;
            activeMessageTextBox.Font = new Font("Consolas", activeCodeTextBox.Font.Size);
            activeMessageTextBox.BackColor = activeCodeTextBox.BackColor;
            activeMessageTextBox.ForeColor = activeCodeTextBox.ForeColor;

            // ���������� ���������� ���� ���������� �����������.
            messageTextPanel.Controls.Add(activeMessageTextBox);
            messageTextBoxShown = true;
        }
        #endregion

        #region ������ ���������� � ���������� ����.
        /// <summary>
        /// ����� ���������� ���������.
        /// </summary>
        /// <param name="filePath">���� � ������������ �����.</param>
        private void RunProgram(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                // ������������� �������� � ��� ����������.
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(filePath);
                process.StartInfo.UseShellExecute = false;
                // ������ ��������.
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                // ���������� �� ����������.
                activeMessageTextBox.Text = ex.Message;
            }
        }

        /// <summary>
        /// ����� �������� �����.
        /// </summary>
        /// <param name="filePath">���� � �����.</param>
        private void DeleteFile(string filePath)
        {
            // �������� �����, ���� �� ����������.
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// ����� ���������� ���� �� �����.
        /// </summary>
        /// <param name="filePath">���� � �����.</param>
        private void CompileFromText(string filePath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // ������������� �������� � ��� ����������.
                Process compiler = new Process();
                compiler.StartInfo = new ProcessStartInfo(@"cmd.exe");
                compiler.StartInfo.RedirectStandardInput = true;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.UseShellExecute = false;
                compiler.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                // ������ ��������.
                compiler.Start();

                // ���� ������� � ��������� ������ ��� ������� � ����������� � ������� ����������.
                using (StreamWriter sr = compiler.StandardInput)
                {
                    compiler.StandardInput.WriteLine($@"{compilerPath} {filePath}");
                    sr.Close();
                }
                // ���������� ����� �� ��������� ������.
                string[] outputLines = compiler.StandardOutput.ReadToEnd().Split("\n");
                List<string> compilerOutput = new List<string>();

                // ������� ������������� ������ �� ������ ��������� ������.
                for (int i = 4; i < outputLines.Length - 2; i++) compilerOutput.Add(outputLines[i]);

                // ����� ���������� �����������.
                activeMessageTextBox.Text = string.Join('\n', compilerOutput.ToArray());
                // �������� ��������.
                compiler.Close();
            }
            catch (Exception ex)
            {
                // ����� ��������� �� ����������.
                activeMessageTextBox.Text = ex.Message;
            }
        }
        #endregion
    }
}
