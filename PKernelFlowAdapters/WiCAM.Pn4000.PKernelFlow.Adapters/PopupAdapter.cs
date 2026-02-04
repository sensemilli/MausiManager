using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class PopupAdapter
{
	private static char[] trimmarray = new char[1] { ' ' };

	public static PopupLine GetPopupLine(int f)
	{
		PopupLine popupLine = new PopupLine();
		popupLine.idx = f;
		popupLine.typ = WINDOW.get_ipotyp(f);
		popupLine.sel = WINDOW.get_iposel(f);
		popupLine.pyn = WINDOW.get_ipopyn(f);
		popupLine.id1 = WINDOW.get_ipopid(f);
		popupLine.id2 = WINDOW.get_ipopi2(f);
		popupLine.id3 = WINDOW.get_ipopi3(f);
		popupLine.id4 = WINDOW.get_ipopi4(f);
		popupLine.real1 = WINDOW.get_rpopz(f);
		popupLine.real2 = WINDOW.get_rpopz2(f);
		popupLine.real3 = WINDOW.get_rpopz3(f);
		popupLine.real4 = WINDOW.get_rpopz4(f);
		popupLine.inte1 = WINDOW.get_ipopz(f);
		popupLine.inte2 = WINDOW.get_ipopz2(f);
		popupLine.inte3 = WINDOW.get_ipopz3(f);
		popupLine.inte4 = WINDOW.get_ipopz4(f);
		popupLine.box1_x = WINDOW.get_ipobx1(f);
		popupLine.box1_y = WINDOW.get_ipoby1(f);
		popupLine.box1_w = WINDOW.get_ipobb1(f);
		popupLine.box1_h = WINDOW.get_ipobh1(f);
		popupLine.box2_x = WINDOW.get_ipobx2(f);
		popupLine.box2_y = WINDOW.get_ipoby2(f);
		popupLine.box2_w = WINDOW.get_ipobb2(f);
		popupLine.box2_h = WINDOW.get_ipobh2(f);
		popupLine.text = Marshal.PtrToStringAnsi(WINDOW.getcharaddr_popbem(f * 500), 500).TrimEnd(PopupAdapter.trimmarray);
		popupLine.ntext = Marshal.PtrToStringAnsi(WINDOW.getcharaddr_popvar(f * 80), 80).TrimEnd(PopupAdapter.trimmarray);
		if (popupLine.id3 > 20000 && popupLine.id3 <= 30000)
		{
			popupLine.htext = GeneralSystemComponentsAdapter.GetTextById(popupLine.id3);
		}
		else
		{
			popupLine.htext = Marshal.PtrToStringAnsi(WINDOW.getcharaddr_pophlp(f * 200), 200).TrimEnd();
		}
		popupLine.ltext = Marshal.PtrToStringAnsi(WINDOW.getcharaddr_poplog(f * 80), 80).TrimEnd();
		popupLine.real1 = Math.Round(popupLine.real1, 4);
		popupLine.real2 = Math.Round(popupLine.real2, 4);
		popupLine.real3 = Math.Round(popupLine.real3, 4);
		popupLine.real4 = Math.Round(popupLine.real4, 4);
		popupLine.is_change = false;
		popupLine.is_disable = false;
		popupLine.is_presentation_mode = false;
		popupLine.multitext = new List<string>();
		return popupLine;
	}

	public static void Popup_Line_IPOSEL_set(int idx, int value)
	{
		if (value == 2)
		{
			value = -1;
		}
		WINDOW.set_iposel(idx, value);
	}

	public static void Popup_Line_RPOPZ_set(int idx, double value)
	{
		WINDOW.set_rpopz(idx, (float)value);
	}

	public static void Popup_Line_IPOPZ_set(int idx, int value)
	{
		WINDOW.set_ipopz(idx, value);
	}

	public static void Popup_Line_POPVAR_set(int idx, string value)
	{
		MarshalString.ToIntPtrAtFortranStyle(value, WINDOW.getcharaddr_popvar(idx * 80), 80);
	}

	public static void Popup_Line_IPOPYN_set(int idx, int value)
	{
		WINDOW.set_ipopyn(idx, value);
	}

	public static void Popup_Line_IPOPID_set(int idx, int value)
	{
		WINDOW.set_ipopid(idx, value);
	}

	public static void Popup_Line_POPBEM_set(int idx, string value)
	{
		MarshalString.ToIntPtrAtFortranStyle(value, WINDOW.getcharaddr_popbem(idx * 500), 500);
	}
}
