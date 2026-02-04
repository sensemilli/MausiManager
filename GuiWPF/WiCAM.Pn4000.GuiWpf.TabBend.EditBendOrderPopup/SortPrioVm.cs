using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditBendOrderPopup;

internal class SortPrioVm : ISortPrioVm, INotifyPropertyChanged
{
	private readonly ITranslator? _translator;

	private BendSequenceSorts? _sortType;

	public BendSequenceSorts? SortType
	{
		get
		{
			return _sortType;
		}
		set
		{
			if (_sortType != value)
			{
				_sortType = value;
				string text = "l_enum.BendSequence." + _sortType.ToString();
				Description = _translator.Translate(text);
				DescriptionLong = _translator.Translate(text + "_EXPLANATION");
				OnPropertyChanged("SortType");
				OnPropertyChanged("Description");
				OnPropertyChanged("DescriptionLong");
			}
		}
	}

	public string Description { get; private set; }

	public string DescriptionLong { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public SortPrioVm(BendSequenceSorts? type = null, ITranslator? translator = null)
	{
		_translator = translator;
		SortType = type;
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value))
		{
			return false;
		}
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
