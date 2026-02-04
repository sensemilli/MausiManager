using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WiCAM.Pn4000.PN3D.Popup.UI;

public class SelectAllFocusBehavior
{
	public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached("Enable", typeof(bool), typeof(SelectAllFocusBehavior), new FrameworkPropertyMetadata(false, OnEnableChanged));

	public static bool GetEnable(FrameworkElement frameworkElement)
	{
		return (bool)frameworkElement.GetValue(SelectAllFocusBehavior.EnableProperty);
	}

	public static void SetEnable(FrameworkElement frameworkElement, bool value)
	{
		frameworkElement.SetValue(SelectAllFocusBehavior.EnableProperty, value);
	}

	private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement frameworkElement && e.NewValue is bool)
		{
			if ((bool)e.NewValue)
			{
				frameworkElement.GotFocus += SelectAll;
				frameworkElement.PreviewMouseDown += IgnoreMouseButton;
			}
			else
			{
				frameworkElement.GotFocus -= SelectAll;
				frameworkElement.PreviewMouseDown -= IgnoreMouseButton;
			}
		}
	}

	private static void SelectAll(object sender, RoutedEventArgs e)
	{
		FrameworkElement frameworkElement = e.OriginalSource as FrameworkElement;
		if (!(frameworkElement is TextBox textBox))
		{
			if (frameworkElement is PasswordBox passwordBox)
			{
				passwordBox.SelectAll();
			}
		}
		else
		{
			textBox.SelectAll();
		}
	}

	private static void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
	{
		if (sender is FrameworkElement { IsKeyboardFocusWithin: false } frameworkElement)
		{
			e.Handled = true;
			frameworkElement.Focus();
		}
	}
}
