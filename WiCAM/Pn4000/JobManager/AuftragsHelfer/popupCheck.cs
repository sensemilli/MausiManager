using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WiCAM.Pn4000.JobManager;

internal class popupCheck
{
	public static void get_popups(string machineNummer)
	{
		string text = pninit.PNDrive + "\\u\\pn\\pfiles\\00";
		if (directory_exist(text))
		{
			such_popups(text, "??????");
		}
		text = pninit.PNDrive + "\\u\\pn\\machine\\machine_" + machineNummer + "\\pfiles\\" + pninit.PNLanguageDir;
		if (directory_exist(text))
		{
			such_popups(text, "??????");
		}
		text = pninit.PNDrive + "\\u\\pn\\machine\\machine_" + machineNummer + "\\pfiles\\std";
		if (directory_exist(text))
		{
			such_popups(text, "??????");
		}
		text = pninit.PNDrive + "\\u\\pn\\machine\\machine_" + machineNummer + "\\pfiles\\00";
		if (directory_exist(text))
		{
			such_popups(text, "??????");
		}
		text = pninit.PNHomeDrive + pninit.PNHomePath + "\\pn.pop";
		if (directory_exist(text))
		{
			such_popups(text, "??????");
		}
		text = pninit.PNHomeDrive + pninit.PNHomePath + "\\pn.pop";
		if (directory_exist(text))
		{
			such_popups(text, "??????_" + machineNummer);
		}
	}

	public static bool directory_exist(string dirName)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(dirName);
		if (directoryInfo.Exists)
		{
			return true;
		}
		return false;
	}

	public static void such_popups(string verzName, string suchMuster)
	{
		string text = null;
		string text2 = null;
		string text3 = null;
		string text4 = null;
		string text5 = null;
		string[] files = Directory.GetFiles(verzName, suchMuster);
		string[] array = files;
		foreach (string text6 in array)
		{
			string text7 = text6;
			StreamReader streamReader = null;
			try
			{
				streamReader = new StreamReader(text7, Encoding.GetEncoding("windows-1252"));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Fehler beim Öffnen der Datei '" + text7 + "': " + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				break;
			}
			text2 = streamReader.ReadLine();
			if (text2.Contains("VERSION 2.00"))
			{
				text2 = streamReader.ReadLine();
				text2 = streamReader.ReadLine();
				if (text2 != null)
				{
					text = text2.Trim();
				}
				for (int j = 4; j < 25; j++)
				{
					text2 = streamReader.ReadLine();
				}
				text2 = streamReader.ReadLine();
				text2 = text2.Substring(0, 6);
				text2 = text2.Trim();
				text4 = ((text2 == "0") ? text : ((!pninit.popupTexte.TryGetValue(text2, out var value)) ? text : value));
				for (int j = 1; j < 8; j++)
				{
					text2 = streamReader.ReadLine();
				}
				text2 = text2.Trim();
				text5 = text2.Substring(0, 1);
				streamReader.Close();
			}
			else
			{
				text2 = streamReader.ReadLine();
				text4 = text2.Trim();
				text5 = "0";
			}
			text3 = text7;
			int length = text3.Length;
			int num = text3.LastIndexOf("\\");
			num++;
			text3 = text3.Substring(num, length - num);
			text5 = ((!(text5 == "1")) ? "" : "X");
			fuelle_Array(text3, text4, verzName, text5);
		}
	}

	public static void fuelle_Array(string name, string titel, string pfad, string auto)
	{
		string text = name;
		if (text.Length > 6)
		{
			text = text.Substring(0, 6);
		}
		for (int i = 0; i < pninit.arrPopUpListe.GetLength(0); i++)
		{
			if (pninit.arrPopUpListe[i, 0] == text)
			{
				pninit.arrPopUpListe[i, 0] = name;
				pninit.arrPopUpListe[i, 1] = titel;
				pninit.arrPopUpListe[i, 2] = pfad;
				pninit.arrPopUpListe[i, 3] = auto;
				break;
			}
			if (pninit.arrPopUpListe[i, 0] == null)
			{
				pninit.arrPopUpListe[i, 0] = name;
				pninit.arrPopUpListe[i, 1] = titel;
				pninit.arrPopUpListe[i, 2] = pfad;
				pninit.arrPopUpListe[i, 3] = auto;
				break;
			}
		}
	}
}
