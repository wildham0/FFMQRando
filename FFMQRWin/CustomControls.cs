using FFMQLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFMQRWin
{
	public class FlagCheckBox : CheckBox
	{
		public Flags Flags;
		public TextBox FlagstringBox;
		protected override void OnCheckedChanged(EventArgs e)
		{
			base.OnCheckedChanged(e);
			Flags.SetToggleFlag(base.Name.Split(".")[1], Checked);
			FlagstringBox.Text = Flags.GenerateFlagString();
		}
	}
	public class FlagComboBox : ComboBox
	{
		public Flags Flags;
		public TextBox FlagstringBox;
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			Flags.SetEnumFlag(base.Name.Split(".")[1], SelectedIndex);
			FlagstringBox.Text = Flags.GenerateFlagString();
		}
	}
}
