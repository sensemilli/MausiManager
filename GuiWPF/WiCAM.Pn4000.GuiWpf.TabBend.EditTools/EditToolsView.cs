using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public partial class EditToolsView : UserControl, IComponentConnector
{
	private EditToolsViewVertical? _vertical;

	private EditToolsViewHorizontal? _horizontal;

	private EditToolsViewFloating? _floating;

	private Control? _container;

	private bool _initialized;

	private EditToolsViewVertical Vertical
	{
		get
		{
			if (_vertical == null)
			{
				_vertical = new EditToolsViewVertical();
				GridContainer.Children.Add(_vertical);
			}
			return _vertical;
		}
	}

	private EditToolsViewHorizontal Horizontal
	{
		get
		{
			if (_horizontal == null)
			{
				_horizontal = new EditToolsViewHorizontal();
				GridContainer.Children.Add(_horizontal);
			}
			return _horizontal;
		}
	}

	private EditToolsViewFloating Floating
	{
		get
		{
			if (_floating == null)
			{
				_floating = new EditToolsViewFloating();
				GridContainer.Children.Add(_floating);
			}
			return _floating;
		}
	}

	private Control? Container
	{
		get
		{
			return _container;
		}
		set
		{
			if (_container != value)
			{
				if (_container != null)
				{
					_container.Visibility = Visibility.Collapsed;
				}
				_container = value;
				if (_container != null)
				{
					_container.Visibility = Visibility.Visible;
				}
			}
		}
	}

	public EditToolsView(ITranslator translator)
	{
		InitializeComponent();
	}

	private void EditToolsView_OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (_initialized)
		{
			double width = e.NewSize.Width;
			double height = e.NewSize.Height;
			if (width > height * 3.0)
			{
				Container = Horizontal;
			}
			else if (height > width * 3.0)
			{
				Container = Vertical;
			}
			else
			{
				Container = Floating;
			}
		}
	}

	private void EditToolsView_OnLoaded(object sender, RoutedEventArgs e)
	{
		_initialized = true;
	}
}
