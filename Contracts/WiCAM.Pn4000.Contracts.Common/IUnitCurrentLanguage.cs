namespace WiCAM.Pn4000.Contracts.Common;

public interface IUnitCurrentLanguage
{
	string ConvertMmToCurrentUnit(double? mm, bool addUnit, string? format = null);
}
