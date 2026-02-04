using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Wpf.UnitConversion;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.PN3D.Popup.ViewModel;

public class PopupSelectMaterialViewModel : PopupViewModelBase
{
	private readonly PopupSelectMaterialModel _selectedMaterialModel;

	private ObservableCollection<MaterialViewModel> _materials;

	private readonly CollectionView _itemsView;

	private MaterialViewModel _selectedMaterial;

	private double _thicknessDouble;

	private string _thicknessFilter;

	private ICommand _selectDoubleClick;

	private ICommand _filterClickCommand;

	private Action<PopupSelectMaterialViewModel> _closeAction;

	private RecentlyUsedRecord _recentlyUsedMaterial;

	public IMaterialArt SelectedMaterialOfModel => this._selectedMaterialModel.SelectedMaterial;

	public ObservableCollection<MaterialViewModel> Materials
	{
		get
		{
			return this._materials;
		}
		set
		{
			this._materials = value;
			this.OnPropertyChanged("Materials");
		}
	}

	public MaterialViewModel InitialSelectedMaterial { get; private set; }

	public MaterialViewModel SelectedMaterial
	{
		get
		{
			return this._selectedMaterial;
		}
		set
		{
			this._selectedMaterial = value;
			this._selectedMaterialModel.SelectedMaterial = value?.Material;
			this.OnPropertyChanged("SelectedMaterial");
		}
	}

	public string ThicknessFilter
	{
		get
		{
			return this._thicknessFilter;
		}
		set
		{
			this._thicknessFilter = value;
			double.TryParse(this._thicknessFilter, NumberStyles.Any, CultureInfo.InvariantCulture, out this._thicknessDouble);
			this._itemsView.Filter = ItemFilter;
			this._itemsView.Refresh();
			this.OnPropertyChanged("ThicknessFilter");
			this.OnPropertyChanged("ThicknessFilterDouble");
		}
	}

	public double ThicknessFilterDouble => this._thicknessDouble;

	public ICommand SelectDoubleClick => this._selectDoubleClick ?? (this._selectDoubleClick = new global::WiCAM.Pn4000.Common.RelayCommand((Action<object>)delegate
	{
		this.CloseLikeOk();
	}));

	public ICommand FilterClickCommand => this._filterClickCommand ?? (this._filterClickCommand = new global::WiCAM.Pn4000.Common.RelayCommand<object>(delegate
	{
		this.ResetFilter();
	}));

	public PopupSelectMaterialViewModel(PopupSelectMaterialModel selectMaterialModel, IMainWindowDataProvider mainWindowDataProvider)
	{
		this._selectedMaterialModel = selectMaterialModel;
		this._recentlyUsedMaterial = mainWindowDataProvider.GetRecentlyUsedMaterialRecords().FirstOrDefault();
		this.Materials = new ObservableCollection<MaterialViewModel>();
		foreach (IMaterialArt material in this._selectedMaterialModel.Materials)
		{
			this.Materials.Add(new MaterialViewModel(material, this._selectedMaterialModel.GlobalMaterials));
		}
		this._itemsView = (CollectionView)CollectionViewSource.GetDefaultView(this.Materials);
	}

