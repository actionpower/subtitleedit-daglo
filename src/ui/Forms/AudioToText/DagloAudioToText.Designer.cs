namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    sealed partial class DagloAudioToText
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.groupBoxModels = new System.Windows.Forms.GroupBox();
            this.labelChooseLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguages = new Nikse.SubtitleEdit.Controls.NikseComboBox();
            this.linkLabelDagloWebSite = new System.Windows.Forms.LinkLabel();
            this.labelTime = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.buttonBatchMode = new System.Windows.Forms.Button();
            this.groupBoxInputFiles = new System.Windows.Forms.GroupBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonRemoveFile = new System.Windows.Forms.Button();
            this.buttonAddFile = new System.Windows.Forms.Button();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.labelFC = new System.Windows.Forms.Label();
            this.labelElapsed = new System.Windows.Forms.Label();
            this.contextMenuStripWhisperAdvanced = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.runOnlyPostProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparatorRunOnlyPostprocessing = new System.Windows.Forms.ToolStripSeparator();
            this.removeTemporaryFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloadNvidiaCudaForCPPCuBLASToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showWhisperlogtxtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBoxAutoAdjustTimings = new System.Windows.Forms.CheckBox();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.nikseTextBoxApiKey = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.textBoxLog = new Nikse.SubtitleEdit.Controls.NikseTextBox();
            this.groupBoxModels.SuspendLayout();
            this.groupBoxInputFiles.SuspendLayout();
            this.contextMenuStripWhisperAdvanced.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(1052, 300);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(158, 32);
            this.buttonCancel.TabIndex = 94;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonGenerate
            // 
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonGenerate.Location = new System.Drawing.Point(834, 300);
            this.buttonGenerate.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(208, 32);
            this.buttonGenerate.TabIndex = 90;
            this.buttonGenerate.Text = "&Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.ButtonGenerate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(20, 300);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(607, 17);
            this.progressBar1.TabIndex = 7;
            this.progressBar1.Visible = false;
            // 
            // labelProgress
            // 
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(20, 275);
            this.labelProgress.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(118, 18);
            this.labelProgress.TabIndex = 6;
            this.labelProgress.Text = "labelProgress";
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(20, 12);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(451, 18);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Generate text from audio via Daglo speech recognition";
            // 
            // groupBoxModels
            // 
            this.groupBoxModels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxModels.Controls.Add(this.labelChooseLanguage);
            this.groupBoxModels.Controls.Add(this.comboBoxLanguages);
            this.groupBoxModels.Location = new System.Drawing.Point(25, 83);
            this.groupBoxModels.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBoxModels.Name = "groupBoxModels";
            this.groupBoxModels.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBoxModels.Size = new System.Drawing.Size(1185, 114);
            this.groupBoxModels.TabIndex = 10;
            this.groupBoxModels.TabStop = false;
            this.groupBoxModels.Text = "Language and models";
            // 
            // labelChooseLanguage
            // 
            this.labelChooseLanguage.AutoSize = true;
            this.labelChooseLanguage.Location = new System.Drawing.Point(5, 29);
            this.labelChooseLanguage.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelChooseLanguage.Name = "labelChooseLanguage";
            this.labelChooseLanguage.Size = new System.Drawing.Size(151, 18);
            this.labelChooseLanguage.TabIndex = 4;
            this.labelChooseLanguage.Text = "Choose language";
            // 
            // comboBoxLanguages
            // 
            this.comboBoxLanguages.BackColor = System.Drawing.SystemColors.Window;
            this.comboBoxLanguages.BackColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.comboBoxLanguages.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(173)))), ((int)(((byte)(179)))));
            this.comboBoxLanguages.BorderColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(120)))), ((int)(((byte)(120)))));
            this.comboBoxLanguages.ButtonForeColor = System.Drawing.SystemColors.ControlText;
            this.comboBoxLanguages.ButtonForeColorDown = System.Drawing.Color.Orange;
            this.comboBoxLanguages.ButtonForeColorOver = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.comboBoxLanguages.DropDownHeight = 400;
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguages.DropDownWidth = 240;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Location = new System.Drawing.Point(10, 61);
            this.comboBoxLanguages.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.comboBoxLanguages.MaxLength = 32767;
            this.comboBoxLanguages.Name = "comboBoxLanguages";
            this.comboBoxLanguages.SelectedIndex = -1;
            this.comboBoxLanguages.SelectedItem = null;
            this.comboBoxLanguages.SelectedText = "";
            this.comboBoxLanguages.Size = new System.Drawing.Size(323, 33);
            this.comboBoxLanguages.TabIndex = 0;
            this.comboBoxLanguages.UsePopupWindow = true;
            this.comboBoxLanguages.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguages_SelectedIndexChanged);
            // 
            // linkLabelDagloWebSite
            // 
            this.linkLabelDagloWebSite.AutoSize = true;
            this.linkLabelDagloWebSite.Location = new System.Drawing.Point(20, 36);
            this.linkLabelDagloWebSite.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.linkLabelDagloWebSite.Name = "linkLabelDagloWebSite";
            this.linkLabelDagloWebSite.Size = new System.Drawing.Size(121, 18);
            this.linkLabelDagloWebSite.TabIndex = 0;
            this.linkLabelDagloWebSite.TabStop = true;
            this.linkLabelDagloWebSite.Text = "Daglo website";
            this.linkLabelDagloWebSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDagloWebsite_LinkClicked);
            // 
            // labelTime
            // 
            this.labelTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(20, 321);
            this.labelTime.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(147, 18);
            this.labelTime.TabIndex = 6;
            this.labelTime.Text = "Remaining time...";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // buttonBatchMode
            // 
            this.buttonBatchMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBatchMode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBatchMode.Location = new System.Drawing.Point(637, 300);
            this.buttonBatchMode.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonBatchMode.Name = "buttonBatchMode";
            this.buttonBatchMode.Size = new System.Drawing.Size(187, 32);
            this.buttonBatchMode.TabIndex = 92;
            this.buttonBatchMode.Text = "Batch mode";
            this.buttonBatchMode.UseVisualStyleBackColor = true;
            this.buttonBatchMode.Visible = false;
            this.buttonBatchMode.Click += new System.EventHandler(this.buttonBatchMode_Click);
            // 
            // groupBoxInputFiles
            // 
            this.groupBoxInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInputFiles.Controls.Add(this.buttonClear);
            this.groupBoxInputFiles.Controls.Add(this.buttonRemoveFile);
            this.groupBoxInputFiles.Controls.Add(this.buttonAddFile);
            this.groupBoxInputFiles.Controls.Add(this.listViewInputFiles);
            this.groupBoxInputFiles.Location = new System.Drawing.Point(25, 379);
            this.groupBoxInputFiles.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBoxInputFiles.Name = "groupBoxInputFiles";
            this.groupBoxInputFiles.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBoxInputFiles.Size = new System.Drawing.Size(1185, 0);
            this.groupBoxInputFiles.TabIndex = 30;
            this.groupBoxInputFiles.TabStop = false;
            this.groupBoxInputFiles.Text = "Input files";
            this.groupBoxInputFiles.Visible = false;
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(1052, 101);
            this.buttonClear.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(123, 32);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonRemoveFile
            // 
            this.buttonRemoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFile.Location = new System.Drawing.Point(1053, 64);
            this.buttonRemoveFile.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonRemoveFile.Name = "buttonRemoveFile";
            this.buttonRemoveFile.Size = new System.Drawing.Size(123, 32);
            this.buttonRemoveFile.TabIndex = 2;
            this.buttonRemoveFile.Text = "Remove";
            this.buttonRemoveFile.UseVisualStyleBackColor = true;
            this.buttonRemoveFile.Click += new System.EventHandler(this.buttonRemoveFile_Click);
            // 
            // buttonAddFile
            // 
            this.buttonAddFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddFile.Location = new System.Drawing.Point(1053, 26);
            this.buttonAddFile.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.buttonAddFile.Name = "buttonAddFile";
            this.buttonAddFile.Size = new System.Drawing.Size(122, 32);
            this.buttonAddFile.TabIndex = 1;
            this.buttonAddFile.Text = "Add...";
            this.buttonAddFile.UseVisualStyleBackColor = true;
            this.buttonAddFile.Click += new System.EventHandler(this.buttonAddFile_Click);
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFileName});
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(10, 25);
            this.listViewInputFiles.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(1031, 0);
            this.listViewInputFiles.TabIndex = 0;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            this.listViewInputFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewInputFiles_KeyDown);
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 455;
            // 
            // labelFC
            // 
            this.labelFC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelFC.ForeColor = System.Drawing.Color.Gray;
            this.labelFC.Location = new System.Drawing.Point(427, 321);
            this.labelFC.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelFC.Name = "labelFC";
            this.labelFC.Size = new System.Drawing.Size(200, 24);
            this.labelFC.TabIndex = 19;
            this.labelFC.Text = "labelFC";
            this.labelFC.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // labelElapsed
            // 
            this.labelElapsed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelElapsed.Location = new System.Drawing.Point(358, 274);
            this.labelElapsed.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelElapsed.Name = "labelElapsed";
            this.labelElapsed.Size = new System.Drawing.Size(253, 18);
            this.labelElapsed.TabIndex = 22;
            this.labelElapsed.Text = "labelElapsed";
            this.labelElapsed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelElapsed.Visible = false;
            // 
            // contextMenuStripWhisperAdvanced
            // 
            this.contextMenuStripWhisperAdvanced.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStripWhisperAdvanced.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runOnlyPostProcessingToolStripMenuItem,
            this.toolStripSeparatorRunOnlyPostprocessing,
            this.removeTemporaryFilesToolStripMenuItem,
            this.downloadNvidiaCudaForCPPCuBLASToolStripMenuItem,
            this.showWhisperlogtxtToolStripMenuItem});
            this.contextMenuStripWhisperAdvanced.Name = "contextMenuStripWhisperAdvanced";
            this.contextMenuStripWhisperAdvanced.Size = new System.Drawing.Size(407, 138);
            // 
            // runOnlyPostProcessingToolStripMenuItem
            // 
            this.runOnlyPostProcessingToolStripMenuItem.Name = "runOnlyPostProcessingToolStripMenuItem";
            this.runOnlyPostProcessingToolStripMenuItem.Size = new System.Drawing.Size(406, 32);
            this.runOnlyPostProcessingToolStripMenuItem.Text = "Run only post processing";
            this.runOnlyPostProcessingToolStripMenuItem.Click += new System.EventHandler(this.runOnlyPostProcessingToolStripMenuItem_Click);
            // 
            // toolStripSeparatorRunOnlyPostprocessing
            // 
            this.toolStripSeparatorRunOnlyPostprocessing.Name = "toolStripSeparatorRunOnlyPostprocessing";
            this.toolStripSeparatorRunOnlyPostprocessing.Size = new System.Drawing.Size(403, 6);
            // 
            // removeTemporaryFilesToolStripMenuItem
            // 
            this.removeTemporaryFilesToolStripMenuItem.Name = "removeTemporaryFilesToolStripMenuItem";
            this.removeTemporaryFilesToolStripMenuItem.Size = new System.Drawing.Size(406, 32);
            this.removeTemporaryFilesToolStripMenuItem.Text = "Remove temporary files";
            this.removeTemporaryFilesToolStripMenuItem.Click += new System.EventHandler(this.removeTemporaryFilesToolStripMenuItem_Click);
            // 
            // downloadNvidiaCudaForCPPCuBLASToolStripMenuItem
            // 
            this.downloadNvidiaCudaForCPPCuBLASToolStripMenuItem.Name = "downloadNvidiaCudaForCPPCuBLASToolStripMenuItem";
            this.downloadNvidiaCudaForCPPCuBLASToolStripMenuItem.Size = new System.Drawing.Size(406, 32);
            this.downloadNvidiaCudaForCPPCuBLASToolStripMenuItem.Text = "Download Nvidia cuda for Whisper CPP";
            // 
            // showWhisperlogtxtToolStripMenuItem
            // 
            this.showWhisperlogtxtToolStripMenuItem.Name = "showWhisperlogtxtToolStripMenuItem";
            this.showWhisperlogtxtToolStripMenuItem.Size = new System.Drawing.Size(406, 32);
            this.showWhisperlogtxtToolStripMenuItem.Text = "Show whisper_log.txt";
            this.showWhisperlogtxtToolStripMenuItem.Click += new System.EventHandler(this.ShowWhisperLogFileToolStripMenuItem_Click);
            // 
            // checkBoxAutoAdjustTimings
            // 
            this.checkBoxAutoAdjustTimings.AutoSize = true;
            this.checkBoxAutoAdjustTimings.Checked = true;
            this.checkBoxAutoAdjustTimings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoAdjustTimings.Location = new System.Drawing.Point(25, 214);
            this.checkBoxAutoAdjustTimings.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.checkBoxAutoAdjustTimings.Name = "checkBoxAutoAdjustTimings";
            this.checkBoxAutoAdjustTimings.Size = new System.Drawing.Size(189, 22);
            this.checkBoxAutoAdjustTimings.TabIndex = 21;
            this.checkBoxAutoAdjustTimings.Text = "Auto adjust timings";
            this.checkBoxAutoAdjustTimings.UseVisualStyleBackColor = true;
            this.checkBoxAutoAdjustTimings.Visible = false;
            // 
            // labelApiKey
            // 
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelApiKey.Location = new System.Drawing.Point(634, 20);
            this.labelApiKey.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(69, 18);
            this.labelApiKey.TabIndex = 96;
            this.labelApiKey.Text = "API key";
            // 
            // nikseTextBoxApiKey
            // 
            this.nikseTextBoxApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.nikseTextBoxApiKey.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.nikseTextBoxApiKey.Location = new System.Drawing.Point(711, 17);
            this.nikseTextBoxApiKey.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.nikseTextBoxApiKey.Name = "nikseTextBoxApiKey";
            this.nikseTextBoxApiKey.Size = new System.Drawing.Size(499, 28);
            this.nikseTextBoxApiKey.TabIndex = 95;
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.FocusedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.textBoxLog.Location = new System.Drawing.Point(775, 12);
            this.textBoxLog.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(326, 0);
            this.textBoxLog.TabIndex = 0;
            // 
            // DagloAudioToText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1230, 352);
            this.Controls.Add(this.labelApiKey);
            this.Controls.Add(this.nikseTextBoxApiKey);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.labelElapsed);
            this.Controls.Add(this.checkBoxAutoAdjustTimings);
            this.Controls.Add(this.groupBoxInputFiles);
            this.Controls.Add(this.buttonBatchMode);
            this.Controls.Add(this.linkLabelDagloWebSite);
            this.Controls.Add(this.groupBoxModels);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonGenerate);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.labelFC);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MinimumSize = new System.Drawing.Size(944, 325);
            this.Name = "DagloAudioToText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Audio to text";
            this.Activated += new System.EventHandler(this.DagloAudioToText_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioToText_FormClosing);
            this.Load += new System.EventHandler(this.AudioToText_Load);
            this.Shown += new System.EventHandler(this.AudioToText_Shown);
            this.ResizeEnd += new System.EventHandler(this.AudioToText_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AudioToText_KeyDown);
            this.groupBoxModels.ResumeLayout(false);
            this.groupBoxModels.PerformLayout();
            this.groupBoxInputFiles.ResumeLayout(false);
            this.contextMenuStripWhisperAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelProgress;
        private Nikse.SubtitleEdit.Controls.NikseTextBox textBoxLog;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxModels;
        private System.Windows.Forms.LinkLabel linkLabelDagloWebSite;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button buttonBatchMode;
        private System.Windows.Forms.GroupBox groupBoxInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonRemoveFile;
        private System.Windows.Forms.Button buttonAddFile;
        private System.Windows.Forms.Label labelFC;
        private System.Windows.Forms.Label labelChooseLanguage;
        private Nikse.SubtitleEdit.Controls.NikseComboBox comboBoxLanguages;
        private System.Windows.Forms.Label labelElapsed;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWhisperAdvanced;
        private System.Windows.Forms.ToolStripMenuItem removeTemporaryFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runOnlyPostProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorRunOnlyPostprocessing;
        private System.Windows.Forms.ToolStripMenuItem showWhisperlogtxtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloadNvidiaCudaForCPPCuBLASToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoAdjustTimings;
        private System.Windows.Forms.Label labelApiKey;
        private Controls.NikseTextBox nikseTextBoxApiKey;
    }
}
