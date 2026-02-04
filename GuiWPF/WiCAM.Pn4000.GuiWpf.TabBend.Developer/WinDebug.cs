using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.PropertyGrid;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.Developer;

internal partial class WinDebug : Window, IWinDebug, IComponentConnector, IStyleConnector
{
	private Hyperlink? _lastHyper;

	private Brush? _lastHyperBackground;

	private Brush _selectedHyperBackground = new SolidColorBrush(Colors.Yellow);

	public WinDebugViewModel Vm { get; set; }

	public WinDebug(ICurrentDocProvider currentDocProvider)
	{
		base.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
		currentDocProvider.CurrentDocChanged += CurrentDocProvider_CurrentDocChanged;
		CurrentDocProvider_CurrentDocChanged(null, currentDocProvider.CurrentDoc);
		InitializeComponent();
	}

	private void CurrentDocProvider_CurrentDocChanged(IDoc3d docOld, IDoc3d docNew)
	{
		Vm = docNew.Factorio.Resolve<WinDebugViewModel>();
		base.DataContext = Vm;
	}

	public void Init()
	{
		Show();
		base.Visibility = Visibility.Collapsed;
		base.Visibility = Visibility.Visible;
	}

	private void LogStep_OnInitialized(object? sender, EventArgs e)
	{
		RadExpander val = (RadExpander)((sender is RadExpander) ? sender : null);
		if (val == null || !(((FrameworkElement)(object)val).DataContext is LogStepViewModel logStepViewModel))
		{
			return;
		}
		TextBlock textBlock = new TextBlock();
		foreach (var textElement in logStepViewModel.LogStep.TextElements)
		{
			Run run = new Run((textElement.time.HasValue ? (textElement.time.Value.ToString(":mm:ss,") + textElement.time.Value.Millisecond.ToString("000") + " ") : string.Empty) + textElement.text);
			if (textElement.complexInfo == null)
			{
				textBlock.Inlines.Add(run);
				continue;
			}
			Hyperlink hyperlink = new Hyperlink(run);
			hyperlink.Click += Hyper_Click;
			hyperlink.DataContext = textElement.complexInfo;
			textBlock.Inlines.Add(hyperlink);
		}
		((ContentControl)(object)val).Content = textBlock;
		if (logStepViewModel.LogStep.TextElements.Count == 0)
		{
			((UIElement)(object)val).IsEnabled = false;
		}
	}

	private void Hyper_Click(object sender, RoutedEventArgs e)
	{
		if (sender is Hyperlink { DataContext: not null } hyperlink)
		{
			if (_lastHyper != null)
			{
				_lastHyper.Background = _lastHyperBackground;
			}
			_lastHyperBackground = hyperlink.Background;
			_lastHyper = hyperlink;
			hyperlink.Background = _selectedHyperBackground;
			if (hyperlink.DataContext is IEnumerable enumerable)
			{
				Vm.LogDetail = new EnumerableNester(enumerable);
			}
			else
			{
				Vm.LogDetail = hyperlink.DataContext;
			}
		}
	}

	private void RadPropertyGrid_OnAutoGeneratingPropertyDefinition(object? sender, AutoGeneratingPropertyDefinitionEventArgs e)
	{
	}

	private void WinDebug_OnClosing(object? sender, CancelEventArgs e)
	{
		e.Cancel = true;
		base.Visibility = Visibility.Collapsed;
	}

	private void TopMost_OnUnchecked(object sender, RoutedEventArgs e)
	{
		base.Topmost = false;
	}

	private void TopMost_OnChecked(object sender, RoutedEventArgs e)
	{
		base.Topmost = true;
	}
}
