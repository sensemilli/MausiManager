#define TRACE
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace WiCAM.Pn4000.JobManager.LabelPrinter;

internal abstract class LabelPrinterBase
{
	private readonly string _labelPrinterBatchPath;

	protected LabelPrinterBase()
	{
		_labelPrinterBatchPath = Environment.ExpandEnvironmentVariables("%PNDRIVE%\\u\\pn\\bin\\labelprint.bat");
	}

	protected string BuildPartPath(string jobDataPath, string jobName, int number)
	{
		StringBuilder stringBuilder = new StringBuilder(1000);
		BuildJobDataPath(stringBuilder, jobDataPath, jobName);
		stringBuilder.Append("PRTLB_");
		stringBuilder.Append(number);
		stringBuilder.Append(".LAB 0");
		return stringBuilder.ToString();
	}

	protected string BuildPlatePath(string jobDataPath, string jobName, int number)
	{
		StringBuilder stringBuilder = new StringBuilder(1000);
		BuildJobDataPath(stringBuilder, jobDataPath, jobName);
		stringBuilder.Append("PLPLB_");
		stringBuilder.Append(number);
		stringBuilder.Append(".LAB 0");
		return stringBuilder.ToString();
	}

	private void BuildJobDataPath(StringBuilder sb, string jobDataPath, string jobName)
	{
		sb.Append(jobDataPath);
		sb.Append(Path.DirectorySeparatorChar);
		sb.Append(jobName);
		sb.Append(Path.DirectorySeparatorChar);
	}

	protected void ExecuteLabelPrinter(string processArguments)
	{
		if (File.Exists(_labelPrinterBatchPath))
		{
			ProcessStartInfo info = new ProcessStartInfo(_labelPrinterBatchPath, processArguments)
			{
				WindowStyle = ProcessWindowStyle.Hidden
			};
			ExecuteProcess(info);
		}
	}

	private void ExecuteProcess(ProcessStartInfo info)
	{
		using Process process = new Process();
		try
		{
			process.StartInfo = info;
			process.Start();
		}
		catch (Exception ex)
		{
			Trace.WriteLine(ex.Message);
		}
	}
}
