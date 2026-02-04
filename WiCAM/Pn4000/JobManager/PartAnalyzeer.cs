
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.Services.Extensions;
using Color = WiCAM.Pn4000.BendModel.Base.Color;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Services.BendTools.IrregularBendsAnalyzer;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Screen;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.BendModel.Config;
using WiCAM.Pn4000.BendModel.Services.BendTools.PartAnalyzer;
using WiCAM.Pn4000.BendModel.Services.BendTools.PartAnalyzer.Utils;

namespace Sense3D.SenseBendModel.BendModelServices.BendTools.PartAnalyzer;
public class PartAnalyzeer : IPartAnalyzer
{
    private readonly IIrregularBendsAnalyzer _irregularBendsAnalyzer;
    //IIrregularBendsAnalyzer irregularBendsAnalyzer
    public PartAnalyzeer()
    {
        //this._irregularBendsAnalyzer = irregularBendsAnalyzer;
    }


    public bool AnalyzeParts(Model model, AnalyzeConfig analyzeConfig, IImportArg? importSettings = null)
    {
        using (new ActivitySource("Sense3D").StartActivity("PartAnalyzer - AnalyzeParts"))
        {
            bool enabled = analyzeConfig.ReconstructBendsConfig.Enabled;
            if ((importSettings != null && importSettings.ReconstructBends == IImportArg.ReconstructBendsMode.AlwaysReconstruct) || ((importSettings == null || !importSettings.ReconstructBends.HasValue) && analyzeConfig.ReconstructBendsConfig.ReconstructAfterImportMode == ReconstructionMode.AlwaysReconstruct))
            {
                analyzeConfig.ReconstructBendsConfig.Enabled = true;
            }
            PartAnalyzeer.ResetModel(model);
            bool flag = true;
            foreach (Shell shell in model.Shells)
            {
                flag &= this.AnalyzePart(shell, model, analyzeConfig);
            }
            analyzeConfig.ReconstructBendsConfig.Enabled = enabled;
            return flag;
        }
    }

