using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using ProjectionType = WiCAM.Pn4000.Contracts.Screen.ProjectionType;

namespace WiCAM.Pn4000.JobManager
{
    internal class ScreenshotScreeen : IScreenshotScreen
    {
        public ProjectionType ProjectionType { get; set; }

        public void AddModel(Model model, bool render)
        {
            throw new System.NotImplementedException();
        }

        public void ImportCameraState(ICameraState cs)
        {
            throw new System.NotImplementedException();
        }

        public void PrintScreen(Model model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            throw new System.NotImplementedException();
        }

        public void PrintScreen(Model model, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            throw new System.NotImplementedException();
        }

        public void PrintScreen(List<Model> model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            throw new System.NotImplementedException();
        }

        public void PrintScreen(List<Model> models, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveModel(Model model, bool render)
        {
            throw new System.NotImplementedException();
        }

        public void SetViewDirectionByMatrix4d(Matrix4d m, bool render)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateAllModelAppearance()
        {
            throw new System.NotImplementedException();
        }

        public void ZoomExtend(int width, int height, double borderFactor)
        {
            throw new System.NotImplementedException();
        }

        public void ZoomExtend(bool render)
        {
            throw new System.NotImplementedException();
        }
    }
}