using System.Reflection;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Dat;

namespace WiCAM.Pn4000.JobManager;

public class ColumnConfigurationInfo : ViewModelBase
{
	private bool _isSelected;

	private string _translation;

	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (!IsDefault)
			{
				_isSelected = value;
				NotifyPropertyChanged("IsSelected");
				IsChanged = true;
			}
		}
	}

	public bool IsDefault { get; set; }

	public string KeyName { get; set; }

	public string Translation
	{
		get
		{
			return _translation;
		}
		set
		{
			_translation = value;
			NotifyPropertyChanged("Translation");
			IsChanged = true;
		}
	}

	public PropertyInfo Property { get; set; }

	public CppConfigurationLineInfo CppLine { get; set; }

	public bool IsChanged { get; set; }

	public ColumnConfigurationInfo()
	{
	}

	public ColumnConfigurationInfo(PropertyInfo pi)
	{
		Property = pi;
		DatKeyAttribute customAttribute = pi.GetCustomAttribute<DatKeyAttribute>();
		if (customAttribute != null)
		{
			KeyName = customAttribute.Key;
		}
		else
		{
			KeyName = Property.Name;
		}
		if (pi.GetCustomAttribute<ObligatoryAttribute>() != null)
		{
			IsDefault = true;
			_isSelected = true;
		}
	}

	public void Select()
	{
		_isSelected = true;
	}

	public void Translate(string text)
	{
		_translation = text;
	}
}
