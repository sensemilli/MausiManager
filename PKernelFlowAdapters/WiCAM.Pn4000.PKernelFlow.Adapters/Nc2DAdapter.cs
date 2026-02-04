using WiCAM.Pn4000.PKernelFlow.Adapters.Type;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.PKernelFlow.Adapters;

public static class Nc2DAdapter
{
	public static void InitQuelle()
	{
		int iret = 0;
		P2H.p2h059_(ref iret);
	}

	public static int LeseQuelle(int iend)
	{
		int iret = 0;
		P2H.p2h052_(ref iend, ref iret);
		return iend;
	}

	public static void SchreibeQuelle(int iend)
	{
		int iret = 0;
		P2H.p2h110_(ref iend, ref iret);
	}

	public static void LeseQuelleHeader()
	{
		int iret = 0;
		P2H.p2h053_(ref iret);
	}

	public static void SetzePlatineParameter(int inum, int iqnum)
	{
		int iret = 0;
		WIP.wip100_(ref inum, ref iqnum, ref iret);
	}

	public static void InitWerkzeuge(int ibas, int bnr)
	{
		int luwrit = 0;
		int luread = 0;
		PKO.pko007_(ref ibas, ref bnr, ref luwrit, ref luread);
	}

	public static int WerkzeugParameter(float xd, float yd, float dd, float wd, int ir, int it, int inum)
	{
		int iret = 0;
		P2H.p2h118_(ref xd, ref yd, ref dd, ref wd, ref ir, ref it, ref inum, ref iret);
		return inum;
	}

	public static int GetWerkzeugNummer(string name)
	{
		int iret = 0;
		int its = 0;
		P2H.p2h124_(MarshalString.CreateFString(name, 160), ref its, ref iret, 160);
		return its;
	}

	public static int GetWerkzeugNummerAlias(string name)
	{
		int iret = 0;
		int its = 0;
		int ist = 0;
		float wd = 0f;
		int ir = 0;
		P2H.p2h299_(MarshalString.CreateFString(name, 160), ref ist, ref ir, ref wd, ref its, ref iret, 160);
		return its;
	}

	public static int GetAnzahlAktiverWerkzeuge()
	{
		int iret = 0;
		int inum = 0;
		WIP.wip101_(ref inum, ref iret);
		return inum;
	}

	public static void SetNcLaserKonturArt(int iqend, int ikont)
	{
		int iret = 0;
		P2H.p2h087_(ref iqend, ref ikont, ref iret);
	}

	public static void SetNcLaserArt(int iqend, int iart)
	{
		int iret = 0;
		P2H.p2h394_(ref iqend, ref iart, ref iret);
	}

	public static int AddNcLaserZuenden(int iqend, float xa, float ya, float dd, int iwnr, int typ)
	{
		int iret = 0;
		int num = 1;
		int num2 = 1;
		int num3 = 1;
		switch (typ)
		{
		case 1:
			num = 1;
			num2 = 1;
			num3 = 1;
			break;
		case 2:
			num = 1;
			num2 = 8;
			num3 = 20;
			break;
		default:
			num = 1;
			num2 = 1;
			num3 = 1;
			break;
		}
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h091_(ref iqend, ref xa, ref ya, ref dd, ref iret);
		P2H.p2h394_(ref iqend, ref num, ref iret);
		P2H.p2h392_(ref iqend, ref num2, ref iret);
		P2H.p2h087_(ref iqend, ref num3, ref iret);
		P2H.p2h056_(ref iqend, ref iwnr, ref iret);
		return iqend;
	}

