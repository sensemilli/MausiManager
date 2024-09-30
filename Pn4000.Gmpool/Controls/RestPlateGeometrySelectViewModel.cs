using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Controls
{
	public class RestPlateGeometrySelectViewModel : ViewModelBase
	{
		private StockMaterialViewModel _plate;

		private readonly Action<bool, RestPlateFormType> _closeAction;

		private RestPlateFormType _formType;

		private bool _dimensionYEnabled;

		private ICommand _okCommand;

		private ICommand _cancelCommand;

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

		public bool DimensionYEnabled
		{
			get
			{
				return this._dimensionYEnabled;
			}
			set
			{
				this._dimensionYEnabled = value;
				base.NotifyPropertyChanged("DimensionYEnabled");
			}
		}

		public RestPlateFormType FormType
		{
			get
			{
				return this._formType;
			}
			set
			{
				this._formType = value;
				this.DimensionYEnabled = this._formType.Equals(RestPlateFormType.Rectangle);
				base.NotifyPropertyChanged("FormType");
			}
		}

		public ICommand OkCommand
		{
			get
			{
				if (this._okCommand == null)
				{
					this._okCommand = new RelayCommand((object x) => this.Ok(), (object x) => this.CanOk());
				}
				return this._okCommand;
			}
		}

		public StockMaterialViewModel SelectedItem
		{
			get
			{
				return this._plate;
			}
			set
			{
				this._plate = value;
				base.NotifyPropertyChanged("SelectedItem");
			}
		}

		public RestPlateGeometrySelectViewModel(StockMaterialViewModel plate, Action<bool, RestPlateFormType> closeAction)
		{
			this._plate = plate;
			this._closeAction = closeAction;
			this.FormType = RestPlateFormType.Rectangle;
		}

		private void Cancel()
		{
			Action<bool, RestPlateFormType> action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action(false, 0);
		}

		private bool CanOk()
		{
			return this.FormType != RestPlateFormType.NotDefined;
		}

		private void Ok()
		{
			if (this._formType == RestPlateFormType.Circle)
			{
				this._plate.MaxY = this._plate.MaxX;
			}
			(new StockMaterialConverter()).ConvertBack(this._plate, this._plate.OriginalMaterial);
			Action<bool, RestPlateFormType> action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action(true, this._formType);
		}
	}
}