using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.CommonBend;

namespace WiCAM.Pn4000.BendDoc;

internal class CombinedBendDescriptor : ICombinedBendDescriptorInternal, ICombinedBendDescriptor
{
	private readonly Doc3d _doc;

	private List<BendDescriptor> _bendDescriptors = new List<BendDescriptor>();

	private IFingerStopPointInternal selectedStopPointLeft;

	private IFingerStopPointInternal selectedStopPointRight;

	public IBendDescriptor this[int i] => this._bendDescriptors[i];

	public int Count => this._bendDescriptors.Count;

	public IEnumerable<IBendDescriptor> Enumerable => this._bendDescriptors.AsReadOnly();

	public IEnumerable<IBendDescriptor> Enumerable2 => this._bendDescriptors.AsReadOnly();

	public IEnumerable<IBendDescriptor> BendOrderUnfoldModel => this._bendDescriptors.OrderBy((BendDescriptor x) => this._bendDescriptors.FirstOrDefault()?.BendParams.UnfoldFaceGroup.ConcaveAxis.ParameterOfClosestPointOnAxis(x.BendParams.UnfoldFaceGroup.ConcaveAxis.Origin));

	public IEnumerable<IBendDescriptor> BendOrderBendModel => this._bendDescriptors.OrderBy((BendDescriptor x) => this._bendDescriptors.FirstOrDefault()?.BendParams.BendFaceGroup.ConcaveAxis.ParameterOfClosestPointOnAxis(x.BendParams.BendFaceGroup.ConcaveAxis.Origin));

	public List<BendDescriptor> BendsInternal => this._bendDescriptors;

	public IEnumerable<BendDescriptor> BendOrderUnfoldModelInternal => this._bendDescriptors.OrderBy((BendDescriptor x) => this._bendDescriptors.FirstOrDefault()?.BendParams.UnfoldFaceGroup.ConcaveAxis.ParameterOfClosestPointOnAxis(x.BendParams.UnfoldFaceGroup.ConcaveAxis.Origin));

	public IEnumerable<BendDescriptor> BendOrderBendModelInternal => this._bendDescriptors.OrderBy((BendDescriptor x) => this._bendDescriptors.FirstOrDefault()?.BendParams.BendFaceGroup.ConcaveAxis.ParameterOfClosestPointOnAxis(x.BendParams.BendFaceGroup.ConcaveAxis.Origin));

	public int Order { get; set; } = -1;

	public bool IsIncluded { get; set; } = true;

	public bool ToolsFound => this.ToolSetupId > 0;

	public CombinedBendType BendType { get; }

	public double ProgressStart { get; set; } = 1.0;

	public double ProgressStop { get; set; }

	public double StartProductAngleAbs => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * (1.0 - this.ProgressStart);

	public double StopProductAngleAbs => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * (1.0 - this.ProgressStop);

	public double StopProductAngleSigned => (this._bendDescriptors.FirstOrDefault()?.BendParams.Angle ?? 0.0) * (1.0 - this.ProgressStop);

	public double StartBendAngleAbs => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * this.ProgressStart;

	public double StopBendAngleAbs => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * this.ProgressStop;

	public double BendAngleAbsStart => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * (1.0 - this.ProgressStart);

	public double BendAngleAbsStop => (this._bendDescriptors.FirstOrDefault()?.BendParams.AngleAbs ?? 0.0) * (1.0 - this.ProgressStop);

	public double TotalLength
	{
		get
		{
			double num = this.PositioningInfo.StartEndOffsetsBends.Min(((double start, double end, int fgId, double fgOffset) trp) => trp.start);
			return Math.Round(this.PositioningInfo.StartEndOffsetsBends.Max(((double start, double end, int fgId, double fgOffset) trp) => trp.end) - num, 5);
		}
	}

	public double TotalLengthWithoutGaps => this.PositioningInfo.StartEndOffsetsBends.Sum(((double start, double end, int fgId, double fgOffset) trp) => trp.end - trp.start);

	public List<CombinedBendDescriptor> SplitPredecessors { get; set; }

	IReadOnlyList<ICombinedBendDescriptor> ICombinedBendDescriptor.SplitPredecessors => this.SplitPredecessors;

	public List<CombinedBendDescriptor> SplitSuccessors => this._doc._data.CombinedBendDescriptors.Where((CombinedBendDescriptor x) => x.SplitPredecessors?.Contains(this) ?? false).ToList();

	IReadOnlyList<ICombinedBendDescriptor> ICombinedBendDescriptor.SplitSuccessors => this.SplitSuccessors;

