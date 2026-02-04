using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using WiCAM.Pn4000.PN3D.Zipper;

namespace WiCAM.Pn4000.PN3D;

public class EnvironmentConfig
{
	private class EnvironmentJson
	{
		public List<Tuple<string, string>> Variables { get; set; } = new List<Tuple<string, string>>();
	}

	public bool ReadConfig(string filenameConfig)
	{
		return this.ReadFile(filenameConfig);
	}

	public void ChangeEnvironment()
	{
		string text = "environment.json";
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length - 3; i++)
		{
			if (commandLineArgs[i].ToLowerInvariant() == "-wf7")
			{
				string zipFilepath = commandLineArgs[i + 1];
				string text2 = commandLineArgs[i + 2];
				string text3 = commandLineArgs[i + 3];
				F7Zip f7Zip = new F7Zip(zipFilepath, text2, text3);
				if (!Directory.Exists(Path.Combine(text2, text3)))
				{
					f7Zip.Extract();
				}
				this.ReadConfig(Path.Combine(text2, text3, text));
				return;
			}
		}
		List<string> list = new List<string>();
		for (int j = 0; j < commandLineArgs.Length - 1; j++)
		{
			if (commandLineArgs[j] == "-env")
			{
				list.Add(commandLineArgs[j + 1]);
			}
		}
		list.Add(Path.Combine(Environment.CurrentDirectory, text));
		string processPath = Environment.ProcessPath;
		if (!string.IsNullOrEmpty(processPath))
		{
			processPath = new FileInfo(processPath).DirectoryName;
			if (!string.IsNullOrEmpty(processPath))
			{
				list.Add(Path.Combine(processPath, text));
			}
		}
		using List<string>.Enumerator enumerator = list.GetEnumerator();
		while (enumerator.MoveNext() && !this.ReadFile(enumerator.Current))
		{
		}
	}

	private bool ReadFile(string filenameConfig)
	{
		try
		{
			if (File.Exists(filenameConfig))
			{
				EnvironmentJson environmentJson = JsonSerializer.Deserialize<EnvironmentJson>(File.ReadAllText(filenameConfig, Encoding.UTF8));
				if (environmentJson != null)
				{
					foreach (Tuple<string, string> variable in environmentJson.Variables)
					{
						if (!string.IsNullOrEmpty(variable.Item2))
						{
							Environment.SetEnvironmentVariable(variable.Item1, variable.Item2);
						}
					}
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	public void WriteTemplate(string filenameConfig, string overridePnHomeDrive, string overridePnHomePath, string overridePnService)
	{
		string[] obj = new string[4] { "PNDRIVE", "ARDRIVE", "PNMASTER", "PNDRIVECAD3D2PN" };
		EnvironmentJson environmentJson = new EnvironmentJson();
		string[] array = obj;
		foreach (string text in array)
		{
			environmentJson.Variables.Add(new Tuple<string, string>(text, Environment.GetEnvironmentVariable(text)));
		}
		environmentJson.Variables.Add(new Tuple<string, string>("PNHOMEDRIVE", overridePnHomeDrive));
		environmentJson.Variables.Add(new Tuple<string, string>("PNHOMEPATH", overridePnHomePath));
		environmentJson.Variables.Add(new Tuple<string, string>("PNSERVICE", overridePnService));
		File.WriteAllText(filenameConfig, JsonSerializer.Serialize(environmentJson, new JsonSerializerOptions
		{
			WriteIndented = true
		}), Encoding.UTF8);
	}
}
