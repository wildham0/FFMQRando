using FFMQLib;
using Microsoft.VisualBasic.FileIO;
using RomUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using YamlDotNet.Core.Tokens;
using YamlDotNet.RepresentationModel;

namespace FFMQRWin
{
	public partial class Main : Form
	{
		public FFMQRom newRom = new();
		public Flags flags = new();
		public ApConfigs apConfigs = new();


		public FFMQRom FFMQRom = new();
		public ApConfigs ApConfigs = new();
		public Flags Flags = new();
		public byte[] CustomSprite;

		public string romDirectoryPath = "";
		public string apmqDirectoryPath = "";
		public string customDirectoryPath = "";
		public string savePath { get => GetSavePath(); }
		public string romLocation = "";
		public FavoredPaths favoredPath = FavoredPaths.RomLocation;
		public Blob seed;

		public FlagsSettings FlagsSettings;

		ContentModes Mode = ContentModes.Randomizer;

		// launching
		// .set path
		// open APMQ file
		// open ROM file
		// set preferences
		// generate
		// save file

		// command
		// ap
		// set path
		public Main()
		{
			string[] args = Environment.GetCommandLineArgs();
			var filePath = args.Length > 1 ? new FilePath(args[1].Replace("\"", "")) : new FilePath("");

			if (filePath.Type != "apmq")
			{
				InitializeComponent();
			}
			else
			{
				seed = Blob.FromHex("00000000");
				string resultMessage = "";

				// Configure paths
				SetupPaths(filePath.Path);

				if (Settings.Default.RomFileLocation == "")
				{
					MessageBox.Show("Can't generate: No default ROM found. Launch the program normally and select a default ROM.");
					Application.Exit();
				}

				if (!FileManager.LoadAPMQFile(filePath, ApConfigs, Flags, ref resultMessage))
				{
					Application.Exit();
				}

				if (!FileManager.LoadRom(Settings.Default.RomFileLocation, FFMQRom, ref resultMessage))
				{
					Application.Exit();
				}

				// Create preferences
				FileManager.LoadCustomSprites(Settings.Default.CustomSpritesLocation, ref CustomSprite, ref resultMessage);
				var preferences = PreferencesSettings.GetPreferences(CustomSprite);

				// Generate
				try
				{
					FFMQRom.Randomize(seed, flags, preferences, apConfigs);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Generation Error.\n" + ex);
					Application.Exit();
				}

				// Save File
				var outputFile = File.Create(savePath + apConfigs.FileName + ".sfc");
				newRom.Save(outputFile);
				outputFile.Close();

				Application.Exit();
			}
		}
		private string GetSavePath()
		{
			if (favoredPath == FavoredPaths.CustomLocation && customDirectoryPath != "")
			{
				return customDirectoryPath;
			}
			else if (favoredPath == FavoredPaths.APMQFileLocation && apConfigs.ApEnabled)
			{
				return apmqDirectoryPath;
			}
			else
			{
				return romDirectoryPath;
			}
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			seed = Blob.FromHex("00000000");
			var rng = new Random();
			rng.NextBytes(seed);
			seedBox.Text = seed.ToHex();
			this.Text = "FFMQ Randomizer v" + Metadata.Version;

			string resultMessage = "";
			SetupPaths("");

			if (Settings.Default.RomFileLocation != "")
			{
				if (FileManager.LoadRom(Settings.Default.RomFileLocation, FFMQRom, ref resultMessage))
				{
					messageStripLabel.Text = resultMessage;
					textBox1.Text = Settings.Default.RomFileLocation;
					romDirectoryPath = (new FilePath(Settings.Default.RomFileLocation)).Path;
				}
				else
				{
					messageStripLabel.Text = resultMessage;
				}
			}

			if (Settings.Default.LastFlagset != "")
			{
				var flagstring = Settings.Default.LastFlagset;
				flags = new();
				flags.ReadFlagString(flagstring);
				FlagsSettings = new(flags, contentPanel, flagstringBox);
				flagstringBox.Text = flagstring;
			}
			else
			{
				FlagsSettings = new(flags, contentPanel, flagstringBox);
				flagstringBox.Text = flags.GenerateFlagString();
			}

			FlagsSettings.Initialize();

			Mode = (ContentModes)Settings.Default.LastMode;
			SwitchMode(Mode);

			messageStripLabel.Text = "FFMQ Randomzier launched succesfully.";
		}

