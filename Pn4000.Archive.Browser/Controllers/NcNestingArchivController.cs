using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Archive.Nesting;
using WiCAM.Pn4000.Archives;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.WpfControls;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;

namespace WiCAM.Pn4000.Archive.Browser.Controllers
{
	internal class NcNestingArchivController : ArchiveControllerBase, IArchivController
	{
		private const string __hiddenColumnNames = "IsModified;LastModified;Locked;LockedBy;";

		private static NcNestingArchivController _instance;

		private List<NcNestingInfo> _allItems;

		private List<NcNestingInfo> _selectedItems;

		private WpfDataGridController<NcNestingInfo> _gridController;

		private ListFilter<NcNestingInfo> _typeFilter;

		private ListFilter<NcNestingInfo> _nestingsFilter;

		private INestingArchiveReader _nestingReader;

		private List<NcNestingInfo> _filteredNcNestings;

		private string _nestingDbFilter;

		private bool _selectedState;

		private bool _isPreviewDisabled;

		public readonly static DependencyProperty ArchiveContentProperty;

		public ObservableCollection<NcNestingInfo> ArchiveContent
		{
			get
			{
				return base.GetValue(NcNestingArchivController.ArchiveContentProperty) as ObservableCollection<NcNestingInfo>;
			}
			set
			{
				base.SetValue(NcNestingArchivController.ArchiveContentProperty, value);
			}
		}

		public static NcNestingArchivController Instance
		{
			get
			{
				if (NcNestingArchivController._instance == null)
				{
					Logger.Verbose("Initialize NcNestingArchivController");
					NcNestingArchivController._instance = new NcNestingArchivController();
				}
				return NcNestingArchivController._instance;
			}
		}

		public string NestingConnectionString
		{
			get
			{
				return this._nestingReader.ConnectionString;
			}
		}

		static NcNestingArchivController()
		{
			NcNestingArchivController.ArchiveContentProperty = DependencyProperty.Register("ArchiveContent", typeof(ObservableCollection<NcNestingInfo>), typeof(MainWindow));
		}

		private NcNestingArchivController()
		{
			this._typeFilter = new ListFilter<NcNestingInfo>();
			this._selectedItems = new List<NcNestingInfo>();
			this.ArchiveContent = new ObservableCollection<NcNestingInfo>();
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
					(this._grid.Items[i] as NcNestingInfo).IsSelected = isSelected;
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}

		private void DatabaseFilterChanged(FilterConfiguration currentDatabaseFilter)
		{
			PnPathBuilder.ChangeArDrive(currentDatabaseFilter.ArDrivePath);
			this._nestingDbFilter = currentDatabaseFilter.FilterKey;
			if (!string.IsNullOrEmpty(this._nestingDbFilter))
			{
				//this._mainWindow.MenuTextFilter.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("FilterFull"), this._nestingDbFilter);
			}
			else
			{
				//this._mainWindow.MenuTextFilter.Text = StringResourceHelper.Instance.FindString("FilterEmpty");
			}
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = 0;
			this.NcNestingReadAsyncron(WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber, new Action(this.SetListData));
		}

		private bool DeleteFromDb(NcNestingInfo item)
		{
			bool flag = true;
			string str = this.NcNestingDeleteOne(item.Rowid);
			if (!string.IsNullOrEmpty(str))
			{
				Logger.Error(str);
				flag = false;
			}
			return flag;
		}

