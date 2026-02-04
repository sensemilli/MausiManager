using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiCAM.Pn4000.Contracts.LogCenterServices;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public class PnExternalCall
{
	public static bool Start(string command, ILogCenterService logCenterService)
	{
		return PnExternalCall.Start(command, -1, logCenterService);
	}

	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static bool Start(string command, int cycle_limit, ILogCenterService logCenterService)
	{
		try
		{
			ProcessStartInfo pri = new ProcessStartInfo("cmd.exe", "/C " + command)
			{
				WorkingDirectory = Directory.GetCurrentDirectory(),
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Normal
			};
			Process p = null;
			Task task = Task.Factory.StartNew(delegate
			{
				p = Process.Start(pri);
				p.WaitForExit();
				p.Close();
			});
			int num = 0;
			while (!task.IsCompleted)
			{
				User32Wrap.PumpMesseges();
				Thread.Sleep(1);
				num++;
				if (cycle_limit > 0 && num > cycle_limit && p != null)
				{
					p.Kill();
					return false;
				}
			}
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
			return false;
		}
		return true;
	}

	[PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
	[PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
	public static bool StartDirectlyWithTimeLimit(string command, string parameters, int cycle_limit, ILogCenterService logCenterService)
	{
		try
		{
			ProcessStartInfo pri = new ProcessStartInfo(command, parameters)
			{
				WorkingDirectory = Directory.GetCurrentDirectory(),
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Normal
			};
			Process p = null;
			Task task = Task.Factory.StartNew(delegate
			{
				p = Process.Start(pri);
				p.WaitForExit();
				p.Close();
			});
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!task.IsCompleted)
			{
				if (!User32Wrap.PumpMesseges())
				{
					Thread.Sleep(1);
				}
				if (stopwatch.ElapsedMilliseconds > cycle_limit && p != null)
				{
					stopwatch.Stop();
					p.Kill();
					return false;
				}
			}
			stopwatch.Stop();
		}
		catch (Exception e)
		{
			logCenterService.CatchRaport(e);
			return false;
		}
		return true;
	}

	private static string GetArgs(string[] command)
	{
		string text = " ";
		for (int i = 1; i < command.Length; i++)
		{
			string text2 = command[i];
			text = ((!text2.Contains(' ')) ? (text + text2 + " ") : (text + "\"" + text2 + "\" "));
		}
		return text;
	}

	private static string GetProcess(string[] command)
	{
		if (command.Length == 0)
		{
			return string.Empty;
		}
		string text = command[0];
		string extension = Path.GetExtension(text);
		if (extension != string.Empty && File.Exists(text))
		{
			return text;
		}
		string text2 = text + ".bat";
		if (File.Exists(text2))
		{
			return text2;
		}
		text2 = text + ".exe";
		if (File.Exists(text2))
		{
			return text2;
		}
		if (extension != string.Empty)
		{
			text2 = PnExternalCall.GetFullPath(text);
			if (text2 != null)
			{
				return text2;
			}
		}
		text2 = PnExternalCall.GetFullPath(text + ".bat");
		if (text2 != null)
		{
			return text2;
		}
		text2 = PnExternalCall.GetFullPath(text + ".exe");
		if (text2 != null)
		{
			return text2;
		}
		return null;
	}

	public static string GetFullPath(string fileName)
	{
		if (File.Exists(fileName))
		{
			return Path.GetFullPath(fileName);
		}
		string[] array = Environment.GetEnvironmentVariable("PATH").Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string text = Path.Combine(array[i], fileName);
			if (File.Exists(text))
			{
				return text;
			}
		}
		return null;
	}

	private static string[] SplitCommandLineArgument(string argumentString)
	{
		StringBuilder stringBuilder = new StringBuilder(argumentString);
		bool flag = false;
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (stringBuilder[i] == '"')
			{
				flag = !flag;
			}
			if (stringBuilder[i] == ' ' && !flag)
			{
				stringBuilder[i] = '\n';
			}
		}
		string[] array = stringBuilder.ToString().Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = PnExternalCall.RemoveMatchingQuotes(array[j]);
		}
		return array;
	}

	public static string RemoveMatchingQuotes(string stringToTrim)
	{
		int num = stringToTrim.IndexOf('"');
		int num2 = stringToTrim.LastIndexOf('"');
		while (num != num2)
		{
			stringToTrim = stringToTrim.Remove(num, 1);
			stringToTrim = stringToTrim.Remove(num2 - 1, 1);
			num = stringToTrim.IndexOf('"');
			num2 = stringToTrim.LastIndexOf('"');
		}
		return stringToTrim;
	}
}
