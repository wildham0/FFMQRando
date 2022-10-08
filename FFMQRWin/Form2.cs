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
            var interpreter = new ScriptsInterpreter(textBox3.Text, Convert.FromHexString(textBox2.Text.Replace("\n", "").Replace("\r", "")).ToList());
            textBox1.Text = interpreter.TextualData.Replace("\n", Environment.NewLine);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
