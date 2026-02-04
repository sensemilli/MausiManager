using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public partial class TextInput : Window, ITextInput, IComponentConnector
{
	private LikeModalMode _likeModalMode;

	private ILogCenterService _logCenterService;

	private IConfigProvider _configProvider;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private string _str1;

	private string _str2;

	public bool IsOkExit { get; private set; }

	public string Result { get; set; }

	public TextInput(ILogCenterService logCenterService, IConfigProvider configProvider, IMainWindowDataProvider mainWindowDataProvider)
	{
		this._logCenterService = logCenterService;
		this._configProvider = configProvider;
		this._mainWindowDataProvider = mainWindowDataProvider;
	}

	public ITextInput Init(string str1, string str2)
	{
		this._str1 = str1;
		this._str2 = str2;
		this._likeModalMode = this._mainWindowDataProvider.GetLikeModalMode();
		this.InitializeComponent();
		return this;
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
		if (this.IsOkExit)
		{
			this.Result = this.textBox1.Text;
		}
		else
		{
			this.Result = "$CANCEL$";
		}
		base.Hide();
		this._likeModalMode.ResetLikeModalMode();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		this.label1.Content = this._str2.Trim();
		this.textBox1.MaxLength = 0;
		string[] array = this._str1.Split(' ');
		this.IsOkExit = false;
		if (this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().LowerCaseAllowed)
		{
			this.textBox1.CharacterCasing = CharacterCasing.Normal;
		}
		else
		{
			this.textBox1.CharacterCasing = CharacterCasing.Upper;
		}
		try
		{
			this.textBox1.MaxLength = Convert.ToInt32(array[0]);
			this.textBox1.Text = this._str1.Substring(array[0].Length + 1);
			this.textBox1.SelectAll();
			this.textBox1.Focusable = true;
			Keyboard.Focus(this.textBox1);
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
	}

	private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
	{
		this.IsOkExit = true;
		base.Close();
	}

	private void button1_Click(object sender, RoutedEventArgs e)
	{
		this.IsOkExit = true;
		base.Close();
	}

	private void button2_Click(object sender, RoutedEventArgs e)
	{
		this.IsOkExit = false;
		base.Close();
	}

	private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			e.Handled = true;
			this.IsOkExit = true;
			base.Close();
		}
	}

	public void StartLikeModalMode()
	{
		this._likeModalMode.SetMode(EndLikeModalMode);
		this._likeModalMode.WaitEndModal();
	}

	private bool EndLikeModalMode()
	{
		if (base.Visibility == Visibility.Visible)
		{
			this.IsOkExit = true;
			base.Close();
		}
		return true;
	}

	Window ITextInput.Owner()
	{
		return base.Owner;
	}

	void ITextInput.Owner(Window value)
	{
		base.Owner = value;
	}

	void ITextInput.Show()
	{
		base.Show();
	}
}
