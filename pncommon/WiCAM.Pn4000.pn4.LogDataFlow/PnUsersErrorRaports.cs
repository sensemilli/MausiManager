using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WiCAM.Pn4000.Contracts.LogCenterServices.Enum;

namespace WiCAM.Pn4000.pn4.LogDataFlow;

public class PnUsersErrorRaports
{
	private const string _errorFolder = "pn.error4";

	private const int _keepForDays = 7;

	private int count;

	private Exception last_exception;

	public void ClearHistory()
	{
		try
		{
			if (!Directory.Exists("pn.error4"))
			{
				return;
			}
			string[] files = Directory.GetFiles("pn.error4", "*.txt");
			foreach (string path in files)
			{
				if (File.GetLastWriteTime(path) < DateTime.Now - TimeSpan.FromDays(7))
				{
					File.Delete(path);
				}
			}
		}
		catch
		{
		}
	}

    public string ErrorRaport(string message, ErrorLevel errorLevel, Exception exception = null)
    {
        // Early returns for rate limiting and duplicate exceptions
        if (count > 25)
        {
            return string.Empty;
        }

        if (last_exception != null && exception?.StackTrace == last_exception.StackTrace)
        {
            return string.Empty;
        }

        last_exception = exception;
        count++;

        try
        {
            // Generate error report content
            var reportLines = new List<string>
        {
            $"Message: {message}"
        };

            // Add exception details if present
            if (exception != null)
            {
                AppendExceptionDetails(reportLines, exception);
            }

            // Create error report file
            Directory.CreateDirectory(_errorFolder);
            string fileName = GenerateErrorFileName(errorLevel, DateTime.Now);
            string fullPath = Path.Combine(_errorFolder, fileName);

            File.WriteAllLines(fullPath, reportLines, Encoding.UTF8);
            return fullPath;
        }
        catch
        {
            return string.Empty;
        }
    }

    private void AppendExceptionDetails(List<string> reportLines, Exception exception)
    {
        int depth = 0;
        while (exception != null)
        {
            string prefix = depth == 0 ? "Exception " : $"InnerException({depth}) ";

            reportLines.AddRange(new[]
            {
            $"{prefix}Source: {exception.Source}",
            $"{prefix}Message: {exception.Message}",
            $"{prefix}StackTrace:\r\n {exception.StackTrace}",
            string.Empty // Empty line between exceptions
        });

            exception = exception.InnerException;
            depth++;
        }
    }

    private string GenerateErrorFileName(ErrorLevel errorLevel, DateTime timestamp)
    {
        return $"Raport_{errorLevel}_{timestamp:yyyy_MM_dd_HH_mm_ss_fff}.txt";
    }
}
