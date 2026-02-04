using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;
using Wintellect.PowerCollections;

namespace WiCAM.Pn4000.BendDoc;

internal static class CombinedBendsSorter
{
	private class StartShortestBendAndFollowClosestBendData
	{
		public CombinedBendDescriptor LastBend { get; set; }
	}

	private delegate IOrderedEnumerable<CombinedBendDescriptor> SortDelegate(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData);

	private static readonly Dictionary<BendSequenceSorts, SortDelegate> SortActions = new Dictionary<BendSequenceSorts, SortDelegate>
	{
		{
			BendSequenceSorts.InToOutModel,
			InToOutModel
		},
		{
			BendSequenceSorts.OutToInModel,
			OutToInModel
		},
		{
			BendSequenceSorts.ShortToLongWithGaps,
			ShortToLongWithGaps
		},
		{
			BendSequenceSorts.LongToShortWithGaps,
			LongToShortWithGaps
		},
		{
			BendSequenceSorts.InToOutMainFace,
			InToOutMainFace
		},
		{
			BendSequenceSorts.OutToInMainFace,
			OutToInMainFace
		},
		{
			BendSequenceSorts.LongToShortWithoutGaps,
			LongToShortWithoutGaps
		},
		{
			BendSequenceSorts.ShortToLongWithoutGaps,
			ShortToLongWithoutGaps
		},
		{
			BendSequenceSorts.CommonBendsFirst,
			CommonBendsFirst
		},
		{
			BendSequenceSorts.ParallelsFirstByX,
			ParallelsFirstX
		},
		{
			BendSequenceSorts.ParallelsLastByX,
			ParallelsLastX
		},
		{
			BendSequenceSorts.ParallelsFirstByY,
			ParallelsFirstY
		},
		{
			BendSequenceSorts.ParallelsLastByY,
			ParallelsLastY
		},
		{
			BendSequenceSorts.StartShortestBendAndFollowClosestBend,
			StartShortestBendAndFollowClosestBend
		},
		{
			BendSequenceSorts.BendAngleAscending,
			BendAngleAscending
		},
		{
			BendSequenceSorts.BendAngleDescending,
			BendAngleDescending
		}
	};

