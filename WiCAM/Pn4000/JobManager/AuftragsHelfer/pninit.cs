using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WiCAM.Pn4000.JobManager;

public static class pninit
{
	public static int Sprache = 1;

	public static string programmtitel = "PNxPertConfig Mundal-2024";

	public static string ueberPNxPert = "Dieses Programm ist Freeware. Es darf kopiert und kostenfrei weitergegeben werden.\nKeine Garantie. Dieses Programm ist ein privates Projekt.\n\nAlle Rechte vorbehalten!\n\n\nThis program is freeware. It may be copied and passed to others without charge.\nNo guarantee. This program is a private project.\n\nAll rights reserved!";

	public static string PNDrive;

	public static string PNHomeDrive;

	public static string PNHomePath;

	public static string ARDrive;

	public static string TextEditor = "Notepad";

	public static string configFile = "pnxpert.cfg";

	public static string PNLanguageDir = "01";

	public static string[,] arrMaschinenListe = new string[500, 5];

	public static string[,] arrLizenzInfo = new string[100, 4];

	public static string serverInfo = null;

	public static SortedList<string, string> popupTexte = new SortedList<string, string>();

	public static SortedList<string, string> popupTexteCustom = new SortedList<string, string>();

	public static string[,] arrPopUpListe = new string[1200, 4];

	public static bool popupSortAufsteigend = true;

	public static int popupSortLastColumn = -1;

	public static bool maschineSortAufsteigend = true;

	public static int maschineSortLastColumn = -1;

	public static int intAnzahlAktiveMaschinen = 0;

	public static bool bool_Maschinen_Filter_Aktiv = false;

	public static bool boolSaveDirSelected = false;

	public static string[,] arrFileSelectionAuto = new string[20000, 2];

	public static string[,] arrFileSelectionManu = new string[1000, 2];

	public static string strLastPickDirectory = PNDrive + "\\u\\pn";

	public static string strTargetDirOld = "";

	public static string strTargetDirNew = "";

	public static int intAnzahlFilesAuto = 0;

	public static int intAnzahlFilesManu = 0;

	public static string strSortListTitel = "";

	public static string strSortListMachNummer = "";

	public static string strSortlistMachName = "";

	public static string[,] arrmtsort = new string[20, 2];

	public static bool multitool_vorhanden = false;

	public static int idubletten = 0;

	public static string DialogSortWinkelDaten;

	public static string DialogSortWinkelValue;

	public static bool DialogReturnOK;

	public static bool DialogWinkelSortieren = false;

	public static bool checkForPTT = false;

	public static bool pnlinkKopieren = false;

	public static string[,] maskenTextTyp = new string[8, 2];

	public static string meKurzText;

	public static string meLangText;

	public static string meTextTyp;

	public static int meIndex;

	public static bool boolPopupFilterActive = false;

	public static string strPopupFilterText = "";

	public static string liftTyp = "0";

	public static bool liftReserveplatzVorhanden = false;

	public static bool liftReservePlatzAusgeben = false;

	public static bool liftReservePlatzNumAusgeben = false;

	public static string strMoveLiftTitel;