    public bool AnalyzePart(Shell shell, Model model, AnalyzeConfig analyzeConfig)
    {
        using (new ActivitySource("Sense3D").StartActivity("PartAnalyzer - AnalyzePart"))
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (model.PartInfo.PartType.HasFlag(PartType.Error) || !shell.IsMeshClosed() || !shell.HasMeshNoDegeneratedTriangles() || !shell.ThereAreNoDuplicatedEdges() || !shell.IsEdgeTopologyClosedAndVisualize())
            {
                stopwatch.Stop();
                model.PartInfo.PartType = PartType.Error | PartType.OpenMesh;
                model.CreateCollisionTree();
                PartAnalyzeer.HandleUnassignedFaces(shell);
                return false;
            }
            shell.Faces.SelectMany((Face f) => f.Mesh).Count();
            _ = shell.VertexCache.Count;
            stopwatch.Stop();
            stopwatch.Restart();
          //  shell.GetFaceGroupsAndThickness(detectTubes: true);
            stopwatch.Stop();
            stopwatch.Restart();
            if (analyzeConfig.ReconstructBendsConfig.Enabled)
            {
                try
                {
                   // this._irregularBendsAnalyzer.ReconstructIrregularBends(shell, analyzeConfig.ReconstructBendsConfig);
                    stopwatch.Stop();
                    stopwatch.Restart();
                    PartAnalyzeer.ResetShell(shell);
              //      shell.GetFaceGroupsAndThickness(detectTubes: true);
                    stopwatch.Stop();
                    stopwatch.Restart();
                }
                catch
                {
                    model.PartInfo.PartType = PartType.InvalidGeometry | PartType.Error;
                    shell.CreateCollisionTree();
                    PartAnalyzeer.HandleUnassignedFaces(shell);
                    return false;
                }
            }
            try
            {
              //  if (!shell.FindRoundGroupConnectingFaces())
                {
                  //  model.PartInfo.PartType = PartType.InvalidGeometry | PartType.Error;
                  //  shell.CreateCollisionTree();
                  //  PartAnalyzeer.HandleUnassignedFaces(shell);
                //    return false;
                }
                if (!shell.IsEdgeTopologyClosedAndVisualize())
                {
                    shell.CreateCollisionTree();
                    PartAnalyzeer.HandleUnassignedFaces(shell);
                    return false;
                }
            }
            catch
            {
                model.PartInfo.PartType = PartType.InvalidGeometry | PartType.Error;
                shell.CreateCollisionTree();
                PartAnalyzeer.HandleUnassignedFaces(shell);
                return false;
            }
         //   shell.RemoveEmptyFaceGroups();
            stopwatch.Stop();
            stopwatch.Restart();
       //     shell.FindFlatGroupConnectingFaces();
            stopwatch.Stop();
            stopwatch.Restart();
         //   model.GenerateFaceGroupGraph();
            stopwatch.Stop();
          //  model.AssignFaceGroupFlags();
            stopwatch.Restart();
          //  model.FindJointBendingZones();
            stopwatch.Stop();
            stopwatch.Restart();
         ///   shell.AnalyzePartType(analyzeConfig, model, detectTube: true);
            if (model.PartInfo.PartType.HasFlag(PartType.Unknown))
            {
             //   PartAnalyzer.ResetModel(model);
            //    shell.GetFaceGroupsAndThickness(detectTubes: false);
          //      if (!shell.FindRoundGroupConnectingFaces())
            //    {
           //         model.PartInfo.PartType = PartType.InvalidGeometry | PartType.Error;
            //        shell.CreateCollisionTree();
             //       PartAnalyzeer.HandleUnassignedFaces(shell);
             //       return false;
             //   }
              //  shell.RemoveEmptyFaceGroups();
            //    shell.FindFlatGroupConnectingFaces();
             //   model.GenerateFaceGroupGraph();
             //   model.AssignFaceGroupFlags();
             //   model.FindJointBendingZones();
                stopwatch.Stop();
            }
            stopwatch.Restart();
        //    shell.CheckAndCorrectFaceGroupCount();
         //   shell.CreateHoleHirarchy();
            stopwatch.Stop();
            stopwatch.Restart();
            shell.CreateCollisionTree();
            stopwatch.Stop();
            try
            {
                stopwatch.Restart();
                shell.AnalyzeMacros(model, analyzeConfig);
                stopwatch.Stop();
            }
            catch (Exception)
            {
                model.PartInfo.PartType = PartType.InvalidGeometry | PartType.Error;
                PartAnalyzeer.HandleUnassignedFaces(shell);
                return false;
            }
            stopwatch.Restart();
          //  model.MergeInvalidBendsWithFlat();
            stopwatch.Stop();
            stopwatch.Restart();
           // shell.AnalyzePartType(analyzeConfig, model, model.PartInfo.PartType.HasFlag(PartType.Tube));
            stopwatch.Stop();
            stopwatch.Restart();
            if (!shell.IsMeshClosed() || !shell.ThereAreNoDuplicatedEdges() || !shell.IsEdgeTopologyClosedAndVisualize())
            {
                stopwatch.Stop();
                model.PartInfo.PartType = PartType.Error | PartType.OpenMeshDuringCalculation;
                shell.CreateCollisionTree();
                PartAnalyzeer.HandleUnassignedFaces(shell);
                return false;
            }
            stopwatch.Stop();
            if (model.PartInfo.PartType.HasFlag(PartType.Tube) && model.PartInfo.TubeType == TubeType.RoundTube)
            {
                stopwatch.Restart();
           //     shell.CutRoundTube();
                stopwatch.Stop();
                stopwatch.Restart();
                shell.CreateCollisionTree();
                stopwatch.Stop();
            }
            foreach (FaceGroup roundFaceGroup in shell.RoundFaceGroups)
            {
                roundFaceGroup.OriginalBendRadius = roundFaceGroup.ConcaveAxis.Radius;
            }
            PartAnalyzeer.HandleUnassignedFaces(shell);
            return true;
        }
    }

    public static void HandleUnassignedFaces(Shell shell)
    {
        IEnumerable<Face> enumerable = shell.Faces.Where((Face f) => f.FaceGroup == null);
        if (!enumerable.Any())
        {
            return;
        }
        FaceGroup faceGroup = new FaceGroup();
        shell.FlatFaceGroups.Add(faceGroup);
        foreach (Face item in enumerable)
        {
            faceGroup.ConnectingFaces.Add(item);
        }
    }

    public static void ResetModel(Model model)
    {
      //  model.PartInfo = new PartInfo();
        model.PartInfo.NotConformFaces.Clear();
        foreach (Shell shell in model.Shells)
        {
            PartAnalyzeer.ResetShell(shell);
        }
    }

    public static void ResetShell(Shell shell)
    {
        shell.RoundFaceGroups.Clear();
        shell.RoundFaceGroupsByFace.Clear();
        shell.FlatFaceGroups.Clear();
        shell.FlatFaceGroupsByFace.Clear();
        shell.Macros.Clear();
        foreach (Face face in shell.Faces)
        {
            face.Color = new Color(0.5f, 0.5f, 0.5f, 1f);
            face.Macro = null;
        }
    }

    public void AnalyzeParts(DisassemblySimpleModel mainModel, IDoc3d doc, string fileName,
        Dictionary<string, int> materialImportToPnMatId, out List<DisassemblyPart> disassemblyParts, bool useBackgroundWorker, IScreenshotScreen screenshotScreen)
    {
        throw new NotImplementedException();
    }

    public void Analyze(DisassemblyPart part, IMessageDisplay messageDisplay, int partsCount,
        IDoc3d orgDoc, bool isAssembly, Dictionary<string, int> materialImportToPnMatId,
        out HashSet<Triple<FaceGroup, double, double>> notAdjustableBends,
        out Dictionary<int, BendTableReturnValues> bendTableResults, out Model entryModel3D,
        List<PurchasedPartsMerger.PurchasedPart> purchasedParts = null, List<Parts1dMerger.Part1d> parts1d = null, IImportArg importSettings = null)
    {
        throw new NotImplementedException();
    }
}