	private static IOrderedEnumerable<CombinedBendDescriptor> ShortToLongWithGaps(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => x.TotalLength);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> LongToShortWithGaps(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => x.TotalLength);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> ShortToLongWithoutGaps(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => x.TotalLengthWithoutGaps);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> LongToShortWithoutGaps(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => x.TotalLengthWithoutGaps);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> InToOutModel(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => x.GetDistanceToModelMiddleUnfoldModel());
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> OutToInModel(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => x.GetDistanceToModelMiddleUnfoldModel());
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> InToOutMainFace(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => x.BranchLength);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> OutToInMainFace(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => x.BranchLength);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> CommonBendsFirst(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => x.Count);
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> ParallelsFirstX(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => Math.Abs(x.GetBendingLineDirectionUnfoldModel().X));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> ParallelsLastX(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => Math.Abs(x.GetBendingLineDirectionUnfoldModel().X));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> ParallelsFirstY(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => Math.Abs(x.GetBendingLineDirectionUnfoldModel().Y));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> ParallelsLastY(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => Math.Abs(x.GetBendingLineDirectionUnfoldModel().Y));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> BendAngleAscending(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => Math.Abs(x.StopProductAngleAbs));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> BendAngleDescending(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenByDescending((CombinedBendDescriptor x) => Math.Abs(x.StopProductAngleAbs));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> DistanceBbCenterOutToIn(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		return bends.ThenBy((CombinedBendDescriptor x) => Math.Abs(x.GetBendingLineDirectionUnfoldModel().Y));
	}

	private static IOrderedEnumerable<CombinedBendDescriptor> StartShortestBendAndFollowClosestBend(IOrderedEnumerable<CombinedBendDescriptor> bends, int step, Dictionary<BendSequenceSorts, object> metaData)
	{
		object value;
		bool num = metaData.TryGetValue(BendSequenceSorts.StartShortestBendAndFollowClosestBend, out value);
		StartShortestBendAndFollowClosestBendData startShortestBendAndFollowClosestBendData = (value as StartShortestBendAndFollowClosestBendData) ?? new StartShortestBendAndFollowClosestBendData();
		if (!num)
		{
			metaData.Add(BendSequenceSorts.StartShortestBendAndFollowClosestBend, startShortestBendAndFollowClosestBendData);
		}
		if (step == 0)
		{
			CombinedBendDescriptor minLenBend = (from x in bends.ToList()
				orderby x.TotalLength
				select x).FirstOrDefault();
			startShortestBendAndFollowClosestBendData.LastBend = minLenBend;
			return bends.ThenBy((CombinedBendDescriptor x) => (x != minLenBend) ? 1 : 0);
		}
		global::WiCAM.Pn4000.BendModel.Base.Pair<Vector3d, Vector3d> lastPoints = startShortestBendAndFollowClosestBendData.LastBend.GetEndPointsInWorldCoordsUnfoldModel();
		CombinedBendDescriptor minDistBend = bends.ToList().OrderBy(delegate(CombinedBendDescriptor x)
		{
			global::WiCAM.Pn4000.BendModel.Base.Pair<Vector3d, Vector3d> endPointsInWorldCoordsUnfoldModel = x.GetEndPointsInWorldCoordsUnfoldModel();
			return Math.Min(Math.Min((lastPoints.Item1 - endPointsInWorldCoordsUnfoldModel.Item1).LengthSquared, (lastPoints.Item1 - endPointsInWorldCoordsUnfoldModel.Item2).LengthSquared), Math.Min((lastPoints.Item2 - endPointsInWorldCoordsUnfoldModel.Item1).LengthSquared, (lastPoints.Item2 - endPointsInWorldCoordsUnfoldModel.Item2).LengthSquared));
		}).FirstOrDefault();
		startShortestBendAndFollowClosestBendData.LastBend = minDistBend;
		return bends.ThenBy((CombinedBendDescriptor x) => (x != minDistBend) ? 1 : 0);
	}

	private static Vector3d GetMachineInsertVectorWorldSpace(CombinedBendDescriptor combinedBendDescriptor)
	{
		int digits = 4;
		FaceGroup unfoldFaceGroup = combinedBendDescriptor.Enumerable.First().BendParams.UnfoldFaceGroup;
		Model unfoldFaceGroupModel = combinedBendDescriptor.Enumerable.First().BendParams.UnfoldFaceGroupModel;
		Matrix4d worldMatrix = unfoldFaceGroup.GetAllFaces().First().Shell.GetWorldMatrix(unfoldFaceGroupModel);
		Vector3d vector3d = worldMatrix.TransformNormal(unfoldFaceGroup.BendMiddlePointNormal.Normalized);
		Vector3d vector3d2 = worldMatrix.TransformNormal(unfoldFaceGroup.ConcaveAxis.Direction);
		Vector3d vector3d3 = vector3d.Normalized.Cross(vector3d2.Normalized);
		if (combinedBendDescriptor.PositioningInfo.CalcIsReversedGeometry(combinedBendDescriptor.MachinePartInsertionDirection))
		{
			vector3d2 *= -1.0;
			vector3d3 *= -1.0;
		}
		vector3d2 = vector3d3;
		return new Vector3d(Math.Round(vector3d2.X, digits), Math.Round(vector3d2.Y, digits), Math.Round(vector3d2.Z, digits));
	}

	public static void SortBendsBruteforce(List<CombinedBendDescriptor> bendSequence, int bruteforceIndex)
	{
		List<int> fgIds = (from x in bendSequence.SelectMany((CombinedBendDescriptor x) => x.Enumerable.Select((IBendDescriptor b) => b.BendParams.EntryFaceGroup.ID))
			orderby x
			select x).ToList();
		fgIds = Algorithms.GeneratePermutations(fgIds).Skip(bruteforceIndex).First()
			.ToList();
		CombinedBendsSorter.SortBends(null, bendSequence, groupByParallels: false, null, (IOrderedEnumerable<CombinedBendDescriptor> cbds) => cbds.OrderBy((CombinedBendDescriptor cbd) => cbd.Enumerable.Min((IBendDescriptor b) => fgIds.IndexOf(b.BendParams.EntryFaceGroup.ID))));
	}

	public static void SortBends(List<BendSequenceSorts> bendPriorities, List<CombinedBendDescriptor> bendSequence, bool groupByParallels, List<BendSequenceSorts> bendPrioritiesInGroup)
	{
		CombinedBendsSorter.SortBends(bendPriorities, bendSequence, groupByParallels, bendPrioritiesInGroup, null);
	}

	private static void SortBends(List<BendSequenceSorts> bendPriorities, List<CombinedBendDescriptor> bendSequence, bool groupByParallels, List<BendSequenceSorts> bendPrioritiesInGroup, Func<IOrderedEnumerable<CombinedBendDescriptor>, IOrderedEnumerable<CombinedBendDescriptor>>? FuncOrderNextBendOverride)
	{
		foreach (CombinedBendDescriptor item in bendSequence)
		{
			item.UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		Dictionary<IBendDescriptor, List<CombinedBendDescriptor>> dictionary = new Dictionary<IBendDescriptor, List<CombinedBendDescriptor>>();
		if (groupByParallels)
		{
			if (bendPrioritiesInGroup.Count == 0)
			{
				bendPrioritiesInGroup = bendPriorities;
			}
			foreach (List<CombinedBendDescriptor> item2 in from g in bendSequence.GroupBy(GetMachineInsertVectorWorldSpace)
				select g.ToList())
			{
				foreach (IBendDescriptor item3 in item2.SelectMany((CombinedBendDescriptor x) => x.Enumerable).Distinct())
				{
					dictionary.Add(item3, item2);
				}
			}
		}
		List<IBendDescriptor> list = new List<IBendDescriptor>();
		List<CombinedBendDescriptor> newOrderedBends = new List<CombinedBendDescriptor>();
		List<CombinedBendDescriptor> list2 = bendSequence.ToList();
		Dictionary<BendSequenceSorts, object> metaData = new Dictionary<BendSequenceSorts, object>();
		while (list2.Count > 0)
		{
			foreach (CombinedBendDescriptor item4 in newOrderedBends)
			{
				item4.UnfoldBendInUnfoldModel(0.0, relative: true, noGeometryChange: true);
			}
			for (int i = 0; i < list2.Count; i++)
			{
				CombinedBendDescriptor combinedBendDescriptor = list2[i];
				if (combinedBendDescriptor.Enumerable.Count() > 1)
				{
					int j = 0;
					List<int> list3 = combinedBendDescriptor.Enumerable.Select((IBendDescriptor x) => j++).Skip(1).ToList();
					CombinedBendDescriptorHelper.TrySplitCombinedBend(combinedBendDescriptor, list3, list2);
					i += list3.Count;
				}
			}
			CombinedBendDescriptorHelper.CombinePossibleBendsInUnfoldModel(list2);
			foreach (CombinedBendDescriptor item5 in newOrderedBends)
			{
				item5.UnfoldBendInUnfoldModel(1.0, relative: true, noGeometryChange: true);
			}
			IOrderedEnumerable<CombinedBendDescriptor> orderedEnumerable = list2.OrderBy((CombinedBendDescriptor x) => 1);
			List<CombinedBendDescriptor> orderedBends = ((FuncOrderNextBendOverride != null) ? FuncOrderNextBendOverride(orderedEnumerable) : bendPriorities.Aggregate(orderedEnumerable, (IOrderedEnumerable<CombinedBendDescriptor> current, BendSequenceSorts order) => CombinedBendsSorter.SortActions[order](current, newOrderedBends.Count, metaData))).ThenByDescending((CombinedBendDescriptor x) => x.Enumerable.FirstOrDefault()?.BendParams.EntryFaceGroup.ID ?? (-1)).ToList();
			if (groupByParallels && (list.Count > 0 || dictionary[orderedBends.First().Enumerable.First()].Count() > 1))
			{
				if (list.Count == 0)
				{
					list = bendPrioritiesInGroup.Aggregate(dictionary[orderedBends.First().Enumerable.First()].OrderBy((CombinedBendDescriptor x) => 1), (IOrderedEnumerable<CombinedBendDescriptor> current, BendSequenceSorts order) => CombinedBendsSorter.SortActions[order](current, newOrderedBends.Count, metaData)).SelectMany((CombinedBendDescriptor x) => x.Enumerable).Distinct()
						.ToList();
				}
				List<CombinedBendDescriptor> primaryBends = (from dest in list
					select orderedBends.FirstOrDefault((CombinedBendDescriptor x) => x.Enumerable.Contains(dest)) into x
					where x != null
					select x).Distinct().ToList();
				orderedBends = Enumerable.Concat(second: orderedBends.Where((CombinedBendDescriptor x) => !primaryBends.Contains(x)), first: primaryBends).ToList();
			}
			CombinedBendDescriptor combinedBendDescriptor2 = null;
			for (int k = 0; k < orderedBends.Count; k++)
			{
				CombinedBendDescriptor combinedBendDescriptor3 = orderedBends[k];
				bool flag = true;
				for (int l = k + 1; l < orderedBends.Count; l++)
				{
					CombinedBendDescriptor next = orderedBends[l];
					List<CombinedBendDescriptor> splitPredecessors = combinedBendDescriptor3.SplitPredecessors;
					if (splitPredecessors != null && splitPredecessors.Any((CombinedBendDescriptor x) => x == next))
					{
						List<CombinedBendDescriptor> list4 = orderedBends;
						int index = l;
						List<CombinedBendDescriptor> list5 = orderedBends;
						int index2 = k;
						CombinedBendDescriptor value = orderedBends[k];
						CombinedBendDescriptor value2 = orderedBends[l];
						list4[index] = value;
						list5[index2] = value2;
						k--;
						flag = false;
						break;
					}
				}
				if (flag)
				{
					combinedBendDescriptor2 = combinedBendDescriptor3;
					break;
				}
			}
			list2 = orderedBends;
			newOrderedBends.Add(combinedBendDescriptor2);
			list2.Remove(combinedBendDescriptor2);
			foreach (IBendDescriptor item6 in combinedBendDescriptor2.Enumerable)
			{
				if (list.Contains(item6))
				{
					list.Remove(item6);
				}
			}
		}
		foreach (CombinedBendDescriptor item7 in newOrderedBends)
		{
			item7.UnfoldBendInUnfoldModel(1.0, relative: false, noGeometryChange: true);
		}
		bendSequence.Clear();
		bendSequence.AddRange(newOrderedBends);
		CombinedBendDescriptorHelper.SetOrder(bendSequence);
	}
}
