using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Information;

public static class NumberOnlyBehaviour
{
	public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(NumberOnlyBehaviour), new UIPropertyMetadata(false, OnValueChanged));

	public static bool GetIsEnabled(Control o)
	{
		return (bool)o.GetValue(NumberOnlyBehaviour.IsEnabledProperty);
	}

	public static void SetIsEnabled(Control o, bool value)
	{
		o.SetValue(NumberOnlyBehaviour.IsEnabledProperty, value);
	}

	private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
	{
		if (dependencyObject is Control control)
		{
			if (e.NewValue is bool && (bool)e.NewValue)
			{
				control.PreviewTextInput += OnTextInput;
				control.PreviewKeyDown += OnPreviewKeyDown;
				DataObject.AddPastingHandler(control, OnPaste);
			}
			else
			{
				control.PreviewTextInput -= OnTextInput;
				control.PreviewKeyDown -= OnPreviewKeyDown;
				DataObject.RemovePastingHandler(control, OnPaste);
			}
		}
	}

	private static void OnTextInput(object sender, TextCompositionEventArgs e)
	{
		if (e.Text.Any((char c) => !char.IsDigit(c)))
		{
			e.Handled = true;
		}
	}

	private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Space)
		{
			e.Handled = true;
		}
	}

	private static void OnPaste(object sender, DataObjectPastingEventArgs e)
	{
		if (e.DataObject.GetDataPresent(DataFormats.Text))
		{
			if (Convert.ToString(e.DataObject.GetData(DataFormats.Text)).Trim().Any((char c) => !char.IsDigit(c)))
			{
				e.CancelCommand();
			}
		}
		else
		{
			e.CancelCommand();
		}
	}
}
