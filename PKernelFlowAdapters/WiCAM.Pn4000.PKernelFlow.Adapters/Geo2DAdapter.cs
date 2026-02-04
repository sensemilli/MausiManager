using WiCAM.Pn4000.BendModel.Geometry2D.Data;
using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class Geo2DAdapter
{
	public static float DrawScale => GSLCOM.gslcom_get_drasca();

	public static int CommonGslGeoCount
	{
		get
		{
			return GSLGEO.gslgeo_get_icgend();
		}
		set
		{
			GSLGEO.gslgeo_set_icgend(value);
		}
	}
	/// <summary>
	/// WICHTIG
	/// </summary>
	public static void LoadDefaultCad2DAndRedraw()
	{
		int luwrit = 0;
		int luread = 0;
		WIP.wip030_(ref luwrit, ref luread);
	}

	public static void AddLine(int color, float x1, float y1, float x2, float y2)
	{
		int iret = 0;
		P2H.p2h152_(ref color, ref x1, ref y1, ref x2, ref y2, ref iret);
	}

	public static void AddArc(int color, float xm, float ym, float d, float wa, float we, int ori)
	{
		int iret = 0;
		P2H.p2h154_(ref color, ref xm, ref ym, ref d, ref wa, ref we, ref ori, ref iret);
	}

	public static void AddText(float x, float y, float ang, int color, float rtho, string text)
	{
		int iret = 0;
		PMA.pma120_(ref x, ref y, ref ang, ref color, ref rtho, MarshalString.CreateFString(text.Replace(',', '.'), 160), ref iret, 160);
	}

	public static int GetCount()
	{
		int ianz = 0;
		WIP.wip013_(ref ianz);
		return ianz;
	}

	public static void ShowTextOn()
	{
		if (GSLCOM.gslcom_get_igtakt() == 0)
		{
			int luwrit = 0;
			int luread = 0;
			PMA.pma063_(ref luwrit, ref luread);
		}
	}

	public static void CommonGslGeoRedraw()
	{
		int iret = 0;
		P2H.p2h296_(ref iret);
		P2H.p2h298_(ref iret);
	}

	public static void DrawMarkElement(int idx)
	{
		int iret = 0;
		int iele = idx + 1;
		int icol = 2;
		P2H.p2h220_(ref iele, ref icol, ref iret);
	}

	public static void DrawMarkPoint(float x, float y)
	{
		int iret = 0;
		P2H.p2h282_(ref x, ref y, ref iret);
	}

	public static void CommonGslGeoRedrawMaxZoom()
	{
		int iret = 0;
		P2H.p2h296_(ref iret);
		P2H.p2h300_(ref iret);
	}

	public static void SetElement(int idx, int type, float r1, float r2, float r3, float r4)
	{
		GSLGEO.set_icgele(idx - 1, type);
		GSLGEO.set_rcgww1(idx - 1, r1);
		GSLGEO.set_rcgww2(idx - 1, r2);
		GSLGEO.set_rcgww3(idx - 1, r3);
		GSLGEO.set_rcgww4(idx - 1, r4);
	}

	public static void SetElement(int idx, Pn2DEntity entity)
	{
		GSLGEO.set_icgele(idx, entity.type);
		GSLGEO.set_icgcol(idx, entity.color);
		GSLGEO.set_icgrsi(idx, entity.arc_direction);
		GSLGEO.set_rcgww1(idx, entity.r1);
		GSLGEO.set_rcgww2(idx, entity.r2);
		GSLGEO.set_rcgww3(idx, entity.r3);
		GSLGEO.set_rcgww4(idx, entity.r4);
		GSLGEO.set_rcgww5(idx, entity.r5);
	}

	public static Pn2DEntity GetElement(int idx)
	{
		Pn2DEntity pn2DEntity = new Pn2DEntity();
		int iret = 0;
		P2H.p2h202_(ref idx, ref pn2DEntity.type, ref pn2DEntity.arc_direction, ref pn2DEntity.r1, ref pn2DEntity.r2, ref pn2DEntity.r3, ref pn2DEntity.r4, ref pn2DEntity.r5, ref iret);
		if (iret != 0)
		{
			return null;
		}
		P2H.p2h200_(ref idx, ref pn2DEntity.color, ref iret);
		if (iret != 0)
		{
			return null;
		}
		return pn2DEntity;
	}

	public static void ResetDynamicMemoryCounter()
	{
		int luwrit = 0;
		int luread = 0;
		PST.pst005_(ref luwrit, ref luread);
	}

	public static void InitGlsgeo()
	{
		int iret = 0;
		P2H.p2h146_(ref iret);
	}

	public static void InitTexte()
	{
		int iok = 0;
		int luwrit = 0;
		int luread = 0;
		WIP.wip019_(ref iok, ref luwrit, ref luread);
	}

	public static void InitCadTXT()
	{
		int iret = 0;
		WIP.wip021_(ref iret);
	}

	public static void InitCadgeoHeader(string name)
	{
		int iret = 0;
		int ianf = 0;
		P2R.p2r030_(ref ianf, ref iret);
	}

	public static void LoadCadgeoToCommon()
	{
		int iret = 0;
		P2H.p2h270_(ref iret);
	}

	public static void LoadCadTextToCommon()
	{
		int luwrit = 0;
		int luread = 0;
		PMA.pma010_(ref luwrit, ref luread);
	}
}
