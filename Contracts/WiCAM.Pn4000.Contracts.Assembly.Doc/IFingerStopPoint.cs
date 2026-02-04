using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IFingerStopPoint
{
	Vector3d StopPoint { get; set; }

	Vector3d StopPointRelativeToPart { get; set; }

	IFingerStopCombination StopCombination { get; set; }
}
