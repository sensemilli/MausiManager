using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.pn4.pn4Services;

public interface ILanguageDictionary
{
	List<PipLstEntry> PipLst { get; set; }

	int CurrentLanguage { get; }

	event Action<int> OnLanguageChanged;

	bool ChangeActiveLanguage();

	int GetCurrentLanguage();

	int GetInchMode();

	int GetViewer();

	int SetCurrentPnLanguageId();

	string GetPophlp(int idx);

	string GetMsg2Int(string key);

	string GetPipbas(int id);

	string GetPiphlp(int id);

	bool CheckRibbonTabOff(int idx);

	string GetCurrentLanguageXmlString();

	string GetButtonLabel(int idLabel, string defaultLabel);

	string GetButtonToolTip(int helpid, int textid, string deflabel);
}
