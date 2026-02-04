using Microsoft.Office.Core;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.XObjects;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Autoloop;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Gmpool.Controls;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.Materials;
using WiCAM.Pn4000.WpfControls;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Page = UglyToad.PdfPig.Content.Page;

namespace WiCAM.Pn4000.Gmpool
{
	public class MaterialWindowViewModel : DependencyObject
	{
		public MainWindow _view;

		private WpfDataGridController<StockMaterialInfo> _gridHelper;

		public DataGrid _grid;

		public readonly FilterHelper<StockMaterialInfo> _filterHelper;

		public FilterCriteria _filterCriteria;

		private List<StockMaterialInfo> _materials;

		private List<StockMaterialInfo> _filteredByType;

		private GridSettingsEditViewModel _settingsModel;

		private ViewAction _viewAction;

		private Task<bool> _task;

		private static Action __materialSelectionChangedHandler;

		public readonly static DependencyProperty MaterialsCollectionProperty;

		public readonly static DependencyProperty SelectedMaterialProperty;

		public readonly static DependencyProperty IsNormalSelectedProperty;

		public readonly static DependencyProperty IsZuschnittSelectedProperty;

		public readonly static DependencyProperty IsRestPlateSelectedProperty;

		public readonly static DependencyProperty IsCoilSelectedProperty;

		public readonly static DependencyProperty IsBevorzugtSelectedProperty;

		public readonly static DependencyProperty ShowDeletedItemsProperty;

		public readonly static DependencyProperty ItemsTypeTextProperty;

		public readonly static DependencyProperty StatusTextProperty;

		public readonly static DependencyProperty DeletedColorProperty;

		public  static DependencyProperty ActiveViewProperty;

		public readonly static DependencyProperty AddRestPlateVisibilityProperty;

		public readonly static DependencyProperty AddButtonVisibilityProperty;

		public readonly static DependencyProperty RestoreButtonVisibilityProperty;

		private ICommand _enterCommand;

		private ICommand _okCommand;
        private ICommand _ReadPDFCommand;

        private ICommand _cancelCommand;

		private ICommand _copyCommand;

		private ICommand _addCommand;

		private ICommand _editCommand;

		private ICommand _settingsCommand;

		private ICommand _saveGmpoolCommand;

		private ICommand _saveCsvCommand;

		private ICommand _deleteCommand;

		private ICommand _showDeletedCommand;

		private ICommand _addRestPlateCommand;

		private ICommand _restorePlateCommand;
        private string _artikel;
        public bool savedData;
        private int imageIndex;
        private string amountline;
        public static MaterialWindowViewModel _MaterialWindowViewModel;


        public FrameworkElement ActiveView
		{
			get
			{
				return (FrameworkElement)base.GetValue(MaterialWindowViewModel.ActiveViewProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.ActiveViewProperty, value);
			}
		}

		public Visibility AddButtonVisibility
		{
			get
			{
				return (Visibility)base.GetValue(MaterialWindowViewModel.AddButtonVisibilityProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.AddButtonVisibilityProperty, value);
			}
		}

		public ICommand AddCommand
		{
			get
			{
				if (this._addCommand == null)
				{
					this._addCommand = new RelayCommand((object x) => this.Add(), (object x) => this.CanAdd());
				}
				return this._addCommand;
			}
		}

		public ICommand AddRestPlateCommand
		{
			get
			{
				if (this._addRestPlateCommand == null)
				{
					this._addRestPlateCommand = new RelayCommand((object x) => this.AddRestPlate(), (object x) => true);
				}
				return this._addRestPlateCommand;
			}
		}

		public Visibility AddRestPlateVisibility
		{
			get
			{
				return (Visibility)base.GetValue(MaterialWindowViewModel.AddRestPlateVisibilityProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.AddRestPlateVisibilityProperty, value);
			}
		}

		public bool AreButtonsEnabled
		{
			get;
			set;
		}

		public ICommand CancelCommand
		{
			get
			{
				if (this._cancelCommand == null)
				{
					this._cancelCommand = new RelayCommand((object x) => this.Cancel(), (object x) => true);
				}
				return this._cancelCommand;
			}
		}

		public ICommand CopyCommand
		{
			get
			{
				if (this._copyCommand == null)
				{
					this._copyCommand = new RelayCommand((object x) => this.Copy(), (object x) => this.CanCopy());
				}
				return this._copyCommand;
			}
		}

		public ICommand DeleteCommand
		{
			get
			{
				if (this._deleteCommand == null)
				{
					this._deleteCommand = new RelayCommand((object x) => this.Delete(), (object x) => this.CanDelete());
				}
				return this._deleteCommand;
			}
		}

		public Brush DeletedColor
		{
			get
			{
				return (Brush)base.GetValue(MaterialWindowViewModel.DeletedColorProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.DeletedColorProperty, value);
			}
		}

		public ICommand EditCommand
		{
			get
			{
				if (this._editCommand == null)
				{
					this._editCommand = new RelayCommand((object x) => this.Edit(ViewAction.Edit), (object x) => this.CanEdit());
				}
				return this._editCommand;
			}
		}

