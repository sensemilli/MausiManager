using System;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.Archive.Browser;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.JobManager;

namespace WiCAM.Pn4000.Archive.Browser.Controllers
{
	internal interface IArchivController
	{
		void ApplyGridSettings(bool isMultiSelect);

		void ChangeSelection(bool isSelected);

		void DeleteSelected();

		void ExportToCsv();

		ObservableCollection<WpfGridColumnInfo> GridColumnsConfiguration();

		void Initialize(MainWindow mainWindow);

		void InitializeMainWindowToolbar();

		void SaveSettings();

		void WriteErg0File();
	}
}