		private void SwitchMode(ContentModes mode)
		{
			if (mode == ContentModes.Randomizer)
			{
				FlagsSettings.ShowList(filterBox.Text);
				this.flagsToolStripMenuItem.Checked = true;
				this.archipelagoToolStripMenuItem.Checked = false;
				this.preferencesToolStripMenuItem.Checked = false;

				this.seedBox.Enabled = true;
				this.flagstringBox.Enabled = true;
				this.seedButton.Enabled = true;
				this.filterLabel.Visible = true;
				this.filterBox.Visible = true;

				generateButton.Enabled = true;
				messageStripLabel.Text = "";

				Settings.Default.LastMode = 0;
				Settings.Default.Save();
			}
			else if (mode == ContentModes.Archipelago)
			{
				FlagsSettings.HideList();
				this.flagsToolStripMenuItem.Checked = false;
				this.archipelagoToolStripMenuItem.Checked = true;
				this.preferencesToolStripMenuItem.Checked = false;

				this.seedBox.Enabled = false;
				this.flagstringBox.Enabled = false;
				this.seedButton.Enabled = false;
				this.filterLabel.Visible = false;
				this.filterBox.Visible = false;

				if (!ApConfigs.ApEnabled)
				{
					messageStripLabel.Text = "Awaiting APMQ file...";
					generateButton.Enabled = false;
				}
				else
				{
					messageStripLabel.Text = "APMQ file sucessfully loaded.";
					generateButton.Enabled = true;
				}

				Settings.Default.LastMode = 2;
				Settings.Default.Save();
			}
		}
		private void generate_Click(object sender, EventArgs e)
		{
			FFMQRom.RestoreOriginalData();
			//try
			//{
				// Manage custom sprite, a bit clunky, but should work for now
				string resultMessage = "";
				
				FileManager.LoadCustomSprites(Settings.Default.CustomSpritesLocation, ref CustomSprite, ref resultMessage);
				messageStripLabel.Text = resultMessage;
				var preferences = PreferencesSettings.GetPreferences(CustomSprite);

				// Randomize
				FFMQRom.Randomize(seed, Flags, preferences, ApConfigs);

				var outputFile = File.Create(savePath + "FFMQR_" + seed.ToHex() + ".sfc");
				FFMQRom.Save(outputFile);
				outputFile.Close();
				messageStripLabel.Text = "Randomized ROM file generated successfully.";
			/*}
			catch (Exception ex)
			{
				messageStripLabel.Text = "Generation error.";
				MessageBox.Show("Generation error.\n" + ex.Message);
			}*/
		}
		private void rollSeed_Click(object sender, EventArgs e)
		{
			var rng = new Random();
			rng.NextBytes(seed);
			seedBox.Text = seed.ToHex();
		}

		private void seedBox_TextChanged(object sender, EventArgs e)
		{
			if (seedBox.Text.Length <= 8)
			{
				try
				{
					var tempSeed = seedBox.Text.PadLeft(8, '0');
					seed = Blob.FromHex(tempSeed);
				}
				catch (Exception ex)
				{
					messageStripLabel.Text = ex.Message;
				}
			}
		}
				private void flagstringBox_TextChanged(object sender, EventArgs e)
		{
			try
			{
				flags.ReadFlagString(((TextBox)sender).Text);
				Settings.Default.LastFlagset = ((TextBox)sender).Text;
				Settings.Default.Save();
				FlagsSettings.UpdateValues();
			}
			catch (Exception ex)
			{
				messageStripLabel.Text = ex.Message;
			}
		}

		private void flagFilterBox_TextChanged(object sender, EventArgs e)
		{
			SwitchMode(Mode);
		}

		private void versionComparerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RomComparer comparerForm = new RomComparer();
			comparerForm.ShowDialog();
		}

		private void scriptingToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Form2 interpreterForm = new Form2();
			interpreterForm.ShowDialog();
		}

		private void jsonExportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//exportTilesPropJson();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("FFMQ Randomizer v" + FFMQLib.Metadata.Version + "\nMain Developer: wildham\ngithub: https://github.com/wildham0/FFMQRando");
		}

		private void selectROMFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string resultMessage = "";
			var romPath = FileManager.OpenFileDialog("sfc", "c:\\");

			if (romPath.Path != "")
			{
				if (FileManager.LoadRom(romPath.Full, FFMQRom, ref resultMessage))
				{
					messageStripLabel.Text = resultMessage;
					romDirectoryPath = romPath.Path;
					textBox1.Text = romPath.Full;
					Settings.Default.RomFileLocation = romPath.Full;
					Settings.Default.Save();
				}
				else
				{
					messageStripLabel.Text = resultMessage;
				}
			}
		}
		private void SetupPaths(string apmqpath)
		{
			apmqDirectoryPath = apmqpath;
			customDirectoryPath = Settings.Default.CustomSavePath;
			romDirectoryPath = new FilePath(Settings.Default.RomFileLocation).Path;
			favoredPath = (FavoredPaths)Settings.Default.FavoredPath;
		}

		private void loadAPMQFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string resultMessage = "";
			var apmqFile = FileManager.OpenFileDialog("apmq", "c:\\");


			if (apmqFile.Path != "")
			{
				if (FileManager.LoadAPMQFile(apmqFile, ApConfigs, Flags, ref resultMessage))
				{
					messageStripLabel.Text = resultMessage;
					apmqDirectoryPath = apmqFile.Path;
				}
				else
				{
					messageStripLabel.Text = resultMessage;
				}
			}
		}
		private void flagsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Mode = ContentModes.Randomizer;
			SwitchMode(Mode);
		}

		private void archipelagoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Mode = ContentModes.Archipelago;
			SwitchMode(Mode);
		}

		private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PreferencesForm preferencesForm = new PreferencesForm();
			preferencesForm.ShowDialog();
		}

		private void preferencesToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
			PreferencesForm preferencesForm = new PreferencesForm();
			preferencesForm.ShowDialog();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}

	public enum ContentModes
	{
		Randomizer,
		Archipelago,
		ImprovedVanilla,
	}

	public enum FavoredPaths
	{ 
		RomLocation,
		APMQFileLocation,
		CustomLocation,
	}

	public static class LayoutValues
	{
		public const int xOffset = 20;
		public const int yInitialOffset = 20;
		public const int yIncrement = 30;
		public const int checkBoxWidth = 200;
		public const int comboBoxWidth = 200;
		public const int comboLabelWidth = 150;
	}
}
