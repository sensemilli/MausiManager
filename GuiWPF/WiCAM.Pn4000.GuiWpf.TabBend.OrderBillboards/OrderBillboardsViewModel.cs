using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend.OrderBillboards;

internal class OrderBillboardsViewModel : ViewModelBase, IOrderBillboardsViewModel, IStyleReceiver
{
	private const int DefaultLayer = 2;

	private const int SelectedLayer = 1;

	private const int HoveredLayer = 0;

	private bool _appendLetters;

	private bool _showBillboards;

	private readonly ConcurrentDictionary<IBendDescriptor, IBillboard> _billboardLookup = new ConcurrentDictionary<IBendDescriptor, IBillboard>();

	private ICombinedBendDescriptorInternal? _maxCombinedBend;

	private readonly IScreen3DDoc _screenDoc;

	private readonly IConfigProvider _configProvider;

	private readonly IStyleProvider _styleProvider;

	public bool ShowBillboards
	{
		get
		{
			return _showBillboards;
		}
		set
		{
			if (value)
			{
				SetBillboardState(showNumber: true, null);
			}
			else
			{
				SetBillboardState(showNumber: false, null);
			}
		}
	}

	public bool HideBillboards
	{
		get
		{
			return !_showBillboards;
		}
		set
		{
			if (value)
			{
				SetBillboardState(showNumber: false, null);
			}
		}
	}

	public bool ShowNumbers
	{
		get
		{
			if (_showBillboards)
			{
				return !_appendLetters;
			}
			return false;
		}
		set
		{
			if (value)
			{
				SetBillboardState(showNumber: true, false);
			}
		}
	}

	public bool ShowNumbersAndLetters
	{
		get
		{
			if (_showBillboards)
			{
				return _appendLetters;
			}
			return false;
		}
		set
		{
			if (value)
			{
				SetBillboardState(showNumber: true, true);
			}
		}
	}

	public double SizeMultiplier
	{
		get
		{
			return _styleProvider.BillboardSizeMultiplier;
		}
		set
		{
			_styleProvider.BillboardSizeMultiplier = value;
			NotifyPropertyChanged("SizeMultiplier");
		}
	}

	public double SizeMultiplierLog
	{
		get
		{
			return Math.Log10(SizeMultiplier);
		}
		set
		{
			SizeMultiplier = Math.Pow(10.0, value);
		}
	}

	public static double MinSizeMultiplier => -1.0;

	public static double MaxSizeMultiplier => 0.5;

	public IDoc3d CurrentDoc { get; set; }

	private ScreenD3D11? _screen3D => _screenDoc.Screen?.ScreenD3D;

	public OrderBillboardsViewModel(IScreen3DDoc screen3D, IConfigProvider configProvider, IStyleProvider styleProvider, IDoc3d doc)
	{
		_screenDoc = screen3D;
		_configProvider = configProvider;
		_styleProvider = styleProvider;
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		_showBillboards = generalUserSettingsConfig.BendOrderBillboardShow;
		_appendLetters = generalUserSettingsConfig.BendOrderBillboardAppendLetter;
		_styleProvider.RegisterBillboardStilesChanged(this);
	}

	private void SetBillboardState(bool showNumber, bool? showLetter)
	{
		_showBillboards = showNumber;
		if (showLetter.HasValue)
		{
			_appendLetters = showLetter.Value;
		}
		NotifyPropertyChanged("ShowBillboards");
		NotifyPropertyChanged("HideBillboards");
		NotifyPropertyChanged("ShowNumbers");
		NotifyPropertyChanged("ShowNumbersAndLetters");
		UpdateOrAddBillboards(render: true);
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		generalUserSettingsConfig.BendOrderBillboardShow = showNumber;
		if (showLetter.HasValue)
		{
			generalUserSettingsConfig.BendOrderBillboardAppendLetter = showLetter.Value;
		}
		_configProvider.Push(generalUserSettingsConfig);
		_configProvider.Save<GeneralUserSettingsConfig>();
	}

