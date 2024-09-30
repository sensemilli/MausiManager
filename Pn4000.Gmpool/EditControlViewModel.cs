using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Autoloop;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Gmpool.Controls;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	public class EditControlViewModel : Common.ViewModelBase
    {
		private readonly ObservableCollection<StockMaterialInfo> _materialsCollection;

		private readonly List<StockMaterialInfo> _materialsList;

		private readonly ViewAction _action;

		private readonly Action _closeAction;

		private readonly StockMaterialDatabaseHelper _dbHelper;

		private readonly StockMaterialInfo _originalItem;

		private Collection<PlateTypeInfo> _plateTypes;

		private FrameworkElement _activeView;

		private StockMaterialViewModel _selectedItem;

		private ICommand _selectMaterialCommand;

		private ICommand _selectMachinesCommand;

		private ICommand _okCommand;

		private ICommand _cancelCommand;

		public FrameworkElement ActiveView
		{
			get
			{
				return this._activeView;
			}
			set
			{
				this._activeView = value;
				base.NotifyPropertyChanged("ActiveView");
			}
		}

		public ICommand CancelCommand
		{
			get
			{
				if (this._cancelCommand == null)
				{
					this._cancelCommand = new RelayCommand((object x) => this.Cancel(), (object x) => this.CanCancel());
				}
				return this._cancelCommand;
			}
		}

		public string DoubleFormat
		{
			get;
			set;
		}

		public ICommand OkCommand
		{
			get
			{
				if (this._okCommand == null)
				{
					this._okCommand = new RelayCommand((object x) => this.Ok(), (object x) => this.CanOkCommand());
				}
				return this._okCommand;
			}
		}

		public Collection<PlateTypeInfo> PlateTypes
		{
			get
			{
				return this._plateTypes;
			}
			set
			{
				this._plateTypes = value;
				base.NotifyPropertyChanged("PlateTypes");
			}
		}

		public StockMaterialViewModel SelectedItem
		{
			get
			{
				return this._selectedItem;
			}
			set
			{
				this._selectedItem = value;
				base.NotifyPropertyChanged("SelectedItem");
			}
		}

		public ICommand SelectMachinesCommand
		{
			get
			{
				if (this._selectMachinesCommand == null)
				{
					this._selectMachinesCommand = new RelayCommand((object x) => this.SelectMachines(), (object x) => true);
				}
				return this._selectMachinesCommand;
			}
		}

		public ICommand SelectMaterialCommand
		{
			get
			{
				if (this._selectMaterialCommand == null)
				{
					this._selectMaterialCommand = new RelayCommand((object x) => this.SelectMaterial(), (object x) => true);
				}
				return this._selectMaterialCommand;
			}
		}

		public EditControlViewModel(ObservableCollection<StockMaterialInfo> materials, List<StockMaterialInfo> materialsList, StockMaterialInfo plate, ViewAction action, Action closeAction)
		{
			this._materialsCollection = materials;
			this._materialsList = materialsList;
			this._action = action;
			this._closeAction = closeAction;
			this._plateTypes = PlateTypeInfo.Types();
			foreach (PlateTypeInfo _plateType in this._plateTypes)
			{
				StringResourceHelper instance = StringResourceHelper.Instance;
				StockMaterialType type = _plateType.Type;
				_plateType.Text = instance.FindString(type.ToString());
			}
			this.DoubleFormat = SystemConfiguration.WpfFormatDouble;
			if (ApplicationConfigurationInfo.Instance.TracingEnabled)
			{
				DataManager.Instance.ShowTracing();
			}
			if (this._dbHelper == null)
			{
				this._dbHelper = DataManager.Instance.MaterialsHelper as StockMaterialDatabaseHelper;
			}
			if (string.IsNullOrEmpty(plate.MachineNummer))
			{
				plate.MachineNummer = "0;";
			}
			this._originalItem = plate;
			plate.MaterialName = this.FindMaterialName(plate.MatNumber);
			this.SelectedItem = (new StockMaterialConverter()).Convert(plate);
			this.SelectedItem.OriginalMaterial = plate;
		}

		private bool CanCancel()
		{
			return true;
		}

		private void Cancel()
		{
			this._originalItem.Locked = 0;
			this._originalItem.PcName = string.Empty;
			if (!DataManager.Instance.UpdateMaterial(this._originalItem))
			{
				Logger.Error("Problems saving material {0}", new object[] { this._originalItem.PlName });
			}
			Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();
            MainWindow.Instance.dockEdit.Visibility = Visibility.Hidden;

        }

        private bool CanOkCommand()
		{
			return true;
		}

		private StockMaterialInfo CheckHasDifferences(StockMaterialViewModel model)
		{
			bool flag;
			if (this._dbHelper == null)
			{
				return null;
			}
			if (model.Mpid == 0)
			{
				return null;
			}
			StockMaterialInfo stockMaterialInfo = this._dbHelper.StockMaterialSelectOne(model.Mpid);
			DateTime? modifiedDate = stockMaterialInfo.ModifiedDate;
			if (!modifiedDate.HasValue)
			{
				return null;
			}
			modifiedDate = stockMaterialInfo.ModifiedDate;
			DateTime dateTime = model.ModifiedDate;
			if (modifiedDate.HasValue)
			{
				flag = (modifiedDate.HasValue ? modifiedDate.GetValueOrDefault() != dateTime : false);
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				return stockMaterialInfo;
			}
			return null;
		}

		private void ControlPlateData(StockMaterialInfo plate)
		{
			plate.ControlMachinesString();
			if (string.IsNullOrEmpty(plate.MachineNummer))
			{
				plate.MachineNummer = "0;";
			}
			plate.Locked = 0;
			plate.PcName = Environment.MachineName;
			plate.DeleteDate = DateTime.Today;
			plate.ModifiedDate = new DateTime?(DateTime.Now);
		}

		private bool DeleteItem(StockMaterialInfo plate)
		{
			DataManager.Instance.WriteLopFile(plate, LopType.Delete);
			return true;
		}

		private string FindMaterialName(int materialNumber)
		{
			MaterialArtInfo materialArtInfo = DataRepository.Instance.MaterialArts.Find((MaterialArtInfo x) => x.Number == materialNumber);
			if (materialArtInfo == null)
			{
				return string.Empty;
			}
			return materialArtInfo.Name;
		}

		private bool InsertItem(StockMaterialInfo plate)
		{
			this.ControlPlateData(plate);
			plate.CreationDate = DateTime.Now;
			EnumerableHelper.UpdateItem(this._originalItem, plate);
			DataRepository.Instance.CalculateValues(this._originalItem);
			if (DataManager.Instance.MaterialsHelper is StockMaterialDatabaseHelper && this._dbHelper != null && !string.IsNullOrEmpty(this._dbHelper.Filter))
			{
				this._originalItem.Filter = this._dbHelper.Filter;
			}
			if (!DataManager.Instance.InsertMaterial(this._originalItem))
			{
				Logger.Error("Problems inserting material {0}", new object[] { plate.PlName });
				return false;
			}
			StockMaterialInfo stockMaterialInfo = DataManager.Instance.SelectMaterial(this._originalItem);
			if (stockMaterialInfo != null)
			{
				if (stockMaterialInfo.Mpid != this._originalItem.Mpid)
				{
					this._originalItem.Mpid = stockMaterialInfo.Mpid;
				}
				this._materialsCollection.Insert(0, this._originalItem);
				this._materialsList.Insert(0, this._originalItem);
			}
			return true;
		}

		private bool IsEqual(StockMaterialInfo p1, StockMaterialInfo p2)
		{
			if (p1.Mpid == p2.Mpid)
			{
				return false;
			}
			return p1.IsEqual(p2);
		}

		private bool ItemAlreadyExist(StockMaterialInfo plate)
		{
			return this._materialsList.Find((StockMaterialInfo x) => this.IsEqual(x, plate)) != null;
		}

		private void MaterialSelectClosed()
		{
			this.ActiveView = null;
		}

		private void Ok()
		{
			StockMaterialConverter stockMaterialConverter = new StockMaterialConverter();
			StockMaterialInfo stockMaterialInfo = this.CheckHasDifferences(this.SelectedItem);
			if (stockMaterialInfo != null)
			{
				StockMaterialInfo stockMaterialInfo1 = new StockMaterialInfo();
				stockMaterialConverter.ConvertBack(this.SelectedItem, stockMaterialInfo1);
				List<CompareResult> compareResults = (new ItemsComparer()).Compare(stockMaterialInfo1, stockMaterialInfo);
				if (compareResults.Count > 0)
				{
					if (!(new ShowModifiedWindow()
					{
						DataContext = new ShowModifiedWindowViewModel(stockMaterialInfo1, compareResults)
					}).ShowDialog().GetValueOrDefault(false))
					{
						return;
					}
					StockMaterialViewModel stockMaterialViewModel = (new StockMaterialConverter()).Convert(stockMaterialInfo1);
					EnumerableHelper.UpdateItem(this.SelectedItem, stockMaterialViewModel);
				}
			}
			StockMaterialInfo stockMaterialInfo2 = new StockMaterialInfo();
			stockMaterialConverter.ConvertBack(this.SelectedItem, stockMaterialInfo2);
			if (this.ItemAlreadyExist(stockMaterialInfo2))
			{
				MessageHelper.Stop(StringResourceHelper.Instance.FindString(WiCAM.Pn4000.Gmpool.CS.MsgItemAlreadyExist));
				return;
			}
			stockMaterialConverter.ConvertBack(this.SelectedItem, this._originalItem);
			ViewAction viewAction = this._action;
			if (viewAction == ViewAction.Create)
			{
				this.InsertItem(this._originalItem);
			}
			else if (viewAction == ViewAction.Edit)
			{
				this.SaveItem(this._originalItem);
			}
            MainWindow.Instance.dockEdit.Visibility = Visibility.Hidden;

            Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();

        }

        private bool SaveItem(StockMaterialInfo plate)
		{
			this.ControlPlateData(plate);
			if (!DataManager.Instance.UpdateMaterial(this._originalItem))
			{
				Logger.Error("Problems saving material {0}", new object[] { plate.PlName });
			}
			return true;
		}

		private void SelectMachines()
		{
			MachinesSelectControl machinesSelectControl = new MachinesSelectControl();
			MachinesSelectControlViewModel machinesSelectControlViewModel = new MachinesSelectControlViewModel(this.SelectedItem, () => this.ActiveView = null);
			machinesSelectControl.DataContext = machinesSelectControlViewModel;
			this.ActiveView = machinesSelectControl;
		}

		private void SelectMaterial()
		{
			WiCAM.Pn4000.Gmpool.MaterialSelectControl materialSelectControl = new WiCAM.Pn4000.Gmpool.MaterialSelectControl();
			MaterialSelectControlViewModel materialSelectControlViewModel = new MaterialSelectControlViewModel(this.SelectedItem, materialSelectControl, new Action(this.MaterialSelectClosed));
			materialSelectControl.DataContext = materialSelectControlViewModel;
			this.ActiveView = materialSelectControl;
		}
	}
}