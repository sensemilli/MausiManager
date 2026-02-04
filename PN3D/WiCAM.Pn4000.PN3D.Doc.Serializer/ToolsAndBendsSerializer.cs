using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer;

internal class ToolsAndBendsSerializer : IToolsAndBendsSerializer
{
	private class DeserializationArgs
	{
		private readonly Dictionary<int, IToolCluster> _refClusters = new Dictionary<int, IToolCluster>();

		private readonly IBendMachineTools _bendMachine;

		private readonly IToolsAndBendsSerializerToolMapping _toolsAndBendsSerializerToolMapping;

		public DeserializationArgs(IBendMachineTools bendMachine, IToolsAndBendsSerializerToolMapping toolsAndBendsSerializerToolMapping)
		{
			this._bendMachine = bendMachine;
			this._toolsAndBendsSerializerToolMapping = toolsAndBendsSerializerToolMapping;
		}

		public void Add(IToolCluster cluster, int id)
		{
			this._refClusters.Add(id, cluster);
		}

		public IToolCluster? GetById(int? id)
		{
			if (id.HasValue)
			{
				return this._refClusters.GetValueOrDefault(id.Value, null);
			}
			return null;
		}

		public IMultiToolProfile? GetMultiToolProfile(int multiToolProfileId)
		{
			return this._toolsAndBendsSerializerToolMapping.TryGetMultiToolProfile(multiToolProfileId);
		}

		public IPunchProfile? GetPunchProfile(int? profileId)
		{
			return this._toolsAndBendsSerializerToolMapping.TryGetToolProfile(profileId) as IPunchProfile;
		}

		public IDieProfile? GetDieProfile(int? profileId)
		{
			return this._toolsAndBendsSerializerToolMapping.TryGetToolProfile(profileId) as IDieProfile;
		}

		public IToolPieceProfile? GetToolPiece(int pieceProfileId, IMultiToolProfile toolProfile)
		{
			return toolProfile.PieceProfiles.FirstOrDefault((IToolPieceProfile x) => x.ToolId == pieceProfileId);
		}

		public ISensorDisk? GetSensorDisksMapping(int? sensorDisksMappingId)
		{
			return this._bendMachine.SensorDisks.FirstOrDefault((ISensorDisk x) => x.Id == sensorDisksMappingId);
		}
	}

	private class SerializationArgs
	{
		private int _nextId = 1;

		private readonly Dictionary<IToolCluster, int> _refClusters = new Dictionary<IToolCluster, int>();

		public int Add(IToolCluster cluster)
		{
			int num = this._nextId++;
			this._refClusters.Add(cluster, num);
			return num;
		}

		public int? GetId(IToolCluster? cluster)
		{
			if (cluster == null)
			{
				return null;
			}
			return this._refClusters.GetValueOrDefault(cluster, -1);
		}
	}

	private readonly IToolFactory _toolFactory;

	private readonly IToolsAndBendsSerializerToolMapping _toolsAndBendsSerializerToolMapping;

	public ToolsAndBendsSerializer(IToolFactory toolFactory, IToolsAndBendsSerializerToolMapping toolsAndBendsSerializerToolMapping)
	{
		this._toolFactory = toolFactory;
		this._toolsAndBendsSerializerToolMapping = toolsAndBendsSerializerToolMapping;
	}

	public string Convert(ISToolSetups setups)
	{
		return JsonSerializer.Serialize(setups as SToolSetups);
	}

	public ISToolSetups ConvertBack(string setups)
	{
		return JsonSerializer.Deserialize<SToolSetups>(setups);
	}

	public ISToolSetups Convert(IToolSetups setups)
	{
		return ToolsAndBendsSerializer.Map(setups, new SerializationArgs());
	}

	public IToolSetups ConvertBack(ISToolSetups setups, IBendMachineTools bendMachine)
	{
		if (!(setups is SToolSetups model))
		{
			throw new ArgumentException("");
		}
		return this.Map(model, new DeserializationArgs(bendMachine, this._toolsAndBendsSerializerToolMapping));
	}

	public ISToolsAndBends Convert(IToolsAndBends toolsAndBends)
	{
		SerializationArgs args = new SerializationArgs();
		SToolsAndBends sToolsAndBends = new SToolsAndBends
		{
			ToolSetups = new List<SToolSetups>(),
			BendPositions = new List<SBendPositioning>()
		};
		foreach (IToolSetups toolSetup in toolsAndBends.ToolSetups)
		{
			sToolsAndBends.ToolSetups.Add(ToolsAndBendsSerializer.Map(toolSetup, args));
		}
		foreach (IBendPositioning bendPosition in toolsAndBends.BendPositions)
		{
			sToolsAndBends.BendPositions.Add(ToolsAndBendsSerializer.Map(bendPosition, args));
		}
		return sToolsAndBends;
	}

