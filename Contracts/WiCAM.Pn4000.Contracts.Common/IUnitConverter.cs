namespace WiCAM.Pn4000.Contracts.Common;

public interface IUnitConverter
{
	IUnitConversion Length { get; }

	IUnitConversion Angle { get; }

	void SetLengthUnit(LengthUnits newUnit);

	void SetAngleUnit(AngleUnits newUnit);
}
