using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BendDataSourceModel.DeepCopy;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.BendTools.TubeInfos;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using Convert = WiCAM.Pn4000.PN3D.Converter.Convert;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class DisassemblyPart
{
    public Action<DisassemblyPart> LoadingCompleted;

    public IDoc3d Doc;

    public WeakReference<IDoc3d> DocTemp;

    public int ID { get; set; }

    public string AssemblyGuid { get; set; }

    public DisassemblyPartHistory PartHistory { get; set; }

    public DisassemblyPartInfo PartInfo { get; set; } = new DisassemblyPartInfo();


    public string Name { get; set; } = "";


    public string OriginalName { get; set; } = "";


    public string OriginalAssemblyName { get; set; } = "";


    public string ModifiedAssemblyName { get; set; } = "";


    public string OriginalGeometryName { get; set; } = "";


    public string ModifiedGeometryName { get; set; } = "";


    public string MaterialName { get; set; } = "";


    public string OriginalMaterialName { get; set; } = "";


    public bool IsAssemblyName { get; set; }

    public int InstanceNumber { get; set; }

    public double LenX { get; set; }

    public double LenY { get; set; }

    public double LenZ { get; set; }

    public double Thickness { get; set; }

    public int PnMaterialID { get; set; }

    public bool PnMaterialByUser { get; set; }

    public int? MachineNo { get; set; }

    public int BendsCount { get; set; }

    public SpecialTools Tools { get; set; } = new SpecialTools();


    public List<string> Instances { get; set; }

    public List<WiCAM.Pn4000.BendModel.Base.Matrix4d> Matrixes { get; set; }

    public List<AssemblyCommonBendInfo> CommonBends { get; set; }

    public List<ValidationIntrinsicError> ValidationIntrinsicErrors { get; set; }

    public List<ValidationDistanceError> ValidationDistanceErrors { get; set; }

    public bool IsLoaded { get; set; }

    public Model ModelLowTesselation { get; set; }

    public bool IsAdditionalPart { get; set; }

    public bool Deleted { get; set; }

    public DocMetadata Metadata { get; set; }

    public bool? ValidationSelfCollision { get; set; }

    public string ImportedFilename { get; set; }

    public bool PartTypeSmallPart { get; set; }

    public List<UserProperty> UserProperties => PartInfo.UserProperties;

    public DisassemblyPart()
    {
    }

    public DisassemblyPart(IDisassemblyPart part)
    {
        if (part == null)
        {
            return;
        }
        ID = part.ID;
        PartHistory = (DisassemblyPartHistory)part.PartHistory;
        PartInfo = new DisassemblyPartInfo
        {
            PartType = (WiCAM.Pn4000.BendModel.BendTools.PartType)part.PartInfo.PartType,
            OriginalPartType = (WiCAM.Pn4000.BendModel.BendTools.PartType)part.PartInfo.OriginalPartType,
            TubeType = (WiCAM.Pn4000.BendModel.BendTools.TubeType)part.PartInfo.TubeType,
            SimulationInstances = new List<WiCAM.Pn4000.BendModel.BendTools.SimulationInstance>(),
            PurchasedPart = part.PartInfo.PurchasedPart,
            IgnoreCollision = part.PartInfo.IgnoreCollision,
            UserProperties = part.PartInfo.UserProperties.Select((WiCAM.Pn4000.PartsReader.DataClasses.UserProperty x) => new UserProperty
            {
                Name = x.Name,
                Properties = x.Properties.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value)
            }).ToList(),
            GeometryType = (GeometryType)part.PartInfo.GeometryType
        };
        if (part.PartInfo.TubeInfo != null)
        {
            Type type = part.PartInfo.TubeInfo.GetType();
            if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)part.PartInfo.TubeInfo).Height;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)part.PartInfo.TubeInfo).Height;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).CornerRadius1 = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).CornerRadius1;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).CornerRadius2 = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).CornerRadius2;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Angle = ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)part.PartInfo.TubeInfo).Angle;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)part.PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).CornerRadius = ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)part.PartInfo.TubeInfo).CornerRadius;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Radius = ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)part.PartInfo.TubeInfo).Radius;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).CornerRadius = ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)part.PartInfo.TubeInfo).CornerRadius;
            }
            else if (type == typeof(WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo))
            {
                PartInfo.TubeInfo = new WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo();
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)part.PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)part.PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)part.PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)part.PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Angle = ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)part.PartInfo.TubeInfo).Angle;
            }
        }
        Name = part.Name;
        OriginalName = part.OriginalName;
        OriginalGeometryName = part.OriginalGeometryName;
        OriginalAssemblyName = part.OriginalAssemblyName;
        ModifiedGeometryName = part.ModifiedGeometryName;
        ModifiedAssemblyName = part.ModifiedAssemblyName;
        MaterialName = part.MaterialName;
        OriginalMaterialName = part.OriginalMaterialName;
        IsAssemblyName = part.IsAssemblyName;
        InstanceNumber = part.InstanceNumber;
        LenX = part.LenX;
        LenY = part.LenY;
        LenZ = part.LenZ;
        Thickness = part.Thickness;
        PnMaterialID = part.PnMaterialID;
        BendsCount = part.BendsCount;
        IsLoaded = part.IsLoaded;
        Deleted = part.Deleted;
        Tools = part.Tools.Copy();
        Instances = part.Instances;
        Matrixes = new List<WiCAM.Pn4000.BendModel.Base.Matrix4d>();
        if (part.Matrixes != null)
        {
            foreach (WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d matrix in part.Matrixes)
            {
                Matrixes.Add(ConvMatrix(matrix));
                static WiCAM.Pn4000.BendModel.Base.Matrix4d ConvMatrix(WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d m)
                {
                    WiCAM.Pn4000.BendModel.Base.Matrix4d result = default(WiCAM.Pn4000.BendModel.Base.Matrix4d);
                    result.M00 = m.M00;
                    result.M01 = m.M01;
                    result.M02 = m.M02;
                    result.M03 = m.M03;
                    result.M10 = m.M10;
                    result.M11 = m.M11;
                    result.M12 = m.M12;
                    result.M13 = m.M13;
                    result.M20 = m.M20;
                    result.M21 = m.M21;
                    result.M22 = m.M22;
                    result.M23 = m.M23;
                    result.M30 = m.M30;
                    result.M31 = m.M31;
                    result.M32 = m.M32;
                    result.M33 = m.M33;
                    return result;
                }
            }
        }
        CommonBends = new List<AssemblyCommonBendInfo>();
        if (part.CommonBends != null)
        {
            foreach (CommonBendInfo commonBend in part.CommonBends)
            {
                CommonBends.Add(new AssemblyCommonBendInfo(commonBend.ID, commonBend.FaceGroupIds, commonBend.BendsCount, commonBend.LengthWithGaps, commonBend.LengthWithoutGaps, commonBend.Angle * Math.PI / 180.0, commonBend.Radius));
            }
        }
        if (part.PartInfo.SimulationInstances == null)
        {
            return;
        }
        foreach (WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance simulationInstance2 in part.PartInfo.SimulationInstances)
        {
            static WiCAM.Pn4000.BendModel.Base.Matrix4d ConvMatrix(WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d m)
            {
                WiCAM.Pn4000.BendModel.Base.Matrix4d result = default(WiCAM.Pn4000.BendModel.Base.Matrix4d);
                result.M00 = m.M00;
                result.M01 = m.M01;
                result.M02 = m.M02;
                result.M03 = m.M03;
                result.M10 = m.M10;
                result.M11 = m.M11;
                result.M12 = m.M12;
                result.M13 = m.M13;
                result.M20 = m.M20;
                result.M21 = m.M21;
                result.M22 = m.M22;
                result.M23 = m.M23;
                result.M30 = m.M30;
                result.M31 = m.M31;
                result.M32 = m.M32;
                result.M33 = m.M33;
                return result;
            }
            WiCAM.Pn4000.BendModel.BendTools.SimulationInstance simulationInstance = new WiCAM.Pn4000.BendModel.BendTools.SimulationInstance
            {
                WorldMatrix = ConvMatrix(simulationInstance2.WorldMatrix),
                AdditionalParts = new List<WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometry>()
            };
            PartInfo.SimulationInstances.Add(simulationInstance);
            foreach (WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchasedPart purchasedPart in simulationInstance2.PurchasedParts)
            {
                WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometry additionalGeometry = new WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometry
                {
                    Model = null,
                    ModelUid = purchasedPart.PurchasedModelUid,
                    AssemblyName = purchasedPart.AssemblyName,
                    IgnoreCollisionInSimulation = purchasedPart.IgnoreCollision,
                    Instances = new List<WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometryInstance>()
                };
                simulationInstance.AdditionalParts.Add(additionalGeometry);
                foreach (WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchaseInstance purchasedInstance in purchasedPart.PurchasedInstances)
                {
                    additionalGeometry.Instances.Add(new WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometryInstance
                    {
                        FaceGroupId = purchasedInstance.FaceId,
                        Transformation = ConvMatrix(purchasedInstance.Transformation)
                    });
                }
            }
        }
    }

    public DisassemblyPart(AssemblyPartInfo partInfo, MinMax modelBoundary)
        : this()
    {
        ID = partInfo.ID;
        LenX = modelBoundary.LenX();
        LenY = modelBoundary.LenY();
        LenZ = modelBoundary.LenZ();
        OriginalGeometryName = partInfo.Name;
        OriginalAssemblyName = partInfo.AssemblyName;
        ModifiedGeometryName = ReplaceInvalidChars(OriginalGeometryName);
        ModifiedAssemblyName = ReplaceInvalidChars(OriginalAssemblyName);
        Name = partInfo.AssemblyName;
        OriginalName = partInfo.AssemblyName;
        InstanceNumber = partInfo.Count;
        MaterialName = partInfo.MaterialName;
        OriginalMaterialName = partInfo.OriginalMaterialName;
        Instances = partInfo.Instances;
        Matrixes = partInfo.Matrixes;
        CommonBends = new List<AssemblyCommonBendInfo>();
        PartInfo.GeometryType = partInfo.GeometryType;
    }

    public DisassemblyPart(AssemblyNode node, AssemblyBody body, string assemblyGuid)
    {
        ID = body.BodyId.Value;
        AssemblyGuid = assemblyGuid;
        OriginalGeometryName = node.Name;
        OriginalAssemblyName = node.RefName;
        ModifiedGeometryName = ReplaceInvalidChars(OriginalGeometryName);
        ModifiedAssemblyName = ReplaceInvalidChars(OriginalAssemblyName);
        OriginalMaterialName = body.Material;
        if (body.Dimension.HasValue)
        {
            DisassemblyPartInfo partInfo = PartInfo;
            partInfo.GeometryType = body.Dimension.Value switch
            {
                1 => GeometryType.Contour,
                2 => GeometryType.Surface,
                _ => GeometryType.Volume,
            };
        }
        CommonBends = new List<AssemblyCommonBendInfo>();
    }

    private string ReplaceInvalidChars(string str)
    {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        foreach (char oldChar in invalidFileNameChars)
        {
            str = str.Replace(oldChar, '_');
        }
        return str.Replace('.', '_');
    }

    public void LoadDataFromMetadata()
    {
        PnMaterialID = Metadata.PnMaterialNo.GetValueOrDefault(-1);
        PnMaterialByUser = Metadata.PnMaterialByUser;
        MachineNo = Metadata.BendMachineNo;
        PartTypeSmallPart = Metadata.SheetType.HasFlag(WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart);
        PartInfo.OriginalPartType = Metadata.SheetType;
        PartInfo.PartType = Metadata.SheetType;
    }

    public void SaveDataToMetadata()
    {
        if (Metadata != null)
        {
            Metadata.BendMachineNo = MachineNo;
            Metadata.PnMaterialNo = ((PnMaterialID < 0) ? null : new int?(PnMaterialID));
            Metadata.PnMaterialByUser = PnMaterialByUser;
            WiCAM.Pn4000.BendModel.BendTools.PartType partType = Metadata.SheetType | WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
            if (!PartTypeSmallPart)
            {
                partType ^= WiCAM.Pn4000.BendModel.BendTools.PartType.SmallPart;
            }
            Metadata.SheetType = partType;
        }
    }

    public IDoc3d GetDoc(bool force)
    {
        if (Doc != null)
        {
            return Doc;
        }
        IDoc3d target = null;
        DocTemp?.TryGetTarget(out target);
        if (target != null)
        {
            return target;
        }
        if (force)
        {
            throw new Exception("force not implemented yet");
        }
        return null;
    }

    public string GetSizeString(bool isInchMode)
    {
        double x = Metadata?.LenX ?? LenX;
        double y = Metadata?.LenY ?? LenY;
        double z = Metadata?.LenZ ?? LenZ;
        string unit = isInchMode ? "inch" : "mm";

        if (isInchMode)
        {
            x = Convert.MmToInch(x);
            y = Convert.MmToInch(y);
            z = Convert.MmToInch(z);
        }

        return string.Format(CultureInfo.InvariantCulture,
            "{0:0.00} x {1:0.00} x {2:0.00} {3}",
            x, y, z, unit);
    }

    public WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart GetConvertedPart()
    {
        WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart disassemblyPart = new WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPart
        {
            ID = ID,
            PartHistory = (WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPartHistory)PartHistory,
            PartInfo = new WiCAM.Pn4000.PartsReader.DataClasses.DisassemblyPartInfo
            {
                PartType = (WiCAM.Pn4000.PartsReader.DataClasses.PartType)PartInfo.PartType,
                OriginalPartType = (WiCAM.Pn4000.PartsReader.DataClasses.PartType)PartInfo.OriginalPartType,
                TubeType = (WiCAM.Pn4000.PartsReader.DataClasses.TubeType)PartInfo.TubeType,
                SimulationInstances = new List<WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance>(),
                PurchasedPart = PartInfo.PurchasedPart,
                IgnoreCollision = PartInfo.IgnoreCollision,
                UserProperties = PartInfo.UserProperties.Select((UserProperty x) => new WiCAM.Pn4000.PartsReader.DataClasses.UserProperty
                {
                    Name = x.Name,
                    Properties = x.Properties.ToDictionary((KeyValuePair<string, string> x) => x.Key, (KeyValuePair<string, string> x) => x.Value)
                }).ToList(),
                GeometryType = (int)PartInfo.GeometryType
            },
            Name = Name,
            OriginalName = OriginalName,
            OriginalGeometryName = OriginalGeometryName,
            OriginalAssemblyName = OriginalAssemblyName,
            ModifiedGeometryName = ModifiedGeometryName,
            ModifiedAssemblyName = ModifiedAssemblyName,
            MaterialName = MaterialName,
            OriginalMaterialName = OriginalMaterialName,
            IsAssemblyName = IsAssemblyName,
            InstanceNumber = InstanceNumber,
            LenX = LenX,
            LenY = LenY,
            LenZ = LenZ,
            Thickness = Thickness,
            PnMaterialID = PnMaterialID,
            BendsCount = BendsCount,
            Instances = Instances,
            IsLoaded = IsLoaded,
            Deleted = Deleted,
            Matrixes = new List<WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d>(),
            CommonBends = new List<CommonBendInfo>(),
            ValidationIntrinsicErrors = null,
            ValidationDistanceErrors = null,
            ValidationSelfCollision = null
        };
        if (PartInfo.TubeInfo != null)
        {
            Type type = PartInfo.TubeInfo.GetType();
            if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.EllipsoidTubeInfo)disassemblyPart.PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.EllipsoidTubeInfo)PartInfo.TubeInfo).Height;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.OvalTubeInfo)disassemblyPart.PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.OvalTubeInfo)PartInfo.TubeInfo).Height;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).CornerRadius1 = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).CornerRadius1;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).CornerRadius2 = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).CornerRadius2;
                ((WiCAM.Pn4000.PartsReader.DataClasses.ParallelogramTubeInfo)disassemblyPart.PartInfo.TubeInfo).Angle = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.ParallelogramTubeInfo)PartInfo.TubeInfo).Angle;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RectangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).CornerRadius = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RectangularTubeInfo)PartInfo.TubeInfo).CornerRadius;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.RoundTubeInfo)disassemblyPart.PartInfo.TubeInfo).Radius = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.RoundTubeInfo)PartInfo.TubeInfo).Radius;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.SquareTubeInfo)disassemblyPart.PartInfo.TubeInfo).CornerRadius = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.SquareTubeInfo)PartInfo.TubeInfo).CornerRadius;
            }
            else if (type == typeof(WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo))
            {
                disassemblyPart.PartInfo.TubeInfo = new WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo();
                ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Thickness = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Thickness;
                ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Length = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Length;
                ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Width = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Width;
                ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Height = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Height;
                ((WiCAM.Pn4000.PartsReader.DataClasses.TriangularTubeInfo)disassemblyPart.PartInfo.TubeInfo).Angle = ((WiCAM.Pn4000.BendModel.BendTools.TubeInfos.TriangularTubeInfo)PartInfo.TubeInfo).Angle;
            }
        }
        disassemblyPart.Tools = Tools.Copy();
        if (Matrixes != null)
        {
            foreach (WiCAM.Pn4000.BendModel.Base.Matrix4d matrix in Matrixes)
            {
                disassemblyPart.Matrixes.Add(ConvMatrix(matrix));
            }
        }
        if (CommonBends != null)
        {
            foreach (AssemblyCommonBendInfo commonBend in CommonBends)
            {
                disassemblyPart.CommonBends.Add(new CommonBendInfo
                {
                    ID = commonBend.ID,
                    FaceGroupIds = commonBend.FaceGroupIds,
                    BendsCount = commonBend.BendsCount,
                    LengthWithGaps = commonBend.LengthWithGaps,
                    LengthWithoutGaps = commonBend.LengthWithoutGaps,
                    Angle = commonBend.Angle * 180.0 / Math.PI,
                    Radius = commonBend.Radius
                });
            }
        }
        if (ValidationIntrinsicErrors != null)
        {
            disassemblyPart.ValidationIntrinsicErrors = new List<ValidationIntrinsicError>(ValidationIntrinsicErrors);
        }
        if (ValidationDistanceErrors != null)
        {
            disassemblyPart.ValidationDistanceErrors = new List<ValidationDistanceError>(ValidationDistanceErrors);
        }
        disassemblyPart.ValidationSelfCollision = ValidationSelfCollision;
        if (PartInfo.SimulationInstances != null)
        {
            foreach (WiCAM.Pn4000.BendModel.BendTools.SimulationInstance simulationInstance2 in PartInfo.SimulationInstances)
            {
                WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance simulationInstance = new WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance
                {
                    WorldMatrix = ConvMatrix(simulationInstance2.WorldMatrix),
                    PurchasedParts = new List<WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchasedPart>()
                };
                disassemblyPart.PartInfo.SimulationInstances.Add(simulationInstance);
                foreach (WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometry additionalPart in simulationInstance2.AdditionalParts)
                {
                    WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchasedPart purchasedPart = new WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchasedPart
                    {
                        PurchasedModelUid = additionalPart.ModelUid,
                        IgnoreCollision = additionalPart.IgnoreCollisionInSimulation,
                        AssemblyName = additionalPart.AssemblyName,
                        PurchasedInstances = new List<WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchaseInstance>()
                    };
                    simulationInstance.PurchasedParts.Add(purchasedPart);
                    foreach (WiCAM.Pn4000.BendModel.BendTools.SimulationInstance.AdditionalGeometryInstance instance in additionalPart.Instances)
                    {
                        purchasedPart.PurchasedInstances.Add(new WiCAM.Pn4000.PartsReader.DataClasses.SimulationInstance.PurchaseInstance
                        {
                            FaceId = instance.FaceGroupId,
                            Transformation = ConvMatrix(instance.Transformation)
                        });
                    }
                }
            }
        }
        return disassemblyPart;
        static WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d ConvMatrix(WiCAM.Pn4000.BendModel.Base.Matrix4d m)
        {
            return new WiCAM.Pn4000.PartsReader.DataClasses.Matrix4d
            {
                M00 = m.M00,
                M01 = m.M01,
                M02 = m.M02,
                M03 = m.M03,
                M10 = m.M10,
                M11 = m.M11,
                M12 = m.M12,
                M13 = m.M13,
                M20 = m.M20,
                M21 = m.M21,
                M22 = m.M22,
                M23 = m.M23,
                M30 = m.M30,
                M31 = m.M31,
                M32 = m.M32,
                M33 = m.M33
            };
        }
    }

    public void CalcLen(Model model)
    {
        if (model != null)
        {
            Pair<WiCAM.Pn4000.BendModel.Base.Vector3d, WiCAM.Pn4000.BendModel.Base.Vector3d> boundary = model.GetBoundary(WiCAM.Pn4000.BendModel.Base.Matrix4d.Identity);
            WiCAM.Pn4000.BendModel.Base.Vector3d vector3d = boundary.Item2 - boundary.Item1;
            LenX = vector3d.X;
            LenY = vector3d.Y;
            LenZ = vector3d.Z;
        }
        else
        {
            LenX = double.NaN;
            LenY = double.NaN;
            LenZ = double.NaN;
        }
    }
}
