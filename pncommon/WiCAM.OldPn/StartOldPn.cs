using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiCAM.OldPn.Enums;
using WiCAM.Pn4000;
using WiCAM.Pn4000.Contracts.PnPathServices;

namespace WiCAM.OldPn;

public class StartOldPn
{
	public static StarOldPnAnswer Start(IPnPathService pnPathService, string configID)
	{
		string pnMasterOrDrive = pnPathService.PnMasterOrDrive;
		string text = pnPathService.BuildPath(pnMasterOrDrive, $"\\u\\pn\\gfiles\\OPNSTART({configID}).xml");
		if (!File.Exists(text))
		{
			return StarOldPnAnswer.ERROR_LOADCONFIG;
		}
		StartOldPnConfiguration startOldPnConfiguration;
		try
		{
			startOldPnConfiguration = StartOldPnConfiguration.Load(text);
		}
		catch
		{
			return StarOldPnAnswer.ERROR_LOADCONFIG;
		}
		string text2 = pnPathService.BuildPath(startOldPnConfiguration.PNHOMEDRIVE, startOldPnConfiguration.PNHOMEPATH);
		if (!Directory.Exists(text2))
		{
			return StarOldPnAnswer.ERROR_NOPN;
		}
		try
		{
			foreach (string fromNewHomeToOldFileCopy in startOldPnConfiguration.FromNewHomeToOldFileCopyList)
			{
				File.Copy(fromNewHomeToOldFileCopy, Path.Combine(text2, fromNewHomeToOldFileCopy), overwrite: true);
			}
		}
		catch
		{
			return StarOldPnAnswer.ERROR_NOFILETOCOPY;
		}
		ProcessStartInfo startInfo = new ProcessStartInfo();
		startInfo.EnvironmentVariables["PNDRIVE"] = startOldPnConfiguration.PNDRIVE;
		startInfo.EnvironmentVariables["PNHOMEDRIVE"] = startOldPnConfiguration.PNHOMEDRIVE;
		startInfo.EnvironmentVariables["ARDRIVE"] = startOldPnConfiguration.ARDRIVE;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(pnPathService.BuildPath(startOldPnConfiguration.PNDRIVE, "\\u\\pn\\bin;"));
		stringBuilder.Append(Environment.GetEnvironmentVariable("PATH"));
		startInfo.EnvironmentVariables["Path"] = stringBuilder.ToString();
		startInfo.FileName = pnPathService.BuildPath(startOldPnConfiguration.PNDRIVE, "\\u\\pn\\run\\pn4.exe");
		startInfo.Arguments = startOldPnConfiguration.StartParameters;
		startInfo.WorkingDirectory = text2;
		startInfo.UseShellExecute = false;
		try
		{
			Process p = null;
			CancellationToken token = new CancellationTokenSource().Token;
			Task task = Task.Factory.StartNew(delegate
			{
				p = Process.Start(startInfo);
				while (!token.IsCancellationRequested && !p.HasExited)
				{
					p.WaitForExit(10);
				}
				if (!p.HasExited)
				{
					p.Kill();
				}
				p.Close();
			});
			while (!task.IsCompleted)
			{
				StartOldPn.DoModalEvants();
				Thread.Sleep(1);
			}
		}
		catch (Exception)
		{
			return StarOldPnAnswer.ERROR_EXECALLEXCEPTION;
		}
		try
		{
			foreach (string fromOldHomeToNewFileCopy in startOldPnConfiguration.FromOldHomeToNewFileCopyList)
			{
				File.Copy(Path.Combine(text2, fromOldHomeToNewFileCopy), fromOldHomeToNewFileCopy, overwrite: true);
			}
		}
		catch
		{
			return StarOldPnAnswer.ERROR_NOFILETOCOPYBACK;
		}
		return StarOldPnAnswer.OK;
	}

	public static void DoModalEvants()
	{
		User32Wrap.PumpMesseges();
	}
}