		public ICommand EnterCommand
		{
			get
			{
				if (this._enterCommand == null)
				{
					this._enterCommand = new RelayCommand((object x) => this.Enter(), (object x) => this.CanEnter());
				}
				return this._enterCommand;
			}
		}

		public bool IsBevorzugtSelected
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.IsBevorzugtSelectedProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.IsBevorzugtSelectedProperty, value);
			}
		}

		public bool IsCoilSelected
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.IsCoilSelectedProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.IsCoilSelectedProperty, value);
			}
		}

		public bool IsDatabaseActive
		{
			get;
			set;
		}

		public bool IsNormalSelected
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.IsNormalSelectedProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.IsNormalSelectedProperty, value);
			}
		}

		public bool IsRestPlateSelected
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.IsRestPlateSelectedProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.IsRestPlateSelectedProperty, value);
			}
		}

		public bool IsZuschnittSelected
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.IsZuschnittSelectedProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.IsZuschnittSelectedProperty, value);
			}
		}

		public string ItemsTypeText
		{
			get
			{
				return (string)base.GetValue(MaterialWindowViewModel.ItemsTypeTextProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.ItemsTypeTextProperty, value);
			}
		}

		public ObservableCollection<StockMaterialInfo> MaterialsCollection
		{
			get
			{
				return (ObservableCollection<StockMaterialInfo>)base.GetValue(MaterialWindowViewModel.MaterialsCollectionProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.MaterialsCollectionProperty, value);
			}
		}

		public ICommand OkCommand
		{
			get
			{
				if (this._okCommand == null)
				{
					this._okCommand = new RelayCommand((object x) => this.Ok(), (object x) => true);
				}
				return this._okCommand;
			}
		}
        public ICommand ReadPDFCommand
        {
            get
            {
                if (this._ReadPDFCommand == null)
                {
                    this._ReadPDFCommand = new RelayCommand((object x) => this.ReadPDF(), (object x) => true);
                }
                return this._ReadPDFCommand;
            }
        }

        public Visibility RestoreButtonVisibility
		{
			get
			{
				return (Visibility)base.GetValue(MaterialWindowViewModel.RestoreButtonVisibilityProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.RestoreButtonVisibilityProperty, value);
			}
		}

		public ICommand RestorePlateCommand
		{
			get
			{
				if (this._restorePlateCommand == null)
				{
					this._restorePlateCommand = new RelayCommand((object x) => this.RestorePlate(), (object x) => this.CanRestorePlate());
				}
				return this._restorePlateCommand;
			}
		}

		public ICommand SaveCsvCommand
		{
			get
			{
				if (this._saveCsvCommand == null)
				{
					this._saveCsvCommand = new RelayCommand((object x) => this.SaveCsv(), (object x) => this.CanSaveCsv());
				}
				return this._saveCsvCommand;
			}
		}

		public ICommand SaveGmpoolCommand
		{
			get
			{
				if (this._saveGmpoolCommand == null)
				{
					this._saveGmpoolCommand = new RelayCommand((object x) => this.SaveGmpool(), (object x) => this.CanSaveGmpool());
				}
				return this._saveGmpoolCommand;
			}
		}

		public StockMaterialInfo SelectedMaterial
		{
			get
			{
				return (StockMaterialInfo)base.GetValue(MaterialWindowViewModel.SelectedMaterialProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.SelectedMaterialProperty, value);
			}
		}

		public ICommand SettingsCommand
		{
			get
			{
				if (this._settingsCommand == null)
				{
					this._settingsCommand = new RelayCommand((object x) => this.Settings(), (object x) => this.CanShowSettings());
				}
				return this._settingsCommand;
			}
		}

		public ICommand ShowDeletedCommand
		{
			get
			{
				if (this._showDeletedCommand == null)
				{
					this._showDeletedCommand = new RelayCommand((object x) => this.ShowDeleted(), (object x) => this.CanShowDeleted());
				}
				return this._showDeletedCommand;
			}
		}

		public bool ShowDeletedItems
		{
			get
			{
				return (bool)base.GetValue(MaterialWindowViewModel.ShowDeletedItemsProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.ShowDeletedItemsProperty, value);
			}
		}

		public string StatusText
		{
			get
			{
				return (string)base.GetValue(MaterialWindowViewModel.StatusTextProperty);
			}
			set
			{
				base.SetValue(MaterialWindowViewModel.StatusTextProperty, value);
			}
		}

		static MaterialWindowViewModel()
		{
			MaterialWindowViewModel.MaterialsCollectionProperty = DependencyProperty.Register("MaterialsCollection", typeof(ObservableCollection<StockMaterialInfo>), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.SelectedMaterialProperty = DependencyProperty.Register("SelectedMaterial", typeof(StockMaterialInfo), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.IsNormalSelectedProperty = DependencyProperty.Register("IsNormalSelected", typeof(bool), typeof(MaterialWindowViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(MaterialWindowViewModel.TypeSelectionChanged)));
			MaterialWindowViewModel.IsZuschnittSelectedProperty = DependencyProperty.Register("IsZuschnittSelected", typeof(bool), typeof(MaterialWindowViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(MaterialWindowViewModel.TypeSelectionChanged)));
			MaterialWindowViewModel.IsRestPlateSelectedProperty = DependencyProperty.Register("IsRestPlateSelected", typeof(bool), typeof(MaterialWindowViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(MaterialWindowViewModel.TypeSelectionChanged)));
			MaterialWindowViewModel.IsCoilSelectedProperty = DependencyProperty.Register("IsCoilSelected", typeof(bool), typeof(MaterialWindowViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(MaterialWindowViewModel.TypeSelectionChanged)));
			MaterialWindowViewModel.IsBevorzugtSelectedProperty = DependencyProperty.Register("IsBevorzugtSelected", typeof(bool), typeof(MaterialWindowViewModel), new FrameworkPropertyMetadata(new PropertyChangedCallback(MaterialWindowViewModel.IsBevorzugtSelectedChanged)));
			MaterialWindowViewModel.ShowDeletedItemsProperty = DependencyProperty.Register("ShowDeletedItems", typeof(bool), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.ItemsTypeTextProperty = DependencyProperty.Register("ItemsTypeText", typeof(string), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.StatusTextProperty = DependencyProperty.Register("StatusText", typeof(string), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.DeletedColorProperty = DependencyProperty.Register("DeletedColor", typeof(Brush), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.ActiveViewProperty = DependencyProperty.Register("ActiveView", typeof(FrameworkElement), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.AddRestPlateVisibilityProperty = DependencyProperty.Register("AddRestPlateVisibility", typeof(Visibility), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.AddButtonVisibilityProperty = DependencyProperty.Register("AddButtonVisibility", typeof(Visibility), typeof(MaterialWindowViewModel));
			MaterialWindowViewModel.RestoreButtonVisibilityProperty = DependencyProperty.Register("RestoreButtonVisibility", typeof(Visibility), typeof(MaterialWindowViewModel));
		}

		public MaterialWindowViewModel()
		{
            _MaterialWindowViewModel = this;

            this.MaterialsCollection = new ObservableCollection<StockMaterialInfo>();
			this._filterHelper = new FilterHelper<StockMaterialInfo>();
			this._filteredByType = new List<StockMaterialInfo>();
			this.IsBevorzugtSelected = false;
			this.IsDatabaseActive = WiCAM.Pn4000.Materials.DataManager.Instance.IsInitialised;
			int num = 1;
			bool flagbool = false;
			if (num == 1)
				flagbool = true;
			bool flag = flagbool;// (bool)num;
			this.IsZuschnittSelected = flagbool; // (bool)num;
			bool flag1 = flag;
			bool flag2 = flag1;
			this.IsRestPlateSelected = flag1;
			bool flag3 = flag2;
			bool flag4 = flag3;
			this.IsNormalSelected = flag3;
			this.IsCoilSelected = flag4;
			this.RestoreButtonVisibility = Visibility.Collapsed;
            Logger.Info("MaterialWindowViewModel : {0}" + this, (object)DateTime.Now.ToString("s"));

        }

        private void Add()
		{
			if (this._gridHelper.SelectedItem != null)
			{
				this._grid.SelectedItems.Clear();
			}
			this._gridHelper.SelectedItem = new StockMaterialInfo();
			TypeInitializer.Initialize(this._gridHelper.SelectedItem);
			StockMaterialInfo.Initialize(this._gridHelper.SelectedItem, SystemConfiguration.UseInch);
			this.SelectedMaterial = this._gridHelper.SelectedItem;
			this.Edit(ViewAction.Create);
		}

		private void AddFoundByMaterialType(bool isSelected, int type)
		{
			if (isSelected)
			{
				List<StockMaterialInfo> stockMaterialInfos = this._materials.FindAll((StockMaterialInfo x) => x.PlTyp == type);
				if (!EnumerableHelper.IsNullOrEmpty(stockMaterialInfos))
				{
					this._filteredByType.AddRange(stockMaterialInfos);
				}
			}
		}

		private void AddRestPlate()
		{
			if (this._gridHelper.SelectedItem != null)
			{
				this._grid.SelectedItems.Clear();
			}
			this._gridHelper.SelectedItem = new StockMaterialInfo();
			TypeInitializer.Initialize(this._gridHelper.SelectedItem);
			StockMaterialInfo.Initialize(this._gridHelper.SelectedItem, SystemConfiguration.UseInch);
			this.SelectedMaterial = this._gridHelper.SelectedItem;
			StockMaterialViewModel selectedMaterial = (new StockMaterialConverter()).Convert(this.SelectedMaterial);
			selectedMaterial.OriginalMaterial = this.SelectedMaterial;
			RestPlateGeometrySelectionControl restPlateGeometrySelectionControl = new RestPlateGeometrySelectionControl()
			{
				DataContext = new RestPlateGeometrySelectViewModel(selectedMaterial, new Action<bool, RestPlateFormType>(this.ShowAddRestPlate))
			};
			this._grid.IsEnabled = false;
			this.ActiveView = restPlateGeometrySelectionControl;
		}

		private bool CanAdd()
		{
			return this.CheckCanPerformAction(null, string.Empty);
		}

		private void Cancel()
		{
		//	this._view.DialogResult = new bool?(false);
		//	this._view.Close();
		}

		private bool CanCopy()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnCopy);
		}

		private bool CanDelete()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnDelete);
		}

		private bool CanEdit()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnEdit);
		}

		private bool CanEnter()
		{
			return true;
		}

		private bool CanRestorePlate()
		{
			if (this.SelectedMaterial == null)
			{
				return false;
			}
			return this.SelectedMaterial.IsDeleted;
		}

		private bool CanSaveCsv()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnExcelXml);
		}

		private bool CanSaveGmpool()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnGmpool);
		}

		private bool CanShowDeleted()
		{
			return DataRepository.Instance.IsDatabase;
		}

		private bool CanShowSettings()
		{
			return this.CheckCanPerformAction(new Func<bool>(this.CheckMaterialIsSelected), ButtonsStateManager.BtnSettings);
		}

		private bool CheckCanPerformAction(Func<bool> checkAction, string buttonKey)
		{
			if (ApplicationConfigurationInfo.Instance.IsReadOnly())
			{
				return false;
			}
			if (!string.IsNullOrEmpty(buttonKey) && !ApplicationConfigurationInfo.Instance.ButtonsState.FindState(buttonKey))
			{
				return false;
			}
			bool areButtonsEnabled = this.AreButtonsEnabled;
			if (areButtonsEnabled && this._viewAction == ViewAction.NotDefined && checkAction != null)
			{
				areButtonsEnabled = checkAction();
			}
			return areButtonsEnabled;
		}

		private bool CheckMaterialIsSelected()
		{
			return this.SelectedMaterial != null;
		}

		private void CloseAction()
		{
			this.ActiveView = null;
			this._task = Task.Run<bool>(() => this.Enable(this._grid));
		}

		private void Copy()
		{
			StockMaterialInfo stockMaterialInfo = EnumerableHelper.CopyItem<StockMaterialInfo>(this._gridHelper.SelectedItem);
			stockMaterialInfo.Mpid = 0;
			string str = StringResourceHelper.Instance.FindString("CopyOf");
			stockMaterialInfo.PlName = string.Format(CultureInfo.CurrentCulture, "{0}{1}", str, this._gridHelper.SelectedItem.PlName);
			this._gridHelper.SelectedItem = stockMaterialInfo;
			this.SelectedMaterial = this._gridHelper.SelectedItem;
			this.Edit(ViewAction.Create);
		}

		private void Delete()
		{
			if (this._grid.SelectedItems.Count > 0)
			{
				if (ApplicationConfigurationInfo.Instance.IsFromPnControl && MessageHelper.AskDelete() != MessageBoxResult.OK)
				{
					return;
				}
				int count = this._grid.SelectedItems.Count - 1;
				while (count > -1)
				{
					int num = count;
					count = num - 1;
					this._gridHelper.SelectedItem = this._grid.SelectedItems[num] as StockMaterialInfo;
					if (this._gridHelper.SelectedItem == null)
					{
						continue;
					}
					if (this._gridHelper.SelectedItem.IsDeleted)
					{
						Logger.Verbose("Delete material");
						WiCAM.Pn4000.Materials.DataManager.Instance.DeleteMaterial(this._gridHelper.SelectedItem);
					}
					else
					{
						PlateLopInfo plateLopInfo = new PlateLopInfo()
						{
							PLATE_ACTION = "DELETE",
							PLATE_NAME = this._gridHelper.SelectedItem.PlName,
							PLATE_IDENT = this._gridHelper.SelectedItem.IdentNr
						};
						int arNumber = this._gridHelper.SelectedItem.ArNumber;
						plateLopInfo.PLATE_ARCHIV = arNumber.ToString();
						plateLopInfo.PLATE_DESCRIPTION = this._gridHelper.SelectedItem.Bezeichnung;
						arNumber = this._gridHelper.SelectedItem.Lagerplatz;
						plateLopInfo.PLATE_STORAGE_NO = arNumber.ToString();
						PlateLopInfo plateLopInfo1 = plateLopInfo;
						string str = CommonLopHelper.Instance.BuildPlateLopName(this._gridHelper.SelectedItem.PlName);
						Logger.Verbose(string.Concat("Delete MAT lop path=", str));
						PlateLopHelper.Instance.WritePlateLopFiles(plateLopInfo1, str);
					}
					this.MaterialsCollection.Remove(this._gridHelper.SelectedItem);
				}
			}
			this._gridHelper.SelectedItem = null;
			
        }

		private void Edit(ViewAction action)
		{
			if (this._task != null)
			{
				this._task.Dispose();
				this._task = null;
			}

			EditControl editControl = new EditControl()
			{
				DataContext = new EditControlViewModel(this.MaterialsCollection, this._materials, this.SelectedMaterial, action, () => this.CloseAction())
			};
			this._grid.IsEnabled = false;
			this.ActiveView = editControl;
			//MainWindow.Instance.xCControl = editControl;
			//	MainWindow.Instance.dockEdit = new DockPanel();
			MainWindow.Instance.dockEdit.Children.Clear();
               MainWindow.Instance.dockEdit.Children.Add(editControl);
			
            MainWindow.Instance.dockEdit.Visibility = Visibility.Visible;
        }

		private bool Enable(DataGrid grid)
		{
			Task.Delay(100).Wait();
			this.ToDispatcher(() => grid.IsEnabled = true);
			return true;
		}

		private void Enter()
		{
			if (ApplicationConfigurationInfo.Instance.IsFromPnControl)
			{
				return;
			}
			this.Ok();
		}

		private void FillList(IEnumerable<StockMaterialInfo> list)
		{
			this._view.gridData.BeginInit();
			this.MaterialsCollection.Clear();
			if (!EnumerableHelper.IsNullOrEmpty(list))
			{
				foreach (StockMaterialInfo stockMaterialInfo in list)
				{
					if (stockMaterialInfo.IsDeleted != this.ShowDeletedItems)
					{
						continue;
					}
					this.MaterialsCollection.Add(stockMaterialInfo);
				}
			}
			this._view.gridData.EndInit();
			if (!this.IsDatabaseActive)
			{
				this.StatusText = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", this.MaterialsCollection.Count, StringResourceHelper.Instance.FindString("StatusOf"), this._materials.Count);
				return;
			}
			int count = this._materials.FindAll((StockMaterialInfo x) => x.IsDeleted).Count;
			this.StatusText = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}   [{3}={4}]", new object[] { this.MaterialsCollection.Count, StringResourceHelper.Instance.FindString("StatusOf"), this._materials.Count, StringResourceHelper.Instance.FindString("StatusDeleted"), count });
		}

		private void FilterTextChanged(object sender, RoutedEventArgs e)
		{
			List<StockMaterialInfo> stockMaterialInfos = this._filterHelper.FilterAll(this._filteredByType, this._view.filter.DataFilterCriteria.ListFilters);
			this.FillList(stockMaterialInfos);
		}

		public void InitializeControl(Type itemType, List<CppConfigurationLineInfo> configuration)
		{
			this._view.filter.FieldNames.Clear();
			this._filterCriteria = (new FilterCriteria(WiCAM.Pn4000.Gmpool.CS.GmpoolBrowser, WiCAM.Pn4000.Gmpool.CS.MaterialsFilter)).ReadConfiguration();
			this._filterCriteria.DateFilterVisibility = Visibility.Hidden;
			int num = 0;
			foreach (CppConfigurationLineInfo cppConfigurationLineInfo in configuration)
			{
				DataGridColumn boundColumn = cppConfigurationLineInfo.BoundColumn;
				if (boundColumn == null)
				{
					continue;
				}
				string path = ((Binding)((DataGridBoundColumn)boundColumn).Binding).Path.Path;
				string str = StringResourceHelper.Instance.FindString(cppConfigurationLineInfo.Key);
			////	if (!this._view.filter.FieldNames.ContainsKey(path))
			////	{
			//		this._view.filter.FieldNames.Add(path, str);
			//	}
				if (boundColumn.Visibility != Visibility.Visible)
				{
					continue;
				}
				if (num < this._filterCriteria.ListFilters.Count && this._filterCriteria.ListFilters[num].FieldName == null)
				{
					this._filterCriteria.ListFilters[num].FieldName = path;
				}
				num++;
			}
		}

		private static void IsBevorzugtSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MaterialWindowViewModel mainWindowViewModel = d as MaterialWindowViewModel;
			if (mainWindowViewModel != null)
			{
				mainWindowViewModel.MaterialSelectionChanged();
			}
		}

		private void MaterialSelectionChanged()
		{
			Logger.Info("Materials selection changed");
			this._filteredByType.Clear();
			if (!this.IsNormalSelected || !this.IsZuschnittSelected || !this.IsRestPlateSelected || !this.IsCoilSelected)
			{
				this.AddFoundByMaterialType(this.IsNormalSelected, 1);
				this.AddFoundByMaterialType(this.IsZuschnittSelected, 2);
				this.AddFoundByMaterialType(this.IsRestPlateSelected, 3);
				this.AddFoundByMaterialType(this.IsCoilSelected, 4);
			}
			else
			{
				this._filteredByType.AddRange(this._materials);
			}
			if (this.IsBevorzugtSelected)
			{
				this._filteredByType = this._filteredByType.FindAll((StockMaterialInfo x) => x.Bevorzug.Equals(1));
			}
			this.FillList(this._filteredByType);
		}

		private void Ok()
		{
			if (!ApplicationConfigurationInfo.Instance.IsReadOnly() && !WiCAM.Pn4000.Materials.DataManager.Instance.WriteFile(this._materials))
			{
				Logger.Error("GMPOOL write error!");
			}
			this.WriteErg0File();

			//this._view.DialogResult = new bool?(true);
			//this._view.Close();
			Bitmap nitti = new Bitmap(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\palette.jpg");
			var text = GetText(nitti);
			Console.WriteLine(text);
        }
        private void ReadPDF()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Runny(filePath);

            }
        }

        public async Task Runny(string filePath)
        {
            using (var document = PdfDocument.Open(filePath))
            {
                imageIndex = 1;

                foreach (var page in document.GetPages())
                {

                    foreach (var image in page.GetImages())
                    {
                        savedData = false;
                        Add();
                        await waitAsync(image, page);
                        savedData = true;

                    }
                }
            }
        }

        public async Task waitAsync(IPdfImage image, Page page)
        {
            if (!image.TryGetBytes(out var b))
            {
                b = image.RawBytes;

            }
            var type = string.Empty;
            switch (image)
            {
                case XObjectImage ximg:
                    type = "XObject";
                    break;
                case InlineImage inline:
                    type = "Inline";
                    break;
            }
            image.TryGetPng(out byte[] bytess);
            Bitmap bitmapp = new Bitmap(new MemoryStream(bytess));
            bitmapp.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\DOC" + imageIndex + ".png");
            Console.WriteLine($"Image with {b.Count} bytes of type '{type}' on page {page.Number}. Location: {image.Bounds}.");
            var ocrtxt = GetText(bitmapp);
            string aLine, aParagraph = null;
            int iLine = 0;
            int iFound = 0;
            StringReader strReader = new StringReader(ocrtxt);
            while (true)
            {

                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    if (aLine.Contains("Menge ME 2"))
                    {
                        iFound = iLine;
                    }
                    if (iLine >= iFound + 1 && iLine <= iFound + 10)
                    {
                        aParagraph = aParagraph + aLine + " " + "\n";
                        if (aLine.Contains("S355MC"))
                        {
                            amountline = aLine;
                            EditControlViewModel._EditControlViewModel.SelectedItem.MatNumber = 5;
                            EditControlViewModel._EditControlViewModel.SelectedItem.MaterialName = "1.0355";
                        }
                        if (aLine.Contains(" mm "))
                            _artikel = aLine;
                        if (aLine.Contains("Mat"))
                            EditControlViewModel._EditControlViewModel.SelectedItem.Bezeichnung = aLine.Replace("MatNr.:", "");
                        if (aLine.Contains("Bestellnummer"))
                            EditControlViewModel._EditControlViewModel.SelectedItem.IdentNr = aLine.Replace("Bestellnummer:", "");
                        if (aLine.Contains("Lieferant"))
                            EditControlViewModel._EditControlViewModel.SelectedItem.IdentNr = EditControlViewModel._EditControlViewModel.SelectedItem.IdentNr + " - " + aLine.Replace("Lieferantencharge:", "");
                        if (aLine.Contains("Auftrag"))
                            EditControlViewModel._EditControlViewModel.SelectedItem.Bezeichnung = EditControlViewModel._EditControlViewModel.SelectedItem.Bezeichnung + " - " + aLine.Replace("für Auftrag:", "");
                        if (aLine.Contains("Gesamt"))
                        {
                            string[] splitweight = aLine.Split(' ');
                            EditControlViewModel._EditControlViewModel.SelectedItem.Res3 = splitweight[1].Replace(".", "");
                        }
                    }
                }
                else
                {
                    aParagraph = aParagraph + "\n";
                    break;
                }

                iLine++;
            }
            //EditControlViewModel._EditControlViewModel.SelectedItem.PlName = _artikel;
            EditControlViewModel._EditControlViewModel.SelectedItem.ArNumber = 90;
            double thick = Convert.ToDouble(_artikel.Remove(1));
            string[] splitartikel = _artikel.Split('x');

            foreach (var (item, index) in splitartikel.Select((item, index) => (item, index)))
            {
                if (index == 0)
                {
                    Console.WriteLine(item);
                    string item2 = item.Trim();
                    EditControlViewModel._EditControlViewModel.SelectedItem.PlName = item2;
                    EditControlViewModel._EditControlViewModel.SelectedItem.PlThick = item2;
                    //	EditControlViewModel._EditControlViewModel.SelectedItem.PlLength = item;
                }
                else if (index == 1)
                {
                    Console.WriteLine(item);
                    string item2 = item.Trim();
                    item2 = item2.Replace(".", "");
                    EditControlViewModel._EditControlViewModel.SelectedItem.PlName = item2 + "-" + EditControlViewModel._EditControlViewModel.SelectedItem.PlName;
                    EditControlViewModel._EditControlViewModel.SelectedItem.MaxY = item2;
                }
                else if (index == 2)
                {
                    Console.WriteLine(item.Remove(6));
                    string item2 = item.Remove(6);
                    item2 = item2.Trim();
                    item2 = item2.Replace(".", "");
                    EditControlViewModel._EditControlViewModel.SelectedItem.PlName = "N-" + item2 + "-" + EditControlViewModel._EditControlViewModel.SelectedItem.PlName;
                    EditControlViewModel._EditControlViewModel.SelectedItem.MaxX = item2;
                }
            }
            string date = DateTime.Now.ToString("yyyyMMdd");
            EditControlViewModel._EditControlViewModel.SelectedItem.ErstellungsDatum = int.Parse(date);
            Console.WriteLine(date);
            int amountpos = amountline.IndexOf("ST");
            string amount = amountline.Substring(amountpos - 3, 2);
            Console.WriteLine(amountpos);
            amount = amount.Trim();
            EditControlViewModel._EditControlViewModel.SelectedItem.Amount = int.Parse(amount);
            //   Ok();
            Console.WriteLine("Modified text:\n\n{0}", aParagraph);
            imageIndex++;

            await WaitForConditionAsync(() => savedData, CancellationToken.None);
        }
        public string GetText(Bitmap imgsource)
        {
            var ocrtext = string.Empty;
            using (var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "deu", (Tesseract.EngineMode)EngineMode.Default))
            {
                using (var img = PixConverter.ToPix(imgsource))
                {
                    using (var page = engine.Process(img))
                    {
                        ocrtext = page.GetText();
                    }
                }
            }
            return ocrtext;
        }
        private static async Task WaitForConditionAsync(Func<bool> condition, CancellationToken cancellationToken)
        {
            while (!condition())
            {
                Console.WriteLine("Waiting for condition to be true...");
                await Task.Delay(1000, cancellationToken); // Polling interval
            }
        }
        private void RestorePlate()
		{
			this.SelectedMaterial.IsDeleted = false;
			WiCAM.Pn4000.Materials.DataManager.Instance.UpdateMaterial(this.SelectedMaterial);
		}

		private void SaveConfiguration()
		{
			this._gridHelper.Save();
			this.UpdateFilterNames();
			IniFileHelper.Instance.WindowSize.ReadSettings(this._view);
			IniFileHelper.Instance.WriteIniFile();
		}

		private void SaveCsv()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				FileName = WiCAM.Pn4000.Materials.CS.GmpoolFileName,
				Filter = "XML-Files|*xml||",
				DefaultExt = WiCAM.Pn4000.Common.CS.ExtXml,
				AddExtension = true
			};
			if (saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				string fileName = saveFileDialog.FileName;
				string userName = Environment.UserName;
				ExcelXmlExportHelper.ToExcelXml<StockMaterialInfo>(this.MaterialsCollection, fileName, userName, string.Empty, false);
				Process.Start(fileName);
			}
		}

		private void SaveGmpool()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				FileName = WiCAM.Pn4000.Materials.CS.GmpoolFileName
			};
			if (saveFileDialog.ShowDialog().GetValueOrDefault(false))
			{
				string fileName = saveFileDialog.FileName;
				if (!(new StockMaterialFileHelper(null)).WriteFile(this.MaterialsCollection, fileName))
				{
					Logger.Error("GMPOOL can not be written");
				}
			}
		}

		private void SelectedMaterialChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (e.AddedItems.Count > 0)
				{
					StockMaterialInfo item = e.AddedItems[0] as StockMaterialInfo;
					if (item != null)
					{
						string empty = string.Empty;
						if (ApplicationConfigurationInfo.Instance.PlatesArchiv != null)
						{
							empty = ArchiveStructureHelper.Instance.PathByType(item.PlName, ApplicationConfigurationInfo.Instance.PlatesArchiv.Number, ArchiveFolderType.c2d);
						}
						if (!IOHelper.FileExists(empty))
						{
							empty = (new PlateCadgeoHelper()).WriteGeo(item);
							this._view.cadgeoViewer1.LoadNewCadGeo(empty);
						}
						else
						{
							this._view.cadgeoViewer1.LoadNewCadGeo(empty);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}

		private void Settings()
		{
			if (this._settingsModel == null)
			{
				this._settingsModel = new GridSettingsEditViewModel(this._view.dockParent, this._gridHelper.GridResources)
				{
					UpdateConfiguration = new Action(this.SaveConfiguration)
				};
			}
			this._view.dockParent.Visibility = Visibility.Visible;
			this._settingsModel.Show(IniFileHelper.Instance.MaterialsColumns);
		}

		public bool Show()
		{
			if (this._view == null)
			{
				this._view = MainWindow.Instance;  //new MainWindow();

                this._view.Loaded += new RoutedEventHandler(this.Window_Loaded);
				this._view.Closing += new CancelEventHandler(this.Window_Closing);
			//	this._view.DataContext = this;
				this._grid = this._view.gridData;
				this._view.filter.FilterTextChanged += new RoutedEventHandler(this.FilterTextChanged);
			}
			Style item = (Style)this._view.Resources["StyleRightAlignedCell"];
			this._gridHelper = new WpfDataGridController<StockMaterialInfo>(this._grid, "gmpool", IniFileHelper.Instance.MaterialsColumns, null, item, null, false)
			{
				SelectionChanged = new SelectionChangedEventHandler(this.SelectedMaterialChanged)
			};
			this.AddRestPlateVisibility = Visibility.Hidden;
			this.AddButtonVisibility = Visibility.Visible;

			return true; //this._view.ShowDialog().GetValueOrDefault(false);
		}

		private void ShowAddRestPlate(bool status, RestPlateFormType formType)
		{
			this.ActiveView = null;
			if (!status)
			{
				return;
			}
			AddRestPlateControl addRestPlateControl = new AddRestPlateControl()
			{
				DataContext = new AddRestPlateControlViewModel(this.MaterialsCollection, this._materials, this.SelectedMaterial, formType, () => this.CloseAction())
			};
			this._grid.IsEnabled = false;
			this.ActiveView = addRestPlateControl;
		}

		private void ShowDeleted()
		{
			this.ShowDeletedItems = !this.ShowDeletedItems;
			if (!this.ShowDeletedItems)
			{
				this.ItemsTypeText = string.Empty;
				this.DeletedColor = Brushes.Transparent;
				this.RestoreButtonVisibility = Visibility.Collapsed;
			}
			else
			{
				this.ItemsTypeText = StringResourceHelper.Instance.FindString("StatusDeleted");
				this.DeletedColor = Brushes.OrangeRed;
				this.RestoreButtonVisibility = Visibility.Visible;
			}
			List<StockMaterialInfo> stockMaterialInfos = this._filterHelper.FilterAll(this._filteredByType, this._view.filter.DataFilterCriteria.ListFilters);
			this.FillList(stockMaterialInfos);
		}

		private void ToDispatcher(Action action)
		{
			Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal, Array.Empty<object>());
		}

		private static void TypeSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (MaterialWindowViewModel.__materialSelectionChangedHandler != null)
			{
				MaterialWindowViewModel.__materialSelectionChangedHandler();
			}
		}

		private void UpdateFilterNames()
		{
		//	this._view.filter.InitializeControl(typeof(StockMaterialInfo), IniFileHelper.Instance.MaterialsColumns);
			this._view.filter.DataFilterCriteria = this._filterCriteria;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			this._filterCriteria.Save();
			this.SaveConfiguration();
		}

        
        private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			bool flag;
			bool flag1;
			int num;
			bool flag2;
			if (IniFileHelper.Instance.WindowSize != null)
			{
				IniFileHelper.Instance.WindowSize.Apply(this._view);
			}
			ResourceDictionary resourceDictionaries = StringResourceHelper.Instance.FindDictionary(SystemConfiguration.PnLanguage);
			if (resourceDictionaries != null)
			{
				this._view.Resources.MergedDictionaries.Add(resourceDictionaries);
				this._view.gridData.Resources.MergedDictionaries.Add(resourceDictionaries);
				this._view.filter.Resources.MergedDictionaries.Add(resourceDictionaries);
			}
			this._materials = DataRepository.Instance.Materials;
			if (EnumerableHelper.IsNullOrEmpty(this._materials))
			{
				this._materials = new List<StockMaterialInfo>();
			}
			if (ApplicationConfigurationInfo.Instance.WriteBackup)
			{
				(new BackupHelper()).WriteBackup(this._materials);
			}
			this.MaterialSelectionChanged();
			MaterialWindowViewModel.__materialSelectionChangedHandler = new Action(this.MaterialSelectionChanged);
			if (!EnumerableHelper.IsNullOrEmpty(IniFileHelper.Instance.MaterialsColumns))
			{
				this.InitializeControl(typeof(StockMaterialInfo), IniFileHelper.Instance.MaterialsColumns);
				this.UpdateFilterNames();
				this._view.filter.SetFocusOnFirstTextBox();
			}
			else
			{
				Logger.Error("PNGMCF.INI is not found or corrupt");
			}
			foreach (DataGridColumn column in this._grid.Columns)
			{
				if (string.IsNullOrEmpty(column.SortMemberPath) || !column.SortMemberPath.Equals("Bevorzug"))
				{
					continue;
				}
				DataGridBoundColumn dataGridBoundColumn = column as DataGridBoundColumn;
				if (dataGridBoundColumn == null)
				{
					break;
				}
				dataGridBoundColumn.Binding.StringFormat = "{0:0}";
				if (ApplicationConfigurationInfo.Instance.IsFromPnControl)
				{
					if (!string.IsNullOrEmpty(ApplicationConfigurationInfo.Instance.PlateName))
					{
						this._view.filter.SetFirstFieldValue(ApplicationConfigurationInfo.Instance.PlateName);
					}
					this.AddRestPlateVisibility = Visibility.Visible;
					this.AddButtonVisibility = Visibility.Collapsed;
					num = 0;
					bool flagbool = false;
					flag1 = flagbool; // (bool)num;
					this.IsZuschnittSelected = flagbool; // (bool)num;
					flag2 = flag1;
					flag = flag2;
					this.IsNormalSelected = flag2;
					this.IsCoilSelected = flag;
				}
				return;
			}
			if (ApplicationConfigurationInfo.Instance.IsFromPnControl)
			{
				if (!string.IsNullOrEmpty(ApplicationConfigurationInfo.Instance.PlateName))
				{
					this._view.filter.SetFirstFieldValue(ApplicationConfigurationInfo.Instance.PlateName);
				}
				this.AddRestPlateVisibility = Visibility.Visible;
				this.AddButtonVisibility = Visibility.Collapsed;
				num = 0;
                bool flagbool = false;
                flag1 = flagbool; // (bool)num;
                this.IsZuschnittSelected = flagbool; // (bool)num;
                flag2 = flag1;
				flag = flag2;
				this.IsNormalSelected = flag2;
				this.IsCoilSelected = flag;
			}
		}

		private void WriteErg0File()
		{
			if (this._gridHelper.SelectedItem != null)
			{
				WiCAM.Pn4000.Materials.DataManager.Instance.WriteErg0(this._gridHelper.SelectedItem);
				SerializeHelper.SerializeToXml<StockMaterialInfo>(this._gridHelper.SelectedItem, "SMPOOL.XML", false);
			}
		}
	}
}