using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow.Settings;

public class ValidationSettingsViewModel : ViewModelBase, IValidationSettingsViewModel
{
	public class IntrinsicTest
	{
		public class Parameter
		{
			public string Symbol { get; set; }

			public string DescShort { get; set; }

			public string DescLong { get; set; }

			public string DescSuffix { get; set; }

			public double Value { get; set; }
		}

		public Action Save;

		public bool IsActive { get; set; }

		public string DescShort { get; set; }

		public string DescLong { get; set; }

		public string ImagePath { get; set; }

		public ImageSource Image
		{
			get
			{
				if (File.Exists(ImagePath))
				{
					return new BitmapImage(new Uri(ImagePath));
				}
				return null;
			}
		}

		public ObservableCollection<Parameter> Parameters { get; set; }
	}

	public class ValidationGroup : ViewModelBase
	{
		private string _desc;

		public int Ind { get; set; }

		public string Desc
		{
			get
			{
				return _desc;
			}
			set
			{
				if (_desc != value)
				{
					_desc = value;
					NotifyPropertyChanged("Desc");
				}
			}
		}

		public bool IsBendingZone { get; set; }

		public ObservableCollection<ValidationParam> Params { get; set; } = new ObservableCollection<ValidationParam>();

		public ObservableCollection<ValidationMacro> Macros { get; set; } = new ObservableCollection<ValidationMacro>();

		public Func<object, bool> DragSourceCanDragItem { get; set; }

		private bool _dragSourceCanDragItem(object obj)
		{
			return !((ValidationMacro)obj).IsConstElement;
		}

		public ValidationGroup()
		{
			DragSourceCanDragItem = _dragSourceCanDragItem;
		}
	}

	public class ValidationMacro : ViewModelBase
	{
		public string Desc { get; set; }

		public string Type { get; set; }

		public bool IsConstElement { get; set; }

		public override string ToString()
		{
			return Desc;
		}
	}

	public class ValidationParam : ViewModelBase
	{
		public ValidationGroup Origin { get; set; }

		public ValidationGroup Target { get; set; }

		public double Const { get; set; }

		public double Thickness { get; set; }

		public double ConstMin { get; set; }

		public double ConstMax { get; set; }

		public bool Active { get; set; }

		public double RadiusConcaveBending { get; set; }
	}

	private PopupUnfoldSettingModel Model;

	private ValidationSettingsConfig Config;

	private IntrinsicTest _selectedIntrinsicTest;

	private ValidationGroup _selectedGroup;

	private string ImageBasePath;

	public bool AutoValidationOnFileLoaded { get; set; }

	public ObservableCollection<IntrinsicTest> IntrinsicTests { get; set; }

	public IntrinsicTest SelectedIntrinsicTest
	{
		get
		{
			return _selectedIntrinsicTest;
		}
		set
		{
			if (_selectedIntrinsicTest != value)
			{
				_selectedIntrinsicTest = value;
				NotifyPropertyChanged("SelectedIntrinsicTest");
				if (_selectedIntrinsicTest != null)
				{
					VisibilityIntrinsicDetails = Visibility.Visible;
					VisibilityIntrinsicDetailsHint = Visibility.Collapsed;
				}
				else
				{
					VisibilityIntrinsicDetails = Visibility.Collapsed;
					VisibilityIntrinsicDetailsHint = Visibility.Visible;
				}
				NotifyPropertyChanged("VisibilityIntrinsicDetails");
				NotifyPropertyChanged("VisibilityIntrinsicDetailsHint");
			}
		}
	}

	public Visibility VisibilityIntrinsicDetails { get; set; } = Visibility.Collapsed;

	public Visibility VisibilityIntrinsicDetailsHint { get; set; }

	public RelayCommand<object> CmdAddNewGroup { get; }

	public RelayCommand<ValidationGroup> CmdDeleteGroup { get; }

	public double ValiDistBendingAlpha { get; set; }

	public ObservableCollection<ValidationGroup> ValiGroups { get; set; }

	public ObservableCollection<ValidationMacro> ValiMacros { get; set; }

	public ObservableCollection<ValidationMacro> UnassignedMacros { get; set; }

