using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface IPlug
{
	int? Id { get; set; }

	string Coupling { get; set; }

	double? Angle { get; set; }

	Vector3d Position { get; set; }

	double? Radius { get; set; }

	bool CanConnect(ISocket socket);
}
