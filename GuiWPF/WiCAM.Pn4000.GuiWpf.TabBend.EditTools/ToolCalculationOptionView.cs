using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.PropertyGrid;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.GuiWpf.UiBasic;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public partial class ToolCalculationOptionView : UserControl, IComponentConnector
{
	private readonly ITranslator _translator;

	private const string PropPrefix = "ToolCalcSettings.Prop.";

	public ToolCalculationOptionView(ITranslator translator)
	{
		_translator = translator;
		InitializeComponent();
	}

	private void RadPropertyGrid_OnAutoGeneratingPropertyDefinition(object? sender, AutoGeneratingPropertyDefinitionEventArgs e)
	{
		if (!(base.DataContext is ToolCalculationOptionViewModel toolCalculationOptionViewModel))
		{
			e.Cancel = true;
			return;
		}
		string displayName = e.PropertyDefinition.DisplayName;
		string text = string.Empty;
		if (toolCalculationOptionViewModel.DefaultValues.TryGetValue(displayName, out object value))
		{
			string text2 = value?.ToString();
			if (value is bool)
			{
				text2 = ((!(bool)value) ? _translator.Translate("ToolCalcSettings.DefaultBoolNo") : _translator.Translate("ToolCalcSettings.DefaultBoolYes"));
			}
			else if (value is System.Enum)
			{
				e.Cancel = true;
				return;
			}
			text = Environment.NewLine + _translator.Translate("ToolCalcSettings.DefaultValue") + " " + text2;
		}
		_ = e.PropertyDefinition.SourceProperty.PropertyType == typeof(bool?);
		if (!_translator.TryTranslate("ToolCalcSettings.Prop." + displayName, out string result))
		{
			result = displayName;
		}
		e.PropertyDefinition.DisplayName = result;
		if (_translator.TryTranslate("ToolCalcSettings.Prop." + displayName + "Group", out string result2))
		{
			e.PropertyDefinition.GroupName = result2;
		}
		else
		{
			e.PropertyDefinition.GroupName = "miscellaneous";
		}
		if (!_translator.TryTranslate("ToolCalcSettings.Prop." + displayName + "Tt", out string result3))
		{
			result3 = "";
		}
		e.PropertyDefinition.Description = result3 + text + Environment.NewLine + "[" + displayName + "]";
	}

	private void RadPropertyGrid_OnEditEnded(object? sender, PropertyGridEditEndedEventArgs e)
	{
	}

	private void RadPropertyGrid_OnBeginningEdit(object? sender, PropertyGridBeginningEditEventArgs e)
	{
	}

	private void RadAutoCompleteBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!(sender is RadAutoCompleteBox radAutoCompleteBox) || !(base.DataContext is ToolCalculationOptionViewModel toolCalculationOptionViewModel) || toolCalculationOptionViewModel.SelectedProfile?.Data == null)
		{
			return;
		}
		List<Guid> list = new List<Guid>();
		foreach (object selectedItem in radAutoCompleteBox.SelectedItems)
		{
			if (selectedItem is ComboboxEntry<Guid> comboboxEntry)
			{
				list.Add(comboboxEntry.Value);
			}
		}
		toolCalculationOptionViewModel.SelectedProfile.Data.BendOrderStrategyGuids = list;
	}
}
