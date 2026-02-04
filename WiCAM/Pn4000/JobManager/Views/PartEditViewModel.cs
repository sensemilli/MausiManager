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
using static WiCAM.Pn4000.JobManager.PartEditControl;
using AnyCAD.Foundation;
using static WiCAM.Pn4000.Common.NativeMethods;
using UglyToad.PdfPig.Graphics.Operations.TextObjects;
using static iTextSharp.awt.geom.Point2D;

namespace WiCAM.Pn4000.JobManager
{
    public class PartEditViewModel :
      ViewModelBase,
      IPartEditViewModel,
      IViewModel
    {
        private ICommand _SavePartEdit;
        
        public double dichte { get; private set; }

        public IView View { get; private set; }

        public static PartEditViewModel _PartEditViewModel;
        public static PartEditViewModel _instance;
        private ICommand _PartEditCmd1;
        private ICommand _FPR997ToggleCommand;
        private ICommand _FPR998ToggleCommand;
        private ICommand _FPR999ToggleCommand;
        private ICommand _MessenCommand;
   
        private ICommand _ShowLeftFlyoutCommandMain;
        private bool traced;
        private ICommand _ClickLeft;
        private ICommand _ClickRight;
        private ICommand _ClickTop;
        private ICommand _ClickBottom;
        private ICommand _ClickFront;
        private ICommand _ClickBack;
        private ICommand _ShowAll;
        private ICommand _ShowWires;
        private ICommand _DXFtxtAnpassenCommand;

        public static PartEditViewModel Instance
        {
            get
            {
                if (PartEditViewModel._instance == null)
                {
                    //  Logger.Verbose("Initialize CadPartArchiveController");
                    PartEditViewModel._instance = new PartEditViewModel();
                }
                return PartEditViewModel._instance;
            }
        }


        public void Initialize(IView view, IJobManagerServiceProvider provider)
        {
            this.View = view;
            _PartEditViewModel = this;
            string desktoppfad = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach (var file in System.IO.Directory.GetFiles(desktoppfad + "\\stp"))
            {
                if (file.Contains(".stp") || file.Contains(".dxf") || file.Contains(".STP") || file.Contains(".DXF"))
                {
                    Console.WriteLine(file);
                    RootDirectoryItems.Add(file);

                }
            }
        }
        public void ViewReady()
        {
            PartEditControl._PartEditControl.mRenderCtrl.SetHilightingCallback((PickedResult pr) =>
            {
                var pt = pr.GetItem().GetPosition();
                MousePosition = $"{pt.x} {pt.y} {pt.z}";
             //   Console.WriteLine(MousePosition);
                return true;
            });

            PartEditControl._PartEditControl.mRenderCtrl.SetAnimationCallback((ViewerListener.AnimationHandler)((float timer) =>
            {
                RunAnimation(PartEditControl._PartEditControl.mRenderCtrl, timer);
            }));

            var topcol = new Vector3(3, 5, 0);
            var bottcol = new Vector3(0, 5, 3);
            var backcol = new GradientColorBackground(topcol, bottcol);
            PartEditControl._PartEditControl.mRenderCtrl.Viewer.SetBackground(backcol);
        }
        public static void RunAnimation(IRenderView render, float timer)
        {
            if (render != null)
            {
               Animation(render, timer);
            }
        }
        public static  void Animation(IRenderView render, float time)
        {
        }
        public ObservableCollection<Object> RootDirectoryItems { get; }
            = new ObservableCollection<object>();

        public string MousePosition { get; set; }
        
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

        public ICommand SavePartEdit
        {
            get
            {
                if (this._SavePartEdit == null)
                    this._SavePartEdit = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSavePartEdit()), (Predicate<object>)(x => true));
                return this._SavePartEdit;
            }
        }

        public void ButtonSavePartEdit()
        {
            Console.WriteLine("SavePartEdit");
            Logger.Info("SavePartEdit : {0}", (object)DateTime.Now.ToString("s"));
            PartEditControl._PartEditControl.SavePartEdit();
        }


        public ICommand PartEditCmd1
        {
            get
            {
                if (this._PartEditCmd1 == null)
                    this._PartEditCmd1 = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonPartEditCmd1()), (Predicate<object>)(x => true));
                return this._PartEditCmd1;
            }
        }

        public void ButtonPartEditCmd1()
        {
            Console.WriteLine("PartEditCmd1");
            Logger.Info("PartEditCmd1 : {0}", (object)DateTime.Now.ToString("s"));
            string file = PartEditControl._PartEditControl.pathToUserPartSource;
                    while (CheckLineNumber(file, "04" + MainWindow.mainWindow.xPW1from.Text + "0050") == 1)
                    {
                        string lineName = CheckLineName(file, "04" + MainWindow.mainWindow.xPW1from.Text + "0050");
                        Console.WriteLine(lineName);
                        Console.WriteLine(ChangeInLine(file, lineName, lineName, "04" + MainWindow.mainWindow.xPW1to.Text + "0050"));
                        string lineName1 = CheckLineName(file, "   1   " + MainWindow.mainWindow.xPW1from.Text + ".0000    5.0000");
                        Console.WriteLine(lineName1);
                        Console.WriteLine(ChangeInLine(file, lineName1, lineName1, "   1   " + MainWindow.mainWindow.xPW1to.Text + ".0000    5.0000"));

                        //xlWorksheetFileNames.Cells[iForFiles, 1] = Path.GetFileNameWithoutExtension(file);      76.2000        X-DIM OF PARTING TOOL........................
                    }
            PartEditControl._PartEditControl.ReadFPRtoEditBox();
        }
        
      
        private string CheckLineName(string fileName, string trackText)
        {
            int lineNumber = 0;
            string[] textLine = System.IO.File.ReadAllLines(fileName);
            string lineName = "";
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(trackText)) // Match text
                    {
                        //textLine[i] = newText; // replace text with new one.
                        traced = false;
                        //System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = 1;
                        lineName = textLine[i];
                        break; //go out from loop
                    }
            }
            return lineName;
        }
        private int ChangeInLine(string fileName, string trackText, string oldText, string newText)
        {
            int lineNumber = 0;
            //   fileName = "P:" + fileName;
            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(oldText)) // Match text
                    {
                      
                        Console.WriteLine(oldText.Length);
                        if (oldText.Length >= 10)
                        {
                            string endText = oldText.Substring(24, oldText.Length);
                            textLine[i] = newText + endText;
                        }
                        else
                        {
                            textLine[i] = newText;
                        }
                         // replace text with new one.
                        Console.WriteLine(textLine[i]);
                        traced = false;
                        System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = i;
                        break; //go out from loop
                    }
            }
            return lineNumber;
        }
        
        public ICommand FPR997toggleCommand
        {
            get
            {
                if (this._FPR997ToggleCommand == null)
                    this._FPR997ToggleCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonFPR997Toggle()), (Predicate<object>)(x => true));
                return this._FPR997ToggleCommand;
            }
        }

        public void ButtonFPR997Toggle()
        {
            Console.WriteLine("ButtonFPR997Toggle");
            Logger.Info("ButtonFPR997Toggle : {0}", (object)DateTime.Now.ToString("s"));
            Console.WriteLine("FPR997 =  " + AutoLoopControl._AutoLoopControl.pathToFPR);
            if (MainWindow.mainWindow.FPR997toggle.IsChecked == true)
            {
                AutoLoopControl._AutoLoopControl.pathToFPR = AutoLoopControl._AutoLoopControl.pathToPNdrive + AutoLoopControl._AutoLoopControl.pathToFPR997;
                MainWindow.mainWindow.FPR998toggle.IsChecked = false;
                MainWindow.mainWindow.FPR996toggle.IsChecked = false;
                MainWindow.mainWindow.FPR999toggle.IsChecked = false;
                MainWindow.mainWindow.saveFPR.Label = "FPR997 speichern";

            }
            Console.WriteLine("FPR Pfad =  " + AutoLoopControl._AutoLoopControl.pathToFPR + "  " + MainWindow.mainWindow.FPR997toggle.IsChecked);
            AutoLoopControl._AutoLoopControl.CheckToolBar();
            AutoLoopControl._AutoLoopControl.ReadFPRtoEditBox();
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
                MainWindow.mainWindow.FPR997toggle.IsChecked = false;
                MainWindow.mainWindow.FPR996toggle.IsChecked = false;
                MainWindow.mainWindow.saveFPR.Label = "FPR998 speichern";

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
                MainWindow.mainWindow.FPR997toggle.IsChecked = false;
                MainWindow.mainWindow.FPR996toggle.IsChecked = false;
                MainWindow.mainWindow.saveFPR.Label = "FPR999 speichern";

            }
            Console.WriteLine("FPR Pfad =  " + AutoLoopControl._AutoLoopControl.pathToFPR + "  " + MainWindow.mainWindow.FPR999toggle.IsChecked);
            AutoLoopControl._AutoLoopControl.CheckToolBar();
            AutoLoopControl._AutoLoopControl.ReadFPRtoEditBox();
        }


 


      

        public ICommand MessenCommand
        {
            get
            {
                if (this._MessenCommand == null)
                    this._MessenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.RibbonButtonMessenCommand_Click()), (Predicate<object>)(x => true));
                return this._MessenCommand;
            }
        }

        private void RibbonButtonMessenCommand_Click()
        {
            Console.WriteLine("RibbonButtonMessenCommand_Click =  ");
            if (MainWindow.mainWindow.xMessen.IsChecked == true)
                PartEditControl._PartEditControl.mRenderCtrl.SetEditor(new MyDistanceMeasureEditor());
            else
                PartEditControl._PartEditControl.mRenderCtrl.SetEditor(new AnyCAD.Foundation.StackEditor());

            Console.WriteLine("RibbonButtonMessenCommand_Click =  " + MainWindow.mainWindow.xMessen.IsChecked);
        }



        public ICommand ClickLeft
        {
            get
            {
                if (this._ClickLeft == null)
                    this._ClickLeft = (ICommand)new RelayCommand((Action<object>)(x => this.ClickLeftOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickLeft;
            }
        }

        public ICommand ClickRight
        {
            get
            {
                if (this._ClickRight == null)
                    this._ClickRight = (ICommand)new RelayCommand((Action<object>)(x => this.ClickRightOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickRight;
            }
        }

        public ICommand ClickTop
        {
            get
            {
                if (this._ClickTop == null)
                    this._ClickTop = (ICommand)new RelayCommand((Action<object>)(x => this.ClickTopOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickTop;
            }
        }
        public ICommand ClickBottom
        {
            get
            {
                if (this._ClickBottom == null)
                    this._ClickBottom = (ICommand)new RelayCommand((Action<object>)(x => this.ClickBottomOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickBottom;
            }
        }
        public ICommand ClickFront
        {
            get
            {
                if (this._ClickFront == null)
                    this._ClickFront = (ICommand)new RelayCommand((Action<object>)(x => this.ClickFrontOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickFront;
            }
        }
        public ICommand ClickBack
        {
            get
            {
                if (this._ClickBack == null)
                    this._ClickBack = (ICommand)new RelayCommand((Action<object>)(x => this.ClickBackOn3Dnav()), (Predicate<object>)(x => true));
                return this._ClickBack;
            }
        }

        private void ClickLeftOn3Dnav()
        {
            Console.WriteLine("ClickLeftOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Left, true);
        }
        private void ClickRightOn3Dnav()
        {
            Console.WriteLine("ClickRightOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Right, true);
        }
        private void ClickTopOn3Dnav()
        {
            Console.WriteLine("ClickTopOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Top, true);
        }
        private void ClickBottomOn3Dnav()
        {
            Console.WriteLine("ClickBottomOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Bottom, true);
        }
        private void ClickFrontOn3Dnav()
        {
            Console.WriteLine("ClickFrontOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Front, true);
        }
        private void ClickBackOn3Dnav()
        {
            Console.WriteLine("ClickBackOn3Dnav");
            PartEditControl._PartEditControl.mRenderCtrl.SetStandardView(EnumStandardView.Back, true);
        }



        public ICommand ShowAll
        {
            get
            {
                if (this._ShowAll == null)
                    this._ShowAll = (ICommand)new RelayCommand((Action<object>)(x => this.ClickShowAllOn3Dnav()), (Predicate<object>)(x => true));
                return this._ShowAll;
            }
        }

        private void ClickShowAllOn3Dnav()
        {
            Console.WriteLine("showall");
            PartEditControl._PartEditControl.mRenderCtrl.ViewContext.SetDisplayMode(EnumDisplayMode.Color);
        }

        public ICommand ShowWires
        {
            get
            {
                if (this._ShowWires == null)
                    this._ShowWires = (ICommand)new RelayCommand((Action<object>)(x => this.ClickShowWiresOn3Dnav()), (Predicate<object>)(x => true));
                return this._ShowWires;
            }
        }

        private void ClickShowWiresOn3Dnav()
        {
            Console.WriteLine("showwires");
            PartEditControl._PartEditControl.mRenderCtrl.ViewContext.SetDisplayMode(EnumDisplayMode.Wireframe);
        }

        public ICommand DXFtxtAnpassenCommand
        {
            get
            {
                if (this._DXFtxtAnpassenCommand == null)
                    this._DXFtxtAnpassenCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ClickDXFtxtAnpassenCommand()), (Predicate<object>)(x => true));
                return this._DXFtxtAnpassenCommand;
            }
        }

        private void ClickDXFtxtAnpassenCommand()
        {
            var dlg = new FolderBrowserDialog()
            {
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            dlg.ShowDialog();

            foreach (var file in System.IO.Directory.GetFiles(dlg.SelectedPath))
            {
                if (file.Contains(".dxf") || file.Contains(".DXF"))
                {
                    //Console.WriteLine(file);
                    Console.WriteLine(CheckLineNumber(file, "_BD_"));

                 //   PartEditViewModel._PartEditViewModel.RootDirectoryItems.Add(file);

                }
            }
        }

        private int CheckLineNumber(string fileName, string trackText)
        {
            int lineNumber = 0;
            // fileName = "P:" + fileName;
            Console.WriteLine(fileName);
            Console.WriteLine(trackText);

            string[] textLine = System.IO.File.ReadAllLines(fileName);
            for (int i = 0; i < textLine.Length; i++)
            {
                if (textLine[i].Contains(trackText)) //start finding matching text after.
                    traced = true;
                if (traced)
                    if (textLine[i].Contains(trackText)) // Match text
                    {
                        //textLine[i] = newText; // replace text with new one.
                        //System.IO.File.WriteAllLines(fileName, textLine);
                        lineNumber = i;
                        textLine[lineNumber - 4] = "25.00000";
                        string[] bendtextfull = textLine[lineNumber].Split(';');
                        string textToparse = bendtextfull[0].Replace("_BD_", "");
                        double angle = double.Parse(textToparse);
                        Console.WriteLine(angle + "     parsed");
                        //string forfloat = angle.ToString().Replace(',', '.');
                        string parsedangle;
                        
                        if (textToparse.Contains("-"))
                        {
                            angle = -180.00000f - angle;                            
                            Console.WriteLine(MyFloatToString(angle) + "  ins Minus");
                            textLine[lineNumber] = MyFloatToString(angle);
                        }
                        else
                        {
                            angle = 180.00000f - angle;
                            Console.WriteLine(MyFloatToString(angle) + "   ins Plus");
                            textLine[lineNumber] =  MyFloatToString(angle);
                        }
                        
                        Console.WriteLine(textLine[lineNumber]);
                    }           
            }
            System.IO.File.WriteAllLines(fileName, textLine);
            traced = false;
            return lineNumber;
        }
        string MyFloatToString(double f)
        {
            return f.ToString("0.0");
        }

        public ObservableCollection<UserStepFileTree> UserStepFileTree { get; set; } = new ObservableCollection<UserStepFileTree>();

    }

    public class UserStepFileTree
{
    public ObservableCollection<UserStepFile> Files { get; set; } = new ObservableCollection<UserStepFile>();
    public ObservableCollection<UserStepFileTree> Subfolders { get; set; } = new ObservableCollection<UserStepFileTree>();

    //  Concat demands a non-null argument
    public IEnumerable Items { get { return Subfolders?.Cast<Object>().Concat(Files); } }
    public bool IsExpanded { get; set; }

    public String DirectoryPath { get; set; }
    public String Name { get { return DirectoryPath; } }
}

public class UserStepFile
{
    public string FilePath { get; set; }
    public SceneNode Category { get; set; }
    public bool IsExpanded { get; set; }

    public string Name { get { return FilePath; } }

    public uint Ident { get; internal set; }
}


public class UserDirectory
    {
        public ObservableCollection<UserFile> Files { get; set; } = new ObservableCollection<UserFile>();
        public ObservableCollection<UserDirectory> Subfolders { get; set; } = new ObservableCollection<UserDirectory>();

        //  Concat demands a non-null argument
        public IEnumerable Items { get { return Subfolders?.Cast<Object>().Concat(Files); } }

        public String DirectoryPath { get; set; }
        public String Name { get { return System.IO.Path.GetFileName(DirectoryPath); } }
    }

    public class UserFile
    {
        public string FilePath { get; set; }
        public string Category { get; set; }

        public string Name { get { return System.IO.Path.GetFileName(FilePath); } }
    }
}
