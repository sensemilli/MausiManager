using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	public class CompareResult : ViewModelBase
	{
		private bool _selectFirstValue;

		private object _firstValue;

		private bool _selectSecondValue;

		private object _secondValue;

		public object FirstValue
		{
			get
			{
				return this._firstValue;
			}
			set
			{
				this._firstValue = value;
				base.NotifyPropertyChanged("FirstValue");
			}
		}

		public PropertyInfo ModifiedProperty
		{
			get;
			set;
		}

		public PropertyInfo OriginalProperty
		{
			get;
			set;
		}

		public object SecondValue
		{
			get
			{
				return this._secondValue;
			}
			set
			{
				this._secondValue = value;
				base.NotifyPropertyChanged("SecondValue");
			}
		}

		public bool SelectFirstValue
		{
			get
			{
				return this._selectFirstValue;
			}
			set
			{
				this._selectFirstValue = value;
				base.NotifyPropertyChanged("SelectFirstValue");
			}
		}

		public bool SelectSecondValue
		{
			get
			{
				return this._selectSecondValue;
			}
			set
			{
				this._selectSecondValue = value;
				this.SelectFirstValue = !this._selectSecondValue;
				base.NotifyPropertyChanged("SelectSecondValue");
			}
		}

		public CompareResult()
		{
		}

		public object ReadValue()
		{
			if (this.SelectFirstValue)
			{
				return this.FirstValue;
			}
			return this.SecondValue;
		}
	}
}