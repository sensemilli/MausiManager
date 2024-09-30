using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class SelectedTextBoxBackgroundConverter : IValueConverter
	{
		public Brush Active
		{
			get;
			set;
		}

		public Brush Normal
		{
			get;
			set;
		}

		public SelectedTextBoxBackgroundConverter()
		{
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool && (bool)value)
			{
				return this.Active;
			}
			return this.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}