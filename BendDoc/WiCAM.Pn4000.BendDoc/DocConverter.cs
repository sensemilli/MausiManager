using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine.FingerStop;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Adapter;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.BendDoc.Services;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendModel.Serialization;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.Materials.Contracts.Matcfg;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Doc.Serializer;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;
using WiCAM.Pn4000.PN3D.FingerStop;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.BendDoc;

internal class DocConverter : IDocConverter
{
    [CompilerGenerated]
    private IPnPathService _003C_pathService_003EP;

    [CompilerGenerated]
    private IInternalDocFactory _003C_docFactory_003EP;

    [CompilerGenerated]
    private IToolsAndBendsSerializer _003C_toolSerializer_003EP;

    [CompilerGenerated]
    private IToolFactory _003C_toolFactory_003EP;

    [CompilerGenerated]
    private IToolOperator _003C_toolOperator_003EP;

    private Dictionary<int, HolderPart> dictUpperHolderParts;

    private Dictionary<int, HolderPart> dictLowerHolderParts;

    private Dictionary<int, PunchPart> dictPunchParts;

    private Dictionary<int, DiePart> dictDieParts;

    private Dictionary<int, AdapterPart> dictUpperAdapterParts;

    private Dictionary<int, AdapterPart> dictLowerAdapterParts;

    private FingerPositionConverter _fingerPositionConverter;

    public DocConverter(IPnPathService _pathService, IInternalDocFactory _docFactory, IToolsAndBendsSerializer _toolSerializer, IToolFactory _toolFactory, IToolOperator _toolOperator)
    {
        _003C_pathService_003EP = _pathService;
        _003C_docFactory_003EP = _docFactory;
        _003C_toolSerializer_003EP = _toolSerializer;
        _003C_toolFactory_003EP = _toolFactory;
        _003C_toolOperator_003EP = _toolOperator;
        dictUpperHolderParts = new Dictionary<int, HolderPart>();
        dictLowerHolderParts = new Dictionary<int, HolderPart>();
        dictPunchParts = new Dictionary<int, PunchPart>();
        dictDieParts = new Dictionary<int, DiePart>();
        dictUpperAdapterParts = new Dictionary<int, AdapterPart>();
        dictLowerAdapterParts = new Dictionary<int, AdapterPart>();
        _fingerPositionConverter = new FingerPositionConverter();
     //   base._002Ector();
    }

