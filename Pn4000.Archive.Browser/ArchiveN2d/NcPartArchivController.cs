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
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveN2d
{
	internal class NcPartArchivController : ArchiveControllerBase, IArchivController
	{
		private static NcPartArchivController _instance;

		private const string __hiddenColumnNames = "IsModified;LastModified;";

		public List<NcPartInfo> _selectedItems;

		private WpfDataGridController<NcPartInfo> _gridController;

		private bool _selectedState;

		private bool _isPreviewDisabled;

		private Dictionary<bool, Action<NcPartInfo>> _multipleSelectionChanged = new Dictionary<bool, Action<NcPartInfo>>();

		private NcPartDataRepository _dataRepository;

		private NcPartDeleteHelper _deleteHelper;

		public readonly static DependencyProperty ArchiveContentProperty;

		public readonly static DependencyProperty SearchStringProperty;

		public ObservableCollection<NcPartInfo> ArchiveContent
		{
			get
			{
				return base.GetValue(NcPartArchivController.ArchiveContentProperty) as ObservableCollection<NcPartInfo>;
			}
			set
			{
				base.SetValue(NcPartArchivController.ArchiveContentProperty, value);
			}
		}

		public static NcPartArchivController Instance
		{
			get
			{
				if (NcPartArchivController._instance == null)
				{
					Logger.Verbose("Initialize NcPartBehaviourController");
					NcPartArchivController._instance = new NcPartArchivController();
				}
				return NcPartArchivController._instance;
			}
		}

		public string SearchString
		{
			get
			{
				return (string)base.GetValue(NcPartArchivController.SearchStringProperty);
			}
			set
			{
				base.SetValue(NcPartArchivController.SearchStringProperty, value);
			}
		}

		static NcPartArchivController()
		{
			NcPartArchivController.ArchiveContentProperty = DependencyProperty.Register("ArchiveContent", typeof(ObservableCollection<NcPartInfo>), typeof(MainWindow));
			NcPartArchivController.SearchStringProperty = DependencyProperty.Register("SearchString", typeof(string), typeof(NcPartArchivController), new PropertyMetadata(null, new PropertyChangedCallback(NcPartArchivController.SearchStringChanged)));
		}

		public NcPartArchivController()
		{
			this._dataRepository = new NcPartDataRepository();
			this._deleteHelper = new NcPartDeleteHelper(ArchiveStructureHelper.Instance);
			this._selectedItems = new List<NcPartInfo>();
			this.ArchiveContent = new ObservableCollection<NcPartInfo>();
			this._multipleSelectionChanged.Add(true, new Action<NcPartInfo>(this.SelectedAdd));
			this._multipleSelectionChanged.Add(false, new Action<NcPartInfo>(this.SelectedRemove));
		}

		public void ApplyGridSettings(bool isMultiSelect)
		{
			this._gridController.ApplySettings(isMultiSelect);
		}

		private void ArchivFilterChangedHandler(object ignored)
		{
			this.SelectedArchiveChanged(0);
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
					NcPartInfo item = this._grid.Items[i] as NcPartInfo;
					if (item != null)
					{
						item.IsSelected = isSelected;
						this._multipleSelectionChanged[isSelected](item);
					}
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
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ChangeDatabaseFilter(currentDatabaseFilter, new Action<object>(this.ArchivFilterChangedHandler));
			this._dataRepository.DbFilter = currentDatabaseFilter.FilterKey;
			if (!string.IsNullOrEmpty(currentDatabaseFilter.FilterKey))
			{
			//	this._mainWindow.MenuTextFilter.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("FilterFull"), currentDatabaseFilter.FilterKey);
			}
			else
			{
			//	this._mainWindow.MenuTextFilter.Text = StringResourceHelper.Instance.FindString("FilterEmpty");
			}
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = 0;
			this.SelectedArchiveChanged(0);
		}

		public void DeleteSelected()
		{
			List<NcPartInfo> ncPartInfos;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				ncPartInfos = new List<NcPartInfo>();
				if (this._grid.SelectedItem is NcPartInfo)
				{
					ncPartInfos.Add((NcPartInfo)this._grid.SelectedItem);
				}
			}
			else
			{
				ncPartInfos = this.ArchiveContent.ToList<NcPartInfo>().FindAll((NcPartInfo x) => x.IsSelected);
			}
			foreach (NcPartInfo ncPartInfo in ncPartInfos)
			{
				string str = this._dataRepository.DeleteOne(ncPartInfo.Rowid);
				if (string.IsNullOrEmpty(str))
				{
					this._deleteHelper.DeleteFiles(ncPartInfo);
					this.ArchiveContent.Remove(ncPartInfo);
					this._dataRepository.Parts.Remove(ncPartInfo);
					if (EnumerableHelper.IsNullOrEmpty(this._selectedItems) || !this._selectedItems.Contains(ncPartInfo))
					{
						continue;
					}
					this._selectedItems.Remove(ncPartInfo);
				}
				else
				{
					Logger.Error(str);
				}
			}
			this.SetStatusText();
		}

		public void ExportToCsv()
		{
			ExcelCsvExportHelper.ToCsvFromWpfDataGrid<NcPartInfo>(this.ArchiveContent, this._grid, "archive.csv");
			ProcessHelper.ExecuteNoWait("archive.csv", string.Empty);
		}

		private void FilterControl_FilterTextChanged(object sender, RoutedEventArgs e)
		{
			this._dataRepository.ByFilter(this._archiveFilterCriteria.ListFilters);
			if (this._dataRepository.MaxDate > new DateTime(1990, 1, 1))
			{
				this._archiveFilterCriteria.DateTill = this._dataRepository.MaxDate;
			}
			List<NcPartInfo> ncPartInfos = this._dataRepository.ByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
			this.SetPartListData(ncPartInfos);
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
						NcPartInfo item = (NcPartInfo)isSelected.Item;
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
			Predicate<NcPartInfo> predicate = null;
			if (this._grid.SelectionMode == DataGridSelectionMode.Extended)
			{
				IInputElement inputElement = this._grid.InputHitTest(e.GetPosition(this._grid));
				if (inputElement != null)
				{
					DataGridRow dataGridRow = WpfVisualHelper.FindVisualParent<DataGridRow>(inputElement as UIElement);
					if (dataGridRow != null)
					{
						try
						{
							NcPartInfo item = (NcPartInfo)dataGridRow.Item;
							DataGridCell dataGridCell = WpfVisualHelper.FindVisualParent<DataGridCell>(inputElement as UIElement);
							if (dataGridCell == null || dataGridCell.Column.DisplayIndex > 1)
							{
								for (int i = this._grid.SelectedItems.Count - 1; i >= 0; i--)
								{
									List<NcPartInfo> ncPartInfos = this._selectedItems;
									Predicate<NcPartInfo> predicate1 = predicate;
									if (predicate1 == null)
									{
										Predicate<NcPartInfo> rowid = (NcPartInfo x) => x.Rowid == ((NcPartInfo)this._grid.SelectedItems[i]).Rowid;
										Predicate<NcPartInfo> predicate2 = rowid;
										predicate = rowid;
										predicate1 = predicate2;
									}
									if (ncPartInfos.Find(predicate1) == null)
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
								this._isPreviewDisabled = true;
								int count = this._grid.SelectedItems.Count - 1;
								this._lastSelectedIndex = this._grid.Items.IndexOf(this._grid.SelectedItems[count]);
								if (this._lastSelectedIndex < this._firstSelectedIndex)
								{
									int num = this._lastSelectedIndex;
									this._lastSelectedIndex = this._firstSelectedIndex;
									this._firstSelectedIndex = num;
								}
								for (int j = this._firstSelectedIndex; j <= this._lastSelectedIndex; j++)
								{
									NcPartInfo ncPartInfo = this._grid.Items[j] as NcPartInfo;
									if (ncPartInfo != null)
									{
										ncPartInfo.IsSelected = true;
										if (this._selectedItems.Contains(ncPartInfo))
										{
											this._selectedItems.Remove(ncPartInfo);
										}
										this._selectedItems.Add(ncPartInfo);
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
		}

		private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!EnumerableHelper.IsNullOrEmpty(e.AddedItems))
			{
				NcPartInfo item = e.AddedItems[0] as NcPartInfo;
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
			this._isDatabaseActive = this._dataRepository.IsDatabase;
			if (!this._isDatabaseActive)
			{
				mainWindow.BtnDelete.Visibility = Visibility.Hidden;
			}
			base.InitializeBase(mainWindow, typeof(NcPartInfo), AppConfiguration.Instance.DataListCaptions, "WiCAM.ArchivBrowser");
			this.DatabaseFilterChangedHandler = new Action<FilterConfiguration>(this.DatabaseFilterChanged);
			NcPartArchivController ncPartArchivController = this;
			this._mainWindow.WriteErg0Handler = new Action(ncPartArchivController.WriteErg0File);
			this._mainWindow.SelectedArchivChangedHandler = new Action<int>(this.SelectedArchiveChanged);
			NcPartArchivController ncPartArchivController1 = this;
			this._mainWindow.ConfigurationColumnsHandler = new Func<ObservableCollection<WpfGridColumnInfo>>(ncPartArchivController1.GridColumnsConfiguration);
			this._mainWindow.ApplyGridSettingsHandler = new Action<bool>(this.SaveGridColumnSettings);
			NcPartArchivController ncPartArchivController2 = this;
			this._mainWindow.GridSelectionHandler = new Action<bool>(ncPartArchivController2.ChangeSelection);
			NcPartArchivController ncPartArchivController3 = this;
			this._mainWindow.DeleteSelectedHandler = new Action(ncPartArchivController3.DeleteSelected);
			NcPartArchivController ncPartArchivController4 = this;
			this._mainWindow.ExportToCsvHandler = new Action(ncPartArchivController4.ExportToCsv);
			NcPartArchivController ncPartArchivController5 = this;
			this._mainWindow.InitializeMainWindowHandler = new Action(ncPartArchivController5.InitializeMainWindowToolbar);
			this._mainWindow.PreviewKeyUp += new KeyEventHandler(this.Shift_KeyUp);
			this._mainWindow.PreviewKeyDown += new KeyEventHandler(this.Shift_KeyDown);
			this._filterControl.FilterTextChanged += new RoutedEventHandler(this.FilterControl_FilterTextChanged);
			Style item = (Style)this._mainWindow.Resources["StyleRightAlignedCell"];
			this._gridController = new WpfDataGridController<NcPartInfo>(this._grid, "WiCAM.ArchivBrowser", AppConfiguration.Instance.ListConfiguration, null, item, null, AppArguments.Instance.IsMultiselect)
			{
				IsDragDropEnabled = false,
				HiddenColumns = "IsModified;LastModified;"
			};
			this._grid.SelectionChanged += new SelectionChangedEventHandler(this.Grid_SelectionChanged);
			this._grid.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.Grid_PreviewMouseDoubleClick);
			this._grid.MouseRightButtonUp += new MouseButtonEventHandler(this.Grid_MouseRightButtonUp);
			this._grid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.Grid_PreviewMouseLeftButtonDown);
			this._grid.PreviewKeyDown += new KeyEventHandler(this.Grid_PreviewKeyDown);
			base.InitializeFilters(typeof(NcPartInfo));
		}

		public void InitializeMainWindowToolbar()
		{
			base.InitializeFiltersToolbar(this._dataRepository.ConnectionString);
			this._dataRepository.ReadAsyncron(AppArguments.Instance.ArchiveNumber(), new Action(this.SetPartListData), this._mainWindow);
		}

		private void LoadCadgeo(NcPartInfo ncp)
		{
			if (this._isPreviewDisabled)
			{
				return;
			}
			string str = this._dataRepository.Path(ncp);
			string empty = string.Empty;
			ArchiveInfo archiveInfo = ArchiveStructureHelper.Instance.FindArchiveOrSubArchive(ncp.ArchiveNumber);
			if (archiveInfo != null)
			{
				empty = archiveInfo.FullName;
			}
			base.LoadCadgeoPreview(str, empty, ncp.PartName, ncp.DimensionX, ncp.DimensionY);
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

		private static void SearchStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private void SelectedAdd(NcPartInfo nci)
		{
			if (!this._selectedItems.Contains(nci))
			{
				this._selectedItems.Add(nci);
			}
		}

		private void SelectedArchiveChanged(int archivNumber)
		{
			this._dataRepository.ReadAsyncron(archivNumber, new Action(this.SetPartListData), this._mainWindow);
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ExpandSelected();
		}

		private void SelectedRemove(NcPartInfo nci)
		{
			if (this._selectedItems.Contains(nci))
			{
				this._selectedItems.Remove(nci);
			}
		}

		private void SetPartListData()
		{
			List<NcPartInfo> parts = this._dataRepository.Parts;
			if (this._mainWindow.FilterControl1.HasToBeFiltered)
			{
				this._dataRepository.ByFilter(this._archiveFilterCriteria.ListFilters);
				parts = this._dataRepository.ByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
			}
			this.SetPartListData(parts);
		}

		private void SetPartListData(List<NcPartInfo> list)
		{
			this.ArchiveContent.Clear();
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				foreach (NcPartInfo ncPartInfo in list)
				{
					base.ModifyDateValues(ncPartInfo);
					this.ArchiveContent.Add(ncPartInfo);
				}
			}
			_grid = MainWindow.Instance.GrdArchiveData;

            if (this._grid.SelectionMode == DataGridSelectionMode.Extended)
			{
				if (!EnumerableHelper.IsNullOrEmpty(this._selectedItems))
				{
					int num = 0;
					foreach (NcPartInfo _selectedItem in this._selectedItems)
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
			this.SetStatusText();
			string searchString = AppArguments.Instance.SearchString;
			if (!string.IsNullOrEmpty(searchString))
			{
				AppArguments.Instance.SearchString = string.Empty;
				this._filterControl.ChangeFilterValue("PartName", searchString);
				AppArguments.Instance.SearchString = string.Empty;
			}
			this._mainWindow.Cursor = Cursors.Arrow;
		}

		private void SetStatusText()
		{
			int count = 0;
			if (!EnumerableHelper.IsNullOrEmpty(this.ArchiveContent))
			{
				count = this.ArchiveContent.Count;
			}
			int num = 0;
			if (!EnumerableHelper.IsNullOrEmpty(this._dataRepository.Parts))
			{
				num = this._dataRepository.Parts.Count;
			}
			this._mainWindow.StatusText.Text = string.Format(CultureInfo.CurrentCulture, StringResourceHelper.Instance.FindString("MessageFilesAmount"), count, num);
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
				if (e.Key == Key.A)
				{
					if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
					{
						return;
					}
					if (this._isCtrlPressed)
					{
						this._selectedState = !this._selectedState;
						this.ChangeSelection(this._selectedState);
					}
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
			List<NcPartInfo> ncPartInfos;
			if (this._grid.SelectionMode != DataGridSelectionMode.Extended)
			{
				ncPartInfos = new List<NcPartInfo>();
				NcPartInfo selectedItem = this._grid.SelectedItem as NcPartInfo;
				if (selectedItem != null)
				{
					ncPartInfos.Add(selectedItem);
				}
			}
			else
			{
				List<NcPartInfo> ncPartInfos1 = new List<NcPartInfo>(this._selectedItems);
				this._selectedItems.Clear();
				foreach (object item in (IEnumerable)this._grid.Items)
				{
					NcPartInfo ncPartInfo = item as NcPartInfo;
					if (ncPartInfo == null || !ncPartInfo.IsSelected)
					{
						continue;
					}
					this._selectedItems.Add(ncPartInfo);
					if (!ncPartInfos1.Contains(ncPartInfo))
					{
						continue;
					}
					ncPartInfos1.Remove(ncPartInfo);
				}
				if (ncPartInfos1.Count > 0)
				{
					this._selectedItems.AddRange(ncPartInfos1);
				}
				ncPartInfos = this._selectedItems;
			}
			if (!EnumerableHelper.IsNullOrEmpty(ncPartInfos))
			{
				(new Erg0Helper()).WriteNcPartsErg0(ncPartInfos);
			}
		}
	}
}