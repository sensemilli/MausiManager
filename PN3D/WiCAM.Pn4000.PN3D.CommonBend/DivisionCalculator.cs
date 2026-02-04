using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools;

namespace WiCAM.Pn4000.PN3D.CommonBend;

public class DivisionCalculator
{
	private static List<ToolPartBase> FillGreedy(List<ToolPartBase> availableParts, double totalLength, bool checkRest)
	{
		List<ToolPartBase> list = new List<ToolPartBase>();
		int num = 0;
		bool flag;
		do
		{
			flag = false;
			foreach (ToolPartBase availablePart in availableParts)
			{
				if (checkRest)
				{
					if (availablePart.Rest > 0 && totalLength - (double)num >= (double)(int)availablePart.Length)
					{
						list.Add(availablePart);
						num += (int)availablePart.Length;
						availablePart.Rest--;
						flag = true;
						break;
					}
				}
				else if (totalLength - (double)num >= (double)(int)availablePart.Length)
				{
					list.Add(availablePart);
					num += (int)availablePart.Length;
					flag = true;
					break;
				}
			}
		}
		while (flag);
		return list;
	}

	public static List<List<ToolPartBase>> GetAllToolLengthCombinations(int toolCount, List<ToolPartBase> availableParts, Func<List<ToolPartBase>, bool> predicate)
	{
		List<List<ToolPartBase>> list = new List<List<ToolPartBase>>();
		List<int> list2 = new List<int>(toolCount);
		for (int i = 0; i < toolCount; i++)
		{
			list2.Add(0);
		}
		for (int j = 0; (double)j < Math.Pow(availableParts.Count, toolCount); j++)
		{
			List<ToolPartBase> list3 = new List<ToolPartBase>(toolCount);
			for (int k = 0; k < toolCount; k++)
			{
				list3.Add(availableParts[list2[k]]);
			}
			if (predicate(list3))
			{
				list.Add(list3);
			}
			int num = list2.FindIndex((int x) => x < availableParts.Count - 1);
			if (num == -1)
			{
				continue;
			}
			list2[num]++;
			if (num > 0)
			{
				for (int l = 0; l < num; l++)
				{
					list2[l] = 0;
				}
			}
		}
		return list;
	}

	public static List<ToolPartBase> FindMinExtendingReplacement(List<ToolPartBase> availableParts, int toolLength, bool checkRest)
	{
		ToolPartBase toolPartBase = null;
		toolPartBase = ((!checkRest) ? availableParts.LastOrDefault() : availableParts.LastOrDefault((ToolPartBase p) => p.Rest > 0));
		if (toolPartBase == null)
		{
			return new List<ToolPartBase>();
		}
		Func<List<ToolPartBase>, bool> func = delegate(List<ToolPartBase> x)
		{
			bool flag = true;
			if ((from i in x
				group i by i).Any((IGrouping<ToolPartBase, ToolPartBase> grp) => grp.Count() > grp.Key.Amount || ((grp.Key.Rest < 1 || grp.Count() > grp.Key.Rest) && checkRest)))
			{
				flag = false;
			}
			return flag && (int)x.Select((ToolPartBase p) => p.Length).Sum() > toolLength;
		};
		if (toolLength == (int)toolPartBase.Length)
		{
			List<ToolPartBase> list = new List<ToolPartBase>();
			if (availableParts.Count == 1)
			{
				list = new List<ToolPartBase> { toolPartBase, toolPartBase };
			}
			else if ((int)toolPartBase.Length * 2 < (int)availableParts[availableParts.Count - 2].Length)
			{
				list = new List<ToolPartBase> { toolPartBase, toolPartBase };
			}
			else if (availableParts.FindLastIndex((ToolPartBase x) => (int)x.Length > toolLength) != -1)
			{
				ToolPartBase item = availableParts.FindLast((ToolPartBase x) => (int)x.Length > toolLength);
				list = new List<ToolPartBase> { item };
			}
			else
			{
				list = new List<ToolPartBase> { toolPartBase, toolPartBase };
			}
			if (func(list))
			{
				return list;
			}
		}
		List<List<ToolPartBase>> allToolLengthCombinations = DivisionCalculator.GetAllToolLengthCombinations(1, availableParts, func);
		allToolLengthCombinations.AddRange(DivisionCalculator.GetAllToolLengthCombinations(2, availableParts, func));
		allToolLengthCombinations.AddRange(DivisionCalculator.GetAllToolLengthCombinations(3, availableParts, func));
		allToolLengthCombinations.AddRange(DivisionCalculator.GetAllToolLengthCombinations(4, availableParts, func));
		if (allToolLengthCombinations.Count < 1)
		{
			return new List<ToolPartBase>();
		}
		int minLength = allToolLengthCombinations.Min((List<ToolPartBase> Y) => (int)Y.Select((ToolPartBase p) => p.Length).Sum());
		IEnumerable<List<ToolPartBase>> source = allToolLengthCombinations.Where((List<ToolPartBase> X) => (int)X.Select((ToolPartBase p) => p.Length).Sum() == minLength);
		if (source.Count() < 1)
		{
			return new List<ToolPartBase>();
		}
		int minToolCount = source.Min((List<ToolPartBase> Y) => Y.Count);
		return source.Where((List<ToolPartBase> X) => X.Count == minToolCount).First();
	}

