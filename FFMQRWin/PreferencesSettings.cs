using FFMQLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using YamlDotNet.Core;
using static FFMQRWin.Main;

namespace FFMQRWin
{
    public class PreferencesSettings
    {
		private Dictionary<string, CheckBox> checkBoxes;
		private Dictionary<string, ComboBox> comboBoxes;
		private Dictionary<string, TextBox> textBoxes;
		private Dictionary<string, Label> boxLabels;
		private Dictionary<string, SpriteSelector> spriteBoxes;
		private Dictionary<string, Button> buttons;

		//private Preferences preferences;
		private string typeString;
		private Panel contentPanel;
		private PlayerSprites playerSprites;
		//private byte[] customPlayerSprite;

		private int maxIndex;

		public PreferencesSettings(Panel _contentPanel)
		{
			checkBoxes = new();
			comboBoxes = new();
			textBoxes = new();
			spriteBoxes = new();
			boxLabels = new();
			buttons = new();
			//preferences = _preferences;
			contentPanel = _contentPanel;
			typeString = "Pref";
			playerSprites = new(PlayerSpriteMode.Icons);
			//customPlayerSprite;
		}

		public void Initialize()
		{
			CreateCheckBox("RandomBenjaminPalette", "Randomize Benjamin's Palette");
			// Play Sprites
			CreateCheckBox("DarkKingTrueForm", "Randomize Dark King's True Form");
			CreateComboBox("MusicMode", "Music Mode", new List<string>() { "Normal", "Shuffle Tracks", "Mute" });
			CreateCheckBox("ReduceBattleFlash", "Reduce Battle Flashes");
			CreateSpriteSelector();
			CreateTextBox("CustomSpriteLocation", "Custom Sprite Location");
			CreateComboBox("FavoredPath", "ROM File Save Location", new List<string>() { "Original ROM File Location", "APMQ File Location", "Custom Save Location" });
			CreateTextBox("CustomSaveLocation", "Custom Save Location");

		}
		private void CreateComboBox(string name, string text, List<string> options)
		{
			ComboBox pathComboBox = new();
			Label pathComboLabel = new();

			int yOffset = LayoutValues.yInitialOffset + maxIndex * LayoutValues.yIncrement;

			pathComboBox.Name = "pref." + name;
			pathComboBox.Visible = true;
			pathComboBox.Location = new Point(LayoutValues.xOffset + LayoutValues.comboLabelWidth, yOffset);
			pathComboBox.Width = LayoutValues.comboBoxWidth;

			pathComboLabel.Name = "label." + name;
			pathComboLabel.Text = text;
			pathComboLabel.Visible = true;
			pathComboLabel.Location = new Point(LayoutValues.xOffset, yOffset);
			pathComboLabel.Width = LayoutValues.comboLabelWidth;

			foreach (var option in options)
			{
				pathComboBox.Items.Add(option);
			}

			pathComboBox.SelectedIndex = 0;
			pathComboBox.Text = "";

			comboBoxes.Add(name, pathComboBox);
			boxLabels.Add(name, pathComboLabel);
			contentPanel.Controls.Add(pathComboBox);
			contentPanel.Controls.Add(pathComboLabel);
			maxIndex++;
		}
		private void CreateCheckBox(string name, string text)
		{
			int yOffset = LayoutValues.yInitialOffset + maxIndex * LayoutValues.yIncrement;

			CheckBox checkBox = new();
			checkBox.Name = "pref." + name;
			checkBox.Text = text;
			checkBox.Checked = false;
			checkBox.Visible = true;
			checkBox.Location = new Point(LayoutValues.xOffset, yOffset);
			checkBox.Width = LayoutValues.checkBoxWidth;

			checkBoxes.Add(name, checkBox);
			contentPanel.Controls.Add(checkBox);
			maxIndex++;
		}
		private void CreateTextBox(string name, string text)
		{
			int yOffset = LayoutValues.yInitialOffset + maxIndex * LayoutValues.yIncrement;

			TextBox textBox = new();
			textBox.Name = "pref." + name;
			textBox.Text = "";
			textBox.Visible = true;
			textBox.Location = new Point(LayoutValues.xOffset + LayoutValues.comboLabelWidth, yOffset);
			textBox.Width = LayoutValues.comboBoxWidth;

			Label pathComboLabel = new();
			pathComboLabel.Name = "label." + name;
			pathComboLabel.Text = text;
			pathComboLabel.Visible = true;
			pathComboLabel.Location = new Point(LayoutValues.xOffset, yOffset);
			pathComboLabel.Width = LayoutValues.comboLabelWidth;

			Button button = new();
			button.Name = "button." + name;
			button.Text = "Select";
			button.Visible = true;
			button.Location = new Point(LayoutValues.xOffset + LayoutValues.comboBoxWidth + LayoutValues.comboLabelWidth, yOffset);
			button.Width = 80;
			if (name == "CustomSpriteLocation")
			{
				button.Click += spriteButton_Click;
			}
			else if (name == "CustomSaveLocation")
			{
				button.Click += saveLocationButton_Click;
			}
			

			textBoxes.Add(name, textBox);
			boxLabels.Add(name, pathComboLabel);
			buttons.Add(name, button);
			contentPanel.Controls.Add(textBox);
			contentPanel.Controls.Add(pathComboLabel);
			contentPanel.Controls.Add(button);
			maxIndex++;
		}
		public void CreateSpriteSelector()
		{
			int yOffset = LayoutValues.yInitialOffset + maxIndex * LayoutValues.yIncrement;
			string name = "PlayerSprite";

			SpriteSelector spriteSelector = new(playerSprites);
			spriteSelector.Name = "pref." + name;
			//spriteSelector.SelectedIndex = 0;
			spriteSelector.Visible = true;
			spriteSelector.Location = new Point(LayoutValues.xOffset + LayoutValues.comboLabelWidth, yOffset);
			spriteSelector.Width = LayoutValues.comboBoxWidth;

			Label pathComboLabel = new();
			pathComboLabel.Name = "label." + name;
			pathComboLabel.Text = "Player Sprite";
			pathComboLabel.Visible = true;
			pathComboLabel.Location = new Point(LayoutValues.xOffset, yOffset);
			pathComboLabel.Width = LayoutValues.comboLabelWidth;

			spriteBoxes.Add(name, spriteSelector);
			boxLabels.Add(name, pathComboLabel);
			
			contentPanel.Controls.Add(spriteSelector);
			contentPanel.Controls.Add(pathComboLabel);
			
			maxIndex++;
		}

