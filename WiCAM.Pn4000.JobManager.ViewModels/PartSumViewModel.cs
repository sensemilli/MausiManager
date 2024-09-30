using System.Collections;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.ViewModels;

internal class PartSumViewModel : ViewModelBase
{
	private ICommand _okCommand;

	public int Amount { get; set; }

	public int AmountProduced { get; set; }

	public int AmountStorno { get; set; }

	public int AmountRejected { get; set; }

	public int AmountToProduce { get; set; }

	public ICommand OkCommand
	{
		get
		{
			if (_okCommand == null)
			{
				_okCommand = new RelayCommand(delegate(object x)
				{
					Ok(x);
				}, (object x) => true);
			}
			return _okCommand;
		}
	}

	private void Ok(object parameter)
	{
		if (parameter is Window window)
		{
			window.Close();
		}
	}

	public PartSumViewModel(IList listOfParts)
	{
		Amount = 0;
		AmountProduced = 0;
		AmountStorno = 0;
		AmountRejected = 0;
		foreach (object listOfPart in listOfParts)
		{
			PartInfo partInfo = listOfPart as PartInfo;
			Amount += partInfo.PART_ACOUNT;
			AmountProduced += partInfo.PART_PRODUCED;
			AmountStorno += partInfo.PART_STORNO;
			AmountRejected += partInfo.PART_REJECT;
		}
		AmountToProduce = Amount - AmountProduced + AmountRejected;
	}
}
