using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class Calculator : Window, IComponentConnector
{
	private bool isErrorText;

	private LikeModalMode likeModalMode;

	private IConfigProvider _configProvider;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private Brush RedBrush = new SolidColorBrush(Colors.Red);

	private Brush SilverBrush = new SolidColorBrush(Colors.Silver);

	private string uiSep;

	public string CalcRetValue { get; set; }

	public Calculator(IConfigProvider configProvider, IMainWindowDataProvider mainWindowDataProvider)
	{
		_configProvider = configProvider;
		_mainWindowDataProvider = mainWindowDataProvider;
		likeModalMode = _mainWindowDataProvider.GetLikeModalMode();
		InitializeComponent();
		CalcRetValue = $"{0.0:0.0}";
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		base.Width = generalUserSettingsConfig.CalculatorWidth;
		base.Height = generalUserSettingsConfig.CalculatorHeight;
		base.Left = generalUserSettingsConfig.CalculatorLeft;
		base.Top = generalUserSettingsConfig.CalculatorTop;
	}

	public void Init(string button_value, string label)
	{
		uiSep = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
		separatorButton.Content = uiSep;
		label1.Content = label;
		button1.Content = FormatValue(button_value);
		input.Content = string.Empty;
		Show();
		Activate();
		likeModalMode.SetMode(EndLikeModalMode);
		likeModalMode.WaitEndModal();
	}

	private bool EndLikeModalMode()
	{
		bool result = true;
		if (base.Visibility == Visibility.Visible)
		{
			result = InterpretingEnter();
		}
		return result;
	}

	private void SetError(bool error)
	{
		isErrorText = error;
		border1.BorderBrush = (error ? RedBrush : SilverBrush);
	}

	private string FormatValue(string curr)
	{
		try
		{
			curr = curr.Replace(',', '.');
			string result = Math.Round(Convert.ToDouble(new DataTable().Compute(curr, null)), 5).ToString();
			border1.BorderBrush = new SolidColorBrush(Colors.Silver);
			SetError(error: false);
			return result;
		}
		catch (Exception)
		{
			SetError(error: true);
			return curr;
		}
	}

	private void button_Click(object sender, RoutedEventArgs e)
	{
		string text = ((Button)sender).Content.ToString();
		switch (text)
		{
		case "=":
			InterpretingEnter();
			return;
		case "<-":
			InterpretingBackSpace();
			return;
		case "CE":
			InterpretingErase();
			return;
		}
		string text2 = input.Content.ToString();
		text2 += text;
		input.Content = text2;
		CalculateAndSetFontSize();
	}

	private void InterpretingErase()
	{
		input.Content = string.Empty;
	}

	private void InterpretingBackSpace()
	{
		string text = input.Content.ToString();
		if (!(text == string.Empty))
		{
			input.Content = text.Substring(0, text.Length - 1);
			CalculateAndSetFontSize();
		}
	}

	private void CalculateAndSetFontSize()
	{
		if (!(input.Content.ToString() == string.Empty))
		{
			input.FontSize = 32.0;
			input.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			while (input.FontSize > 8.0 && input.DesiredSize.Width > base.Width - 20.0)
			{
				input.FontSize -= 2.0;
				input.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			}
		}
	}

	private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		InterpretingEnter();
	}

	public bool InterpretingEnter()
	{
		string text = input.Content.ToString();
		if (text != string.Empty)
		{
			string text2 = FormatValue(text);
			if (text != text2 || isErrorText)
			{
				input.Content = text2;
				CalculateAndSetFontSize();
				return false;
			}
		}
		if (base.Visibility == Visibility.Visible)
		{
			Close();
			return true;
		}
		string text3 = input.Content.ToString();
		if (text3 == string.Empty)
		{
			text3 = "0.0";
		}
		CalcRetValue = text3;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.CalculatorWidth = (int)base.Width;
		generalUserSettingsConfig.CalculatorHeight = (int)base.Height;
		generalUserSettingsConfig.CalculatorLeft = (int)base.Left;
		generalUserSettingsConfig.CalculatorTop = (int)base.Top;
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
		likeModalMode.ResetLikeModalMode();
		return true;
	}

	private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		switch (e.Key)
		{
		case Key.Return:
			InterpretingEnter();
			e.Handled = true;
			break;
		case Key.Back:
			InterpretingBackSpace();
			e.Handled = true;
			break;
		case Key.Escape:
			input.Content = "-999999";
			InterpretingEnter();
			e.Handled = true;
			break;
		case Key.V:
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && Clipboard.ContainsText())
			{
				string curr = Clipboard.GetData(DataFormats.Text) as string;
				string text = FormatValue(curr);
				string text2 = input.Content.ToString();
				input.Content = text2 + text;
				CalculateAndSetFontSize();
				e.Handled = true;
			}
			break;
		case Key.C:
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				Clipboard.Clear();
				Clipboard.SetText(input.Content.ToString());
				e.Handled = true;
			}
			break;
		}
	}

	private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		string text = input.Content.ToString();
		switch (e.Text)
		{
		case "0":
			input.Content = text + "0";
			break;
		case "1":
			input.Content = text + "1";
			break;
		case "2":
			input.Content = text + "2";
			break;
		case "3":
			input.Content = text + "3";
			break;
		case "4":
			input.Content = text + "4";
			break;
		case "5":
			input.Content = text + "5";
			break;
		case "6":
			input.Content = text + "6";
			break;
		case "7":
			input.Content = text + "7";
			break;
		case "8":
			input.Content = text + "8";
			break;
		case "9":
			input.Content = text + "9";
			break;
		case "+":
			input.Content = text + "+";
			break;
		case "-":
			input.Content = text + "-";
			break;
		case "*":
			input.Content = text + "*";
			break;
		case "/":
			input.Content = text + "/";
			break;
		case "(":
			input.Content = text + ")";
			break;
		case ")":
			input.Content = text + ")";
			break;
		case ".":
			input.Content = text + uiSep;
			break;
		case ",":
			input.Content = text + uiSep;
			break;
		case "=":
			InterpretingEnter();
			break;
		}
		CalculateAndSetFontSize();
	}

	private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
	}

	private void Window_GotFocus(object sender, RoutedEventArgs e)
	{
	}

	private void Window_MouseLeave(object sender, MouseEventArgs e)
	{
	}

	private void Window_MouseEnter(object sender, MouseEventArgs e)
	{
	}

	private void Window_LostMouseCapture(object sender, MouseEventArgs e)
	{
	}

	private void Window_Deactivated(object sender, EventArgs e)
	{
	}

	private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
	{
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		Hide();
		e.Cancel = true;
		InterpretingEnter();
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		CalculateAndSetFontSize();
	}
}
