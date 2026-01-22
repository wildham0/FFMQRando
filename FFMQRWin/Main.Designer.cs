
namespace FFMQRWin
{
	partial class Main
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
			openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			seedButton = new System.Windows.Forms.Button();
			textBox1 = new System.Windows.Forms.TextBox();
			seedBox = new System.Windows.Forms.TextBox();
			flagstringBox = new System.Windows.Forms.TextBox();
			generateButton = new System.Windows.Forms.Button();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			comboPresets = new System.Windows.Forms.ComboBox();
			menuStrip1 = new System.Windows.Forms.MenuStrip();
			toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			selectROMFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			loadAPMQFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			configToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			flagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			archipelagoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			devToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			scriptingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			versionComparerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			jsonExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			filterBox = new System.Windows.Forms.TextBox();
			filterLabel = new System.Windows.Forms.Label();
			contentPanel = new System.Windows.Forms.Panel();
			statusStrip1 = new System.Windows.Forms.StatusStrip();
			messageStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
			menuStrip1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// openFileDialog1
			// 
			openFileDialog1.FileName = "openFileDialog1";
			// 
			// seedButton
			// 
			seedButton.Location = new System.Drawing.Point(443, 74);
			seedButton.Name = "seedButton";
			seedButton.Size = new System.Drawing.Size(74, 23);
			seedButton.TabIndex = 1;
			seedButton.Text = "Roll Seed";
			seedButton.UseVisualStyleBackColor = true;
			seedButton.Click += rollSeed_Click;
			// 
			// textBox1
			// 
			textBox1.Location = new System.Drawing.Point(73, 36);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(350, 23);
			textBox1.TabIndex = 2;
			// 
			// seedBox
			// 
			seedBox.Location = new System.Drawing.Point(73, 74);
			seedBox.Name = "seedBox";
			seedBox.Size = new System.Drawing.Size(350, 23);
			seedBox.TabIndex = 3;
			seedBox.TextChanged += seedBox_TextChanged;
			// 
			// flagstringBox
			// 
			flagstringBox.Location = new System.Drawing.Point(73, 112);
			flagstringBox.Name = "flagstringBox";
			flagstringBox.Size = new System.Drawing.Size(350, 23);
			flagstringBox.TabIndex = 4;
			flagstringBox.TextChanged += flagstringBox_TextChanged;
			// 
			// generateButton
			// 
			generateButton.Location = new System.Drawing.Point(549, 36);
			generateButton.Name = "generateButton";
			generateButton.Size = new System.Drawing.Size(239, 61);
			generateButton.TabIndex = 6;
			generateButton.Text = "Generate";
			generateButton.UseVisualStyleBackColor = true;
			generateButton.Click += generate_Click;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 40);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(55, 15);
			label1.TabIndex = 10;
			label1.Text = "ROM File";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(12, 78);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(32, 15);
			label2.TabIndex = 11;
			label2.Text = "Seed";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 116);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(59, 15);
			label3.TabIndex = 12;
			label3.Text = "Flagstring";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(12, 152);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(0, 15);
			label5.TabIndex = 14;
			// 
			// comboPresets
			// 
			comboPresets.FormattingEnabled = true;
			comboPresets.Location = new System.Drawing.Point(443, 113);
			comboPresets.Name = "comboPresets";
			comboPresets.Size = new System.Drawing.Size(350, 23);
			comboPresets.TabIndex = 27;
			// 
			// menuStrip1
			// 
			menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, configToolStripMenuItem, devToolsToolStripMenuItem, helpToolStripMenuItem });
			menuStrip1.Location = new System.Drawing.Point(0, 0);
			menuStrip1.Name = "menuStrip1";
			menuStrip1.Size = new System.Drawing.Size(800, 24);
			menuStrip1.TabIndex = 30;
			menuStrip1.Text = "menuStrip1";
			// 
			// toolStripMenuItem1
			// 
			toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { selectROMFileToolStripMenuItem, loadAPMQFileToolStripMenuItem, toolStripSeparator1, preferencesToolStripMenuItem, toolStripSeparator2, exitToolStripMenuItem });
			toolStripMenuItem1.Name = "toolStripMenuItem1";
			toolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
			toolStripMenuItem1.Text = "File";
			// 
			// selectROMFileToolStripMenuItem
			// 
			selectROMFileToolStripMenuItem.Name = "selectROMFileToolStripMenuItem";
			selectROMFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			selectROMFileToolStripMenuItem.Text = "Select ROM File...";
			selectROMFileToolStripMenuItem.Click += selectROMFileToolStripMenuItem_Click;
			// 
			// loadAPMQFileToolStripMenuItem
			// 
			loadAPMQFileToolStripMenuItem.Name = "loadAPMQFileToolStripMenuItem";
			loadAPMQFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			loadAPMQFileToolStripMenuItem.Text = "Load APMQ File...";
			loadAPMQFileToolStripMenuItem.Click += loadAPMQFileToolStripMenuItem_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
			// 
			// preferencesToolStripMenuItem
			// 
			preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
			preferencesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			preferencesToolStripMenuItem.Text = "Preferences";
			preferencesToolStripMenuItem.Click += preferencesToolStripMenuItem_Click_1;
			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";
			toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
			exitToolStripMenuItem.Text = "Exit";
			exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
			// 
			// configToolStripMenuItem
			// 
			configToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { flagsToolStripMenuItem, archipelagoToolStripMenuItem });
			configToolStripMenuItem.Name = "configToolStripMenuItem";
			configToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
			configToolStripMenuItem.Text = "Mode";
			// 
			// flagsToolStripMenuItem
			// 
			flagsToolStripMenuItem.Name = "flagsToolStripMenuItem";
			flagsToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			flagsToolStripMenuItem.Text = "Randomizer";
			flagsToolStripMenuItem.Click += flagsToolStripMenuItem_Click;
			// 
			// archipelagoToolStripMenuItem
			// 
			archipelagoToolStripMenuItem.Name = "archipelagoToolStripMenuItem";
			archipelagoToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
			archipelagoToolStripMenuItem.Text = "Archipelago";
			archipelagoToolStripMenuItem.Click += archipelagoToolStripMenuItem_Click;
			// 
			// devToolsToolStripMenuItem
			// 
			devToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { scriptingToolStripMenuItem, versionComparerToolStripMenuItem, jsonExportToolStripMenuItem });
			devToolsToolStripMenuItem.Name = "devToolsToolStripMenuItem";
			devToolsToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
			devToolsToolStripMenuItem.Text = "Dev Tools";
			// 
			// scriptingToolStripMenuItem
			// 
			scriptingToolStripMenuItem.Name = "scriptingToolStripMenuItem";
			scriptingToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			scriptingToolStripMenuItem.Text = "Scripting";
			scriptingToolStripMenuItem.Click += scriptingToolStripMenuItem_Click;
			// 
			// versionComparerToolStripMenuItem
			// 
			versionComparerToolStripMenuItem.Name = "versionComparerToolStripMenuItem";
			versionComparerToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			versionComparerToolStripMenuItem.Text = "Version Comparer";
			versionComparerToolStripMenuItem.Click += versionComparerToolStripMenuItem_Click;
			// 
			// jsonExportToolStripMenuItem
			// 
			jsonExportToolStripMenuItem.Name = "jsonExportToolStripMenuItem";
			jsonExportToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
			jsonExportToolStripMenuItem.Text = "Json Export";
			jsonExportToolStripMenuItem.Click += jsonExportToolStripMenuItem_Click;
			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutToolStripMenuItem });
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
			helpToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
			aboutToolStripMenuItem.Text = "About";
			aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
			// 
			// filterBox
			// 
			filterBox.Location = new System.Drawing.Point(77, 200);
			filterBox.Name = "filterBox";
			filterBox.Size = new System.Drawing.Size(350, 23);
			filterBox.TabIndex = 31;
			filterBox.TextChanged += flagFilterBox_TextChanged;
			// 
			// filterLabel
			// 
			filterLabel.AutoSize = true;
			filterLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			filterLabel.Location = new System.Drawing.Point(13, 203);
			filterLabel.Name = "filterLabel";
			filterLabel.Size = new System.Drawing.Size(58, 15);
			filterLabel.TabIndex = 32;
			filterLabel.Text = "Flag Filter";
			// 
			// contentPanel
			// 
			contentPanel.AutoScroll = true;
			contentPanel.Location = new System.Drawing.Point(12, 232);
			contentPanel.Name = "contentPanel";
			contentPanel.Size = new System.Drawing.Size(781, 247);
			contentPanel.TabIndex = 33;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { messageStripLabel });
			statusStrip1.Location = new System.Drawing.Point(0, 469);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new System.Drawing.Size(800, 22);
			statusStrip1.TabIndex = 34;
			statusStrip1.Text = "statusStrip1";
			// 
			// messageStripLabel
			// 
			messageStripLabel.Name = "messageStripLabel";
			messageStripLabel.Size = new System.Drawing.Size(105, 17);
			messageStripLabel.Text = "messageStripLabel";
			// 
			// Main
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 491);
			Controls.Add(statusStrip1);
			Controls.Add(filterBox);
			Controls.Add(filterLabel);
			Controls.Add(contentPanel);
			Controls.Add(comboPresets);
			Controls.Add(label5);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(generateButton);
			Controls.Add(flagstringBox);
			Controls.Add(seedBox);
			Controls.Add(textBox1);
			Controls.Add(seedButton);
			Controls.Add(menuStrip1);
			MainMenuStrip = menuStrip1;
			Name = "Main";
			Text = "FFMQ Randomizer (Beta 0.1.0)";
			Load += Form1_Load;
			menuStrip1.ResumeLayout(false);
			menuStrip1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button seedButton;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox seedBox;
		private System.Windows.Forms.TextBox flagstringBox;
		private System.Windows.Forms.Button generateButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox comboPresets;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem selectROMFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem configToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem flagsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem archipelagoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem devToolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem versionComparerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem scriptingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem jsonExportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.TextBox filterBox;
		private System.Windows.Forms.Label filterLabel;
		private System.Windows.Forms.Panel contentPanel;
		private System.Windows.Forms.ToolStripMenuItem loadAPMQFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel messageStripLabel;
	}
}