    public SPnBndDoc ConvertDoc(IDoc3d doc3d, bool skipBendModel)
    {
        if (doc3d is Doc3d doc3d2)
        {
            ModelConverter modelConverter = new ModelConverter();
            SPnBndDoc sPnBndDoc = new SPnBndDoc
            {
                UseDINUnfold = doc3d2.UseDINUnfold,
                Material = Convert(doc3d2.Material),
                InputModel = (skipBendModel ? null : modelConverter.Convert(doc3d2._data.InputModel3D)),
                EntryModel = (skipBendModel ? null : modelConverter.Convert(doc3d2._data.EntryModel3D)),
                BendMachine = new SBndMachineExpert
                {
                    MachinePath = doc3d2?.MachinePath?.Split('\\').Last(),
                    ToolSelectionType = doc3d2.ToolSelectionType
                },
                PreferredProfileStore = Convert(doc3d2.PreferredProfileStore),
                Thickness = doc3d2.Thickness,
                Comment = doc3d2.Comment,
                AmountInAssembly = doc3d2.AmountInAssembly,
                Classification = doc3d2.Classification,
                DrawingNumber = doc3d2.DrawingNumber,
                PnMaterialByUser = doc3d2.PnMaterialByUser,
                KFactorWarningsAcceptedByUser = doc3d2.KFactorWarningsAcceptedByUser,
                NamePP = null,
                NamesPp = doc3d2.NamesPpBase.ToList(),
                NumberPp = doc3d2.NumberPp,
                UserCommentsInt = doc3d2.UserComments.ToDictionary<KeyValuePair<int, string>, int, string>((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value),
                VisibleFaceGroupId = doc3d2.VisibleFaceGroupId,
                VisibleFaceGroupSide = doc3d2.VisibleFaceGroupSide,
                DiskFile = Convert(doc3d2.DiskFile),
                IsReconstructed = doc3d2.IsReconstructed,
                AssemblyGuid = doc3d2.AssemblyGuid,
                CreationUserName = doc3d2.CreationUserName,
                LastModifiedUserName = doc3d2.LastModifiedUserName,
                CreationDate = doc3d2.CreationDate,
                LastModified = doc3d2.LastModified,
                FreezeCombinedBendDescriptors = doc3d2.FreezeCombinedBendDescriptors
            };
            if (doc3d2.SimulationInstancesAdditionalParts != null)
            {
                sPnBndDoc.SimulationInstancesAdditionalParts = doc3d2.SimulationInstancesAdditionalParts.Select(delegate (SimulationInstance x)
                {
                    SSimulationInstance sSimulationInstance = new SSimulationInstance
                    {
                        WorldMatrix = x.WorldMatrix
                    };
                    if (x.AdditionalParts != null)
                    {
                        sSimulationInstance.AdditionalParts = x.AdditionalParts.Select((SimulationInstance.AdditionalGeometry pp) => new SSimulationInstance.SPurchasedPart
                        {
                            AssemblyName = pp.AssemblyName,
                            IgnoreCollisionInSimulation = pp.IgnoreCollisionInSimulation,
                            UserProperties = pp.UserProperties,
                            ModelUid = pp.ModelUid,
                            Model = modelConverter.Convert(pp.Model),
                            AdditionalGeometryType = pp.AdditionalGeometryType,
                            PurchasedInstances = pp.Instances.Select((SimulationInstance.AdditionalGeometryInstance pi) => new SSimulationInstance.SPurchaseInstance
                            {
                                FaceGroupId = pi.FaceGroupId,
                                Transformation = pi.Transformation
                            }).ToList()
                        }).ToList();
                    }
                    return sSimulationInstance;
                }).ToList();
            }
            ConvertCombinedBends(doc3d2, sPnBndDoc);
            sPnBndDoc.ToolsAndBends = ((doc3d2.ToolsAndBends != null) ? (_003C_toolSerializer_003EP.Convert(doc3d2.ToolsAndBends) as SToolsAndBends) : null);
            sPnBndDoc.FingerStopPositionData = _fingerPositionConverter.ConvertStopPositionData();
            sPnBndDoc.FingerStopFaceDescriptorData = _fingerPositionConverter.ConvertStopFaceDescriptorData();
            return sPnBndDoc;
        }
        throw new Exception("Don't know how to convert this object. Need object of type Doc3d");
    }

    public IDoc3d ConvertDoc(SPnBndDoc sDoc, string sourcePath)
    {
        Doc3d doc3d = _003C_docFactory_003EP.InternalCreateDoc(sourcePath);
        ConvertDoc(sDoc, doc3d, sourcePath);
        return doc3d;
    }

    public void ConvertDoc(SPnBndDoc sDoc, IDoc3d doc3d, string sourcePath)
    {
        if (!(doc3d is Doc3d doc3d2))
        {
            return;
        }
        ModelConverter modelConverter = new ModelConverter();
        dictUpperHolderParts = new Dictionary<int, HolderPart>();
        dictLowerHolderParts = new Dictionary<int, HolderPart>();
        doc3d2.IsSerialized = true;
        doc3d2.FreezeCombinedBendDescriptors = sDoc.FreezeCombinedBendDescriptors;
        doc3d2.UseDINUnfold = sDoc.UseDINUnfold;
        doc3d2.Material = Convert(sDoc.Material);
        doc3d2.MachinePath = (string.IsNullOrEmpty(sDoc.BendMachine.MachinePath) ? null : _003C_pathService_003EP.GetMachine3DFolder(sDoc.BendMachine.MachinePath));
        doc3d2.Comment = sDoc.Comment;
        doc3d2.AmountInAssembly = sDoc.AmountInAssembly;
        doc3d2.Classification = sDoc.Classification;
        doc3d2.DrawingNumber = sDoc.DrawingNumber;
        doc3d2.PnMaterialByUser = sDoc.PnMaterialByUser;
        doc3d2.KFactorWarningsAcceptedByUser = sDoc.KFactorWarningsAcceptedByUser;
        doc3d2.NamePPBase = sDoc.NamePP;
        doc3d2._data.NamesPpBase = sDoc.NamesPp.ToList();
        if (sDoc.NamePP != null && !doc3d2.NamesPpBase.Any())
        {
            doc3d2._data.NamesPpBase.Add(sDoc.NamePP);
        }
        doc3d2.NumberPp = sDoc.NumberPp;
        doc3d2.UserComments = sDoc.UserCommentsInt?.ToDictionary((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value) ?? new Dictionary<int, string>();
        doc3d2.Thickness = sDoc.Thickness;
        doc3d2.VisibleFaceGroupId = sDoc.VisibleFaceGroupId;
        doc3d2.VisibleFaceGroupSide = sDoc.VisibleFaceGroupSide;
        doc3d2.AssemblyGuid = sDoc.AssemblyGuid;
        doc3d2.CreationUserName = sDoc.CreationUserName;
        doc3d2.LastModifiedUserName = sDoc.LastModifiedUserName;
        doc3d2.CreationDate = sDoc.CreationDate;
        doc3d2.LastModified = sDoc.LastModified;
        doc3d2.DiskFile = Convert(sDoc.DiskFile) ?? new PnBndFile(sourcePath);
        if (doc3d2.DiskFile.Header.FileVersion == "1.0.0.3")
        {
            throw new Exception("loading of SPnBndDoc version " + doc3d2.DiskFile.Header.FileVersion + " is not supported.");
        }
        if (sDoc.InputModel == null)
        {
            doc3d2._data.InputModel3D = modelConverter.Convert(sDoc.EntryModel);
        }
        else
        {
            doc3d2._data.InputModel3D = modelConverter.Convert(sDoc.InputModel);
            doc3d2._data.EntryModel3D = modelConverter.Convert(sDoc.EntryModel);
        }
        doc3d2.SimulationInstancesAdditionalParts = modelConverter.ConvertSimulationInstancesAdditionalParts(sDoc.SimulationInstancesAdditionalParts ?? sDoc.EntryModel?.PartInfo.SimulationInstances ?? sDoc.InputModel?.PartInfo.SimulationInstances);
        if (doc3d2.BendMachine != null)
        {
            _fingerPositionConverter.ReadStopFaceDescriptorData(sDoc.FingerStopFaceDescriptorData, doc3d2.BendMachine.Geometry.LeftFinger.FingerModel, doc3d2.BendMachine.Geometry.RightFinger.FingerModel);
        }
        _fingerPositionConverter.ReadStopPositionData(sDoc.FingerStopPositionData);
        ConvertCombinedBends(sDoc, doc3d2);
        if (sDoc.Tools != null && doc3d2.BendMachine != null)
        {
            MigrateToToolsAndBends(sDoc, doc3d2);
        }
        if (sDoc.ToolsAndBends != null && doc3d2.BendMachine != null)
        {
            if (doc3d2.DiskFile.Header.FileVersion == "1.0.0.4")
            {
                for (int i = 0; i < doc3d2.CombinedBendDescriptors.Count; i++)
                {
                    ICombinedBendDescriptorInternal combinedBendDescriptorInternal = doc3d2.CombinedBendDescriptors[i];
                    sDoc.ToolsAndBends.BendPositions[i].Bend.FaceGroupIdentifiers = combinedBendDescriptorInternal.Enumerable.Select((IBendDescriptor x) => x.BendParams.UnfoldFaceGroup.ToFaceGroupIdentifier()).ToList();
                }
                Dictionary<int, int> bendIndex = new Dictionary<int, int>();
                foreach (var item in sDoc.ToolsAndBends.BendPositions.SelectMany((SBendPositioning x) => x.Bend.FaceGroupIdentifiers))
                {
                    if (!bendIndex.TryGetValue(item.bendEntryId, out var value) || item.subBendIndex.GetValueOrDefault() >= value)
                    {
                        bendIndex[item.bendEntryId] = item.subBendIndex.GetValueOrDefault();
                    }
                }
                foreach (SBendPositioning bendPosition in sDoc.ToolsAndBends.BendPositions)
                {
                    bendPosition.Bend.FaceGroupIdentifiers = bendPosition.Bend.FaceGroupIdentifiers.Select(delegate ((int bendEntryId, int? subBendCount, int? subBendIndex) x)
                    {
                        x.subBendCount = bendIndex[x.bendEntryId] + 1;
                        if (x.subBendCount.GetValueOrDefault() == 1)
                        {
                            x.subBendCount = null;
                            x.subBendIndex = null;
                        }
                        return x;
                    }).ToList();
                }
            }
            doc3d2.UpdateDoc();
            try
            {
                SToolsAndBends toolsAndBends = sDoc.ToolsAndBends;
                IToolsAndBends toolsAndBends2 = _003C_toolSerializer_003EP.ConvertBack(toolsAndBends, doc3d2.BendMachine.ToolConfig);
                if (string.IsNullOrEmpty(doc3d2.DiskFile.Header.PkernelVersion))
                {
                    List<double> list = toolsAndBends2.BendPositions.Select((IBendPositioning x) => x.BendingZonesOrientated.First().Start + x.OffsetWorldX).ToList();
                    doc3d2.ToolsAndBends = toolsAndBends2;
                    for (int j = 0; j < list.Count; j++)
                    {
                        IBendPositioning bendPositioning = toolsAndBends2.BendPositions[j];
                        double num = list[j];
                        double num2 = bendPositioning.BendingZonesOrientated.First().Start + bendPositioning.OffsetWorldX;
                        double num3 = num - num2;
                        bendPositioning.OffsetWorldX += num3;
                    }
                }
                else
                {
                    doc3d2.ToolsAndBends = toolsAndBends2;
                }
            }
            catch (DeletedToolsException)
            {
                doc3d2.MessageDisplay.ShowTranslatedErrorMessage("DocDeserialize.DeletedToolsException");
            }
        }
        doc3d2.UpdateDoc();
        doc3d2.IsSerialized = false;
    }

    private void MigrateToToolsAndBends(SPnBndDoc sDoc, Doc3d doc)
    {
        IMigrationToolIdMapping migrationToolMapping = doc.BendMachine.MigrationToolMapping;
        Dictionary<int, IToolPieceProfile> dictionary = doc.BendMachine.ToolConfig.AllToolPieceProfiles.ToDictionary((IToolPieceProfile x) => x.ToolId);
        Dictionary<int, IToolProfile> dictionary2 = doc.BendMachine.ToolConfig.AllToolProfiles.ToDictionary((IToolProfile x) => x.ID);
        IToolsAndBends toolsAndBends = _003C_toolFactory_003EP.CreateToolsAndBends(doc.CombinedBendDescriptors, doc.BendMachine);
        IToolSetups toolSetups = _003C_toolFactory_003EP.CreateToolSetup(toolsAndBends, doc.BendMachine.ToolConfig);
        toolSetups.Desc = "migrated";
        foreach (SBendToolSetup setup in sDoc.Tools.Setups)
        {
            IToolCluster cluster = _003C_toolFactory_003EP.CreateSubCluster(toolSetups);
            double? num = null;
            double? num2 = null;
            IToolSection toolSection = null;
            IToolProfile toolProfile = null;
            IToolSection toolSection2 = null;
            IToolProfile toolProfile2 = null;
            foreach (SBendAdapterSection section in setup.UpperAdapterSet.Sections)
            {
                IToolSection toolSection3 = _003C_toolFactory_003EP.CreateSection(cluster, upper: true);
                toolSection3.OffsetWorld = new Vector3d(section.InitializeInsertPoint.X, 0.0, 0.0);
                foreach (SBendAdapter adapter in section.Adapters)
                {
                    _003C_toolFactory_003EP.CreateToolPiece(dictionary[migrationToolMapping.UpperAdapterPieces[adapter.PartId]], toolSection3).Flipped = adapter.Flipped;
                    double valueOrDefault = num2.GetValueOrDefault();
                    if (!num2.HasValue)
                    {
                        valueOrDefault = 0.0 - dictionary2[migrationToolMapping.UpperAdapterProfiles[adapter.ProfileId]].WorkingHeight;
                        num2 = valueOrDefault;
                    }
                }
            }
            foreach (SBendAdapterSection section2 in setup.LowerAdapterSet.Sections)
            {
                IToolSection toolSection4 = _003C_toolFactory_003EP.CreateSection(cluster, upper: false);
                toolSection4.OffsetWorld = new Vector3d(section2.InitializeInsertPoint.X, 0.0, 0.0);
                foreach (SBendAdapter adapter2 in section2.Adapters)
                {
                    _003C_toolFactory_003EP.CreateToolPiece(dictionary[migrationToolMapping.LowerAdapterPieces[adapter2.PartId]], toolSection4).Flipped = adapter2.Flipped;
                    double valueOrDefault = num.GetValueOrDefault();
                    if (!num.HasValue)
                    {
                        valueOrDefault = dictionary2[migrationToolMapping.LowerAdapterProfiles[adapter2.ProfileId]].WorkingHeight;
                        num = valueOrDefault;
                    }
                }
            }
            foreach (SBendToolSection section3 in setup.UpperToolSet.Sections)
            {
                IToolSection toolSection5 = _003C_toolFactory_003EP.CreateSection(cluster, upper: true);
                toolSection5.OffsetWorld = new Vector3d(section3.InitializeInsertPoint.X, 0.0, num2.GetValueOrDefault());
                foreach (SBendTool tool in section3.Tools)
                {
                    _003C_toolFactory_003EP.CreateToolPiece(dictionary[migrationToolMapping.PunchPieces[tool.PartId]], toolSection5).Flipped = tool.Flipped;
                    if (toolSection == null)
                    {
                        toolSection = toolSection5;
                    }
                    if (toolProfile == null)
                    {
                        toolProfile = dictionary2[migrationToolMapping.PunchProfiles[tool.ProfileId]];
                    }
                }
            }
            foreach (SBendToolSection section4 in setup.LowerToolSet.Sections)
            {
                IToolSection toolSection6 = _003C_toolFactory_003EP.CreateSection(cluster, upper: false);
                toolSection6.OffsetWorld = new Vector3d(section4.InitializeInsertPoint.X, 0.0, num.GetValueOrDefault());
                foreach (SBendTool tool2 in section4.Tools)
                {
                    _003C_toolFactory_003EP.CreateToolPiece(dictionary[migrationToolMapping.DiePieces[tool2.PartId]], toolSection6).Flipped = tool2.Flipped;
                    if (toolSection2 == null)
                    {
                        toolSection2 = toolSection6;
                    }
                    if (toolProfile2 == null)
                    {
                        toolProfile2 = dictionary2[migrationToolMapping.DieProfiles[tool2.ProfileId]];
                    }
                }
            }
            if (toolSection == null || toolSection2 == null)
            {
                continue;
            }
            double num3 = setup.UpperToolSet.Sections.First().InitializeInsertPoint.X - toolSection2.Cluster.OffsetWorld.X;
            foreach (SCombinedBendDescriptor combinedBendDescriptor in sDoc.CombinedBendDescriptors)
            {
                if (combinedBendDescriptor.ToolSetupId == setup.Id)
                {
                    IBendPositioning bendPositioning = toolsAndBends.BendPositions[combinedBendDescriptor.Order];
                    _003C_toolOperator_003EP.AssignBendToSections(bendPositioning, toolSection, toolSection2, num3 + combinedBendDescriptor.ToolStationOffset + combinedBendDescriptor.ToolPresenceVector.RefPointOffsetStart, toolProfile, toolProfile2);
                }
            }
        }
        ImmutableArray<IToolSection>.Enumerator enumerator7 = toolSetups.AllSections.ToImmutableArray().GetEnumerator();
        while (enumerator7.MoveNext())
        {
            IToolSection current11 = enumerator7.Current;
            if (current11.Pieces.Any())
            {
                current11.MultiToolProfile = current11.Pieces.First().PieceProfile.MultiToolProfile;
            }
            else
            {
                _003C_toolFactory_003EP.RemoveSection(current11);
            }
        }
        foreach (SCombinedBendDescriptor combinedBendDescriptor2 in sDoc.CombinedBendDescriptors)
        {
            if (combinedBendDescriptor2.AngleMeasurementPosition.HasValue)
            {
                IBendPositioning bendPositioning2 = toolsAndBends.BendPositions[combinedBendDescriptor2.Order];
                doc.CombinedBendDescriptors[combinedBendDescriptor2.Order].AngleMeasurementPositionRel = combinedBendDescriptor2.AngleMeasurementPosition.Value - bendPositioning2.OffsetWorldX;
            }
        }
        doc.ToolsAndBends = toolsAndBends;
    }

    private SMaterial Convert(IMaterialArt material)
    {
        if (material == null)
        {
            return null;
        }
        return new SMaterial
        {
            Number = material.Number,
            Name = material.Name,
            Density = material.Density,
            Rotation = material.Rotation,
            ReferenceNumber = material.ReferenceNumber,
            AlternativeMaterialNumbers = material.AlternativeMaterialNumbers,
            Material3dGroupId = material.MaterialGroupForBendDeduction,
            MaterialGroupId = material.MaterialGroupId,
            Description = material.Description,
            Remark01 = material.Remark01,
            Remark02 = material.Remark02,
            Remark03 = material.Remark03,
            Remark04 = material.Remark04,
            Remark05 = material.Remark05,
            ThicknessMin = material.ThicknessMin,
            ThicknessMax = material.ThicknessMax,
            YieldStrength = material.YieldStrength,
            TensileStrength = material.TensileStrength,
            HeatCapacity = material.HeatCapacity,
            WorkHardeningExponent = material.WorkHardeningExponent,
            EModul = material.EModul,
            Modified = material.Modified
        };
    }

    private IMaterialArt Convert(SMaterial material)
    {
        if (material == null)
        {
            return null;
        }
        return new MaterialArt
        {
            Number = material.Number,
            Name = material.Name,
            Density = material.Density,
            Rotation = material.Rotation,
            ReferenceNumber = material.ReferenceNumber,
            AlternativeMaterialNumbers = material.AlternativeMaterialNumbers,
            MaterialGroupForBendDeduction = material.Material3dGroupId,
            MaterialGroupId = material.MaterialGroupId,
            Description = material.Description,
            Remark01 = material.Remark01,
            Remark02 = material.Remark02,
            Remark03 = material.Remark03,
            Remark04 = material.Remark04,
            Remark05 = material.Remark05,
            ThicknessMin = material.ThicknessMin,
            ThicknessMax = material.ThicknessMax,
            YieldStrength = material.YieldStrength,
            TensileStrength = material.TensileStrength,
            HeatCapacity = material.HeatCapacity,
            WorkHardeningExponent = material.WorkHardeningExponent,
            EModul = material.EModul,
            Modified = material.Modified
        };
    }

    private IEnumerable<U> Convert<T, U>(IEnumerable<T> list, Func<T, U> convert)
    {
        Func<T, U> convert2 = convert;
        return list?.Select((T x) => convert2(x));
    }

    private void Convert(SPreferredProfileStore sProfileStore, Doc3d doc)
    {
        if (sProfileStore?.PreferredProfilesPerFG == null)
        {
            return;
        }
        IPreferredProfileStore preferredProfileStore = doc.PreferredProfileStore;
        foreach (Pair<Pair<int, int>, Pair<SPreferredProfile, ToolSelectionType>> item in sProfileStore.PreferredProfilesPerFG)
        {
            preferredProfileStore.AssignPreferredProfileToCommonBend(Convert(item.Item2.Item1), item.Item1.Item1, item.Item1.Item2, item.Item2.Item2);
        }
    }

    private SPreferredProfileStore Convert(IPreferredProfileStore profileStore)
    {
        SPreferredProfileStore sPreferredProfileStore = new SPreferredProfileStore
        {
            PreferredProfilesPerFG = new List<Pair<Pair<int, int>, Pair<SPreferredProfile, ToolSelectionType>>>()
        };
        foreach (KeyValuePair<(int, int), Pair<IPreferredProfile, ToolSelectionType>> allEntry in profileStore.GetAllEntries())
        {
            sPreferredProfileStore.PreferredProfilesPerFG.Add(new Pair<Pair<int, int>, Pair<SPreferredProfile, ToolSelectionType>>(new Pair<int, int>(allEntry.Key.Item1, allEntry.Key.Item2), new Pair<SPreferredProfile, ToolSelectionType>(Convert(allEntry.Value.Item1), allEntry.Value.Item2)));
        }
        return sPreferredProfileStore;
    }

    private SPreferredProfile Convert(IPreferredProfile preferredProfile)
    {
        throw new NotImplementedException();
    }

    private IPreferredProfile Convert(SPreferredProfile preferredProfile)
    {
        throw new NotImplementedException();
    }

    private SPnBndFile Convert(PnBndFile pnBndFile)
    {
        if (pnBndFile == null)
        {
            return null;
        }
        return new SPnBndFile
        {
            Header = Convert(pnBndFile.Header)
        };
    }

    private PnBndFile Convert(SPnBndFile pnBndFile)
    {
        if (pnBndFile == null)
        {
            return null;
        }
        return new PnBndFile
        {
            Header = Convert(pnBndFile.Header)
        };
    }

    private SPnBndFileHeader Convert(PnBndFileHeader header)
    {
        return new SPnBndFileHeader
        {
            Author = header.Author,
            CreateDate = header.CreateDate,
            FileVersion = header.FileVersion,
            PkernelVersion = header.PkernelVersion,
            IsFromDisassembly = header.IsFromDisassembly,
            ModelName = header.ModelName,
            ModelSourceFileName = header.ModelSourceFileName,
            PnUserPath = header.PnUserPath,
            SaveDate = header.SaveDate,
            ImportedFilename = header.ImportedFilename
        };
    }

    private PnBndFileHeader Convert(SPnBndFileHeader header)
    {
        return new PnBndFileHeader
        {
            Author = header.Author,
            CreateDate = header.CreateDate,
            FileVersion = header.FileVersion,
            PkernelVersion = header.PkernelVersion,
            IsFromDisassembly = header.IsFromDisassembly,
            ModelName = header.ModelName,
            ModelSourceFileName = header.ModelSourceFileName,
            PnUserPath = header.PnUserPath,
            SaveDate = header.SaveDate,
            ImportedFilename = header.ImportedFilename
        };
    }

    private void ConvertCombinedBends(Doc3d docSrc, SPnBndDoc docDest)
    {
        foreach (BendDescriptor bendDescriptor in docSrc._data.BendDescriptors)
        {
            docDest.BendDescriptors.Add(Convert(bendDescriptor));
        }
        foreach (CombinedBendDescriptor combinedBendDescriptor in docSrc._data.CombinedBendDescriptors)
        {
            docDest.CombinedBendDescriptors.Add(Convert(combinedBendDescriptor, docSrc));
        }
    }

    private void ConvertCombinedBends(SPnBndDoc docSrc, Doc3d docDest)
    {
        docDest._data.BendDescriptors.Clear();
        foreach (SBendDescriptor bendDescriptor in docSrc.BendDescriptors)
        {
            docDest._data.BendDescriptors.Add(Convert(bendDescriptor, docDest));
        }
        docDest.CreateUnfoldModel();
        docDest._data.CombinedBendDescriptors.Clear();
        foreach (SCombinedBendDescriptor combinedBendDescriptor in docSrc.CombinedBendDescriptors)
        {
            docDest._data.CombinedBendDescriptors.Add(Convert(combinedBendDescriptor, docDest));
        }
    }

    private SCombinedBendDescriptor Convert(ICombinedBendDescriptorInternal cbd, Doc3d doc)
    {
        Doc3d doc2 = doc;
        return new SCombinedBendDescriptor
        {
            BendDescriptors = cbd.Enumerable.Select((IBendDescriptor x) => doc2.BendDescriptors.IndexOf<IBendDescriptor>(x)).ToList(),
            BendType = cbd.BendType,
            IsIncluded = cbd.IsIncluded,
            Order = cbd.Order,
            ProgressStart = cbd.ProgressStart,
            ProgressStop = cbd.ProgressStop,
            SelectedStopPointLeft = Convert(cbd.SelectedStopPointLeft),
            SelectedStopPointRight = Convert(cbd.SelectedStopPointRight),
            SplitPredecessors = cbd.SplitPredecessors?.Select((ICombinedBendDescriptor x) => x.Order)?.ToList(),
            StopPointsLeft = Convert(cbd.StopPointsLeft, Convert)?.ToList(),
            StopPointsRight = Convert(cbd.StopPointsRight, Convert)?.ToList(),
            FingerPositioningMode = cbd.FingerPositioningMode,
            FingerStability = cbd.FingerStability,
            ToolSelectionAlgorithm = cbd.ToolSelectionAlgorithm,
            UserForcedUpperToolProfile = cbd.UserForcedUpperToolProfile,
            UserForcedLowerToolProfile = cbd.UserForcedLowerToolProfile,
            XLeftRetractAuto = cbd.XLeftRetractAuto,
            XLeftRetractUser = cbd.XLeftRetractUser,
            XRightRetractAuto = cbd.XRightRetractAuto,
            XRightRetractUser = cbd.XRightRetractUser,
            RLeftRetractAuto = cbd.RLeftRetractAuto,
            RLeftRetractUser = cbd.RLeftRetractUser,
            RRightRetractAuto = cbd.RRightRetractAuto,
            RRightRetractUser = cbd.RRightRetractUser,
            ZLeftRetractAuto = cbd.ZLeftRetractAuto,
            ZLeftRetractUser = cbd.ZLeftRetractUser,
            ZRightRetractAuto = cbd.ZRightRetractAuto,
            ZRightRetractUser = cbd.ZRightRetractUser,
            LeftFingerSnap = cbd.LeftFingerSnap,
            RightFingerSnap = cbd.RightFingerSnap,
            ReleasePointUser = cbd.ReleasePointUser,
            AngleMeasurementPositionRel = cbd.AngleMeasurementPositionRel,
            UseAngleMeasurement = cbd.UseAngleMeasurement,
            LeftFrontLiftingAidHorizontalCoordinar = cbd.LeftFrontLiftingAidHorizontalCoordinar,
            LeftFrontLiftingAidRotationCoordinar = cbd.LeftFrontLiftingAidRotationCoordinar,
            LeftFrontLiftingAidVerticalCoordinar = cbd.LeftFrontLiftingAidVerticalCoordinar,
            RightFrontLiftingAidHorizontalCoordinar = cbd.RightFrontLiftingAidHorizontalCoordinar,
            RightFrontLiftingAidRotationCoordinar = cbd.RightFrontLiftingAidRotationCoordinar,
            RightFrontLiftingAidVerticalCoordinar = cbd.RightFrontLiftingAidVerticalCoordinar,
            LeftBackLiftingAidHorizontalCoordinar = cbd.LeftBackLiftingAidHorizontalCoordinar,
            LeftBackLiftingAidRotationCoordinar = cbd.LeftBackLiftingAidRotationCoordinar,
            LeftBackLiftingAidVerticalCoordinar = cbd.LeftBackLiftingAidVerticalCoordinar,
            RightBackLiftingAidHorizontalCoordinar = cbd.RightBackLiftingAidHorizontalCoordinar,
            RightBackLiftingAidRotationCoordinar = cbd.RightBackLiftingAidRotationCoordinar,
            RightBackLiftingAidVerticalCoordinar = cbd.RightBackLiftingAidVerticalCoordinar,
            PositioningInfo = ConvertCombinedBendPositioning(cbd.PositioningInfo),
            MachinePartInsertionDirection = cbd.MachinePartInsertionDirection,
            Comment = cbd.Comment,
            UseLeftFrontLiftingAid = cbd.UseLeftFrontLiftingAid,
            UseRightFrontLiftingAid = cbd.UseRightFrontLiftingAid,
            UseLeftBackLiftingAid = cbd.UseLeftBackLiftingAid,
            UseRightBackLiftingAid = cbd.UseRightBackLiftingAid,
            UserStepChangeMode = cbd.UserStepChangeMode
        };
    }

    private CombinedBendDescriptor Convert(SCombinedBendDescriptor cbd, Doc3d doc)
    {
        Doc3d doc2 = doc;
        CombinedBendDescriptor combinedBendDescriptor = new CombinedBendDescriptor(cbd.BendDescriptors.Select((int x) => doc2._data.BendDescriptors.ElementAt(x)).ToList(), doc2, cbd.BendType)
        {
            IsIncluded = cbd.IsIncluded,
            LowerToolSetId = cbd.LowerToolSetId,
            Order = cbd.Order,
            ProgressStart = cbd.ProgressStart,
            ProgressStop = cbd.ProgressStop,
            SelectedStopPointLeft = Convert(cbd.SelectedStopPointLeft),
            SelectedStopPointRight = Convert(cbd.SelectedStopPointRight),
            SplitPredecessors = cbd.SplitPredecessors?.Select((int x) => doc2._data.CombinedBendDescriptors.ElementAt(x)).ToList(),
            StopPointsLeft = Convert(cbd.StopPointsLeft, Convert)?.ToList(),
            StopPointsRight = Convert(cbd.StopPointsRight, Convert)?.ToList(),
            FingerPositioningMode = cbd.FingerPositioningMode,
            FingerStability = cbd.FingerStability,
            ToolSelectionAlgorithm = cbd.ToolSelectionAlgorithm,
            ToolSetupId = cbd.ToolSetupId,
            ToolStationOffset = cbd.ToolStationOffset,
            UpperToolSetId = cbd.UpperToolSetId,
            UpperAdapterSetId = cbd.UpperAdapterSetId,
            LowerAdapterSetId = cbd.LowerAdapterSetId,
            UserForcedUpperToolProfile = (cbd.UserForcedUpperToolProfile ?? cbd.UserForcedToolProfiles?.punchProfileId),
            UserForcedLowerToolProfile = (cbd.UserForcedLowerToolProfile ?? cbd.UserForcedToolProfiles?.dieProfileId),
            XLeftRetractAuto = cbd.XLeftRetractAuto,
            XLeftRetractUser = cbd.XLeftRetractUser,
            XRightRetractAuto = cbd.XRightRetractAuto,
            XRightRetractUser = cbd.XRightRetractUser,
            RLeftRetractAuto = cbd.RLeftRetractAuto,
            RLeftRetractUser = cbd.RLeftRetractUser,
            RRightRetractAuto = cbd.RRightRetractAuto,
            RRightRetractUser = cbd.RRightRetractUser,
            ZLeftRetractAuto = cbd.ZLeftRetractAuto,
            ZLeftRetractUser = cbd.ZLeftRetractUser,
            ZRightRetractAuto = cbd.ZRightRetractAuto,
            ZRightRetractUser = cbd.ZRightRetractUser,
            LeftFingerSnap = cbd.LeftFingerSnap,
            RightFingerSnap = cbd.RightFingerSnap,
            ReleasePointUser = cbd.ReleasePointUser,
            AngleMeasurementPositionRel = cbd.AngleMeasurementPositionRel,
            UseAngleMeasurement = cbd.UseAngleMeasurement,
            LeftFrontLiftingAidHorizontalCoordinar = cbd.LeftFrontLiftingAidHorizontalCoordinar,
            LeftFrontLiftingAidRotationCoordinar = cbd.LeftFrontLiftingAidRotationCoordinar,
            LeftFrontLiftingAidVerticalCoordinar = cbd.LeftFrontLiftingAidVerticalCoordinar,
            RightFrontLiftingAidHorizontalCoordinar = cbd.RightFrontLiftingAidHorizontalCoordinar,
            RightFrontLiftingAidRotationCoordinar = cbd.RightFrontLiftingAidRotationCoordinar,
            RightFrontLiftingAidVerticalCoordinar = cbd.RightFrontLiftingAidVerticalCoordinar,
            LeftBackLiftingAidHorizontalCoordinar = cbd.LeftBackLiftingAidHorizontalCoordinar,
            LeftBackLiftingAidRotationCoordinar = cbd.LeftBackLiftingAidRotationCoordinar,
            LeftBackLiftingAidVerticalCoordinar = cbd.LeftBackLiftingAidVerticalCoordinar,
            RightBackLiftingAidHorizontalCoordinar = cbd.RightBackLiftingAidHorizontalCoordinar,
            RightBackLiftingAidRotationCoordinar = cbd.RightBackLiftingAidRotationCoordinar,
            RightBackLiftingAidVerticalCoordinar = cbd.RightBackLiftingAidVerticalCoordinar,
            MachinePartInsertionDirection = cbd.MachinePartInsertionDirection,
            Comment = cbd.Comment,
            UseLeftFrontLiftingAid = cbd.UseLeftFrontLiftingAid,
            UseRightFrontLiftingAid = cbd.UseRightFrontLiftingAid,
            UseLeftBackLiftingAid = cbd.UseLeftBackLiftingAid,
            UseRightBackLiftingAid = cbd.UseRightBackLiftingAid,
            UserStepChangeMode = cbd.UserStepChangeMode
        };
        ConvertCombinedBendPositioning(cbd.PositioningInfo, combinedBendDescriptor.PositioningInfo);
        return combinedBendDescriptor;
    }

    private SBendPositioningInfo ConvertCombinedBendPositioning(IBendPositioningInfo posInfo)
    {
        return new SBendPositioningInfo
        {
            BendConcaveAxis = posInfo.BendConvexAxis,
            IsParentLeftOfBendPlane = posInfo.IsParentLeftOfBendPlane,
            IsParentOnSideOfCentroid = posInfo.IsParentOnSideOfCentroid,
            IsReversedGeometry = posInfo.IsReversedGeometry,
            MachineInsertionDirection = posInfo.MachineInsertionDirection,
            PrimaryFaceGroupId = posInfo.PrimaryFaceGroupId,
            StartEndOffsetsBends = null,
            StartEndOffsetsBendsNew = posInfo.StartEndOffsetsBends.Select(((double start, double end, int fgId, double fgOffset) x) => new SStartEndOffsetBend(x.start, x.end, x.fgId, x.fgOffset)).ToList(),
            StartEndOffsetsObstacles = posInfo.StartEndOffsetsObstacles
        };
    }

    private void ConvertCombinedBendPositioning(SBendPositioningInfo source, IBendPositioningInfo destination)
    {
        destination.BendConvexAxis = source.BendConcaveAxis;
        destination.IsParentLeftOfBendPlane = source.IsParentLeftOfBendPlane;
        destination.IsParentOnSideOfCentroid = source.IsParentOnSideOfCentroid;
        destination.MachineInsertionDirection = source.MachineInsertionDirection;
        destination.PrimaryFaceGroupId = source.PrimaryFaceGroupId;
        destination.StartEndOffsetsBends = source.StartEndOffsetsBendsNew?.Select((SStartEndOffsetBend x) => (Start: x.Start, End: x.End, FgId: x.FgId, FgOffset: x.FgOffset)).ToList() ?? source.StartEndOffsetsBends.Select((Triple<double, double, int> x) => (x.Item1, x.Item2, x.Item3, 0.0)).ToList();
        destination.StartEndOffsetsObstacles = source.StartEndOffsetsObstacles;
        destination.GetType().GetProperty("IsReversedGeometry").SetValue(destination, source.IsReversedGeometry);
    }

    private SBendDescriptor Convert(BendDescriptor bd)
    {
        return new SBendDescriptor
        {
            ID = bd.ID,
            Type = bd.Type,
            BendParams = Convert(bd.BendParamsInternal)
        };
    }

    private BendDescriptor Convert(SBendDescriptor bd, Doc3d doc)
    {
        return new BendDescriptor(bd.Type, Convert(bd.BendParams, doc))
        {
            ID = bd.ID
        };
    }

    private BendParametersBase Convert(SBendParametersBase p, Doc3d doc)
    {
        if (p is SSimpleBendParameters sSimpleBendParameters)
        {
            return new SimpleBendParameters(doc.ReconstructedEntryModel.GetFaceGroupById(p.EntryFaceGroupId), doc)
            {
                AngleSign = sSimpleBendParameters.AngleSign,
                ManualBendDeduction = sSimpleBendParameters.ManualBendDeduction,
                ManualRadius = sSimpleBendParameters.ManualRadius,
                ToolRadius = sSimpleBendParameters.ToolRadius
            };
        }
        if (p is SStepBendParameters sStepBendParameters)
        {
            return new StepBendParameters(doc.ReconstructedEntryModel.GetFaceGroupById(p.EntryFaceGroupId), sStepBendParameters.StepIndex, sStepBendParameters.OriginalRadius, doc)
            {
                AngleSign = sStepBendParameters.AngleSign,
                ManualBendDeduction = sStepBendParameters.ManualBendDeduction,
                ManualRadius = sStepBendParameters.ManualRadius,
                ToolRadius = sStepBendParameters.ToolRadius
            };
        }
        return null;
    }

    private SBendParametersBase Convert(BendParametersBase p)
    {
        if (p is SimpleBendParameters simpleBendParameters)
        {
            return new SSimpleBendParameters
            {
                EntryFaceGroupId = simpleBendParameters.EntryFaceGroup.ID,
                AngleSign = simpleBendParameters.AngleSign,
                ManualBendDeduction = simpleBendParameters.ManualBendDeduction,
                ManualRadius = simpleBendParameters.ManualRadius,
                ToolRadius = simpleBendParameters.ToolRadius
            };
        }
        if (p is StepBendParameters stepBendParameters)
        {
            return new SStepBendParameters
            {
                EntryFaceGroupId = stepBendParameters.EntryFaceGroup.ID,
                StepIndex = stepBendParameters.StepIndex,
                OriginalRadius = stepBendParameters.OriginalRadius,
                AngleSign = stepBendParameters.AngleSign,
                ManualBendDeduction = stepBendParameters.ManualBendDeduction,
                ManualRadius = stepBendParameters.ManualRadius,
                ToolRadius = stepBendParameters.ToolRadius
            };
        }
        return null;
    }

    private SFingerStopPoint Convert(IFingerStopPointInternal fingerStopPoint)
    {
        if (fingerStopPoint == null)
        {
            return null;
        }
        return new SFingerStopPoint
        {
            BackEdgeCenterPoint = fingerStopPoint.BackEdgeCenterPoint,
            Finger = fingerStopPoint.Finger,
            RelativeEdgePosition = fingerStopPoint.RelativeEdgePosition,
            StopPoint = fingerStopPoint.StopPoint,
            StopPointRelativeToPart = fingerStopPoint.StopPointRelativeToPart,
            StopCombination = Convert(fingerStopPoint.StopCombination),
            Rating = fingerStopPoint.Rating,
            FingerPosition = _fingerPositionConverter.Convert(fingerStopPoint.FingerPosition)
        };
    }

    private IFingerStopPointInternal Convert(SFingerStopPoint fingerStopPoint)
    {
        if (fingerStopPoint == null)
        {
            return null;
        }
        return new FingerStopPoint(fingerStopPoint.StopPoint, fingerStopPoint.StopPointRelativeToPart, fingerStopPoint.Finger, Convert(fingerStopPoint.StopCombination), _fingerPositionConverter.Convert(fingerStopPoint.FingerPosition), fingerStopPoint.Rating)
        {
            BackEdgeCenterPoint = fingerStopPoint.BackEdgeCenterPoint,
            RelativeEdgePosition = fingerStopPoint.RelativeEdgePosition
        };
    }

    private SFingerStopCombination Convert(IFingerStopCombination combination)
    {
        if (combination != null)
        {
            return new SFingerStopCombination
            {
                CombinationType = (ulong)combination.Type
            };
        }
        return null;
    }

    private IFingerStopCombination Convert(SFingerStopCombination combination)
    {
        if (combination != null && combination.CombinationType.HasValue)
        {
            return new WiCAM.Pn4000.PN3D.FingerStop.FingerStopCombination
            {
                Type = (StopCombinationType)combination.CombinationType.Value
            };
        }
        if (combination != null && combination.BoundingBoxPosition)
        {
            return new WiCAM.Pn4000.PN3D.FingerStop.FingerStopCombination
            {
                Type = StopCombinationType.BoundingBoxPosition
            };
        }
        if (combination != null && combination.NoValidPositionFound)
        {
            return new WiCAM.Pn4000.PN3D.FingerStop.FingerStopCombination
            {
                Type = StopCombinationType.NoValidPosition
            };
        }
        if (combination != null)
        {
            return new WiCAM.Pn4000.PN3D.FingerStop.FingerStopCombination
            {
                Type = StopCombinationTypeExtensions.FromFaceNames(combination.FaceNames)
            };
        }
        return null;
    }
}
