using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.Screen;

public interface IScreenshotScreen
{
	ProjectionType ProjectionType { get; set; }

	void PrintScreen(Model model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState? cameraStateOverride = null);

	void PrintScreen(Model model, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState? cameraStateOverride = null);

	void PrintScreen(List<Model> model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState? cameraStateOverride = null);

	void PrintScreen(List<Model> models, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState? cameraStateOverride = null);

	void UpdateAllModelAppearance();

	void ImportCameraState(ICameraState cs);

	void RemoveModel(Model model, bool render);

	void AddModel(Model model, bool render);

	void SetViewDirectionByMatrix4d(Matrix4d m, bool render);

	void ZoomExtend(int width, int height, double borderFactor);

	void ZoomExtend(bool render);
}
