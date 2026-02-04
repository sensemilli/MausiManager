using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Encodings;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class PnBrowser
{
	private readonly ILogCenterService _logCenterService;

	private Window _parent;

	public PnBrowser(ILogCenterService logCenterService)
	{
		_logCenterService = logCenterService;
	}

	private static string[] ParseArguments(string commandLine)
	{
		char[] array = commandLine.ToCharArray();
		bool flag = false;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '"')
			{
				flag = !flag;
			}
			if (!flag && array[i] == ' ')
			{
				array[i] = '\n';
			}
		}
		return new string(array).Split('\n');
	}

	public string InternalExeAction(string name, string type, Window parent)
	{
		_parent = parent;
		Init();
		string[] param = new string[5]
		{
			string.Empty,
			string.Empty,
			string.Empty,
			type,
			name
		};
		return FileSave(param, ret: true);
	}

	public void Action(string param_str, Window parent)
	{
		_parent = parent;
		Init();
		string[] array = ParseArguments(param_str.Trim(' '));
		if (array.GetLength(0) < 4)
		{
			return;
		}
		string text = array[1];
		if (!(text == "F"))
		{
			if (text == "S")
			{
				if (array[2] == "1")
				{
					FileSave(array, ret: false);
				}
				else
				{
					SelectFolder(array);
				}
			}
		}
		else
		{
			FileLoad(array);
		}
	}

	private void Init()
	{
		try
		{
			if (File.Exists("PNBROWSER"))
			{
				File.Delete("PNBROWSER");
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		try
		{
			if (!File.Exists("PNBROW"))
			{
				File.Copy(Environment.GetEnvironmentVariable("PNDRIVE") + "\\u\\pn\\gfiles\\PNBROWSER.TXT", "PNBROW");
			}
		}
		catch (Exception e2)
		{
			_logCenterService.CatchRaport(e2);
		}
	}

	private void SelectFolder(string[] param)
	{
		List<Tuple<string, string>> pnBrow = GetPnBrow(param[3]);
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		string pnBrowText = GetPnBrowText(pnBrow, "_SAVE_DIR");
		if (pnBrowText != string.Empty)
		{
			folderBrowserDialog.SelectedPath = pnBrowText;
		}
		pnBrowText = GetPnBrowText(pnBrow, "_TITLE");
		if (pnBrowText != string.Empty)
		{
			folderBrowserDialog.Description = pnBrowText;
		}
		string currentDirectory = Environment.CurrentDirectory;
		DialogResult num = folderBrowserDialog.ShowDialog();
		Environment.CurrentDirectory = currentDirectory;
		if (num == DialogResult.OK)
		{
			File.WriteAllLines("PNBROWSER", new string[1] { folderBrowserDialog.SelectedPath }, CurrentEncoding.SystemEncoding);
		}
	}

	private string FileSave(string[] param, bool ret)
	{
		List<Tuple<string, string>> pnBrow = GetPnBrow(param[3]);
		Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
		string pnBrowText = GetPnBrowText(pnBrow, "_SAVE_DIR");
		if (pnBrowText != string.Empty && Directory.Exists(pnBrowText))
		{
			saveFileDialog.InitialDirectory = pnBrowText;
		}
		pnBrowText = GetPnBrowText(pnBrow, "_TITLE");
		if (pnBrowText != string.Empty)
		{
			saveFileDialog.Title = pnBrowText;
		}
		saveFileDialog.Filter = GetFilter(pnBrow);
		saveFileDialog.FileName = param[4].Replace("\"", string.Empty);
		string currentDirectory = Environment.CurrentDirectory;
		bool? flag;
		try
		{
			flag = saveFileDialog.ShowDialog();
		}
		catch
		{
			Environment.CurrentDirectory = currentDirectory;
			return string.Empty;
		}
		Environment.CurrentDirectory = currentDirectory;
		if (flag == true)
		{
			try
			{
				File.WriteAllLines("PNBROWSER", saveFileDialog.FileNames.ToArray(), CurrentEncoding.SystemEncoding);
			}
			catch (Exception e)
			{
				_logCenterService.CatchRaport(e);
			}
		}
		if (ret && flag == true)
		{
			return saveFileDialog.FileName;
		}
		return string.Empty;
	}

	private void FileLoad(string[] param)
	{
		List<Tuple<string, string>> pnBrow = GetPnBrow(param[3]);
		Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
		openFileDialog.Multiselect = param[2] == "2";
		string pnBrowText = GetPnBrowText(pnBrow, "_INIT_DIR");
		if (pnBrowText != string.Empty && Directory.Exists(pnBrowText))
		{
			openFileDialog.InitialDirectory = pnBrowText;
		}
		pnBrowText = GetPnBrowText(pnBrow, "_TITLE");
		if (pnBrowText != string.Empty)
		{
			openFileDialog.Title = pnBrowText;
		}
		openFileDialog.Filter = GetFilter(pnBrow);
		string currentDirectory = Environment.CurrentDirectory;
		bool? flag;
		try
		{
			flag = openFileDialog.ShowDialog();
		}
		catch
		{
			Environment.CurrentDirectory = currentDirectory;
			return;
		}
		Environment.CurrentDirectory = currentDirectory;
		if (flag == true)
		{
			try
			{
				File.WriteAllLines("PNBROWSER", openFileDialog.FileNames.ToArray(), CurrentEncoding.SystemEncoding);
			}
			catch (Exception e)
			{
				_logCenterService.CatchRaport(e);
			}
		}
	}

	private string GetFilter(List<Tuple<string, string>> pnbrow)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Tuple<string, string> item in pnbrow)
		{
			if (item.Item1.Contains("FILTER"))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("|");
				}
				stringBuilder.Append(item.Item2.Substring(0, 40).Trim(' '));
				stringBuilder.Append("|");
				stringBuilder.Append(item.Item2.Substring(40).Trim(' '));
			}
		}
		return stringBuilder.ToString();
	}

	private string GetPnDrive()
	{
		string text = Environment.GetEnvironmentVariable("PNDRIVE");
		if (text == string.Empty)
		{
			return string.Empty;
		}
		if (text.Length > 2 && text[text.Length - 1] == '\\')
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}

	private string GetPnHomeDrive()
	{
		string text = Environment.GetEnvironmentVariable("PNHOMEDRIVE");
		if (text == string.Empty)
		{
			return string.Empty;
		}
		if (text.Length > 2 && text[text.Length - 1] == '\\')
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}

	private string GetPnBrowText(List<Tuple<string, string>> pnbrow, string p)
	{
		foreach (Tuple<string, string> item in pnbrow)
		{
			if (item.Item1.Contains(p))
			{
				return item.Item2.Replace("%PNDRIVE%", GetPnDrive()).Replace("%PNHOMEDRIVE%", GetPnHomeDrive()).Replace("%PNHOMEPATH%", Environment.GetEnvironmentVariable("PNHOMEPATH"))
					.Replace("%ARDRIVE%", Environment.GetEnvironmentVariable("ARDRIVE"));
			}
		}
		return string.Empty;
	}

	private List<Tuple<string, string>> GetPnBrow(string p)
	{
		List<Tuple<string, string>> list = new List<Tuple<string, string>>();
		try
		{
			string[] array = File.ReadAllLines("PNBROW", CurrentEncoding.SystemEncoding);
			foreach (string text in array)
			{
				if (text.Length > 21 && text.Substring(0, p.Length) == p)
				{
					list.Add(new Tuple<string, string>(text.Substring(0, 20).Trim(' '), text.Substring(20).Trim(' ')));
				}
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		return list;
	}
}
