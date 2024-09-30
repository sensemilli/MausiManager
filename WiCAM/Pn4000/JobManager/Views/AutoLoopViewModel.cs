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
    public class AutoLoopViewModel :
      ViewModelBase,
      IAutoLoopViewModel,
      IViewModel
    {
        private ICommand _SaveFPRedit;
        
        public double dichte { get; private set; }

        public IView View { get; private set; }

        public static AutoLoopViewModel _AutoLoopViewModel;
        public static AutoLoopViewModel _instance;
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

        public static AutoLoopViewModel Instance
        {
            get
            {
                if (AutoLoopViewModel._instance == null)
                {
                    //  Logger.Verbose("Initialize CadPartArchiveController");
                    AutoLoopViewModel._instance = new AutoLoopViewModel();
                }
                return AutoLoopViewModel._instance;
            }
        }


        public void Initialize(IView view, IJobManagerServiceProvider provider)
        {
            this.View = view;
            _AutoLoopViewModel = this;
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

        public ICommand SaveFPRedit
        {
            get
            {
                if (this._SaveFPRedit == null)
                    this._SaveFPRedit = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSaveFPRedit()), (Predicate<object>)(x => true));
                return this._SaveFPRedit;
            }
        }

        public void ButtonSaveFPRedit()
        {
            Console.WriteLine("ButtonSaveFPRedit");
            Logger.Info("ButtonSaveFPRedit : {0}", (object)DateTime.Now.ToString("s"));
            AutoLoopControl._AutoLoopControl.SaveFPRedit();
        }


        public ICommand FPR998toggleCommand
        {
            get
            {
                if (this._FPR998ToggleCommand == null)
                    this._FPR998ToggleCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonFPR998Toggle()), (Predicate<object>)(x => true));
                return this._FPR998ToggleCommand;
            }
        }

        public void ButtonFPR998Toggle()
        {
            Console.WriteLine("ButtonFPR998Toggle");
            Logger.Info("ButtonFPR998Toggle : {0}", (object)DateTime.Now.ToString("s"));
            Console.WriteLine("FPR998 =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.FPR998toggle.IsChecked == true)
            {
                AutoLoopControl._AutoLoopControl.pathToFPR = AutoLoopControl._AutoLoopControl.pathToPNdrive + AutoLoopControl._AutoLoopControl.pathToFPR998;
                MainWindow.mainWindow.FPR999toggle.IsChecked = false;
            }
            Console.WriteLine("FPR Pfad =  " + AutoLoopControl._AutoLoopControl.pathToFPR + "  " + MainWindow.mainWindow.FPR998toggle.IsChecked);
            AutoLoopControl._AutoLoopControl.CheckToolBar();
            AutoLoopControl._AutoLoopControl.ReadFPRtoEditBox();
        }

        public ICommand FPR999toggleCommand
        {
            get
            {
                if (this._FPR999ToggleCommand == null)
                    this._FPR999ToggleCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonFPR999Toggle()), (Predicate<object>)(x => true));
                return this._FPR999ToggleCommand;
            }
        }

        public void ButtonFPR999Toggle()
        {
            Console.WriteLine("ButtonFPR999Toggle");
            Logger.Info("ButtonFPR999Toggle : {0}", (object)DateTime.Now.ToString("s"));
            Console.WriteLine("FPR999 =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.FPR999toggle.IsChecked == true)
            {
                AutoLoopControl._AutoLoopControl.pathToFPR = AutoLoopControl._AutoLoopControl.pathToPNdrive + AutoLoopControl._AutoLoopControl.pathToFPR999;
                MainWindow.mainWindow.FPR998toggle.IsChecked = false;
            }
            Console.WriteLine("FPR Pfad =  " + AutoLoopControl._AutoLoopControl.pathToFPR + "  " + MainWindow.mainWindow.FPR999toggle.IsChecked);
            AutoLoopControl._AutoLoopControl.CheckToolBar();
            AutoLoopControl._AutoLoopControl.ReadFPRtoEditBox();
        }

        public ICommand LageCommand
        {
            get
            {
                if (this._LageCommand == null)
                    this._LageCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonLage_Click()), (Predicate<object>)(x => true));
                return this._LageCommand;
            }
        }

        private void RibbonButtonLage_Click()
        {
            Console.WriteLine("Lage =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xLage.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 AusrichtenLage", " 1  0  0   0        .0000       20     0     0 AusrichtenLage", " 1  1  0   0        .0000       20     0     0 AusrichtenLage"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 AusrichtenLage", " 1  1  0   0        .0000       20     0     0 AusrichtenLage", " 1  0  0   0        .0000       20     0     0 AusrichtenLage"));

            Console.WriteLine("Lage =  " + MainWindow.mainWindow.xLage.IsChecked);
        }

        public ICommand XrichtenCommand
        {
            get
            {
                if (this._XrichtenCommand == null)
                    this._XrichtenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonXrichten_Click()), (Predicate<object>)(x => true));
                return this._XrichtenCommand;
            }
        }

        private void RibbonButtonXrichten_Click()
        {
            Console.WriteLine("Xrichten =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xXrichten.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       10     0     0 xRichten", " 1  0  0   0        .0000       10     0     0 xRichten", " 1  1  0   0        .0000       10     0     0 xRichten"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       10     0     0 xRichten", " 1  1  0   0        .0000       10     0     0 xRichten", " 1  0  0   0        .0000       10     0     0 xRichten"));

            Console.WriteLine("Xrichten =  " + MainWindow.mainWindow.xXrichten.IsChecked);
        }

        public ICommand YrichtenCommand
        {
            get
            {
                if (this._YrichtenCommand == null)
                    this._YrichtenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonYrichten_Click()), (Predicate<object>)(x => true));
                return this._YrichtenCommand;
            }
        }

        private void RibbonButtonYrichten_Click()
        {
            Console.WriteLine("Yrichten =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xYrichten.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       10     0     0 yRichten", " 1  0  0   0        .0000       10     0     0 yRichten", " 1  1  0   0        .0000       10     0     0 yRichten"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       10     0     0 yRichten", " 1  1  0   0        .0000       10     0     0 yRichten", " 1  0  0   0        .0000       10     0     0 yRichten"));

            Console.WriteLine("Yrichten =  " + MainWindow.mainWindow.xYrichten.IsChecked);
        }

        public ICommand TrimmenCommand
        {
            get
            {
                if (this._TrimmenCommand == null)
                    this._TrimmenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonTrimmen_Click()), (Predicate<object>)(x => true));
                return this._TrimmenCommand;
            }
        }

        private void RibbonButtonTrimmen_Click()
        {
            Console.WriteLine("Trimmen =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xTrimmen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       10     0     0 Trimmen", " 1  0  0   0        .0000       10     0     0 Trimmen", " 1  1  0   0        .0000       10     0     0 Trimmen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       10     0     0 Trimmen", " 1  1  0   0        .0000       10     0     0 Trimmen", " 1  0  0   0        .0000       10     0     0 Trimmen"));

            Console.WriteLine("Trimmen =  " + MainWindow.mainWindow.xTrimmen.IsChecked);
        }

        public ICommand GleicheDCommand
        {
            get
            {
                if (this._GleicheDCommand == null)
                    this._GleicheDCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonGleicheD_Click()), (Predicate<object>)(x => true));
                return this._GleicheDCommand;
            }
        }

        private void RibbonButtonGleicheD_Click()
        {
            Console.WriteLine("GleicheD =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xGleicheD.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       10     0     0 GleicheD", " 1  0  0   0        .0000       10     0     0 GleicheD", " 1  1  0   0        .0000       10     0     0 GleicheD"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       10     0     0 GleicheD", " 1  1  0   0        .0000       10     0     0 GleicheD", " 1  0  0   0        .0000       10     0     0 GleicheD"));

            Console.WriteLine("GleicheD =  " + MainWindow.mainWindow.xGleicheD.IsChecked);
        }



        public ICommand CADdeleteCommand
        {
            get
            {
                if (this._CADdeleteCommand == null)
                    this._CADdeleteCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonCADdelete_Click()), (Predicate<object>)(x => true));
                return this._CADdeleteCommand;
            }
        }

        private void RibbonButtonCADdelete_Click()
        {
            Console.WriteLine("CADdelete =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xCADdelete.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       10     0     0 CADdelete", " 1  0  0   0        .0000       10     0     0 CADdelete", " 1  1  0   0        .0000       10     0     0 CADdelete"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       10     0     0 CADdelete", " 1  1  0   0        .0000       10     0     0 CADdelete", " 1  0  0   0        .0000       10     0     0 CADdelete"));

            Console.WriteLine("CADdelete =  " + MainWindow.mainWindow.xCADdelete.IsChecked);
        }


        public ICommand GravurCommand
        {
            get
            {
                if (this._GravurCommand == null)
                    this._GravurCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonGravur_Click()), (Predicate<object>)(x => true));
                return this._GravurCommand;
            }
        }

        private void RibbonButtonGravur_Click()
        {
            Console.WriteLine("Gravur1 =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xGravur.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 Gravur1", " 1  0  0   0        .0000       20     0     0 Gravur1", " 1  1  0   0        .0000       20     0     0 Gravur1"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 Gravur1", " 1  1  0   0        .0000       20     0     0 Gravur1", " 1  0  0   0        .0000       20     0     0 Gravur1"));

            Console.WriteLine("Gravur1 =  " + MainWindow.mainWindow.xGravur.IsChecked);
        }



        public ICommand StanzenInnenCommand
        {
            get
            {
                if (this._StanzenInnenCommand == null)
                    this._StanzenInnenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonStanzenInnen_Click()), (Predicate<object>)(x => true));
                return this._StanzenInnenCommand;
            }
        }

        private void RibbonButtonStanzenInnen_Click()
        {
            Console.WriteLine("STANZEN-Innen =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xStanzenInnen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 STANZEN-Innen", " 1  0  0   0        .0000       20     0     0 STANZEN-Innen", " 1  1  0   0        .0000       20     0     0 STANZEN-Innen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 STANZEN-Innen", " 1  1  0   0        .0000       20     0     0 STANZEN-Innen", " 1  0  0   0        .0000       20     0     0 STANZEN-Innen"));

            Console.WriteLine("STANZEN-Innen =  " + MainWindow.mainWindow.xStanzenInnen.IsChecked);
        }

        public ICommand KonturAussenCommand
        {
            get
            {
                if (this._KonturAussenCommand == null)
                    this._KonturAussenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonKonturAussen_Click()), (Predicate<object>)(x => true));
                return this._KonturAussenCommand;
            }
        }

        private void RibbonButtonKonturAussen_Click()
        {
            Console.WriteLine("Konturaussen =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xKonturAussen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 Konturaussen", " 1  0  0   0        .0000       20     0     0 Konturaussen", " 1  1  0   0        .0000       20     0     0 Konturaussen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 Konturaussen", " 1  1  0   0        .0000       20     0     0 Konturaussen", " 1  0  0   0        .0000       20     0     0 Konturaussen"));

            Console.WriteLine("Konturaussen =  " + MainWindow.mainWindow.xKonturAussen.IsChecked);
        }


        public ICommand FlaecheAussenCommand
        {
            get
            {
                if (this._FlaecheAussenCommand == null)
                    this._FlaecheAussenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonFlaecheAussen_Click()), (Predicate<object>)(x => true));
                return this._FlaecheAussenCommand;
            }
        }

        private void RibbonButtonFlaecheAussen_Click()
        {
            Console.WriteLine("FlaecheAussen =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xFlaecheAussen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 FlaecheAussen", " 1  0  0   0        .0000       20     0     0 FlaecheAussen", " 1  1  0   0        .0000       20     0     0 FlaecheAussen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 FlaecheAussen", " 1  1  0   0        .0000       20     0     0 FlaecheAussen", " 1  0  0   0        .0000       20     0     0 FlaecheAussen"));

            Console.WriteLine("FlaecheAussen =  " + MainWindow.mainWindow.xFlaecheAussen.IsChecked);
        }

        public ICommand StanzenAutoCommand
        {
            get
            {
                if (this._StanzenAutoCommand == null)
                    this._StanzenAutoCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonStanzenAuto_Click()), (Predicate<object>)(x => true));
                return this._StanzenAutoCommand;
            }
        }

        private void RibbonButtonStanzenAuto_Click()
        {
            Console.WriteLine("STANZEN-Auto =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xStanzenAuto.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 STANZEN-Auto", " 1  0  0   0        .0000       20     0     0 STANZEN-Auto", " 1  1  0   0        .0000       20     0     0 STANZEN-Auto"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 STANZEN-Auto", " 1  1  0   0        .0000       20     0     0 STANZEN-Auto", " 1  0  0   0        .0000       20     0     0 STANZEN-Auto"));

            Console.WriteLine("STANZEN-Auto =  " + MainWindow.mainWindow.xStanzenAuto.IsChecked);
        }



        public ICommand FlaecheStanzenCommand
        {
            get
            {
                if (this._FlaecheStanzenCommand == null)
                    this._FlaecheStanzenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonFlaecheStanzen_Click()), (Predicate<object>)(x => true));
                return this._FlaecheStanzenCommand;
            }
        }

        private void RibbonButtonFlaecheStanzen_Click()
        {
            Console.WriteLine("FlStanzen =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xFlaecheStanzen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 FlStanzen", " 1  0  0   0        .0000       20     0     0 FlStanzen", " 1  1  0   0        .0000       20     0     0 FlStanzen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 FlStanzen", " 1  1  0   0        .0000       20     0     0 FlStanzen", " 1  0  0   0        .0000       20     0     0 FlStanzen"));

            Console.WriteLine("FlStanzen =  " + MainWindow.mainWindow.xFlaecheStanzen.IsChecked);
        }


        public ICommand WKZaendernCommand
        {
            get
            {
                if (this._WKZaendernCommand == null)
                    this._WKZaendernCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonWKZaendern_Click()), (Predicate<object>)(x => true));
                return this._WKZaendernCommand;
            }
        }

        private void RibbonButtonWKZaendern_Click()
        {
            Console.WriteLine("ChangeWKZ =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.xChangeWKZ.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 ChangeWKZ", " 1  0  0   0        .0000       20     0     0 ChangeWKZ", " 1  1  0   0        .0000       20     0     0 ChangeWKZ"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 ChangeWKZ", " 1  1  0   0        .0000       20     0     0 ChangeWKZ", " 1  0  0   0        .0000       20     0     0 ChangeWKZ"));

            Console.WriteLine("ChangeWKZ =  " + MainWindow.mainWindow.xChangeWKZ.IsChecked);
        }

        public ICommand SortierenCommand
        {
            get
            {
                if (this._SortierenCommand == null)
                    this._SortierenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonSortieren_Click()), (Predicate<object>)(x => true));
                return this._SortierenCommand;
            }
        }

        private void RibbonButtonSortieren_Click()
        {
            Console.WriteLine("SortManual =  " + Path.Combine(AutoLoopControl._AutoLoopControl.pathToPNdrive, AutoLoopControl._AutoLoopControl.pathToFPR));
            if (MainWindow.mainWindow.xSortieren.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 SortManual", " 1  0  0   0        .0000       20     0     0 SortManual", " 1  1  0   0        .0000       20     0     0 SortManual"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 SortManual", " 1  1  0   0        .0000       20     0     0 SortManual", " 1  0  0   0        .0000       20     0     0 SortManual"));

            Console.WriteLine("SortManual =  " + MainWindow.mainWindow.xSortieren.IsChecked);
        }

        public ICommand NCdeleteCommand
        {
            get
            {
                if (this._NCdeleteCommand == null)
                    this._NCdeleteCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonNCdelete_Click()), (Predicate<object>)(x => true));
                return this._NCdeleteCommand;
            }
        }

     

        private void RibbonButtonNCdelete_Click()
        {
            Console.WriteLine("NCLoeschen =  " + Path.Combine(AutoLoopControl._AutoLoopControl.pathToPNdrive, AutoLoopControl._AutoLoopControl.pathToFPR));
            if (MainWindow.mainWindow.xNCLoeschen.IsChecked == true)
                Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  0  0   0        .0000       20     0     0 Loeschen", " 1  0  0   0        .0000       20     0     0 Loeschen", " 1  1  0   0        .0000       20     0     0 Loeschen"));
            else Console.WriteLine(AutoLoopControl._AutoLoopControl.FindLineNumberFPR(AutoLoopControl._AutoLoopControl.pathToFPR, " 1  1  0   0        .0000       20     0     0 Loeschen", " 1  1  0   0        .0000       20     0     0 Loeschen", " 1  0  0   0        .0000       20     0     0 Loeschen"));

            Console.WriteLine("NCLoeschen =  " + MainWindow.mainWindow.xNCLoeschen.IsChecked);
        }



    }
}
