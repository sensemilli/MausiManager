using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Gmpool.Controls
{
	public partial class RestPlateGeometrySelectionControl : UserControl
	{
		public RestPlateGeometrySelectionControl()
		{
			this.InitializeComponent();
		}

		public void TextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (textBox != null)
			{
				textBox.SelectAll();
			}
		}

		public void TextBox_LostFocusDouble(object sender, RoutedEventArgs e)
		{
			TextBox s0 = sender as TextBox;
			if (s0 != null)
			{
				if (string.IsNullOrEmpty(s0.Text))
				{
					s0.Text = WiCAM.Pn4000.Common.CS.S0;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(s0.Text);
				stringBuilder.Replace(",", ".");
				if (stringBuilder[stringBuilder.Length - 1] == '.')
				{
					stringBuilder.Append("0");
				}
				s0.Text = string.Format(CultureInfo.InvariantCulture, SystemConfiguration.WpfFormatDouble, StringHelper.ToDouble(stringBuilder.ToString()));
			}
		}
	}
}