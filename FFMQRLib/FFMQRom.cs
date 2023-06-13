using RomUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FFMQLib
{
	public partial class FFMQRom : SnesRom
	{
		public async Task<bool> ValidateRom()
		{
			using (SHA256 hasher = SHA256.Create())
			{
				byte[] dataToHash = new byte[0x80000];
				Array.Copy(Data, dataToHash, 0x80000);

				// zero out benjamin's sprites
				for (int i = 0x21A20; i < (0x24A20 + 0x180 * 1); i++)
				{
					dataToHash[i] = 0;
				}

				for (int i = 0x24A20; i < (0x24A20 + 0x180 * 5); i++)
				{
					dataToHash[i] = 0;
				}

				// benjamin's palette
				for (int i = 0x3D826; i < (0x3D826 + 0x0E); i++)
				{
					dataToHash[i] = 0;
				}

				Blob hash = await Task.Run(() => hasher.ComputeHash(dataToHash));

				// Console.WriteLine(BitConverter.ToString(hash).Replace("-", ""));
				// if (hash == Blob.FromHex("F71817F55FEBD32FD1DCE617A326A77B6B062DD0D4058ECD289F64AF1B7A1D05")) unadultered hash

				if (hash == Blob.FromHex("92F625478568B1BE262E3F9D62347977CE7EE345E9FF353B4778E8560E16C7CA"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public override bool Validate()
		{
			using (SHA256 hasher = SHA256.Create())
			{
				byte[] dataToHash = new byte[0x80000];
				Array.Copy(Data, dataToHash, 0x80000);

				// zero out benjamin's sprites
				for (int i = 0x21A20; i < (0x24A20 + 0x180 * 1); i++)
				{
					dataToHash[i] = 0;
				}

				for (int i = 0x24A20; i < (0x24A20 + 0x180 * 5); i++)
				{
					dataToHash[i] = 0;
				}

				// benjamin's palette
				for (int i = 0x3D826; i < (0x3D826 + 0x0E); i++)
				{
					dataToHash[i] = 0;
				}

				Blob hash = hasher.ComputeHash(dataToHash);

				//Console.WriteLine(BitConverter.ToString(hash).Replace("-", ""));
				// if (hash == Blob.FromHex("F71817F55FEBD32FD1DCE617A326A77B6B062DD0D4058ECD289F64AF1B7A1D05")) unadultered SHA256 hash

				if (hash == Blob.FromHex("92F625478568B1BE262E3F9D62347977CE7EE345E9FF353B4778E8560E16C7CA"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		public bool IsEmpty()
		{
			if (Data == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public void Clear()
		{
			Data = null;
		}
		public Stream DataStream()
		{
			return new MemoryStream(Data);
		}

		public void Load(byte[] _data)
		{
			Data = new byte[0x80000];
			Array.Copy(_data, Data, 0x80000);
		}
		public byte[] DataReadOnly { get => Data; }
		public Stream SpoilerStream()
		{
			if (spoilers)
			{
				var stream = new MemoryStream();
				var writer = new StreamWriter(stream);
				
				writer.Write(spoilersText);
				writer.Flush();
				stream.Position = 0;
				return stream;
			}
			else
			{
				return null;
			}
		}
		public void PutInBank(int bank, int address, Blob data)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			Put(offset, data);
		}
		public Blob GetFromBank(int bank, int address, int length)
		{
			int offset = (bank * 0x8000) + (address - 0x8000);
			return Get(offset, length);
		}
		public int GetOffset(int bank, int address)
		{
			return (bank * 0x8000) + (address - 0x8000);
		}
		public void ExpandRom()
		{
			Blob newData = new byte[0x100000];
			Array.Copy(Data, newData, 0x80000);
			Data = newData;
		}
		public void BackupOriginalData()
		{
			originalData = new byte[0x80000];
			Array.Copy(Data, originalData, 0x80000);
		}
		public void RestoreOriginalData()
		{
			Data = new byte[0x80000];
			Array.Copy(originalData, Data, 0x80000);
		}
	}
}
