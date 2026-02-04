using System;
using System.IO;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Enum;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.pn4.pn4UILib;

namespace WiCAM.Pn4000.Helpers.Util;

public class ArchiveBrowser
{
	private string _arAnswer = "ERG0";

	private string _arBr { get; set; }

	public bool MultiSelect { get; set; }

	public ArchiveType ArchiveType { get; set; }

	public int ArchiveNumber { get; set; }

	public string ArchivePath { get; set; }

	public string FileName { get; set; }

	public string FullPath { get; set; }

	public bool HasResult { get; set; }

	public ArchiveBrowser()
	{
		this._arBr = PnPathBuilder.PathInPnDrive("u", "pn", "run", "pnarbrdb.exe");
		this.HasResult = false;
		this.Init();
	}

	public ArchiveBrowser(ArchiveType arType)
		: this()
	{
		this.ArchiveType = arType;
	}

	public void Init(int archive = 0, bool multiselect = false, ArchiveType arType = ArchiveType.M2D)
	{
		this.ArchiveNumber = archive;
		this.MultiSelect = multiselect;
		this.ArchiveType = arType;
	}

	public void Start()
	{
		this.Reset();
		PnExternalCall.Start(string.Concat(str1: this.CreateParameterString(ArchiveAdapter.ArchiveID), str0: this._arBr), null);
		this.ReadAnswer();
		if (this.FullPath != null && this.FullPath != string.Empty)
		{
			this.HasResult = true;
		}
		this.Reset();
	}

	public void Start(Action onResult)
	{
		this.Start();
		if (this.HasResult)
		{
			onResult();
		}
	}

	private bool ReadAnswer()
	{
		try
		{
			int num = 0;
			string[] array = File.ReadAllLines(this._arAnswer);
			if (array.Length == 2)
			{
				num = Convert.ToInt32(array[0]);
				this.FileName = array[1];
			}
			else if (array.Length == 3)
			{
				num = Convert.ToInt32(array[0]);
				this.FileName = array[1];
			}
			else if (array.Length == 6)
			{
				num = Convert.ToInt32(array[1]);
				this.FileName = array[2];
			}
			this.ArchiveNumber = num;
			this.ArchivePath = ArchiveAdapter.SetArchiveAndGetPathFor3D(num);
			this.FullPath = this.ArchivePath + this.FileName;
			if (this.ArchiveType == ArchiveType.M2D)
			{
				this.ArchivePath = this.ArchivePath.Replace("\\n3d\\", "\\m2d\\");
				this.FullPath = this.FullPath.Replace("\\n3d\\", "\\m2d\\");
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private string CreateParameterString(int arNum)
	{
		string value = (this.MultiSelect ? "1" : "0");
		string text = "m2d";
		text = ((this.ArchiveType != 0) ? "n3d" : "m2d");
		return $" /archive:{arNum} /multiselect:{value} /type:{text}";
	}

	private void Reset()
	{
		try
		{
			if (File.Exists(this._arAnswer))
			{
				File.Delete(this._arAnswer);
			}
		}
		catch
		{
		}
	}
}
