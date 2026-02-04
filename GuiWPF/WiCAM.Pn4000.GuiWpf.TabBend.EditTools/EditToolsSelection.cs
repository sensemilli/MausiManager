using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsSelection : IEditToolsSelection
{
	[CompilerGenerated]
	private sealed class _003CPiecesToSections_003Ed__68 : IEnumerable<IToolSection>, IEnumerable, IEnumerator<IToolSection>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IToolSection _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<IToolPiece> pieces;

		public IEnumerable<IToolPiece> _003C_003E3__pieces;

		private HashSet<IToolSection> _003Csections_003E5__2;

		private IEnumerator<IToolPiece> _003C_003E7__wrap2;

		IToolSection IEnumerator<IToolSection>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CPiecesToSections_003Ed__68(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Csections_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Csections_003E5__2 = new HashSet<IToolSection>();
					_003C_003E7__wrap2 = pieces.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					IToolPiece current = _003C_003E7__wrap2.Current;
					if (_003Csections_003E5__2.Add(current.ToolSection))
					{
						_003C_003E2__current = current.ToolSection;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<IToolSection> IEnumerable<IToolSection>.GetEnumerator()
		{
			_003CPiecesToSections_003Ed__68 _003CPiecesToSections_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CPiecesToSections_003Ed__ = this;
			}
			else
			{
				_003CPiecesToSections_003Ed__ = new _003CPiecesToSections_003Ed__68(0);
			}
			_003CPiecesToSections_003Ed__.pieces = _003C_003E3__pieces;
			return _003CPiecesToSections_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IToolSection>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CSectionsToCluster_003Ed__69 : IEnumerable<IToolCluster>, IEnumerable, IEnumerator<IToolCluster>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IToolCluster _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<IToolSection> sections;

		public IEnumerable<IToolSection> _003C_003E3__sections;

		private HashSet<IToolCluster> _003Ccluster_003E5__2;

		private IEnumerator<IToolSection> _003C_003E7__wrap2;

		IToolCluster IEnumerator<IToolCluster>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CSectionsToCluster_003Ed__69(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Ccluster_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Ccluster_003E5__2 = new HashSet<IToolCluster>();
					_003C_003E7__wrap2 = sections.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					IToolSection current = _003C_003E7__wrap2.Current;
					if (_003Ccluster_003E5__2.Add(current.Cluster))
					{
						_003C_003E2__current = current.Cluster;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<IToolCluster> IEnumerable<IToolCluster>.GetEnumerator()
		{
			_003CSectionsToCluster_003Ed__69 _003CSectionsToCluster_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CSectionsToCluster_003Ed__ = this;
			}
			else
			{
				_003CSectionsToCluster_003Ed__ = new _003CSectionsToCluster_003Ed__69(0);
			}
			_003CSectionsToCluster_003Ed__.sections = _003C_003E3__sections;
			return _003CSectionsToCluster_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IToolCluster>)this).GetEnumerator();
		}
	}

	private IToolSetups? _currentSetup;

	private EditToolSelectionModes _selectionMode = EditToolSelectionModes.ToolSection;

	private HashSet<IToolPiece> _pieces = new HashSet<IToolPiece>();

	private HashSet<IToolSection> _sections = new HashSet<IToolSection>();

	private HashSet<IToolCluster> _cluster = new HashSet<IToolCluster>();

	public IToolsAndBends ToolsAndBends { get; set; }

	public IToolSetups? CurrentSetups
	{
		get
		{
			return _currentSetup;
		}
		set
		{
			if (_currentSetup != value)
			{
				_currentSetup = value;
				UnselectAll(raiseEvent: false);
				RefreshData();
			}
		}
	}

	public IBendMachine? CurrentMachine { get; set; }

	public HashSet<IToolPiece> BadTools { get; } = new HashSet<IToolPiece>();

	public IPnBndDoc CurrentDoc { get; set; }

	public EditToolSelectionModes SelectionMode
	{
		get
		{
			return _selectionMode;
		}
		set
		{
			if (_selectionMode == value)
			{
				return;
			}
			EditToolSelectionModes selectionMode = _selectionMode;
			_selectionMode = value;
			if (selectionMode < _selectionMode)
			{
				IEnumerable<IToolSection> enumerable;
				if (selectionMode != 0)
				{
					IEnumerable<IToolSection> sections = _sections;
					enumerable = sections;
				}
				else
				{
					enumerable = PiecesToSections(_pieces);
				}
				IEnumerable<IToolSection> enumerable2 = enumerable;
				if (_selectionMode == EditToolSelectionModes.ToolSection)
				{
					_sections = enumerable2.ToHashSet();
				}
				else
				{
					_cluster = SectionsToCluster(enumerable2).ToHashSet();
				}
			}
			else
			{
				IEnumerable<IToolSection> enumerable3;
				if (selectionMode != EditToolSelectionModes.Cluster)
				{
					IEnumerable<IToolSection> sections = _sections;
					enumerable3 = sections;
				}
				else
				{
					enumerable3 = ClusterToSections(_cluster);
				}
				IEnumerable<IToolSection> enumerable4 = enumerable3;
				if (_selectionMode == EditToolSelectionModes.ToolSection)
				{
					_sections = enumerable4.ToHashSet();
				}
				else
				{
					_pieces = SectionsToPieces(enumerable4).ToHashSet();
				}
			}
			this.SelectionChanged?.Invoke();
		}
	}

	public IEnumerable<IToolPiece> AllPiecesInSetups => CurrentSetups?.AllSections.SelectMany((IToolSection s) => s.Pieces) ?? new IToolPiece[0];

	public IEnumerable<IToolPiece> SelectionAsPieces => SelectionMode switch
	{
		EditToolSelectionModes.ToolPiece => _pieces, 
		EditToolSelectionModes.ToolSection => _sections.SelectMany((IToolSection s) => s.Pieces), 
		EditToolSelectionModes.Cluster => _cluster.SelectAndManyRecursive((IToolCluster x) => x.Children).Distinct().SelectMany((IToolCluster c) => c.Sections)
			.SelectMany((IToolSection s) => s.Pieces), 
		_ => throw new InvalidEnumArgumentException(), 
	};

	public IEnumerable<IToolPiece> SelectedPieces => _pieces;

	public IEnumerable<IToolSection> SelectedSections => _sections;

	public IEnumerable<IToolCluster> SelectedClusters => _cluster;

	public Dictionary<Model, IToolPiece> ModelToPieces { get; } = new Dictionary<Model, IToolPiece>();

	public Dictionary<IToolPiece, Model> PiecesToModel { get; } = new Dictionary<IToolPiece, Model>();

	public event Action? SelectionChanged;

	public event Action? DataRefreshed;

	public event Action? NewSetups;

	public EditToolsSelection(IPnBndDoc doc)
	{
		CurrentDoc = doc;
		CurrentDoc.ToolsAndBendsChanged += CurrentDoc_ToolsAndBendsChanged;
	}

	private void CurrentDoc_ToolsAndBendsChanged(IToolsAndBends tabOld, IToolsAndBends tabNew)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			UnselectAll();
			ToolsAndBends = tabNew;
			CurrentSetups = tabNew?.BendPositions?.FirstOrDefault()?.Anchor?.Root ?? tabNew?.ToolSetups.FirstOrDefault();
			RefreshData();
			this.NewSetups?.Invoke();
		});
	}

	public void RefreshData()
	{
		BadTools.Clear();
		if (CurrentSetups != null && CurrentMachine?.ToolConfig != null)
		{
			IBendMachineTools toolConfig = CurrentMachine.ToolConfig;
			double upperBeamXStart = toolConfig.UpperBeamXStart;
			double upperBeamXEnd = toolConfig.UpperBeamXEnd;
			double lowerBeamXStart = toolConfig.LowerBeamXStart;
			double lowerBeamXEnd = toolConfig.LowerBeamXEnd;
			HashSet<IToolPiece> badTools = BadTools;
			foreach (IToolPiece item in CurrentSetups.AllSections.Where((IToolSection x) => !x.MultiToolProfile.ToolProfiles.All((IToolProfile y) => y is IDieFoldExtentionProfile)).SelectMany((IToolSection x) => x.Pieces))
			{
				bool isUpperSection = item.ToolSection.IsUpperSection;
				double num = item.OffsetWorld.X + 1E-06;
				double num2 = num + item.Length - 2E-06;
				if ((isUpperSection && (num < upperBeamXStart || num2 > upperBeamXEnd)) || (!isUpperSection && (num < lowerBeamXStart || num2 > lowerBeamXEnd)))
				{
					badTools.Add(item);
					continue;
				}
				double? num3 = item.ToolSection.ZMin + 1E-06;
				double? num4 = item.ToolSection.ZMax - 1E-06;
				foreach (IToolSection item2 in CurrentSetups.AllSections.Where((IToolSection x) => !x.MultiToolProfile.ToolProfiles.All((IToolProfile y) => y is IDieFoldExtentionProfile)))
				{
					if (item2.IsUpperSection == isUpperSection && item2 != item.ToolSection)
					{
						double? zMin = item2.ZMin;
						double? zMax = item2.ZMax;
						double? xMin = item2.XMin;
						if (xMin < num2 && xMin + item2.Length > num && num4 > zMin && num3 < zMax)
						{
							badTools.Add(item);
						}
					}
				}
			}
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			this.DataRefreshed?.Invoke();
		});
	}

	public void SetData(IPnBndDoc doc, IToolsAndBends toolsAndBends, IToolSetups? toolSetups)
	{
		UnselectAll();
		CurrentDoc = doc;
		ToolsAndBends = toolsAndBends;
		CurrentSetups = toolSetups;
		RefreshData();
		this.NewSetups?.Invoke();
	}

	public void SetDataToolSetups(IToolSetups? toolSetups)
	{
		if (toolSetups != CurrentSetups)
		{
			UnselectAll();
			IToolsAndBends toolsAndBends = ToolsAndBends;
			if (toolsAndBends != null && toolsAndBends.ToolSetups.Contains(toolSetups))
			{
				CurrentSetups = toolSetups;
			}
			RefreshData();
		}
	}

	public void SetSelection(IToolPiece piece, bool isSelected)
	{
		SetSelection(piece.ToIEnumerable(), isSelected);
	}

	public void SetSelection(IEnumerable<IToolPiece> pieces, bool isSelected)
	{
		if (isSelected)
		{
			foreach (IToolPiece piece in pieces)
			{
				_pieces.Add(piece);
			}
		}
		else
		{
			foreach (IToolPiece piece2 in pieces)
			{
				_pieces.Remove(piece2);
			}
		}
		this.SelectionChanged?.Invoke();
	}

	public void SetSelection(IToolSection section, bool isSelected)
	{
		SetSelection(section.ToIEnumerable(), isSelected);
	}

	public void SetSelection(IEnumerable<IToolSection> sections, bool isSelected)
	{
		if (isSelected)
		{
			foreach (IToolSection section in sections)
			{
				_sections.Add(section);
			}
		}
		else
		{
			foreach (IToolSection section2 in sections)
			{
				_sections.Remove(section2);
			}
		}
		this.SelectionChanged?.Invoke();
	}

	public void SetSelection(IToolCluster cluster, bool isSelected)
	{
		SetSelection(cluster.ToIEnumerable(), isSelected);
	}

	public void SetSelection(IEnumerable<IToolCluster> clusters, bool isSelected)
	{
		if (isSelected)
		{
			foreach (IToolCluster cluster in clusters)
			{
				_cluster.Add(cluster);
			}
		}
		else
		{
			foreach (IToolCluster cluster2 in clusters)
			{
				_cluster.Remove(cluster2);
			}
		}
		this.SelectionChanged?.Invoke();
	}

	public void ToggleSelection(IToolPiece piece)
	{
		if (!_pieces.Add(piece))
		{
			_pieces.Remove(piece);
		}
		this.SelectionChanged?.Invoke();
	}

	public void ToggleSelection(IToolSection section)
	{
		if (!_sections.Add(section))
		{
			_sections.Remove(section);
		}
		this.SelectionChanged?.Invoke();
	}

	public void ToggleSelection(IToolCluster cluster)
	{
		if (!_cluster.Add(cluster))
		{
			_cluster.Remove(cluster);
		}
		this.SelectionChanged?.Invoke();
	}

	public void SelectAll()
	{
		throw new NotImplementedException();
	}

	public void UnselectAll()
	{
		UnselectAll(raiseEvent: true);
	}

	public void UnselectAll(bool raiseEvent)
	{
		_pieces.Clear();
		_sections.Clear();
		_cluster.Clear();
		if (raiseEvent)
		{
			this.SelectionChanged?.Invoke();
		}
	}

	[IteratorStateMachine(typeof(_003CPiecesToSections_003Ed__68))]
	private IEnumerable<IToolSection> PiecesToSections(IEnumerable<IToolPiece> pieces)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CPiecesToSections_003Ed__68(-2)
		{
			_003C_003E3__pieces = pieces
		};
	}

	[IteratorStateMachine(typeof(_003CSectionsToCluster_003Ed__69))]
	private IEnumerable<IToolCluster> SectionsToCluster(IEnumerable<IToolSection> sections)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CSectionsToCluster_003Ed__69(-2)
		{
			_003C_003E3__sections = sections
		};
	}

	private IEnumerable<IToolPiece> SectionsToPieces(IEnumerable<IToolSection> sections)
	{
		return sections.SelectMany((IToolSection s) => s.Pieces);
	}

	private IEnumerable<IToolSection> ClusterToSections(IEnumerable<IToolCluster> cluster)
	{
		return cluster.SelectAndManyRecursive((IToolCluster c) => c.Children).Distinct().SelectMany((IToolCluster c) => c.Sections);
	}
}