	public static void get_LizenzInfo()
	{
		int num = 0;
		string text = PNDrive + "\\u\\pn\\config\\licens\\option";
		if (!File.Exists(text))
		{
			return;
		}
		StreamReader streamReader = null;
		try
		{
			streamReader = new StreamReader(text, Encoding.GetEncoding("windows-1252"));
		}
		catch (Exception ex)
		{
			MessageBox.Show("Fehler beim Öffnen der Datei '" + text + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string text6 = null;
		while ((text2 = streamReader.ReadLine()) != null)
		{
			text2 = text2.Trim();
			if (text2 == "")
			{
				break;
			}
			string[] array = text2.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			text3 = array[0];
			text4 = array[1];
			text6 = array[3];
			text6 = ((!(text6 == "99999999")) ? (text6.Substring(6, 2) + "." + text6.Substring(4, 2) + "." + text6.Substring(0, 4)) : Sprachen.arrSprache[25, Sprache]);
			text5 = text3 switch
			{
				"1000" => Sprachen.arrSprache[40, Sprache], 
				"3000" => Sprachen.arrSprache[41, Sprache], 
				"4000" => Sprachen.arrSprache[42, Sprache], 
				"4001" => Sprachen.arrSprache[43, Sprache], 
				"4002" => Sprachen.arrSprache[44, Sprache], 
				"4003" => Sprachen.arrSprache[45, Sprache], 
				"4004" => Sprachen.arrSprache[46, Sprache], 
				"4005" => Sprachen.arrSprache[47, Sprache], 
				"4006" => Sprachen.arrSprache[48, Sprache], 
				"4007" => Sprachen.arrSprache[49, Sprache], 
				"4101" => Sprachen.arrSprache[50, Sprache], 
				"4102" => Sprachen.arrSprache[51, Sprache], 
				"4103" => Sprachen.arrSprache[52, Sprache], 
				"4104" => Sprachen.arrSprache[53, Sprache], 
				"4105" => Sprachen.arrSprache[54, Sprache], 
				"4106" => Sprachen.arrSprache[55, Sprache], 
				"4107" => Sprachen.arrSprache[56, Sprache], 
				"4108" => Sprachen.arrSprache[57, Sprache], 
				"4109" => Sprachen.arrSprache[58, Sprache], 
				"4110" => Sprachen.arrSprache[59, Sprache], 
				"4111" => Sprachen.arrSprache[60, Sprache], 
				"4112" => Sprachen.arrSprache[61, Sprache], 
				"4113" => Sprachen.arrSprache[62, Sprache], 
				"4114" => Sprachen.arrSprache[63, Sprache], 
				"4115" => Sprachen.arrSprache[64, Sprache], 
				"4116" => Sprachen.arrSprache[65, Sprache], 
				"4117" => Sprachen.arrSprache[66, Sprache], 
				"4118" => Sprachen.arrSprache[67, Sprache], 
				"4119" => Sprachen.arrSprache[68, Sprache], 
				"4120" => Sprachen.arrSprache[69, Sprache], 
				"4121" => Sprachen.arrSprache[70, Sprache], 
				"4122" => Sprachen.arrSprache[71, Sprache], 
				"4123" => Sprachen.arrSprache[72, Sprache], 
				"4124" => Sprachen.arrSprache[73, Sprache], 
				"4125" => Sprachen.arrSprache[74, Sprache], 
				"4126" => Sprachen.arrSprache[75, Sprache], 
				"4127" => Sprachen.arrSprache[76, Sprache], 
				"4128" => Sprachen.arrSprache[77, Sprache], 
				"4129" => Sprachen.arrSprache[78, Sprache], 
				"4130" => Sprachen.arrSprache[79, Sprache], 
				"4131" => Sprachen.arrSprache[80, Sprache], 
				"4132" => Sprachen.arrSprache[81, Sprache], 
				"4133" => Sprachen.arrSprache[82, Sprache], 
				"4134" => Sprachen.arrSprache[296, Sprache], 
				"4135" => Sprachen.arrSprache[297, Sprache], 
				"4201" => Sprachen.arrSprache[83, Sprache], 
				"4202" => Sprachen.arrSprache[84, Sprache], 
				"4203" => Sprachen.arrSprache[85, Sprache], 
				"4204" => Sprachen.arrSprache[86, Sprache], 
				"4205" => Sprachen.arrSprache[87, Sprache], 
				"4206" => Sprachen.arrSprache[88, Sprache], 
				"4207" => Sprachen.arrSprache[89, Sprache], 
				"4208" => Sprachen.arrSprache[90, Sprache], 
				"4209" => Sprachen.arrSprache[91, Sprache], 
				"4210" => Sprachen.arrSprache[92, Sprache], 
				"4211" => Sprachen.arrSprache[93, Sprache], 
				"4212" => Sprachen.arrSprache[94, Sprache], 
				"4213" => Sprachen.arrSprache[95, Sprache], 
				"4214" => Sprachen.arrSprache[96, Sprache], 
				"4215" => Sprachen.arrSprache[97, Sprache], 
				"4216" => Sprachen.arrSprache[204, Sprache], 
				"4217" => Sprachen.arrSprache[200, Sprache], 
				"4218" => Sprachen.arrSprache[203, Sprache], 
				"4301" => Sprachen.arrSprache[98, Sprache], 
				"4302" => Sprachen.arrSprache[99, Sprache], 
				"4303" => Sprachen.arrSprache[100, Sprache], 
				"4304" => Sprachen.arrSprache[101, Sprache], 
				"4401" => Sprachen.arrSprache[205, Sprache], 
				"4402" => Sprachen.arrSprache[206, Sprache], 
				"4403" => Sprachen.arrSprache[207, Sprache], 
				"5001" => Sprachen.arrSprache[103, Sprache], 
				"5002" => Sprachen.arrSprache[104, Sprache], 
				_ => Sprachen.arrSprache[105, Sprache], 
			};
			arrLizenzInfo[num, 0] = text3;
			arrLizenzInfo[num, 1] = text4;
			arrLizenzInfo[num, 2] = text5;
			arrLizenzInfo[num, 3] = text6;
			num++;
		}
		streamReader.Close();
		text = PNDrive + "\\u\\pn\\config\\licens\\server";
		streamReader = null;
		try
		{
			streamReader = new StreamReader(text, Encoding.GetEncoding("windows-1252"));
		}
		catch (Exception ex)
		{
			MessageBox.Show("Fehler beim Öffnen der Datei '" + text + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		text2 = null;
		while ((text2 = streamReader.ReadLine()) != null)
		{
			text2 = text2.Trim();
			if (text2 == "")
			{
				break;
			}
			serverInfo = serverInfo + text2 + "\n";
		}
		streamReader.Close();
		streamReader = null;
	}

	public static string get_machineNumber_from_quelle()
	{
		string text = "0";
		string text2 = "";
		string text3 = null;
		text2 = PNHomeDrive + PNHomePath + "\\INITPN";
		if (File.Exists(text2))
		{
			StreamReader streamReader = null;
			try
			{
				streamReader = new StreamReader(text2, Encoding.GetEncoding("windows-1252"));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler beim Öffnen der Datei '" + text2 + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			text3 = streamReader.ReadLine();
			streamReader.Close();
			text3 = get_string_by_seperator(text3, ' ', 1);
			if (text3 == "0")
			{
				text2 = PNHomeDrive + PNHomePath + "\\QUELLE";
			}
			else
			{
				text3 = "000" + text3;
				text3 = text3.Substring(text3.Length - 3, 3);
				text2 = PNHomeDrive + PNHomePath + "\\pn.mpl\\QUELLE." + text3;
			}
		}
		else
		{
			text2 = PNHomeDrive + PNHomePath + "\\QUELLE";
		}
		if (File.Exists(text2))
		{
			StreamReader streamReader = null;
			try
			{
				streamReader = new StreamReader(text2, Encoding.GetEncoding("windows-1252"));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler beim Öffnen der Datei '" + text2 + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return "0";
			}
			text3 = null;
			for (int i = 0; i < 82; i++)
			{
				text3 = streamReader.ReadLine();
			}
			streamReader.Close();
			text = get_string_by_seperator(text3, ' ', 1);
			text = get_string_by_seperator(text3, '.', 1);
			text = "0000" + text;
			return text.Substring(text.Length - 4, 4);
		}
		return "0";
	}

	public static void get_environment()
	{
		PNDrive = Environment.GetEnvironmentVariable("PNDRIVE");
		PNHomeDrive = Environment.GetEnvironmentVariable("PNHOMEDRIVE");
		PNHomePath = Environment.GetEnvironmentVariable("PNHOMEPATH");
		ARDrive = Environment.GetEnvironmentVariable("ARDRIVE");
		string directoryName = Path.GetDirectoryName(Application.ExecutablePath);
		if (Directory.Exists(directoryName + "\\Notepad++"))
		{
			TextEditor = directoryName + "\\Notepad++\\Notepad++.exe";
		}
		if (File.Exists(directoryName + "\\Notepad++.exe"))
		{
			TextEditor = directoryName + "\\Notepad++.exe";
		}
		configFile = directoryName + "\\pnxpert.cfg";
		if (!File.Exists(configFile))
		{
			return;
		}
		StreamReader streamReader = null;
		try
		{
			streamReader = new StreamReader(configFile, Encoding.GetEncoding("windows-1252"));
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				text = text.Trim();
				text = text.Trim('"');
				if (text.ToLower().StartsWith("editor"))
				{
					if (text == "")
					{
						break;
					}
					string[] array = text.Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);
					text = array[1];
					text = text.Trim();
					if (File.Exists(text))
					{
						TextEditor = "\"" + text + "\"";
					}
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Fehler beim Öffnen der Datei '" + configFile + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		streamReader.Close();
	}

	public static void get_pn_machines()
	{
		Array.Clear(arrMaschinenListe, 0, arrMaschinenListe.Length);
		try
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(PNDrive + "\\u\\pn\\machine");
			DirectoryInfo[] directories = directoryInfo.GetDirectories("machine_????");
			int num = 0;
			for (int i = 0; i < directories.Count(); i++)
			{
				string fullName = directories[i].FullName;
				if (fullName.Contains("machine_0000"))
				{
					continue;
				}
				string text = "";
				string text2 = "";
				string text3 = "";
				string text4 = "";
				string text5 = fullName + "\\config\\MACHINE__NAME_.TXT";
				if (File.Exists(text5))
				{
					StreamReader streamReader = null;
					try
					{
						streamReader = new StreamReader(text5, Encoding.GetEncoding("windows-1252"));
					}
					catch (Exception ex)
					{
						MessageBox.Show("Fehler beim Öffnen der Datei '" + text5 + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
						break;
					}
					string text6 = null;
					text6 = streamReader.ReadLine();
					if (text6 != null)
					{
						text = text6.Trim();
					}
					text6 = streamReader.ReadLine();
					if (text6 != null)
					{
						text2 = text6.Trim();
					}
					text6 = streamReader.ReadLine();
					if (text6 != null)
					{
						text3 = text6.Trim();
					}
					text6 = streamReader.ReadLine();
					if (text6 != null)
					{
						text4 = text6.Trim();
					}
					streamReader.Close();
				}
				arrMaschinenListe[num, 0] = fullName.Substring(fullName.Length - 4, 4);
				arrMaschinenListe[num, 1] = text;
				arrMaschinenListe[num, 2] = text2;
				arrMaschinenListe[num, 3] = text3;
				arrMaschinenListe[num, 4] = text4;
				num++;
			}
		}
		catch (IOException ex2)
		{
			MessageBox.Show("Fehler beim Ermitteln der Dateien: {0}.", ex2.Message);
		}
	}

	public static void get_pnSprache()
	{
		string text = PNHomeDrive + PNHomePath + "\\MSGTXT";
		if (File.Exists(text))
		{
			StreamReader streamReader = null;
			try
			{
				streamReader = new StreamReader(text, Encoding.GetEncoding("windows-1252"));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler beim Öffnen der Datei '" + text + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			string text2 = null;
			text2 = streamReader.ReadLine();
			text2 = get_string_by_seperator(text2, ' ', 1);
			if (text2 != "1")
			{
				Sprache = 2;
			}
			text2 = "0" + text2;
			PNLanguageDir = text2.Substring(text2.Length - 2, 2);
			streamReader.Close();
		}
	}

	public static string get_string_by_seperator(string text, char sep, int pos)
	{
		string text2 = "";
		text = text.Trim();
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == sep)
			{
				return text.Substring(0, i);
			}
		}
		return "";
	}

	public static void get_popup_texte()
	{
		string text = PNDrive + "\\u\\pn\\lfiles\\" + PNLanguageDir + "\\POPBAS";
		if (!File.Exists(text))
		{
			return;
		}
		StreamReader streamReader = null;
		try
		{
			streamReader = new StreamReader(text, Encoding.GetEncoding("windows-1252"));
		}
		catch (Exception ex)
		{
			MessageBox.Show("Fehler beim Öffnen der Datei '" + text + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		string text2 = null;
		string text3 = "";
		string text4 = "";
		popupTexte.Clear();
		while (streamReader.Peek() != -1)
		{
			text2 = streamReader.ReadLine();
			if (text2 != "" && !text2.Trim().StartsWith("#"))
			{
				text2.Trim();
				text3 = text2.Substring(1, 6);
				text4 = ((text2.Length <= 8) ? " " : text2.Substring(8, text2.Length - 8));
				if (!popupTexte.TryGetValue(text3, out var _))
				{
					popupTexte.Add(text3, text4);
				}
			}
		}
		streamReader.Close();
	}

	public static void reset_array_popup()
	{
		for (int i = 0; i < arrPopUpListe.GetLength(0); i++)
		{
			arrPopUpListe[i, 0] = null;
		}
	}

	public static string get_maschinenname_by_number(string nummer)
	{
		string result = "";
		for (int i = 0; arrMaschinenListe[i, 0] != null; i++)
		{
			if (arrMaschinenListe[i, 0] == nummer)
			{
				result = arrMaschinenListe[i, 1] + " " + arrMaschinenListe[i, 2];
				break;
			}
		}
		return result;
	}

	public static string[] ReadTextIntoArray(string filename)
	{
		string[] result = null;
		if (File.Exists(filename))
		{
			try
			{
				result = File.ReadAllLines(filename, Encoding.Default);
			}
			catch (Exception)
			{
				throw;
			}
		}
		return result;
	}
}
