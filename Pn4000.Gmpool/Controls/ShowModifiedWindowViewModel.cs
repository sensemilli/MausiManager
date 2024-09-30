using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Controls
{
	public class ShowModifiedWindowViewModel : ViewModelBase
	{
		private readonly StockMaterialInfo _material;

		private ICommand _okCommand;

		private ICommand _cancelCommand;

		public ICommand CancelCommand
		{
			get
			{
				if (this._cancelCommand == null)
				{
					this._cancelCommand = new RelayCommand((object x) => this.Cancel(x), (object x) => this.CanCancel());
				}
				return this._cancelCommand;
			}
		}

		public ObservableCollection<CompareResult> Compare { get; set; } = new ObservableCollection<CompareResult>();

		public string LocalMachineName { get; set; } = Environment.MachineName;

        private StockAuftragInfo _Auftrag;

        public string ModifiedMachineName
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
					this._okCommand = new RelayCommand((object x) => this.Ok(x), (object x) => this.CanOk());
				}
				return this._okCommand;
			}
		}

		public ShowModifiedWindowViewModel(StockMaterialInfo material, List<CompareResult> differences)
		{
			this._material = material;
			this.ModifiedMachineName = this._material.PcName;
			ObservableCollection<CompareResult> compare = this.Compare;
			differences.ForEach(new Action<CompareResult>(compare.Add));
		}

        public ShowModifiedWindowViewModel(StockAuftragInfo auftrag, List<CompareResult> differences)
        {
            this._Auftrag = auftrag;
            this.ModifiedMachineName = this._Auftrag.PcName;
            ObservableCollection<CompareResult> compare = this.Compare;
            differences.ForEach(new Action<CompareResult>(compare.Add));
        }

        private bool CanCancel()
		{
			return true;
		}

		private void Cancel(object view)
		{
			Window window = view as Window;
			if (window == null)
			{
				return;
			}
			window.Close();
		}

		private bool CanOk()
		{
			return this.Compare.Count > 0;
		}

		private void Ok(object view)
		{
			foreach (CompareResult compare in this.Compare)
			{
				compare.OriginalProperty.SetValue(this._material, compare.ReadValue());
			}
			Window nullable = view as Window;
			if (nullable != null)
			{
				nullable.DialogResult = new bool?(true);
				nullable.Close();
			}
		}
	}
}