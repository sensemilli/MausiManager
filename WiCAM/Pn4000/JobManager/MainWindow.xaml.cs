using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Archive.Browser.Classes;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.Gmpool;
using WiCAM.Pn4000.Archive.Browser.ArchiveN2d;
using WiCAM.Pn4000.WpfControls.ArchivesControl;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using Microsoft.Office.Interop.Excel;
using MahApps.Metro.Controls.Dialogs;
using Action = System.Action;
using Point = System.Windows.Point;
using Window = System.Windows.Window;
using Style = System.Windows.Style;
using WiCAM.Pn4000.Archive.Browser.Archive3d;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Screen;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using System.IO;
using static iTextSharp.text.pdf.PdfDocument;
using WiCAM.Pn4000.Screen.Classic2D;
using System.Windows.Interop;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.pn4.Interfaces;
using System.Collections.Generic;

namespace WiCAM.Pn4000.JobManager;

public partial class MainWindow : RibbonWindow, IDialogView, IView, IComponentConnector, IMainWindowDataProvider
{

    public static MainWindow _instance;
    public static MainWindow mainWindow;
    public JobHelper jobHelper;
    private DataGrid _grid;
    public DataGrid _gridArchive;

    private ArchiveWindowSettings _windowSettings;

    public GridLength OpenedArchivControlWidth;
    public Action WriteErg0Handler;

    public Action<int> SelectedArchivChangedHandler;

    public Func<ObservableCollection<WpfGridColumnInfo>> ConfigurationColumnsHandler;

    public Action<bool> ApplyGridSettingsHandler;

    public Action<bool> GridSelectionHandler;

    public Action DeleteSelectedHandler;

    public Action ExportToCsvHandler;

    public Action InitializeMainWindowHandler;
    private readonly static double __minMagnifyFactor;

    private readonly static double __maxMagnifyFactor;

    private double _magnifyFactor = MainWindow.__minMagnifyFactor;

    private Point _mousePosition;
    private WpfDataGridController<NcPartInfo> _gridController;
    private Window window;
    private int consoleID;
    private IntPtr consoleHandle;
    internal static MainWindow _MainWin;

    public static MainWindow Instance
    {
        get
        {
            if (MainWindow._instance == null)
            {
                //  Logger.Verbose("Initialize CadPartArchiveController");
                MainWindow._instance = new MainWindow();
                _MainWin = MainWindow._instance;
            }
            return MainWindow._instance;
        }
    }

    public MainWindowViewModel MainViewModel { get;  set; }
    public nint Handle { get; internal set; }
    public ExeFlow ExeFlow { get; internal set; }
    public int Active_screen_layout { get => ((IMainWindowDataProvider)_MainWin).Active_screen_layout; set => ((IMainWindowDataProvider)_MainWin).Active_screen_layout = value; }

    private string path;
    FileSystemWatcher watcher;
    bool _isCopied = false;
    FileSystemWatcher watcherExt;
    public IFactorio _factorio;
    private IScreen2D screen2D;

    private void watch()
    {
        path = "C:\\u\\pn\\machine\\machine_0001\\config";
        watcher = new FileSystemWatcher();
        watcher.Path = path;
        watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcher.Filter = "*.*";
        watcher.Changed += new FileSystemEventHandler(OnChanged);
        watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object source, FileSystemEventArgs e)
    {
        
        Console.WriteLine("changedInt  ", e);
        FileInfo infoOld = new FileInfo("P:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT");
        FileInfo infoNew = new FileInfo("C:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT");

        if (infoNew.LastWriteTime > infoOld.LastWriteTime)
        {
            File.Copy("C:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT", "P:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT", true);
            _isCopied = true;
            Console.WriteLine("filecopied int  ", e);

        }
        //Copies file to another directory.
    }
    private void watchext()
    {
        path = "P:\\u\\pn\\machine\\machine_0001\\config";
        watcherExt = new FileSystemWatcher();
        watcherExt.Path = path;
        watcherExt.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        watcherExt.Filter = "*.*";
        watcherExt.Changed += new FileSystemEventHandler(OnChangedExt);
        watcherExt.EnableRaisingEvents = true;
    }

