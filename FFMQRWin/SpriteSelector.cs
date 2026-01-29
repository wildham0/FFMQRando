using FFMQLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFMQRWin
{
	public class SpriteSelector : ComboBox
	{
		private PlayerSprites playerSprites;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentSprite { get => GetCurrentSprite(); set => SetPlayerSprite(value); }
		private int randomIndex = 0;
		private int customIndex = 0;
		public SpriteSelector(PlayerSprites playersprites)
		{
			DrawMode = DrawMode.OwnerDrawFixed;
			DropDownStyle = ComboBoxStyle.DropDownList;
			playerSprites = playersprites;
			Items.Add("default");
			foreach (var sprite in playerSprites.sprites)
			{
				Items.Add(sprite);
			}
			randomIndex = Items.Count;
			Items.Add("random");
			customIndex = Items.Count;
			Items.Add("custom");
		}

		// Draw items
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			e.DrawBackground();
			e.DrawFocusRectangle();

			int index = (e.Index < 0) ? 0 : e.Index;

			if (index == 0)
			{
				Image ben;
				var assembly = Assembly.GetExecutingAssembly();
				string filepath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("default-benjamin-icon.png"));
				using (Stream benfile = assembly.GetManifestResourceStream(filepath))
				{
					ben = Image.FromStream(benfile);
				}

				/*
				FileStream stream = new FileStream("default-benjamin-icon.png", FileMode.Open);
				Image ben = Image.FromStream(stream);*/
				e.Graphics.DrawImage(ben, e.Bounds.Left, e.Bounds.Top, 16, 16);
				e.Graphics.DrawString("Default/Benjamin", e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + 18, e.Bounds.Top + 2);
			}
			else if (index == randomIndex)
			{
				e.Graphics.DrawString("Random", e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Top + 2);
			}
			else if (index == customIndex)
			{
				e.Graphics.DrawString("Upload Custom Sprites", e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Top + 2);
			}
			else
			{
				PlayerSprite currentSprite = playerSprites.sprites[index - 1];
				DropDownItem item = new DropDownItem(currentSprite.iconimg);
				e.Graphics.DrawImage(item.Image, e.Bounds.Left, e.Bounds.Top, 16, 16);
				e.Graphics.DrawString(currentSprite.name, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + 18, e.Bounds.Top + 2);
			}

			base.OnDrawItem(e);
		}

		private string GetCurrentSprite()
		{
			if (SelectedIndex == 0)
			{
				return "default";
			}
			else if (SelectedIndex == customIndex)
			{
				return "custom";
			}
			else if (SelectedIndex == randomIndex)
			{
				return "random";
			}
			else
			{
				return playerSprites.sprites[SelectedIndex - 1].filename;
			}
		}

		private void SetPlayerSprite(string value)
		{
			if (value == "default")
			{
				SelectedIndex = 0;
			}
			else if (value == "custom")
			{
				SelectedIndex = customIndex;
			}
			else if (value == "random")
			{
				SelectedIndex = randomIndex;
			}
			else
			{
				int index = playerSprites.sprites.FindIndex(s => s.filename == value);
				if (index < 0)
				{
					SelectedIndex = 0;
				}
				else
				{
					SelectedIndex = index + 1;
				}
			}
		}
	}
	public class DropDownItem
	{
		public string Value
		{
			get { return Value; }
			set { this.Value = value; }
		}
		private string value;
		public Image Image
		{
			get { return img; }
			set { img = value; }
		}
		private Image img;
		public DropDownItem() : this(new byte[0])
		{ }
		public DropDownItem(byte[] rawimage)
		{
			value = "";
			MemoryStream stream = new MemoryStream(rawimage);

			this.img = Image.FromStream(stream);

			//Graphics g = Graphics.FromImage(img);
			//g.Dra
			//Brush b = new SolidBrush(Color.FromName(val));
			//g.DrawRectangle()
		}
		public override string ToString()
		{
			return value;
		}
	}
}
