using System;

namespace WiCAM.Pn4000.Contracts.Tools;

[Obsolete]
public interface IPreferredProfileToolSetDeprecated
{
	int PunchGroupId { get; set; }

	string PunchGroupName { get; set; }

	int DieGroupId { get; set; }

	string DieGroupName { get; set; }

	double VWidth { get; set; }

	double VAngle { get; set; }

	double CornerRadius { get; set; }

	double PunchRadius { get; set; }

	IPreferredProfileToolSetDeprecated Clone();
}
