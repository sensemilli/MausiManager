using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WiCAM.Pn4000.JobManager
{
    public interface IAuftragsDataViewModel :
      IViewModel

    {

        public GridLength Column1Width { get; set; }
        public GridLength GridArchiveHeight { get; set; }
        public GridLength GridFoldersHeight { get; set; }
        public GridLength GridFilesHeight { get; set; }


        double FontSize { get; set; }

        bool HasToIgnoreEnter { get; set; }


        ICommand ButtonSelectArchivCommand { get; }

        ICommand ButtonReadMaterialCSVCommand { get; }
        ICommand ButtonWriteToMaterialDBCommand { get; }
        ICommand ToggleMaterialCommand { get; }
        ICommand ToggleTeileCommand { get; }


        ICommand ButtonPnArchivBrowserCommand { get; }

        ICommand ButtonPnArchivBrowserSelectedCommand { get; }
        ICommand ButtonCreateMaterialPrintExcelCommand { get; }
        ICommand RibbonButtonOrdnerZuExelCommand { get; }
        ICommand ReadExcel2DCommand { get; }
        ICommand WriteExcel2DCommand { get; }
        ICommand ReadExcel3DCommand { get; }
        ICommand WriteExcel3DCommand { get; }
        ICommand Excel2SwiftCommand { get; }
        ICommand AbgleichCommand { get; }

        ICommand AddAuftragCommand { get; }

        ICommand ToggleAuftragCommand { get; }

        public ObservableCollection<GridRowData> GridDataa { get; set; }
        public GridRowData GridSelectedItemProperty { get; set; }
        public FolderFilesList FolderFilesSelectedItemProperty { get; set; }


        public int ArchivesAmount { get; set; }
        public string ArchivesPath { get; set; }

        public string TextFilter { get; set; }
        public string TextFilterFiles { get; set; }


        void SaveSettings();


    }
}
