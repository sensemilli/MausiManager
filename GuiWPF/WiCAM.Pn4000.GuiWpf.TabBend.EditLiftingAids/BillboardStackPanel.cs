using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;

public class BillboardStackPanel
{
	private readonly ScreenD3D11 _screen;

	private List<IBillboard> _contents;

	private Vector3d _anchorPointWorld;

	public double Spacing { get; set; } = 5.0;

	public double Width { get; set; } = 200.0;

	public BillboardStackPanel(ScreenD3D11 screen)
	{
		_screen = screen;
	}

	public void SetBillboards(IEnumerable<IBillboard> billboards, Model parent)
	{
		RemoveBillboards();
		_contents = billboards.ToList();
		Vector3d center = parent.WorldMatrix.Inverted.Transform(_anchorPointWorld);
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		foreach (IBillboard content in _contents)
		{
			if (num > 0.0 && num + content.Extends.X > Width)
			{
				num = 0.0;
				num2 += num3 + Spacing;
				num3 = 0.0;
			}
			content.Offset = new Vector2d(num, num2);
			num += content.Extends.X + Spacing;
			num3 = Math.Max(num3, content.Extends.Y);
		}
		foreach (IBillboard content2 in _contents)
		{
			content2.Center = center;
			_screen.AddBillboard(content2, parent, render: false);
		}
	}

	public void RemoveBillboards()
	{
		foreach (IBillboard item in _contents ?? new List<IBillboard>())
		{
			_screen.RemoveBillboard(item, render: false);
		}
	}

	public void SetAnchor(Vector3d anchorPointWorld)
	{
		_anchorPointWorld = anchorPointWorld;
	}
}
