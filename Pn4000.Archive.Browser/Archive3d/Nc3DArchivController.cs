using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Archive.Browser.ArchiveCad;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archive.Browser.Controllers;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Archives.Cad;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.PN3D.Doc.Serializer;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.WpfControls;

namespace WiCAM.Pn4000.Archive.Browser.Archive3d
{
	internal class Nc3DArchivController : ArchiveControllerBase, IArchivController
	{
		private static Nc3DArchivController _instance;

		private readonly static string __hiddenColumnNames;

		public readonly static DependencyProperty ArchiveContentCollectionProperty;

		private List<CadPartInfo> _allItems;

		private readonly List<CadPartInfo> _selectedItems;

		private WpfDataGridController<CadPartInfo> _gridController;

		private bool _selectedState;

		private readonly Nc3dDataRepository _repository;

		public ObservableCollection<CadPartInfo> ArchiveContent
		{
			get
			{
				return (ObservableCollection<CadPartInfo>)base.GetValue(Nc3DArchivController.ArchiveContentCollectionProperty);
			}
			set
			{
				base.SetValue(Nc3DArchivController.ArchiveContentCollectionProperty, value);
			}
		}

		public static Nc3DArchivController Instance
		{
			get
			{
				if (Nc3DArchivController._instance == null)
				{
					Logger.Verbose("Initialize Nc3DArchivController");
					Nc3DArchivController._instance = new Nc3DArchivController();
				}
				return Nc3DArchivController._instance;
			}
		}

		static Nc3DArchivController()
		{
			Nc3DArchivController.__hiddenColumnNames = "Rowid;";
			Nc3DArchivController.ArchiveContentCollectionProperty = DependencyProperty.Register("ArchiveContentCollection", typeof(ObservableCollection<CadPartInfo>), typeof(CadPartArchiveController));
		}

		private Nc3DArchivController()
		{
			this._repository = new Nc3dDataRepository();
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

		public void DeleteSelected()
		{
		}

		public void ExportToCsv()
		{
			ExcelCsvExportHelper.ToCsvFromWpfDataGrid<CadPartInfo>(this.ArchiveContent, this._grid, "geo3dArchive.csv");
			ProcessHelper.ExecuteNoWait("geo3dArchive.csv", string.Empty);
		}

		private void FilterControl_FilterTextChanged(object sender, RoutedEventArgs e)
		{
			this._repository.ByFilter(this._archiveFilterCriteria.ListFilters);
			List<CadPartInfo> cadPartInfos = this._repository.ByDateInterval(this._archiveFilterCriteria.DateFrom, this._archiveFilterCriteria.DateTill);
			this.SetPartListData(cadPartInfos);
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
							for (int j = this._firstSelectedIndex; j <= this._lastSelectedIndex; j++)
							{
								CadPartInfo cadPartInfo = this._grid.Items[j] as CadPartInfo;
								cadPartInfo.IsSelected = true;
								if (!this._selectedItems.Contains(cadPartInfo))
								{
									this._selectedItems.Add(cadPartInfo);
								}
								this._grid.SelectedItems.Add(this._grid.Items[j]);
							}
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
			base.InitializeBase(mainWindow, typeof(CadPartInfo), AppConfiguration.Instance.DataListCaptions, WiCAM.Pn4000.Archive.Browser.Classes.CS.Geo3DFilter);
			Nc3DArchivController nc3DArchivController = this;
			this._mainWindow.WriteErg0Handler = new Action(nc3DArchivController.WriteErg0File);
			this._mainWindow.SelectedArchivChangedHandler = new Action<int>(this.SelectedArchiveChanged);
			Nc3DArchivController nc3DArchivController1 = this;
			this._mainWindow.ConfigurationColumnsHandler = new Func<ObservableCollection<WpfGridColumnInfo>>(nc3DArchivController1.GridColumnsConfiguration);
			this._mainWindow.ApplyGridSettingsHandler = new Action<bool>(this.SaveGridColumnSettings);
			Nc3DArchivController nc3DArchivController2 = this;
			this._mainWindow.GridSelectionHandler = new Action<bool>(nc3DArchivController2.ChangeSelection);
			Nc3DArchivController nc3DArchivController3 = this;
			this._mainWindow.DeleteSelectedHandler = new Action(nc3DArchivController3.DeleteSelected);
			Nc3DArchivController nc3DArchivController4 = this;
			this._mainWindow.ExportToCsvHandler = new Action(nc3DArchivController4.ExportToCsv);
			Nc3DArchivController nc3DArchivController5 = this;
			this._mainWindow.InitializeMainWindowHandler = new Action(nc3DArchivController5.InitializeMainWindowToolbar);
			this._mainWindow.PreviewKeyUp += new KeyEventHandler(this.Shift_KeyUp);
			this._mainWindow.PreviewKeyDown += new KeyEventHandler(this.Shift_KeyDown);
			this._mainWindow.BtnDelete.Visibility = Visibility.Hidden;
			this._grid.Name = "geo3dGrid";
			Style item = (Style)this._mainWindow.Resources["styleRightAlignedCell"];
			this._gridController = new WpfDataGridController<CadPartInfo>(this._grid, "WiCAM.ArchivBrowser", AppConfiguration.Instance.ListConfiguration, null, item, null, AppArguments.Instance.IsMultiselect)
			{
				HiddenColumns = Nc3DArchivController.__hiddenColumnNames,
				IsDragDropEnabled = false
			};
			this._grid.SelectionChanged += new SelectionChangedEventHandler(this.Grid_SelectionChanged);
			this._grid.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.Grid_PreviewMouseDoubleClick);
			this._grid.MouseRightButtonUp += new MouseButtonEventHandler(this.Grid_MouseRightButtonUp);
			this._grid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.Grid_PreviewMouseLeftButtonDown);
			this._grid.PreviewKeyDown += new KeyEventHandler(this.Grid_PreviewKeyDown);
			this._filterControl.FilterTextChanged += new RoutedEventHandler(this.FilterControl_FilterTextChanged);
		}

