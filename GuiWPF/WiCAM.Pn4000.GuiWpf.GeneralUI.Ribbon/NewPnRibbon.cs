using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Ribbon.NewRibbonEntitiesMV;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Ribbon;

public partial class NewPnRibbon : NewPnRibbonBase, IComponentConnector
{
	public NewPnRibbon()
	{
		base.DataContextChanged += NewPnRibbon_DataContextChanged;
		InitializeComponent();
	}

	private void NewPnRibbon_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		CleanAllData();
		if (e.NewValue == null || !(e.NewValue is INewRibbonDataContext))
		{
			return;
		}
		NewRibbonBaseEntity newRibbonBaseEntity = (NewRibbonBaseEntity)(e.NewValue as INewRibbonDataContext).Root;
		if (newRibbonBaseEntity == null)
		{
			return;
		}
		foreach (INewRibbonBaseEntity child in newRibbonBaseEntity.Children)
		{
			if (child is NewRibbonTab)
			{
				Add(child as NewRibbonTab);
			}
		}
	}

	private void Add(NewRibbonTab? tab)
	{
		if (tab == null || base.TabEntityTemplate == null)
		{
			return;
		}
		FrameworkElement frameworkElement = (FrameworkElement)base.TabEntityTemplate.LoadContent();
		frameworkElement.DataContext = tab;
		RibbonButtonsZone.Children.Add(frameworkElement);
		if (tab.Children == null)
		{
			return;
		}
		Panel panel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		GroupBox groupBox = (GroupBox)base.GroupBoxTemplate.LoadContent();
		panel.Children.Add(groupBox);
		WrapPanel wrapPanel = (WrapPanel)(groupBox.Content = new WrapPanel
		{
			Orientation = Orientation.Vertical
		});
		foreach (INewRibbonBaseEntity child in tab.Children)
		{
			FrameworkElement frameworkElement2 = (FrameworkElement)base.SimpleButtonTemplate.LoadContent();
			frameworkElement2.DataContext = child;
			wrapPanel.Children.Add(frameworkElement2);
		}
		MainRibbonZone.Child = panel;
	}

	private void CleanAllData()
	{
	}


}
