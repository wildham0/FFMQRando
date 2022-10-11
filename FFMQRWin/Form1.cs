using System;
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
            comboBox1.DataSource = Enum.GetValues<FFMQLib.EnemiesDensity>();
            comboBox2.DataSource = Enum.GetValues<FFMQLib.BattlesQty>();

            trackBar1.TickFrequency = 1;
            trackBar1.Maximum = Enum.GetValues<FFMQLib.EnemiesScaling>().Length - 1;
            trackBar1.Minimum = 0;
            trackBar1.Value = (int)flags.EnemiesScaling;
            
            trackBar2.TickFrequency = 1;
            trackBar2.Maximum = Enum.GetValues<FFMQLib.EnemiesScalingSpread>().Length - 1;
            trackBar2.Minimum = 0;
            trackBar2.Value = (int)flags.EnemiesScalingSpread;

            trackBar3.TickFrequency = 1;
            trackBar3.Maximum = Enum.GetValues<FFMQLib.LevelingCurve>().Length - 1;
            trackBar3.Minimum = 0;
            trackBar3.Value = (int)flags.LevelingCurve;

            textBox3.Text = flags.GenerateFlagString();
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
            newRom.Randomize(seed, flags, preferences);

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
                trackBar1.Value = (int)flags.EnemiesScaling;
                label6.Text = "Enemies Scaling: " + flags.EnemiesScaling.GetDescription();
                trackBar2.Value = (int)flags.EnemiesScalingSpread;
                label7.Text = "Scaling Spread: " + flags.EnemiesScalingSpread.GetDescription();
                trackBar3.Value = (int)flags.LevelingCurve;
                label8.Text = "Leveling Curve: " + flags.LevelingCurve.GetDescription();
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
            flags.EnemiesScaling = (EnemiesScaling)((TrackBar)sender).Value;
            label6.Text = "Enemies Scaling: " + flags.EnemiesScaling.GetDescription();
            textBox3.Text = flags.GenerateFlagString();
        }
        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            flags.EnemiesScalingSpread = (EnemiesScalingSpread)((TrackBar)sender).Value;
            label7.Text = "Scaling Spread: " + flags.EnemiesScalingSpread.GetDescription();
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
    }
}
