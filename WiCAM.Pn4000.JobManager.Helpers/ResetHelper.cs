using System;
using System.Text;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Helpers;

internal class ResetHelper
{
	private readonly IJobManagerSettings _settings;

	private readonly DatLineManager _lineManager;

	public ResetHelper(IJobManagerSettings settings)
	{
		_settings = settings;
		_lineManager = new DatLineManager();
	}

	public void Reset(PlateInfo plate)
	{
		UpdatePlateDat(plate);
	}

	private void UpdatePlateDat(PlateInfo plate)
	{
		Logger.Info("DatReader : UpdatePlateDat : {0}", plate.Path);
		string text = IOHelper.FileReadAllText(plate.Path);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(text);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_STATUS", 0, 2);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_PRODUCED", 0, 0);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_PRODUCED_ACT", 0, 0);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_STORNO", 0, 0);
		_lineManager.ReplaceLine(stringBuilder, "PLATE_TIME_REAL", 0, 0);
		foreach (PlatePartInfo platePart in plate.PlateParts)
		{
			string value = _lineManager.BuildDatLine("PLATE_PART_NUMBER", platePart.PLATE_PART_NUMBER.ToString());
			int num = text.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
			if (num > -1)
			{
				_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_AMOUNT_RE", num, 0);
				_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_AMOUNT_SC", num, 0);
				_lineManager.ReplaceLine(stringBuilder, "PLATE_PART_TIME_REAL", num, 0);
			}
			UpdatePartDat(platePart, plate.PLATE_PRODUCED + plate.PLATE_STORNO);
		}
		_lineManager.ReplaceNotExistingLine(stringBuilder, " PRODUCTION_USER", Environment.UserName);
		if (IOHelper.FileCanBeRead(plate.Path, 30))
		{
			IOHelper.FileWriteAllText(plate.Path, stringBuilder.ToString());
		}
		plate.JobReference.CheckStatus();
	}

	private void UpdatePartDat(PlatePartInfo platePart, int plates)
	{
		PartInfo part = platePart.Part;
		string value = IOHelper.FileReadAllText(part.Path);
		if (!string.IsNullOrEmpty(value))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value);
			part.PART_PRODUCED = ReduceValue(part.PART_PRODUCED, platePart.PLATE_PART_AMOUNT * plates);
			part.PART_STORNO = ReduceValue(part.PART_STORNO, platePart.PLATE_PART_AMOUNT_RE);
			part.PART_REJECT = ReduceValue(part.PART_REJECT, platePart.PLATE_PART_AMOUNT_SC);
			_lineManager.ReplaceLine(stringBuilder, "PART_PRODUCED", 0, part.PART_PRODUCED);
			_lineManager.ReplaceLine(stringBuilder, "PART_STORNO", 0, part.PART_STORNO);
			_lineManager.ReplaceLine(stringBuilder, "PART_REJECT", 0, part.PART_REJECT);
			if (IOHelper.FileCanBeRead(part.Path, 30))
			{
				IOHelper.FileWriteAllText(part.Path, stringBuilder.ToString());
			}
		}
	}

	private int ReduceValue(int current, int reduceAmount)
	{
		current -= reduceAmount;
		if (current < 0)
		{
			current = 0;
		}
		return current;
	}
}
