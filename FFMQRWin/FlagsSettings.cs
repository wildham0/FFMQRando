using FFMQLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FFMQRWin.Main;


namespace FFMQRWin
{
    public class FlagsSettings
    {
		private Dictionary<int, FlagCheckBox> checkBoxes;
		private Dictionary<int, FlagComboBox> comboBoxes;
		private Dictionary<int, Label> comboLabels;

		private Flags flags;
		//private Preferences preferences;
		private string typeString;
		private Panel contentPanel;
		private TextBox flagstringBox;

		private int maxIndex;
		private ContentModes mode;


		public FlagsSettings(Flags _flags, Panel _contentPanel, TextBox _flagstringBox)
		{
			checkBoxes = new();
			comboBoxes = new();
			comboLabels = new();
			flags = _flags;
			contentPanel = _contentPanel;
			typeString = "Flags";
			flagstringBox = _flagstringBox;
			mode = ContentModes.Randomizer;
		}
		public void Initialize()
		{
			var properties = typeof(Flags).GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var flagproperties = properties.Where(p => p.CanWrite).OrderBy(p => p.Name).ToList();

			int yOffset = LayoutValues.yInitialOffset;
			int index = 0;

			foreach (var p in flagproperties)
			{
				string bindingString = typeString + "." + p.Name;

				if (p.PropertyType == typeof(bool))
				{
					FlagCheckBox checkBox = new();
					checkBox.Name = bindingString;
					checkBox.Text = Regex.Replace(p.Name, @"\B[A-Z]", " $0");
					checkBox.Flags = flags;
					checkBox.FlagstringBox = flagstringBox;
					checkBox.Checked = flags.GetToggleFlag(p.Name);
					checkBox.Visible = true;
					checkBox.Location = new Point(LayoutValues.xOffset, yOffset);
					checkBox.Width = LayoutValues.checkBoxWidth;

					yOffset += LayoutValues.yIncrement;
					contentPanel.Controls.Add(checkBox);
					checkBoxes.Add(index, checkBox);
					index++;
				}
				else if (p.PropertyType.IsEnum)
				{
					FlagComboBox comboBox = new();
					Label comboLabel = new();

					comboBox.Name = bindingString;
					comboBox.Flags = flags;
					comboBox.FlagstringBox = flagstringBox;
					//checkBox.Checked = true;
					comboBox.Visible = true;
					comboBox.Location = new Point(LayoutValues.xOffset + LayoutValues.comboLabelWidth, yOffset);
					comboBox.Width = LayoutValues.comboBoxWidth;

					comboLabel.Name = "label." + p.Name;
					comboLabel.Text = Regex.Replace(p.Name, @"\B[A-Z]", " $0");
					comboLabel.Visible = true;
					comboLabel.Location = new Point(LayoutValues.xOffset, yOffset);
					comboLabel.Width = LayoutValues.comboLabelWidth;


					yOffset += LayoutValues.yIncrement;

					var itemValues = Enum.GetValues(p.PropertyType);
					foreach (var item in itemValues)
					{
						var type = item.GetType();
						var memberInfo = type.GetMember(item.ToString());
						var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
						var description = attributes.Length > 0 ? ((DescriptionAttribute)attributes[0]).Description : item.ToString();
						//values.Add(description);
						comboBox.Items.Add(description);
					}

					comboBox.SelectedIndex = flags.GetEnumFlag(p.Name);
					comboBox.Text = comboBox.Items[comboBox.SelectedIndex].ToString();


					contentPanel.Controls.Add(comboBox);
					contentPanel.Controls.Add(comboLabel);
					comboBoxes.Add(index, comboBox);
					comboLabels.Add(index, comboLabel);
					index++;
				}
			}

			maxIndex = index;
		}
		public void ShowList(string filter)
		{
			int yOffset = LayoutValues.yInitialOffset;

			for (int i = 0; i < maxIndex; i++)
			{
				if (checkBoxes.TryGetValue(i, out var checkbox))
				{
					//flagFilterBox.Text
					if (String.IsNullOrEmpty(filter) || (checkbox.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
					{
						checkbox.Visible = true;
						checkbox.Location = new Point(LayoutValues.xOffset, yOffset);
						yOffset += LayoutValues.yIncrement;
					}
					else
					{
						checkbox.Visible = false;
					}
				}
				else if (comboBoxes.TryGetValue(i, out var combobox))
				{
					if (String.IsNullOrEmpty(filter) || (combobox.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
					{
						combobox.Visible = true;
						combobox.Location = new Point(LayoutValues.xOffset + LayoutValues.comboLabelWidth, yOffset);
						comboLabels[i].Visible = true;
						comboLabels[i].Location = new Point(LayoutValues.xOffset, yOffset);
						yOffset += LayoutValues.yIncrement;
					}
					else
					{
						combobox.Visible = false;
						comboLabels[i].Visible = false;
					}
				}
			}
		}
		public void HideList()
		{
			for (int i = 0; i < maxIndex; i++)
			{
				if (checkBoxes.TryGetValue(i, out var checkbox))
				{
					checkbox.Visible = false;
				}
				else if (comboBoxes.TryGetValue(i, out var combobox))
				{
					combobox.Visible = false;
					comboLabels[i].Visible = false;
				}
			}
		}
		public void UpdateValues()
		{
			if (mode == ContentModes.Randomizer)
			{
				UpdateFlagValues();
			}
		}
		private void UpdateFlagValues()
		{
			foreach (var checkbox in checkBoxes.Values)
			{
				var result = flags.GetToggleFlag(checkbox.Name.Split(".")[1]);
				checkbox.Checked = result;
			}

			foreach (var combobox in comboBoxes.Values)
			{
				combobox.SelectedIndex = flags.GetEnumFlag(combobox.Name.Split(".")[1]);
			}
		}
	}
}
