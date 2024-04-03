using FFMQLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace FFMQRWin
{
	public enum PatchAction
	{ 
		Delete,
		Insert,
		Replace,
		Offset,
		IncrementWord,
		IncreaseWord,
		DecreaseWord,
	}
	public struct PatchInstruction
	{ 
		public int Address { get; set; }
		public PatchAction Action { get; set; }
		public int Value { get; set; }
		public byte[] Chunk { get; set; }
	}
	
	public partial class RomComparer : Form
	{

		public string PatchBank0(int bank)
		{
			var bankdata10 = rom10.GetFromBank(bank, 0x8000, 0x8000);
			int offset = 0;
			byte[] newbank = new byte[0x8000];

			for (int i = 0x00; i < 0x8000; i++)
			{
				if (bank0instructions.TryGetValue(i, out var instruction))
				{
					if (instruction.Action == PatchAction.IncreaseWord)
					{
						int newvalue = bankdata10[i + offset] + instruction.Value;

						if (newvalue > 0xFF)
						{
							newbank[i] = (byte)(newvalue - 0x100);
							newbank[i + 1] = (byte)(bankdata10[i + 1 + offset] + 1);
							i++;
						}
						else
						{
							newbank[i] = (byte)(newvalue);
						}
					}
					else if (instruction.Action == PatchAction.DecreaseWord)
					{
						int newvalue = bankdata10[i + offset] - instruction.Value;

						if (newvalue < 0)
						{
							newbank[i] = (byte)(newvalue + 0x100);
							newbank[i + 1] = (byte)(bankdata10[i + 1 + offset] - 1);
							i++;
						}
						else
						{
							newbank[i] = (byte)(newvalue);
						}
					}
					else if (instruction.Action == PatchAction.Replace)
					{
						for (int j = 0; j < instruction.Chunk.Length; j++)
						{
							newbank[i + j] = instruction.Chunk[j];
						}

						i += instruction.Chunk.Length - 1;
					}
					else if (instruction.Action == PatchAction.Insert)
					{
						for (int j = 0; j < instruction.Chunk.Length; j++)
						{
							newbank[i + j] = instruction.Chunk[j];
						}

						i += instruction.Chunk.Length - 1;
						offset -= instruction.Chunk.Length;
					}
					else if (instruction.Action == PatchAction.Offset)
					{
						offset += instruction.Value;
						newbank[i] = bankdata10[i + offset];
					}
				}
				else
				{
					newbank[i] = bankdata10[i + offset];
				}
			}

			return Convert.ToHexString(newbank);
		}
		public Dictionary<int, PatchInstruction> bank0instructions = new()
		{
			{ 0x0573, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0585, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B68, new PatchInstruction() { Action = PatchAction.Insert, Chunk = new byte[] { 0xF2 } } },
			{ 0x0B6B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B6D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B6F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B71, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B73, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B75, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B77, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B79, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0B7B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0C3C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0C4A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0CDC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0DC5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0DDA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0E2A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0F67, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0FC2, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x0FE3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1082, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1157, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x131F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1323, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1325, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1327, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1329, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x132B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x132D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x132F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1331, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1333, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x143F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x14F1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x15F4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x16AE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x17B4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x18B7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x197E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B72, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B76, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B78, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B7A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B7C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B7E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B80, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B82, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B84, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1B86, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1BDA, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1C2D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1C66, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1CB4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1CEF, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1D4C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1DC8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E09, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E0D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E0F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E11, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E13, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E15, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E17, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E19, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E1B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E1D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E42, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E5E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1E7A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1EC3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1EF3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F60, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F72, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F86, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F8A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F8C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F8E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F90, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F92, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F94, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F96, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F98, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1F9A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x1FFE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x204D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x20AD, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x210E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x21A6, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x21D3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2246, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2299, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x229D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x229F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22A1, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22A3, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22A5, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22A7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22A9, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22AB, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x22AD, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x262D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2699, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x26AC, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x26F4, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x273F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2782, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x280A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x284D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2851, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2853, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2855, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2857, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2859, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x285B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x285D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x285F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2861, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2891, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2969, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x299F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2A49, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2B47, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2B97, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2BB8, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2C42, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2C76, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2D13, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2D60, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E4D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E51, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E53, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E55, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E57, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E59, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E5B, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E5D, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E5F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2E61, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2ED7, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2EEE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2F92, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x2FEE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3002, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3071, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x30AE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3100, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3171, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x320A, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x320C, new PatchInstruction() { Action = PatchAction.DecreaseWord, Value = 0x01 } },
			{ 0x320E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3210, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3212, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3214, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3216, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x325E, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x32EE, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x3310, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x336C, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
			{ 0x344E, new PatchInstruction() { Action = PatchAction.Offset, Value = 0x01 } },
			{ 0x344F, new PatchInstruction() { Action = PatchAction.IncreaseWord, Value = 0x01 } },
		};

		public FFMQRom rom10;
		public FFMQRom rom11;
		public string directoryPath10 = "";
		public string directoryPath11 = "";
		public RomComparer()
		{
			InitializeComponent();
		}
		private void button1_Click(object sender, EventArgs e)
		{

		}

		public string CompareRom()
		{
			int bank = 0x0F;
			bool dopatch = true;

			if (dopatch)
			{
				return PatchBank0(bank);
			}
			//FFMQRom rom10 = new();
			//FFMQRom rom11 = new();

			var bankdata10 = rom10.GetFromBank(bank, 0x8000, 0x8000);
			var bankdata11 = rom11.GetFromBank(bank, 0x8000, 0x8000);
			string message = "";


			Dictionary<int, int> jumpList = new()
			{
				
				{ 0x0B68, -0x01 },
				{ 0x344D, 0x01 },
				/*{ 0x3DE5, 0x18 },
				{ 0x3E81, 0x03 },
				{ 0x53A3, 0x01 },
				{ 0x53AE, 0x01 },
				{ 0x53B9, -0x1D },*/
			};

			message = "Starting..." + Environment.NewLine;
			int offset = 0x00;
			int lasti = 0;
			int differencerun = 0;
			List<string> messagebatch = new();

			for (int i = 0x00; i < 0x8000; i++)
			{
				if (jumpList.TryGetValue(i, out int offsetbump))
				{
					offset += offsetbump;
				}

				if (i + offset >= 0x8000)
				{
					break;
				}
				byte byte11 = bankdata11[i];
				byte byte10 = bankdata10[i + offset];

				if (byte10 != byte11)
				{
					differencerun++;
					messagebatch.Add($"0x{i:X4}: {byte11:X2} > {byte10:X2} ... {(byte10 - byte11):X2}" + Environment.NewLine);
				}

				if (differencerun >= 5)
				{ 
					
				
				}

				lasti = i;
			}

			message += string.Join("", messagebatch) + "Done!";
			return message;
		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void button1_Click_1(object sender, EventArgs e)
		{
			var fileContent = string.Empty;
			//var filePath = string.Empty;
			rom10 = new FFMQRom();
			//var flags = new Flags();
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";
				openFileDialog.Filter = "sfc files (*.sfc)|*.sfc|smc files (*.smc)|*.smc|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					var filePath = openFileDialog.FileName.Split('\\');
					for (int i = 0; i < (filePath.Length - 1); i++)
					{
						directoryPath10 += filePath[i] + "\\";
					}

					//Read the contents of the file into a stream
					var fileStream = openFileDialog.OpenFile();

					rom10.Load(fileStream);

					textBox1.Text = directoryPath10;
					messageLabel.Text = "ROM 1.0 file loaded successfully.";
				}
			}
		}

		private void button4_Click(object sender, EventArgs e)
		{
			var result = CompareRom();
			resultTextBox.Text = result;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var fileContent = string.Empty;
			//var filePath = string.Empty;
			rom11 = new FFMQRom();
			//var flags = new Flags();
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = "c:\\";
				openFileDialog.Filter = "sfc files (*.sfc)|*.sfc|smc files (*.smc)|*.smc|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					var filePath = openFileDialog.FileName.Split('\\');
					for (int i = 0; i < (filePath.Length - 1); i++)
					{
						directoryPath11 += filePath[i] + "\\";
					}

					//Read the contents of the file into a stream
					var fileStream = openFileDialog.OpenFile();

					rom11.Load(fileStream);

					textBox2.Text = directoryPath10;
					messageLabel.Text = "ROM 1.0 file loaded successfully.";
				}
			}
		}
	}
}