	private static Dictionary<List<ToolPartBase>, List<ToolPartBase>> CalcPossibleRecombinations(List<ToolPartBase> availableParts, ToolPartBase toolPart, bool useToolAmount)
	{
		Dictionary<List<ToolPartBase>, List<ToolPartBase>> dictionary = new Dictionary<List<ToolPartBase>, List<ToolPartBase>>();
		if (availableParts.Count == 0)
		{
			return dictionary;
		}
		List<ToolPartBase> list = (from x in availableParts
			where x.Length < toolPart.Length
			orderby x.Length descending
			select x).ToList();
		List<int> list2 = list.Select((ToolPartBase x) => 0).ToList();
		double num = toolPart.Length + 1E-06;
		double num2 = toolPart.Length - 1E-06;
		double num3 = 0.0;
		bool flag;
		do
		{
			if (num3 >= num2)
			{
				List<ToolPartBase> list3 = new List<ToolPartBase>(list2.Sum());
				for (int i = 0; i < list2.Count; i++)
				{
					for (int j = 0; j < list2[i]; j++)
					{
						list3.Add(list[i]);
					}
				}
				List<ToolPartBase> list4 = new List<ToolPartBase>(1);
				list4.Add(toolPart);
				dictionary.Add(list3, list4);
			}
			flag = false;
			for (int k = 0; k < list2.Count; k++)
			{
				if (num3 + list[k].Length > num || (useToolAmount && list2[k] + 1 > list[k].Amount))
				{
					num3 -= list[k].Length * (double)list2[k];
					list2[k] = 0;
					continue;
				}
				list2[k]++;
				num3 += list[k].Length;
				flag = true;
				break;
			}
		}
		while (flag);
		return dictionary;
	}

	private static Dictionary<List<ToolPartBase>, List<ToolPartBase>> CalcPossibleRecombinations(IEnumerable<ToolPartBase> availableParts, double bendLength, bool useToolAmount)
	{
		Dictionary<List<ToolPartBase>, List<ToolPartBase>> dictionary = new Dictionary<List<ToolPartBase>, List<ToolPartBase>>();
		using IEnumerator<ToolPartBase> enumerator = availableParts.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Dictionary<List<ToolPartBase>, List<ToolPartBase>> dictionary2 = DivisionCalculator.CalcPossibleRecombinations(toolPart: enumerator.Current, availableParts: availableParts.ToList(), useToolAmount: useToolAmount);
			if (dictionary2.Count <= 0)
			{
				continue;
			}
			foreach (KeyValuePair<List<ToolPartBase>, List<ToolPartBase>> item in dictionary2)
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		return dictionary;
	}

	private static bool ContainsList(List<ToolPartBase> bigList, List<ToolPartBase> subList)
	{
		List<ToolPartBase> range = bigList.GetRange(0, bigList.Count());
		int i;
		for (i = 0; i < subList.Count; i++)
		{
			if (range.Any((ToolPartBase p) => (int)p.Length == (int)subList[i].Length))
			{
				range.RemoveAt(range.IndexOf(range.First((ToolPartBase p) => (int)p.Length == (int)subList[i].Length)));
				continue;
			}
			return false;
		}
		return true;
	}

	private static List<ToolPartBase> RemoveList(List<ToolPartBase> bigList, List<ToolPartBase> subList)
	{
		foreach (ToolPartBase i in subList)
		{
			ToolPartBase toolPartBase = bigList.FirstOrDefault((ToolPartBase p) => (int)p.Length == (int)i.Length);
			if (toolPartBase != null)
			{
				bigList.RemoveAt(bigList.IndexOf(toolPartBase));
				if (toolPartBase.Rest < toolPartBase.Amount)
				{
					toolPartBase.Rest++;
				}
			}
		}
		return bigList;
	}