	public IToolsAndBends ConvertBack(ISToolsAndBends toolsAndBends, IBendMachineTools bendMachine)
	{
		if (!(toolsAndBends is SToolsAndBends sToolsAndBends))
		{
			throw new ArgumentException("");
		}
		DeserializationArgs args = new DeserializationArgs(bendMachine, this._toolsAndBendsSerializerToolMapping);
		IToolsAndBends toolsAndBends2 = this._toolFactory.CreateToolsAndBends();
		foreach (SToolSetups toolSetup in sToolsAndBends.ToolSetups)
		{
			toolsAndBends2.ToolSetups.Add(this.Map(toolSetup, args));
		}
		foreach (SBendPositioning bendPosition in sToolsAndBends.BendPositions)
		{
			toolsAndBends2.BendPositionsList.Add(this.Map(bendPosition, args));
		}
		return toolsAndBends2;
	}

	private static SBendPositioning Map(IBendPositioning model, SerializationArgs args)
	{
		List<IAcbPunchPiece> acbs = model.Anchor?.Root.Acbs.OrderBy((IAcbPunchPiece t) => t.OffsetWorld.X).ToList();
		return new SBendPositioning
		{
			AnchorSerializeId = args.GetId(model.Anchor),
			Offset = ToolsAndBendsSerializer.Map(model.Offset),
			Bend = ToolsAndBendsSerializer.Map(model.Bend),
			IsReversedGeometry = model.IsReversedGeometry,
			MachineInsertDirection = (int)model.MachineInsertDirection,
			PunchProfileId = model.PunchProfile?.ID,
			DieProfileId = model.DieProfile?.ID,
			PunchProfileByUserId = model.PunchProfileByUser?.ID,
			DieProfileByUserId = model.DieProfileByUser?.ID,
			UpperWorkingHeightAdapters = model.UpperWorkingHeightAdapters,
			AcbSatus = ((acbs == null) ? null : model.AcbStatus?.Select((KeyValuePair<IAcbPunchPiece, AcbActivationResult> x) => (acbs.IndexOf(x.Key), Value: x.Value)).ToList())
		};
	}

	private static SBend Map(IBend model)
	{
		return new SBend
		{
			Order = model.Order,
			StartAngle = model.StartAngle,
			DestAngle = model.DestAngle,
			BendType = model.BendType,
			FaceGroupIds = model.FaceGroupIds.ToList(),
			FaceGroupIdentifiers = model.FaceGroupIdentifiers.ToList(),
			BendingZones = model.BendingZones.ToList(),
			StartEndOffsetsBends = model.StartEndOffsetsBends.Select(delegate((double start, double end, int fgId, double fgOffset) x)
			{
				SStartEndOffsetsBends sStartEndOffsetsBends = new SStartEndOffsetsBends();
				(sStartEndOffsetsBends.Start, sStartEndOffsetsBends.End, sStartEndOffsetsBends.FgId, sStartEndOffsetsBends.FgOffset) = x;
				return sStartEndOffsetsBends;
			}).ToList(),
			BendState = model.BendStates.Select((IBendState x) => new SBendState(x)).ToList(),
			CollisionIntervals = model.CollisionIntervals?.ToList()
		};
	}

	private IBendPositioning Map(SBendPositioning model, DeserializationArgs args)
	{
		IBend bend = this.Map(model.Bend);
		int insertionDirection = (typeof(MachinePartInsertionDirection).IsEnumDefined(model.MachineInsertDirection) ? model.MachineInsertDirection : 0);
		IBendPositioning bendPositioning = this._toolFactory.DeserializeBendPositioning(null, bend, (MachinePartInsertionDirection)insertionDirection, model.IsReversedGeometry);
		bendPositioning.Anchor = args.GetById(model.AnchorSerializeId);
		bendPositioning.Offset = ToolsAndBendsSerializer.Map(model.Offset);
		bendPositioning.PunchProfile = args.GetPunchProfile(model.PunchProfileId);
		bendPositioning.DieProfile = args.GetDieProfile(model.DieProfileId);
		bendPositioning.PunchProfileByUser = args.GetPunchProfile(model.PunchProfileByUserId);
		bendPositioning.DieProfileByUser = args.GetDieProfile(model.DieProfileByUserId);
		bendPositioning.UpperWorkingHeightAdapters = model.UpperWorkingHeightAdapters;
		List<IAcbPunchPiece> list = bendPositioning.Anchor?.Root.Acbs.OrderBy((IAcbPunchPiece t) => t.OffsetWorld.X).ToList();
		int i;
		for (i = 0; i < list?.Count; i++)
		{
			int num = model.AcbSatus.IndexOf(((int idx, AcbActivationResult status) x) => x.idx == i);
			if (num > -1)
			{
				bendPositioning.AcbStatus.Add(list[i], model.AcbSatus[num].status);
			}
		}
		return bendPositioning;
	}