	public int SplitBendCount
	{
		get
		{
			int num = this.SplitBendOrder;
			ICombinedBendDescriptorInternal succ = this._doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x != null && x.SplitPredecessors?.Contains(this) == true);
			while (succ != null)
			{
				succ = this._doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x != null && x.SplitPredecessors?.Contains(succ) == true);
				num++;
			}
			return num;
		}
	}

	public int SplitBendOrder
	{
		get
		{
			int num = 0;
			List<CombinedBendDescriptor> list = this.SplitPredecessors;
			while (list != null && list.Any())
			{
				list = list.FirstOrDefault()?.SplitPredecessors;
				num++;
			}
			return num;
		}
	}

	public int? UserForcedUpperToolProfile { get; set; }

	public int? UserForcedLowerToolProfile { get; set; }

	public int? ToolSetupId { get; set; }

	public ToolSelectionType ToolSelectionAlgorithm { get; set; }

	[Obsolete]
	public int? UpperToolSetId { get; set; }

	[Obsolete]
	public int? LowerToolSetId { get; set; }

	[Obsolete]
	public int? UpperAdapterSetId { get; set; }

	[Obsolete]
	public int? LowerAdapterSetId { get; set; }

	public IBendPositioningInfoInternal PositioningInfo { get; set; } = new BendPositioningInfo();

	IBendPositioningInfo ICombinedBendDescriptor.PositioningInfo => this.PositioningInfo;

	public bool UseLiftingAid
	{
		get
		{
			if (this.UseLeftFrontLiftingAid == LiftingAidEnum.NoLiftingAid && this.UseRightFrontLiftingAid == LiftingAidEnum.NoLiftingAid && this.UseLeftBackLiftingAid == LiftingAidEnum.NoLiftingAid)
			{
				return this.UseRightBackLiftingAid != LiftingAidEnum.NoLiftingAid;
			}
			return true;
		}
	}

	public LiftingAidEnum UseLeftFrontLiftingAid { get; set; }

	public LiftingAidEnum UseRightFrontLiftingAid { get; set; }

	public LiftingAidEnum UseLeftBackLiftingAid { get; set; }

	public LiftingAidEnum UseRightBackLiftingAid { get; set; }

	public bool UseAngleMeasurement { get; set; }

	public double? AngleMeasurementPositionWorld { get; set; }

	public double? AngleMeasurementPositionRel { get; set; }

	public double? LeftFrontLiftingAidHorizontalCoordinar { get; set; }

	public double? LeftFrontLiftingAidVerticalCoordinar { get; set; }

	public double? LeftFrontLiftingAidRotationCoordinar { get; set; }

	public double? RightFrontLiftingAidHorizontalCoordinar { get; set; }

	public double? RightFrontLiftingAidVerticalCoordinar { get; set; }

	public double? RightFrontLiftingAidRotationCoordinar { get; set; }

	public double? LeftBackLiftingAidHorizontalCoordinar { get; set; }

	public double? LeftBackLiftingAidVerticalCoordinar { get; set; }

	public double? LeftBackLiftingAidRotationCoordinar { get; set; }

	public double? RightBackLiftingAidHorizontalCoordinar { get; set; }

	public double? RightBackLiftingAidVerticalCoordinar { get; set; }

	public double? RightBackLiftingAidRotationCoordinar { get; set; }

	public double ToolStationOffset { get; set; }

	public double ToolStationRefPointOffsetStart => this.ToolPresenceVector.RefPointOffsetStart;

	public IToolPresenceVector ToolPresenceVector { get; set; }

	public IFingerStopPointInternal? SelectedStopPointLeft
	{
		get
		{
			return this.selectedStopPointLeft;
		}
		set
		{
			this.selectedStopPointLeft = value;
			if (this.selectedStopPointLeft == null && this.selectedStopPointRight == null)
			{
				this.FingerPositioningMode = FingerPositioningMode.None;
			}
		}
	}

	public IFingerStopPointInternal? SelectedStopPointRight
	{
		get
		{
			return this.selectedStopPointRight;
		}
		set
		{
			this.selectedStopPointRight = value;
			if (this.selectedStopPointLeft == null && this.selectedStopPointRight == null)
			{
				this.FingerPositioningMode = FingerPositioningMode.None;
			}
		}
	}

	IFingerStopPoint ICombinedBendDescriptor.SelectedStopPointLeft => this.SelectedStopPointLeft;

	IFingerStopPoint ICombinedBendDescriptor.SelectedStopPointRight => this.SelectedStopPointRight;

	public List<IFingerStopPointInternal> StopPointsLeft { get; set; }

	public List<IFingerStopPointInternal> StopPointsRight { get; set; }

	public FingerPositioningMode FingerPositioningMode { get; set; }

	public FingerStability FingerStability { get; set; }

	public double? XLeftRetractAuto { get; set; }

	public double? XLeftRetractUser { get; set; }

	public double? XRightRetractAuto { get; set; }

	public double? XRightRetractUser { get; set; }

	public double? RLeftRetractAuto { get; set; }

	public double? RLeftRetractUser { get; set; }

	public double? RRightRetractAuto { get; set; }

	public double? RRightRetractUser { get; set; }

	public double? ZLeftRetractAuto { get; set; }

	public double? ZLeftRetractUser { get; set; }

	public double? ZRightRetractAuto { get; set; }

	public double? ZRightRetractUser { get; set; }

	public double? ReleasePointUser { get; set; }

	public bool LeftFingerSnap { get; set; } = true;

	public bool RightFingerSnap { get; set; } = true;

	public int BranchLength
	{
		get
		{
			FaceGroup fgVisible = this._doc.UnfoldModel3D.GetFaceGroupById(this._doc.VisibleFaceGroupId)?.GetParentRoot();
			if (fgVisible == null)
			{
				return int.MaxValue;
			}
			FaceGroupModelMapping fgMapping2 = new FaceGroupModelMapping(this._doc.UnfoldModel3D);
			return this._bendDescriptors.Select((BendDescriptor bd) => GetBranchLength(bd.BendParams.UnfoldFaceGroup, fgVisible, fgMapping2)).ToList().DefaultIfEmpty(int.MaxValue)
				.Min();
			static int GetBranchLength(FaceGroup g, FaceGroup fgDestination, FaceGroupModelMapping fgMapping)
			{
				HashSet<FaceGroup> hashSet = new HashSet<FaceGroup>();
				HashSet<FaceGroup> hashSet2 = new HashSet<FaceGroup> { g };
				int num = 0;
				bool flag = true;
				while (flag)
				{
					num++;
					flag = false;
					List<FaceGroup> list = hashSet2.ToList();
					hashSet2.Clear();
					foreach (FaceGroup item in list)
					{
						if (hashSet.Add(item))
						{
							Model model = item.GetModel(fgMapping);
							foreach (FaceGroup item2 in model.NeighborMapping.Neighbors0(item).Concat(model.NeighborMapping.Neighbors1(item)))
							{
								if (item2 == fgDestination)
								{
									return num;
								}
								hashSet2.Add(item2);
							}
							flag = true;
						}
					}
				}
				return int.MaxValue;
			}
		}
	}

	public string Comment { get; set; }

	[Obsolete]
	public int PreferredProfilePrio { get; set; }

	public MachinePartInsertionDirection MachinePartInsertionDirection { get; set; } = MachinePartInsertionDirection.PartCentroidInFrontOfMachine;

	public double AutoPressForce
	{
		get
		{
			IBendPositioning bendPositioning = this._doc.ToolsAndBends.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == this.Order);
			if (bendPositioning == null || bendPositioning.PunchProfile == null || bendPositioning.DieProfile == null)
			{
				return 0.0;
			}
			if (!bendPositioning.PunchProfile.Radius.HasValue || !bendPositioning.DieProfile.VWidth.HasValue)
			{
				if (this.BendType == CombinedBendType.HemBend && this._doc.BendMachine.PressForceHemTable.Entries.Any())
				{
					return this._doc.BendMachine.PressForceHemTable.GetInterpolatedValue(this._doc.Material.Number, this._doc.Thickness, this.TotalLengthWithoutGaps);
				}
				return Math.Max(this._doc.BendMachine.PressBrakeData.PressForceMin, Math.Min(this._doc.BendMachine.PressBrakeData.PressForceMax, Math.Min(bendPositioning.PunchProfile.MaxToolLoad * this.TotalLengthWithoutGaps * 0.001, bendPositioning.DieProfile.MaxToolLoad * this.TotalLengthWithoutGaps * 0.001)));
			}
			double value = bendPositioning.PunchProfile.Radius.Value;
			double value2 = bendPositioning.DieProfile.VWidth.Value;
			double tensileStrength = this._doc.Material.TensileStrength;
			double thickness = this._doc.Thickness;
			double num = Math.Round(value2 / thickness, 6);
			return ((num <= 6.0) ? 1.85 : ((!(num > 6.0) || !(num < 16.0)) ? 1.6 : 1.72)) * this.TotalLengthWithoutGaps * tensileStrength * Math.Pow(thickness, 2.0) / (value2 - 2.0 * Math.Cos(double.DegreesToRadians(45.0)) * value) / 1000.0;
		}
	}

	public double? UserPressForce { get; set; }

	public int? UserStepChangeMode { get; set; }

	public CombinedBendDescriptor(List<BendDescriptor> bends, Doc3d doc, CombinedBendType bendType = CombinedBendType.Bend)
	{
		this._doc = doc;
		this._bendDescriptors.AddRange(bends);
		this.BendType = bendType;
		this.ReorderBends();
		this.UpdatePositioningInfo();
	}

	public CombinedBendDescriptor(CombinedBendDescriptor cbd, CombinedBendType bendType = CombinedBendType.Bend, Doc3d doc = null, IEnumerable<IBendDescriptor>? bendDescriptors = null)
	{
		this._doc = doc ?? cbd._doc;
		if (bendDescriptors != null)
		{
			this._bendDescriptors.AddRange(bendDescriptors.Select((IBendDescriptor x) => x as BendDescriptor));
		}
		else
		{
			this._bendDescriptors.AddRange(cbd._bendDescriptors);
		}
		this.BendType = bendType;
		cbd.CopyMembers(this);
		this.ReorderBends();
		this.UpdatePositioningInfo();
	}

	public void UpdatePositioningInfo()
	{
		this.PositioningInfo.UpdateBendOffsets(this._bendDescriptors.SelectMany(delegate(BendDescriptor x)
		{
			FaceGroup unfoldFaceGroup = x.BendParams.UnfoldFaceGroup;
			return (!unfoldFaceGroup.SubGroups.Any()) ? new List<FaceGroup> { unfoldFaceGroup } : unfoldFaceGroup.SubGroups.ToList();
		}).ToList(), this._doc.UnfoldModel3D);
	}

	public void Add(BendDescriptor desc)
	{
		this._bendDescriptors.Add(desc);
	}

	public void Remove(BendDescriptor desc)
	{
		this._bendDescriptors.Remove(desc);
	}

	public bool IsCompatibleBendUnfoldModel(ICombinedBendDescriptorInternal other)
	{
		return this.IsCompatibleBend(other, (IBendDescriptor bend) => (fg: bend?.BendParams.UnfoldFaceGroup, model: bend?.BendParams.UnfoldFaceGroupModel), delegate(ICombinedBendDescriptorInternal cbd, double step)
		{
			cbd.UnfoldBendInUnfoldModel(step, relative: false, noGeometryChange: true);
		});
	}

	public bool IsCompatibleBendBendModel(ICombinedBendDescriptorInternal other)
	{
		return this.IsCompatibleBend(other, (IBendDescriptor bend) => (fg: bend?.BendParams.BendFaceGroup, model: bend?.BendParams.BendFaceGroupModel), delegate(ICombinedBendDescriptorInternal cbd, double step)
		{
			cbd.UnfoldBendInBendModel(step, relative: false, noGeometryChange: true);
		});
	}

	private bool IsCompatibleBend(ICombinedBendDescriptorInternal other, Func<IBendDescriptor, (FaceGroup fg, Model model)> modelSelector, Action<ICombinedBendDescriptorInternal, double> unfoldAction)
	{
		if (Math.Abs(this.ProgressStart - other.ProgressStart) > 1E-06 || Math.Abs(this.ProgressStop - other.ProgressStop) > 1E-06)
		{
			return false;
		}
		(FaceGroup, Model) tuple = modelSelector(this._bendDescriptors.FirstOrDefault());
		(FaceGroup, Model) tuple2 = modelSelector(other.Enumerable.FirstOrDefault());
		FaceGroup item = tuple.Item1;
		FaceGroup item2 = tuple2.Item1;
		item = item.ParentGroup ?? item;
		item2 = item2.ParentGroup ?? item2;
		if (!item.IsBendingZone || !item2.IsBendingZone)
		{
			return false;
		}
		double num = item.InvalidChangedBendRadius ?? item.ConvexAxis.Radius;
		double num2 = item2.InvalidChangedBendRadius ?? item2.ConvexAxis.Radius;
		double num3 = this.ProgressStart - this.ProgressStop;
		double num4 = other.ProgressStart - other.ProgressStop;
		double num5 = item.ConvexAxis.OpeningAngle * num3;
		double num6 = item2.ConvexAxis.OpeningAngle * num4;
		if (Math.Abs(num - num2) > 0.005 || Math.Abs(num5 - num6) > 0.001)
		{
			return false;
		}
		double bendStep = item.BendStep;
		double bendStep2 = item2.BendStep;
		Model item3 = tuple.Item2;
		Model item4 = tuple2.Item2;
		bool flag = true;
		unfoldAction(this, this.ProgressStop);
		unfoldAction(other, other.ProgressStop);
		Line line = new Line(item.BendMiddlePoint, item.ConcaveAxis.Direction);
		Matrix4d matrix4d = item4.WorldMatrix * item3.WorldMatrix.Inverted;
		Vector3d v = item2.BendMiddlePoint;
		matrix4d.TransformInPlace(ref v);
		double length = (line.ClosestPointOnAxis(v) - v).Length;
		Vector3d v2 = matrix4d.TransformNormal(item2.ConcaveAxis.Direction);
		flag = length < 0.01 && item.ConcaveAxis.Direction.IsParallel(v2);
		if (flag)
		{
			Line line2 = new Line(item.BendMinPoint, item.ConcaveAxis.Direction);
			Vector3d v3 = item2.BendMinPoint;
			Vector3d v4 = item2.BendMaxPoint;
			matrix4d.TransformInPlace(ref v3);
			matrix4d.TransformInPlace(ref v4);
			double length2 = (line2.ClosestPointOnAxis(v3) - v3).Length;
			double length3 = (line2.ClosestPointOnAxis(v4) - v4).Length;
			flag = length2 < 0.01 || length3 < 0.01;
			if (flag)
			{
				unfoldAction(this, this.ProgressStart);
				unfoldAction(other, other.ProgressStart);
				Line line3 = new Line(item.BendMiddlePoint, item.ConcaveAxis.Direction);
				matrix4d = item4.WorldMatrix * item3.WorldMatrix.Inverted;
				v = item2.BendMiddlePoint;
				matrix4d.TransformInPlace(ref v);
				double length4 = (line3.ClosestPointOnAxis(v) - v).Length;
				v2 = matrix4d.TransformNormal(item2.ConcaveAxis.Direction);
				flag = length4 < 0.01 && item.ConcaveAxis.Direction.IsParallel(v2);
				if (flag)
				{
					Line line4 = new Line(item.BendMinPoint, item.ConcaveAxis.Direction);
					v3 = item2.BendMinPoint;
					v4 = item2.BendMaxPoint;
					matrix4d.TransformInPlace(ref v3);
					matrix4d.TransformInPlace(ref v4);
					length2 = (line4.ClosestPointOnAxis(v3) - v3).Length;
					length3 = (line4.ClosestPointOnAxis(v4) - v4).Length;
					flag = length2 < 0.01 || length3 < 0.01;
				}
			}
		}
		unfoldAction(this, bendStep);
		unfoldAction(other, bendStep2);
		return flag;
	}

	public bool IsSplitableBend()
	{
		return this._bendDescriptors.Count > 1;
	}

	public void UnfoldBendInModifiedEntryModel(double step, bool relative, bool noGeometryChange = false)
	{
		this.UnfoldBendInModel(step, relative, UiModelType.ModifiedEntry, noGeometryChange);
	}

	public void UnfoldBendInUnfoldModel(double step, bool relative, bool noGeometryChange = false)
	{
		this.UnfoldBendInModel(step, relative, UiModelType.Unfold, noGeometryChange);
	}

	public void UnfoldBendInBendModel(double step, bool relative, bool noGeometryChange = false)
	{
		this.UnfoldBendInModel(step, relative, UiModelType.Bend, noGeometryChange);
	}

	public void UnfoldBendInModel(double step, bool relative, UiModelType uiModelType, bool noGeometryChange = false)
	{
		Func<BendDescriptor, FaceGroup> func = null;
		Model model = null;
		switch (uiModelType)
		{
		case UiModelType.Bend:
			func = (BendDescriptor x) => x.BendParamsInternal.BendFaceGroup;
			model = this._doc.BendModel3D;
			break;
		case UiModelType.Unfold:
			func = (BendDescriptor x) => x.BendParamsInternal.UnfoldFaceGroup;
			model = this._doc.UnfoldModel3D;
			break;
		case UiModelType.ModifiedEntry:
			func = (BendDescriptor x) => x.BendParamsInternal.ModifiedEntryFaceGroup;
			model = this._doc.ModifiedEntryModel3D;
			break;
		default:
			throw new Exception("Invalid model");
		}
		List<FaceGroup> groups = this._bendDescriptors.Select(func).ToList();
		FaceGroupModelMapping fgMapping = new FaceGroupModelMapping(model);
		double step2 = step;
		if (relative)
		{
			step2 = (this.ProgressStart - this.ProgressStop) * step + this.ProgressStop;
		}
		this.UnfoldBends(groups, fgMapping, step2, noGeometryChange);
	}

	public bool SplitAtGapIndicesUnfoldModel(List<int> splitGaps, out List<CombinedBendDescriptor> result)
	{
		return this.SplitAtGapIndices(splitGaps, this.BendOrderUnfoldModelInternal.ToList(), out result);
	}

	public bool SplitAtGapIndicesBendModel(List<int> splitGaps, out List<CombinedBendDescriptor> result)
	{
		return this.SplitAtGapIndices(splitGaps, this.BendOrderBendModelInternal.ToList(), out result);
	}

	private bool SplitAtGapIndices(List<int> splitGaps, List<BendDescriptor> bendOrder, out List<CombinedBendDescriptor> result)
	{
		result = new List<CombinedBendDescriptor>();
		if (splitGaps.Count == 0)
		{
			return false;
		}
		int num = 0;
		List<CombinedBendDescriptor> list = this._doc._data.CombinedBendDescriptors.Where((CombinedBendDescriptor x) => x.SplitPredecessors?.Contains(this) ?? false).ToList();
		foreach (int splitGap in splitGaps)
		{
			CombinedBendDescriptor cbd = new CombinedBendDescriptor(bendOrder.Skip(num).Take(splitGap - num).ToList(), this._doc, this.BendType);
			this.CopyMembers(cbd);
			cbd.SplitPredecessors = this.SplitPredecessors?.Where((CombinedBendDescriptor predCbd) => predCbd.Enumerable.Any((IBendDescriptor bendDesc) => cbd._bendDescriptors.Contains(bendDesc))).ToList();
			result.Add(cbd);
			num = splitGap;
		}
		CombinedBendDescriptor cbdEnd = new CombinedBendDescriptor(bendOrder.Skip(num).ToList(), this._doc, this.BendType);
		this.CopyMembers(cbdEnd);
		cbdEnd.SplitPredecessors = this.SplitPredecessors?.Where((CombinedBendDescriptor predCbd) => predCbd.Enumerable.Any((IBendDescriptor bendDesc) => cbdEnd._bendDescriptors.Contains(bendDesc))).ToList();
		result.Add(cbdEnd);
		foreach (CombinedBendDescriptor successor in list)
		{
			successor.SplitPredecessors.Remove(this);
			successor.SplitPredecessors.AddRange(result.Where((CombinedBendDescriptor predCbd) => predCbd.Enumerable.Any((IBendDescriptor bendDesc) => successor.Enumerable.Contains(bendDesc))));
		}
		return true;
	}

	public Pair<Vector3d, Vector3d> GetEndPointsInWorldCoordsUnfoldModel()
	{
		return this.GetEndPointsInWorldCoords((IBendDescriptor bd) => (fg: bd.BendParams.UnfoldFaceGroup, model: bd.BendParams.UnfoldFaceGroupModel));
	}

	public Pair<Vector3d, Vector3d> GetEndPointsInWorldCoordsBendModel()
	{
		return this.GetEndPointsInWorldCoords((IBendDescriptor bd) => (fg: bd.BendParams.BendFaceGroup, model: bd.BendParams.BendFaceGroupModel));
	}

	public double GetDistanceToModelMiddleUnfoldModel()
	{
		Pair<Vector3d, Vector3d> boundary = this._doc.UnfoldModel3D.GetBoundary(Matrix4d.Identity);
		return (new Vector3d((boundary.Item2.X + boundary.Item1.X) / 2.0, (boundary.Item2.Y + boundary.Item1.Y) / 2.0, (boundary.Item2.Z + boundary.Item1.Z) / 2.0) - this.GetBendLineMiddlePointUnfoldModel()).Length;
	}

	public Vector3d GetBendingLineDirectionUnfoldModel(int precision = 5)
	{
		Pair<Vector3d, Vector3d> endPointsInWorldCoordsUnfoldModel = this.GetEndPointsInWorldCoordsUnfoldModel();
		Vector3d normalized = (endPointsInWorldCoordsUnfoldModel.Item2 - endPointsInWorldCoordsUnfoldModel.Item1).Normalized;
		return new Vector3d(Math.Round(normalized.X, precision), Math.Round(normalized.Y, precision), Math.Round(normalized.Z, precision));
	}

	public (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetAllNeighboursInBendModel()
	{
		return this.GetDirectNeighbours((IBendDescriptor bd) => (fg: bd.BendParams.BendFaceGroup, model: bd.BendParams.BendFaceGroupModel));
	}

	public (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetAllNeighboursInUnfoldModel()
	{
		return this.GetDirectNeighbours((IBendDescriptor bd) => (fg: bd.BendParams.UnfoldFaceGroup, model: bd.BendParams.UnfoldFaceGroupModel));
	}

	private (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetDirectNeighbours(Func<IBendDescriptor, (FaceGroup fg, Model model)> modelSelector)
	{
		(HashSet<FaceGroup>, HashSet<FaceGroup>) result = (new HashSet<FaceGroup>(), new HashSet<FaceGroup>());
		(FaceGroup, Model) tuple = modelSelector(this._bendDescriptors.FirstOrDefault());
		FaceGroup item = tuple.Item1;
		Matrix4d worldMatrix = tuple.Item2.WorldMatrix;
		Plane plane = new Plane(worldMatrix.Transform(item.BendMiddlePoint), worldMatrix.TransformNormal(item.ConcaveAxis.Direction ^ item.BendMiddlePointNormal.Normalized));
		foreach (BendDescriptor bendDescriptor in this._bendDescriptors)
		{
			(FaceGroup, Model) tuple2 = modelSelector(bendDescriptor);
			FaceGroup item2 = tuple2.Item1;
			Matrix4d worldMatrix2 = tuple2.Item2.WorldMatrix;
			foreach (KeyValuePair<FaceGroup, List<Pair<TriangleHalfEdge, TriangleHalfEdge>>> transitionEdge in item2.GetTransitionEdges(tuple2.Item2))
			{
				if (transitionEdge.Value.Any())
				{
					if (plane.SignedDistanceToPoint(worldMatrix2.Transform(transitionEdge.Value.FirstOrDefault().Item1.V0.Pos)) < 0.0)
					{
						result.Item1.Add(transitionEdge.Key);
					}
					else
					{
						result.Item2.Add(transitionEdge.Key);
					}
				}
			}
		}
		return result;
	}

	public (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBendInBendModel()
	{
		return this.GetLegsOfBend((IBendDescriptor bd) => (fg: bd.BendParams.BendFaceGroup, model: bd.BendParams.BendFaceGroupModel), new FaceGroupModelMapping(this._doc.BendModel3D));
	}

	public (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBendInUnfoldModel()
	{
		return this.GetLegsOfBend((IBendDescriptor bd) => (fg: bd.BendParams.UnfoldFaceGroup, model: bd.BendParams.UnfoldFaceGroupModel), new FaceGroupModelMapping(this._doc.UnfoldModel3D));
	}

	private (HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBend(Func<IBendDescriptor, (FaceGroup fg, Model model)> modelSelector, FaceGroupModelMapping fgMapping)
	{
		(HashSet<FaceGroup>, HashSet<FaceGroup>) directNeighbours = this.GetDirectNeighbours(modelSelector);
		HashSet<FaceGroup> hashSet = this._bendDescriptors.Select((BendDescriptor bd) => modelSelector(bd).Item1).ToHashSet();
		Stack<FaceGroup> stack = new Stack<FaceGroup>();
		foreach (FaceGroup item in directNeighbours.Item1)
		{
			stack.Push(item);
		}
		while (stack.Any())
		{
			FaceGroup faceGroup = stack.Pop();
			Model gModel = faceGroup.GetModel(fgMapping);
			foreach (FaceGroup item2 in faceGroup.SubGroups.SelectMany((FaceGroup x) => gModel.NeighborMapping.Neighbors0(x).Concat(gModel.NeighborMapping.Neighbors1(x))).Concat(gModel.NeighborMapping.Neighbors0(faceGroup)).Concat(gModel.NeighborMapping.Neighbors1(faceGroup))
				.ToList())
			{
				if (!hashSet.Contains(item2) && !directNeighbours.Item1.Contains(item2))
				{
					directNeighbours.Item1.Add(item2);
					stack.Push(item2);
				}
			}
		}
		foreach (FaceGroup item3 in directNeighbours.Item2)
		{
			stack.Push(item3);
		}
		while (stack.Any())
		{
			FaceGroup faceGroup2 = stack.Pop();
			Model gModel2 = faceGroup2.GetModel(fgMapping);
			foreach (FaceGroup item4 in faceGroup2.SubGroups.SelectMany((FaceGroup x) => gModel2.NeighborMapping.Neighbors0(x).Concat(gModel2.NeighborMapping.Neighbors1(x))).Concat(gModel2.NeighborMapping.Neighbors0(faceGroup2)).Concat(gModel2.NeighborMapping.Neighbors1(faceGroup2))
				.ToList())
			{
				if (!hashSet.Contains(item4) && !directNeighbours.Item2.Contains(item4))
				{
					directNeighbours.Item2.Add(item4);
					stack.Push(item4);
				}
			}
		}
		return directNeighbours;
	}

	public void ReorderBends()
	{
		this._bendDescriptors = this._bendDescriptors.OrderBy((BendDescriptor x) => x.ID).ToList();
	}

	public void CopyMembers(CombinedBendDescriptor dest)
	{
		dest.Order = this.Order;
		dest.IsIncluded = this.IsIncluded;
		dest.MachinePartInsertionDirection = this.MachinePartInsertionDirection;
		dest.ProgressStart = this.ProgressStart;
		dest.ProgressStop = this.ProgressStop;
		dest.LowerToolSetId = this.LowerToolSetId;
		dest.UpperToolSetId = this.UpperToolSetId;
		dest.LowerAdapterSetId = this.LowerAdapterSetId;
		dest.UpperAdapterSetId = this.UpperAdapterSetId;
		dest.ToolSetupId = this.ToolSetupId;
		dest.ToolSelectionAlgorithm = this.ToolSelectionAlgorithm;
		dest.UserForcedUpperToolProfile = this.UserForcedUpperToolProfile;
		dest.UserForcedLowerToolProfile = this.UserForcedLowerToolProfile;
		dest.StopPointsLeft = this.StopPointsLeft;
		dest.StopPointsRight = this.StopPointsRight;
		dest.SelectedStopPointLeft = this.SelectedStopPointLeft;
		dest.SelectedStopPointRight = this.SelectedStopPointRight;
		dest.XLeftRetractAuto = this.XLeftRetractAuto;
		dest.XLeftRetractUser = this.XLeftRetractUser;
		dest.XLeftRetractAuto = this.XRightRetractAuto;
		dest.XLeftRetractUser = this.XRightRetractUser;
		dest.UseLeftFrontLiftingAid = this.UseLeftFrontLiftingAid;
		dest.UseRightFrontLiftingAid = this.UseRightFrontLiftingAid;
		dest.UseLeftBackLiftingAid = this.UseLeftBackLiftingAid;
		dest.UseRightBackLiftingAid = this.UseRightBackLiftingAid;
		dest.LeftBackLiftingAidHorizontalCoordinar = this.LeftBackLiftingAidHorizontalCoordinar;
		dest.LeftBackLiftingAidRotationCoordinar = this.LeftBackLiftingAidRotationCoordinar;
		dest.LeftBackLiftingAidVerticalCoordinar = this.LeftBackLiftingAidVerticalCoordinar;
		dest.LeftFrontLiftingAidHorizontalCoordinar = this.LeftFrontLiftingAidHorizontalCoordinar;
		dest.LeftFrontLiftingAidRotationCoordinar = this.LeftFrontLiftingAidRotationCoordinar;
		dest.LeftFrontLiftingAidVerticalCoordinar = this.LeftFrontLiftingAidVerticalCoordinar;
		dest.RightBackLiftingAidHorizontalCoordinar = this.RightBackLiftingAidHorizontalCoordinar;
		dest.RightBackLiftingAidRotationCoordinar = this.RightBackLiftingAidRotationCoordinar;
		dest.RightBackLiftingAidVerticalCoordinar = this.RightBackLiftingAidVerticalCoordinar;
		dest.RightFrontLiftingAidHorizontalCoordinar = this.RightFrontLiftingAidHorizontalCoordinar;
		dest.RightFrontLiftingAidRotationCoordinar = this.RightFrontLiftingAidRotationCoordinar;
		dest.RightFrontLiftingAidVerticalCoordinar = this.RightFrontLiftingAidVerticalCoordinar;
	}

	public Pair<Vector3d, Vector3d> GetEndPointsInWorldCoords(Func<IBendDescriptor, (FaceGroup fg, Model model)> modelSelector)
	{
		(FaceGroup, Model) tuple = modelSelector(this._bendDescriptors.FirstOrDefault());
		FaceGroup item = tuple.Item1;
		Matrix4d worldMatrix = tuple.Item2.WorldMatrix;
		Vector3d v = item.BendMiddlePoint;
		Vector3d v2 = item.ConcaveAxis.Direction;
		worldMatrix.TransformInPlace(ref v);
		worldMatrix.TransformNormalInPlace(ref v2);
		Line line = new Line(v, v2);
		double num = item.ConcaveAxis.MinParameter;
		double num2 = item.ConcaveAxis.MaxParameter;
		Vector3d pointOnAxisByParameter = line.GetPointOnAxisByParameter(num);
		Vector3d pointOnAxisByParameter2 = line.GetPointOnAxisByParameter(num2);
		foreach (BendDescriptor item3 in this._bendDescriptors.Skip(1))
		{
			(FaceGroup, Model) tuple2 = modelSelector(item3);
			FaceGroup item2 = tuple2.Item1;
			Matrix4d worldMatrix2 = tuple2.Item2.WorldMatrix;
			Vector3d v3 = item2.BendMiddlePoint;
			Vector3d v4 = item2.ConcaveAxis.Direction;
			worldMatrix2.TransformInPlace(ref v3);
			worldMatrix2.TransformNormalInPlace(ref v4);
			Line line2 = new Line(v3, v4);
			Vector3d pointOnAxisByParameter3 = line2.GetPointOnAxisByParameter(item2.ConcaveAxis.MinParameter);
			Vector3d pointOnAxisByParameter4 = line2.GetPointOnAxisByParameter(item2.ConcaveAxis.MaxParameter);
			double val = line.ParameterOfClosestPointOnAxis(pointOnAxisByParameter3);
			double val2 = line.ParameterOfClosestPointOnAxis(pointOnAxisByParameter4);
			double num3 = Math.Min(val, val2);
			double num4 = Math.Max(val, val2);
			if (num3 < num)
			{
				num = num3;
				pointOnAxisByParameter = line.GetPointOnAxisByParameter(num);
			}
			if (num4 > num2)
			{
				num2 = num4;
				pointOnAxisByParameter2 = line.GetPointOnAxisByParameter(num2);
			}
		}
		return new Pair<Vector3d, Vector3d>(pointOnAxisByParameter, pointOnAxisByParameter2);
	}

	private List<int> GetBendSplitIndices(Func<BendDescriptor, (FaceGroup fg, Model model)> modelSelector, Action<double> unfoldAction)
	{
		HashSet<int> hashSet = new HashSet<int>();
		List<BendDescriptor> source = this.BendOrderUnfoldModelInternal.ToList();
		(FaceGroup, Model) tuple = modelSelector(source.FirstOrDefault());
		FaceGroup item = tuple.Item1;
		Model item2 = tuple.Item2;
		double bendStep = item.BendStep;
		unfoldAction(this.ProgressStart);
		int num = 1;
		foreach (var item4 in from zone in source.Skip(1)
			select ((FaceGroup fg, Model model))modelSelector(zone))
		{
			FaceGroup item3 = item4.fg;
			double num2 = item.InvalidChangedBendRadius ?? item.ConvexAxis.Radius;
			double num3 = item3.InvalidChangedBendRadius ?? item3.ConvexAxis.Radius;
			double num4 = this.ProgressStart - this.ProgressStop;
			double num5 = item.ConvexAxis.OpeningAngle * num4;
			double num6 = item3.ConvexAxis.OpeningAngle * num4;
			if (Math.Abs(num2 - num3) > 0.005 || Math.Abs(num5 - num6) > 1E-06)
			{
				hashSet.Add(num);
				num++;
				continue;
			}
			Line line = new Line(item.BendMiddlePoint, item.ConcaveAxis.Direction);
			Matrix4d matrix4d = item4.model.WorldMatrix * item2.WorldMatrix.Inverted;
			Vector3d v = item3.BendMiddlePoint;
			matrix4d.TransformInPlace(ref v);
			if (!((line.ClosestPointOnAxis(v) - v).Length < 0.01))
			{
				hashSet.Add(num);
				num++;
				continue;
			}
			Line line2 = new Line(item.BendMinPoint, item.ConcaveAxis.Direction);
			Vector3d v2 = item3.BendMinPoint;
			Vector3d v3 = item3.BendMaxPoint;
			matrix4d.TransformInPlace(ref v2);
			matrix4d.TransformInPlace(ref v3);
			double length = (line2.ClosestPointOnAxis(v2) - v2).Length;
			double length2 = (line2.ClosestPointOnAxis(v3) - v3).Length;
			if (!(length < 0.01) && !(length2 < 0.01))
			{
				hashSet.Add(num);
				num++;
			}
			else
			{
				num++;
			}
		}
		unfoldAction(this.ProgressStop);
		num = 1;
		foreach (var item5 in from zone in source.Skip(1)
			select ((FaceGroup fg, Model model))modelSelector(zone))
		{
			var (faceGroup, _) = item5;
			if (hashSet.Contains(num))
			{
				num++;
				continue;
			}
			Line line3 = new Line(item.BendMiddlePoint, item.ConcaveAxis.Direction);
			Matrix4d matrix4d2 = item5.model.WorldMatrix * item2.WorldMatrix.Inverted;
			Vector3d v4 = faceGroup.BendMiddlePoint;
			matrix4d2.TransformInPlace(ref v4);
			if (!((line3.ClosestPointOnAxis(v4) - v4).Length < 0.01))
			{
				hashSet.Add(num);
				num++;
				continue;
			}
			Line line4 = new Line(item.BendMinPoint, item.ConcaveAxis.Direction);
			Vector3d v5 = faceGroup.BendMinPoint;
			Vector3d v6 = faceGroup.BendMaxPoint;
			matrix4d2.TransformInPlace(ref v5);
			matrix4d2.TransformInPlace(ref v6);
			double length3 = (line4.ClosestPointOnAxis(v5) - v5).Length;
			double length4 = (line4.ClosestPointOnAxis(v6) - v6).Length;
			if (!(length3 < 0.01) && !(length4 < 0.01))
			{
				hashSet.Add(num);
				num++;
			}
			else
			{
				num++;
			}
		}
		unfoldAction(bendStep);
		return hashSet.OrderBy((int x) => x).ToList();
	}

	private Vector3d GetBendLineMiddlePointUnfoldModel()
	{
		Pair<Vector3d, Vector3d> endPointsInWorldCoordsUnfoldModel = this.GetEndPointsInWorldCoordsUnfoldModel();
		return (endPointsInWorldCoordsUnfoldModel.Item2 + endPointsInWorldCoordsUnfoldModel.Item1) * 0.5;
	}

	private void UnfoldBends(List<FaceGroup> groups, FaceGroupModelMapping fgMapping, double step, bool noGeometryChange = false)
	{
		foreach (FaceGroup item in groups.Distinct())
		{
			item.Unfold(fgMapping, item.UnfoldBendLength, item.KFactor, this._doc.Thickness, step, noGeometryChange);
		}
	}

	public void ResetMachineSpecificData()
	{
		this.ResetStopPoints();
		this.ResetAcbLaser();
		this.LowerAdapterSetId = null;
		this.UpperAdapterSetId = null;
		foreach (BendDescriptor bendDescriptor in this._bendDescriptors)
		{
			bendDescriptor.BendParamsInternal.ToolRadius = null;
		}
	}

	public void ResetStopPoints()
	{
		this.SelectedStopPointLeft = null;
		this.SelectedStopPointRight = null;
		this.StopPointsLeft = new List<IFingerStopPointInternal>();
		this.StopPointsRight = new List<IFingerStopPointInternal>();
		this.SelectedStopPointLeft = null;
		this.SelectedStopPointRight = null;
		this.FingerPositioningMode = FingerPositioningMode.None;
	}

	public void ResetAcbLaser()
	{
		this.UseAngleMeasurement = false;
		this.AngleMeasurementPositionRel = null;
	}

	public void ResetTools()
	{
		this.ToolPresenceVector = null;
		this.ToolSelectionAlgorithm = ToolSelectionType.NoTools;
		this.ToolSetupId = null;
		this.ToolStationOffset = 0.0;
		this.UserForcedUpperToolProfile = null;
		this.UserForcedLowerToolProfile = null;
		foreach (BendDescriptor bendDescriptor in this._bendDescriptors)
		{
			bendDescriptor.BendParamsInternal.ToolRadius = null;
		}
	}

	public List<CombinedBendDescriptor> GetAllSplitSuccessors()
	{
		List<CombinedBendDescriptor> list = new List<CombinedBendDescriptor>();
		Stack<CombinedBendDescriptor> stack = new Stack<CombinedBendDescriptor>(this.SplitSuccessors);
		while (stack.Any())
		{
			CombinedBendDescriptor combinedBendDescriptor = stack.Pop();
			list.Add(combinedBendDescriptor);
			foreach (CombinedBendDescriptor splitSuccessor in combinedBendDescriptor.SplitSuccessors)
			{
				stack.Push(splitSuccessor);
			}
		}
		return list;
	}

	public List<CombinedBendDescriptor> GetAllSplitPredecessors()
	{
		List<CombinedBendDescriptor> list = new List<CombinedBendDescriptor>();
		Stack<CombinedBendDescriptor> stack = new Stack<CombinedBendDescriptor>(this.SplitPredecessors);
		while (stack.Any())
		{
			CombinedBendDescriptor combinedBendDescriptor = stack.Pop();
			list.Add(combinedBendDescriptor);
			foreach (CombinedBendDescriptor splitPredecessor in combinedBendDescriptor.SplitPredecessors)
			{
				stack.Push(splitPredecessor);
			}
		}
		return list;
	}

	public void ActivateAndAutoPositionAngleMeasurementSystem(bool active, bool recalcSim)
	{
		this.UseAngleMeasurement = active;
		if (this.UseAngleMeasurement)
		{
			IBendPositioning bendPositioning = this._doc.ToolsAndBends?.GetBend(this);
			if (bendPositioning != null)
			{
				this.AngleMeasurementPositionRel = bendPositioning.BendingZonesOrientated.OrderByDescending((IRange x) => x.Length).First().Center;
			}
		}
		else
		{
			_ = this._doc.BendSimulation?.State.LinearAxesByType[AxisType.W];
			this.AngleMeasurementPositionRel = null;
		}
		if (recalcSim)
		{
			this._doc.RecalculateSimulation();
		}
	}

	public CombinedBendDescriptor Copy(Doc3d newDoc)
	{
		CombinedBendDescriptor combinedBendDescriptor = new CombinedBendDescriptor(newDoc._data.BendDescriptors.Where((BendDescriptor bd) => this.BendsInternal.Any((BendDescriptor x) => x.ID == bd.ID)).ToList(), newDoc, this.BendType);
		this.CopyMembers(combinedBendDescriptor);
		if (this.SplitPredecessors != null)
		{
			combinedBendDescriptor.SplitPredecessors = new List<CombinedBendDescriptor>();
			combinedBendDescriptor.SplitPredecessors.AddRange(this.SplitPredecessors.Select((CombinedBendDescriptor x) => newDoc._data.CombinedBendDescriptors[this._doc._data.CombinedBendDescriptors.IndexOf(x)]));
		}
		return combinedBendDescriptor;
	}

	public void Merge(CombinedBendDescriptor other)
	{
		this.BendsInternal.AddRange(other.BendsInternal);
		this.SplitPredecessors?.Remove(other);
		other.SplitPredecessors?.Remove(this);
		if (this.SplitPredecessors == null)
		{
			this.SplitPredecessors = other.SplitPredecessors;
		}
		else
		{
			this.SplitPredecessors.AddRange(other.SplitPredecessors ?? new List<CombinedBendDescriptor>());
			this.SplitPredecessors = this.SplitPredecessors.Distinct().ToList();
		}
		foreach (CombinedBendDescriptor combinedBendDescriptor in this._doc._data.CombinedBendDescriptors)
		{
			if (combinedBendDescriptor == this)
			{
				continue;
			}
			List<CombinedBendDescriptor> splitPredecessors = combinedBendDescriptor.SplitPredecessors;
			if (splitPredecessors != null && splitPredecessors.Contains(other))
			{
				combinedBendDescriptor.SplitPredecessors.Remove(other);
				if (!combinedBendDescriptor.SplitPredecessors.Contains(this))
				{
					combinedBendDescriptor.SplitPredecessors.Add(this);
				}
			}
		}
		this.ReorderBends();
		this.UpdatePositioningInfo();
	}
}
