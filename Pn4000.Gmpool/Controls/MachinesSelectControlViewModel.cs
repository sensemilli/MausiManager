using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Machine;

namespace WiCAM.Pn4000.Gmpool.Controls
{
	public class MachinesSelectControlViewModel : ViewModelBase
	{
		private readonly StockMaterialViewModel _material;

		private readonly Action _closeAction;

		private readonly List<MachineInfo> _machines;

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

		public ObservableCollection<MachineInfo> MachinesCollection { get; set; } = new ObservableCollection<MachineInfo>();

		public string MachinesString
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

		public MachinesSelectControlViewModel(StockMaterialViewModel material, Action closeAction)
		{
			this._material = material;
			this._closeAction = closeAction;
			this._machines = PnMachineHelper.ReadAll();
			if (EnumerableHelper.IsNullOrEmpty(this._machines))
			{
				this._machines = new List<MachineInfo>();
			}
			List<MachineInfo> machineInfos = this._machines;
			ObservableCollection<MachineInfo> machinesCollection = this.MachinesCollection;
			machineInfos.ForEach(new Action<MachineInfo>(machinesCollection.Add));
			this.MachinesString = material.MachineNummer.Trim();
			if (!this.MachinesString.Equals("0;", StringComparison.CurrentCultureIgnoreCase))
			{
				string[] strArrays = this.MachinesString.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					MachineInfo machineInfo = this._machines.Find((MachineInfo x) => x.Number == StringHelper.ToInt(str));
					if (machineInfo != null)
					{
						machineInfo.IsVisible = true;
					}
				}
			}
		}

		private void Cancel()
		{
			Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();
		}

		private void Ok()
		{
			StringBuilder stringBuilder = new StringBuilder(80);
			foreach (MachineInfo machinesCollection in this.MachinesCollection)
			{
				if (!machinesCollection.IsVisible)
				{
					continue;
				}
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0};", machinesCollection.Number);
			}
			string str = stringBuilder.ToString();
			if (str.Length == 0)
			{
				str = "0;";
			}
			this.MachinesString = str;
			this._material.MachineNummer = this.MachinesString;
			Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();
		}
	}
}