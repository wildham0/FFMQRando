using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FFMQLib;

namespace FFMQRWin
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var address = textBox3.Text;
            string addressmask = "008000";
            if (address.Length < 6)
            {
                address = addressmask.Substring(0, 6 - address.Length) + address;
            }
            else
            {
                address = address.Substring(0, 6);
            }
            
            var interpreter = new ScriptsInterpreter(address, Convert.FromHexString(textBox2.Text.Replace("\n", "").Replace("\r", "").Replace(" ","")).ToList());
            textBox1.Text = interpreter.TextualData.Replace("\n", Environment.NewLine);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
