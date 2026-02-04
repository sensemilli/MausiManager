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
using Microsoft.Office.Interop.Excel;

using iTextSharp.text.pdf.parser;

namespace WiCAM.Pn4000.JobManager
{
    public class CommonCutViewModel :
      ViewModelBase,
      ICommonCutViewModel,
      IViewModel
    {
        
        public double dichte { get; private set; }

        public IView View { get; private set; }

        public static CommonCutViewModel _CommonCutViewModel;
        public static CommonCutViewModel _instance;
        private ICommand _MultiShearCommand;
        private ICommand _StandartCommand;
        private ICommand _SechsundfuenzigCommand;
        private ICommand _SechsundvierzigCommand;
        private ICommand _DreissigCommand;

        private ICommand _SortXCommand;
        private ICommand _SelectVerticalTrimTool762x5Command;
        private ICommand _SelectVerticalTrimTool76x5Command;
        private ICommand _SelectVerticalTrimTool56x5Command;
        private ICommand _SelectVerticalTrimTool46x5Command;
        private ICommand _SelectVerticalTrimTool30x5Command;
        private ICommand _SelectVerticalTrimTool15x5Command;

        private ICommand _ShowLeftFlyoutCommandMain;
        private ICommand _SelectTrimTool762x5Command;
        private ICommand _SelectTrimTool76x5Command;
        private ICommand _SelectTrimTool56x5Command;
        private ICommand _SelectTrimTool46x5Command;
        private ICommand _SelectTrimTool30x5Command;
        private ICommand _SechundsiebzigzusechsundsiebzigzweiCommand;
        private ICommand _WLASTCommand;

        public static CommonCutViewModel Instance
        {
            get
            {
                if (CommonCutViewModel._instance == null)
                {
                    //  Logger.Verbose("Initialize CadPartArchiveController");
                    CommonCutViewModel._instance = new CommonCutViewModel();
                }
                return CommonCutViewModel._instance;
            }
        }


        public void Initialize(IView view, IJobManagerServiceProvider provider)
        {
            this.View = view;
            _CommonCutViewModel = this;
        
         }


        private void ShowLeftFlyoutMain()
        {
            Console.WriteLine("layout");
          //  MainWindow.mainWindow.LeftFlyout.IsOpen = !MainWindow.mainWindow.LeftFlyout.IsOpen;
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

        public ICommand MultiShearCommand
        {
            get
            {
                if (this._MultiShearCommand == null)
                    this._MultiShearCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonMultiShearCommand()), (Predicate<object>)(x => true));
                return this._MultiShearCommand;
            }
        }
        public void ButtonMultiShearCommand()
        {
            Console.WriteLine("MultiShearCommand");
            Logger.Info("MultiShearCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickMultiShearButton();
        }

        public ICommand StandartCommand
        {
            get
            {
                if (this._StandartCommand == null)
                    this._StandartCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonStandartCommand()), (Predicate<object>)(x => true));
                return this._StandartCommand;
            }
        }
        public void ButtonStandartCommand()
        {
            Console.WriteLine("ButtonStandartCommand");
            Logger.Info("ButtonStandartCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickStandartButton();
        }

        public ICommand SechsundfuenzigCommand
        {
            get
            {
                if (this._SechsundfuenzigCommand == null)
                    this._SechsundfuenzigCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSechsundfuenzigCommand()), (Predicate<object>)(x => true));
                return this._SechsundfuenzigCommand;
            }
        }
        public void ButtonSechsundfuenzigCommand()
        {
            Console.WriteLine("ButtonSechsundfuenzigCommand");
            Logger.Info("ButtonSechsundfuenzigCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSechsundfuenzigButton();
        }

        public ICommand SechsundvierzigCommand
        {
            get
            {
                if (this._SechsundvierzigCommand == null)
                    this._SechsundvierzigCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSechsundvierzigCommand()), (Predicate<object>)(x => true));
                return this._SechsundvierzigCommand;
            }
        }
        private void ButtonSechsundvierzigCommand()
        {
            Console.WriteLine("ButtonSechsundvierzigCommand");
            Logger.Info("ButtonSechsundvierzigCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSechsundvierzigButton();
        }

        public ICommand DreissigCommand
        {
            get
            {
                if (this._DreissigCommand == null)
                    this._DreissigCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonDreissigCommand()), (Predicate<object>)(x => true));
                return this._DreissigCommand;
            }
        }
        private void ButtonDreissigCommand()
        {
            Console.WriteLine("ButtonDreissigCommand");
            Logger.Info("ButtonDreissigCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickDreissigButton();
        }

        public ICommand SortXCommand
        {
            get
            {
                if (this._SortXCommand == null)
                    this._SortXCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSortXCommand()), (Predicate<object>)(x => true));
                return this._SortXCommand;
            }
        }
        public void ButtonSortXCommand()
        {
            Console.WriteLine("ButtonSortXCommand");
            Logger.Info("ButtonSortXCommand : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSortXButton();
        }

        public ICommand SelectVerticalTrimTool762x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool762x5Command == null)
                    this._SelectVerticalTrimTool762x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool762x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool762x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool762x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool762x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool762x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool762x5Button();
        }

        public ICommand SelectVerticalTrimTool76x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool76x5Command == null)
                    this._SelectVerticalTrimTool76x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool76x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool76x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool76x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool76x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool76x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool76x5Button();
        }

        public ICommand SelectVerticalTrimTool56x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool56x5Command == null)
                    this._SelectVerticalTrimTool56x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool56x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool56x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool56x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool56x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool56x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool56x5Button();
        }

        public ICommand SelectVerticalTrimTool46x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool46x5Command == null)
                    this._SelectVerticalTrimTool46x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool46x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool46x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool46x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool46x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool46x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool46x5Button();
        }

        public ICommand SelectVerticalTrimTool30x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool30x5Command == null)
                    this._SelectVerticalTrimTool30x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool30x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool30x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool30x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool30x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool30x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool30x5Button();
        }

        public ICommand SelectVerticalTrimTool15x5Command
        {
            get
            {
                if (this._SelectVerticalTrimTool15x5Command == null)
                    this._SelectVerticalTrimTool15x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectVerticalTrimTool15x5Command()), (Predicate<object>)(x => true));
                return this._SelectVerticalTrimTool15x5Command;
            }
        }
        public void ButtonSelectVerticalTrimTool15x5Command()
        {
            Console.WriteLine("ButtonSelectVerticalTrimTool15x5Command");
            Logger.Info("ButtonSelectVerticalTrimTool15x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectVerticalTrimTool15x5Button();
        }

        public ICommand SelectTrimTool30x5Command
        {
            get
            {
                if (this._SelectTrimTool30x5Command == null)
                    this._SelectTrimTool30x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectTrimTool30x5Command()), (Predicate<object>)(x => true));
                return this._SelectTrimTool30x5Command;
            }
        }
        public void ButtonSelectTrimTool30x5Command()
        {
            Console.WriteLine("ButtonSelectTrimTool30x5Command");
            Logger.Info("ButtonSelectTrimTool30x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectTrimTool30x5Button();
        }

        public ICommand SelectTrimTool46x5Command
        {
            get
            {
                if (this._SelectTrimTool46x5Command == null)
                    this._SelectTrimTool46x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectTrimTool46x5Command()), (Predicate<object>)(x => true));
                return this._SelectTrimTool46x5Command;
            }
        }
        public void ButtonSelectTrimTool46x5Command()
        {
            Console.WriteLine("ButtonSelectTrimTool46x5Command");
            Logger.Info("ButtonSelectTrimTool46x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectTrimTool46x5Button();
        }

        public ICommand SelectTrimTool56x5Command
        {
            get
            {
                if (this._SelectTrimTool56x5Command == null)
                    this._SelectTrimTool56x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectrimTool56x5Command()), (Predicate<object>)(x => true));
                return this._SelectTrimTool56x5Command;
            }
        }
        public void ButtonSelectrimTool56x5Command()
        {
            Console.WriteLine("ButtonSelectrimTool56x5Command");
            Logger.Info("ButtonSelectrimTool56x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectTrimTool56x5Button();
        }

        public ICommand SelectTrimTool76x5Command
        {
            get
            {
                if (this._SelectTrimTool76x5Command == null)
                    this._SelectTrimTool76x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectTrimTool76x5Command()), (Predicate<object>)(x => true));
                return this._SelectTrimTool76x5Command;
            }
        }
        public void ButtonSelectTrimTool76x5Command()
        {
            Console.WriteLine("ButtonSelectTrimTool76x5Command");
            Logger.Info("ButtonSelectTrimTool76x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectTrimTool76x5Button();
        }

        public ICommand SelectTrimTool762x5Command
        {
            get
            {
                if (this._SelectTrimTool762x5Command == null)
                    this._SelectTrimTool762x5Command = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectTrimTool762x5Command()), (Predicate<object>)(x => true));
                return this._SelectTrimTool762x5Command;
            }
        }
        public void ButtonSelectTrimTool762x5Command()
        {
            Console.WriteLine("ButtonSelectTrimTool762x5Command");
            Logger.Info("ButtonSelectTrimTool762x5Command : {0}", (object)DateTime.Now.ToString("s"));
            CommonCutControl._CommonCutControl.ClickSelectTrimTool762x5Button();
        }

        public ICommand SechundsiebzigzusechsundsiebzigzweiCommand
        {
            get
            {
                if (this._SechundsiebzigzusechsundsiebzigzweiCommand == null)
                    this._SechundsiebzigzusechsundsiebzigzweiCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSechundsiebzigzusechsundsiebzigzweiCommand()), (Predicate<object>)(x => true));
                return this._SechundsiebzigzusechsundsiebzigzweiCommand;
            }
        }
        public void ButtonSechundsiebzigzusechsundsiebzigzweiCommand()
        {
            Console.WriteLine("ButtonSechundsiebzigzusechsundsiebzigzweiCommand");
            Logger.Info("ButtonSechundsiebzigzusechsundsiebzigzweiCommand : {0}", (object)DateTime.Now.ToString("s"));
            //  CommonCutControl._CommonCutControl.Click76zu762Button();
   
          
        }
       
        public ICommand WLASTCommand
        {
            get
            {
                if (this._WLASTCommand == null)
                    this._WLASTCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonWLASTCommand()), (Predicate<object>)(x => true));
                return this._WLASTCommand;
            }
        }
        public void ButtonWLASTCommand()
        {
            string xTool = "";
            Console.WriteLine("WLASTCommand");
            Logger.Info("WLASTCommand : {0}", (object)DateTime.Now.ToString("s"));
            if (MainWindow.mainWindow.WLAST762x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST762x5.Name;
            if (MainWindow.mainWindow.WLAST76x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST76x5.Name;
            if (MainWindow.mainWindow.WLAST56x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST56x5.Name;
            if (MainWindow.mainWindow.WLAST46x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST46x5.Name;
            if (MainWindow.mainWindow.WLAST30x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST30x5.Name;
            if (MainWindow.mainWindow.WLAST15x5.IsChecked == true)
                xTool = MainWindow.mainWindow.WLAST15x5.Name;

            CommonCutControl._CommonCutControl.ClickWLASTCommandButton(xTool);
        }
    }
}
