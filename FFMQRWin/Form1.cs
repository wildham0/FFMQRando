﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using FFMQLib;
using RomUtilities;
using System.Configuration;
using System.IO.Pipes;
using static System.Net.Mime.MediaTypeNames;


namespace FFMQRWin
{
	public partial class Form1 : Form
	{
		public FFMQRom newRom = new();
		public Flags flags = new();
		public Preferences preferences = new();
		public string directoryPath = "";
		public Blob seed;
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			seed = Blob.FromHex("00000000");
			var rng = new Random();
			rng.NextBytes(seed);
			textBox2.Text = seed.ToHex();
			/*
			comboBox1.DataSource = Enum.GetValues<FFMQLib.EnemiesDensity>();
			comboBox2.DataSource = Enum.GetValues<FFMQLib.BattlesQty>();

			trackBar1.TickFrequency = 1;
			trackBar1.Maximum = Enum.GetValues<FFMQLib.EnemiesScaling>().Length - 1;
			trackBar1.Minimum = 0;

			trackBar2.TickFrequency = 1;
			trackBar2.Maximum = Enum.GetValues<FFMQLib.EnemiesScalingSpread>().Length - 1;
			trackBar2.Minimum = 0;

			trackBar3.TickFrequency = 1;
			trackBar3.Maximum = Enum.GetValues<FFMQLib.LevelingCurve>().Length - 1;
			trackBar3.Minimum = 0;
			trackBar3.Value = (int)flags.LevelingCurve;
			*/

			if (Settings.Default.RomFileLocation != "")
			{
				var rompath = Settings.Default.RomFileLocation;
				textBox1.Text = rompath;
				newRom = new();
				FileStream fileStream;
				try
				{
					fileStream = new FileStream(rompath, FileMode.Open);
					newRom.Load(fileStream);
				}
				catch (Exception ex)
				{
					label5.Text = ex.Message;
				}

				if (newRom.Validate())
				{
					label5.Text = "ROM file loaded successfully.";
					newRom.BackupOriginalData();

					var filePath = rompath.Split('\\');
					for (int i = 0; i < (filePath.Length - 1); i++)
					{
						directoryPath += filePath[i] + "\\";
					}
				}
				else
				{
					newRom = new();
					label5.Text = "Saved path was invalid.";
				}
			}

			if (Settings.Default.LastFlagset != "")
			{
				var flagstring = Settings.Default.LastFlagset;
				flags = new();
				flags.ReadFlagString(flagstring);
				textBox3.Text = flagstring;

			}
			else
			{
				textBox3.Text = flags.GenerateFlagString();
			}
			
			label5.Text = "FFMQ Randomzier launched succesfully.";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			var fileContent = string.Empty;
			//var filePath = string.Empty;
			newRom = new FFMQRom();
			//var flags = new Flags();
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";
				openFileDialog.Filter = "sfc files (*.sfc)|*.sfc|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					var filePath = openFileDialog.FileName.Split('\\');
					for (int i = 0; i < (filePath.Length - 1); i++)
					{
						directoryPath += filePath[i] + "\\";
					}

					//Read the contents of the file into a stream
					var fileStream = openFileDialog.OpenFile();

					newRom.Load(fileStream);

					if (newRom.Validate())
					{
						textBox1.Text = openFileDialog.FileName;

						label5.Text = "ROM file loaded successfully.";

						newRom.BackupOriginalData();
						Settings.Default.RomFileLocation = openFileDialog.FileName;
						Settings.Default.Save();
					}
					else
					{
						newRom = new();
						label5.Text = "Non valid ROM, please use headerless NA Rev1.1 rom (MD5: f7faeae5a847c098d677070920769ca2)";
					}





					/*
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }*/
				}
			}



		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void button4_Click(object sender, EventArgs e)
		{
			ApConfigs apconfigs = new();

			newRom.RestoreOriginalData();

			newRom.Randomize(seed, flags, preferences, apconfigs);

			var outputFile = File.Create(directoryPath + "FFMQR_" + seed.ToHex() + ".sfc");

			newRom.Save(outputFile);

			outputFile.Close();

			label5.Text = "Randomized ROM file generated successfully.";
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			flags.ShuffleEnemiesPosition = ((CheckBox)sender).Checked;
			textBox3.Text = flags.GenerateFlagString();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			flags.EnemiesDensity = Enum.Parse<FFMQLib.EnemiesDensity>(((ComboBox)sender).SelectedItem.ToString());
			textBox3.Text = flags.GenerateFlagString();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var rng = new Random();
			rng.NextBytes(seed);
			textBox2.Text = seed.ToHex();
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			if (textBox2.Text.Length <= 8)
			{
				try
				{
					var tempSeed = textBox2.Text.PadLeft(8, '0');
					seed = Blob.FromHex(tempSeed);
				}
				catch (Exception ex)
				{
					label5.Text = ex.Message;
				}
			}
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			try
			{
				flags.ReadFlagString(((TextBox)sender).Text);
				checkBox2.Checked = flags.ShuffleEnemiesPosition;
				comboBox1.SelectedItem = flags.EnemiesDensity;
				comboBox2.SelectedItem = flags.BattlesQuantity;
				trackBar3.Value = (int)flags.LevelingCurve;
				label8.Text = "Leveling Curve: " + flags.LevelingCurve.GetDescription();

				Settings.Default.LastFlagset = ((TextBox)sender).Text;
				Settings.Default.Save();
			}
			catch (Exception ex)
			{
				label5.Text = ex.Message;
			}
		}

		private void label6_Click(object sender, EventArgs e)
		{

		}

		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			textBox3.Text = flags.GenerateFlagString();
		}
		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			textBox3.Text = flags.GenerateFlagString();
		}

		private void trackBar3_Scroll(object sender, EventArgs e)
		{
			flags.LevelingCurve = (LevelingCurve)((TrackBar)sender).Value;
			label8.Text = "Leveling Curve: " + flags.LevelingCurve.GetDescription();
			textBox3.Text = flags.GenerateFlagString();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			MessageBox.Show("FFMQ Randomizer Beta v" + FFMQLib.Metadata.Version + "\nMain Developer: wildham\ngithub: https://github.com/wildham0/FFMQRando");
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			flags.BattlesQuantity = Enum.Parse<FFMQLib.BattlesQty>(((ComboBox)sender).SelectedItem.ToString());
			textBox3.Text = flags.GenerateFlagString();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			Form2 interpreterForm = new Form2();
			interpreterForm.ShowDialog();
		}

		private void comparerButton_Click(object sender, EventArgs e)
		{
			RomComparer comparerForm = new RomComparer();
			comparerForm.ShowDialog();
		}
	}
}
