using System.Drawing;
using System.Windows.Forms;

namespace FFMQRWin
{
    partial class PreferencesForm
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
			mainPanel = new Panel();
			saveButton = new Button();
			cancelButton = new Button();
			messageStrip = new StatusStrip();
			messageStripLabel = new ToolStripStatusLabel();
			messageStrip.SuspendLayout();
			SuspendLayout();
			// 
			// mainPanel
			// 
			mainPanel.AutoScroll = true;
			mainPanel.Location = new Point(12, 12);
			mainPanel.Name = "mainPanel";
			mainPanel.Size = new Size(520, 405);
			mainPanel.TabIndex = 0;
			// 
			// saveButton
			// 
			saveButton.Location = new Point(376, 423);
			saveButton.Name = "saveButton";
			saveButton.Size = new Size(75, 23);
			saveButton.TabIndex = 1;
			saveButton.Text = "Save";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += saveButton_Click;
			// 
			// cancelButton
			// 
			cancelButton.Location = new Point(457, 423);
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size(75, 23);
			cancelButton.TabIndex = 2;
			cancelButton.Text = "Cancel";
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += cancelButton_Click;
			// 
			// messageStrip
			// 
			messageStrip.Items.AddRange(new ToolStripItem[] { messageStripLabel });
			messageStrip.Location = new Point(0, 449);
			messageStrip.Name = "messageStrip";
			messageStrip.Size = new Size(544, 22);
			messageStrip.TabIndex = 3;
			messageStrip.Text = "statusStrip1";
			// 
			// messageStripLabel
			// 
			messageStripLabel.Name = "messageStripLabel";
			messageStripLabel.Size = new Size(118, 17);
			messageStripLabel.Text = "toolStripStatusLabel1";
			messageStripLabel.Click += toolStripStatusLabel1_Click;
			// 
			// PreferencesForm
			// 
			ClientSize = new Size(544, 471);
			Controls.Add(messageStrip);
			Controls.Add(cancelButton);
			Controls.Add(saveButton);
			Controls.Add(mainPanel);
			Name = "PreferencesForm";
			Text = "Preferences";
			Load += PreferencesForm_Load;
			messageStrip.ResumeLayout(false);
			messageStrip.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

		}
		private Panel mainPanel;
		private Button saveButton;
		private Button cancelButton;
		#endregion

		private StatusStrip messageStrip;
		private ToolStripStatusLabel messageStripLabel;
	}
}