		public bool DeleteNestingFile(NcNestingInfo nesting)
		{
			Logger.Verbose("DeleteNestingFile: {0}", new object[] { nesting.JobNumber });
			if (!this.DeleteFromDb(nesting))
			{
				Logger.Error("Nesting '{0}' can not be deleted in DB!", new object[] { nesting.JobNumber });
				return false;
			}
			string str = ArchiveStructureHelper.Instance.PathByType(nesting.JobNumber, nesting.ArchiveNumber, ArchiveFolderType.mpl);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("Nesting '{0}' can not be deleted in file system!", new object[] { str });
				return false;
			}
			str = ArchiveStructureHelper.Instance.PathByType(nesting.JobNumber, nesting.ArchiveNumber, ArchiveFolderType.sba);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("Nesting '{0}' can not be deleted in file system!", new object[] { str });
				return false;
			}
			str = this.NestingCadgeoPath(nesting);
			if (IOHelper.FileDelete(str))
			{
				return true;
			}
			Logger.Error("Nesting '{0}' can not be deleted in file system!", new object[] { str });
			return false;
		}

		public void DeleteSelected()
		{
			List<NcNestingInfo> ncNestingInfos;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				ncNestingInfos = new List<NcNestingInfo>();
				if (this._grid.SelectedItem is NcNestingInfo)
				{
					ncNestingInfos.Add((NcNestingInfo)this._grid.SelectedItem);
				}
			}
			else
			{
				ncNestingInfos = this.ArchiveContent.ToList<NcNestingInfo>().FindAll((NcNestingInfo x) => x.IsSelected);
			}
			foreach (NcNestingInfo ncNestingInfo in ncNestingInfos)
			{
				if (!this.DeleteNestingFile(ncNestingInfo))
				{
					continue;
				}
				this.ArchiveContent.Remove(ncNestingInfo);
				if (EnumerableHelper.IsNullOrEmpty(this._selectedItems) || !this._selectedItems.Contains(ncNestingInfo))
				{
					continue;
				}
				this._selectedItems.Remove(ncNestingInfo);
			}
		}

		public void ExportToCsv()
		{
			ExcelCsvExportHelper.ToCsvFromWpfDataGrid<NcNestingInfo>(this.ArchiveContent, this._grid, "archiveMpl.csv");
			ProcessHelper.ExecuteNoWait("archiveMpl.csv", string.Empty);
		}

		private void FillListData(List<NcNestingInfo> list)
		{
			this.ArchiveContent.Clear();
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				foreach (NcNestingInfo ncNestingInfo in list)
				{
					this.ArchiveContent.Add(ncNestingInfo);
				}
			}
			if (this._grid.SelectionMode == DataGridSelectionMode.Extended)
			{
				if (!EnumerableHelper.IsNullOrEmpty(this._selectedItems))
				{
					int num = 0;
					foreach (NcNestingInfo _selectedItem in this._selectedItems)
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
				if (AppArguments.Instance.MultiselectValue == 10)
				{
					if (this.ArchiveContent.Count > 0)
					{
						this.ArchiveContent[0].IsSelected = true;
					}
				}
				else if (AppArguments.Instance.MultiselectValue == 20)
				{
					foreach (NcNestingInfo archiveContent in this.ArchiveContent)
					{
						archiveContent.IsSelected = true;
					}
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
				this._filterControl.ChangeFilterValue("JobNumber", searchString);
				AppArguments.Instance.SearchString = string.Empty;
			}
		}

		private void FilterControl_FilterTextChanged(object sender, RoutedEventArgs e)
		{
			this.NestingsByFilter(this._archiveFilterCriteria.ListFilters);
			List<NcNestingInfo> ncNestingInfos = this.NestingsByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
			this.FillListData(ncNestingInfos);
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
						NcNestingInfo item = (NcNestingInfo)isSelected.Item;
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
			Predicate<NcNestingInfo> predicate = null;
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
						NcNestingInfo item = (NcNestingInfo)dataGridRow.Item;
						DataGridCell dataGridCell = WpfVisualHelper.FindVisualParent<DataGridCell>(inputElement as UIElement);
						if (dataGridCell == null || dataGridCell.Column.DisplayIndex > 1)
						{
							for (int i = this._grid.SelectedItems.Count - 1; i >= 0; i--)
							{
								List<NcNestingInfo> ncNestingInfos = this._selectedItems;
								Predicate<NcNestingInfo> predicate1 = predicate;
								if (predicate1 == null)
								{
									Predicate<NcNestingInfo> rowid = (NcNestingInfo x) => x.Rowid == ((NcNestingInfo)this._grid.SelectedItems[i]).Rowid;
									Predicate<NcNestingInfo> predicate2 = rowid;
									predicate = rowid;
									predicate1 = predicate2;
								}
								if (ncNestingInfos.Find(predicate1) == null)
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
								NcNestingInfo ncNestingInfo = this._grid.Items[j] as NcNestingInfo;
								ncNestingInfo.IsSelected = true;
								if (!this._selectedItems.Contains(ncNestingInfo))
								{
									this._selectedItems.Add(ncNestingInfo);
								}
							}
							this._isShiftPressed = false;
							this._isPreviewDisabled = false;
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
				NcNestingInfo item = e.AddedItems[0] as NcNestingInfo;
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
			this.NcNestingInitialize();
			this._isDatabaseActive = NcNestingDbHelper.Instance.IsInitialised;
			base.InitializeBase(mainWindow, typeof(NcNestingInfo), AppConfiguration.Instance.DataListCaptions, WiCAM.Pn4000.Archive.Browser.Classes.CS.MplFilter);
			this.DatabaseFilterChangedHandler = new Action<FilterConfiguration>(this.DatabaseFilterChanged);
			NcNestingArchivController ncNestingArchivController = this;
			this._mainWindow.WriteErg0Handler = new Action(ncNestingArchivController.WriteErg0File);
			this._mainWindow.SelectedArchivChangedHandler = new Action<int>(this.SelectedArchiveChanged);
			NcNestingArchivController ncNestingArchivController1 = this;
			this._mainWindow.ConfigurationColumnsHandler = new Func<ObservableCollection<WpfGridColumnInfo>>(ncNestingArchivController1.GridColumnsConfiguration);
			this._mainWindow.ApplyGridSettingsHandler = new Action<bool>(this.SaveGridColumnSettings);
			NcNestingArchivController ncNestingArchivController2 = this;
			this._mainWindow.GridSelectionHandler = new Action<bool>(ncNestingArchivController2.ChangeSelection);
			NcNestingArchivController ncNestingArchivController3 = this;
			this._mainWindow.DeleteSelectedHandler = new Action(ncNestingArchivController3.DeleteSelected);
			NcNestingArchivController ncNestingArchivController4 = this;
			this._mainWindow.ExportToCsvHandler = new Action(ncNestingArchivController4.ExportToCsv);
			NcNestingArchivController ncNestingArchivController5 = this;
			this._mainWindow.InitializeMainWindowHandler = new Action(ncNestingArchivController5.InitializeMainWindowToolbar);
			this._mainWindow.BtnDelete.Visibility = Visibility.Hidden;
			this._mainWindow.PreviewKeyUp += new KeyEventHandler(this.Shift_KeyUp);
			this._mainWindow.PreviewKeyDown += new KeyEventHandler(this.Shift_KeyDown);
			this._filterControl.FilterTextChanged += new RoutedEventHandler(this.FilterControl_FilterTextChanged);
			this._grid.Name = "mplGrid";
			Style item = (Style)this._mainWindow.Resources["styleRightAlignedCell"];
			this._gridController = new WpfDataGridController<NcNestingInfo>(this._grid, "WiCAM.ArchivBrowser", AppConfiguration.Instance.ListConfiguration, null, item, null, AppArguments.Instance.IsMultiselect)
			{
				IsDragDropEnabled = false,
				HiddenColumns = "IsModified;LastModified;Locked;LockedBy;"
			};
			this._grid.SelectionChanged += new SelectionChangedEventHandler(this.Grid_SelectionChanged);
			this._grid.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.Grid_PreviewMouseDoubleClick);
			this._grid.MouseRightButtonUp += new MouseButtonEventHandler(this.Grid_MouseRightButtonUp);
			this._grid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.Grid_PreviewMouseLeftButtonDown);
			this._grid.PreviewKeyDown += new KeyEventHandler(this.Grid_PreviewKeyDown);
			base.InitializeFilters(typeof(NcNestingInfo));
		}

		public void InitializeMainWindowToolbar()
		{
			base.InitializeFiltersToolbar(this.NestingConnectionString);
		}

		private void LoadCadgeo(NcNestingInfo nesting)
		{
			string str = this.NestingCadgeoPath(nesting);
			if (!IOHelper.FileExists(str))
			{
				return;
			}
			string empty = string.Empty;
			ArchiveInfo archiveInfo = ArchiveStructureHelper.Instance.FindArchiveOrSubArchive(nesting.ArchiveNumber);
			ArchiveInfo archiveInfo1 = archiveInfo;
			archiveInfo1 = archiveInfo;
			if (archiveInfo1 != null)
			{
				empty = archiveInfo1.FullName;
			}
			this.LoadNestingCadgeoPreview(str, empty, nesting.JobNumber);
		}

		private void LoadNestingCadgeoPreview(string path, string archivName, string itemName)
		{
			if (this._isPreviewDisabled)
			{
				return;
			}
			Cursor cursor = this._mainWindow.Cursor;
			this._mainWindow.Cursor = Cursors.Wait;
			bool flag = this._mainWindow.CadgeoViewer1.LoadNestingCadgeo(path, this._mainWindow.CadgeoViewer1.ActualWidth, this._mainWindow.CadgeoViewer1.ActualHeight, false);
			this._mainWindow.Cursor = cursor;
			base.UpdateStatusText(flag, itemName, archivName);
		}

		public string NcNestingDeleteOne(long rowId)
		{
			if (this._nestingReader is NcNestingDbHelper)
			{
				NcNestingDbHelper ncNestingDbHelper = this._nestingReader as NcNestingDbHelper;
				if (ncNestingDbHelper != null)
				{
					return ncNestingDbHelper.NcNestingDeleteOne(rowId);
				}
			}
			return string.Empty;
		}

		public bool NcNestingInitialize()
		{
			if (this._nestingReader == null)
			{
				if (!NcNestingDbHelper.Instance.IsInitialised)
				{
					Logger.Info("--- FILESYSTEM");
					this._nestingReader = NcNestingIOHelper.Instance;
				}
				else
				{
					Logger.Info("--- DATABASE");
					this._nestingReader = NcNestingDbHelper.Instance;
				}
			}
			this._nestingsFilter = new ListFilter<NcNestingInfo>();
			return true;
		}

		public void NcNestingReadAsyncron(int archiveNumber, Action onReadyAction)
		{
			this._nestingReader.ReadAsync(archiveNumber, onReadyAction);
		}

		private string NestingCadgeoPath(NcNestingInfo nesting)
		{
			return ArchiveStructureHelper.Instance.PathByType(nesting.JobNumber, nesting.ArchiveNumber, ArchiveFolderType.s2d);
		}

		public List<NcNestingInfo> NestingsByDateInterval(DateTime from, DateTime till)
		{
			return this._filteredNcNestings.FindAll((NcNestingInfo x) => {
				if (x.SDate < from.Date)
				{
					return false;
				}
				return x.SDate <= till.Date;
			});
		}

		public List<NcNestingInfo> NestingsByFilter(List<FilterInfo> filters)
		{
			List<NcNestingInfo> ncNestingInfos = this._nestingsFilter.Filter(this._nestingReader.Nestings, filters);
			List<NcNestingInfo> ncNestingInfos1 = ncNestingInfos;
			this._filteredNcNestings = ncNestingInfos;
			return ncNestingInfos1;
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
			this.NcNestingReadAsyncron(archivNumber, new Action(this.SetListData));
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ExpandSelected();
		}

		private void SetListData()
		{
			ListFilter<NcNestingInfo> listFilter = this._typeFilter;
			List<NcNestingInfo> nestings = this._nestingReader.Nestings;
			List<NcNestingInfo> ncNestingInfos = nestings;
			this._allItems = nestings;
			this.FillListData(listFilter.Filter(ncNestingInfos, this._archiveFilterCriteria.ListFilters));
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
			List<NcNestingInfo> ncNestingInfos;
			int archiveNumber;
			StringBuilder stringBuilder = new StringBuilder();
			FilterConfiguration filterConfiguration = null;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				ncNestingInfos = new List<NcNestingInfo>();
				if (this._grid.SelectedItem is NcNestingInfo)
				{
					ncNestingInfos.Add((NcNestingInfo)this._grid.SelectedItem);
				}
			}
			else
			{
				ncNestingInfos = this.ArchiveContent.ToList<NcNestingInfo>().FindAll((NcNestingInfo x) => x.IsSelected);
			}
			if (AppConfiguration.Erg0Format != 2)
			{
				foreach (NcNestingInfo ncNestingInfo in ncNestingInfos)
				{
					if (ncNestingInfo == null)
					{
						continue;
					}
					archiveNumber = ncNestingInfo.ArchiveNumber;
					stringBuilder.AppendLine(archiveNumber.ToString());
					stringBuilder.AppendLine(ncNestingInfo.JobNumber);
				}
			}
			else
			{
				stringBuilder.AppendLine(" VERSION 2.00");
				foreach (NcNestingInfo ncNestingInfo1 in ncNestingInfos)
				{
					if (ncNestingInfo1 == null)
					{
						continue;
					}
					archiveNumber = ncNestingInfo1.ArchiveNumber;
					stringBuilder.AppendLine(archiveNumber.ToString());
					stringBuilder.AppendLine(ncNestingInfo1.JobNumber);
					if (DatabaseFilterHelper.Instance.DatabaseFilters == null)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine(PnPathBuilder.ArDrive);
					}
					else
					{
						filterConfiguration = DatabaseFilterHelper.Instance.DatabaseFilters.Find((FilterConfiguration x) => x.FilterKey == ncNestingInfo1.Filter);
						if (filterConfiguration == null)
						{
							stringBuilder.AppendLine();
							stringBuilder.AppendLine(PnPathBuilder.ArDrive);
						}
						else
						{
							stringBuilder.AppendLine(filterConfiguration.FilterKey);
							stringBuilder.AppendLine(filterConfiguration.ArDrivePath);
						}
					}
				}
			}
			IOHelper.FileWriteAllText("ERG0", stringBuilder.ToString());
		}
	}
}