using System;
using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.DocInfo;

internal class Status3dDocInfoViewModel : ViewModelBase, IStatus3dDocInfoViewModel, IPnStatusViewModel
{
	private readonly ITranslator _translator;

	private readonly IUnitCurrentLanguage _unitCurrentLanguage;

	private readonly IMessageLogGlobal _logger;

	private string _lastModelName;

	private IDoc3d? _doc;

	private bool _isActive;

	public IDoc3d? Doc
	{
		get
		{
			return _doc;
		}
		set
		{
			if (_doc != value)
			{
				if (_doc != null)
				{
					_doc.UpdateGeneralInfoAutoEvent -= _doc_UpdateGeneralInfoAutoEvent;
				}
				_doc = value;
				if (_doc != null)
				{
					_doc.UpdateGeneralInfoAutoEvent += _doc_UpdateGeneralInfoAutoEvent;
				}
				NotifyPropertyChanged("Doc");
			}
			UpdateData();
		}
	}

	public string DescName { get; private set; }

	public string ArchiveNumber { get; private set; }

	public string ArchiveFile { get; private set; }

	public string NamePp { get; private set; }

	public string ModelName { get; private set; }

	public Status3dDocInfoViewModel(ITranslator translator, IUnitCurrentLanguage unitCurrentLanguage, IMessageLogGlobal logger)
	{
		_translator = translator;
		_unitCurrentLanguage = unitCurrentLanguage;
		_translator.ResourcesChangedStrong += delegate
		{
			UpdateTranslations();
		};
		_logger = logger;
		UpdateTranslations();
	}

	private void UpdateTranslations()
	{
		DescName = _translator.Translate("Name");
		NotifyPropertyChanged("DescName");
	}

	private void _doc_UpdateGeneralInfoAutoEvent(IDoc3d doc)
	{
		Application.Current.Dispatcher.Invoke(UpdateData);
	}

	public void SetActive(bool isActive)
	{
		_isActive = isActive;
		if (isActive)
		{
			UpdateData();
		}
	}

	private void UpdateData()
	{
		IDoc3d doc = Doc;
		ArchiveNumber = ((doc != null && doc.SavedArchiveNumber >= 0) ? doc.SavedArchiveNumber.ToString() : "-");
		ArchiveFile = (string.IsNullOrWhiteSpace(doc?.SavedFileName) ? "-" : doc.SavedFileName);
		NamePp = string.Join(", ", doc?.NamesPpBase ?? Array.Empty<string>());
		ModelName = Doc?.DiskFile.Header.ModelName ?? string.Empty;
		NotifyPropertyChanged("ArchiveNumber");
		NotifyPropertyChanged("ArchiveFile");
		NotifyPropertyChanged("NamePp");
		NotifyPropertyChanged("ModelName");
		NotifyPropertyChanged("Doc");
		string text = doc?.DiskFile.Header.ModelName;
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		if (text.Length > 80)
		{
			if (_lastModelName != text)
			{
				_logger.ShowWarningMessage("Name des Bauteils ist zu lang und musste auf 80 Zeichen gekürzt werden!");
			}
			_lastModelName = text;
			text = text.Substring(0, 80);
		}
		else
		{
			_lastModelName = text;
		}
		GeneralSystemComponentsAdapter.SetApplicationTitle(text);
	}
}
