using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using WiCAM.Pn4000.Contracts.PnCommands;

namespace pncommon.WiCAM.Pn4000.pn4.pn4Services;

public class RFileRecord : INotifyPropertyChanged, IRFileRecord
{
	private string _visualText;

	private ImageSource _visualIcon;

	public RFileType Type { get; set; }

	public int VisualGroup { get; set; }

	public string DefaultLabel { get; set; }

	public int FunctionGroup { get; set; }

	public string FunctionName { get; set; }

	public int IconSize { get; set; }

	public int ShowLabel { get; set; }

	public string LetterShortcut { get; set; }

	public int IdLabel { get; set; }

	public int IdHelp { get; set; }

	public int AddValue1 { get; set; }

	public int AddValue2 { get; set; }

	public string AddValue3 { get; set; }

	public string cfg { get; set; }

	public object tmp_object { get; set; }

	public object tmp_object2 { get; set; }

	public List<IRFileRecord> SubRecords { get; set; }

	public bool DrawArrow { get; set; }

	public string VisualText
	{
		get
		{
			return this._visualText;
		}
		set
		{
			this._visualText = value;
			this.OnPropertyChanged("VisualText");
		}
	}

	public ImageSource VisualIcon
	{
		get
		{
			return this._visualIcon;
		}
		set
		{
			this._visualIcon = value;
			this.OnPropertyChanged("VisualIcon");
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public static RFileRecord FromString(string cfg)
	{
		string[] array = cfg.Split(';');
		if (array.GetLength(0) < 12 || array.GetLength(0) > 13)
		{
			return null;
		}
		RFileRecord rFileRecord = new RFileRecord();
		rFileRecord.cfg = cfg;
		try
		{
			rFileRecord.Type = (RFileType)Convert.ToInt32(array[0]);
			rFileRecord.VisualGroup = Convert.ToInt32(array[1]);
			rFileRecord.DefaultLabel = array[2];
			rFileRecord.FunctionGroup = Convert.ToInt32(array[3]);
			rFileRecord.FunctionName = array[4];
			rFileRecord.IconSize = Convert.ToInt32(array[5]);
			rFileRecord.ShowLabel = Convert.ToInt32(array[6]);
			rFileRecord.LetterShortcut = array[7];
			rFileRecord.IdLabel = Convert.ToInt32(array[8]);
			rFileRecord.IdHelp = Convert.ToInt32(array[9]);
			rFileRecord.AddValue1 = Convert.ToInt32(array[10]);
			rFileRecord.AddValue2 = Convert.ToInt32(array[11]);
			if (array.GetLength(0) >= 13)
			{
				rFileRecord.AddValue3 = array[12];
			}
			rFileRecord.SubRecords = null;
			rFileRecord.DrawArrow = false;
			return rFileRecord;
		}
		catch
		{
			return null;
		}
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public RFileRecord(IRFileRecord info)
	{
		this.DefaultLabel = info.DefaultLabel;
		this.FunctionGroup = info.FunctionGroup;
		this.FunctionName = info.FunctionName;
		this.IdLabel = info.IdLabel;
		this.IdHelp = info.IdHelp;
		this.AddValue1 = info.AddValue1;
		this.AddValue2 = info.AddValue2;
		this.AddValue3 = info.AddValue3;
		this.LetterShortcut = info.LetterShortcut;
	}

	public RFileRecord(IPnCommand info)
	{
		this.DefaultLabel = info.CurrentLabel;
		this.FunctionGroup = info.Group;
		this.FunctionName = info.Command;
		this.IdLabel = info.LabelId;
		this.IdHelp = info.ToolTipId;
		this.AddValue1 = info.AddValue1;
		this.AddValue2 = info.AddValue2;
		this.AddValue3 = info.AddValue3;
	}

	public RFileRecord(MFileLineLine file, int x, int y)
	{
		this.DefaultLabel = file.default_label;
		this.FunctionGroup = file.group;
		this.FunctionName = file.fname;
		this.IdLabel = file.lang_id1;
		this.IdHelp = file.lang_id2;
		this.AddValue1 = x;
		this.AddValue2 = y;
	}

	public RFileRecord()
	{
	}

	public string GetOutputText()
	{
		// Create array of values in order they should appear
		object[] values = new[]
		{
		(object)3,               // Appears to be a fixed type identifier
        this.VisualGroup,
		this.DefaultLabel,
		this.FunctionGroup,
		this.FunctionName,
		this.IconSize,
		this.ShowLabel,
		this.LetterShortcut,
		this.IdLabel,
		this.IdHelp,
		this.AddValue1,
		this.AddValue2
	};

		// Join values with semicolons using invariant culture
		return string.Join(";", values);
	}
}
// Example usage