namespace CodeStats.Forms
{
    partial class SettingsForm
    {

        private System.Windows.Forms.Label lblAPIKey;
        private System.Windows.Forms.TextBox txtAPIKey;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.lblAPIKey = new System.Windows.Forms.Label();
            this.txtAPIKey = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtProxy = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkDebugMode = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkStats = new System.Windows.Forms.CheckBox();
            this.txtAPIURL = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkUseCustomMapping = new System.Windows.Forms.CheckBox();
            this.radioDetectionPriority_lexerLanguage = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.radioDetectionPriority_extensionMapping = new System.Windows.Forms.RadioButton();
            this.btnOpenCustomMappingFile = new System.Windows.Forms.Button();
            this.labelDetectionOrder = new System.Windows.Forms.Label();
            this.chkUseExtensionMapping = new System.Windows.Forms.CheckBox();
            this.chkUseLexerLanguage = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblAPIKey
            // 
            this.lblAPIKey.AutoSize = true;
            this.lblAPIKey.Location = new System.Drawing.Point(54, 16);
            this.lblAPIKey.Name = "lblAPIKey";
            this.lblAPIKey.Size = new System.Drawing.Size(61, 15);
            this.lblAPIKey.TabIndex = 0;
            this.lblAPIKey.Text = "API token:";
            // 
            // txtAPIKey
            // 
            this.txtAPIKey.Location = new System.Drawing.Point(124, 13);
            this.txtAPIKey.MaxLength = 255;
            this.txtAPIKey.Name = "txtAPIKey";
            this.txtAPIKey.Size = new System.Drawing.Size(371, 23);
            this.txtAPIKey.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(315, 402);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(87, 27);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(408, 402);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 27);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // txtProxy
            // 
            this.txtProxy.Location = new System.Drawing.Point(124, 87);
            this.txtProxy.MaxLength = 255;
            this.txtProxy.Name = "txtProxy";
            this.txtProxy.Size = new System.Drawing.Size(371, 23);
            this.txtProxy.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Proxy (optional):";
            // 
            // chkDebugMode
            // 
            this.chkDebugMode.AutoSize = true;
            this.chkDebugMode.Location = new System.Drawing.Point(124, 145);
            this.chkDebugMode.Name = "chkDebugMode";
            this.chkDebugMode.Size = new System.Drawing.Size(169, 19);
            this.chkDebugMode.TabIndex = 6;
            this.chkDebugMode.Text = "Enable debug level logging";
            this.chkDebugMode.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(121, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(232, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Example: https://user:password@host:port";
            // 
            // chkStats
            // 
            this.chkStats.AutoSize = true;
            this.chkStats.Checked = true;
            this.chkStats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStats.Location = new System.Drawing.Point(124, 171);
            this.chkStats.Name = "chkStats";
            this.chkStats.Size = new System.Drawing.Size(204, 19);
            this.chkStats.TabIndex = 7;
            this.chkStats.Text = "Anonymous usage stats reporting";
            this.chkStats.UseVisualStyleBackColor = true;
            // 
            // txtAPIURL
            // 
            this.txtAPIURL.Location = new System.Drawing.Point(124, 43);
            this.txtAPIURL.Name = "txtAPIURL";
            this.txtAPIURL.Size = new System.Drawing.Size(371, 23);
            this.txtAPIURL.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "API URL:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(121, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(292, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Only change API URL if you know what you are doing.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkUseCustomMapping);
            this.groupBox1.Controls.Add(this.radioDetectionPriority_lexerLanguage);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.radioDetectionPriority_extensionMapping);
            this.groupBox1.Controls.Add(this.btnOpenCustomMappingFile);
            this.groupBox1.Controls.Add(this.labelDetectionOrder);
            this.groupBox1.Controls.Add(this.chkUseExtensionMapping);
            this.groupBox1.Controls.Add(this.chkUseLexerLanguage);
            this.groupBox1.Location = new System.Drawing.Point(120, 200);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 183);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Language detection";
            // 
            // chkUseCustomMapping
            // 
            this.chkUseCustomMapping.AutoSize = true;
            this.chkUseCustomMapping.Location = new System.Drawing.Point(8, 89);
            this.chkUseCustomMapping.Name = "chkUseCustomMapping";
            this.chkUseCustomMapping.Size = new System.Drawing.Size(297, 19);
            this.chkUseCustomMapping.TabIndex = 9;
            this.chkUseCustomMapping.Text = "Override file extension mappings with custom ones";
            this.chkUseCustomMapping.UseVisualStyleBackColor = true;
            this.chkUseCustomMapping.CheckedChanged += new System.EventHandler(this.LanguageDetectionUIRefresh);
            // 
            // radioDetectionPriority_lexerLanguage
            // 
            this.radioDetectionPriority_lexerLanguage.AutoSize = true;
            this.radioDetectionPriority_lexerLanguage.Location = new System.Drawing.Point(240, 64);
            this.radioDetectionPriority_lexerLanguage.Name = "radioDetectionPriority_lexerLanguage";
            this.radioDetectionPriority_lexerLanguage.Size = new System.Drawing.Size(102, 19);
            this.radioDetectionPriority_lexerLanguage.TabIndex = 7;
            this.radioDetectionPriority_lexerLanguage.Text = "lexer language";
            this.radioDetectionPriority_lexerLanguage.UseVisualStyleBackColor = true;
            this.radioDetectionPriority_lexerLanguage.CheckedChanged += new System.EventHandler(this.LanguageDetectionUIRefresh);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 65);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 15);
            this.label6.TabIndex = 6;
            this.label6.Text = "Detection priority:";
            // 
            // radioDetectionPriority_extensionMapping
            // 
            this.radioDetectionPriority_extensionMapping.AutoSize = true;
            this.radioDetectionPriority_extensionMapping.Location = new System.Drawing.Point(112, 64);
            this.radioDetectionPriority_extensionMapping.Name = "radioDetectionPriority_extensionMapping";
            this.radioDetectionPriority_extensionMapping.Size = new System.Drawing.Size(127, 19);
            this.radioDetectionPriority_extensionMapping.TabIndex = 5;
            this.radioDetectionPriority_extensionMapping.Text = "extension mapping";
            this.radioDetectionPriority_extensionMapping.UseVisualStyleBackColor = true;
            this.radioDetectionPriority_extensionMapping.CheckedChanged += new System.EventHandler(this.LanguageDetectionUIRefresh);
            // 
            // btnOpenCustomMappingFile
            // 
            this.btnOpenCustomMappingFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenCustomMappingFile.Location = new System.Drawing.Point(311, 86);
            this.btnOpenCustomMappingFile.Name = "btnOpenCustomMappingFile";
            this.btnOpenCustomMappingFile.Size = new System.Drawing.Size(49, 23);
            this.btnOpenCustomMappingFile.TabIndex = 4;
            this.btnOpenCustomMappingFile.Text = "Open";
            this.btnOpenCustomMappingFile.UseVisualStyleBackColor = true;
            // 
            // labelDetectionOrder
            // 
            this.labelDetectionOrder.Location = new System.Drawing.Point(8, 121);
            this.labelDetectionOrder.Name = "labelDetectionOrder";
            this.labelDetectionOrder.Size = new System.Drawing.Size(352, 48);
            this.labelDetectionOrder.TabIndex = 3;
            this.labelDetectionOrder.Text = "Detection order: custom file extension mapping, file extension mapping, lexer lan" +
    "guage (custom language definitions are unsupported due to plugin interface limit" +
    "ations)";
            // 
            // chkUseExtensionMapping
            // 
            this.chkUseExtensionMapping.AutoSize = true;
            this.chkUseExtensionMapping.Location = new System.Drawing.Point(8, 16);
            this.chkUseExtensionMapping.Name = "chkUseExtensionMapping";
            this.chkUseExtensionMapping.Size = new System.Drawing.Size(231, 19);
            this.chkUseExtensionMapping.TabIndex = 2;
            this.chkUseExtensionMapping.Text = "Use Code::Stats file extension mapping";
            this.chkUseExtensionMapping.UseVisualStyleBackColor = true;
            this.chkUseExtensionMapping.CheckedChanged += new System.EventHandler(this.LanguageDetectionUIRefresh);
            // 
            // chkUseLexerLanguage
            // 
            this.chkUseLexerLanguage.AutoSize = true;
            this.chkUseLexerLanguage.Location = new System.Drawing.Point(8, 40);
            this.chkUseLexerLanguage.Name = "chkUseLexerLanguage";
            this.chkUseLexerLanguage.Size = new System.Drawing.Size(321, 19);
            this.chkUseLexerLanguage.TabIndex = 1;
            this.chkUseLexerLanguage.Text = "Use Notepad++ lexer language (top menu → Language)";
            this.chkUseLexerLanguage.UseVisualStyleBackColor = true;
            this.chkUseLexerLanguage.CheckedChanged += new System.EventHandler(this.LanguageDetectionUIRefresh);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(-303, 240);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(367, 19);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Include user-defined langs (Language → Define your language...)";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Visible = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(336, 160);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Show";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Visible = false;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "Custom local file extension mapping",
            "File extension mapping",
            "Lexer language (top menu → Language)"});
            this.checkedListBox1.Location = new System.Drawing.Point(-56, 136);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(120, 94);
            this.checkedListBox1.TabIndex = 13;
            this.checkedListBox1.Visible = false;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 441);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.checkedListBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtProxy);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAPIURL);
            this.Controls.Add(this.chkStats);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkDebugMode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtAPIKey);
            this.Controls.Add(this.lblAPIKey);
            this.Controls.Add(this.label4);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Code::Stats Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtProxy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkDebugMode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkStats;
        private System.Windows.Forms.TextBox txtAPIURL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnOpenCustomMappingFile;
        private System.Windows.Forms.Label labelDetectionOrder;
        private System.Windows.Forms.CheckBox chkUseExtensionMapping;
        private System.Windows.Forms.CheckBox chkUseLexerLanguage;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.CheckBox chkUseCustomMapping;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.RadioButton radioDetectionPriority_lexerLanguage;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton radioDetectionPriority_extensionMapping;
    }
}