	public ValidationGroup SelectedGroup
	{
		get
		{
			return _selectedGroup;
		}
		set
		{
			if (_selectedGroup != value)
			{
				_selectedGroup = value;
				NotifyPropertyChanged("SelectedGroup");
				NotifyPropertyChanged("VisibilityDistBending");
			}
		}
	}

	public Visibility VisibilityDistBending
	{
		get
		{
			if (SelectedGroup != null && SelectedGroup.IsBendingZone)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}
	}

	public ImageSource ImageDistanceAll => new BitmapImage(new Uri(ImageBasePath + "ValiDistAll.png"));

	public ImageSource ImageDistanceBendingAll => new BitmapImage(new Uri(ImageBasePath + "ValiDistBendingAll.png"));

	public ImageSource ImageDistanceBendingEdge => new BitmapImage(new Uri(ImageBasePath + "ValiDistBendingEdge.png"));

	private string Translate(string key)
	{
		return ((string)Application.Current.FindResource("l_popup.PopupUnfoldSetting." + key)) ?? key;
	}

	public ValidationSettingsViewModel()
	{
		CmdAddNewGroup = new RelayCommand<object>(AddNewGroup);
		CmdDeleteGroup = new RelayCommand<ValidationGroup>(DeleteGroup, CanDeleteGroup);
	}

