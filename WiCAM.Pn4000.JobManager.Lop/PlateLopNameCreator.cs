using System.IO;
using WiCAM.Pn4000.Autoloop;
using WiCAM.Pn4000.Jobdata.Classes;

namespace WiCAM.Pn4000.JobManager.Lop;

internal class PlateLopNameCreator
{
	private readonly LopTemplateInfo _template;

	public PlateLopNameCreator(LopTemplateInfo template)
	{
		_template = template;
	}

	public string CreateFilePath(Plate plateDat, PlateProductionInfo plate)
	{
		string path = CreateFileName(plateDat, plate).Normalize();
		return Path.ChangeExtension(Path.Combine(_template.DestinationPath, path), _template.TypeOfFeedback.ToString());
	}

	private string CreateFileName(Plate plateDat, PlateProductionInfo plate)
	{
		if (_template.NameLength > 0)
		{
			if (!string.IsNullOrEmpty(_template.NameFormat))
			{
				return CreateFormattedFixedLengthName(plateDat, plate);
			}
			return CreateFixedLengthName(plate);
		}
		if (!string.IsNullOrEmpty(_template.NameFormat))
		{
			return CreateFormattedName(plateDat, plate);
		}
		return CreateStandardName(plate);
	}

	private string CreateFormattedFixedLengthName(Plate plateDat, PlateProductionInfo plate)
	{
		string text = CreateFormattedName(plateDat, plate);
		if (text.Length > _template.NameLength)
		{
			return text.Substring(0, _template.NameLength);
		}
		return text;
	}

	private string CreateFormattedName(Plate plateDat, PlateProductionInfo plate)
	{
		return new FormattedNameCreator(_template).Create(plateDat, plate, null);
	}

	private string CreateStandardName(PlateProductionInfo plate)
	{
		return CommonLopHelper.BuildLopFileName(plate.Plate.PLATE_HEADER_TXT_1);
	}

	private string CreateFixedLengthName(PlateProductionInfo plate)
	{
		string text = CreateStandardName(plate);
		if (text.Length > _template.NameLength)
		{
			return text.Substring(0, _template.NameLength);
		}
		return text;
	}
}
