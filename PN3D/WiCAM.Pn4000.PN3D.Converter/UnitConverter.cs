using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.PN3D.Converter;

public class UnitConverter
{
	public double ConvertedValue
	{
		get
		{
			return this.Value / this.Factor;
		}
		set
		{
			this.Value = value * this.Factor;
		}
	}

	public double Value { get; set; }

	public double Factor { get; }

	public bool UseInch { get; }

	public UnitConverter(double value, double factor)
	{
		this.Value = value;
		this.Factor = (SystemConfiguration.UseInch ? factor : 1.0);
	}
}
