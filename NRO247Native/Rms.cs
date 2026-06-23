#define DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

public class Rms
{
	public static int status;

	public static sbyte[] data;

	public static string filename;

	public static void saveRMS(string filename, sbyte[] data)
	{
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			__saveRMS(filename, data);
		}
		else
		{
			_saveRMS(filename, data);
		}
	}

	public static sbyte[] loadRMS(string filename)
	{
		if (Thread.CurrentThread.Name == Main.mainThreadName)
		{
			return __loadRMS(filename);
		}
		return _loadRMS(filename);
	}

	public static string loadRMSString(string fileName)
	{
		if (NRO_v247.AutoLogin.IsEnabled)
		{
			if (fileName == "acc") return NRO_v247.AutoLogin.Account;
			if (fileName == "pass") return NRO_v247.AutoLogin.Password;
		}
		sbyte[] array = loadRMS(fileName);
		if (array == null)
		{
			return null;
		}
		DataInputStream dataInputStream = new DataInputStream(array);
		try
		{
			string result = dataInputStream.readUTF();
			dataInputStream.close();
			return result;
		}
		catch (Exception ex)
		{
			Cout.println(ex.StackTrace);
		}
		return null;
	}

	public static void saveRMSString(string filename, string data)
	{
		if (NRO_v247.AutoLogin.IsEnabled && (filename == "acc" || filename == "pass")) return;
		DataOutputStream dataOutputStream = new DataOutputStream();
		try
		{
			dataOutputStream.writeUTF(data);
			saveRMS(filename, dataOutputStream.toByteArray());
			dataOutputStream.close();
		}
		catch (Exception ex)
		{
			Cout.println(ex.StackTrace);
		}
	}

	private static void _saveRMS(string filename, sbyte[] data)
	{
		if (status != 0)
		{
			Debug.WriteLine("Cannot save RMS " + filename + " because current is saving " + Rms.filename);
			return;
		}
		Rms.filename = filename;
		Rms.data = data;
		status = 2;
		int i;
		for (i = 0; i < 500; i++)
		{
			Thread.Sleep(5);
			if (status == 0)
			{
				break;
			}
		}
		if (i == 500)
		{
			Debug.WriteLine("TOO LONG TO SAVE RMS " + filename);
		}
	}

	private static sbyte[] _loadRMS(string filename)
	{
		if (status != 0)
		{
			Debug.WriteLine("Cannot load RMS " + filename + " because current is loading " + Rms.filename);
			return null;
		}
		Rms.filename = filename;
		data = null;
		status = 3;
		int i;
		for (i = 0; i < 500; i++)
		{
			Thread.Sleep(5);
			if (status == 0)
			{
				break;
			}
		}
		if (i == 500)
		{
			Debug.WriteLine("TOO LONG TO LOAD RMS " + filename);
		}
		return data;
	}

	public static int loadRMSInt(string file)
	{
		if (NRO_v247.AutoLogin.IsEnabled && file == ServerListScreen.RMS_svselect)
		{
			return (NRO_v247.AutoLogin.server > 0) ? (NRO_v247.AutoLogin.server - 1) : 0;
		}
		sbyte[] array = loadRMS(file);
		return (array != null) ? array[0] : (-1);
	}

	public static void saveRMSInt(string file, int x)
	{
		if (NRO_v247.AutoLogin.IsEnabled && file == ServerListScreen.RMS_svselect) return;
		try
		{
			saveRMS(file, new sbyte[1] { (sbyte)x });
			if (file == ServerListScreen.RMS_svselect)
			{
				Debug.WriteLine(">>>>>>>>Save saveRMSInt: " + file + "  index:" + x);
			}
		}
		catch (Exception)
		{
		}
	}

	public static string GetiPhoneDocumentsPath()
	{
		string rmsPath = "RMS";

		try
		{
			if (!Directory.Exists(rmsPath))
				Directory.CreateDirectory(rmsPath);
        }
		catch (Exception ex)
		{
			Debug.WriteLine("Không thể tạo thư mục RMS: " + ex);
        }

		return rmsPath;
    }

	private static void __saveRMS(string filename, sbyte[] data)
	{
		string text = GetiPhoneDocumentsPath() + "/" + filename;
		FileStream fileStream = new FileStream(text, FileMode.Create);
		fileStream.Write(ArrayCast.cast(data), 0, data.Length);
		fileStream.Flush();
		fileStream.Close();
	}

	private static sbyte[] __loadRMS(string filename)
	{
		try
		{
			string fullPath = Path.Combine(GetiPhoneDocumentsPath(), filename);
			byte[] array = File.ReadAllBytes(fullPath);
			return ArrayCast.cast(array);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static void clearAll()
	{
		Cout.LogError3("clean rms");
		FileInfo[] files = new DirectoryInfo(GetiPhoneDocumentsPath() + "/").GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			fileInfo.Delete();
		}
	}

	public static void DeleteStorage(string path)
	{
		try
		{
			File.Delete(GetiPhoneDocumentsPath() + "/" + path);
		}
		catch (Exception)
		{
		}
	}

	public static void saveIP(string strID)
	{
		saveRMSString("NRIPlink", strID);
	}
}
