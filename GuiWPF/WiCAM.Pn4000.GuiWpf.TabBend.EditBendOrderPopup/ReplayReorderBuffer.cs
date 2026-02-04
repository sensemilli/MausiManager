using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class ReplayReorderBuffer
{
	public interface IReorderOperation
	{
		int LastCurrentBendProgress { get; set; }

		bool Do(IPnBndDoc doc);

		bool? GetLastReturnValue();

		bool TryUndo(IPnBndDoc doc);

		void ConfigureUndo(IPnBndDoc doc);
	}

	public class StrategyOperation : IReorderOperation
	{
		private readonly List<BendSequenceSorts> _sortList;

		private readonly bool _groupParallesInAutoSort;

		private readonly List<BendSequenceSorts> _sortListGroup;

		private IBendSequenceStrategyFactory _bendSequenceStrategyFactory;

		private List<List<Tuple<int, int, int>>> _oldFingerprint;

		public int LastCurrentBendProgress { get; set; }

		public StrategyOperation(List<BendSequenceSorts> sortList, bool groupParallesInAutoSort, List<BendSequenceSorts> sortListGroup, IBendSequenceStrategyFactory bendSequenceStrategyFactory)
		{
			_sortList = sortList.ToList();
			_groupParallesInAutoSort = groupParallesInAutoSort;
			_sortListGroup = sortListGroup.ToList();
			_bendSequenceStrategyFactory = bendSequenceStrategyFactory;
		}

		public bool Do(IPnBndDoc doc)
		{
			IBendSequenceOrder bendSequenceOrder = _bendSequenceStrategyFactory.CreateNewSequenceOrder("", enabled: true, Guid.NewGuid());
			bendSequenceOrder.Sequences.AddRange(_sortList.Select((BendSequenceSorts x) => x));
			if (_groupParallesInAutoSort)
			{
				bendSequenceOrder.Groupings.Add(_bendSequenceStrategyFactory.CreateGrouping("", 1));
				bendSequenceOrder.Groupings[0].InnerSortSequences.AddRange(_sortListGroup.Select((BendSequenceSorts x) => x));
			}
			doc.CombinedBendsAutoSort(bendSequenceOrder);
			return true;
		}

		public bool? GetLastReturnValue()
		{
			return true;
		}

		public bool TryUndo(IPnBndDoc doc)
		{
			return AtomicBendDescriptor.Undo(doc, _oldFingerprint);
		}

		public void ConfigureUndo(IPnBndDoc doc)
		{
			_oldFingerprint = AtomicBendDescriptor.DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		}
	}

	public class PermutationOperation : IReorderOperation
	{
		private List<List<Tuple<int, int, int>>> _oldFingerprint;

		private readonly bool _autoCombine;

		private readonly List<int> _permutation;

		private bool? _lastReturnValue;

		public int LastCurrentBendProgress { get; set; }

		public PermutationOperation(List<int> permutation, bool autoCombine)
		{
			_autoCombine = autoCombine;
			_permutation = permutation;
		}

		public bool Do(IPnBndDoc doc)
		{
			List<ICombinedBendDescriptorInternal> order = _permutation.Select((int i) => doc.CombinedBendDescriptors.ElementAt(i)).ToList();
			bool flag = doc.ApplyBendOrder(order, _autoCombine);
			_lastReturnValue = flag;
			return flag;
		}

		public bool? GetLastReturnValue()
		{
			return _lastReturnValue;
		}

		public bool TryUndo(IPnBndDoc doc)
		{
			return AtomicBendDescriptor.Undo(doc, _oldFingerprint);
		}

		public void ConfigureUndo(IPnBndDoc doc)
		{
			_oldFingerprint = AtomicBendDescriptor.DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		}
	}

	public class FlipOperation : IReorderOperation
	{
		private readonly int _oldIndex;

		private readonly int _newIndex;

		private readonly bool _autoCombine;

		private bool? _lastReturnValue;

		private List<List<Tuple<int, int, int>>> _oldFingerprint;

		public int LastCurrentBendProgress { get; set; }

		public FlipOperation(int oldIndex, int newIndex, bool autoCombine)
		{
			_oldIndex = oldIndex;
			_newIndex = newIndex;
			_autoCombine = autoCombine;
		}

		public bool Do(IPnBndDoc doc)
		{
			List<ICombinedBendDescriptorInternal> list = doc.CombinedBendDescriptors.ToList();
			ICombinedBendDescriptorInternal item = list[_oldIndex];
			list.RemoveAt(_oldIndex);
			list.Insert(_newIndex, item);
			bool flag = false;
			flag = ((!_autoCombine) ? doc.ApplyBendOrder(list, _autoCombine) : doc.ApplyBendOrder(list));
			_lastReturnValue = flag;
			return flag;
		}

		public bool? GetLastReturnValue()
		{
			return _lastReturnValue;
		}

		public bool TryUndo(IPnBndDoc doc)
		{
			return AtomicBendDescriptor.Undo(doc, _oldFingerprint);
		}

		public void ConfigureUndo(IPnBndDoc doc)
		{
			_oldFingerprint = AtomicBendDescriptor.DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		}
	}

	public class SplitOperation : IReorderOperation
	{
		private readonly int _cbdIndex;

		private readonly List<int> _splitIdx;

		private bool? _lastReturnValue;

		private List<List<Tuple<int, int, int>>> _oldFingerprint;

		public int LastCurrentBendProgress { get; set; }

		public SplitOperation(int cbdIndex, List<int> splitIdx)
		{
			_cbdIndex = cbdIndex;
			_splitIdx = splitIdx;
		}

		public bool Do(IPnBndDoc doc)
		{
			bool flag = doc.SplitCombinedBends(_cbdIndex, _splitIdx, update: true);
			_lastReturnValue = flag;
			return flag;
		}

		public bool? GetLastReturnValue()
		{
			return _lastReturnValue;
		}

		public bool TryUndo(IPnBndDoc doc)
		{
			return AtomicBendDescriptor.Undo(doc, _oldFingerprint);
		}

		public void ConfigureUndo(IPnBndDoc doc)
		{
			_oldFingerprint = AtomicBendDescriptor.DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		}
	}

	public class MergeOperation : IReorderOperation
	{
		private readonly int _index;

		private bool? _lastReturnValue;

		private List<List<Tuple<int, int, int>>> _oldFingerprint;

		public int LastCurrentBendProgress { get; set; }

		public MergeOperation(int index)
		{
			_index = index;
		}

		public bool Do(IPnBndDoc doc)
		{
			bool flag = doc.TryMergeWithNext(doc.CombinedBendDescriptors[_index]);
			_lastReturnValue = flag;
			return flag;
		}

		public bool? GetLastReturnValue()
		{
			return _lastReturnValue;
		}

		public bool TryUndo(IPnBndDoc doc)
		{
			return AtomicBendDescriptor.Undo(doc, _oldFingerprint);
		}

		public void ConfigureUndo(IPnBndDoc doc)
		{
			_oldFingerprint = AtomicBendDescriptor.DeriveAllKeys(doc.CombinedBendDescriptors).ToList();
		}
	}

	private List<IReorderOperation> Operations { get; set; } = new List<IReorderOperation>();

	public void DoAll(IPnBndDoc doc)
	{
		foreach (IReorderOperation operation in Operations)
		{
			operation.GetLastReturnValue();
			operation.Do(doc);
		}
	}

	public bool DoLast(IPnBndDoc doc)
	{
		return Operations.LastOrDefault()?.Do(doc) ?? false;
	}

	public bool AddAndDoLast(IReorderOperation op, IPnBndDoc doc, int currentBentProgress)
	{
		op.LastCurrentBendProgress = currentBentProgress;
		op.ConfigureUndo(doc);
		Operations.Add(op);
		bool num = DoLast(doc);
		if (!num)
		{
			Operations.Remove(op);
		}
		return num;
	}

	public void Clear()
	{
		Operations.Clear();
	}

	public bool Undo(IPnBndDoc doc, out int currentBentProgress)
	{
		if (!Operations.Any())
		{
			currentBentProgress = -1;
			return false;
		}
		IReorderOperation reorderOperation = Operations.Last();
		bool result = reorderOperation.TryUndo(doc);
		currentBentProgress = reorderOperation.LastCurrentBendProgress;
		Operations.Remove(reorderOperation);
		return result;
	}
}