	public static List<ToolPartBase> RecombineTools(List<ToolPartBase> division, List<ToolPartBase> availableParts, double bendLength, bool useToolAmount)
	{
		List<ToolPartBase> list = division.OrderByDescending((ToolPartBase X) => X.Length).ToList();
		List<KeyValuePair<List<ToolPartBase>, List<ToolPartBase>>> list2 = (from x in DivisionCalculator.CalcPossibleRecombinations(availableParts, bendLength, useToolAmount)
			orderby x.Key.Count descending
			select x).ToList();
		bool flag = false;
		do
		{
			flag = false;
			foreach (KeyValuePair<List<ToolPartBase>, List<ToolPartBase>> item in list2)
			{
				if (DivisionCalculator.ContainsList(list, item.Key))
				{
					list = DivisionCalculator.RemoveList(list, item.Key);
					if (item.Value.All((ToolPartBase i) => i.Rest > 0))
					{
						list.AddRange(item.Value);
						DivisionCalculator.ReduceRest(item.Value);
						flag = true;
					}
					else
					{
						list.AddRange(item.Key);
						DivisionCalculator.ReduceRest(item.Key);
					}
				}
			}
		}
		while (flag);
		return list;
	}

	private static void ReduceRest(List<ToolPartBase> parts)
	{
		foreach (ToolPartBase part in parts)
		{
			if (part.Rest > 0)
			{
				part.Rest--;
			}
		}
	}

	private static void ExecuteMinExtendingReplacements(List<ToolPartBase> availableParts, ref List<ToolPartBase> division, double bendLength, double maxOverlength, bool checkRest)
	{
		int num = 0;
		bool flag;
		int num5;
		do
		{
			flag = false;
			int index = -1;
			int num2 = int.MaxValue;
			for (int i = 0; i < division.Count; i++)
			{
				int num3 = (int)division[i].Length;
				int num4 = (int)(from p in DivisionCalculator.FindMinExtendingReplacement(availableParts, num3, checkRest)
					select p.Length).Sum();
				if (num4 - num3 < num2)
				{
					index = i;
					num2 = num4 - num3;
				}
			}
			division.Select((ToolPartBase p) => p.Length).Sum();
			if ((double)(num2 + (int)division.Select((ToolPartBase p) => p.Length).Sum()) <= bendLength + maxOverlength)
			{
				ToolPartBase toolPartBase = division[index];
				division.RemoveAt(index);
				if (checkRest && toolPartBase.Rest < toolPartBase.Amount)
				{
					toolPartBase.Rest++;
				}
				List<ToolPartBase> list = DivisionCalculator.FindMinExtendingReplacement(availableParts, (int)toolPartBase.Length, checkRest);
				division.AddRange(list);
				foreach (ToolPartBase item in list)
				{
					if (checkRest && item.Rest > 0)
					{
						item.Rest--;
					}
				}
				flag = true;
			}
			num5 = (int)division.Select((ToolPartBase p) => p.Length).Sum();
			if (num5 > num)
			{
				num = num5;
			}
		}
		while (!((double)num5 > bendLength) && num5 >= num && flag);
	}

	public static List<ToolPartBase> CalcDivision(List<ToolPartBase> availableParts, double bendLength, double maxOverlength)
	{
		bool flag = true;
		ToolPartBase toolPartBase = availableParts.Where((ToolPartBase p) => p.Rest > 0).LastOrDefault();
		if (toolPartBase == null)
		{
			return new List<ToolPartBase>();
		}
		if (bendLength + maxOverlength < (double)(int)toolPartBase.Length)
		{
			return new List<ToolPartBase>();
		}
		List<ToolPartBase> division = DivisionCalculator.FillGreedy(availableParts, bendLength, flag);
		if (toolPartBase.Rest > 0 && division.Count == 0 && (double)(int)toolPartBase.Length < bendLength + maxOverlength)
		{
			division.Add(toolPartBase);
			if (toolPartBase.Rest > 0)
			{
				toolPartBase.Rest--;
			}
		}
		if (division.Select((ToolPartBase p) => p.Length).Sum() >= bendLength + 0.001)
		{
			return division;
		}
		if (availableParts.Any((ToolPartBase x) => x.Rest > 0) || !flag)
		{
			DivisionCalculator.ExecuteMinExtendingReplacements(availableParts, ref division, bendLength, maxOverlength, flag);
		}
		return DivisionCalculator.RecombineTools(division, availableParts, bendLength, flag);
	}
}
