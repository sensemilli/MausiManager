using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public partial class DockingContainerXaml : UserControl, IComponentConnector, IStyleConnector
{
	private class CustomGeneratedItemsFactory : DefaultGeneratedItemsFactory
	{
		private readonly DockingContainerXaml _dockingContainer;

		public CustomGeneratedItemsFactory(DockingContainerXaml dockingContainer)
		{
			_dockingContainer = dockingContainer;
		}

		public override ToolWindow CreateToolWindow()
		{
			ToolWindow toolWindow = base.CreateToolWindow();
			toolWindow.Style = _dockingContainer.GetStyle("MyToolWindowStyle");
			toolWindow.MinWidth = 50.0;
			toolWindow.MinHeight = 50.0;
			return toolWindow;
		}
	}

	private DockingContainer Container { get; set; }

	public DockingContainerXaml(DockingContainer container)
	{
		Container = container;
		InitializeComponent();
	}

	protected override void OnInitialized(EventArgs e)
	{
		DockingContainer.GeneratedItemsFactory = new CustomGeneratedItemsFactory(this);
		DockingContainer.DockingPanesFactory = new CustomDockingPanesFactory(this);
		base.OnInitialized(e);
	}

	private void RadDocking_Loaded(object sender, RoutedEventArgs e)
	{
		Thickness padding = DockingContainer.Padding;
		Thickness borderThickness = DockingContainer.BorderThickness;
		ChildContainer.Margin = new Thickness
		{
			Bottom = 0.0 - (padding.Bottom + borderThickness.Bottom),
			Left = 0.0 - (padding.Left + borderThickness.Left),
			Right = 0.0 - (padding.Right + borderThickness.Right),
			Top = 0.0 - (padding.Top + borderThickness.Top)
		};
	}

	public Style GetStyle(string key)
	{
		return FindResource(key) as Style;
	}

	public DataTemplate GetDataTemplate(string key)
	{
		return FindResource(key) as DataTemplate;
	}

	private void OnTitleInitialized(object? sender, EventArgs e)
	{
		if (sender is FrameworkElement frameworkElement)
		{
			frameworkElement.DataContext = ParentOfTypeExtensions.ParentOfType<ToolWindow>((DependencyObject)frameworkElement)?.DataContext;
		}
	}
}
