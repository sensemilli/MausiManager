using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	internal class MaterialSelectControlViewModel : ViewModelBase
	{
		private readonly WiCAM.Pn4000.Gmpool.MaterialSelectControl _view;

		private readonly Action _closeHandler;

		private readonly WpfDataGridHelper<MaterialArtInfo> _gridHelper;

		private readonly StockMaterialViewModel _material;

		private MaterialArtInfo _selected;

		private string _searchString;

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

		public ObservableCollection<MaterialArtInfo> MaterialArts
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
					this._okCommand = new RelayCommand((object x) => this.Ok(), (object x) => true);
				}
				return this._okCommand;
			}
		}

		public string SearchString
		{
			get
			{
				return this._searchString;
			}
			set
			{
				this._searchString = value;
				base.NotifyPropertyChanged("SearchString");
				this.Search(this._searchString);
			}
		}

		public MaterialArtInfo Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				if (this._selected != null)
				{
					this._selected.IsSelected = false;
				}
				this._selected = value;
				if (this._selected != null)
				{
					this._selected.IsSelected = true;
				}
				base.NotifyPropertyChanged("Selected");
			}
		}

		public MaterialSelectControlViewModel(StockMaterialViewModel material, WiCAM.Pn4000.Gmpool.MaterialSelectControl view, Action closeHandler)
		{
			this._view = view;
			this._closeHandler = closeHandler;
			if (!EnumerableHelper.IsNullOrEmpty(DataRepository.Instance.MaterialArts))
			{
				this.MaterialArts = new ObservableCollection<MaterialArtInfo>(DataRepository.Instance.MaterialArts);
				DataRepository.Instance.MaterialArts.ForEach((MaterialArtInfo x) => x.IsSelected = false);
			}
			try
			{
				this._material = material;
				this._view.Loaded += new RoutedEventHandler(this.View_Loaded);
				this._gridHelper = new WpfDataGridHelper<MaterialArtInfo>(view.gridData, IniFileHelper.Instance.MaterialArtsColumns)
				{
					DataGridRightAlignStyle = (Style)view.Resources["styleRightAlignedCell"]
				};
				this.Selected = DataRepository.Instance.FindMaterialArt(material.MatNumber);
				view.gridData.PreviewMouseDoubleClick += new MouseButtonEventHandler(this.GridData_PreviewMouseDoubleClick);
				view.gridData.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.GridData_PreviewMouseRightButtonUp);
				view.gridData.SelectedItem = this.Selected;
			}
			catch (Exception exception)
			{
				Logger.Exception(exception);
			}
		}

		private void Cancel()
		{
			Action action = this._closeHandler;
			if (action == null)
			{
				return;
			}
			action();
		}

		private string DoubleToString(double value)
		{
			return value.ToString(SystemConfiguration.WpfDoubleFormat, CultureInfo.InvariantCulture);
		}

		private void GridData_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			this.Ok();
		}

		private void GridData_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.Ok();
		}

		private void Ok()
		{
			if (this.Selected != null)
			{
				this._material.MatNumber = this.Selected.Number;
				this._material.MaterialName = this.Selected.Name;
				double num = StringHelper.ToDouble(this._material.PlThick);
				if (this.Selected.ThicknessMin > 0 && num < this.Selected.ThicknessMin)
				{
					this._material.PlThick = this.DoubleToString(this.Selected.ThicknessMin);
				}
				if (this.Selected.ThicknessMax > 0 && this.Selected.ThicknessMax < 9999 && num > this.Selected.ThicknessMax)
				{
					this._material.PlThick = this.DoubleToString(this.Selected.ThicknessMin);
				}
				Action action = this._closeHandler;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		private void Search(string searchString)
		{
			this.MaterialArts.Clear();
			if (string.IsNullOrEmpty(searchString))
			{
				List<MaterialArtInfo> materialArts = DataRepository.Instance.MaterialArts;
				ObservableCollection<MaterialArtInfo> observableCollection = this.MaterialArts;
				materialArts.ForEach(new Action<MaterialArtInfo>(observableCollection.Add));
				return;
			}
			List<MaterialArtInfo> materialArtInfos = (new ListFilter<MaterialArtInfo>()).Filter(DataRepository.Instance.MaterialArts, "Name", searchString);
			if (materialArtInfos.Count > 0)
			{
				ObservableCollection<MaterialArtInfo> materialArts1 = this.MaterialArts;
				materialArtInfos.ForEach(new Action<MaterialArtInfo>(materialArts1.Add));
			}
		}

		private void View_Loaded(object sender, RoutedEventArgs e)
		{
			int num = 0;
			foreach (object item in (IEnumerable)this._view.gridData.Items)
			{
				if (item != this.Selected)
				{
					num++;
				}
				else
				{
					this._view.gridData.SelectedItem = item;
					this._view.gridData.Focus();
					DataGridRow dataGridRow = this._view.gridData.ItemContainerGenerator.ContainerFromIndex(num) as DataGridRow;
					if (dataGridRow == null)
					{
						this._view.gridData.ScrollIntoView(item);
						dataGridRow = this._view.gridData.ItemContainerGenerator.ContainerFromIndex(num) as DataGridRow;
					}
					dataGridRow.IsSelected = true;
					return;
				}
			}
		}
	}
}