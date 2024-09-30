using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archive.Browser.Controllers;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveCad
{
	internal class CadPartArchiveController : ArchiveControllerBase, IArchivController
	{
		private static CadPartArchiveController _instance;

		public readonly static DependencyProperty ArchiveContentCollectionProperty;

		private const string __hiddenColumnNames = "Rowid;";

		private bool _isPreviewDisabled;

		private CadPartDataRepository _repository;

		private List<CadPartInfo> _allItems;

		private List<CadPartInfo> _selectedItems;

		private WpfDataGridController<CadPartInfo> _gridController;

		private bool _selectedState;

		public ObservableCollection<CadPartInfo> ArchiveContent
		{
			get
			{
				return (ObservableCollection<CadPartInfo>)base.GetValue(CadPartArchiveController.ArchiveContentCollectionProperty);
			}
			set
			{
				base.SetValue(CadPartArchiveController.ArchiveContentCollectionProperty, value);
			}
		}

		public static CadPartArchiveController Instance
		{
			get
			{
				if (CadPartArchiveController._instance == null)
				{
					Logger.Verbose("Initialize CadPartArchiveController");
					CadPartArchiveController._instance = new CadPartArchiveController();
				}
				return CadPartArchiveController._instance;
			}
		}

		static CadPartArchiveController()
		{
			CadPartArchiveController.ArchiveContentCollectionProperty = DependencyProperty.Register("ArchiveContentCollection", typeof(ObservableCollection<CadPartInfo>), typeof(CadPartArchiveController));
		}

		private CadPartArchiveController()
		{
			this._repository = new CadPartDataRepository();
			this._allItems = new List<CadPartInfo>();
			this._selectedItems = new List<CadPartInfo>();
			this.ArchiveContent = new ObservableCollection<CadPartInfo>();
		}

		public void ApplyGridSettings(bool isMultiSelect)
		{
			this._gridController.ApplySettings(isMultiSelect);
		}

		public void ChangeSelection(bool isSelected)
		{
			if (!isSelected)
			{
				this._grid.UnselectAll();
			}
			else
			{
				this._grid.SelectAll();
			}
			try
			{
				for (int i = 0; i < this._grid.Items.Count; i++)
				{
					(this._grid.Items[i] as CadPartInfo).IsSelected = isSelected;
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}

		public string DeleteOne(long rowId)
		{
			INcPartsArchivReader instance = null;
			if (!NcPartsDataReader.Instance.IsInitialised)
			{
				Logger.Info("--- FILESYSTEM");
				instance = NcPartIOHelper.Instance;
			}
			else
			{
				Logger.Info("--- DATABASE");
				instance = NcPartsDataReader.Instance;
			}
			if (instance is DataManager)
			{
				DataManager dataManager = instance as DataManager;
				if (dataManager != null)
				{
					return dataManager.DeleteFromDb(rowId);
				}
			}
			else if (instance is NcPartsDataReader)
			{
				DataManager dataManager1 = new DataManager();
				if (dataManager1.InitializeDatabase())
				{
					return dataManager1.DeleteFromDb(rowId);
				}
			}
			return string.Empty;
		}

		public void DeleteSelected()
		{
			List<CadPartInfo> cadPartInfos;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				cadPartInfos = new List<CadPartInfo>();
				if (this._grid.SelectedItem is CadPartInfo)
				{
					cadPartInfos.Add((CadPartInfo)this._grid.SelectedItem);
				}
			}
			else
			{
				cadPartInfos = this.ArchiveContent.ToList<CadPartInfo>().FindAll((CadPartInfo x) => x.IsSelected);
			}
			foreach (CadPartInfo cadPartInfo in cadPartInfos)
			{
				string str = this.DeleteOne((long)cadPartInfo.Rowid);
				if (string.IsNullOrEmpty(str))
				{
					this._repository.DeleteFiles(cadPartInfo);
					this.ArchiveContent.Remove(cadPartInfo);
					if (EnumerableHelper.IsNullOrEmpty(this._selectedItems) || !this._selectedItems.Contains(cadPartInfo))
					{
						continue;
					}
					this._selectedItems.Remove(cadPartInfo);
				}
				else
				{
					Logger.Error(str);
				}
			}
		}

		public void ExportToCsv()
		{
			ExcelCsvExportHelper.ToCsvFromWpfDataGrid<CadPartInfo>(this.ArchiveContent, this._grid, "cadArchive.csv");
			ProcessHelper.ExecuteNoWait("cadArchive.csv", string.Empty);
		}

		private void FilterControl_FilterTextChanged(object sender, RoutedEventArgs e)
		{
			this._repository.ByFilter(this._archiveFilterCriteria.ListFilters);
			List<CadPartInfo> cadPartInfos = this._repository.ByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
			this.SetPartListData(cadPartInfos);
		}

		private void Grid_AutoGeneratedColumns(object sender, EventArgs e)
		{
			if (!AppArguments.Instance.IsMultiselect && this._gridController.HiddenColumns.IndexOf("IsSelected;") == -1)
			{
				WpfDataGridController<CadPartInfo> wpfDataGridController = this._gridController;
				wpfDataGridController.HiddenColumns = string.Concat(wpfDataGridController.HiddenColumns, "IsSelected;");
			}
			this._gridController.ApplySettings(AppArguments.Instance.IsMultiselect);
		}

		private void Grid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			DependencyObject dependencyObject = base.GetDependencyObject((DependencyObject)e.OriginalSource, typeof(DataGridCell));
			if (dependencyObject != null)
			{
				DataGridRow isSelected = WpfVisualHelper.FindVisualParent<DataGridRow>(dependencyObject as UIElement);
				if (isSelected != null)
				{
					try
					{
						CadPartInfo item = (CadPartInfo)isSelected.Item;
						item.IsSelected = true;
						isSelected.IsSelected = item.IsSelected;
						e.Handled = true;
						base.ButtokOkClick();
					}
					catch (Exception exception)
					{
						Logger.Exception(exception);
					}
				}
			}
		}

		private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Predicate<CadPartInfo> predicate = null;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				return;
			}
			IInputElement inputElement = this._grid.InputHitTest(e.GetPosition(this._grid));
			if (inputElement != null)
			{
				DataGridRow dataGridRow = WpfVisualHelper.FindVisualParent<DataGridRow>(inputElement as UIElement);
				if (dataGridRow != null)
				{
					try
					{
						CadPartInfo item = (CadPartInfo)dataGridRow.Item;
						DataGridCell dataGridCell = WpfVisualHelper.FindVisualParent<DataGridCell>(inputElement as UIElement);
						if (dataGridCell == null || dataGridCell.Column.DisplayIndex > 1)
						{
							for (int i = this._grid.SelectedItems.Count - 1; i >= 0; i--)
							{
								List<CadPartInfo> cadPartInfos = this._selectedItems;
								Predicate<CadPartInfo> predicate1 = predicate;
								if (predicate1 == null)
								{
									Predicate<CadPartInfo> rowid = (CadPartInfo x) => x.Rowid == ((CadPartInfo)this._grid.SelectedItems[i]).Rowid;
									Predicate<CadPartInfo> predicate2 = rowid;
									predicate = rowid;
									predicate1 = predicate2;
								}
								if (cadPartInfos.Find(predicate1) == null)
								{
									this._grid.SelectedItems.RemoveAt(i);
								}
							}
							dataGridRow.IsSelected = true;
						}
						else
						{
							if (item.IsSelected)
							{
								if (this._selectedItems.Contains(item))
								{
									this._selectedItems.Remove(item);
								}
							}
							else if (!this._selectedItems.Contains(item))
							{
								this._selectedItems.Add(item);
							}
							bool isSelected = !item.IsSelected;
							bool flag = isSelected;
							dataGridRow.IsSelected = isSelected;
							item.IsSelected = flag;
						}
						this.LoadCadgeo(item);
						if (this._isShiftPressed)
						{
							int count = this._grid.SelectedItems.Count - 1;
							this._lastSelectedIndex = this._grid.Items.IndexOf(this._grid.SelectedItems[count]);
							if (this._lastSelectedIndex < this._firstSelectedIndex)
							{
								int num = this._lastSelectedIndex;
								this._lastSelectedIndex = this._firstSelectedIndex;
								this._firstSelectedIndex = num;
							}
							this._isPreviewDisabled = true;
							for (int j = this._firstSelectedIndex; j <= this._lastSelectedIndex; j++)
							{
								CadPartInfo cadPartInfo = this._grid.Items[j] as CadPartInfo;
								cadPartInfo.IsSelected = true;
								if (!this._selectedItems.Contains(cadPartInfo))
								{
									this._selectedItems.Add(cadPartInfo);
								}
							}
							this._isPreviewDisabled = false;
							this._isShiftPressed = false;
						}
					}
					catch (Exception exception)
					{
						Logger.Exception(exception);
					}
				}
			}
		}

		private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!EnumerableHelper.IsNullOrEmpty(e.AddedItems))
			{
				CadPartInfo item = e.AddedItems[0] as CadPartInfo;
				if (item != null)
				{
					this.LoadCadgeo(item);
				}
			}
		}

		public ObservableCollection<WpfGridColumnInfo> GridColumnsConfiguration()
		{
			return this._gridController.GridColumnsConfiguration();
		}

		public void Initialize(MainWindow mainWindow)
		{
			ArchiveControllerBase.OkClickHandler = new Action(this.OkButtonClick);
			if (AppConfiguration.Erg0Format < 3)
			{
				AppConfiguration.Erg0Format = 1;
			}
			base.InitializeBase(mainWindow, typeof(CadPartInfo), AppConfiguration.Instance.DataListCaptions, WiCAM.Pn4000.Archive.Browser.Classes.CS.CadFilter);
			CadPartArchiveController cadPartArchiveController = this;
			this._mainWindow.WriteErg0Handler = new Action(cadPartArchiveController.WriteErg0File);
			this._mainWindow.SelectedArchivChangedHandler = new Action<int>(this.SelectedArchiveChanged);
			CadPartArchiveController cadPartArchiveController1 = this;
			this._mainWindow.ConfigurationColumnsHandler = new Func<ObservableCollection<WpfGridColumnInfo>>(cadPartArchiveController1.GridColumnsConfiguration);
			this._mainWindow.ApplyGridSettingsHandler = new Action<bool>(this.SaveGridColumnSettings);
			CadPartArchiveController cadPartArchiveController2 = this;
			this._mainWindow.GridSelectionHandler = new Action<bool>(cadPartArchiveController2.ChangeSelection);
			CadPartArchiveController cadPartArchiveController3 = this;
			this._mainWindow.DeleteSelectedHandler = new Action(cadPartArchiveController3.DeleteSelected);
			CadPartArchiveController cadPartArchiveController4 = this;
			this._mainWindow.ExportToCsvHandler = new Action(cadPartArchiveController4.ExportToCsv);
			CadPartArchiveController cadPartArchiveController5 = this;
			this._mainWindow.InitializeMainWindowHandler = new Action(cadPartArchiveController5.InitializeMainWindowToolbar);
			this._mainWindow.PreviewKeyUp += new KeyEventHandler(this.Shift_KeyUp);
			this._mainWindow.PreviewKeyDown += new KeyEventHandler(this.Shift_KeyDown);
			this._mainWindow.BtnDelete.Visibility = Visibility.Hidden;
			this._grid.Name = "cadGrid";
			Style item = (Style)this._mainWindow.Resources["styleRightAlignedCell"];
			this._gridController = new WpfDataGridController<CadPartInfo>(this._grid, "WiCAM.ArchivBrowser", AppConfiguration.Instance.ListConfiguration, null, item, null, AppArguments.Instance.IsMultiselect)
			{
				HiddenColumns = "Rowid;",
				IsDragDropEnabled = false
			};
			this._grid.SelectionChanged += new SelectionChangedEventHandler(this.Grid_SelectionChanged);
			this._grid.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.Grid_PreviewMouseDoubleClick);
			this._grid.MouseRightButtonUp += new MouseButtonEventHandler(this.Grid_MouseRightButtonUp);
			this._grid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.Grid_PreviewMouseLeftButtonDown);
			this._grid.AutoGeneratedColumns += new EventHandler(this.Grid_AutoGeneratedColumns);
			this._grid.PreviewKeyDown += new KeyEventHandler(this.Grid_PreviewKeyDown);
			this._filterControl.FilterTextChanged += new RoutedEventHandler(this.FilterControl_FilterTextChanged);
			base.InitializeFilters(typeof(CadPartInfo));
		}

		public void InitializeMainWindowToolbar()
		{
			base.InitializeFiltersToolbar(this._repository.ConnectionString);
			this._repository.ReadAsyncron(AppArguments.Instance.ArchiveNumber(), new Action(this.SetPartListData));
		}

		private void LoadCadgeo(CadPartInfo part)
		{
			if (this._isPreviewDisabled)
			{
				return;
			}
			ArchiveInfo archiveInfo = ArchiveStructureHelper.Instance.ArchiveData.Find((ArchiveInfo x) => x.FullArchiveNumber() == part.ArchiveNumber);
			string empty = string.Empty;
			if (archiveInfo != null)
			{
				empty = archiveInfo.Name;
			}
			base.LoadCadgeoPreview(part.Path(), empty, part.PartName, 0, 0);
		}

		private void OkButtonClick()
		{
			this.WriteErg0File();
		//	this._mainWindow.Close();
		}

		private void SaveGridColumnSettings(bool isMultiSelect)
		{
			this.ApplyGridSettings(isMultiSelect);
			this._gridController.Save();
		}

		public void SaveSettings()
		{
			this._gridController.Save();
			if (ArchiveDataReadHelper.Instance != null)
			{
				ArchiveDataReadHelper.Instance.Dispose();
			}
			if (!this._archiveFilterCriteria.Save())
			{
				Logger.Error("Problems writing archive filter criteria!");
			}
		}

		private void SelectedArchiveChanged(int archivNumber)
		{
			this._repository.ReadAsyncron(archivNumber, new Action(this.SetPartListData));
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ExpandSelected();
		}

		private void SetPartListData()
		{
			List<CadPartInfo> parts = this._repository.Parts;
			List<CadPartInfo> cadPartInfos = parts;
			this._allItems = parts;
			this.SetPartListData(cadPartInfos);
			if (this._mainWindow.FilterControl1.HasToBeFiltered)
			{
				List<CadPartInfo> cadPartInfos1 = this._repository.ByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
				this.SetPartListData(cadPartInfos1);
			}
		}

		private void SetPartListData(List<CadPartInfo> list)
		{
			this.ArchiveContent.Clear();
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				foreach (CadPartInfo cadPartInfo in list)
				{
					this.ArchiveContent.Add(cadPartInfo);
				}
			}
			if (this._grid.SelectionMode == DataGridSelectionMode.Extended)
			{
				if (!EnumerableHelper.IsNullOrEmpty(this._selectedItems))
				{
					int num = 0;
					foreach (CadPartInfo _selectedItem in this._selectedItems)
					{
						if (this.ArchiveContent.Contains(_selectedItem))
						{
							continue;
						}
						int num1 = num;
						num = num1 + 1;
						this.ArchiveContent.Insert(num1, _selectedItem);
					}
				}
				if (AppArguments.Instance.MultiselectValue == 10 && this.ArchiveContent.Count > 0)
				{
					this.ArchiveContent[0].IsSelected = true;
				}
			}
			int count = 0;
			if (!EnumerableHelper.IsNullOrEmpty(this.ArchiveContent))
			{
				count = this.ArchiveContent.Count;
			}
			int count1 = 0;
			if (!EnumerableHelper.IsNullOrEmpty(this._allItems))
			{
				count1 = this._allItems.Count;
			}
			this._mainWindow.StatusText.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("MessageFilesAmount"), count, count1);
			string searchString = AppArguments.Instance.SearchString;
			if (!string.IsNullOrEmpty(searchString))
			{
				AppArguments.Instance.SearchString = string.Empty;
				this._filterControl.ChangeFilterValue("PartName", searchString);
				AppArguments.Instance.SearchString = string.Empty;
			}
		}

		private void Shift_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return && this.ArchiveContent != null && this.ArchiveContent.Count == 1)
			{
				this._grid.SelectedItem = this.ArchiveContent[0];
				this.OkButtonClick();
			}
			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				this._isShiftPressed = true;
				if (this._grid.SelectedItems.Count > 0)
				{
					this._firstSelectedIndex = this._grid.Items.IndexOf(this._grid.SelectedItems[0]);
					return;
				}
			}
			else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
			{
				this._isCtrlPressed = true;
			}
		}

		private void Shift_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
			{
				this._isShiftPressed = false;
				return;
			}
			if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
			{
				this._isCtrlPressed = false;
				return;
			}
			if (e.Key != Key.Delete)
			{
				if (e.Key == Key.Return)
				{
					this.OkButtonClick();
					return;
				}
				if (e.Key == Key.A && this._grid.SelectionMode == DataGridSelectionMode.Extended && this._isCtrlPressed)
				{
					this._selectedState = !this._selectedState;
					this.ChangeSelection(this._selectedState);
				}
			}
			else if (this._isCtrlPressed)
			{
				this.ChangeSelection(false);
				return;
			}
		}

		public void WriteErg0File()
		{
			List<CadPartInfo> cadPartInfos;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				cadPartInfos = new List<CadPartInfo>();
				if (this._grid.SelectedItem is CadPartInfo)
				{
					cadPartInfos.Add((CadPartInfo)this._grid.SelectedItem);
				}
			}
			else
			{
				cadPartInfos = this.ArchiveContent.ToList<CadPartInfo>().FindAll((CadPartInfo x) => x.IsSelected);
			}
			(new Erg0Helper()).WriteCadPartsErg0(cadPartInfos);
		}
	}
}