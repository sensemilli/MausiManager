using System;
using System.Collections.Generic;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.GuiWpf.TabViewer3D;

public class HierarchicNode : ViewModelBase
{
	public Action<HierarchicNode> SetSelectedByTreeView;

	public Action<HierarchicNode, bool> SettingVisibility;

	private bool _isExpanded;

	private bool _isSelected;

	private bool? _visible;

	private Brush _background = Brushes.Transparent;

	private bool _isHovered;

	public string DescBoundingBox { get; set; }

	public Model Model { get; set; }

	public List<HierarchicNode> SubNodes { get; set; } = new List<HierarchicNode>();

	public HierarchicNode ParentNode { get; set; }

	public int Level { get; set; }

	public bool IsExpanded
	{
		get
		{
			return _isExpanded;
		}
		set
		{
			if (_isExpanded != value)
			{
				_isExpanded = value;
				NotifyPropertyChanged("IsExpanded");
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (_isSelected != value)
			{
				_isSelected = value;
				NotifyPropertyChanged("IsSelected");
				NotifyPropertyChanged("IsSelectedByTreeView");
				CalculateBackground();
			}
		}
	}

	public bool IsSelectedByTreeView
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value)
			{
				SetSelectedByTreeView?.Invoke(this);
			}
		}
	}

	public bool? Visible
	{
		get
		{
			return _visible;
		}
		set
		{
			if (value.HasValue)
			{
				SettingVisibility?.Invoke(this, value.Value);
			}
		}
	}

	public Brush Background => _background;

	public bool IsHovered
	{
		get
		{
			return _isHovered;
		}
		set
		{
			if (_isHovered != value)
			{
				_isHovered = value;
				NotifyPropertyChanged("IsHovered");
				CalculateBackground();
			}
		}
	}

	public string Desc
	{
		get
		{
			if (SubNodes.Count > 0)
			{
				if (Level == 0)
				{
					return "Root";
				}
				string text = Model?.Name;
				if (string.IsNullOrWhiteSpace(text))
				{
					return Level.ToString();
				}
				return text;
			}
			return DescBoundingBox;
		}
	}

	public void SetVisibilityForCb(bool? newVis)
	{
		if (_visible != newVis)
		{
			_visible = newVis;
			NotifyPropertyChanged("Visible");
		}
	}

	private void CalculateBackground()
	{
		Brush brush = Brushes.Transparent;
		if (_isSelected)
		{
			brush = Brushes.Yellow;
		}
		else if (_isHovered)
		{
			Color color = default(Color);
			color.ScR = 0.5f;
			color.ScG = 1f;
			color.ScB = 0.5f;
			color.ScA = 1f;
			brush = new SolidColorBrush(color);
		}
		if (_background != brush)
		{
			_background = brush;
			NotifyPropertyChanged("Background");
		}
	}
}
