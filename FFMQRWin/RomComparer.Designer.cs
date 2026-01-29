namespace FFMQRWin
{
	partial class RomComparer
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
			rom10Label = new System.Windows.Forms.Label();
			rom11Label = new System.Windows.Forms.Label();
			textBox1 = new System.Windows.Forms.TextBox();
			textBox2 = new System.Windows.Forms.TextBox();
			button1 = new System.Windows.Forms.Button();
			button2 = new System.Windows.Forms.Button();
			messageLabel = new System.Windows.Forms.Label();
			resultTextBox = new System.Windows.Forms.TextBox();
			button4 = new System.Windows.Forms.Button();
			SuspendLayout();
			// 
			// rom10Label
			// 
			rom10Label.AutoSize = true;
			rom10Label.Location = new System.Drawing.Point(12, 9);
			rom10Label.Name = "rom10Label";
			rom10Label.Size = new System.Drawing.Size(50, 15);
			rom10Label.TabIndex = 0;
			rom10Label.Text = "Rom 1.0";
			rom10Label.Click += label1_Click;
			// 
			// rom11Label
			// 
			rom11Label.AutoSize = true;
			rom11Label.Location = new System.Drawing.Point(12, 40);
			rom11Label.Name = "rom11Label";
			rom11Label.Size = new System.Drawing.Size(50, 15);
			rom11Label.TabIndex = 1;
			rom11Label.Text = "Rom 1.1";
			// 
			// textBox1
			// 
			textBox1.Location = new System.Drawing.Point(68, 6);
			textBox1.Name = "textBox1";
			textBox1.Size = new System.Drawing.Size(303, 23);
			textBox1.TabIndex = 2;
			// 
			// textBox2
			// 
			textBox2.Location = new System.Drawing.Point(68, 37);
			textBox2.Name = "textBox2";
			textBox2.Size = new System.Drawing.Size(303, 23);
			textBox2.TabIndex = 3;
			// 
			// button1
			// 
			button1.Location = new System.Drawing.Point(377, 6);
			button1.Name = "button1";
			button1.Size = new System.Drawing.Size(75, 23);
			button1.TabIndex = 4;
			button1.Text = "Load Rom";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click_1;
			// 
			// button2
			// 
			button2.Location = new System.Drawing.Point(377, 40);
			button2.Name = "button2";
			button2.Size = new System.Drawing.Size(75, 23);
			button2.TabIndex = 5;
			button2.Text = "Load Rom";
			button2.UseVisualStyleBackColor = true;
			button2.Click += button2_Click;
			// 
			// messageLabel
			// 
			messageLabel.AutoSize = true;
			messageLabel.Location = new System.Drawing.Point(471, 17);
			messageLabel.Name = "messageLabel";
			messageLabel.Size = new System.Drawing.Size(38, 15);
			messageLabel.TabIndex = 6;
			messageLabel.Text = "label1";
			// 
			// resultTextBox
			// 
			resultTextBox.Location = new System.Drawing.Point(12, 108);
			resultTextBox.Multiline = true;
			resultTextBox.Name = "resultTextBox";
			resultTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			resultTextBox.Size = new System.Drawing.Size(449, 320);
			resultTextBox.TabIndex = 7;
			// 
			// button4
			// 
			button4.Location = new System.Drawing.Point(332, 67);
			button4.Name = "button4";
			button4.Size = new System.Drawing.Size(68, 25);
			button4.TabIndex = 8;
			button4.Text = "Compare";
			button4.UseVisualStyleBackColor = true;
			button4.Click += button4_Click;
			// 
			// RomComparer
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			ClientSize = new System.Drawing.Size(800, 450);
			Controls.Add(button4);
			Controls.Add(resultTextBox);
			Controls.Add(messageLabel);
			Controls.Add(button2);
			Controls.Add(button1);
			Controls.Add(textBox2);
			Controls.Add(textBox1);
			Controls.Add(rom11Label);
			Controls.Add(rom10Label);
			Name = "RomComparer";
			Text = "Form3";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.Label rom10Label;
		private System.Windows.Forms.Label rom11Label;
		private System.Windows.Forms.TextBox resultTextBox;
		private System.Windows.Forms.Button button4;
	}
}