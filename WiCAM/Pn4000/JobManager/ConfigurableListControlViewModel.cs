using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class ConfigurableListControlViewModel : ViewModelBase
{
	private string _text;

	private string _selectedText;

	private bool _isChanged;

	private ICommand _addTextCommand;

	private ICommand _deleteTextCommand;

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			_text = value;
			NotifyPropertyChanged("Text");
		}
	}

	public string SelectedText
	{
		get
		{
			return _selectedText;
		}
		set
		{
			_selectedText = value;
			NotifyPropertyChanged("SelectedText");
		}
	}

	public bool IsChanged
	{
		get
		{
			return _isChanged;
		}
		set
		{
			_isChanged = value;
			NotifyPropertyChanged("IsChanged");
		}
	}

	public ObservableCollection<string> Texts { get; set; } = new ObservableCollection<string>();


	public ICommand AddTextCommand
	{
		get
		{
			if (_addTextCommand == null)
			{
				_addTextCommand = new RelayCommand(delegate
				{
					AddText();
				}, (object x) => CanAddText());
			}
			return _addTextCommand;
		}
	}

	public ICommand DeleteTextCommand
	{
		get
		{
			if (_deleteTextCommand == null)
			{
				_deleteTextCommand = new RelayCommand(delegate
				{
					DeleteText();
				}, (object x) => CanDeleteText());
			}
			return _deleteTextCommand;
		}
	}

	private void AddText()
	{
		if (!Texts.Contains(Text))
		{
			IsChanged = true;
			Texts.Add(Text);
			Text = string.Empty;
		}
	}

	private bool CanAddText()
	{
		return !string.IsNullOrEmpty(Text);
	}

	private void DeleteText()
	{
		IsChanged = true;
		Texts.Remove(SelectedText);
		SelectedText = string.Empty;
	}

	private bool CanDeleteText()
	{
		return !string.IsNullOrEmpty(SelectedText);
	}

	public void AddTexts(IEnumerable<string> texts)
	{
		Texts.Clear();
		foreach (string text in texts)
		{
			Texts.Add(text);
		}
	}
}
