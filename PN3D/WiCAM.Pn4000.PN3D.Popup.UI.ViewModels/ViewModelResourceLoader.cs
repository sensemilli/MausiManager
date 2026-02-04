using System.IO;
using System.Threading;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Loader;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public static class ViewModelResourceLoader
{
	public static void LoadArrows(string pnDrive, ScreenD3D11 screen)
	{
		new Thread((ThreadStart)delegate
		{
			global::WiCAM.Pn4000.BendModel.Model model = new global::WiCAM.Pn4000.BendModel.Model();
			if (File.Exists(pnDrive + "\\u\\pn\\pixmap\\img\\allArrows.stl"))
			{
				model = StlLoader.LoadStl(pnDrive + "\\u\\pn\\pixmap\\img\\allArrows.stl", buildCollisionTree: false);
			}
			model.ModelType = ModelType.System;
			model.PartRole = PartRole.Tripod;
			screen.AddModel(model);
		}).Start();
	}

	public static void LoadLetters(string pnDrive, ScreenD3D11 screen)
	{
		new Thread((ThreadStart)delegate
		{
			string[] array = new string[3] { "letterX", "letterY", "letterZ" };
			Vector3d[] array2 = new Vector3d[3]
			{
				new Vector3d(60.0, 0.0, 0.0),
				new Vector3d(0.0, 60.0, 0.0),
				new Vector3d(0.0, 0.0, 60.0)
			};
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				string text2 = pnDrive + "\\u\\pn\\pixmap\\img\\" + text + ".stl";
				if (File.Exists(text2))
				{
					global::WiCAM.Pn4000.BendModel.Model model = StlLoader.LoadStl(text2, buildCollisionTree: false);
					model.Name = text;
					model.Transform = Matrix4d.Scale(0.5, 0.5, 0.5);
					model.Transform *= Matrix4d.Translation(array2[i]);
					model.ModelType = ModelType.System;
					model.PartRole = PartRole.BillboardModel;
					screen.AddModel(model);
				}
			}
		}).Start();
	}
}