	public void Init(IDoc3d doc, Action<PopupSelectMaterialViewModel> closeAction)
	{
		this._selectedMaterialModel.Init(doc);
		this._closeAction = closeAction;
		this.SelectedMaterial = this.Materials.FirstOrDefault((MaterialViewModel m) => m.Number == this._selectedMaterialModel.ActualMaterialID);
		if (this.SelectedMaterial == null)
		{
			this.SelectedMaterial = this.Materials.FirstOrDefault((MaterialViewModel m) => m.Number == this._recentlyUsedMaterial?.ArchiveID);
			if (this.SelectedMaterial == null)
			{
				MaterialViewModel materialViewModel2 = (this.SelectedMaterial = this.Materials.FirstOrDefault());
			}
		}
		this.InitialSelectedMaterial = this.SelectedMaterial;
		this.ThicknessFilter = new InchConversion(Math.Round(Math.Max(this._selectedMaterialModel.Thickness, 0.0), 2)).Converted.ToString(CultureInfo.InvariantCulture);
		this._itemsView.Filter = ItemFilter;
		this._itemsView.Refresh();
		base.Button0_DeleteVisibility = Visibility.Collapsed;
		base.Button1_EditVisibility = Visibility.Collapsed;
		base.Button2_AddVisibility = Visibility.Collapsed;
		base.Button3_BackVisibility = Visibility.Collapsed;
		base.Button4_SwapVisibility = Visibility.Collapsed;
		base.Button5_CancelVisibility = Visibility.Visible;
		base.Button6_MarkVisibility = Visibility.Collapsed;
		base.Button7_LearnVisibility = Visibility.Collapsed;
		base.Button8_CopyVisibility = Visibility.Collapsed;
		base.Button9_ListVisibility = Visibility.Collapsed;
		base.Button10_GraphicsVisibility = Visibility.Collapsed;
		base.Button11_LoadingVisibility = Visibility.Collapsed;
		base.Button12_Certain1Visibility = Visibility.Collapsed;
		base.Button13_Certain2Visibility = Visibility.Collapsed;
		base.Button14_Certain3Visibility = Visibility.Collapsed;
		base.Button15_PrintVisibility = Visibility.Visible;
		base.Button16_OkVisibility = Visibility.Visible;
		base.Button5_CancelClick = new global::WiCAM.Pn4000.Common.RelayCommand<object>(CancelClick, CanCancelClick);
		base.Button15_PrintClick = new global::WiCAM.Pn4000.ScreenD3D.Controls.Base.RelayCommand<object>(PrintClick, CanPrintClick);
		base.Button16_OkClick = new global::WiCAM.Pn4000.Common.RelayCommand<object>(OkClick, CanOkClick);
	}

	private bool ItemFilter(object obj)
	{
		MaterialViewModel materialViewModel = (MaterialViewModel)obj;
		bool num = !string.IsNullOrEmpty(this.ThicknessFilter);
		bool flag = false;
		if (num && this._thicknessDouble >= materialViewModel.ThicknessMin && this._thicknessDouble <= materialViewModel.ThicknessMax)
		{
			flag = true;
		}
		bool flag2 = true;
		if (num)
		{
			flag2 = flag2 && flag;
		}
		return flag2;
	}

	private void ResetFilter()
	{
		this.ThicknessFilter = new InchConversion(Math.Round(this._selectedMaterialModel.Thickness, 2)).Converted.ToString(CultureInfo.InvariantCulture);
	}

	private void CancelClick(object obj)
	{
		this.SelectedMaterial = this.InitialSelectedMaterial;
		this.CloseLikeCancel();
	}

	private void PrintClick(object obj)
	{
        var stringBuilder = new StringBuilder(50000);

        foreach (var material in Materials)
        {
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                "{0,-25} {1,8} {2,8} {3,8:0.000} {4,8:0.000} {5,8:0.000} {6,8:0.000} " +
                "{7,8:0.000} {8,8:0.000} {9,8:0.000} {10,8:0.000} {11,30}",
                material.Name,
                material.Number,
                material.MaterialGroupId,
                material.Density,
                material.ThicknessMin,
                material.ThicknessMax,
                material.YieldStrength,
                material.TensileStrength,
                material.HeatCapacity,
                material.WorkHardeningExponent,
                material.EModul,
                material.Description);

            stringBuilder.AppendLine();
        }
		IOHelper.FileWriteAllText("DRPLOT.TXT", stringBuilder.ToString());
		string text = PnPathBuilder.PathInPnDrive("\\u\\pn\\bin\\popprint.bat");
		if (!IOHelper.FileExists(text))
		{
			return;
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo(text, "1 DRPLOT.TXT")
		{
			WindowStyle = ProcessWindowStyle.Hidden
		};
		using Process process = new Process();
		Logger.Verbose("Process : {0}", processStartInfo.FileName);
		Logger.Verbose("Args    : {0}", processStartInfo.Arguments);
		process.StartInfo = processStartInfo;
		process.Start();
	}

	private void OkClick(object obj)
	{
		this.CloseLikeOk();
	}

	private static bool CanCancelClick(object obj)
	{
		return true;
	}

	private static bool CanPrintClick(object obj)
	{
		return true;
	}

	private static bool CanOkClick(object obj)
	{
		return true;
	}

	private void CloseLikeOk()
	{
		this._selectedMaterialModel.AcceptMaterialAsUser();
		this._closeAction?.Invoke(this);
		base.CloseView();
	}

	private void CloseLikeCancel()
	{
		this._closeAction?.Invoke(this);
		base.CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3)
		{
			this.CloseLikeOk();
		}
	}
}
