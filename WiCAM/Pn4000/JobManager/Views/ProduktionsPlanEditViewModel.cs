using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Jobdata.Helpers;
using WiCAM.Pn4000.JobManager.LabelPrinter;
using WiCAM.Pn4000.Machine;
using WiCAM.Pn4000.WpfControls;
using WiCAM.Pn4000.Jobdata.Classes;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using WiCAM.Pn4000.TechnoTable;
using System.Windows.Controls.Ribbon;

namespace WiCAM.Pn4000.JobManager
{
    public class ProduktionsPlanEditViewModel :
      ViewModelBase,
      IProduktionsPlanEditViewModel,
      IViewModel
    {
        private ICommand _SavePartEdit;

        public double dichte { get; private set; }

        public IView View { get; private set; }

        public static ProduktionsPlanEditViewModel _ProduktionsPlanEditViewModel;
        public static ProduktionsPlanEditViewModel _instance;
        private ICommand _PartEditCmd1;
        private ICommand _FPR997ToggleCommand;
        private ICommand _FPR998ToggleCommand;
        private ICommand _FPR999ToggleCommand;
        private ICommand _SortierenCommand;
        private ICommand _NCdeleteCommand;
        private ICommand _FlaecheStanzenCommand;
        private ICommand _WKZaendernCommand;
        private ICommand _StanzenAutoCommand;
        private ICommand _FlaecheAussenCommand;
        private ICommand _KonturAussenCommand;
        private ICommand _StanzenInnenCommand;
        private ICommand _GravurCommand;
        private ICommand _CADdeleteCommand;
        private ICommand _GleicheDCommand;
        private ICommand _TrimmenCommand;
        private ICommand _YrichtenCommand;
        private ICommand _XrichtenCommand;
        private ICommand _LageCommand;
        private ICommand _ShowLeftFlyoutCommandMain;
        private ICommand _HuelleNeuCommand;
        private bool traced;
        private ICommand _ProduktionsPlanReadCmd;
        private ICommand _FilterVerpacktCmd;

        private AuftragsListeData _ProduktionsPlanDataGridSelectedItemProperty;
        private ICommand _FilterBiegenCmd;

        public static ProduktionsPlanEditViewModel Instance
        {
            get
            {
                if (ProduktionsPlanEditViewModel._instance == null)
                {
                    //  Logger.Verbose("Initialize CadPartArchiveController");
                    ProduktionsPlanEditViewModel._instance = new ProduktionsPlanEditViewModel();
                }
                return ProduktionsPlanEditViewModel._instance;
            }
        }


        public void Initialize(IView view, IJobManagerServiceProvider provider)
        {
            this.View = view;
            _ProduktionsPlanEditViewModel = this;
        }




        private void ShowLeftFlyoutMain()
        {
            Console.WriteLine("layout");
            // MainWindow.mainWindow.LeftFlyout.IsOpen = !MainWindow.mainWindow.LeftFlyout.IsOpen;
        }

        public ICommand ShowLeftFlyoutCommandMain
        {
            get
            {
                if (this._ShowLeftFlyoutCommandMain == null)
                    this._ShowLeftFlyoutCommandMain = (ICommand)new RelayCommand((Action<object>)(x => this.ShowLeftFlyoutMain()), (Predicate<object>)(x => true));
                return this._ShowLeftFlyoutCommandMain;
            }
        }

        public ObservableCollection<AuftragsListeData> ProduktionsPlanDataGrid { get; set; }
        public AuftragsListeData ProduktionsPlanDataGridSelectedItemProperty
        {
            get { return _ProduktionsPlanDataGridSelectedItemProperty; }
            set
            {
                _ProduktionsPlanDataGridSelectedItemProperty = value;
                if (_ProduktionsPlanDataGridSelectedItemProperty != null)
                {

                    OnPropertyChanged("ProduktionsPlanDataGridSelectedItemProperty");
                    Console.WriteLine(
                        value.Vorgangsname + "     " + value.Auftrag + "     " + value.RowNumber);
                }
            }
        }

        public ICommand ProduktionsPlanReadCmd
        {
            get
            {
                if (this._ProduktionsPlanReadCmd == null)
                    this._ProduktionsPlanReadCmd = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonProduktionsPlanReadCmd()), (Predicate<object>)(x => true));
                return this._ProduktionsPlanReadCmd;
            }

        }

        public void ButtonProduktionsPlanReadCmd()
        {
            Console.WriteLine("ProduktionsPlanReadCmd");
            Logger.Info("ProduktionsPlanReadCmd : {0}", (object)DateTime.Now.ToString("s"));
            ProduktionsPlanEditControl._ProduktionsPlanEditControl.ProduktionsPlanRead();
        }

        public ICommand FilterVerpacktCmd
        {
            get
            {
                if (this._FilterVerpacktCmd == null)
                    this._FilterVerpacktCmd = (ICommand)new RelayCommand((Action<object>)(x => this.ChckBoxFilterVerpacktCmd()), (Predicate<object>)(x => true));
                return this._FilterVerpacktCmd;
            }

        }
        public void ChckBoxFilterVerpacktCmd()
        {
            Console.WriteLine("ChckBoxFilterVerpacktCmd");
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}", (object)DateTime.Now.ToString("s"));
            ProduktionsPlanEditControl._ProduktionsPlanEditControl.ProduktionsPlanReadFilteredVerpackt();
        }



        public ICommand FilterBiegenCmd
        {
            get
            {
                if (this._FilterBiegenCmd == null)
                    this._FilterBiegenCmd = (ICommand)new RelayCommand((Action<object>)(x => this.ChckBoxFilterBiegenCmd()), (Predicate<object>)(x => true));
                return this._FilterBiegenCmd;
            }

        }
        public void ChckBoxFilterBiegenCmd()
        {
            Console.WriteLine("ChckBoxFilterVerpacktCmd");
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}", (object)DateTime.Now.ToString("s"));
            ProduktionsPlanEditControl._ProduktionsPlanEditControl.ProduktionsPlanReadFilteredBiegen();
        }
    }
        public class AuftragsListeData
        {
            public int? IDB050 { get; set; }

            public string? Auslieferung { get; set; }
            public string? Object { get; set; }
            public string? Vorgangsname { get; set; }
            public string? Auftrag { get; set; }
            public string? Bestellnummer { get; set; }
            public string? Artikel { get; set; }

            public string? Anfang { get; set; }
            public string? Fertig { get; set; }

            public string? Oberflaeche { get; set; }

            public string? Infos { get; set; }

            public string? Zustaendig { get; set; }
            public string? AV { get; set; }

            public string? MaterialBestellt { get; set; }
            public string? Programmiert { get; set; }
            public string? Abgewickelt { get; set; }
            public string? Stanzen { get; set; }
            public string? Biegen { get; set; }
            public string? Schweissen { get; set; }
            public string? Verpacken { get; set; }

            public int? RowNumber { get; set; }
        }
    }