    private void OnChangedExt(object source, FileSystemEventArgs e)
    {
        if (_isCopied)
        {
            _isCopied = false;
            return;
        }
        Console.WriteLine("changedExt  ", e);
        FileInfo infoOld = new FileInfo("C:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT");
        FileInfo infoNew = new FileInfo("P:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT");

        if (infoNew.LastWriteTime > infoOld.LastWriteTime)
        {
            File.Copy("P:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT", "C:\\u\\pn\\machine\\machine_0001\\config\\NORM_01__TOOLS.TXT", true);
            Console.WriteLine("filecopied ext ", e);

        }


        //Copies file to another directory.
    }
    public MainWindow()
    {
        _factorio = App.serviceProvider.GetRequiredService<IFactorio>();
        MainWindow._instance = this;
        mainWindow = this;
        jobHelper = new JobHelper();

        InitializeComponent();

        Handle = new WindowInteropHelper(this).Handle;
        watch();
        watchext();

        Style item = (Style)this.Resources["StyleRightAlignedCell"];



        ArchiveTreeFilterControlViewModel.SelectedArchiveChangedHandler = new Action<ArchiveInfo>(this.SelectedArchiveChanged);
        AppConfiguration.Erg0Format = AppArguments.Instance.Erg0Version();
        if (AppArguments.Instance.IsMultiselect)
        {
            Logger.Verbose("MS" + "hiiihiier");

            this.GrdArchiveData.SelectionMode = DataGridSelectionMode.Extended;
            this.GrdArchiveData.SelectionUnit = DataGridSelectionUnit.FullRow;
            //    this.MnuFilterSwitch.IsEnabled = false;
            AppConfiguration.Erg0Format = 1;
        }
        Logger.Verbose(GrdArchiveData + "hiiihiier");

        if (AppConfiguration.Instance.ArchiveType == ArchiveFileType.N2D)
        {
            Logger.Verbose(ArchiveFileType.N2D.ToString() + "hiiihiier");

            NcPartArchivController.Instance.Initialize(this);
           // Nc3DArchivController.Instance.Initialize(this);

            this._windowSettings = new ArchiveWindowSettings("WiCAM.ArchivBrowser", "WiCAM.ArchivBrowser");
        }

        ResourceDictionary resourceDictionaries = StringResourceHelper.Instance.FindDictionary(SystemConfiguration.PnLanguage);
        if (resourceDictionaries != null)
        {
            base.Resources.MergedDictionaries.Add(resourceDictionaries);
            this.FilterControl1.Resources.MergedDictionaries.Add(resourceDictionaries);
            this.ArchiveFilter.Resources.MergedDictionaries.Add(resourceDictionaries);
        }
        if (this._windowSettings != null)
        {
            this._windowSettings.ApplyWindowSettings(this);
            if (AppConfiguration.Instance.ArchiveType == ArchiveFileType.VAR)
            {
                this.Expander.IsExpanded = false;
                this.Expander.IsEnabled = false;
                this.ColumnExpanded.Width = new GridLength(1, GridUnitType.Pixel);
            }
        }
        if (this.InitializeMainWindowHandler != null)
        {
            this.InitializeMainWindowHandler();
        }
        //WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.ArchivesAreReadHandler = new Action(WiCAM.Pn4000.JobManager.MainWindow._instance.ArchivesAreRead);
        // WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ChangeSelectedArchive(7);
        WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber = AppArguments.Instance.ArchiveNumber();
        WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.Initialize(WiCAM.Pn4000.JobManager.MainWindow._instance.ArchiveFilter);
        winni.Title = string.Concat("Mausi", new string(' ', 20), "Version : ", "1.2 Release");

        //window = new Window();
        //window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //window.Topmost = true;
        //this.LocationChanged += OnLocationchanged;
        //window.Show();
        xConsoleControl.StartProcess("cmd.exe", string.Empty);
        //StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
        //standardOutput.AutoFlush = true;
        //Console.SetOut(standardOutput);
        consoleID = xConsoleControl.processInterface.process.Id;
        consoleHandle = xConsoleControl.processInterface.process.Handle;
        //AttachConsole(consoleHandle);
        //outputter = new TextBoxOutputter(TestBox);
        //  Console.SetOut(outputter);
        Console.WriteLine("StartedConsole  " + consoleHandle + "  " + consoleID);
        ThemeManager.Current.ChangeTheme(this, "Light.Green");
        xOrderFlyOut.Theme = FlyoutTheme.Dark;

        //App.serviceProvider.GetRequiredService<IFactorio>();
        //DialogManager.ShowDialogExternally<MessageDialog>(DialogManager.MessageDialog(), this);
        //MahApps.Metro.Controls.Dialogs.CustomDialog md = new CustomDialog();
        //md.Title = "TestDialog";
        //DialogManager.ShowDialogExternally<CustomDialog>(md, this);
        // md.ShowMessage("This is the title", "Some message");
    }