	public IValidationSettingsViewModel Init(PopupUnfoldSettingModel model, string pndrive)
	{
		Model = model;
		Config = Model.ValidationGeometryConfig;
		AutoValidationOnFileLoaded = Config.ValidationTestsAuto;
		if (Config.DefaultValues)
		{
			Config = ValidationSettingsConfig.ValidationSettingsDefaults();
			modifyConfig(Config);
		}
		ImageBasePath = pndrive + "\\u\\pn\\pfiles\\img\\";
		IntrinsicTests = new ObservableCollection<IntrinsicTest>();
		IntrinsicTest test = new IntrinsicTest
		{
			DescShort = Translate("ValiISimpelHoleTitle"),
			DescLong = Translate("ValiISimpelHoleDesc"),
			ImagePath = ImageBasePath + "ValiISimpelHole.png",
			IsActive = Config.ValiIntrinsicSimpelHole,
			Parameters = new ObservableCollection<IntrinsicTest.Parameter>
			{
				new IntrinsicTest.Parameter
				{
					Symbol = "r>=",
					DescShort = Translate("ValiISimpelHoleRDesc"),
					DescLong = Translate("ValiISimpelHoleRTooltip"),
					DescSuffix = Translate("ValiISimpelHoleRSuffix"),
					Value = Config.ValiIntrinsicSimpelHoleR
				}
			}
		};
		test.Save = delegate
		{
			Config.ValiIntrinsicSimpelHole = test.IsActive;
			Config.ValiIntrinsicSimpelHoleR = test.Parameters.First().Value;
		};
		IntrinsicTests.Add(test);
		IntrinsicTest testBending = new IntrinsicTest
		{
			DescShort = Translate("ValiIBendingTitle"),
			DescLong = Translate("ValiIBendingDesc"),
			ImagePath = ImageBasePath + "ValiIBending.png",
			IsActive = Config.ValiIntrinsicBending,
			Parameters = new ObservableCollection<IntrinsicTest.Parameter>
			{
				new IntrinsicTest.Parameter
				{
					Symbol = "H>=",
					DescShort = Translate("ValiIBendingHDesc"),
					DescLong = Translate("ValiIBendingHTooltip"),
					DescSuffix = Translate("ValiIBendingHSuffix"),
					Value = Config.ValiIntrinsicBendingH
				},
				new IntrinsicTest.Parameter
				{
					Symbol = "r>=",
					DescShort = Translate("ValiIBendingRDesc"),
					DescLong = Translate("ValiIBendingRTooltip"),
					DescSuffix = Translate("ValiIBendingRSuffix"),
					Value = Config.ValiIntrinsicBendingR
				}
			}
		};
		testBending.Save = delegate
		{
			Config.ValiIntrinsicBending = testBending.IsActive;
			Config.ValiIntrinsicBendingH = testBending.Parameters[0].Value;
			Config.ValiIntrinsicBendingR = testBending.Parameters[1].Value;
		};
		IntrinsicTests.Add(testBending);
		IntrinsicTest testSelfCollision = new IntrinsicTest
		{
			DescShort = Translate("ValiSelfCollisionTitle"),
			DescLong = Translate("ValiSelfCollisionDesc"),
			ImagePath = ImageBasePath + "ValiSelfCollision.png",
			IsActive = Config.ValiSelfCollision,
			Parameters = new ObservableCollection<IntrinsicTest.Parameter>()
		};
		testSelfCollision.Save = delegate
		{
			Config.ValiSelfCollision = testSelfCollision.IsActive;
		};
		IntrinsicTests.Add(testSelfCollision);
		ValiDistBendingAlpha = Config.ValiDistanceBendingNeighborEdgesAngle * 180.0 / Math.PI;
		List<string> source = new List<string>
		{
			"SimpleHole", "Border", "CounterSink", "TwoSidedCounterSink", "EmbossedCounterSink", "Bolt", "Louver", "BridgeLance", "EmbossmentStamp", "Deepening",
			"BlindHole", "ConicBlindHole", "SphericalBlindHole", "Lance", "EmbossedCircle", "EmbossedLine", "EmbossedSquare", "EmbossedRectangle", "EmbossedSquareRounded", "EmbossedRectangleRounded",
			"EmbossedFreeform"
		};
		ValiMacros = new ObservableCollection<ValidationMacro>(source.Select((string x) => new ValidationMacro
		{
			Type = x,
			Desc = (((string)Application.Current.FindResource("l_popup.PnInterfaceSettings.Macro" + x)) ?? x)
		}));
		Dictionary<string, ValidationMacro> dictMacro = ValiMacros.ToDictionary((ValidationMacro x) => x.Type);
		ValiGroups = new ObservableCollection<ValidationGroup>(from x in Config.ValiDistancesGroups.Select(delegate(ValidationSettingsConfig.ValiDistGroup gr)
			{
				ValidationGroup validationGroup = new ValidationGroup
				{
					Desc = gr.Desc,
					IsBendingZone = (gr.Ind == 1),
					Ind = gr.Ind
				};
				foreach (string item in gr.MacroTypes.Where((string x) => dictMacro.ContainsKey(x)))
				{
					validationGroup.Macros.Add(dictMacro[item]);
					dictMacro.Remove(item);
				}
				return validationGroup;
			})
			orderby x.Ind
			select x);
		ValidationGroup validationGroup2 = ValiGroups.First((ValidationGroup x) => x.Ind == 1);
		ValidationGroup validationGroup3 = ValiGroups.First((ValidationGroup x) => x.Ind == 2);
		validationGroup2.Macros.Insert(0, new ValidationMacro
		{
			Desc = Translate("ValiGroupBending"),
			IsConstElement = true
		});
		validationGroup3.Macros.Insert(0, new ValidationMacro
		{
			Desc = Translate("ValiGroupEdges"),
			IsConstElement = true
		});
		UnassignedMacros = new ObservableCollection<ValidationMacro>(dictMacro.Values);
		foreach (ValidationGroup valiGroup in ValiGroups)
		{
			foreach (ValidationGroup valiGroup2 in ValiGroups)
			{
				valiGroup.Params.Add(new ValidationParam
				{
					Origin = valiGroup,
					Target = valiGroup2
				});
			}
		}
		if (Config.ValiDistancesParams != null)
		{
			foreach (var item2 in from m in Config.ValiDistancesParams
				join vm in ValiGroups.SelectMany((ValidationGroup x) => x.Params) on new Tuple<int, int>(m.OriginGroup, m.TargetGroup) equals new Tuple<int, int>(vm.Origin.Ind, vm.Target.Ind)
				select new { m, vm })
			{
				item2.vm.Active = !item2.m.Ignore;
				item2.vm.Const = item2.m.Const;
				item2.vm.ConstMin = item2.m.ConstMin;
				item2.vm.ConstMax = item2.m.ConstMax;
				item2.vm.Thickness = item2.m.Thickness;
				item2.vm.RadiusConcaveBending = item2.m.RadiusConcave;
			}
		}
		return this;
	}

