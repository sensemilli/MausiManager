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

namespace WiCAM.Pn4000.JobManager;

public partial class MainWindow : RibbonWindow, IDialogView, IView, IComponentConnector
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
    public static MainWindow Instance
    {
        get
        {
            if (MainWindow._instance == null)
            {
                //  Logger.Verbose("Initialize CadPartArchiveController");
                MainWindow._instance = new MainWindow();
            }
            return MainWindow._instance;
        }
    }
    public MainWindow()
    {
        MainWindow._instance = this;
        mainWindow = this;
        jobHelper = new JobHelper();
        InitializeComponent();

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
        winni.Title = string.Concat("Mausi", new string(' ', 20), "Version : ", "1.2");

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
        
        //DialogManager.ShowDialogExternally<MessageDialog>(DialogManager.MessageDialog(), this);
        //MahApps.Metro.Controls.Dialogs.CustomDialog md = new CustomDialog();
        //md.Title = "TestDialog";
        //DialogManager.ShowDialogExternally<CustomDialog>(md, this);
       // md.ShowMessage("This is the title", "Some message");
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
}
