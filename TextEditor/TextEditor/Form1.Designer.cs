
namespace TextEditor
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.codeFomattingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.themeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.themeComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.syntaxHighlightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keywordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stringsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.variablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.methodsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.classesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.autosaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.intervalComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.loggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loggingToolStripComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.compilerPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compilerPathToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.autosaveTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllTextContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.formatContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolPanel = new System.Windows.Forms.Panel();
            this.runToolBarButton = new System.Windows.Forms.PictureBox();
            this.buildToolBarButton = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.openToolStripButton = new System.Windows.Forms.PictureBox();
            this.separatorPanel = new System.Windows.Forms.Panel();
            this.strikeoutToolStripButton = new System.Windows.Forms.PictureBox();
            this.underlineToolStripButton = new System.Windows.Forms.PictureBox();
            this.boldToolStripButton = new System.Windows.Forms.PictureBox();
            this.italicToolStripButton = new System.Windows.Forms.PictureBox();
            this.fontToolStripButton = new System.Windows.Forms.PictureBox();
            this.pasteToolStripButton = new System.Windows.Forms.PictureBox();
            this.copyToolStripButton = new System.Windows.Forms.PictureBox();
            this.cutToolStripButton = new System.Windows.Forms.PictureBox();
            this.closeToolStripButton = new System.Windows.Forms.PictureBox();
            this.saveToolStripButton = new System.Windows.Forms.PictureBox();
            this.newToolStripButton = new System.Windows.Forms.PictureBox();
            this.tabPanel = new System.Windows.Forms.Panel();
            this.textBoxPanel = new System.Windows.Forms.Panel();
            this.reserveTextBox = new System.Windows.Forms.RichTextBox();
            this.borderPanel = new System.Windows.Forms.Panel();
            this.messageTextPanel = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusPanel = new System.Windows.Forms.Panel();
            this.autosavingProgressBar = new System.Windows.Forms.ProgressBar();
            this.autosavingStatusLabel = new System.Windows.Forms.Label();
            this.statusBorderPanel = new System.Windows.Forms.Panel();
            this.toolPanelBorderPanel = new System.Windows.Forms.Panel();
            this.statusStripEventTimer = new System.Windows.Forms.Timer(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.loggingTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.toolPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.runToolBarButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buildToolBarButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.openToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.strikeoutToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.underlineToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.boldToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.italicToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fontToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pasteToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.copyToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cutToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.closeToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.saveToolStripButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.newToolStripButton)).BeginInit();
            this.textBoxPanel.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.statusBorderPanel.SuspendLayout();
            this.toolPanelBorderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.formatToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(2058, 44);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.newWindowToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.saveAllToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(71, 36);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
            this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // newWindowToolStripMenuItem
            // 
            this.newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
            this.newWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.N)));
            this.newWindowToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.newWindowToolStripMenuItem.Text = "New in separate window";
            this.newWindowToolStripMenuItem.Click += new System.EventHandler(this.newWindowToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
            this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(567, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.saveAsToolStripMenuItem.Text = "Save &As";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAllToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.saveAllToolStripMenuItem.Text = "Save all documents";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(567, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.E)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(570, 44);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator3,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator4,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(74, 36);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.Z)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.redoToolStripMenuItem.Text = "&Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(356, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutContextMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyContextMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteContextMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(356, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(359, 44);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllTextContextMenuItem_Click);
            // 
            // formatToolStripMenuItem
            // 
            this.formatToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fontToolStripMenuItem,
            this.codeFomattingToolStripMenuItem});
            this.formatToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.formatToolStripMenuItem.Name = "formatToolStripMenuItem";
            this.formatToolStripMenuItem.Size = new System.Drawing.Size(109, 36);
            this.formatToolStripMenuItem.Text = "&Format";
            // 
            // fontToolStripMenuItem
            // 
            this.fontToolStripMenuItem.Name = "fontToolStripMenuItem";
            this.fontToolStripMenuItem.Size = new System.Drawing.Size(328, 44);
            this.fontToolStripMenuItem.Text = "&Font...";
            this.fontToolStripMenuItem.Click += new System.EventHandler(this.fontToolStripMenuItem_Click);
            // 
            // codeFomattingToolStripMenuItem
            // 
            this.codeFomattingToolStripMenuItem.Name = "codeFomattingToolStripMenuItem";
            this.codeFomattingToolStripMenuItem.Size = new System.Drawing.Size(328, 44);
            this.codeFomattingToolStripMenuItem.Text = "Autoformat code";
            this.codeFomattingToolStripMenuItem.Click += new System.EventHandler(this.codeFomattingToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.themeMenuItem,
            this.syntaxHighlightToolStripMenuItem,
            this.toolStripSeparator8,
            this.autosaveToolStripMenuItem,
            this.loggingToolStripMenuItem,
            this.compilerPathToolStripMenuItem});
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(120, 36);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // themeMenuItem
            // 
            this.themeMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.themeComboBox});
            this.themeMenuItem.Name = "themeMenuItem";
            this.themeMenuItem.Size = new System.Drawing.Size(404, 44);
            this.themeMenuItem.Text = "Theme...";
            // 
            // themeComboBox
            // 
            this.themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.themeComboBox.Items.AddRange(new object[] {
            "Light",
            "Dark"});
            this.themeComboBox.Name = "themeComboBox";
            this.themeComboBox.Size = new System.Drawing.Size(121, 37);
            this.themeComboBox.SelectedIndexChanged += new System.EventHandler(this.themeComboBox_SelectedIndexChanged);
            // 
            // syntaxHighlightToolStripMenuItem
            // 
            this.syntaxHighlightToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keywordsToolStripMenuItem,
            this.stringsToolStripMenuItem,
            this.variablesToolStripMenuItem,
            this.methodsToolStripMenuItem,
            this.classesToolStripMenuItem});
            this.syntaxHighlightToolStripMenuItem.Name = "syntaxHighlightToolStripMenuItem";
            this.syntaxHighlightToolStripMenuItem.Size = new System.Drawing.Size(404, 44);
            this.syntaxHighlightToolStripMenuItem.Text = "Syntax highlight colors...";
            // 
            // keywordsToolStripMenuItem
            // 
            this.keywordsToolStripMenuItem.Name = "keywordsToolStripMenuItem";
            this.keywordsToolStripMenuItem.Size = new System.Drawing.Size(324, 44);
            this.keywordsToolStripMenuItem.Text = "Keywords color...";
            this.keywordsToolStripMenuItem.Click += new System.EventHandler(this.keywordsToolStripMenuItem_Click);
            // 
            // stringsToolStripMenuItem
            // 
            this.stringsToolStripMenuItem.Name = "stringsToolStripMenuItem";
            this.stringsToolStripMenuItem.Size = new System.Drawing.Size(324, 44);
            this.stringsToolStripMenuItem.Text = "Strings color...";
            this.stringsToolStripMenuItem.Click += new System.EventHandler(this.stringsToolStripMenuItem_Click);
            // 
            // variablesToolStripMenuItem
            // 
            this.variablesToolStripMenuItem.Name = "variablesToolStripMenuItem";
            this.variablesToolStripMenuItem.Size = new System.Drawing.Size(324, 44);
            this.variablesToolStripMenuItem.Text = "Variables color...";
            this.variablesToolStripMenuItem.Click += new System.EventHandler(this.variablesToolStripMenuItem_Click);
            // 
            // methodsToolStripMenuItem
            // 
            this.methodsToolStripMenuItem.Name = "methodsToolStripMenuItem";
            this.methodsToolStripMenuItem.Size = new System.Drawing.Size(324, 44);
            this.methodsToolStripMenuItem.Text = "Methods color...";
            this.methodsToolStripMenuItem.Click += new System.EventHandler(this.methodsToolStripMenuItem_Click);
            // 
            // classesToolStripMenuItem
            // 
            this.classesToolStripMenuItem.Name = "classesToolStripMenuItem";
            this.classesToolStripMenuItem.Size = new System.Drawing.Size(324, 44);
            this.classesToolStripMenuItem.Text = "Classes color...";
            this.classesToolStripMenuItem.Click += new System.EventHandler(this.classesToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(401, 6);
            // 
            // autosaveToolStripMenuItem
            // 
            this.autosaveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.intervalComboBox});
            this.autosaveToolStripMenuItem.Name = "autosaveToolStripMenuItem";
            this.autosaveToolStripMenuItem.Size = new System.Drawing.Size(404, 44);
            this.autosaveToolStripMenuItem.Text = "Autosave interval";
            // 
            // intervalComboBox
            // 
            this.intervalComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.intervalComboBox.Items.AddRange(new object[] {
            "2 minutes",
            "5 minutes",
            "10 minutes"});
            this.intervalComboBox.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.intervalComboBox.Name = "intervalComboBox";
            this.intervalComboBox.Size = new System.Drawing.Size(121, 40);
            this.intervalComboBox.SelectedIndexChanged += new System.EventHandler(this.intervalComboBox_SelectedIndexChanged);
            // 
            // loggingToolStripMenuItem
            // 
            this.loggingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loggingToolStripComboBox});
            this.loggingToolStripMenuItem.Name = "loggingToolStripMenuItem";
            this.loggingToolStripMenuItem.Size = new System.Drawing.Size(404, 44);
            this.loggingToolStripMenuItem.Text = "Logging interval";
            // 
            // loggingToolStripComboBox
            // 
            this.loggingToolStripComboBox.Items.AddRange(new object[] {
            "2 minutes",
            "5 minutes",
            "10 minutes"});
            this.loggingToolStripComboBox.Name = "loggingToolStripComboBox";
            this.loggingToolStripComboBox.Size = new System.Drawing.Size(121, 40);
            this.loggingToolStripComboBox.SelectedIndexChanged += new System.EventHandler(this.loggingToolStripComboBox_SelectedIndexChanged);
            // 
            // compilerPathToolStripMenuItem
            // 
            this.compilerPathToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compilerPathToolStripTextBox});
            this.compilerPathToolStripMenuItem.Name = "compilerPathToolStripMenuItem";
            this.compilerPathToolStripMenuItem.Size = new System.Drawing.Size(404, 44);
            this.compilerPathToolStripMenuItem.Text = "Compiler path...";
            // 
            // compilerPathToolStripTextBox
            // 
            this.compilerPathToolStripTextBox.Name = "compilerPathToolStripTextBox";
            this.compilerPathToolStripTextBox.Size = new System.Drawing.Size(100, 39);
            this.compilerPathToolStripTextBox.TextChanged += new System.EventHandler(this.compilerPathToolStripTextBox_TextChanged);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(84, 36);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(227, 44);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semilight", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(618, 562);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(758, 72);
            this.label2.TabIndex = 6;
            this.label2.Text = "OPEN A FILE USING THE MENU OR TOOLBAR\r\nTO SEE INFORMATION USE \'ABOUT\' BUTTON IN T" +
    "HE \'HELP\' MENU";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "file+txt+icon-1320183613100464335.png");
            this.imageList1.Images.SetKeyName(1, "file+rtf+icon-1320183613100464335.png");
            this.imageList1.Images.SetKeyName(2, "file+icon-1320183613100464335.png");
            // 
            // autosaveTimer
            // 
            this.autosaveTimer.Interval = 120000;
            this.autosaveTimer.Tick += new System.EventHandler(this.autosaveTimer_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllTextContextMenuItem,
            this.cutContextMenuItem,
            this.copyContextMenuItem,
            this.pasteContextMenuItem,
            this.formatContextMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(231, 194);
            // 
            // selectAllTextContextMenuItem
            // 
            this.selectAllTextContextMenuItem.Name = "selectAllTextContextMenuItem";
            this.selectAllTextContextMenuItem.Size = new System.Drawing.Size(230, 38);
            this.selectAllTextContextMenuItem.Text = "Select all text";
            this.selectAllTextContextMenuItem.Click += new System.EventHandler(this.selectAllTextContextMenuItem_Click);
            // 
            // cutContextMenuItem
            // 
            this.cutContextMenuItem.Name = "cutContextMenuItem";
            this.cutContextMenuItem.Size = new System.Drawing.Size(230, 38);
            this.cutContextMenuItem.Text = "Cut";
            this.cutContextMenuItem.Click += new System.EventHandler(this.cutContextMenuItem_Click);
            // 
            // copyContextMenuItem
            // 
            this.copyContextMenuItem.Name = "copyContextMenuItem";
            this.copyContextMenuItem.Size = new System.Drawing.Size(230, 38);
            this.copyContextMenuItem.Text = "Copy";
            this.copyContextMenuItem.Click += new System.EventHandler(this.copyContextMenuItem_Click);
            // 
            // pasteContextMenuItem
            // 
            this.pasteContextMenuItem.Name = "pasteContextMenuItem";
            this.pasteContextMenuItem.Size = new System.Drawing.Size(230, 38);
            this.pasteContextMenuItem.Text = "Paste";
            this.pasteContextMenuItem.Click += new System.EventHandler(this.pasteContextMenuItem_Click);
            // 
            // formatContextMenuItem
            // 
            this.formatContextMenuItem.Name = "formatContextMenuItem";
            this.formatContextMenuItem.Size = new System.Drawing.Size(230, 38);
            this.formatContextMenuItem.Text = "Format (font)";
            this.formatContextMenuItem.Click += new System.EventHandler(this.formatContextMenuItem_Click);
            // 
            // toolPanel
            // 
            this.toolPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.toolPanel.Controls.Add(this.runToolBarButton);
            this.toolPanel.Controls.Add(this.buildToolBarButton);
            this.toolPanel.Controls.Add(this.panel1);
            this.toolPanel.Controls.Add(this.openToolStripButton);
            this.toolPanel.Controls.Add(this.separatorPanel);
            this.toolPanel.Controls.Add(this.strikeoutToolStripButton);
            this.toolPanel.Controls.Add(this.underlineToolStripButton);
            this.toolPanel.Controls.Add(this.boldToolStripButton);
            this.toolPanel.Controls.Add(this.italicToolStripButton);
            this.toolPanel.Controls.Add(this.fontToolStripButton);
            this.toolPanel.Controls.Add(this.pasteToolStripButton);
            this.toolPanel.Controls.Add(this.copyToolStripButton);
            this.toolPanel.Controls.Add(this.cutToolStripButton);
            this.toolPanel.Controls.Add(this.closeToolStripButton);
            this.toolPanel.Controls.Add(this.saveToolStripButton);
            this.toolPanel.Controls.Add(this.newToolStripButton);
            this.toolPanel.Location = new System.Drawing.Point(0, 0);
            this.toolPanel.Margin = new System.Windows.Forms.Padding(6);
            this.toolPanel.Name = "toolPanel";
            this.toolPanel.Size = new System.Drawing.Size(62, 1062);
            this.toolPanel.TabIndex = 11;
            // 
            // runToolBarButton
            // 
            this.runToolBarButton.BackColor = System.Drawing.Color.Transparent;
            this.runToolBarButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.runToolBarButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.runToolBarButton.Image = ((System.Drawing.Image)(resources.GetObject("runToolBarButton.Image")));
            this.runToolBarButton.Location = new System.Drawing.Point(10, 724);
            this.runToolBarButton.Margin = new System.Windows.Forms.Padding(6);
            this.runToolBarButton.Name = "runToolBarButton";
            this.runToolBarButton.Size = new System.Drawing.Size(40, 40);
            this.runToolBarButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.runToolBarButton.TabIndex = 15;
            this.runToolBarButton.TabStop = false;
            this.toolTip1.SetToolTip(this.runToolBarButton, "Run");
            this.runToolBarButton.Click += new System.EventHandler(this.RunClick);
            // 
            // buildToolBarButton
            // 
            this.buildToolBarButton.BackColor = System.Drawing.Color.Transparent;
            this.buildToolBarButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.buildToolBarButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buildToolBarButton.Image = ((System.Drawing.Image)(resources.GetObject("buildToolBarButton.Image")));
            this.buildToolBarButton.Location = new System.Drawing.Point(10, 646);
            this.buildToolBarButton.Margin = new System.Windows.Forms.Padding(6);
            this.buildToolBarButton.Name = "buildToolBarButton";
            this.buildToolBarButton.Size = new System.Drawing.Size(40, 40);
            this.buildToolBarButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.buildToolBarButton.TabIndex = 14;
            this.buildToolBarButton.TabStop = false;
            this.toolTip1.SetToolTip(this.buildToolBarButton, "Build");
            this.buildToolBarButton.Click += new System.EventHandler(this.BuildClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.panel1.Location = new System.Drawing.Point(8, 598);
            this.panel1.Margin = new System.Windows.Forms.Padding(6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(44, 2);
            this.panel1.TabIndex = 13;
            // 
            // openToolStripButton
            // 
            this.openToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.openToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.openToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.openToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripButton.Image")));
            this.openToolStripButton.Location = new System.Drawing.Point(10, 88);
            this.openToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.openToolStripButton.Name = "openToolStripButton";
            this.openToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.openToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.openToolStripButton.TabIndex = 1;
            this.openToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.openToolStripButton, "Open file");
            this.openToolStripButton.Click += new System.EventHandler(this.openToolStripButton_Click);
            // 
            // separatorPanel
            // 
            this.separatorPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.separatorPanel.Location = new System.Drawing.Point(8, 308);
            this.separatorPanel.Margin = new System.Windows.Forms.Padding(6);
            this.separatorPanel.Name = "separatorPanel";
            this.separatorPanel.Size = new System.Drawing.Size(44, 2);
            this.separatorPanel.TabIndex = 12;
            // 
            // strikeoutToolStripButton
            // 
            this.strikeoutToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.strikeoutToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.strikeoutToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.strikeoutToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("strikeoutToolStripButton.Image")));
            this.strikeoutToolStripButton.Location = new System.Drawing.Point(10, 964);
            this.strikeoutToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.strikeoutToolStripButton.Name = "strikeoutToolStripButton";
            this.strikeoutToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.strikeoutToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.strikeoutToolStripButton.TabIndex = 11;
            this.strikeoutToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.strikeoutToolStripButton, "Strikeout");
            this.strikeoutToolStripButton.Click += new System.EventHandler(this.strikeoutToolStripButton_Click);
            // 
            // underlineToolStripButton
            // 
            this.underlineToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.underlineToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.underlineToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.underlineToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("underlineToolStripButton.Image")));
            this.underlineToolStripButton.Location = new System.Drawing.Point(10, 886);
            this.underlineToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.underlineToolStripButton.Name = "underlineToolStripButton";
            this.underlineToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.underlineToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.underlineToolStripButton.TabIndex = 10;
            this.underlineToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.underlineToolStripButton, "Underline");
            this.underlineToolStripButton.Click += new System.EventHandler(this.underlineToolStripButton_Click);
            // 
            // boldToolStripButton
            // 
            this.boldToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.boldToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.boldToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.boldToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("boldToolStripButton.Image")));
            this.boldToolStripButton.Location = new System.Drawing.Point(10, 808);
            this.boldToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.boldToolStripButton.Name = "boldToolStripButton";
            this.boldToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.boldToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.boldToolStripButton.TabIndex = 9;
            this.boldToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.boldToolStripButton, "Bold");
            this.boldToolStripButton.Click += new System.EventHandler(this.boldToolStripButton_Click);
            // 
            // italicToolStripButton
            // 
            this.italicToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.italicToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.italicToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.italicToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("italicToolStripButton.Image")));
            this.italicToolStripButton.Location = new System.Drawing.Point(10, 724);
            this.italicToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.italicToolStripButton.Name = "italicToolStripButton";
            this.italicToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.italicToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.italicToolStripButton.TabIndex = 8;
            this.italicToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.italicToolStripButton, "Italic");
            this.italicToolStripButton.Click += new System.EventHandler(this.italicToolStripButton_Click);
            // 
            // fontToolStripButton
            // 
            this.fontToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.fontToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.fontToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.fontToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("fontToolStripButton.Image")));
            this.fontToolStripButton.Location = new System.Drawing.Point(10, 646);
            this.fontToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.fontToolStripButton.Name = "fontToolStripButton";
            this.fontToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.fontToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.fontToolStripButton.TabIndex = 7;
            this.fontToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.fontToolStripButton, "Font");
            this.fontToolStripButton.Click += new System.EventHandler(this.fontToolStripButton_Click);
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.pasteToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pasteToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pasteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripButton.Image")));
            this.pasteToolStripButton.Location = new System.Drawing.Point(10, 512);
            this.pasteToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.pasteToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pasteToolStripButton.TabIndex = 6;
            this.pasteToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.pasteToolStripButton, "Paste");
            this.pasteToolStripButton.Click += new System.EventHandler(this.pasteContextMenuItem_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.copyToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.copyToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.copyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripButton.Image")));
            this.copyToolStripButton.Location = new System.Drawing.Point(10, 430);
            this.copyToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.copyToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.copyToolStripButton.TabIndex = 5;
            this.copyToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.copyToolStripButton, "Copy");
            this.copyToolStripButton.Click += new System.EventHandler(this.copyContextMenuItem_Click);
            // 
            // cutToolStripButton
            // 
            this.cutToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.cutToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.cutToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cutToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripButton.Image")));
            this.cutToolStripButton.Location = new System.Drawing.Point(10, 348);
            this.cutToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.cutToolStripButton.Name = "cutToolStripButton";
            this.cutToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.cutToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.cutToolStripButton.TabIndex = 4;
            this.cutToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.cutToolStripButton, "Cut");
            this.cutToolStripButton.Click += new System.EventHandler(this.cutContextMenuItem_Click);
            // 
            // closeToolStripButton
            // 
            this.closeToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.closeToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.closeToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.closeToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("closeToolStripButton.Image")));
            this.closeToolStripButton.Location = new System.Drawing.Point(10, 242);
            this.closeToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.closeToolStripButton.Name = "closeToolStripButton";
            this.closeToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.closeToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.closeToolStripButton.TabIndex = 3;
            this.closeToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.closeToolStripButton, "Close current tab");
            this.closeToolStripButton.Click += new System.EventHandler(this.closeToolStripButton_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.saveToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.saveToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripButton.Image")));
            this.saveToolStripButton.Location = new System.Drawing.Point(10, 162);
            this.saveToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.saveToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.saveToolStripButton.TabIndex = 2;
            this.saveToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.saveToolStripButton, "Save");
            this.saveToolStripButton.Click += new System.EventHandler(this.saveToolStripButton_Click);
            // 
            // newToolStripButton
            // 
            this.newToolStripButton.BackColor = System.Drawing.Color.Transparent;
            this.newToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.newToolStripButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.newToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripButton.Image")));
            this.newToolStripButton.Location = new System.Drawing.Point(10, 16);
            this.newToolStripButton.Margin = new System.Windows.Forms.Padding(6);
            this.newToolStripButton.Name = "newToolStripButton";
            this.newToolStripButton.Size = new System.Drawing.Size(40, 40);
            this.newToolStripButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.newToolStripButton.TabIndex = 0;
            this.newToolStripButton.TabStop = false;
            this.toolTip1.SetToolTip(this.newToolStripButton, "New file");
            this.newToolStripButton.Click += new System.EventHandler(this.newToolStripButton_Click);
            // 
            // tabPanel
            // 
            this.tabPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabPanel.Location = new System.Drawing.Point(64, 48);
            this.tabPanel.Margin = new System.Windows.Forms.Padding(6);
            this.tabPanel.Name = "tabPanel";
            this.tabPanel.Size = new System.Drawing.Size(1996, 68);
            this.tabPanel.TabIndex = 12;
            this.tabPanel.Visible = false;
            // 
            // textBoxPanel
            // 
            this.textBoxPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPanel.Controls.Add(this.reserveTextBox);
            this.textBoxPanel.Location = new System.Drawing.Point(64, 116);
            this.textBoxPanel.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxPanel.Name = "textBoxPanel";
            this.textBoxPanel.Size = new System.Drawing.Size(1996, 994);
            this.textBoxPanel.TabIndex = 13;
            this.textBoxPanel.Visible = false;
            // 
            // reserveTextBox
            // 
            this.reserveTextBox.Location = new System.Drawing.Point(1976, 976);
            this.reserveTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.reserveTextBox.Name = "reserveTextBox";
            this.reserveTextBox.Size = new System.Drawing.Size(0, 0);
            this.reserveTextBox.TabIndex = 0;
            this.reserveTextBox.Text = "";
            this.reserveTextBox.Visible = false;
            // 
            // borderPanel
            // 
            this.borderPanel.Location = new System.Drawing.Point(1948, 560);
            this.borderPanel.Margin = new System.Windows.Forms.Padding(6);
            this.borderPanel.Name = "borderPanel";
            this.borderPanel.Size = new System.Drawing.Size(0, 0);
            this.borderPanel.TabIndex = 1;
            this.borderPanel.Visible = false;
            // 
            // messageTextPanel
            // 
            this.messageTextPanel.Location = new System.Drawing.Point(1920, 356);
            this.messageTextPanel.Margin = new System.Windows.Forms.Padding(6);
            this.messageTextPanel.Name = "messageTextPanel";
            this.messageTextPanel.Size = new System.Drawing.Size(0, 0);
            this.messageTextPanel.TabIndex = 2;
            this.messageTextPanel.Visible = false;
            // 
            // statusPanel
            // 
            this.statusPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusPanel.Controls.Add(this.autosavingProgressBar);
            this.statusPanel.Controls.Add(this.autosavingStatusLabel);
            this.statusPanel.Location = new System.Drawing.Point(0, 2);
            this.statusPanel.Margin = new System.Windows.Forms.Padding(6);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Size = new System.Drawing.Size(2058, 48);
            this.statusPanel.TabIndex = 14;
            // 
            // autosavingProgressBar
            // 
            this.autosavingProgressBar.Location = new System.Drawing.Point(180, 18);
            this.autosavingProgressBar.Margin = new System.Windows.Forms.Padding(6);
            this.autosavingProgressBar.Name = "autosavingProgressBar";
            this.autosavingProgressBar.Size = new System.Drawing.Size(148, 20);
            this.autosavingProgressBar.TabIndex = 1;
            this.autosavingProgressBar.Visible = false;
            // 
            // autosavingStatusLabel
            // 
            this.autosavingStatusLabel.AutoSize = true;
            this.autosavingStatusLabel.Location = new System.Drawing.Point(16, 10);
            this.autosavingStatusLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.autosavingStatusLabel.Name = "autosavingStatusLabel";
            this.autosavingStatusLabel.Size = new System.Drawing.Size(148, 32);
            this.autosavingStatusLabel.TabIndex = 0;
            this.autosavingStatusLabel.Text = "Autosaving...";
            this.autosavingStatusLabel.Visible = false;
            // 
            // statusBorderPanel
            // 
            this.statusBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusBorderPanel.Controls.Add(this.statusPanel);
            this.statusBorderPanel.Location = new System.Drawing.Point(0, 1110);
            this.statusBorderPanel.Margin = new System.Windows.Forms.Padding(6);
            this.statusBorderPanel.Name = "statusBorderPanel";
            this.statusBorderPanel.Size = new System.Drawing.Size(2058, 50);
            this.statusBorderPanel.TabIndex = 15;
            // 
            // toolPanelBorderPanel
            // 
            this.toolPanelBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.toolPanelBorderPanel.Controls.Add(this.toolPanel);
            this.toolPanelBorderPanel.Location = new System.Drawing.Point(0, 48);
            this.toolPanelBorderPanel.Margin = new System.Windows.Forms.Padding(6);
            this.toolPanelBorderPanel.Name = "toolPanelBorderPanel";
            this.toolPanelBorderPanel.Size = new System.Drawing.Size(64, 1062);
            this.toolPanelBorderPanel.TabIndex = 16;
            // 
            // statusStripEventTimer
            // 
            this.statusStripEventTimer.Interval = 3000;
            this.statusStripEventTimer.Tick += new System.EventHandler(this.statusStripEventTimer_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(740, 440);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(576, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 17;
            this.pictureBox1.TabStop = false;
            // 
            // loggingTimer
            // 
            this.loggingTimer.Enabled = true;
            this.loggingTimer.Tick += new System.EventHandler(this.loggingTimer_Tick);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(2058, 1160);
            this.Controls.Add(this.borderPanel);
            this.Controls.Add(this.textBoxPanel);
            this.Controls.Add(this.messageTextPanel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.toolPanelBorderPanel);
            this.Controls.Add(this.statusBorderPanel);
            this.Controls.Add(this.tabPanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Notepad+";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.runToolBarButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buildToolBarButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.openToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.strikeoutToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.underlineToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.boldToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.italicToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fontToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pasteToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.copyToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cutToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.closeToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.saveToolStripButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.newToolStripButton)).EndInit();
            this.textBoxPanel.ResumeLayout(false);
            this.statusPanel.ResumeLayout(false);
            this.statusPanel.PerformLayout();
            this.statusBorderPanel.ResumeLayout(false);
            this.toolPanelBorderPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem рщелуныToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem themeMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem autosaveToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox intervalComboBox;
        private System.Windows.Forms.Timer autosaveTimer;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectAllTextContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteContextMenuItem;
        private System.Windows.Forms.ToolStripMenuItem formatContextMenuItem;
        private System.Windows.Forms.Panel toolPanel;
        private System.Windows.Forms.PictureBox newToolStripButton;
        private System.Windows.Forms.PictureBox saveToolStripButton;
        private System.Windows.Forms.PictureBox openToolStripButton;
        private System.Windows.Forms.Panel separatorPanel;
        private System.Windows.Forms.PictureBox strikeoutToolStripButton;
        private System.Windows.Forms.PictureBox underlineToolStripButton;
        private System.Windows.Forms.PictureBox boldToolStripButton;
        private System.Windows.Forms.PictureBox italicToolStripButton;
        private System.Windows.Forms.PictureBox fontToolStripButton;
        private System.Windows.Forms.PictureBox pasteToolStripButton;
        private System.Windows.Forms.PictureBox copyToolStripButton;
        private System.Windows.Forms.PictureBox cutToolStripButton;
        private System.Windows.Forms.PictureBox closeToolStripButton;
        private System.Windows.Forms.Panel tabPanel;
        private System.Windows.Forms.Panel textBoxPanel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel statusPanel;
        private System.Windows.Forms.Panel statusBorderPanel;
        private System.Windows.Forms.Panel toolPanelBorderPanel;
        private System.Windows.Forms.Label autosavingStatusLabel;
        private System.Windows.Forms.ProgressBar autosavingProgressBar;
        private System.Windows.Forms.ToolStripMenuItem newWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.Timer statusStripEventTimer;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripMenuItem loggingToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox loggingToolStripComboBox;
        private System.Windows.Forms.Timer loggingTimer;
        private System.Windows.Forms.RichTextBox reserveTextBox;
        private System.Windows.Forms.PictureBox buildToolBarButton;
        private System.Windows.Forms.PictureBox runToolBarButton;
        private System.Windows.Forms.Panel messageTextPanel;
        private System.Windows.Forms.Panel borderPanel;
        private System.Windows.Forms.ToolStripMenuItem syntaxHighlightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keywordsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        public System.Windows.Forms.ToolStripComboBox themeComboBox;
        private System.Windows.Forms.ToolStripMenuItem stringsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem variablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem methodsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem classesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compilerPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox compilerPathToolStripTextBox;
        private System.Windows.Forms.ToolStripMenuItem codeFomattingToolStripMenuItem;
    }
}

