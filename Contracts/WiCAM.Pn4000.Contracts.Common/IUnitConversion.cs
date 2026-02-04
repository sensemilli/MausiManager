namespace WiCAM.Pn4000.Contracts.Common;

public interface IUnitConversion
{
	string UnitSuffix { get; }

	string Unit => this.UnitSuffix.Trim();

	int AdditionalDecimalPlaces { get; }

	double FromUi(double ui, int? decimalPlaces = null);

	double ToUi(double value, int? decimalPlaces = null);

	double? ToUi(double? value, int? decimalPlaces = null)
	{
		if (!value.HasValue)
		{
			return null;
		}
		return this.ToUi(value.Value, decimalPlaces);
	}

	string ToUiUnit(double value, int? decimalPlaces = null);

	string? ToUiUnit(double? value, int? decimalPlaces = null)
	{
		if (!value.HasValue)
		{
			return null;
		}
		return this.ToUiUnit(value.Value, decimalPlaces);
	}
}
