using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.DrawGeometry;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

internal class DrawToolProfiles : IDrawToolProfiles
{
	public void LoadPreview3D(string geometryFile, IScreen3D image)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(geometryFile));
	}

	public void LoadPreview2D(string? geometryFile, double workingHeightMinY, double workingHeightMaxY, double offsetX, Canvas image, bool showWorkingHeight = true)
	{
		if (!string.IsNullOrEmpty(geometryFile))
		{
			CadGeoInfo cadGeoInfo = new CadGeoHelper().ReadCadgeo(geometryFile);
			LoadPreview2D(cadGeoInfo, workingHeightMinY, workingHeightMaxY, offsetX, image, showWorkingHeight);
		}
	}

	public void LoadPreview2D(CadGeoInfo? cadGeoInfo, double workingHeightMinY, double workingHeightMaxY, double offsetX, Canvas image, bool showWorkingHeight = true)
	{
		if (cadGeoInfo == null)
		{
			return;
		}
		double num = workingHeightMaxY - workingHeightMinY;
		double num2 = double.MinValue;
		double num3 = double.MaxValue;
		foreach (GeoElementInfo geoElement in cadGeoInfo.GeoElements)
		{
			geoElement.PnColor = 2;
			if (geoElement is GeoLineInfo geoLineInfo)
			{
				num2 = Math.Max(num2, Math.Max(geoLineInfo.X0, geoLineInfo.X1));
				num3 = Math.Min(num3, Math.Min(geoLineInfo.X0, geoLineInfo.X1));
			}
		}
		CenterYAxsisOfCadGeoInfo(cadGeoInfo);
		CadGeoInfo cadGeoInfo2 = new CadGeoInfo();
		if (showWorkingHeight)
		{
			GeoLineInfo gi = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = offsetX,
				Y0 = workingHeightMinY,
				X1 = offsetX,
				Y1 = workingHeightMaxY
			};
			cadGeoInfo2.AddElement(gi);
			GeoLineInfo gi2 = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = num3,
				Y0 = workingHeightMaxY,
				X1 = num2,
				Y1 = workingHeightMaxY
			};
			cadGeoInfo2.AddElement(gi2);
			GeoLineInfo gi3 = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = num3,
				Y0 = workingHeightMinY,
				X1 = num2,
				Y1 = workingHeightMinY
			};
			cadGeoInfo2.AddElement(gi3);
		}
		int num4 = (int)image.ActualHeight;
		int num5 = (int)image.ActualWidth;
		image.Children.Clear();
		Path path = DrawGeo.DrawCadGeo(cadGeoInfo, num5, num4, Colors.Red);
		image.Children.Add(path);
		image.Children.Add(DrawGeo.DrawCadGeo(cadGeoInfo2, num5, num4, Colors.Gray, new DoubleCollection { 4.0, 4.0 }, path.Data.Transform));
		if (showWorkingHeight)
		{
			double num6 = (double)num4 / cadGeoInfo.Height;
			image.Children.Add(new TextBlock
			{
				Text = num + "mm",
				FontSize = 15.0,
				Margin = new Thickness(num5 / 2, num / 2.0 * num6 - 7.0, num5 / 2, (num / 2.0 + cadGeoInfo.Height - num) * num6 + 7.0)
			});
		}
	}

	public void LoadPunchPreview2D(PunchProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true)
	{
		LoadPunchPreview2D(machine?.MachinePath + machine?.UpperToolsGeometry + profile.GeometryFile, profile.WorkingHeight, image, showWorkingHeight);
	}

	public void LoadPunchPreview2D(IPunchProfile profile, Canvas image, IBendMachineTools machine, bool showWorkingHeight = true)
	{
		if (profile == null)
		{
			image.Children.Clear();
		}
		else
		{
			LoadPunchPreview2D(profile.MultiToolProfile.GeometryFileFull, profile.WorkingHeight, image, showWorkingHeight);
		}
	}

	public void LoadPunchPreview2D(string geometryFile, double workingHeight, Canvas image, bool showWorkingHeight = true)
	{
		if (string.IsNullOrEmpty(geometryFile))
		{
			return;
		}
		CadGeoInfo cadGeoInfo = new CadGeoHelper().ReadCadgeo(geometryFile);
		double num = double.MinValue;
		double num2 = double.MaxValue;
		foreach (GeoElementInfo geoElement in cadGeoInfo.GeoElements)
		{
			geoElement.PnColor = 2;
			if (geoElement is GeoLineInfo geoLineInfo)
			{
				num = Math.Max(num, Math.Max(geoLineInfo.X0, geoLineInfo.X1));
				num2 = Math.Min(num2, Math.Min(geoLineInfo.X0, geoLineInfo.X1));
			}
		}
		CenterYAxsisOfCadGeoInfo(cadGeoInfo);
		CadGeoInfo cadGeoInfo2 = new CadGeoInfo();
		if (showWorkingHeight)
		{
			GeoLineInfo gi = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = 0.0,
				Y0 = 0.0,
				X1 = 0.0,
				Y1 = workingHeight
			};
			cadGeoInfo2.AddElement(gi);
			GeoLineInfo gi2 = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = num2,
				Y0 = workingHeight,
				X1 = num,
				Y1 = workingHeight
			};
			cadGeoInfo2.AddElement(gi2);
			GeoLineInfo gi3 = new GeoLineInfo
			{
				GeoType = GeoElementType.Line,
				X0 = num2,
				Y0 = 0.0,
				X1 = num,
				Y1 = 0.0
			};
			cadGeoInfo2.AddElement(gi3);
		}
		int num3 = (int)image.ActualHeight;
		int num4 = (int)image.ActualWidth;
		image.Children.Clear();
		Path path = DrawGeo.DrawCadGeo(cadGeoInfo, num4, num3, Colors.Red);
		image.Children.Add(path);
		image.Children.Add(DrawGeo.DrawCadGeo(cadGeoInfo2, num4, num3, Colors.Gray, new DoubleCollection { 4.0, 4.0 }, path.Data.Transform));
		if (showWorkingHeight)
		{
			double num5 = (double)num3 / cadGeoInfo.Height;
			image.Children.Add(new TextBlock
			{
				Text = workingHeight + "mm",
				FontSize = 15.0,
				Margin = new Thickness(num4 / 2, (workingHeight / 2.0 + cadGeoInfo.Height - workingHeight) * num5 - 7.0, num4 / 2, workingHeight / 2.0 * num5 + 7.0)
			});
		}
	}

	private void CenterYAxsisOfCadGeoInfo(CadGeoInfo cadGeoInfo)
	{
		double x = cadGeoInfo.TopLeft.X;
		double x2 = cadGeoInfo.BottomRight.X;
		double y = cadGeoInfo.TopLeft.Y;
		x2 = Math.Max(Math.Abs(x), Math.Abs(x2));
		x = 0.0 - x2;
		cadGeoInfo.AddElement(new GeoLineInfo
		{
			GeoType = GeoElementType.Line,
			X0 = x,
			Y0 = y,
			X1 = x,
			Y1 = y
		});
		cadGeoInfo.AddElement(new GeoLineInfo
		{
			GeoType = GeoElementType.Line,
			X0 = x2,
			Y0 = y,
			X1 = x2,
			Y1 = y
		});
	}

	public void LoadPunchPreview3D(PunchProfile profile, IScreen3D image, BendMachine machine)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(profile.GeometryFile));
	}

	public void LoadDiePreview2D(DieProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true)
	{
		LoadDiePreview2D(machine.MachinePath + machine.LowerToolsGeometry + profile.GeometryFile, profile.WorkingHeight, image, showWorkingHeight);
	}

	public void LoadDiePreview2D(IDieProfile profile, Canvas image, IBendMachineTools machine, bool showWorkingHeight = true)
	{
		if (profile == null)
		{
			image.Children.Clear();
		}
		else
		{
			LoadDiePreview2D(profile.MultiToolProfile.GeometryFileFull, profile.WorkingHeight, image, showWorkingHeight);
		}
	}

	public void LoadDiePreview2D(string geometryFile, double workingHeight, Canvas image, bool showWorkingHeight = true)
	{
		LoadPreview2D(geometryFile, 0.0 - workingHeight, 0.0, 0.0, image, showWorkingHeight);
	}

	public void LoadDiePreview3D(DieProfile profile, IScreen3D image, BendMachine machine)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(profile.GeometryFile));
	}

	public void LoadDiePreview2D(HemProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true)
	{
		LoadPreview2D(machine.MachinePath + machine.LowerToolsGeometry + profile.GeometryFile, 0.0 - profile.WorkingHeight, 0.0, 0.0, image, showWorkingHeight);
	}

	public void LoadDiePreview3D(HemProfile profile, IScreen3D image, BendMachine machine)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(profile.GeometryFile));
	}

	public void LoadAdapterPreview2D(AdapterProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true)
	{
		LoadPreview2D(machine.MachinePath + machine.AdapterGeometry + profile.GeometryFile, 0.0, profile.WorkingHeight, 0.0, image, showWorkingHeight);
	}

	public void LoadAdapterPreview3D(AdapterProfile profile, IScreen3D image, BendMachine machine)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(profile.GeometryFile));
	}

	public void LoadHolderPreview2D(HolderProfile profile, Canvas image, BendMachine machine, bool showWorkingHeight = true)
	{
		LoadPreview2D(machine.MachinePath + machine.HolderGeometry + profile.GeometryFile, 0.0, 0.0, profile.WorkingHeight, image, showWorkingHeight);
	}

	public void LoadHolderPreview3D(HolderProfile profile, IScreen3D image, BendMachine machine)
	{
		image.ScreenD3D.RemoveModel(null);
		image.ScreenD3D.RemoveBillboard(null);
		image.ScreenD3D.AddModel(N3dLoader.LoadN3dFile(profile.GeometryFile));
	}
}
