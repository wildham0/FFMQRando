using FFMQLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FFMQRWin
{
	public struct FilePath
	{
		public string Path;
		public string Name;
		public string Type;
		public string Full => Path + Name + "." + Type;

		public FilePath(string fullpath)
		{
			if (fullpath != "")
			{
				var filePath = fullpath.Split('\\');
				var pathindex = fullpath.LastIndexOf("\\");
				var nameindex = fullpath.LastIndexOf(".");
				Path = fullpath[0..(pathindex + 1)];
				Name = fullpath[(pathindex + 1)..(nameindex)];
				Type = fullpath[(nameindex + 1)..];
			}
			else
			{
				Path = "";
				Name = "";
				Type = "";
			}
		}
	}


	public static class FileManager
	{
		public static bool LoadCustomSprites(string customSpriteLocation, ref byte[] customSprite, ref string message) 
		{
			if (customSpriteLocation != "")
			{
				FileStream fileStream;
				try
				{
					fileStream = new FileStream(customSpriteLocation, FileMode.Open);
					customSprite = (new BinaryReader(fileStream)).ReadBytes((int)fileStream.Length);
				}
				catch (Exception ex)
				{
					message = "Custom Sprites: Couldn't open file.\n" + ex.Message;
					return false;
				}
			}
			else
			{
				message = "No Custom Sprites.";
				return false;
			}

			message = "Custom Sprites: Sprite file opened successfuly.";
			return true;
		}

		public static bool LoadRom(string rompath, FFMQRom rom, ref string message)
		{
			if (rompath != "")
			{
				FileStream fileStream;
				try
				{
					fileStream = new FileStream(rompath, FileMode.Open);
					rom.Load(fileStream);
					fileStream.Close();
				}
				catch (Exception ex)
				{
					MessageBox.Show("ROM File Error.\n" + ex.Message);
					message = "ROM File Error.\n" + ex.Message;
					return false;
				}

				if (rom.Validate())
				{
					rom.BackupOriginalData();
					message = "ROM file loaded successfully.";
					return true;
				}
				else
				{
					message = "Invalid ROM file. Please use NA Rev1.0 or 1.1 rom.";
					return false;
				}
			}

			message = "No path";
			return false;
		}

		public static bool LoadAPMQFile(FilePath apmqpath, ApConfigs apconfigs, Flags flags, ref string message)
		{
			string yamlPreset = "";

			try
			{
				var fileStream = new FileStream(apmqpath.Full, FileMode.Open);
				MemoryStream memZip = new();
				fileStream.CopyTo(memZip);
				fileStream.Close();

				apconfigs.FileName = apmqpath.Name;
				using (ZipArchive configContainer = new ZipArchive(memZip))
				{
					foreach (var file in configContainer.Entries)
					{
						if (file.Name == "itemplacement.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								apconfigs.ItemPlacementYaml = streamReader.ReadToEnd();
							}
						}
						else if (file.Name == "startingitems.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								apconfigs.StartingItemsYaml = streamReader.ReadToEnd();
							}
						}
						else if (file.Name == "flagset.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								yamlPreset = streamReader.ReadToEnd();
							}
						}
						else if (file.Name == "setup.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								apconfigs.SetupYaml = streamReader.ReadToEnd();
							}
						}
						else if (file.Name == "rooms.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								apconfigs.RoomsYaml = streamReader.ReadToEnd();
							}
						}
						else if (file.Name == "externalplacement.yaml")
						{
							using (var streamReader = new StreamReader(file.Open(), Encoding.UTF8))
							{
								apconfigs.ExternalPlacementYaml = streamReader.ReadToEnd();
							}
						}

					}
				}

				bool success = apconfigs.ProcessYaml();
				if (success)
				{
					try
					{
						flags.ReadApYaml(yamlPreset, apconfigs);
					}
					catch (Exception ex)
					{
						message = "APMQ file error. Couldn't process flagset.\n" + ex.Message;
						MessageBox.Show(message);
						return false;
					}
				}
				else
				{
					message = "APMQ file error. Couldn't process file.";
					MessageBox.Show(message);
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				message = "APMQ file error.\n" + ex.Message;
				MessageBox.Show(message);
				return false;
			}
		}

		public static FilePath OpenFileDialog(string type, string defaultlocation)
		{
			var fileContent = string.Empty;
			FilePath filePath;
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = defaultlocation;
				openFileDialog.Filter = $"{type} files (*.{type})|*.{type}|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					//Get the path of specified file
					filePath = new FilePath(openFileDialog.FileName);
				}
				else
				{
					filePath = new FilePath("");
				}
			}

			return filePath;
		}
	}
}
