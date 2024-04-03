
namespace FFMQRWin
{
	partial class Form1
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
			button1 = new System.Windows.Forms.Button();
			openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			button2 = new System.Windows.Forms.Button();
			textBox1 = new System.Windows.Forms.TextBox();
			textBox2 = new System.Windows.Forms.TextBox();
			textBox3 = new System.Windows.Forms.TextBox();
			button4 = new System.Windows.Forms.Button();
			comboBox1 = new System.Windows.Forms.ComboBox();
			checkBox1 = new System.Windows.Forms.CheckBox();
			checkBox2 = new System.Windows.Forms.CheckBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			trackBar1 = new System.Windows.Forms.TrackBar();
			label7 = new System.Windows.Forms.Label();
			trackBar2 = new System.Windows.Forms.TrackBar();
			trackBar3 = new System.Windows.Forms.TrackBar();
			label8 = new System.Windows.Forms.Label();
			button3 = new System.Windows.Forms.Button();
			label9 = new System.Windows.Forms.Label();
			comboBox2 = new System.Windows.Forms.ComboBox();
			button5 = new System.Windows.Forms.Button();
			comboPresets = new System.Windows.Forms.ComboBox();
			comparerButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackBar2).BeginInit();
			((System.ComponentModel.ISupportInitialize)trackBar3).BeginInit();
			SuspendLayout();
			// 
			// button1
			// 
			button1.Location = new System.Drawing.Point(443, 36);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(74, 23);
			button1.TabIndex = 0;
			button1.Text = "Load File";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// openFileDialog1
			// 
			openFileDialog1.FileName = "openFileDialog1";
			// 
			// button2
			// 
			button2.Location = new System.Drawing.Point(443, 74);
			button2.Name = "button2";
			button2.Size = new System.Drawing.Size(74, 23);
			button2.TabIndex = 1;
			button2.Text = "Roll Seed";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// textBox1
			// 
			textBox1.Location = new System.Drawing.Point(73, 36);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(350, 23);
			textBox1.TabIndex = 2;
			// 
			// textBox2
			// 
			textBox2.Location = new System.Drawing.Point(73, 74);
			textBox2.Name = "textBox2";
			textBox2.Size = new System.Drawing.Size(350, 23);
			textBox2.TabIndex = 3;
			textBox2.TextChanged += textBox2_TextChanged;
			// 
			// textBox3
			// 
			textBox3.Location = new System.Drawing.Point(73, 112);
			textBox3.Name = "textBox3";
			textBox3.Size = new System.Drawing.Size(350, 23);
			textBox3.TabIndex = 4;
			textBox3.TextChanged += textBox3_TextChanged;
			// 
			// button4
			// 
			button4.Location = new System.Drawing.Point(549, 36);
			button4.Name = "button4";
			button4.Size = new System.Drawing.Size(239, 61);
			button4.TabIndex = 6;
			button4.Text = "Generate";
			button4.UseVisualStyleBackColor = true;
			button4.Click += button4_Click;
			// 
			// comboBox1
			// 
			comboBox1.FormattingEnabled = true;
			comboBox1.Location = new System.Drawing.Point(167, 234);
			comboBox1.Name = "comboBox1";
			comboBox1.Size = new System.Drawing.Size(350, 23);
			comboBox1.TabIndex = 7;
			comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
			// 
			// checkBox1
			// 
			checkBox1.AutoSize = true;
			checkBox1.Checked = true;
			checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			checkBox1.Enabled = false;
			checkBox1.Location = new System.Drawing.Point(73, 184);
			checkBox1.Name = "checkBox1";
			checkBox1.Size = new System.Drawing.Size(129, 19);
			checkBox1.TabIndex = 8;
			checkBox1.Text = "Shuffle Quest Items";
			checkBox1.UseVisualStyleBackColor = true;
			checkBox1.CheckedChanged += checkBox1_CheckedChanged;
			// 
			// checkBox2
			// 
			checkBox2.AutoSize = true;
			checkBox2.Location = new System.Drawing.Point(73, 209);
			checkBox2.Name = "checkBox2";
			checkBox2.Size = new System.Drawing.Size(164, 19);
			checkBox2.TabIndex = 9;
			checkBox2.Text = "Shuffle Enemies' Positions";
			checkBox2.UseVisualStyleBackColor = true;
			checkBox2.CheckedChanged += checkBox2_CheckedChanged;
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
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(73, 237);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(93, 15);
			label4.TabIndex = 13;
			label4.Text = "Enemies Density";
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Location = new System.Drawing.Point(12, 152);
			label5.Name = "label5";
			label5.Size = new System.Drawing.Size(0, 15);
			label5.TabIndex = 14;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Location = new System.Drawing.Point(73, 266);
			label6.Name = "label6";
			label6.Size = new System.Drawing.Size(126, 15);
			label6.TabIndex = 16;
			label6.Text = "Enemies Scaling: 100%";
			label6.Click += label6_Click;
			// 
			// trackBar1
			// 
			trackBar1.Location = new System.Drawing.Point(217, 263);
			trackBar1.Name = "trackBar1";
			trackBar1.Size = new System.Drawing.Size(146, 45);
			trackBar1.TabIndex = 17;
			trackBar1.Scroll += trackBar1_Scroll;
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Location = new System.Drawing.Point(384, 269);
			label7.Name = "label7";
			label7.Size = new System.Drawing.Size(106, 15);
			label7.TabIndex = 19;
			label7.Text = "Scaling Spread: 0%";
			// 
			// trackBar2
			// 
			trackBar2.Location = new System.Drawing.Point(509, 263);
			trackBar2.Name = "trackBar2";
			trackBar2.Size = new System.Drawing.Size(146, 45);
			trackBar2.TabIndex = 20;
			trackBar2.Scroll += trackBar2_Scroll;
			// 
			// trackBar3
			// 
			trackBar3.Location = new System.Drawing.Point(217, 305);
			trackBar3.Name = "trackBar3";
			trackBar3.Size = new System.Drawing.Size(146, 45);
			trackBar3.TabIndex = 22;
			trackBar3.Scroll += trackBar3_Scroll;
			// 
			// label8
			// 
			label8.AutoSize = true;
			label8.Location = new System.Drawing.Point(73, 308);
			label8.Name = "label8";
			label8.Size = new System.Drawing.Size(103, 15);
			label8.TabIndex = 21;
			label8.Text = "Leveling Curve: 1x";
			// 
			// button3
			// 
			button3.Location = new System.Drawing.Point(712, 7);
			button3.Name = "button3";
			button3.Size = new System.Drawing.Size(76, 23);
			button3.TabIndex = 23;
			button3.Text = "About";
			button3.UseVisualStyleBackColor = true;
			button3.Click += button3_Click;
			// 
			// label9
			// 
			label9.AutoSize = true;
			label9.Location = new System.Drawing.Point(73, 347);
			label9.Name = "label9";
			label9.Size = new System.Drawing.Size(91, 15);
			label9.TabIndex = 25;
			label9.Text = "Battlefields' Size";
			// 
			// comboBox2
			// 
			comboBox2.FormattingEnabled = true;
			comboBox2.Location = new System.Drawing.Point(167, 344);
			comboBox2.Name = "comboBox2";
			comboBox2.Size = new System.Drawing.Size(350, 23);
			comboBox2.TabIndex = 24;
			comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
			// 
			// button5
			// 
			button5.Location = new System.Drawing.Point(549, 7);
			button5.Name = "button5";
			button5.Size = new System.Drawing.Size(101, 23);
			button5.TabIndex = 26;
			button5.Text = "Script Tool";
			button5.UseVisualStyleBackColor = true;
			button5.Click += button5_Click;
			// 
			// comboPresets
			// 
			comboPresets.FormattingEnabled = true;
			comboPresets.Location = new System.Drawing.Point(443, 113);
			comboPresets.Name = "comboPresets";
			comboPresets.Size = new System.Drawing.Size(350, 23);
			comboPresets.TabIndex = 27;
			// 
			// comparerButton
			// 
			comparerButton.Location = new System.Drawing.Point(442, 7);
			comparerButton.Name = "comparerButton";
			comparerButton.Size = new System.Drawing.Size(101, 23);
			comparerButton.TabIndex = 28;
			comparerButton.Text = "Comparer Tool";
			comparerButton.UseVisualStyleBackColor = true;
			comparerButton.Click += comparerButton_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(comparerButton);
			Controls.Add(comboPresets);
			Controls.Add(button5);
			Controls.Add(label9);
			Controls.Add(comboBox2);
			Controls.Add(button3);
			Controls.Add(trackBar3);
			Controls.Add(label8);
			Controls.Add(trackBar2);
			Controls.Add(label7);
			Controls.Add(trackBar1);
			Controls.Add(label6);
			Controls.Add(label5);
			Controls.Add(label4);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(checkBox2);
			Controls.Add(checkBox1);
			Controls.Add(comboBox1);
			Controls.Add(button4);
			Controls.Add(textBox3);
			Controls.Add(textBox2);
			Controls.Add(textBox1);
			Controls.Add(button2);
			Controls.Add(button1);
			Name = "Form1";
			Text = "FFMQ Randomizer (Beta 0.1.0)";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
			((System.ComponentModel.ISupportInitialize)trackBar2).EndInit();
			((System.ComponentModel.ISupportInitialize)trackBar3).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TrackBar trackBar2;
		private System.Windows.Forms.TrackBar trackBar3;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.ComboBox comboPresets;
		private System.Windows.Forms.Button comparerButton;
	}
}

