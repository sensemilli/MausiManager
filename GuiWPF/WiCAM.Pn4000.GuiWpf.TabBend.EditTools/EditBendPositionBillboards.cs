using System.Collections.Generic;
using System.Windows.Input;
using SharpDX;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditBendPositionBillboards : SubViewModelBase, IEditBendPositionBillboards, ISubViewModel
{
	private const string GlyphMoveLeft = "&#xe01a;";

	private const string GlyphMoveRight = "&#xe018;";

	private const string GlyphMove = "&#xe141;";

	private readonly IScreen3DMain _screen3D;

	private readonly IRadGlyphConverter _glyphConverter;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IEditToolsDragModelVisualizer _dragModelVisualizer;

	private readonly IEditToolsViewModel _cmd;

	private readonly ICollection<IBillboard> _billboards = new List<IBillboard>();

	private readonly BackgroundStyle _background;

	private readonly BackgroundStyle _backgroundHover;

	private readonly BackgroundStyle _backgroundMouseDown;

	private readonly GlyphStyle _glyphStyle1;

	private readonly GlyphStyle _glyphStyle2;

	private bool _billboardsInitialized;

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	public EditBendPositionBillboards(IScreen3DMain screen3D, IStyleProvider styles, IRadGlyphConverter glyphConverter, IShortcutSettingsCommon shortcutSettingsCommon, IEditToolsViewModel cmd, IEditToolsDragModelVisualizer dragModelVisualizer)
	{
		_screen3D = screen3D;
		_glyphConverter = glyphConverter;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_dragModelVisualizer = dragModelVisualizer;
		_cmd = cmd;
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

	public override bool Close()
	{
		HideBillboards();
		_dragModelVisualizer.Stop();
		return base.Close();
	}

	public override void MouseMove(object sender, MouseEventArgs e)
	{
		base.MouseMove(sender, e);
		if (_dragModelVisualizer.IsDragging)
		{
			_dragModelVisualizer.MouseMove(sender, e);
		}
	}

	public override void KeyUp(object sender, IPnInputEventArgs e)
	{
		base.KeyUp(sender, e);
		if (e.Handled || !_dragModelVisualizer.IsDragging)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = e.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			e.Handle();
			double parameter = _dragModelVisualizer.Stop();
			_cmd.CmdMoveBendByDistance.ExecuteWithParameter(parameter);
			Close();
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs2 = e.MouseButtonEventArgs;
		if ((mouseButtonEventArgs2 != null && mouseButtonEventArgs2.ChangedButton == MouseButton.Right) || _shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			e.Handle();
			_dragModelVisualizer.Stop();
			Close();
		}
	}

	public bool StartSubMenu(object sender, ITriangleEventArgs e)
	{
		return MouseSelectTriangleValidObject(sender, e) == true;
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		KeyUp(sender, e.Args);
		if (!e.Args.Handled)
		{
			HideBillboards();
		}
		if (MouseSelectTriangleValidObject(sender, e) == false)
		{
			Close();
		}
	}

	public bool? MouseSelectTriangleValidObject(object sender, ITriangleEventArgs e)
	{
		if (e.Args.Handled)
		{
			return null;
		}
		if (e.Model == null)
		{
			return false;
		}
		Model? model = e.Model;
		if (model != null && model.PartRole == PartRole.BendModel)
		{
			IPnInputEventArgs args = e.Args;
			if (args != null && args.MouseButtonEventArgs?.ChangedButton == MouseButton.Right)
			{
				Triangle? tri = e.Tri;
				if (tri == null || tri.Face?.FaceGroup?.IsBendingZone != true)
				{
					ShowBillboards(e.HitPoint);
					e.Args.Handle();
					return true;
				}
			}
		}
		return false;
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (_dragModelVisualizer.IsDragging)
		{
			_dragModelVisualizer.ColorModelParts(paintTool);
		}
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active)
		{
			_billboardsInitialized = false;
		}
	}

	public void ShowBillboards(Vector3d position)
	{
		if (!_billboardsInitialized)
		{
			_billboardsInitialized = true;
			CreateBillboards();
		}
		foreach (IBillboard billboard in _billboards)
		{
			billboard.Center = position;
			billboard.IsVisible = true;
			ScreenD3D.UpdateBillboardPosition(billboard, render: false);
		}
		ScreenD3D.Render(skipQueuedFrames: false);
	}

	public void HideBillboards()
	{
		foreach (IBillboard billboard in _billboards)
		{
			billboard.IsVisible = false;
		}
		ScreenD3D.Render(skipQueuedFrames: false);
	}

	private void CreateBillboards()
	{
		_billboards.Clear();
		IButtonBillboard buttonBillboard = CreateBillboard(new Vector2d(-100.0, 0.0), "&#xe01a;", _glyphStyle2);
		buttonBillboard.OnClick += MoveLeft;
		_billboards.Add(buttonBillboard);
		IButtonBillboard buttonBillboard2 = CreateBillboard(new Vector2d(100.0, 0.0), "&#xe018;", _glyphStyle2);
		buttonBillboard2.OnClick += MoveRight;
		_billboards.Add(buttonBillboard2);
		IButtonBillboard buttonBillboard3 = CreateBillboard(Vector2d.Zero, "&#xe141;", _glyphStyle1);
		buttonBillboard3.OnClick += StartDragging;
		_billboards.Add(buttonBillboard3);
		foreach (IBillboard billboard in _billboards)
		{
			ScreenD3D.AddBillboard(billboard, render: false);
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
				_cmd.CmdMoveBendLeft.Execute(null);
			}
			else
			{
				_cmd.CmdMoveBendRight.Execute(null);
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
				_cmd.CmdMoveBendRight.Execute(null);
			}
			else
			{
				_cmd.CmdMoveBendLeft.Execute(null);
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
				_dragModelVisualizer.Start();
				RaiseRequestRepaint();
			}
		}
	}

	private bool IsCamaraInFront()
	{
		Matrix transformation = ScreenD3D.Renderer.Root.Transform ?? Matrix.Identity;
		ScreenD3D.CreateRay(ref transformation, out var _, out var eyeDir, out var eyeUp, out var _);
		return (eyeDir.Y > 0.0) ^ (eyeUp.Z < 0.0);
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