	private IBend Map(SBend model)
	{
		IBend bend = this._toolFactory.DeserializeBend(model.FaceGroupIdentifiers, model.BendingZones, model.BendState.Select((SBendState x) => (FaceGroupId: x.FaceGroupId, Angle: x.Angle, KFactor: x.KFactor, FinalRadius: x.FinalRadius)), model.CollisionIntervals, model.StartEndOffsetsBends?.Select((SStartEndOffsetsBends x) => (Start: x.Start, End: x.End, FgId: x.FgId, FgOffset: x.FgOffset)).ToList());
		bend.Order = model.Order;
		bend.StartAngle = model.StartAngle;
		bend.DestAngle = model.DestAngle;
		bend.BendType = model.BendType;
		return bend;
	}

	private static SToolSetups Map(IToolSetups model, SerializationArgs args)
	{
		int serializeId = args.Add(model);
		SToolSetups sToolSetups = new SToolSetups
		{
			OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal),
			Children = new List<SToolCluster>(),
			Sections = model.Sections.Select(Map).ToList(),
			Number = model.Number,
			SerializeId = serializeId,
			Desc = model.Desc,
			MountTypeIdUpperBeam = model.MountTypeIdUpperBeam,
			MountTypeIdLowerBeam = model.MountTypeIdLowerBeam,
			MachineNumber = model.MachineNumber,
			LowerBeamXStart = model.LowerBeamXStart,
			LowerBeamXEnd = model.LowerBeamXEnd,
			UpperBeamXStart = model.UpperBeamXStart,
			UpperBeamXEnd = model.UpperBeamXEnd
		};
		foreach (IToolCluster child in model.Children)
		{
			sToolSetups.Children.Add(ToolsAndBendsSerializer.Map(child, args));
		}
		return sToolSetups;
	}

	private static SToolCluster Map(IToolCluster model, SerializationArgs args)
	{
		int serializeId = args.Add(model);
		SToolCluster sToolCluster = new SToolCluster
		{
			OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal),
			Children = new List<SToolCluster>(),
			Sections = model.Sections.Select(Map).ToList(),
			Number = model.Number,
			SerializeId = serializeId
		};
		foreach (IToolCluster child in model.Children)
		{
			sToolCluster.Children.Add(ToolsAndBendsSerializer.Map(child, args));
		}
		return sToolCluster;
	}

	private static SToolSection Map(IToolSection model)
	{
		return new SToolSection
		{
			Pieces = (from x in model.Pieces.Select((IToolPiece x, int i) => (piece: x, i: i))
				where !(x.piece is IAcbPunchPiece)
				select x).Select(Map).ToList(),
			AcbPieces = (from x in model.Pieces.Select((IToolPiece x, int i) => (piece: x as IAcbPunchPiece, i: i))
				where x.piece != null
				select x).Select(Map).ToList(),
			MultiToolProfileId = model.MultiToolProfile.ID,
			IsUpperSection = model.IsUpperSection,
			OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal),
			Length = model.Length,
			Flipped = model.Flipped,
			ReservedSpaceLeft = model.ReservedSpaceLeft,
			ReservedSpaceRight = model.ReservedSpaceRight
		};
	}

	private static SToolPiece Map((IToolPiece model, int i) pair)
	{
		return new SToolPiece
		{
			Order = pair.i,
			PieceProfileId = pair.model.PieceProfile.ToolId,
			Flipped = pair.model.Flipped,
			OffsetLocal = ToolsAndBendsSerializer.Map(pair.model.OffsetLocal)
		};
	}

	private static SAcbPunchPiece Map((IAcbPunchPiece model, int i) pair)
	{
		return new SAcbPunchPiece
		{
			Order = pair.i,
			PieceProfileId = pair.model.PieceProfile.ToolId,
			Flipped = pair.model.Flipped,
			OffsetLocal = ToolsAndBendsSerializer.Map(pair.model.OffsetLocal),
			DiskMappingId = pair.model.SensorDisks?.Id
		};
	}

	private IToolSetups Map(SToolSetups model, DeserializationArgs args)
	{
		IToolSetups toolSetups = this._toolFactory.DeserializeToolSetup(null, model.UpperBeamXStart, model.UpperBeamXEnd, model.LowerBeamXStart, model.LowerBeamXEnd, model.MountTypeIdUpperBeam, model.MountTypeIdLowerBeam, model.Number);
		args.Add(toolSetups, model.SerializeId);
		toolSetups.Desc = model.Desc;
		toolSetups.Number = model.Number;
		toolSetups.MachineNumber = model.MachineNumber;
		toolSetups.OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal);
		foreach (SToolCluster child in model.Children)
		{
			this.Map(toolSetups, child, args);
		}
		foreach (SToolSection section in model.Sections)
		{
			this.Map(toolSetups, section, args);
		}
		return toolSetups;
	}

	private IToolCluster Map(IToolCluster parent, SToolCluster model, DeserializationArgs args)
	{
		IToolCluster toolCluster = this._toolFactory.CreateSubCluster(parent);
		args.Add(toolCluster, model.SerializeId);
		toolCluster.Number = model.Number;
		toolCluster.OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal);
		foreach (SToolCluster child in model.Children)
		{
			this.Map(toolCluster, child, args);
		}
		foreach (SToolSection section in model.Sections)
		{
			this.Map(toolCluster, section, args);
		}
		return toolCluster;
	}

	private IToolSection Map(IToolCluster parent, SToolSection model, DeserializationArgs args)
	{
		bool isUpperSection = model.IsUpperSection;
		IToolSection toolSection = this._toolFactory.CreateSection(parent, isUpperSection);
		toolSection.Flipped = model.Flipped;
		toolSection.ReservedSpaceLeft = model.ReservedSpaceLeft;
		toolSection.ReservedSpaceRight = model.ReservedSpaceRight;
		toolSection.OffsetLocal = ToolsAndBendsSerializer.Map(model.OffsetLocal);
		toolSection.MultiToolProfile = args.GetMultiToolProfile(model.MultiToolProfileId) ?? throw new DeletedToolsException($"MultiToolProfile {model.MultiToolProfileId} missing in bendMachine config.");
		foreach (SToolPiece item in from x in model.Pieces.Concat(model.AcbPieces).ToList()
			orderby x.Order
			select x)
		{
			if (item is SAcbPunchPiece model2)
			{
				this.Map(toolSection, model2, args);
			}
			else
			{
				this.Map(toolSection, item, args);
			}
		}
		return toolSection;
	}

	private IToolPiece Map(IToolSection parent, SToolPiece model, DeserializationArgs args)
	{
		IToolPieceProfile? pieceProfile = args.GetToolPiece(model.PieceProfileId, parent.MultiToolProfile) ?? throw new DeletedToolsException($"ToolPieceProfile {model.PieceProfileId} for MultiToolProfile {parent.MultiToolProfile.ID} missing in bendMachine config.");
		IToolPiece toolPiece = this._toolFactory.CreateToolPiece(pieceProfile, parent);
		toolPiece.Flipped = model.Flipped;
		return toolPiece;
	}

	private IAcbPunchPiece Map(IToolSection parent, SAcbPunchPiece model, DeserializationArgs args)
	{
		IToolPieceProfile? pieceProfile = args.GetToolPiece(model.PieceProfileId, parent.MultiToolProfile) ?? throw new DeletedToolsException($"ToolPieceProfile {model.PieceProfileId} for MultiToolProfile {parent.MultiToolProfile.ID} missing in bendMachine config.");
		IAcbPunchPiece obj = (IAcbPunchPiece)this._toolFactory.CreateToolPiece(pieceProfile, parent);
		obj.Flipped = model.Flipped;
		obj.SensorDisks = args.GetSensorDisksMapping(model.DiskMappingId);
		return obj;
	}

	private static Vector3d Map(SVector3d model)
	{
		return new Vector3d(model.X, model.Y, model.Z);
	}

	private static SVector3d Map(Vector3d model)
	{
		return new SVector3d
		{
			X = model.X,
			Y = model.Y,
			Z = model.Z
		};
	}
}