    public void initScreen2D()
    {
        ExeFlow = _factorio.Resolve<ExeFlow>();

        MainViewModel = MainWindowViewModel.Instance;
        MainViewModel.IsTab3D = true;
         screen2D = _factorio.Resolve<IScreen2D>();
     //   screen2D.OnUpdateSize += ExeFlow.KernelScreenSize;
      //  screen2D.OnMouseMove += ExeFlow.KernelScreenMouseMove;
    //    screen2D.OnMouseUp += ExeFlow.KernelScreenMouseUp;
      //  screen2D.OnMouseDown += ExeFlow.KernelScreenMouseDown;
        //screen2D.OnKeyDown += ExeFlow.ToKernelKeyDown;
        //screen2D.OnShortCut += ExeFlow.SupportShortCutKey;
        //screen2D.OnMultiModiTest += PartPanePanel.ScreenDragDropAction;
        //screen2D.IsDropAllowed += IsDropAllowed;
        //screen2D.FileDrop += FileDrop;
        //screen2D.IsWindowForCloseOutsideWindow += IsWindowForCloseOutsideWindow;
        screen2D.Init(partEditControl.KernelScreenPanel);
        (PresentationSource.FromVisual(this) as HwndSource).AddHook(WndProc);
        ClassicScreen2D classicScreen2D = (ClassicScreen2D)screen2D;
        WindowsFormsScreenControl screenControl = (WindowsFormsScreenControl)classicScreen2D.Host.Child;
        screenControl.drawManager = new DrawManager(Handle, 1000, 1000);
        screen2D.Swap();
        Set3D();
    }
    public void Set3D()
    {
  //      currentDocProvider.CurrentDoc?.UpdateGeneralInfo();
      partEditControl.Kernel3DScreen.Visibility = Visibility.Visible;
       partEditControl.KernelScreenPanel.Visibility = Visibility.Collapsed;
     /*   GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
        if (generalUserSettingsConfig.HideLeftTray)
        {
            ShowLeftToolBar(Visibility.Collapsed);
        }
        if (generalUserSettingsConfig.HideRightTray)
        {
            ShowRightToolBar(Visibility.Collapsed);
        }
        if (generalUserSettingsConfig.HideTopTray)
        {
            ShowTopToolBar(Visibility.Collapsed);
        }
        if (generalUserSettingsConfig.RibbonMode)
        {
            ribbon1.PulldownMode = false;
            ribbon1.ChangeRibbonMode();
        }
     */
    }
private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (hwnd != Handle)
        {
            return IntPtr.Zero;
        }
        switch (msg)
        {
            case 563:
                {
                    int num = WindowEvents.DragQueryFile(wParam, uint.MaxValue, null, 0);
                    string[] array = new string[num];
                    StringBuilder stringBuilder = new StringBuilder(1024);
                    for (uint num2 = 0u; num2 < num; num2++)
                    {
                        WindowEvents.DragQueryFile(wParam, num2, stringBuilder, 1024);
                        array[num2] = stringBuilder.ToString();
                    }
              //      FileDrop(array);
                    return IntPtr.Zero;
                }
            case 522:
                {
                    int delta = WindowEvents.GET_WHEEL_DELTA_WPARAM(wParam);
                //    if (QuickLunchManager.IsQuickLunchMenuVisible())
                  //  {
                    //    QuickLunchManager.MoueWheelToMenu(delta);
                    //}
                    //else
                    //{
                     //   ExeFlow.ToKernelMouseWheel(delta);
                   // }
                    handled = false;
                    return IntPtr.Zero;
                }
            case 1024:
                if ((int)wParam != 124)
                {
                    break;
                }
                try
                {
                    if (File.Exists(WindowEvents.parametertransferfile))
                    {
                      //  SetRecentlyUsed(File.ReadAllLines(WindowEvents.parametertransferfile));
                    //    Call_ru_param();
                        File.Delete(WindowEvents.parametertransferfile);
                    }
                }
                catch (Exception e)
                {
               //    LogCenterService.CatchRaport(e);
                }
                return IntPtr.Zero;
        }
        /*
        if (msg == 1024 && (int)wParam == MultitaskingManager.MtInfo)
        {
            handled = true;
            return new IntPtr(1);
        }
        if (msg == 1024 && (int)wParam == MultitaskingManager.MtSwithtome)
        {
            handled = true;
            if (!_closingAlready)
            {
                MultitaskingManager.Setup((int)lParam);
                base.Visibility = Visibility.Visible;
                WaitForRender();
                MultitaskingManager.LoadAndPropagateCurrentMutlitaskLocation();
                SendWindowState();
                ExeFlow.setup2D3D.screenAddOnManager.Update();
                MultitaskingManager.ChangeManagment();
                MultitaskingManager.HideAllNotMe();
            }
            return new IntPtr(1);
        }
        if (msg == 1024 && (int)wParam == MultitaskingManager.MtClose)
        {
            handled = true;
            if (!_closingAlready)
            {
                Close();
            }
            return new IntPtr(1);
        }
        if (msg == 1024 && (int)wParam == MultitaskingManager.MtCloseOnlyMe)
        {
            handled = true;
            MultitaskingManager.NotCheckClosingPossibility = true;
            if (!_closingAlready)
            {
                Close();
            }
            return new IntPtr(1);
        }
        if (msg == 1024 && (int)wParam == MultitaskingManager.MtHideMe)
        {
            handled = true;
            base.Visibility = Visibility.Hidden;
            WaitForRender();
            SendWindowState();
            return new IntPtr(1);
        }
        if (msg == 1024 && (int)wParam == 123)
        {
            ribbon1.SetServiceProvider();
            ExeFlow.Start();
            return IntPtr.Zero;
        }
        if (msg == 1024 && (int)wParam == 456)
        {
            PartPanePanel.UserEvent();
            return IntPtr.Zero;
        }
        */
        return IntPtr.Zero;
    }

    public void WaitForRender()
    {
        base.Dispatcher.Invoke((Action)delegate
        {
        }, DispatcherPriority.Render, null);
        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (ThreadStart)delegate
        {
        });
    }

    private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.ribbonTab1.IsSelected)
        {
            this.gridMain.Visibility = Visibility.Visible;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab2.IsSelected)
        {
            this.gridMain.Visibility = Visibility.Visible;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab04.IsSelected)
        {
            this.gridMain.Visibility = Visibility.Visible;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab05.IsSelected)
        {
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Visible;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab3.IsSelected)
        {
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Visible;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab6.IsSelected)
        {
            this.gridJob.Visibility = Visibility.Visible;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab7.IsSelected)
        {
            this.FPReditor.Visibility = Visibility.Visible;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab8.IsSelected)
        {
            this.gridCommonCut.Visibility = Visibility.Visible;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab9.IsSelected)
        {
            this.gridPNxpert.Visibility = Visibility.Visible;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
        else if (this.ribbonTab10.IsSelected)
        {
            this.gridPNpartEdit.Visibility = Visibility.Visible;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
            initScreen2D();
        }
        else if (this.ribbonTab11.IsSelected)
        {
            this.gridProduktionsPlan.Visibility = Visibility.Visible;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.MaterialView.Visibility = Visibility.Hidden;
        }
        else
        {
            if (!this.ribbonTab5.IsSelected)
                return;
            this.MaterialView.Visibility = Visibility.Visible;
            this.gridMain.Visibility = Visibility.Hidden;
            this.gridSettings.Visibility = Visibility.Hidden;
            this.PNarchive.Visibility = Visibility.Hidden;
            this.FPReditor.Visibility = Visibility.Hidden;
            this.gridJob.Visibility = Visibility.Hidden;
            this.gridCommonCut.Visibility = Visibility.Hidden;
            this.gridPNxpert.Visibility = Visibility.Hidden;
            this.gridPNpartEdit.Visibility = Visibility.Hidden;
            this.gridProduktionsPlan.Visibility = Visibility.Hidden;
        }
    }

    public void SetFlyoutState()
    {
        if (xOrderFlyOut.IsOpen == false)
            xOrderFlyOut.IsOpen = true;
        else
            xOrderFlyOut.IsOpen = false;
    }

    internal void ArchivesAreRead()
    {
        Logger.Verbose("archread   ++++" + "hiiihiier");
        Console.WriteLine("archread   ++++" + "hiiihiier");
        if (WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber <= 0)
        {
            WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ChangeSelectedArchive(7);
        }
        else
        {
            WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ChangeSelectedArchive(WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ArchiveNumber);
            if (WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.SelectedArchiv != null)
            {
                this.ChangeArchiveName(WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.SelectedArchiv);
            }
        }
        if (AppArguments.Instance.HasToExpandAll)
        {
            WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.ExpandAll();
        }
    }

    private void ChangeArchiveName(ArchiveInfo archive)
    {
        if (archive == null)
        {
            this.TxtArchiveName.Text = string.Empty;
            return;
        }
        if (archive.Number != 0)
        {
            this.TxtArchiveName.Text = archive.FullName;
            return;
        }
        this.TxtArchiveName.Text = StringResourceHelper.Instance.FindString("All");
    }

    private void SelectedArchiveChanged(ArchiveInfo archive)
    {
        this.StatusText2.Text = string.Empty;
        this.StatusImage2.Source = null;
        this.ChangeArchiveName(archive);
        if (archive == null)
        {
            return;
        }
        WiCAM.Pn4000.Archive.Browser.Helpers.ArchiveStructureManager.Instance.SelectedArchiv = archive;
        Action<int> selectedArchivChangedHandler = this.SelectedArchivChangedHandler;
        if (selectedArchivChangedHandler == null)
        {
            return;
        }
        selectedArchivChangedHandler(archive.FullArchiveNumber());
        Logger.Verbose(archive.FullArchiveNumber() + "hiiihiier");

    }
    private void ToggleSwitch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {

    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {

    }

    private void switchButtonEnabled(object sender, DependencyPropertyChangedEventArgs e)
    {

    }
    private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        Console.WriteLine("rowdoubleclick");
    }
    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      //  base.Close();
    }

    private void btnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (this.DeleteSelectedHandler != null)
        {
            if (MessageHelper.Question(StringResourceHelper.Instance.FindString(WiCAM.Pn4000.Archive.Browser.Classes.CS.MessageDeleteSelectedItems)) != MessageBoxResult.Yes)
            {
                return;
            }
            this.DeleteSelectedHandler();
        }
    }

    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
        if (this.WriteErg0Handler != null)
        {
            this.WriteErg0Handler();
        }
      //  base.Close();
    }

    private void expander_Collapsed(object sender, RoutedEventArgs e)
    {
        this.OpenedArchivControlWidth = this.ColumnExpanded.Width;
        this.ColumnExpanded.Width = new GridLength(25, GridUnitType.Pixel);
    }

    private void expander_Expanded(object sender, RoutedEventArgs e)
    {
        this.ColumnExpanded.Width = this.OpenedArchivControlWidth;
    }

    private void spCadGeo_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        this._mousePosition = e.GetPosition(this.CadgeoViewer1);
    }

    private void spCadGeo_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        double num = this._magnifyFactor;
        if (e.Delta >= 0)
        {
            this._magnifyFactor /= 2;
            if (this._magnifyFactor < MainWindow.__minMagnifyFactor)
            {
                this._magnifyFactor = MainWindow.__minMagnifyFactor;
            }
        }
        else
        {
            this._magnifyFactor *= 2;
            if (this._magnifyFactor > MainWindow.__maxMagnifyFactor)
            {
                this._magnifyFactor = MainWindow.__maxMagnifyFactor;
            }
        }
        this.CadgeoViewer1.MagnifyCenter = this._mousePosition;
        this.CadgeoViewer1.MagnifyFactor = this._magnifyFactor;
    }



    [SpecialName]
    bool? IDialogView.DialogResult()
    {
        return this.DialogResult;
    }


    [SpecialName]
    void IDialogView.DialogResult(bool? value)
    {
        this.DialogResult = value;
    }

    bool? IDialogView.ShowDialog()
    {
        return ShowDialog();
    }

    void IDialogView.Close()
    {
        Close();
    }

    [SpecialName]
    object IView.DataContext()
    {
        return this.DataContext;
    }

    [SpecialName]
    void IView.DataContext(object value)
    {
        this.DataContext = value;
    }

    internal bool Is2DAlternativeScreen()
    {
        throw new NotImplementedException();
    }

    public LikeModalMode GetLikeModalMode()
    {
        return ((IMainWindowDataProvider)_MainWin).GetLikeModalMode();
    }

    public void BlockUI_Command(string command)
    {
        ((IMainWindowDataProvider)_MainWin).BlockUI_Command(command);
    }

    public void OnScreenInfo_ClearStrings()
    {
        ((IMainWindowDataProvider)_MainWin).OnScreenInfo_ClearStrings();
    }

    public void OnScreenInfo_CalculateLocation(int ID)
    {
        ((IMainWindowDataProvider)_MainWin).OnScreenInfo_CalculateLocation(ID);
    }

    public void OnScreenInfo_UpdateString(int ID, string text)
    {
        ((IMainWindowDataProvider)_MainWin).OnScreenInfo_UpdateString(ID, text);
    }

    public object Get3DKernel()
    {
        return ((IMainWindowDataProvider)_MainWin).Get3DKernel();
    }

    public void AddRecentlyUsedRecord(RecentlyUsedRecord rec)
    {
        ((IMainWindowDataProvider)_MainWin).AddRecentlyUsedRecord(rec);
    }

    public void DeleteRecentlyUsedRecord(RecentlyUsedRecord rec)
    {
        ((IMainWindowDataProvider)_MainWin).DeleteRecentlyUsedRecord(rec);
    }

    public void DelRecentlyUsedRecordForType(string type)
    {
        ((IMainWindowDataProvider)_MainWin).DelRecentlyUsedRecordForType(type);
    }

    public void SetAllRecentlyUsedRecordForType(string type, IEnumerable<RecentlyUsedRecord> allRecords)
    {
        ((IMainWindowDataProvider)_MainWin).SetAllRecentlyUsedRecordForType(type, allRecords);
    }

    public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMachineRecords()
    {
        return ((IMainWindowDataProvider)_MainWin).GetRecentlyUsedMachineRecords();
    }

    public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMaterialRecords()
    {
        return ((IMainWindowDataProvider)_MainWin).GetRecentlyUsedMaterialRecords();
    }

    public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedImportRecords()
    {
        return ((IMainWindowDataProvider)_MainWin).GetRecentlyUsedImportRecords();
    }

    public void Ribbon_Activate3DViewTab()
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_Activate3DViewTab();
    }

    public void Ribbon_ActivateBendTab(bool force = false)
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_ActivateBendTab(force);
    }

    public void Ribbon_ActivateUnfoldTab(bool force = false)
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_ActivateUnfoldTab(force);
    }

    public void Ribbon_Activate3DTab(bool force = false)
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_Activate3DTab(force);
    }

    public void Ribbon_CloseSubMenu()
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_CloseSubMenu();
    }

    public void Ribbon_ShowSubMenu(string TabName, string RibbonFileName, int AssignedToTabID)
    {
        ((IMainWindowDataProvider)_MainWin).Ribbon_ShowSubMenu(TabName, RibbonFileName, AssignedToTabID);
    }

    public void Print3D()
    {
        ((IMainWindowDataProvider)_MainWin).Print3D();
    }

    public void SetViewerTab()
    {
        ((IMainWindowDataProvider)_MainWin).SetViewerTab();
    }

    public void SetUnfoldTab()
    {
        ((IMainWindowDataProvider)_MainWin).SetUnfoldTab();
    }

    public void SetViewForConfig(object element)
    {
        ((IMainWindowDataProvider)_MainWin).SetViewForConfig(element);
    }

    public UserControl GetActualConfig()
    {
        return ((IMainWindowDataProvider)_MainWin).GetActualConfig();
    }

    public void Set2D()
    {
        ((IMainWindowDataProvider)_MainWin).Set2D();
    }

    bool IMainWindowDataProvider.Is2DAlternativeScreen()
    {
        return ((IMainWindowDataProvider)_MainWin).Is2DAlternativeScreen();
    }

    public void Set2DAlternativeScreen(UserControl element, Action alternativeClosingAction)
    {
        ((IMainWindowDataProvider)_MainWin).Set2DAlternativeScreen(element, alternativeClosingAction);
    }

    public void StartHelp(string function_name)
    {
        ((IMainWindowDataProvider)_MainWin).StartHelp(function_name);
    }

    public void ShowRibbon(Visibility visibility)
    {
        ((IRibbon)_MainWin).ShowRibbon(visibility);
    }

    public void ShowLeftToolBar(Visibility visibility)
    {
        ((IRibbon)_MainWin).ShowLeftToolBar(visibility);
    }

    public void ShowRightToolBar(Visibility visibility)
    {
        ((IRibbon)_MainWin).ShowRightToolBar(visibility);
    }

    public void ShowTopToolBar(Visibility visibility)
    {
        ((IRibbon)_MainWin).ShowTopToolBar(visibility);
    }

    public void AddRibbonOverlay(UserControl element)
    {
        ((IRibbon)_MainWin).AddRibbonOverlay(element);
    }

    public void Ribbon_SelectButton(int PnCommandGroup, string PnCommandName, bool value)
    {
        ((IRibbon)_MainWin).Ribbon_SelectButton(PnCommandGroup, PnCommandName, value);
    }

    public int Ribbon_GetActiveTabID()
    {
        return ((IRibbon)_MainWin).Ribbon_GetActiveTabID();
    }

    public void Ribbon_SetActiveTab(int ID)
    {
        ((IRibbon)_MainWin).Ribbon_SetActiveTab(ID);
    }

    public void Ribbon_ActivateFirstVisibleTab()
    {
        ((IRibbon)_MainWin).Ribbon_ActivateFirstVisibleTab();
    }

    public void OnActiveTabChanged()
    {
        ((IRibbon)_MainWin).OnActiveTabChanged();
    }
}
