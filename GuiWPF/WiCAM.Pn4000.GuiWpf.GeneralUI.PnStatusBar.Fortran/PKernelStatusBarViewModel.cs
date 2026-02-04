using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public class PKernelStatusBarViewModel : IPKernelStatusBarViewModel, INotifyPropertyChanged
{
	private IPKernelStatusBarModel _model;

	public ObservableCollection<IPKernelStatusBarVisualData> VisualData { get; }

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public PKernelStatusBarViewModel(IPKernelStatusBarModel model, IPnStatusBarHelper pnStatusBarHelper)
	{
		_model = model;
		VisualData = new ObservableCollection<IPKernelStatusBarVisualData>(from x in _model.GetData()
			select new PKernelStatusBarVisualData(x));
		UpdateViewModel();
		model.OnStatusChanged += PnStatusBarHelper_OnStatusBarModelUpdate;
	}

	private void PnStatusBarHelper_OnStatusBarModelUpdate()
	{
		UpdateViewModel();
	}

	private void UpdateViewModel()
	{
		foreach (IPKernelStatusBarVisualData visualDatum in VisualData)
		{
			visualDatum.UpdateData();
		}
	}
}
