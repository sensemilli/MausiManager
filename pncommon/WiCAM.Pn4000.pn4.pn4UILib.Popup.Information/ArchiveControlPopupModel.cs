using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.WpfControls.ArchivesControl;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

public class ArchiveControlPopupModel : ArchivControlModelBase
{
	private List<PopupLine> _lines;

	private Dictionary<PopupLine, ArchiveInfo> _popupArchiveDictionary = new Dictionary<PopupLine, ArchiveInfo>();

	private string _totalsText;

	private ArchiveInfo _selected;

	public bool IsTreeViewVirtual { get; set; } = true;

	public string TotalsText
	{
		get
		{
			return this._totalsText;
		}
		set
		{
			this._totalsText = value;
			base.NotifyPropertyChanged("TotalsText");
		}
	}

	public void InitializeModel(ArchiveTreeFilterControl view, List<PopupLine> lines)
	{
		this._lines = lines;
		base.SelectAllHandler = SelectAll;
		base.View = view;
		base.AllArchives = new List<ArchiveInfo>();
		base.ArchivesCollection.Clear();
		view.DataContext = this;
		view.TreeView().Loaded -= ArchiveControlPopupModel_Loaded;
		view.TreeView().Loaded += ArchiveControlPopupModel_Loaded;
		this.GetDataFromPopupLines();
		base.InitializeWithoutItemSelection(view);
		view.TreeView().SelectedItemChanged += TreeView_SelectedItemChanged;
	}

	private void ArchiveControlPopupModel_Loaded(object sender, RoutedEventArgs e)
	{
		if (this._selected != null)
		{
			ArchiveTreeView.SelectItemByIndex(base.View.TreeView(), this._selected);
		}
	}

	private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		if (e.NewValue != null)
		{
			base.HasToSelectAll = false;
			this.SelectAll();
			if (base.SelectedArchiv != null)
			{
				base.SelectedArchiv.IsSelected = false;
			}
			base.SelectedArchiv = e.NewValue as ArchiveInfo;
			if (base.SelectedArchiv != null)
			{
				base.SelectedArchiv.IsSelected = true;
			}
		}
	}

	private void SelectAll()
	{
		foreach (KeyValuePair<PopupLine, ArchiveInfo> item in this._popupArchiveDictionary)
		{
			item.Value.IsSelected = base.HasToSelectAll;
			if (EnumerableHelper.IsNullOrEmpty(item.Value.SubArchives))
			{
				continue;
			}
			foreach (SubArchiveInfo subArchive in item.Value.SubArchives)
			{
				subArchive.IsSelected = base.HasToSelectAll;
			}
		}
	}

	private void GetDataFromPopupLines()
	{
		base.AllArchives = new List<ArchiveInfo>();
		base.ArchivesCollection.Clear();
		if (this._lines.Count < 2)
		{
			return;
		}
		foreach (PopupLine line in this._lines)
		{
			line.text = line.text.Replace("\\v", "\v");
		}
		bool flag = this.AreSubArchives();
		int rowId = 0;
		int num = 1;
		for (int i = 1; i < this._lines.Count; i++)
		{
			if (i == 1)
			{
				base.IsSingleSelection = this._lines[i].typ == 6;
				if (base.IsSingleSelection)
				{
					base.CanSelectAll = false;
				}
			}
			string[] array = this._lines[i].text.Split('\v');
			if (flag)
			{
				int result = 0;
				int.TryParse(array[1], out result);
				if (result == 0)
				{
					num = 1;
					ArchiveInfo archiveInfo = this.FromData(array[0], array[2]);
					archiveInfo.RowId = rowId++;
					archiveInfo.SubArchives = new ObservableCollection<SubArchiveInfo>();
					base.AllArchives.Add(archiveInfo);
					this._popupArchiveDictionary.Add(this._lines[i], archiveInfo);
					if (this._lines[i].sel == 1)
					{
						archiveInfo.IsSelected = true;
						this._selected = archiveInfo;
					}
					continue;
				}
				ArchiveInfo archiveInfo2 = this.FromData(array[1], array[3]);
				archiveInfo2.RowId = rowId;
				archiveInfo2.SubArchiveViewNumber = num++;
				archiveInfo2.IsSubArchiv = true;
				int result2 = 0;
				int.TryParse(array[0].Substring(0, array[0].Length - 4), out result2);
				if (result2 <= 0)
				{
					continue;
				}
				ArchiveInfo archiveByNumber = this.GetArchiveByNumber(result2);
				if (archiveByNumber != null)
				{
					archiveByNumber.SubArchives.Add(archiveInfo2);
					this._popupArchiveDictionary.Add(this._lines[i], archiveInfo2);
					archiveInfo2.ParentNumber = result2;
					if (this._lines[i].sel == 1)
					{
						archiveByNumber.IsExpanded = true;
						archiveInfo2.IsSelected = true;
						this._selected = archiveInfo2;
					}
				}
			}
			else
			{
				num = 1;
				ArchiveInfo archiveInfo3 = this.FromData(array[0], array[1]);
				archiveInfo3.RowId = rowId++;
				base.AllArchives.Add(archiveInfo3);
				this._popupArchiveDictionary.Add(this._lines[i], archiveInfo3);
				if (this._lines[i].sel == 1)
				{
					archiveInfo3.IsSelected = true;
					this._selected = archiveInfo3;
				}
			}
		}
		foreach (ArchiveInfo allArchive in base.AllArchives)
		{
			base.ArchivesCollection.Add(allArchive);
		}
		base.HasSubArchives = base.CheckHasSubArchives();
		if (this._selected != null)
		{
			ArchiveTreeView.SelectItemByIndex(base.View.TreeView(), this._selected);
		}
	}

	private ArchiveInfo FromData(string number, string name)
	{
		ArchiveInfo archiveInfo = new ArchiveInfo
		{
			Number = Convert.ToInt32(number),
			Name = name
		};
		archiveInfo.FullName = archiveInfo.Number.ToString("D4") + " " + archiveInfo.Name;
		archiveInfo.SubArchives = new ObservableCollection<SubArchiveInfo>();
		return archiveInfo;
	}

	private ArchiveInfo GetArchiveByNumber(int number)
	{
		foreach (ArchiveInfo allArchive in base.AllArchives)
		{
			if (allArchive.Number == number)
			{
				return allArchive;
			}
		}
		return null;
	}

	private bool AreSubArchives()
	{
		if (this._lines.Count < 2)
		{
			return false;
		}
		return this._lines[0].text.Split('\v').Length == 4;
	}

	internal bool IsSelectedPopupLine(PopupLine popupLine)
	{
		if (!this._popupArchiveDictionary.ContainsKey(popupLine))
		{
			return false;
		}
		ArchiveInfo archiveInfo = this._popupArchiveDictionary[popupLine];
		if (archiveInfo.IsSelected)
		{
			Logger.Info($"ArchiveControlPopupModel : Selected {archiveInfo.FullArchiveNumber()}");
		}
		return archiveInfo.IsSelected;
	}
}