	public void UpdateOrAddBillboards(bool render)
	{
		HashSet<IBillboard> hashSet = _billboardLookup.Values.ToHashSet();
		int? maxDisplayedOrder = _maxCombinedBend?.Order;
		int uidIndex = 0;
		if (CurrentDoc.CombinedBendDescriptors.Count > 0)
		{
			Dictionary<IBendDescriptor, string> dictionary = (from x in CurrentDoc.BendDescriptors
				orderby x.BendParams.BendFaceGroup.BendEntryId, x.BendParams.BendFaceGroup.ID
				select x).ToDictionary((IBendDescriptor x) => x, (IBendDescriptor x) => uidIndex++.NumberToAlphabet());
			foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in CurrentDoc.CombinedBendDescriptors)
			{
				foreach (IBendDescriptor item2 in combinedBendDescriptor.Enumerable)
				{
					int? fgId = item2.BendParams.BendFaceGroup?.ID;
					if (!fgId.HasValue)
					{
						continue;
					}
					string text = string.Join(",", from cbd in CurrentDoc.CombinedBendDescriptors
						where cbd.Enumerable.Any(delegate(IBendDescriptor e)
						{
							FaceGroup bendFaceGroup = e.BendParams.BendFaceGroup;
							return (bendFaceGroup == null) ? (!fgId.HasValue) : (bendFaceGroup.ID == fgId);
						})
						select (!(cbd.Order > maxDisplayedOrder)) ? (cbd.Order + 1).ToString() : "-");
					if (_appendLetters)
					{
						text = text + " " + dictionary[item2];
					}
					UpdateOrCreateBillboard(item2, text, hashSet).IsVisible = _showBillboards;
				}
			}
		}
		foreach (var (key, item) in _billboardLookup)
		{
			if (hashSet.Contains(item) && _billboardLookup.Remove(key, out var value))
			{
				_screen3D?.RemoveBillboard(value, render: false);
			}
		}
		if (render)
		{
			_screen3D?.Render(skipQueuedFrames: false);
		}
	}

	public void RemoveBillboards(bool render)
	{
		foreach (IBendDescriptor key in _billboardLookup.Keys)
		{
			if (_billboardLookup.Remove(key, out var value))
			{
				_screen3D?.RemoveBillboard(value, render: false);
			}
		}
		if (render)
		{
			_screen3D?.Render(skipQueuedFrames: false);
		}
	}

	public void SetMaxCombinedBend(ICombinedBendDescriptorInternal? combinedBend)
	{
		_maxCombinedBend = combinedBend;
	}

	public void SetSelectedAndHoveredCombinedBend(ICombinedBendDescriptorInternal? selected, ICombinedBendDescriptorInternal? hovered)
	{
		foreach (var (value, billboard2) in _billboardLookup)
		{
			if (selected != null && selected.Enumerable.Contains(value))
			{
				billboard2.SortingLayer = 1;
			}
			else if (hovered != null && hovered.Enumerable.Contains(value))
			{
				billboard2.SortingLayer = 0;
			}
			else
			{
				billboard2.SortingLayer = 2;
			}
		}
	}

	public IBendDescriptor? GetBend(IBillboard billboard)
	{
		return _billboardLookup.FirstOrDefault<KeyValuePair<IBendDescriptor, IBillboard>>((KeyValuePair<IBendDescriptor, IBillboard> x) => x.Value == billboard).Key;
	}

	public ICombinedBendDescriptorInternal? GetCombinedBend(IBillboard billboard)
	{
		IBendDescriptor bend = GetBend(billboard);
		return CurrentDoc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Enumerable.Contains(bend));
	}

	private IBillboard UpdateOrCreateBillboard(IBendDescriptor bend, string billboardText, ICollection<IBillboard> oldBillboards)
	{
		BackgroundStyle background = _styleProvider.BillboardBackgroundStyle;
		Color? color = bend.BendParams.BendFaceGroupModel?.EdgeColor;
		if (color.HasValue)
		{
			background.Color = new Color
			{
				R = 1f - (1f - color.Value.R) * 0.3f,
				G = 1f - (1f - color.Value.G) * 0.3f,
				B = 1f - (1f - color.Value.B) * 0.3f,
				A = 1f
			};
			background.BorderColor = color.Value;
			background.BorderThickness *= 2f;
		}
		TextStyle billboardTextStyle = _styleProvider.BillboardTextStyle;
		billboardTextStyle.FontFamily = "Consolas";
		TextStyle textStyle = billboardTextStyle;
		return _billboardLookup.AddOrUpdate(bend, delegate
		{
			FaceGroup bendFaceGroup = bend.BendParams.BendFaceGroup;
			if (bendFaceGroup == null)
			{
				return (IBillboard)null;
			}
			Vector3d centerPointInModelSpace = bendFaceGroup.GetCenterPointInModelSpace();
			TextContent content = new TextContent
			{
				PlainText = billboardText,
				TextStyle = textStyle,
				Background = background
			};
			Billboard billboard2 = new Billboard
			{
				Center = centerPointInModelSpace,
				Content = content,
				IsVisible = ShowBillboards,
				IsInteractive = true,
				ScaleWithDistance = false,
				RenderOnTop = true,
				SortingLayer = 2
			};
			_screen3D?.AddBillboard(billboard2, bend.BendParams.BendFaceGroupModel, render: false);
			return billboard2;
		}, delegate(IBendDescriptor _, IBillboard billboard)
		{
			if (billboard != null)
			{
				oldBillboards.Remove(billboard);
				if (billboard.Content is ITextContent textContent)
				{
					textContent.PlainText = billboardText;
					textContent.TextStyle = textStyle;
					textContent.Background = background;
				}
				_screen3D?.UpdateBillboardAppearance(billboard, render: false);
			}
			return billboard;
		});
	}

	private void UpdateEnableBillboards()
	{
		foreach (IBillboard value in _billboardLookup.Values)
		{
			value.IsVisible = ShowBillboards;
		}
		_screen3D?.Render(skipQueuedFrames: false);
	}

	public void BillboardStylesChanged()
	{
		UpdateOrAddBillboards(render: true);
	}
}
