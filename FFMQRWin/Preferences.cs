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
		private PlayerSprites PlayerSprites;
		private int currentSprite;

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

			PlayerSprites = new(PlayerSpriteMode.Icons);
			currentSprite = 0;

			messageStripLabel.Text = "Preferences loaded successfully.";
		}
		private void CreateSpriteBox()
		{ 
			
		
		}


		/*
		@foreach(var sprite in currentsprites)
		{
							< DropdownItem Clicked = "@(() => OnSpriteSelect(@sprite.filename))" >< img src = "@(ConvertImageBytes(sprite.iconimg))" class="sprite-icon" /> @sprite.name</DropdownItem>
						}*/
		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void toolStripStatusLabel1_Click(object sender, EventArgs e)
		{

		}
		private string ConvertImageBytes(byte[] image)
		{
			var imagesrc = Convert.ToBase64String(image);
			return string.Format("data:image/png;base64,{0}", imagesrc);
		}
		/*
		private string CurrentSpriteSelection()
		{
			if (preferences.PlayerSprite == "default")
			{
				return "<img src=\"skin-icons/default-benjamin-icon.png\" class=\nsprite-icon\n /> Default/Benjamin";
			}
			else if (preferences.PlayerSprite == "random")
			{
				return "Random";

			}
			else if (preferences.PlayerSprite == "custom")
			{
				return "Upload Custom Sprites";
			}
			else
			{
				var currentsprite = playersprites.sprites.Where(s => s.filename == preferences.PlayerSprite).ToList();
				if (currentsprite.Any())
				{
					return "<img src=\"" + ConvertImageBytes(currentsprite.First().iconimg) + "\" class=\nsprite-icon\n /> " + currentsprite.First().name;
				}
				else
				{
					return "Error";
				}
			}
		}*/

	}
}
