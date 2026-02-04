using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.pn4.pn4Services;

namespace WiCAM.Pn4000.PN3D.Converter;

public class UnitCurrentLanguage : IUnitCurrentLanguage
{
	private readonly IUnitConverters _unitConverters;

	private readonly ILanguageDictionary _languageDictionary;

	public UnitCurrentLanguage(IUnitConverters unitConverters, ILanguageDictionary languageDictionary)
	{
		this._unitConverters = unitConverters;
		this._languageDictionary = languageDictionary;
	}

	public string ConvertMmToCurrentUnit(double? mm, bool addUnit, string? format)
	{
		if (this._languageDictionary.GetInchMode() == 1)
		{
			return CombineResult(this._unitConverters.ConvertMmToInch(mm), "inch", addUnit, format);
		}
		return CombineResult(mm, "mm", addUnit, format);
		static string CombineResult(double? value, string unit, bool addUnit, string format)
		{
			if (value.HasValue)
			{
				if (!addUnit)
				{
					return value.Value.ToString(format);
				}
				return value.Value.ToString(format) + " " + unit;
			}
			return string.Empty;
		}
	}
}