	private void modifyConfig(ValidationSettingsConfig validationSettingsConfig)
	{
		if (validationSettingsConfig.DefaultValues)
		{
			validationSettingsConfig.ValiDistancesGroups.First((ValidationSettingsConfig.ValiDistGroup x) => x.Ind == 1).Desc = Translate("ValiGroupBending");
			validationSettingsConfig.ValiDistancesGroups.First((ValidationSettingsConfig.ValiDistGroup x) => x.Ind == 2).Desc = Translate("ValiGroupEdges");
			validationSettingsConfig.ValiDistancesGroups.First((ValidationSettingsConfig.ValiDistGroup x) => x.Ind == 3).Desc = Translate("ValiGroupEmbossed");
		}
	}

	public void Save()
	{
		foreach (IntrinsicTest intrinsicTest in IntrinsicTests)
		{
			intrinsicTest.Save?.Invoke();
		}
		Config.ValiDistanceBendingNeighborEdgesAngle = ValiDistBendingAlpha * Math.PI / 180.0;
		Config.ValiDistancesGroups = new List<ValidationSettingsConfig.ValiDistGroup>(ValiGroups.Select((ValidationGroup x) => new ValidationSettingsConfig.ValiDistGroup(x.Ind, x.Desc, (from m in x.Macros
			where !m.IsConstElement
			select m.Type).ToList())));
		Config.ValiDistancesParams = new List<ValidationSettingsConfig.ValiDistParam>(from x in ValiGroups.SelectMany((ValidationGroup x) => x.Params)
			where x.Active || x.Const != 0.0 || x.ConstMax != 0.0 || x.ConstMin != 0.0 || x.RadiusConcaveBending != 0.0 || x.Thickness != 0.0
			select new ValidationSettingsConfig.ValiDistParam
			{
				OriginGroup = x.Origin.Ind,
				TargetGroup = x.Target.Ind,
				Const = x.Const,
				ConstMin = x.ConstMin,
				ConstMax = x.ConstMax,
				Ignore = !x.Active,
				Thickness = x.Thickness,
				RadiusConcave = x.RadiusConcaveBending
			});
		Config.DefaultValues = false;
		ValidationSettingsConfig validationSettingsConfig = ValidationSettingsConfig.ValidationSettingsDefaults();
		modifyConfig(validationSettingsConfig);
		validationSettingsConfig.DefaultValues = false;
		Config.ValidationTestsAuto = AutoValidationOnFileLoaded;
		validationSettingsConfig.ValidationTestsAuto = AutoValidationOnFileLoaded;
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(ValidationSettingsConfig), new Type[2]
		{
			typeof(ValidationSettingsConfig.ValiDistGroup),
			typeof(ValidationSettingsConfig.ValiDistParam)
		});
		MemoryStream memoryStream = new MemoryStream();
		xmlSerializer.Serialize(memoryStream, validationSettingsConfig);
		MemoryStream memoryStream2 = new MemoryStream();
		xmlSerializer.Serialize(memoryStream2, Config);
		if (memoryStream.GetBuffer().SequenceEqual(memoryStream2.GetBuffer()))
		{
			Model.ValidationGeometryConfig = new ValidationSettingsConfig
			{
				ValidationTestsAuto = AutoValidationOnFileLoaded
			};
		}
		else
		{
			Model.ValidationGeometryConfig = Config;
		}
	}

	private bool CanDeleteGroup(ValidationGroup obj)
	{
		if (obj != null && obj.Ind > 2)
		{
			return obj.Macros.Count == 0;
		}
		return false;
	}

	private void DeleteGroup(ValidationGroup obj)
	{
		ValiGroups.Remove(obj);
		foreach (ValidationGroup valiGroup in ValiGroups)
		{
			foreach (ValidationParam item in valiGroup.Params.Where((ValidationParam x) => x.Target == obj).ToList())
			{
				valiGroup.Params.Remove(item);
			}
		}
	}

	private void AddNewGroup(object obj)
	{
		ValidationGroup grNew = new ValidationGroup
		{
			Desc = "",
			IsBendingZone = false,
			Ind = Math.Max(2, ValiGroups.Max((ValidationGroup x) => x.Ind)) + 1
		};
		grNew.Params = new ObservableCollection<ValidationParam>(ValiGroups.Select((ValidationGroup x) => new ValidationParam
		{
			Origin = grNew,
			Target = x
		}));
		ValiGroups.Add(grNew);
		foreach (ValidationGroup valiGroup in ValiGroups)
		{
			valiGroup.Params.Add(new ValidationParam
			{
				Origin = valiGroup,
				Target = grNew
			});
		}
	}
}
