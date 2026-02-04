using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class AtomicBendDescriptor
{
	public enum SetRelation
	{
		Same,
		Subset,
		Superset,
		Disjoint,
		Neither
	}

	public static Tuple<int, int, int> DeriveKey(IReadOnlyList<ICombinedBendDescriptorInternal> cbds, ICombinedBendDescriptor combinedBendDescriptor, IBendDescriptor bd)
	{
		int item = cbds.Take(cbds.IndexOf(combinedBendDescriptor)).SelectMany((ICombinedBendDescriptorInternal x) => x.Enumerable).Count((IBendDescriptor x) => x == bd);
		return new Tuple<int, int, int>(bd.BendParams.BendFaceGroup.BendEntryId, bd.BendParams.BendFaceGroup.ID, item);
	}

	public static IEnumerable<Tuple<int, int, int>> DeriveKeys(IReadOnlyList<ICombinedBendDescriptorInternal> cbds, int index)
	{
		return cbds[index].Enumerable.Select((IBendDescriptor x) => DeriveKey(cbds, cbds[index], x));
	}

	public static IEnumerable<List<Tuple<int, int, int>>> DeriveAllKeys(IReadOnlyList<ICombinedBendDescriptorInternal> cbds)
	{
		return cbds.Select((ICombinedBendDescriptorInternal x) => DeriveKeys(cbds, cbds.IndexOf<ICombinedBendDescriptorInternal>(x)).ToList());
	}

	public static bool AllKeysEqual(List<List<Tuple<int, int, int>>> a, List<List<Tuple<int, int, int>>> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (!KeysEqual(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool KeysEqual(List<Tuple<int, int, int>> a, List<Tuple<int, int, int>> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (!object.Equals(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static SetRelation CompareKeys(List<Tuple<int, int, int>> a, List<Tuple<int, int, int>> b)
	{
		if (KeysEqual(a, b))
		{
			return SetRelation.Same;
		}
		if (a.ToHashSet().IsSubsetOf(b.ToHashSet()))
		{
			return SetRelation.Subset;
		}
		if (b.ToHashSet().IsSubsetOf(a.ToHashSet()))
		{
			return SetRelation.Superset;
		}
		if (!a.ToHashSet().Intersect(b.ToHashSet()).Any())
		{
			return SetRelation.Disjoint;
		}
		return SetRelation.Neither;
	}

	public static bool Undo(IPnBndDoc doc, List<List<Tuple<int, int, int>>> oldFingerprint)
	{
		List<List<Tuple<int, int, int>>> source = DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		doc.SplitCombinedBends();
		List<Tuple<int, int, int>> source2 = oldFingerprint.SelectMany((List<Tuple<int, int, int>> x) => x).ToList();
		List<Tuple<int, int, int>> source3 = source.SelectMany((List<Tuple<int, int, int>> x) => x).ToList();
		Dictionary<Tuple<int, int, int>, int> indexMap = source3.Select((Tuple<int, int, int> value, int index) => new { value, index }).ToDictionary(x => x.value, x => x.index);
		List<ICombinedBendDescriptorInternal> order = (from i in source2.Select((Tuple<int, int, int> value) => indexMap[value]).ToList()
			select doc.CombinedBendDescriptors[i]).ToList();
		doc.ApplyBendOrder(order, autoCombine: false);
		for (int j = 0; j < oldFingerprint.Count; j++)
		{
			for (int k = 0; k < oldFingerprint[j].Count - 1; k++)
			{
				doc.TryMergeWithNext(doc.CombinedBendDescriptors[j]);
			}
		}
		DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		return true;
	}
}