		private void spriteButton_Click(object sender, EventArgs e)
		{
			var fileContent = string.Empty;
			string pathLocation;

			//var flags = new Flags();
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";
				openFileDialog.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					pathLocation = openFileDialog.FileName;
					Settings.Default.CustomSpritesLocation = pathLocation;
					textBoxes["CustomSpriteLocation"].Text = pathLocation;
				}
			}
		}

		private void saveLocationButton_Click(object sender, EventArgs e)
		{
			var fileContent = string.Empty;
			string spriteFileLocation;

			//var flags = new Flags();
			using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					spriteFileLocation = openFileDialog.SelectedPath;
					Settings.Default.CustomSavePath = spriteFileLocation;
					textBoxes["CustomSaveLocation"].Text = spriteFileLocation;
				}
			}
		}

		public void ShowList()
		{
			checkBoxes.Values.ToList().ForEach(b => b.Visible = true);
			comboBoxes.Values.ToList().ForEach(b => b.Visible = true);
			textBoxes.Values.ToList().ForEach(b => b.Visible = true);
			boxLabels.Values.ToList().ForEach(b => b.Visible = true);
		}
		public void HideList()
		{
			checkBoxes.Values.ToList().ForEach(b => b.Visible = false);
			comboBoxes.Values.ToList().ForEach(b => b.Visible = false);
			textBoxes.Values.ToList().ForEach(b => b.Visible = false);
			boxLabels.Values.ToList().ForEach(b => b.Visible = false);
		}
		public void UpdateValues()
		{
			checkBoxes["RandomBenjaminPalette"].Checked = Settings.Default.RandomBenjaminPalette;
			checkBoxes["DarkKingTrueForm"].Checked = Settings.Default.DarkKingTrueForm;
			checkBoxes["ReduceBattleFlash"].Checked = Settings.Default.ReduceBattleFlash;

			comboBoxes["FavoredPath"].SelectedIndex = Settings.Default.FavoredPath;
			comboBoxes["MusicMode"].SelectedIndex = Settings.Default.MusicMode;

			spriteBoxes["PlayerSprite"].CurrentSprite = Settings.Default.PlayerSprite;
			textBoxes["CustomSpriteLocation"].Text = Settings.Default.CustomSpritesLocation;

			textBoxes["CustomSaveLocation"].Text = Settings.Default.CustomSavePath;


		}
		public void SaveValues()
		{
			Settings.Default.RandomBenjaminPalette = checkBoxes["RandomBenjaminPalette"].Checked;
			Settings.Default.DarkKingTrueForm = checkBoxes["DarkKingTrueForm"].Checked;
			Settings.Default.ReduceBattleFlash = checkBoxes["ReduceBattleFlash"].Checked;

			Settings.Default.FavoredPath = comboBoxes["FavoredPath"].SelectedIndex;
			Settings.Default.MusicMode = comboBoxes["MusicMode"].SelectedIndex;

			Settings.Default.PlayerSprite = spriteBoxes["PlayerSprite"].CurrentSprite;
			Settings.Default.CustomSpritesLocation = textBoxes["CustomSpriteLocation"].Text;

			Settings.Default.CustomSavePath = textBoxes["CustomSaveLocation"].Text;
			Settings.Default.Save();
		}
		public Preferences CreatePreferences()
		{
			Preferences pref = new();

			pref.RandomBenjaminPalette = checkBoxes["RandomBenjaminPalette"].Checked;
			pref.DarkKingTrueForm = checkBoxes["DarkKingTrueForm"].Checked;
			pref.ReduceBattleFlash = checkBoxes["ReduceBattleFlash"].Checked;
			pref.MusicMode = (MusicMode)comboBoxes["MusicMode"].SelectedIndex;
			pref.PlayerSprite = spriteBoxes["PlayerSprite"].CurrentSprite;

			return pref;
		}
		public static Preferences GetPreferences(byte[] customSprite)
		{
			Preferences pref = new();

			pref.RandomBenjaminPalette = Settings.Default.RandomBenjaminPalette;
			pref.DarkKingTrueForm = Settings.Default.DarkKingTrueForm;
			pref.ReduceBattleFlash = Settings.Default.ReduceBattleFlash;
			pref.MusicMode = (MusicMode)Settings.Default.MusicMode;
			pref.PlayerSprite = Settings.Default.PlayerSprite;
			pref.CustomSprites = customSprite;

			try
			{
				pref.ValidateCustomSprites();
			}
			catch (Exception ex)
			{
				pref.PlayerSprite = "default";
			}

			return pref;
		}
	}
}