	public static int AddNcLaserLinie(int iqend, int iwnr, float xa, float ya, float xe, float ye, int irl)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h088_(ref iqend, ref xa, ref ya, ref xe, ref ye, ref irl, ref iret);
		P2H.p2h056_(ref iqend, ref iwnr, ref iret);
		return iqend;
	}

	public static int AddNcLaserKreis(int iqend, int iwnr, float xm, float ym, float dd, float wa, float we, int isr, int irl)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h090_(ref iqend, ref xm, ref ym, ref dd, ref wa, ref we, ref isr, ref irl, ref iret);
		P2H.p2h056_(ref iqend, ref iwnr, ref iret);
		return iqend;
	}

	public static int AddNcHubNorm(int iqend, float xp, float yp, float ang, int its)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		PAN.pan017_(ref iqend, ref xp, ref yp, ref ang, ref its, ref iret);
		return iqend;
	}

	public static int AddNcHubSonder(int iqend, float xp, float yp, float ang, int its)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h082_(ref iqend, ref xp, ref yp, ref ang, ref its, ref iret);
		return iqend;
	}

	public static int AddNcHubRund(int iqend, float xp, float yp, float dd)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h078_(ref iqend, ref xp, ref yp, ref dd, ref iret);
		return iqend;
	}

	public static int AddNcHubLangloch(int iqend, float xp, float yp, float ang, float breite, float hoehe, float durch)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h080_(ref iqend, ref xp, ref yp, ref ang, ref breite, ref hoehe, ref durch, ref iret);
		return iqend;
	}

	public static int AddNcNibbelLinie(int iqend, int iwnr, float xa, float ya, float xe, float ye, int irl)
	{
		int iret = 0;
		irl = ((irl == 0) ? (-1) : irl);
		int num = CONWRK.get_icwtyp(iwnr);
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h084_(ref iqend, ref xa, ref ya, ref xe, ref ye, ref irl, ref iret);
		if (num < 5)
		{
			P2H.p2h056_(ref iqend, ref iwnr, ref iret);
		}
		else
		{
			P2H.p2h068_(ref iqend, ref iwnr, ref iret);
		}
		return iqend;
	}

	public static int AddNcNibbelBogen(int iqend, int iwnr, float xm, float ym, float dd, float wa, float we, int isr, int irl)
	{
		int iret = 0;
		irl = ((irl == 0) ? (-1) : irl);
		int num = CONWRK.get_icwtyp(iwnr);
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h086_(ref iqend, ref xm, ref ym, ref dd, ref wa, ref we, ref isr, ref irl, ref iret);
		if (num < 5)
		{
			P2H.p2h056_(ref iqend, ref iwnr, ref iret);
		}
		else
		{
			P2H.p2h068_(ref iqend, ref iwnr, ref iret);
		}
		return iqend;
	}

	public static int AddNcEntso1(int iqend)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h062_(ref iqend, ref iret);
		return iqend;
	}

	public static int AddNcEntso2(int iqend)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h064_(ref iqend, ref iret);
		return iqend;
	}

	public static int AddNcEntsoStop(int iqend)
	{
		int iret = 0;
		P2H.p2h054_(ref iqend, ref iret);
		P2H.p2h060_(ref iqend, ref iret);
		return iqend;
	}

	public static int AddNcEntsoLift(int iqnum, int iqend, float xp, float yp)
	{
		int iret = 0;
		int imode = 1;
		int iqlift = 0;
		PEL.pel108_(ref imode, ref xp, ref yp, ref iqnum, ref iqlift, ref iqend, ref iret);
		return iqend;
	}

	public static int GetMaschNummer(string mName)
	{
		int iret = 0;
		int imasch = 0;
		PBE.pbe078_(MarshalString.CreateFString(mName, 160), ref imasch, ref iret, 160);
		return imasch;
	}

	public static string GetMaschName(int iMasch)
	{
		byte[] array = MarshalString.CreateFString(160);
		int iret = 0;
		PBE.pbe062_(ref iMasch, array, ref iret, 160);
		return MarshalString.FString2String(array);
	}

	public static int MatIdtoNum(string matName)
	{
		int iret = 0;
		int imat = 0;
		PBE.pbe077_(MarshalString.CreateFString(matName, 160), ref imat, ref iret, 160);
		return imat;
	}
}
