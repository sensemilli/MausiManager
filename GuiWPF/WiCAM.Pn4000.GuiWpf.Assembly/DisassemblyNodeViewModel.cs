using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Assembly;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public class DisassemblyNodeViewModel : ViewModelBase
{
	private int _id;

	private bool _isExpanded;

	private string _nameGeometry;

	private DisassemblyPartNode _node;

	private bool _isMouseHovering;

	private bool _isMouseSelected;

	public int Id
	{
		get
		{
			return _id;
		}
		set
		{
			if (_id != value)
			{
				_id = value;
				NotifyPropertyChanged("Id");
			}
		}
	}

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

	public Brush BackgroundCol => PartViewModel?.BackgroundCol ?? Brushes.MediumPurple;

	public string NameGeometry
	{
		get
		{
			return _nameGeometry;
		}
		set
		{
			if (_nameGeometry != value)
			{
				_nameGeometry = value;
				NotifyPropertyChanged("NameGeometry");
				if (_nameGeometry.EndsWith("x"))
				{
					IsMouseSelected = true;
				}
			}
		}
	}

	public string Desc => ContentCount + " " + NameGeometry + " " + PartViewModel?.Desc;

	public DisassemblyPartNode Node
	{
		get
		{
			return _node;
		}
		set
		{
			if (_node != value)
			{
				_node = value;
				NotifyPropertyChanged("Node");
			}
		}
	}

	public ObservableCollection<DisassemblyNodeViewModel> Children { get; }

	public bool IsMouseHovering
	{
		get
		{
			return _isMouseHovering;
		}
		set
		{
			if (_isMouseHovering != value)
			{
				_isMouseHovering = value;
				NotifyPropertyChanged("IsMouseHovering");
				NotifyPropertyChanged("Desc");
				if (PartViewModel != null)
				{
					PartViewModel.IsMouseHoveringByCode = value;
				}
			}
		}
	}

	public bool IsMouseHoveringByCode
	{
		get
		{
			return _isMouseHovering;
		}
		set
		{
			if (_isMouseHovering != value)
			{
				_isMouseHovering = value;
				NotifyPropertyChanged("IsMouseHoveringByCode");
				NotifyPropertyChanged("IsMouseHovering");
				NotifyPropertyChanged("Desc");
			}
		}
	}

	public bool IsMouseSelected
	{
		get
		{
			return _isMouseSelected;
		}
		set
		{
			if (_isMouseSelected != value)
			{
				_isMouseSelected = value;
				NotifyPropertyChanged("IsMouseSelected");
				NotifyPropertyChanged("Desc");
				if (PartViewModel != null)
				{
					PartViewModel.IsMouseSelectedByCode = value;
				}
			}
		}
	}

	public bool IsMouseSelectedByCode
	{
		get
		{
			return _isMouseSelected;
		}
		set
		{
			if (_isMouseSelected != value)
			{
				_isMouseSelected = value;
				NotifyPropertyChanged("IsMouseSelectedByCode");
				NotifyPropertyChanged("IsMouseSelected");
				NotifyPropertyChanged("Desc");
			}
		}
	}

	public DisassemblyPartViewModel PartViewModel { get; set; }

	public int ContentCount => Children.Sum((DisassemblyNodeViewModel x) => x.ContentCount) + 1;

	public void RefreshColor()
	{
		NotifyPropertyChanged("BackgroundCol");
	}

	public DisassemblyNodeViewModel(DisassemblyPartNode node)
	{
		_node = node;
		_id = node.Part?.ID ?? (-1);
		_nameGeometry = node.Part?.OriginalGeometryName;
		Children = new ObservableCollection<DisassemblyNodeViewModel>(node.Children.Select((DisassemblyPartNode n) => new DisassemblyNodeViewModel(n)));
	}
}
