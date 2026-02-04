using System.Globalization;
using System.Windows.Media;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Material;

internal class Status3dMaterialViewModel : ViewModelBase, IPnStatusViewModel, IStatus3dMaterialViewModel
{
	private readonly IMaterialManager _materials;

	private readonly IUnitCurrentLanguage _unitCurrentLanguage;

	private readonly ITranslator _translator;

	private string _header;

	private Brush _headerColor;

	private Brush ColorOk = Brushes.White;

	private Brush ColorWarning = Brushes.Orange;

	private string _uiThickness;

	private string _displayGroupName;

	private string _display3dGroupName;

	private IMaterialArt? _material;

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

	public Brush HeaderColor
	{
		get
		{
			return _headerColor;
		}
		private set
		{
			if (_headerColor != value)
			{
				_headerColor = value;
				NotifyPropertyChanged("HeaderColor");
			}
		}
	}

	public string UiThickness
	{
		get
		{
			return _uiThickness;
		}
		set
		{
			if (_uiThickness != value)
			{
				_uiThickness = value;
				NotifyPropertyChanged("UiThickness");
			}
		}
	}

	public string DisplayGroupName
	{
		get
		{
			return _displayGroupName;
		}
		set
		{
			if (_displayGroupName != value)
			{
				_displayGroupName = value;
				NotifyPropertyChanged("DisplayGroupName");
			}
		}
	}

	public string Display3dGroupName
	{
		get
		{
			return _display3dGroupName;
		}
		set
		{
			if (_display3dGroupName != value)
			{
				_display3dGroupName = value;
				NotifyPropertyChanged("Display3dGroupName");
			}
		}
	}

	public IMaterialArt? Material
	{
		get
		{
			return _material;
		}
		set
		{
			if (_material != value)
			{
				_material = value;
				NotifyPropertyChanged("Material");
			}
		}
	}

	public string DescMaterial { get; private set; }

	public string DescMaterialGroup { get; private set; }

	public string DescMaterial3dGroup { get; private set; }

	public string DescThickness { get; private set; }

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

	private void _doc_UpdateGeneralInfoAutoEvent(IDoc3d doc)
	{
		UpdateData();
	}

	public Status3dMaterialViewModel(IMaterialManager materials, IUnitCurrentLanguage unitCurrentLanguage, ITranslator translator)
	{
		_materials = materials;
		_unitCurrentLanguage = unitCurrentLanguage;
		_translator = translator;
		_translator.ResourcesChangedStrong += delegate
		{
			UpdateTranslations();
		};
		UpdateTranslations();
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
		UpdateMaterial(_doc?.Material, _doc?.Thickness);
	}

	private void UpdateTranslations()
	{
		DescMaterial = _translator.Translate("Material");
		DescMaterialGroup = _translator.Translate("Material Group");
		DescMaterial3dGroup = _translator.Translate("3D Material Group");
		DescThickness = _translator.Translate("Thickness");
		NotifyPropertyChanged("DescMaterial");
		NotifyPropertyChanged("DescMaterialGroup");
		NotifyPropertyChanged("DescMaterial3dGroup");
		NotifyPropertyChanged("DescThickness");
	}

	public void UpdateMaterial(IMaterialArt? material, double? thickness)
	{
		if (!_isActive)
		{
			return;
		}
		if (material != null)
		{
			IDoc3d? doc = Doc;
			if (doc != null && !doc.PnMaterialByUser)
			{
				HeaderColor = ColorWarning;
				goto IL_003d;
			}
		}
		HeaderColor = ColorOk;
		goto IL_003d;
		IL_003d:
		Material = material;
		DisplayGroupName = "";
		if (material != null && _materials.MaterialGroup.TryGetValue(material.MaterialGroupId, out string value))
		{
			DisplayGroupName = value;
		}
		Display3dGroupName = "";
		if (material != null && _materials.Material3DGroup.TryGetValue(material.MaterialGroupForBendDeduction, out string value2))
		{
			Display3dGroupName = value2;
		}
		UiThickness = _unitCurrentLanguage.ConvertMmToCurrentUnit(thickness, addUnit: true, "0.##");
		if (material != null)
		{
            Header = string.Format(CultureInfo.InvariantCulture,
			 "{0}   ({1}/{2})   {3}",
			 material.Name,
			 material.Number,
			 material.MaterialGroupId,
			 UiThickness);
        }
		else
		{
			Header = "";
			UiThickness = "";
		}
	}
}
