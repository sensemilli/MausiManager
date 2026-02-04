using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class EditFingersBillboards : IEditFingersBillboards
{
	private const string GlyphMoveR = "&#xe035;";

	private const string GlyphMoveX = "&#xe141;";

	private const string GlyphSnap = "&#xe132;";

	private const string GlyphSnapLeft = "&#xe130;";

	private const string GlyphSnapRight = "&#xe130;";

	private readonly IScreen3DMain _screen3D;

	private readonly IRadGlyphConverter _glyphConverter;

	private readonly ICollection<IBillboard> _billboards = new List<IBillboard>();

	private readonly ICollection<IBillboard> _billboardsForOnePieceOrSection = new List<IBillboard>();

	private readonly ICollection<IBillboard> _billboardsForSection = new List<IBillboard>();

	private IBillboard _billboardAddAdapterTop;

	private IBillboard _billboardAddAdapterBottom;

	private readonly BackgroundStyle _background;

	private readonly BackgroundStyle _backgroundHover;

	private readonly BackgroundStyle _backgroundMouseDown;

	private readonly GlyphStyle _glyphStyle1;

	private readonly GlyphStyle _glyphStyle2;

	private readonly GlyphStyle _glyphStyleRot;

	private GlyphStyle _glyphStyleSnapToggle;

	private bool _billboardsInitialized;

	private bool _isActive;

	private bool _showingBillboard;

	private Vector3d _billboardCenter;

	private Canvas ScreenOverlay => _screen3D.Screen3D.Overlay;

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	private IEnumerable<IBillboard> AllBillboards => from x in _billboards.Concat(_billboardsForSection).Concat(_billboardsForOnePieceOrSection).Concat(new _003C_003Ez__ReadOnlyArray<IBillboard>(new IBillboard[2] { _billboardAddAdapterTop, _billboardAddAdapterBottom }))
		where x != null
		select x;

	public event Action? RequestRepaint;

	public event Action<IPnInputEventArgs> CmdMoveR;

	public event Action<IPnInputEventArgs> CmdMoveXZ;

	public event Action<IPnInputEventArgs> CmdSnapUp;

	public event Action<IPnInputEventArgs> CmdSnapLeft;

	public event Action<IPnInputEventArgs> CmdSnapRight;

	public EditFingersBillboards(IScreen3DMain screen3D, IStyleProvider styles, IRadGlyphConverter glyphConverter)
	{
		_screen3D = screen3D;
		_glyphConverter = glyphConverter;
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
		glyphStyle = _glyphStyle2;
		glyphStyle.Rotation = Math.PI;
		_glyphStyleRot = glyphStyle;
		glyphStyle = _glyphStyle2;
		glyphStyle.Color = new Color(0f, 1f, 0f, 1f);
		_glyphStyleSnapToggle = glyphStyle;
	}

	public void SetActive(bool active)
	{
		_isActive = active;
		if (active)
		{
			_billboardsInitialized = false;
		}
		RefreshBillboards();
	}

	public void ShowBillboards()
	{
		_showingBillboard = true;
		RefreshBillboards();
	}

	public void HideBillboards()
	{
		_showingBillboard = false;
		RefreshBillboards();
	}

	private void RefreshBillboards()
	{
		if (_isActive && _showingBillboard)
		{
			CreateBillboardsIfNeeded();
			foreach (IBillboard allBillboard in AllBillboards)
			{
				allBillboard.IsVisible = true;
			}
			this.RequestRepaint?.Invoke();
			return;
		}
		foreach (IBillboard allBillboard2 in AllBillboards)
		{
			allBillboard2.IsVisible = false;
		}
		if (_isActive)
		{
			this.RequestRepaint?.Invoke();
		}
	}

	public void SetBillboardLocation(Vector3d position, PartRole finger)
	{
		_billboardCenter = position;
		if (!_isActive || !_showingBillboard)
		{
			return;
		}
		CreateBillboardsIfNeeded();
		foreach (IBillboard billboard in _billboards)
		{
			billboard.Center = _billboardCenter;
			ScreenD3D.UpdateBillboardPosition(billboard, render: false);
		}
	}

	private void CreateBillboardsIfNeeded()
	{
		if (_billboardsInitialized)
		{
			return;
		}
		_billboardsInitialized = true;
		foreach (IBillboard billboard in _billboards)
		{
			ScreenD3D.RemoveBillboard(billboard, render: false);
		}
		_billboards.Clear();
		_billboardsForSection.Clear();
		_billboardsForOnePieceOrSection.Clear();
		IButtonBillboard buttonBillboard = CreateBillboard(new Vector2d(-30.0, 30.0), "&#xe141;", _glyphStyle2);
		buttonBillboard.OnClick += MoveXZ;
		_billboards.Add(buttonBillboard);
		IButtonBillboard buttonBillboard2 = CreateBillboard(new Vector2d(-60.0, 90.0), "&#xe130;", _glyphStyleRot);
		buttonBillboard2.OnClick += SnapLeft;
		_billboards.Add(buttonBillboard2);
		IButtonBillboard buttonBillboard3 = CreateBillboard(new Vector2d(60.0, 90.0), "&#xe130;", _glyphStyle2);
		buttonBillboard3.OnClick += SnapRight;
		_billboards.Add(buttonBillboard3);
		IButtonBillboard buttonBillboard4 = CreateBillboard(new Vector2d(30.0, 30.0), "&#xe035;", _glyphStyle2);
		buttonBillboard4.OnClick += MoveR;
		_billboards.Add(buttonBillboard4);
		IButtonBillboard buttonBillboard5 = CreateBillboard(new Vector2d(0.0, 90.0), "&#xe132;", _glyphStyleRot);
		buttonBillboard5.OnClick += SnapUp;
		_billboards.Add(buttonBillboard5);
		foreach (IBillboard allBillboard in AllBillboards)
		{
			allBillboard.Center = _billboardCenter;
			ScreenD3D.AddBillboard(allBillboard, render: false);
		}
	}

	private void MoveR(IPnInputEventArgs args, IBillboard _)
	{
		this.CmdMoveR?.Invoke(args);
	}

	private void MoveXZ(IPnInputEventArgs args, IBillboard _)
	{
		this.CmdMoveXZ?.Invoke(args);
	}

	private void SnapLeft(IPnInputEventArgs args, IBillboard _)
	{
		this.CmdSnapLeft?.Invoke(args);
	}

	private void SnapRight(IPnInputEventArgs args, IBillboard _)
	{
		this.CmdSnapRight?.Invoke(args);
	}

	private void SnapUp(IPnInputEventArgs args, IBillboard _)
	{
		this.CmdSnapUp?.Invoke(args);
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
