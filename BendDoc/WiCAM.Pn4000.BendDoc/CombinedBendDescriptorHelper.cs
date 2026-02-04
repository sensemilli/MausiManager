using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.BendDoc;

internal static class CombinedBendDescriptorHelper
{
	public static void SetOrder(IReadOnlyList<CombinedBendDescriptor> commonBendSequence)
	{
		for (int i = 0; i < commonBendSequence.Count; i++)
		{
			commonBendSequence[i].Order = i;
		}
	}

	public static void TrySplitCombinedBend(CombinedBendDescriptor cbd, List<int> splitGap, List<CombinedBendDescriptor> commonBends)
	{
		if (cbd.SplitAtGapIndicesUnfoldModel(splitGap, out List<CombinedBendDescriptor> result))
		{
			int index = commonBends.IndexOf(cbd);
			commonBends.RemoveAt(index);
			commonBends.InsertRange(index, result);
		}
	}

	public static bool CombinePossibleBendsInUnfoldModel(List<CombinedBendDescriptor> bends)
	{
		bool result = false;
		for (int i = 0; i < bends.Count; i++)
		{
			CombinedBendDescriptor combinedBendDescriptor = bends[i];
			for (int j = i + 1; j < bends.Count; j++)
			{
				CombinedBendDescriptor combinedBendDescriptor2 = bends[j];
				if (combinedBendDescriptor.IsCompatibleBendUnfoldModel(combinedBendDescriptor2))
				{
					combinedBendDescriptor.Merge(combinedBendDescriptor2);
					bends.Remove(combinedBendDescriptor2);
					j--;
					result = true;
				}
			}
		}
		return result;
	}

	public static List<List<ICombinedBendDescriptorInternal>> ComputePotentialBendGroups(List<ICombinedBendDescriptorInternal> bends)
	{
		int count = bends.Count;
		int[] parent = new int[count];
		for (int i = 0; i < count; i++)
		{
			parent[i] = i;
		}
		for (int j = 0; j < count; j++)
		{
			for (int k = j + 1; k < count; k++)
			{
				if (bends[j].IsCompatibleBendUnfoldModel(bends[k]))
				{
					Union(j, k);
				}
			}
		}
		Dictionary<int, List<ICombinedBendDescriptorInternal>> dictionary = new Dictionary<int, List<ICombinedBendDescriptorInternal>>();
		for (int l = 0; l < count; l++)
		{
			int key = Find(l);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = new List<ICombinedBendDescriptorInternal>();
			}
			dictionary[key].Add(bends[l]);
		}
		return dictionary.Values.ToList();
		int Find(int x)
		{
			if (parent[x] != x)
			{
				parent[x] = Find(parent[x]);
			}
			return parent[x];
		}
		void Union(int x, int y)
		{
			int num = Find(x);
			int num2 = Find(y);
			if (num != num2)
			{
				parent[num2] = num;
			}
		}
	}
}
