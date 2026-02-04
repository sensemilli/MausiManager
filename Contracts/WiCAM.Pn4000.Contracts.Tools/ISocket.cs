using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISocket
{
	int? Id { get; set; }

	string Coupling { get; set; }

	double? Angle { get; set; }

	double? MaxRadius { get; set; }

	double? MinRadius { get; set; }

	Vector3d Position { get; set; }
}
