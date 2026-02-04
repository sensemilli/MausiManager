using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PnKiller : IPnKiller
{
	private readonly IPnPathService _pnPathService;

	public PnKiller(IPnPathService pnPathService)
	{
		this._pnPathService = pnPathService;
	}

	public void Kill(bool initialMultiTask)
	{
		this.KillAllPn("pn4");
		if (initialMultiTask)
		{
			this.KillMultitaskFromLatestSession("pn4");
		}
		this.RegisterAndCheck();
	}

	public void KillAllPnInSeparateFolder(string path, string key)
	{
		string[] files;
		try
		{
			files = Directory.GetFiles(path + "\\pn.reg\\");
		}
		catch (Exception)
		{
			return;
		}
		string[] array = files;
		foreach (string path2 in array)
		{
			try
			{
				int processId = Convert.ToInt32(Path.GetFileName(path2));
				Process process;
				try
				{
					process = Process.GetProcessById(processId);
				}
				catch
				{
					process = null;
				}
				process?.Kill();
			}
			catch (Exception)
			{
			}
		}
	}

	private void KillAllPn(string key)
	{
		string[] files;
		try
		{
			string path = this._pnPathService.PNHOMEDRIVE + this._pnPathService.PNHOMEPATH + "\\pn.reg\\";
			if (!Directory.Exists(path))
			{
				return;
			}
			files = Directory.GetFiles(path);
		}
		catch (Exception)
		{
			return;
		}
		List<string> list = new List<string>(files);
		string[] array = files;
		foreach (string text in array)
		{
			try
			{
				int num = Convert.ToInt32(Path.GetFileName(text));
				if (num == Process.GetCurrentProcess().Id)
				{
					list.Remove(text);
					continue;
				}
				Process process = null;
				try
				{
					Process[] processes = Process.GetProcesses();
					foreach (Process process2 in processes)
					{
						if (process2.Id == num)
						{
							process = process2;
						}
					}
				}
				catch
				{
					process = null;
				}
				if (process != null && !process.HasExited && File.ReadAllLines(text)[0].Contains(key) && process.ProcessName.ToLower() == key.ToLower() && Process.GetCurrentProcess().Id != num)
				{
					this.CheckForParametersTransfer(process);
					Process.GetCurrentProcess().Kill();
					while (true)
					{
						Task.Delay(1000);
					}
				}
				list.Remove(text);
				File.Delete(text);
			}
			catch (Exception)
			{
			}
		}
		foreach (string item in list)
		{
			try
			{
				int processId = Convert.ToInt32(Path.GetFileName(item));
				Process process3;
				try
				{
					process3 = Process.GetProcessById(processId);
				}
				catch
				{
					process3 = null;
				}
				process3?.Kill();
			}
			catch (Exception)
			{
			}
		}
	}

	private void CheckForParametersTransfer(Process proc)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = commandLineArgs;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == "-recentlyused")
			{
				try
				{
					File.WriteAllLines(WindowEvents.parametertransferfile, commandLineArgs);
					WindowEvents.PostMessage(proc.MainWindowHandle, 1024, new IntPtr(124), IntPtr.Zero);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void RegisterAndCheck()
	{
		try
		{
			Directory.CreateDirectory("pn.reg");
			string[] files = Directory.GetFiles(this._pnPathService.PNHOMEDRIVE + this._pnPathService.PNHOMEPATH + "\\pn.reg\\");
			foreach (string path in files)
			{
				try
				{
					int processId = Convert.ToInt32(Path.GetFileName(path));
					Process process;
					try
					{
						process = Process.GetProcessById(processId);
					}
					catch
					{
						process = null;
					}
					if (process == null || process.HasExited)
					{
						File.Delete(path);
					}
					else if (File.ReadAllLines(path)[0] != process.ProcessName)
					{
						File.Delete(path);
					}
				}
				catch (Exception)
				{
				}
			}
			Process currentProcess = Process.GetCurrentProcess();
			StreamWriter streamWriter = new StreamWriter("pn.reg\\" + currentProcess.Id);
			streamWriter.WriteLine(currentProcess.ProcessName);
			string text = string.Empty;
			files = Environment.GetCommandLineArgs();
			for (int i = 0; i < files.Length; i++)
			{
				text = text + files[i] + " ";
			}
			streamWriter.WriteLine(text);
			streamWriter.Close();
		}
		catch (Exception)
		{
		}
	}

	public void Unregister()
	{
		string path = string.Concat(str3: Process.GetCurrentProcess().Id.ToString(), str0: this._pnPathService.PNHOMEDRIVE, str1: this._pnPathService.PNHOMEPATH, str2: "\\pn.reg\\");
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception)
			{
			}
		}
	}

	private void KillMultitaskFromLatestSession(string key)
	{
		string path = "MultiTask\\";
		if (!Directory.Exists(path))
		{
			return;
		}
		DirectoryInfo[] directories = new DirectoryInfo(path).GetDirectories();
		foreach (DirectoryInfo directoryInfo in directories)
		{
			this.KillAllPnInSeparateFolder(directoryInfo.FullName, key);
			string path2 = directoryInfo.FullName + "\\MT_STATUS.xml";
			try
			{
				if (File.Exists(path2))
				{
					File.Delete(path2);
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
