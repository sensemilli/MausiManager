using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.PN3D.Converter;

public class UnitConverter : IUnitConverter
{
    public IUnitConversion Length => throw new System.NotImplementedException();

    public IUnitConversion Angle => throw new System.NotImplementedException();

    public void SetAngleUnit(AngleUnits newUnit)
    {
        throw new System.NotImplementedException();
    }

    public void SetLengthUnit(LengthUnits newUnit)
    {
        throw new System.NotImplementedException();
    }

   
        public double ConvertedValue
        {
            get => this.Value / this.Factor;
            set => this.Value = value * this.Factor;
        }

        public double Value { get; set; }

        public double Factor { get; }

        public bool UseInch { get; }

        public UnitConverter(double value, double factor)
        {
            this.Value = value;
            this.Factor = SystemConfiguration.UseInch ? factor : 1.0;
        }
    }

