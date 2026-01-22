using FFMQLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFMQRWin
{
	public partial class PreferencesForm : Form
	{

		public PreferencesSettings PreferencesSettings;
		public PreferencesForm()
		{
			InitializeComponent();
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			PreferencesSettings.SaveValues();
			messageStripLabel.Text = "Preferences saved successfully.";
		}

		private void PreferencesForm_Load(object sender, EventArgs e)
		{
			PreferencesSettings = new(mainPanel);
			PreferencesSettings.Initialize();
			PreferencesSettings.UpdateValues();

			messageStripLabel.Text = "Preferences loaded successfully.";
		}
		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void toolStripStatusLabel1_Click(object sender, EventArgs e)
		{

		}
	}
}