		public void InitializeMainWindowToolbar()
		{
			base.InitializeFiltersToolbar(this._repository.ConnectionString);
			this._repository.ReadAsyncron(AppArguments.Instance.ArchiveNumber(), new Action(this.SetPartListData));
		}

		private void LoadCadgeo(CadPartInfo part)
		{
			try
			{
				ArchiveInfo archiveInfo = ArchiveStructureHelper.Instance.FindArchiveOrSubArchive(part.ArchiveNumber);
				string empty = string.Empty;
				if (archiveInfo != null)
				{
					empty = archiveInfo.Name;
				}
				string str = Path.Combine(archiveInfo.Paths.Find((ArchivePathInfo x) => x.FolderType == ArchiveFolderType.n3d).Path, Path.ChangeExtension(part.PartName, part.Extension));
				bool flag = IOHelper.FileExists(str);
				base.UpdateStatusText(flag, part.PartName, empty);
				this._mainWindow.TxtN3dMeasures.Text = string.Empty;
			//	this._mainWindow.Geometry3dViewer.ScreenD3D.RemoveModel(null, false);
				Pair<Vector3d, Vector3d> boundary = null;
				if (flag)
				{
					string upper = part.Extension.ToUpper();
					if (upper == ".C3DO")
					{
				//		Model model = DocSerializer.DeserializeGeometry(str);
					//	this._mainWindow.Geometry3dViewer.ScreenD3D.AddModel(model, false);
					//	boundary = model.GetBoundary(Matrix4d.Identity, false);
					}
					else if (upper == ".C3MO")
					{
				//		Model model1 = ModelSerializer.Deserialize(str);
				//		this._mainWindow.Geometry3dViewer.ScreenD3D.AddModel(model1, false);
					//	boundary = model1.GetBoundary(Matrix4d.Identity, false);
					}
					else
					{
						return;
					}
					if (boundary != null)
					{
				//		this._mainWindow.Geometry3dViewer.ScreenD3D.ZoomExtend(true);
						double num = boundary.Item1.X;
						Vector3d item2 = boundary.Item2;
						double num1 = Math.Abs(num - item2.X);
						double y = boundary.Item1.Y;
						item2 = boundary.Item2;
						double num2 = Math.Abs(y - item2.Y);
						double z = boundary.Item1.Z;
						item2 = boundary.Item2;
						double num3 = Math.Abs(z - item2.Z);
						this._mainWindow.TxtN3dMeasures.Text = string.Format(CultureInfo.InvariantCulture, "X={0:0.000}   Y={1:0.000}   Z={2:0.000} ", num1, num2, num3);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
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
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = archivNumber;
			WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ExpandSelected();
		}

		private void SetPartListData()
		{
			List<CadPartInfo> parts = this._repository.Parts;
			List<CadPartInfo> cadPartInfos = parts;
			this._allItems = parts;
			this.SetPartListData(cadPartInfos);
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
			(new Erg0Helper()).Write3DPartsErg0(cadPartInfos);
		}
	}
}