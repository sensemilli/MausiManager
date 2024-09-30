using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Gmpool.Controls;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	public class AddRestPlateControlViewModel : ViewModelBase
	{
		private readonly ObservableCollection<StockMaterialInfo> _materialsCollection;

		private readonly List<StockMaterialInfo> _materialsList;

		private readonly Action _closeAction;

		private readonly StockMaterialDatabaseHelper _dbHelper;

		private readonly StockMaterialInfo _originalItem;

		private readonly RestPlateFormType _formType;

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

		public AddRestPlateControlViewModel(ObservableCollection<StockMaterialInfo> materials, List<StockMaterialInfo> materialsList, StockMaterialInfo plate, RestPlateFormType formType, Action closeAction)
		{
			this._materialsCollection = materials;
			this._materialsList = materialsList;
			this._closeAction = closeAction;
			this._formType = formType;
			this._plateTypes = PlateTypeInfo.RestTypes();
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
			string str = string.Concat(new object[] { "R_", plate.MaxX, "_", plate.MaxY });
			List<StockMaterialInfo> stockMaterialInfos = this._materialsList.FindAll((StockMaterialInfo x) => x.PlName.StartsWith(str));
			int count = 1;
			if (stockMaterialInfos.Count > 0)
			{
				count = stockMaterialInfos.Count + 1;
			}
			plate.PlName = string.Concat(str, "_", count.ToString());
			plate.PlTyp = 3;
			plate.Amount = 1;
			plate.ArNumber = 90;
			plate.CreationDate = DateTime.Now;
			DateTime today = DateTime.Today;
			plate.ErstellungsDatum = StringHelper.ToInt(today.ToString("yyyyMMdd"));
			plate.MaterialName = this.FindMaterialName(plate.MatNumber);
			plate.ReferenceNr = ApplicationConfigurationInfo.Instance.NcNumber;
			string[] userName = new string[2];
			today = DateTime.Today;
			userName[0] = today.ToString("yyyyMMdd");
			userName[1] = ApplicationConfigurationInfo.Instance.UserName;
			plate.Bemerk2 = string.Join("; ", userName);
			plate.MachineNummer = ApplicationConfigurationInfo.Instance.RestMachineNumber;
			this._originalItem = plate;
			this.SelectedItem = (new StockMaterialConverter()).Convert(plate);
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
			bool flag = this.ItemAlreadyExist(plate);
			if (flag)
			{
				MessageHelper.Stop(StringResourceHelper.Instance.FindString(WiCAM.Pn4000.Gmpool.CS.MsgItemAlreadyExist));
			}
			else
			{
				this.ControlPlateData(plate);
				plate.CreationDate = DateTime.Now;
				EnumerableHelper.UpdateItem(this._originalItem, plate);
				DataRepository.Instance.CalculateValues(this._originalItem);
				if (DataManager.Instance.MaterialsHelper is StockMaterialDatabaseHelper && this._dbHelper != null && !string.IsNullOrEmpty(this._dbHelper.Filter))
				{
					this._originalItem.Filter = this._dbHelper.Filter;
				}
				if (DataManager.Instance.InsertMaterial(this._originalItem))
				{
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
				}
				else
				{
					Logger.Error("Problems inserting material {0}", new object[] { plate.PlName });
				}
			}
			return flag;
		}

		private bool ItemAlreadyExist(StockMaterialInfo plate)
		{
			return this._materialsList.ToList<StockMaterialInfo>().Find((StockMaterialInfo x) => x.IsEqual(plate)) != null;
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
			stockMaterialConverter.ConvertBack(this.SelectedItem, this._originalItem);
			if (this.InsertItem(this._originalItem))
			{
				return;
			}
			if (this._formType != RestPlateFormType.NotDefined)
			{
				try
				{
					(new CadGeoCreator(this._formType, this._originalItem)).CreateCadGeo();
				}
				catch (Exception exception)
				{
					Logger.Exception(exception);
				}
			}
			Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();
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