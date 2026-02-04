using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISensorDiskProfile
{
	int Amount { get; set; }

	double CornerRadius { get; set; }

	double DiskThickness { get; set; }

	string GeometryFile { get; set; }

	Model Model { get; }

	int Id { get; set; }

	string Name { get; set; }

	int Type { get; set; }
}
