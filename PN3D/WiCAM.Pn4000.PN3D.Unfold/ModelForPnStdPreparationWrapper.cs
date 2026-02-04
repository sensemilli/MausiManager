using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Unfold;

public class ModelForPnStdPreparationWrapper : IModelForPnStdPreparationWrapper
{
	private readonly IFactorio _factorio;

	private IGlobals _globalsBackfield;

	private readonly IMaterial3dFortran _material3dFortran;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly IMaterialManager _materials;

	private IGlobals _globals => this._globalsBackfield ?? (this._globalsBackfield = this._factorio.Resolve<IGlobals>());

	public event Action OnTransfer2d;

	public ModelForPnStdPreparationWrapper(IFactorio factorio, IMaterial3dFortran material3dFortran, IConfigProvider configProvider, IPnPathService pathService, IMaterialManager materials, ILanguageDictionary languageDictionary)
	{
		this._factorio = factorio;
		this._material3dFortran = material3dFortran;
		this._configProvider = configProvider;
		this._pathService = pathService;
		this._materials = materials;
	}

	public F2exeReturnCode Apply2D<T>(IDoc3d doc, Model model, Face face, Model faceModel, bool removeProjectionHoles, bool includeZeroBorders) where T : IModelForPnStdPreparation
	{
		this.OnTransfer2d?.Invoke();
		F2exeReturnCode f2exeReturnCode = F2exeReturnCode.OK;
		if (doc?.UnfoldModel3D == null || !doc.HasModel)
		{
			this._globals.MessageDisplay.WithContext(doc).ShowTranslatedErrorMessage("Transfer2DError.ERROR_NO_DATA");
			return F2exeReturnCode.ERROR_NO_DATA;
		}
		if (doc.UnfoldModel3D.PartInfo.PartType == PartType.Unassigned)
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("Transfer2DError.ERROR_INVALID_GEOMETRY");
			return F2exeReturnCode.ERROR_INVALID_GEOMETRY;
		}
		if (doc.UnfoldModel3D == model && !this._configProvider.InjectOrCreate<General3DConfig>().P3D_IgnoreSelfCollision && doc.CheckSelfCollisionUnfoldModel())
		{
			List<string> messageKey = new List<string> { "Transfer2DError.ERROR_SELF_COLLISION", "", "l_popup.PopupUnfoldSetting.Hint", "l_popup.PopupUnfoldSetting.IgnoreSelfCollisionHint l_popup.PnInterfaceSettings.Title -> l_popup.PnInterfaceSettings.UnfoldSettings" };
			doc.MessageDisplay.ShowTranslatedErrorMessages(messageKey);
			return F2exeReturnCode.ERROR_INVALID_GEOMETRY;
		}
		F2exeReturnCode f2exeReturnCode2 = doc.CheckWarningKFactorMaterialByUser();
		if (f2exeReturnCode2 != 0)
		{
			f2exeReturnCode = f2exeReturnCode2;
		}
		this._material3dFortran.SetActiveMaterial(doc.Material.Number);
		this.SendThickness(doc);
		if (!this.Generate2DForCurrentDoc<T>(doc, this._globals, model, face, faceModel, removeProjectionHoles, includeZeroBorders))
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("Transfer2DError.ERROR_INVALID_GEOMETRY");
			return F2exeReturnCode.ERROR_INVALID_GEOMETRY;
		}
		this.GenerateBitmapsFor2D(doc, this._globals);
		Geo2DAdapter.LoadDefaultCad2DAndRedraw();
		if (f2exeReturnCode != 0)
		{
			return f2exeReturnCode;
		}
		if (this._configProvider.InjectOrCreate<General3DConfig>().P3D_Mark_ReliefIn2D && model == doc.UnfoldModel3D && model.GetAllFaces().Any((Face f) => f.IsTessellated.HasValue))
		{
			return F2exeReturnCode.WARN_CUTOUTS_ADDED;
		}
		return f2exeReturnCode;
	}

	private bool Generate2DForCurrentDoc<T>(IDoc3d doc, IGlobals globals, Model model, Face face, Model faceModel, bool removeProjectionHoles, bool includeZeroBorders) where T : IModelForPnStdPreparation
	{
		T val = this._factorio.Resolve<T>();
		val.Apply2D(doc, model, face, faceModel, face == null, removeProjectionHoles, includeZeroBorders);
		if (val.ErrorDetected)
		{
			return false;
		}
		val.Db2D.SaveCadGeo(0.0);
		val.Db2D.SaveCadTxt(this._pathService.PNHOMEDRIVE, this._pathService.PNHOMEPATH);
		this.SaveUnfoldData(val.Db2D.Boundary.Min, val.Db2D.Boundary.Max, doc, globals);
		return true;
	}

	private void GenerateBitmapsFor2D(IDoc3d doc, IGlobals globals)
	{
		try
		{
			globals.ScreenshotScreen.PrintScreen(doc.EntryModel3D, "P3DTR2D.png", Matrix4d.Identity, 10, 800, 800);
			GadGeoDrawMultiOutput gadGeoDrawMultiOutput = this._factorio.Resolve<GadGeoDrawMultiOutput>();
			gadGeoDrawMultiOutput.Init("CADGEO");
			BitmapSource source = gadGeoDrawMultiOutput.DrawCadGeo(800, 800, 0) as BitmapSource;
			BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
			bitmapEncoder.Frames.Add(BitmapFrame.Create(source));
			using Stream stream = File.Create("P3DTR2D_2D.png");
			bitmapEncoder.Save(stream);
		}
		catch (Exception e)
		{
			globals.logCenterService.CatchRaport(e);
		}
	}

	private void SaveUnfoldData(Vector3d min2D, Vector3d max2D, IDoc3d doc, IGlobals globals)
	{
		if (!this._configProvider.InjectOrCreate<General3DConfig>().P3D_ExportInfoFile)
		{
			return;
		}
		Pair<Vector3d, Vector3d> pair = doc.EntryModel3D?.GetBoundary(Matrix4d.Identity);
		Pair<Vector3d, Vector3d> pair2 = doc.UnfoldModel3D?.GetBoundary(Matrix4d.Identity);
		Pair<Vector3d, Vector3d> pair3 = doc.ModifiedEntryModel3D?.GetBoundary(Matrix4d.Identity);
		Vector3d vector3d = default(Vector3d);
		if (pair != null)
		{
			vector3d = new Vector3d(Math.Round(Math.Abs(pair.Item2.X - pair.Item1.X), 5), Math.Round(Math.Abs(pair.Item2.Y - pair.Item1.Y), 5), Math.Round(Math.Abs(pair.Item2.Z - pair.Item1.Z), 5));
		}
		Vector3d vector3d2 = default(Vector3d);
		if (pair2 != null)
		{
			vector3d2 = new Vector3d(Math.Round(Math.Abs(pair2.Item2.X - pair2.Item1.X), 5), Math.Round(Math.Abs(pair2.Item2.Y - pair2.Item1.Y), 5), Math.Round(Math.Abs(pair2.Item2.Z - pair2.Item1.Z), 5));
		}
		Vector3d vector3d3 = default(Vector3d);
		if (pair3 != null)
		{
			vector3d3 = new Vector3d(Math.Round(Math.Abs(pair3.Item2.X - pair3.Item1.X), 5), Math.Round(Math.Abs(pair3.Item2.Y - pair3.Item1.Y), 5), Math.Round(Math.Abs(pair3.Item2.Z - pair3.Item1.Z), 5));
		}
		Vector3d vector3d4 = new Vector3d(Math.Round(Math.Abs(max2D.X - min2D.X), 5), Math.Round(Math.Abs(max2D.Y - min2D.Y), 5), Math.Round(Math.Abs(max2D.Z - min2D.Z), 5));
		string text = this._pathService.PNHOMEDRIVE + this._pathService.PNHOMEPATH + "\\ModelInfo\\";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text = text + doc.DiskFile.Header.ModelName + ".txt";
		if (File.Exists(text))
		{
			File.Delete(text);
		}
		using StreamWriter streamWriter = new StreamWriter(text, append: true);
		streamWriter.WriteLine("File-Name: " + doc.EntryModel3D?.FileName);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Model-Name: " + doc.DiskFile.Header.ModelName);
		streamWriter.WriteLine("Part-Type: " + doc.EntryModel3D?.PartInfo.PartType.ToString());
		streamWriter.WriteLine("Thickness: " + Math.Round(doc.Thickness, 5));
		streamWriter.WriteLine("Material: " + doc.Material.Name);
		streamWriter.WriteLine("Original-Material: " + doc.EntryModel3D?.OriginalMaterialName);
		streamWriter.WriteLine("3D-Material: " + this._materials.GetMaterial3DGroupName(doc.Material.MaterialGroupForBendDeduction));
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Entry-Model-Boundary:");
		streamWriter.WriteLine("LenX: " + vector3d.X + " LenY: " + vector3d.Y + " LenZ: " + vector3d.Z);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Unfold-Model-Boundary:");
		streamWriter.WriteLine("LenX: " + vector3d2.X + " LenY: " + vector3d2.Y + " LenZ: " + vector3d2.Z);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Modified-Model-Boundary:");
		streamWriter.WriteLine("LenX: " + vector3d3.X + " LenY: " + vector3d3.Y + " LenZ: " + vector3d3.Z);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("2D-Model-Boundary:");
		streamWriter.WriteLine("LenX: " + vector3d4.X + " LenY: " + vector3d4.Y + " LenZ: " + vector3d4.Z);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Number-Faces: " + doc.EntryModel3D?.GetAllFaces().Count());
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Number-Bends: " + doc.CombinedBendDescriptors?.Count);
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Bend-Info: ");
		IReadOnlyList<ICombinedBendDescriptorInternal> combinedBendDescriptors = doc.CombinedBendDescriptors;
		if (combinedBendDescriptors == null || !combinedBendDescriptors.Any())
		{
			return;
		}
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in doc.CombinedBendDescriptors)
		{
			var (punchProfile, dieProfile, _) = doc.PreferredProfileStore.GetBestPunchDieProfiles(combinedBendDescriptor);
			streamWriter.WriteLine("No.: " + combinedBendDescriptor.Order + " AngleDeg: " + Math.Round(combinedBendDescriptor[0].BendParams.Angle * 180.0 / Math.PI, 5) + " Original-Radius: " + Math.Round(combinedBendDescriptor[0].BendParams.OriginalRadius, 5) + " Radius: " + Math.Round(combinedBendDescriptor[0].BendParams.FinalRadius, 5) + " BD: " + Math.Round(combinedBendDescriptor[0].BendParams.FinalBendDeduction, 5) + " Din: " + Math.Round(combinedBendDescriptor[0].BendParams.DinLength, 5) + " K-Factor: " + Math.Round(combinedBendDescriptor[0].BendParams.KFactor, 5) + " K.Factor-Algorithm: " + combinedBendDescriptor[0].BendParams.KFactorAlgorithm.ToString() + " Tool-Algorithm: " + combinedBendDescriptor.ToolSelectionAlgorithm.ToString() + " VWidth: " + dieProfile?.VWidth + " VAngle: " + dieProfile?.VAngleDeg + " CornerRadius: " + dieProfile?.CornerRadius + " PunchRadius: " + punchProfile?.Radius);
		}
	}

	private void SendThickness(IDoc3d currentDoc)
	{
		GeneralSystemComponentsAdapter.Thickness = (float)Math.Round(currentDoc.Thickness, 3);
		GeneralSystemComponentsAdapter.UpdateInfoPaneForMaterialAndThickness();
	}
}
