using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Screen;

public interface ICameraState
{
	Matrix4d? RootTransform { get; set; }

	float Zoom { get; set; }

	float CamDistOrtho { get; set; }
}
