using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IBendTableItem
{
	bool IsValid { get; }

	int? Material3DGroupID { get; set; }

	double? Thickness { get; set; }

	double? Angle { get; set; }

	double? R { get; set; }

	double? KFactor { get; set; }

	string Tag { get; set; }

	double? SpringBack { get; set; }

	double? MinRadius { get; set; }

	double? MaxRadius { get; set; }

	double? VWidth { get; set; }

	double? CornerRadius { get; set; }

	CornerType CornerType { get; set; }

	double? PunchRadius { get; set; }

	double? PunchAngle { get; set; }

	double? BendLengthMin { get; set; }

	double? BendLengthMax { get; set; }

	IBendTableItem CreateCopy();
}
