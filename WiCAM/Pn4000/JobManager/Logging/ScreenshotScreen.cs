// Decompiled with JetBrains decompiler
// Type: WiCAM.Pn4000.ScreenD3D.Controls.ScreenshotScreen
// Assembly: WiCAM.Pn4000.ScreenD3D, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ADA7AC84-ED4F-4E53-AAB8-AD7F1D8DADAD
// Assembly location: C:\u\pn\run\WiCAM.Pn4000.ScreenD3D.dll

using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Renderer;
using Model = WiCAM.Pn4000.BendModel.Model;

#nullable enable
namespace WiCAM.Pn4000.ScreenD3D.Controls
{
    public class ScreenshotScreen : IScreenshotScreen
    {
        private readonly
#nullable disable
        IScreen3DMain _screen3DMain;

        private Screen3D _screen => this._screen3DMain.Screen3D;

        public ProjectionType ProjectionType
        {
            get => this._screen.ProjectionType;
            set => this._screen.ProjectionType = value;
        }

        public void PrintScreen(
          Model model,
          string targetPath,
          Matrix4d transform,
          int border = -1,
          int width = -1,
          int height = -1,
#nullable enable
          CameraState? cameraStateOverride = null)
        {
            this._screen.PrintScreen(model, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void PrintScreen(
#nullable disable
          List<Model> model,
          string targetPath,
          Matrix4d transform,
          int border = -1,
          int width = -1,
          int height = -1,
#nullable enable
          CameraState? cameraStateOverride = null)
        {
            this._screen.PrintScreen(model, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void UpdateAllModelAppearance() => this._screen.UpdateAllModelAppearance();

        public ScreenshotScreen(
#nullable disable
        IScreen3DMain screen3DMain) => this._screen3DMain = screen3DMain;

        public void ImportCameraState(CameraState cs)
        {
            this._screen.ScreenD3D.Renderer.ImportCameraState(cs);
        }

        public void RemoveModel(Model model, bool render)
        {
            this._screen.ScreenD3D.RemoveModel(model, render);
        }

        public void AddModel(Model model, bool render)
        {
            this._screen.ScreenD3D.AddModel(model, render);
        }

        public void SetViewDirectionByMatrix4d(Matrix4d m, bool render)
        {
            this._screen.ScreenD3D.SetViewDirectionByMatrix4d(m, render);
        }

        public void ZoomExtend(int width, int height, double borderFactor)
        {
            this._screen.ScreenD3D.ZoomExtend(width, height, borderFactor);
        }

        public void ZoomExtend(bool render) => this._screen.ScreenD3D.ZoomExtend(render);

        public void PrintScreen(Model model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            this._screen.PrintScreen(model, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void PrintScreen(Model model, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            this._screen.PrintScreen(model, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void PrintScreen(List<Model> model, string targetPath, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            this._screen.PrintScreen(model, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void PrintScreen(List<Model> models, string targetPath, PnImageFormat format, Matrix4d transform, int border = -1, int width = -1, int height = -1, ICameraState cameraStateOverride = null)
        {
            this._screen.PrintScreen(models, targetPath, transform, border, width, height, cameraStateOverride);
        }

        public void ImportCameraState(ICameraState cs)
        {
            this._screen.ScreenD3D.Renderer.ImportCameraState(cs);
        }
    }
}
