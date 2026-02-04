using System;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IDieGroup : IProfileGroup
{
	double VWidth { get; set; }

	double Radius { get; set; }

	double VAngle { get; set; }

	double VAngleRad
	{
		get
		{
			return this.VAngle * global::System.Math.PI / 180.0;
		}
		set
		{
			this.VAngle = value / global::System.Math.PI * 180.0;
		}
	}

	CornerType CornerType { get; set; }

	int PrimaryToolId { get; set; }

	string PrimaryToolName { get; set; }

	IDieGroup Copy();
}
