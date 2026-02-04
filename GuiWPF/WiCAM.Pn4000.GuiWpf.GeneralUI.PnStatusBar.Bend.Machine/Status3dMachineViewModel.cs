using System.Linq;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Machine;

internal class Status3dMachineViewModel : ViewModelBase, IStatus3dMachineViewModel, IPnStatusViewModel
{
	private readonly ITranslator _translator;

	private readonly IUnitCurrentLanguage _unitCurrentLanguage;

	private string _header;

	private IDoc3d? _doc;

	private bool _isActive;

	public string Header
	{
		get
		{
			return _header;
		}
		set
		{
			if (_header != value)
			{
				_header = value;
				NotifyPropertyChanged("Header");
			}
		}
	}

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
				NotifyPropertyChanged("BendMachine");
			}
			UpdateData(_doc);
		}
	}

	public IBendMachine? BendMachine => _doc?.BendMachine;

	public string PunchName { get; private set; }

	public string PunchRadius { get; private set; }

	public string DieName { get; private set; }

	public string VWidth { get; private set; }

	public string VAngle { get; private set; }

	public string CornerRadius { get; private set; }

	public string DescPressBrakeDataName { get; private set; }

	public string DescMachineNo { get; private set; }

	public string DescPunchName { get; private set; }

	public string DescPunchRadius { get; private set; }

	public string DescDieName { get; private set; }

	public string DescVWidth { get; private set; }

	public string DescVAngle { get; private set; }

	public string DescCornerRadius { get; private set; }

	public Status3dMachineViewModel(ITranslator translator, IUnitCurrentLanguage unitCurrentLanguage)
	{
		_translator = translator;
		_unitCurrentLanguage = unitCurrentLanguage;
		_translator.ResourcesChangedStrong += delegate
		{
			UpdateTranslations();
		};
		UpdateTranslations();
	}

	private void _doc_UpdateGeneralInfoAutoEvent(IDoc3d doc)
	{
		UpdateData(doc);
	}

	public void SetActive(bool isActive)
	{
		_isActive = isActive;
		if (isActive)
		{
			UpdateData(Doc);
		}
	}

	private void UpdateTranslations()
	{
		DescPressBrakeDataName = _translator.Translate("Machine");
		DescMachineNo = _translator.Translate("StatusBar.MachineNo");
		DescPunchName = _translator.Translate("PunchName");
		DescPunchRadius = _translator.Translate("PunchRadius");
		DescDieName = _translator.Translate("DieName");
		DescVWidth = _translator.Translate("VWidth");
		DescVAngle = _translator.Translate("VAngle");
		DescCornerRadius = _translator.Translate("CornerRadius");
		NotifyPropertyChanged("DescPressBrakeDataName");
		NotifyPropertyChanged("DescMachineNo");
		NotifyPropertyChanged("DescPunchName");
		NotifyPropertyChanged("DescPunchRadius");
		NotifyPropertyChanged("DescDieName");
		NotifyPropertyChanged("DescVWidth");
		NotifyPropertyChanged("DescVAngle");
		NotifyPropertyChanged("DescCornerRadius");
	}

	public void UpdateData(IDoc3d? doc)
	{
		if (!_isActive)
		{
			return;
		}
		IBendMachine bendMachine = doc?.BendMachine;
		if (bendMachine != null)
		{
			Header = bendMachine.Name;
			if (doc != null && !doc.PreferredProfileStore.IsEmpty())
			{
				ICombinedBendDescriptorInternal cbd = doc.CombinedBendDescriptors.FirstOrDefault();
				(IPunchProfile? upperTool, IDieProfile? lowerTool, ToolSelectionType tst) bestPunchDieProfiles = doc.PreferredProfileStore.GetBestPunchDieProfiles(cbd);
				IPunchProfile item = bestPunchDieProfiles.upperTool;
				IDieProfile item2 = bestPunchDieProfiles.lowerTool;
				PunchRadius = _unitCurrentLanguage.ConvertMmToCurrentUnit(item?.Radius, addUnit: true);
				VWidth = _unitCurrentLanguage.ConvertMmToCurrentUnit(item2?.VWidth, addUnit: true);
				VAngle = ((item2 != null) ? (item2.VAngleDeg + " °") : string.Empty);
				CornerRadius = _unitCurrentLanguage.ConvertMmToCurrentUnit(item2?.CornerRadius, addUnit: true);
				PunchName = (item?.Group as IPunchGroup)?.PrimaryToolName ?? string.Empty;
				DieName = (item2?.Group as IDieGroup)?.PrimaryToolName ?? string.Empty;
			}
			else
			{
				PunchName = string.Empty;
				PunchRadius = string.Empty;
				DieName = string.Empty;
				VWidth = string.Empty;
				VAngle = string.Empty;
				CornerRadius = string.Empty;
			}
			NotifyPropertyChanged("PunchName");
			NotifyPropertyChanged("PunchRadius");
			NotifyPropertyChanged("DieName");
			NotifyPropertyChanged("VWidth");
			NotifyPropertyChanged("VAngle");
			NotifyPropertyChanged("CornerRadius");
			NotifyPropertyChanged("BendMachine");
		}
	}
}
