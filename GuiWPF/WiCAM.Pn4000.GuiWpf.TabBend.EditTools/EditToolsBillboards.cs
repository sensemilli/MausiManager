using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.SubViews;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsBillboards : IEditToolsBillboards
{
	private class SubAddAdaptersViewModel : ICustomAutoCompleteBoxViewModel
	{
		public class SubAdaptersViewModel : ICustomAutoCompleteBoxViewModel.IItem
		{
			public IToolProfile Profile { get; }

			public string CustomAutoCompleteBoxItemDisplayName => Profile.Name ?? "";

			public SubAdaptersViewModel(IToolProfile profile)
			{
				Profile = profile;
			}
		}

		private readonly List<SubAdaptersViewModel> _adapterProfiles;

		public IEnumerable<ICustomAutoCompleteBoxViewModel.IItem> CustomAutoCompleteBoxItems => ((IEnumerable<SubAdaptersViewModel>)_adapterProfiles).Select((Func<SubAdaptersViewModel, ICustomAutoCompleteBoxViewModel.IItem>)((SubAdaptersViewModel x) => x));

		public SubAddAdaptersViewModel(IEnumerable<IToolProfile> adapterProfiles)
		{
			_adapterProfiles = adapterProfiles.Select((IToolProfile x) => new SubAdaptersViewModel(x)).ToList();
		}
	}

	private readonly IScopedFactorio _factorio;

	private readonly IScreen3DMain _screen3D;

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IEditToolsViewModel _cmd;

	private readonly IRadGlyphConverter _glyphConverter;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IEditToolsExtendPiecesVisualizer _extendPiecesVisualizer;

	private readonly IEditToolsDragPiecesVisualizer _dragPiecesVisualizer;

	private readonly IEditToolsAddPieceVisualizer _addPieceVisualizer;

	private readonly IBendSelection _bendSelection;

	private readonly IToolCalculations _toolCalculations;

	private readonly IToolOperator _toolOperator;

	private readonly ShortcutEditTools _shortcutEditTools;

	private readonly IPnBndDoc _doc;

	private readonly ICollection<IBillboard> _billboards = new List<IBillboard>();

	private readonly ICollection<IBillboard> _billboardsForOnePieceOrSection = new List<IBillboard>();

	private readonly ICollection<IBillboard> _billboardsForSection = new List<IBillboard>();

	private IBillboard _billboardAddAdapterTop;

	private IBillboard _billboardAddAdapterBottom;

	private IBillboard _billboardAddAdapterBack;

	private IBillboard _billboardAddAdapterFront;

	private IBillboard _billboardAcb;

	private readonly BackgroundStyle _background;

	private readonly BackgroundStyle _backgroundHover;

	private readonly BackgroundStyle _backgroundMouseDown;

	private readonly GlyphStyle _glyphStyle1;

	private readonly GlyphStyle _glyphStyle2;

	private bool _billboardsInitialized;

	private Canvas ScreenOverlay => _screen3D.Screen3D.Overlay;

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	private IEnumerable<IBillboard> AllBillboards => from x in _billboards.Concat(_billboardsForSection).Concat(_billboardsForOnePieceOrSection).Concat(new _003C_003Ez__ReadOnlyArray<IBillboard>(new IBillboard[5] { _billboardAddAdapterTop, _billboardAddAdapterBottom, _billboardAddAdapterBack, _billboardAddAdapterFront, _billboardAcb }))
		where x != null
		select x;

	public event Action? RequestRepaint;

	public EditToolsBillboards(IEditToolsSelection toolsSelection, IScreen3DMain screen3D, IStyleProvider styles, IRadGlyphConverter glyphConverter, IEditToolsViewModel editToolsViewModel, IPnBndDoc doc, IShortcutSettingsCommon shortcutSettingsCommon, ShortcutEditTools shortcutEditTools, IScopedFactorio factorio, IEditToolsExtendPiecesVisualizer extendPiecesVisualizer, IEditToolsDragPiecesVisualizer dragPiecesVisualizer, IEditToolsAddPieceVisualizer addPieceVisualizer, IBendSelection bendSelection, IToolCalculations toolCalculations, IToolOperator toolOperator)
	{
		_toolsSelection = toolsSelection;
		_screen3D = screen3D;
		_glyphConverter = glyphConverter;
		_cmd = editToolsViewModel;
		_doc = doc;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_shortcutEditTools = shortcutEditTools;
		_factorio = factorio;
		_extendPiecesVisualizer = extendPiecesVisualizer;
		_dragPiecesVisualizer = dragPiecesVisualizer;
		_addPieceVisualizer = addPieceVisualizer;
		_bendSelection = bendSelection;
		_toolCalculations = toolCalculations;
		_toolOperator = toolOperator;
		_background = new BackgroundStyle
		{
			Color = styles.AccentBackgroundColor,
			BorderColor = styles.AccentBorderColor,
			BorderThickness = 5f,
			Padding = 5f,
			MinWidth = 50f,
			MinHeight = 50f
		};
		BackgroundStyle background = _background;
		background.Color = styles.AccentMouseOverBackgroundColor;
		background.BorderColor = styles.AccentMouseOverBorderColor;
		_backgroundHover = background;
		background = _background;
		background.Color = styles.AccentPressedBackgroundColor;
		background.BorderColor = styles.AccentMouseOverBorderColor;
		_backgroundMouseDown = background;
		_glyphStyle1 = new GlyphStyle
		{
			Size = 30,
			Color = styles.AccentForegroundColor
		};
		GlyphStyle glyphStyle = _glyphStyle1;
		glyphStyle.Size = 40;
		_glyphStyle2 = glyphStyle;
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
		if (_dragPiecesVisualizer.IsDragging)
		{
			_dragPiecesVisualizer.MouseMove(sender, e);
		}
		if (_extendPiecesVisualizer.IsActive)
		{
			_extendPiecesVisualizer.MouseMove(sender, e);
		}
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
		if (e.Handled)
		{
			return;
		}
		if (_dragPiecesVisualizer.IsDragging)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = e.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				e.Handle();
				double num = _dragPiecesVisualizer.Stop();
				int? indexForDistance = _dragPiecesVisualizer.GetIndexForDistance(num);
				if (indexForDistance.HasValue)
				{
					_cmd.CmdMoveToIndexInSection.ExecuteWithParameter(indexForDistance.Value);
				}
				else
				{
					_cmd.CmdMoveByDistanceAndMerge.ExecuteWithParameter(num);
				}
			}
			else
			{
				MouseButtonEventArgs? mouseButtonEventArgs2 = e.MouseButtonEventArgs;
				if ((mouseButtonEventArgs2 != null && mouseButtonEventArgs2.ChangedButton == MouseButton.Right) || _shortcutSettingsCommon.Cancel.IsShortcut(e))
				{
					e.Handle();
					_dragPiecesVisualizer.Stop();
					this.RequestRepaint?.Invoke();
				}
			}
		}
		if (e.Handled || !_extendPiecesVisualizer.IsActive)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs3 = e.MouseButtonEventArgs;
		if (mouseButtonEventArgs3 != null && mouseButtonEventArgs3.ChangedButton == MouseButton.Left)
		{
			e.Handle();
			IDictionary<IToolSection, double> parameter = _extendPiecesVisualizer.Stop();
			if (_extendPiecesVisualizer.IsExtendingLeft)
			{
				_cmd.CmdExtendSectionsLeft.ExecuteWithParameter(parameter);
			}
			else
			{
				_cmd.CmdExtendSectionsRight.ExecuteWithParameter(parameter);
			}
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs4 = e.MouseButtonEventArgs;
		if ((mouseButtonEventArgs4 != null && mouseButtonEventArgs4.ChangedButton == MouseButton.Right) || _shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			e.Handle();
			_extendPiecesVisualizer.Stop();
			this.RequestRepaint?.Invoke();
		}
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		KeyUp(sender, e.Args);
		if (!e.Args.Handled)
		{
			HideBillboards();
			HidePopups();
		}
	}

	public void ColorModelParts(IPaintTool painter)
	{
		if (_dragPiecesVisualizer.IsDragging)
		{
			_dragPiecesVisualizer.ColorModelParts(painter);
		}
		if (_extendPiecesVisualizer.IsActive)
		{
			_extendPiecesVisualizer.ColorModelParts(painter);
		}
		if (_addPieceVisualizer.IsActive)
		{
			_addPieceVisualizer.ColorModelParts(painter);
		}
	}

	public void SetActive(bool active)
	{
		if (active)
		{
			_billboardsInitialized = false;
		}
	}

	public void ShowBillboards(Vector3d position)
	{
		if (!_toolsSelection.SelectionAsPieces.Any())
		{
			return;
		}
		if (!_billboardsInitialized)
		{
			_billboardsInitialized = true;
			CreateBillboards();
		}
		if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection && _toolsSelection.SelectedSections.Count() == 1)
		{
			IToolSection toolSection = _toolsSelection.SelectedSections.First();
			IBillboard billboard = (toolSection.IsUpperSection ? _billboardAddAdapterTop : _billboardAddAdapterBottom);
			billboard.Center = position;
			billboard.IsVisible = true;
			ScreenD3D.UpdateBillboardPosition(billboard, render: false);
			if (!toolSection.IsUpperSection)
			{
				_billboardAddAdapterFront.Center = position;
				_billboardAddAdapterFront.IsVisible = true;
				ScreenD3D.UpdateBillboardPosition(_billboardAddAdapterFront, render: false);
				_billboardAddAdapterBack.Center = position;
				_billboardAddAdapterBack.IsVisible = true;
				ScreenD3D.UpdateBillboardPosition(_billboardAddAdapterBack, render: false);
			}
		}
		if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection)
		{
			foreach (IBillboard item in _billboardsForSection)
			{
				item.Center = position;
				item.IsVisible = true;
				ScreenD3D.UpdateBillboardPosition(item, render: false);
			}
		}
		if ((_toolsSelection.SelectionMode == EditToolSelectionModes.ToolPiece && _toolsSelection.SelectedPieces.Count() == 1) || (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection && _toolsSelection.SelectedSections.Count() == 1))
		{
			foreach (IBillboard item2 in _billboardsForOnePieceOrSection)
			{
				item2.Center = position;
				item2.IsVisible = true;
				ScreenD3D.UpdateBillboardPosition(item2, render: false);
			}
		}
		if (_toolsSelection.SelectionAsPieces.All((IToolPiece x) => x.PieceProfile.SpecialToolType.HasFlag(SpecialToolTypes.Acb) || x.PieceProfile.SpecialToolType.HasFlag(SpecialToolTypes.AcbWireless)))
		{
			_billboardAcb.IsVisible = true;
			_billboardAcb.Center = position;
			ScreenD3D.UpdateBillboardPosition(_billboardAcb, render: false);
		}
		foreach (IBillboard billboard2 in _billboards)
		{
			billboard2.Center = position;
			billboard2.IsVisible = true;
			ScreenD3D.UpdateBillboardPosition(billboard2, render: false);
		}
		ScreenD3D.Render(skipQueuedFrames: false);
	}

	public void HideBillboards()
	{
		foreach (IBillboard allBillboard in AllBillboards)
		{
			allBillboard.IsVisible = false;
		}
		ScreenD3D.Render(skipQueuedFrames: false);
	}

	private void CreateBillboards()
	{
		foreach (IBillboard allBillboard in AllBillboards)
		{
			ScreenD3D.RemoveBillboard(allBillboard, render: false);
		}
		_billboards.Clear();
		_billboardsForSection.Clear();
		_billboardsForOnePieceOrSection.Clear();
		IButtonBillboard buttonBillboard = CreateBillboard(new Vector2d(-100.0, 30.0), "&#xe01a;", _glyphStyle2);
		buttonBillboard.OnClick += MoveLeft;
		_billboards.Add(buttonBillboard);
		IButtonBillboard buttonBillboard2 = CreateBillboard(new Vector2d(100.0, 30.0), "&#xe018;", _glyphStyle2);
		buttonBillboard2.OnClick += MoveRight;
		_billboards.Add(buttonBillboard2);
		IButtonBillboard buttonBillboard3 = CreateBillboard(new Vector2d(-30.0, 30.0), "&#xe141;", _glyphStyle1);
		buttonBillboard3.OnClick += StartDragging;
		_billboards.Add(buttonBillboard3);
		IButtonBillboard buttonBillboard4 = CreateBillboard(new Vector2d(30.0, 30.0), "&#xe507;", _glyphStyle1);
		buttonBillboard4.OnClick += Flip;
		_billboards.Add(buttonBillboard4);
		IButtonBillboard buttonBillboard5 = CreateBillboard(new Vector2d(-100.0, -30.0), "&#xe11e;", _glyphStyle2);
		buttonBillboard5.OnClick += StartAddLeft;
		_billboardsForOnePieceOrSection.Add(buttonBillboard5);
		IButtonBillboard buttonBillboard6 = CreateBillboard(new Vector2d(100.0, -30.0), "&#xe11e;", _glyphStyle2);
		buttonBillboard6.OnClick += StartAddRight;
		_billboardsForOnePieceOrSection.Add(buttonBillboard6);
		IButtonBillboard buttonBillboard7 = CreateBillboard(new Vector2d(0.0, -30.0), "&#xe10c;", _glyphStyle1);
		buttonBillboard7.OnClick += Delete;
		_billboards.Add(buttonBillboard7);
		IButtonBillboard buttonBillboard8 = CreateBillboard(new Vector2d(-100.0, -90.0), "&#xe542;", _glyphStyle1);
		buttonBillboard8.OnClick += StartExtendLeft;
		_billboardsForSection.Add(buttonBillboard8);
		IButtonBillboard buttonBillboard9 = CreateBillboard(new Vector2d(100.0, -90.0), "&#xe540;", _glyphStyle1);
		buttonBillboard9.OnClick += StartExtendRight;
		_billboardsForSection.Add(buttonBillboard9);
		_billboardAddAdapterTop = CreateBillboard(new Vector2d(0.0, 100.0), "&#xe54f;", _glyphStyle1);
		_billboardAddAdapterTop.OnClick += StartAddingAdapter;
		_billboardAddAdapterBottom = CreateBillboard(new Vector2d(0.0, -100.0), "&#xe551;", _glyphStyle1);
		_billboardAddAdapterBottom.OnClick += StartAddingAdapter;
		_billboardAddAdapterBack = CreateBillboard(new Vector2d(-60.0, -100.0), "&#xe052;", _glyphStyle1);
		_billboardAddAdapterBack.OnClick += StartAddingExtension;
		_billboardAddAdapterFront = CreateBillboard(new Vector2d(60.0, -100.0), "&#xe054;", _glyphStyle1);
		_billboardAddAdapterFront.OnClick += StartAddingExtension;
		_billboardAcb = CreateBillboard(new Vector2d(100.0, -100.0), "&#xe551;", _glyphStyle1);
		_billboardAcb.OnClick += ToggleAcb;
		foreach (IBillboard allBillboard2 in AllBillboards)
		{
			ScreenD3D.AddBillboard(allBillboard2, render: false);
		}
	}

	private void MoveLeft(IPnInputEventArgs args, IBillboard _)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			args.Handle();
			if (IsCamaraInFront())
			{
				_cmd.CmdMoveLeft.Execute(null);
			}
			else
			{
				_cmd.CmdMoveRight.Execute(null);
			}
		}
	}

	private void MoveRight(IPnInputEventArgs args, IBillboard _)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			args.Handle();
			if (IsCamaraInFront())
			{
				_cmd.CmdMoveRight.Execute(null);
			}
			else
			{
				_cmd.CmdMoveLeft.Execute(null);
			}
		}
	}

	private void StartDragging(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				_dragPiecesVisualizer.Start();
				this.RequestRepaint?.Invoke();
			}
		}
	}

	private void Flip(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				_cmd.CmdFlipTools.Execute(null);
			}
		}
	}

	private void StartAddLeft(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				ShowAddPiecePopup(args, IsCamaraInFront());
			}
		}
	}

	private void StartAddRight(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				ShowAddPiecePopup(args, !IsCamaraInFront());
			}
		}
	}

	private void StartExtendLeft(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				_extendPiecesVisualizer.Start(IsCamaraInFront());
				this.RequestRepaint?.Invoke();
			}
		}
	}

	private void StartExtendRight(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				_extendPiecesVisualizer.Start(!IsCamaraInFront());
				this.RequestRepaint?.Invoke();
			}
		}
	}

	private void Delete(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
				_cmd.CmdDeleteTools.Execute(null);
			}
		}
	}

	private void StartAddingAdapter(IPnInputEventArgs args, IBillboard billboard)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				ShowAddAdapterPopup(args, billboard);
				HideBillboards();
			}
		}
	}

	private void StartAddingExtension(IPnInputEventArgs args, IBillboard billboard)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				ShowAddDieExtentionPopup(args, billboard);
				HideBillboards();
			}
		}
	}

	private void ToggleAcb(IPnInputEventArgs args, IBillboard _)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs == null || mouseButtonEventArgs.ChangedButton != 0)
		{
			return;
		}
		Dictionary<IAcbPunchPiece, AcbActivationResult> dictionary = _bendSelection.CurrentBend?.AcbStatus;
		IToolSetups toolSetups = _bendSelection.CurrentBend?.Anchor?.Root;
		if (dictionary != null && toolSetups != null)
		{
			foreach (IToolPiece selectionAsPiece in _toolsSelection.SelectionAsPieces)
			{
				if (selectionAsPiece.ToolSection.Cluster.Root == toolSetups && selectionAsPiece is IAcbPunchPiece key)
				{
					AcbActivationResult valueOrDefault = dictionary.GetValueOrDefault(key, AcbActivationResult.None);
					if (!valueOrDefault.HasFlag(AcbActivationResult.UserActivated))
					{
						valueOrDefault = ((!valueOrDefault.HasFlag(AcbActivationResult.UserDeactivated)) ? (valueOrDefault | AcbActivationResult.UserActivated) : (valueOrDefault ^ AcbActivationResult.UserDeactivated));
					}
					else
					{
						valueOrDefault ^= AcbActivationResult.UserActivated;
						valueOrDefault |= AcbActivationResult.UserDeactivated;
					}
					dictionary[key] = valueOrDefault;
				}
			}
		}
		this.RequestRepaint?.Invoke();
		args.Handle();
	}

	private void Test(IPnInputEventArgs args, IBillboard _)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				HideBillboards();
			}
		}
	}

	private bool IsCamaraInFront()
	{
		return _screen3D.ScreenD3D.IsCamaraInFront();
	}

	private void ShowAddPiecePopup(IPnInputEventArgs args, bool addLeft)
	{
		IToolPiece toolPiece = _toolsSelection.SelectionAsPieces.FirstOrDefault();
		if (toolPiece == null || args.MouseEventArgs == null)
		{
			return;
		}
		ISubToolPieceViewModel subToolPieceViewModel = _factorio.Resolve<ISubToolPieceViewModel>();
		subToolPieceViewModel.Init(_doc, toolPiece);
		ShowPopup(args, subToolPieceViewModel, delegate(ToolPieceProfileVm x)
		{
			if (addLeft)
			{
				_cmd.CmdAddToolPieceLeft.ExecuteWithParameter(x.PieceProfile);
			}
			else
			{
				_cmd.CmdAddToolPieceRight.ExecuteWithParameter(x.PieceProfile);
			}
		}, delegate(ToolPieceProfileVm x)
		{
			_addPieceVisualizer.AddPiece(x.PieceProfile, addLeft);
		});
		_addPieceVisualizer.StartAddingPieces(addLeft);
		this.RequestRepaint?.Invoke();
	}

	private void ShowAddAdapterPopup(IPnInputEventArgs args, IBillboard billboard)
	{
		IToolSection section = _toolsSelection.SelectedSections.FirstOrDefault();
		if (section == null || args.MouseEventArgs == null)
		{
			return;
		}
		IToolManager toolManager = _doc.ToolManager;
		IEnumerable<IToolProfile> source = new List<IToolProfile>();
		if (billboard == _billboardAddAdapterTop)
		{
			source = from x in toolManager.GetUpperAdapterProfiles()
				where x.AdapterDirection == AdapterDirections.TopDown
				select x;
		}
		else if (billboard == _billboardAddAdapterBottom)
		{
			source = from x in toolManager.GetLowerAdapterProfiles()
				where x.AdapterDirection == AdapterDirections.TopDown
				select x;
		}
		IToolCalculationOption options = _toolCalculations.CreateDefaultOptions(_doc);
		IToolListManager toolListManager = options.ToolListManager;
		Dictionary<IAliasPieceProfile, int> usedAmounts = _toolOperator.CountToolProfileAmount(section.Cluster.Root);
		source = source.Where((IToolProfile x) => section.MultiToolProfile.ToolProfiles.Select((IToolProfile p) => p.MountTypeId).Contains(x.MountTypeChildId.Value) && x.PieceProfiles.Any((IToolPieceProfile pp) => options.IgnoreToolPieceNumbers || toolListManager.GetAvailableAmount(pp, usedAmounts) > 0));
		SubAddAdaptersViewModel viewModel = new SubAddAdaptersViewModel(source.Where((IToolProfile x) => x.Implemented));
		ShowPopup(args, viewModel, delegate(SubAddAdaptersViewModel.SubAdaptersViewModel x)
		{
			_cmd.CmdAddAdapterToSection.ExecuteWithParameter(x.Profile as IAdapterProfile);
		}, delegate(SubAddAdaptersViewModel.SubAdaptersViewModel x)
		{
			_addPieceVisualizer.AddAdapter(x.Profile as IAdapterProfile);
		});
		_addPieceVisualizer.StartAddingAdapters();
		this.RequestRepaint?.Invoke();
	}

	private void ShowAddDieExtentionPopup(IPnInputEventArgs args, IBillboard billboard)
	{
		IToolSection toolSection = _toolsSelection.SelectedSections.FirstOrDefault();
		if (toolSection == null || args.MouseEventArgs == null)
		{
			return;
		}
		IToolManager toolManager = _doc.ToolManager;
		IToolCalculationOption options = _toolCalculations.CreateDefaultOptions(_doc);
		IToolListManager toolListManager = options.ToolListManager;
		Dictionary<IAliasPieceProfile, int> usedAmounts = _toolOperator.CountToolProfileAmount(toolSection.Cluster.Root);
		IEnumerable<IToolProfile> source = new List<IToolProfile>();
		List<IToolProfile> source2 = new List<IToolProfile>();
		if (billboard == _billboardAddAdapterFront)
		{
			source2 = toolSection.MultiToolProfile.ToolProfiles.Where((IToolProfile x) => x.IsAdapter && x.AdapterDirection == AdapterDirections.Front).ToList();
			HashSet<int?> mountIds = source2.Select((IToolProfile x) => x.MountTypeChildId).ToHashSet();
			source = from x in toolManager.GetLowerToolExtensionProfiles()
				where mountIds.Contains(x.MountTypeId) && x.PieceProfiles.Any((IToolPieceProfile pp) => options.IgnoreToolPieceNumbers || toolListManager.GetAvailableAmount(pp, usedAmounts) > 0)
				select x;
		}
		else if (billboard == _billboardAddAdapterBack)
		{
			source2 = toolSection.MultiToolProfile.ToolProfiles.Where((IToolProfile x) => x.IsAdapter && x.AdapterDirection == AdapterDirections.Back).ToList();
			HashSet<int?> mountIds2 = source2.Select((IToolProfile x) => x.MountTypeChildId).ToHashSet();
			source = from x in toolManager.GetLowerToolExtensionProfiles()
				where mountIds2.Contains(x.MountTypeId) && x.PieceProfiles.Any((IToolPieceProfile pp) => options.IgnoreToolPieceNumbers || toolListManager.GetAvailableAmount(pp, usedAmounts) > 0)
				select x;
		}
		SubAddAdaptersViewModel viewModel = new SubAddAdaptersViewModel(source.Where((IToolProfile x) => x.Implemented));
		ShowPopup(args, viewModel, delegate(SubAddAdaptersViewModel.SubAdaptersViewModel x)
		{
			_cmd.CmdAddExtensionToSection.ExecuteWithParameter(x.Profile);
		}, delegate(SubAddAdaptersViewModel.SubAdaptersViewModel x)
		{
			_addPieceVisualizer.AddExtension(x.Profile as IDieFoldExtentionProfile);
		});
		_addPieceVisualizer.StartAddingExtensions(source2.FirstOrDefault());
		this.RequestRepaint?.Invoke();
	}

	private void ShowPopup<T>(IPnInputEventArgs args, ICustomAutoCompleteBoxViewModel viewModel, Action<T> addPiece, Action<T> visualize) where T : ICustomAutoCompleteBoxViewModel.IItem
	{
		HidePopups();
		if (_toolsSelection.SelectionAsPieces.FirstOrDefault() == null || args.MouseEventArgs == null)
		{
			return;
		}
		Point position = args.MouseEventArgs.GetPosition(ScreenOverlay);
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			CustomAutoCompleteBox<T> customAutoCompleteBox = new CustomAutoCompleteBox<T>(viewModel);
			RadAutoCompleteBox dropdown = customAutoCompleteBox.DropDown;
			dropdown.SelectionChanged += delegate
			{
				if (dropdown.SelectedItem is T obj)
				{
					addPiece(obj);
				}
			};
			customAutoCompleteBox.SelectedItemChanged += visualize;
			customAutoCompleteBox.Loaded += delegate
			{
				dropdown.Focus();
				dropdown.Populate(string.Empty);
				dropdown.LostMouseCapture += delegate
				{
					HidePopups();
					this.RequestRepaint?.Invoke();
				};
			};
			ScreenOverlay.Children.Add(customAutoCompleteBox);
			Canvas.SetLeft(customAutoCompleteBox, position.X - dropdown.Width / 2.0);
			Canvas.SetTop(customAutoCompleteBox, position.Y);
		});
	}

	private void HidePopups()
	{
		_addPieceVisualizer.Stop();
		Application.Current.Dispatcher.Invoke(ScreenOverlay.Children.Clear);
	}

	private void UpdateScreen(IBillboard b)
	{
		ScreenD3D.UpdateBillboardAppearance(b);
	}

	private IButtonBillboard CreateBillboard(Vector2d offset, string glyph, GlyphStyle glyphStyle)
	{
		return new ButtonBillboard(UpdateScreen)
		{
			Content = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _background,
				GlyphStyle = glyphStyle
			},
			HoverContent = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _backgroundHover,
				GlyphStyle = glyphStyle
			},
			MouseDownContent = new GlyphContent(_glyphConverter)
			{
				Glyph = glyph,
				Background = _backgroundMouseDown,
				GlyphStyle = glyphStyle
			},
			IsVisible = false,
			IsInteractive = true,
			Offset = offset
		};
	}
}
