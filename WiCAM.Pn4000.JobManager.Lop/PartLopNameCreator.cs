using System.IO;
using WiCAM.Pn4000.Autoloop;
using WiCAM.Pn4000.Jobdata.Classes;

namespace WiCAM.Pn4000.JobManager.Lop;

internal class PartLopNameCreator
{
	private readonly LopTemplateInfo _template;

	public PartLopNameCreator(LopTemplateInfo template)
	{
		_template = template;
	}

	public string CreateFilePath(Plate plateDat, PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		string path = CreateFileName(plateDat, plate, part).Normalize();
		return Path.ChangeExtension(Path.Combine(_template.DestinationPath, path), _template.TypeOfFeedback.ToString());
	}

	private string CreateFileName(Plate plateDat, PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		if (_template.NameLength > 0)
		{
			if (!string.IsNullOrEmpty(_template.NameFormat))
			{
				return CreateFormattedFixedLengthName(plateDat, plate, part);
			}
			return CreateFixedLengthName(part);
		}
		if (!string.IsNullOrEmpty(_template.NameFormat))
		{
			return CreateFormattedName(plateDat, plate, part);
		}
		return CreateStandardName(part);
	}

	private string CreateFormattedFixedLengthName(Plate plateDat, PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		string text = CreateFormattedName(plateDat, plate, part);
		if (text.Length > _template.NameLength)
		{
			return text.Substring(0, _template.NameLength);
		}
		return text;
	}

	private string CreateFormattedName(Plate plateDat, PlateProductionInfo plate, PlatePartProductionInfo part)
	{
		return new FormattedNameCreator(_template).Create(plateDat, plate, part);
	}

	private string CreateStandardName(PlatePartProductionInfo part)
	{
		return CommonLopHelper.BuildLopFileName(part.PlatePart.Part.PART_NAME);
	}

	private string CreateFixedLengthName(PlatePartProductionInfo part)
	{
		string text = CreateStandardName(part);
		if (text.Length > _template.NameLength)
		{
			return text.Substring(0, _template.NameLength);
		}
		return text;
	}
}
