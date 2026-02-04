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
using Microsoft.Office.Interop.Excel;
using System.Printing;
using System.Drawing.Printing;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Materials;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Gmpool;
using WiCAM.Pn4000.Archive;
using System.Windows.Forms.Design;
using WiCAM.Pn4000.Database;
using DataManager = WiCAM.Pn4000.Materials.DataManager;
using System.Data;
using System.Windows.Data;
using System.Windows.Shapes;
using WiCAM.Pn4000.JobManager.AuftragsHelfer;
using MahApps.Metro.Controls;
using System.Xml.Linq;
using WiCAM.Pn4000.BendModel.BendTools.Macros;
using WiCAM.Pn4000.PN3D.BendSimulation.PP.Bystronic_BPX;
using WiCAM.Pn4000.PN3D.Popup.Information;
using System.IO.Compression;

namespace WiCAM.Pn4000.JobManager
{
    public class AuftragsDataViewModel :
      ViewModelBase,
      IAuftragsDataViewModel,
      IViewModel,

      IPreviewObserver
    {
        private IJobManagerServiceProvider _provider;
        private IJobManagerSettings _settings;
        private IStateManager _stateManager;

        private double _fontSize = 12.0;
        private int _ArchivesAmount;
        private string _ArchivesPath;
        private GridLength _GridArchiveHeight;
        private GridLength _gridFoldersHeight;
        private int _maxJobsAmount = 100000;
        private int _readJobsAmount;
        private Visibility _progressVisibility = Visibility.Hidden;
        private int _jobsFiltered;

        private ICommand _RibbonButtonOrdnerZuExelCommand;
        private ICommand _ButtonReadExcel2DCommand;
        private ICommand _ButtonSelectArchivCommand;
        private ICommand _ButtonReadMaterialCSVCommand;
        private ICommand _ButtonPnArchivBrowserCommand;
        private ICommand _ButtonPnArchivBrowserSelectedCommand;
        private ICommand _ButtonCreateMaterialPrintExcelCommand;
        private OrdersData _OrdersGridSelectedItemProperty;
        private OrdersData _SelectedAuftrag;
        private int _ReadArchivesAmount;
        private int _MaxArchivesAmount;
        private FrameworkElement _activeView;
        private string _TextFilterFiles;
        public ObservableCollection<PlateData> _PlateData;
        private ICommand _ToggleMaterialCommand;
        private ICommand _ButtonWriteToMaterialDBCommand;
        private GridRowData _gridSelectedItemProperty;
        private PartOrderData _PartOrderGridSelectedItemProperty;
        private string _TextFilter;

        private string pathToPNdrive;
        private int iiii;
        private string connectionString;
        private string readablePhrase;
        private string[] kk;
        private double resultP;
        private string[] mm;
        private string[] pp;
        private double resultM;
        private double resultK;

        private GridLength _gridPlateHeight;
        public static AuftragsDataViewModel _instance;
        private string part1;
        private string part2;
        private string txtANummer;
        private string txtAName;
        public static int _archivesAmount;
        private GridLength _gridFilesHeight;
        private GridLength _column1Width;


        public double dichte { get; private set; }

        public IView View { get; private set; }

        public InternJobPool _InternJobPool;
        private SQLMaterialPool _SQLMaterialPool;
        public static AuftragsDataViewModel _auftragsDataViewModel;

        public ObservableCollection<JobInfo> Jobs { get; set; } = new ObservableCollection<JobInfo>();

        public ObservableCollection<PlateInfo> Plates { get; set; } = new ObservableCollection<PlateInfo>();

        public ObservableCollection<PartInfo> Parts { get; set; } = new ObservableCollection<PartInfo>();

        public ObservableCollection<PartOrderData> PartOrderList { get; set; } = new ObservableCollection<PartOrderData>();
        public ObservableCollection<PartOrderData> _PartOrderList;
        public ObservableCollection<OrdersData> _OrdersData;

        public ObservableCollection<FolderFilesList> _FolderFilesList;

        public ObservableCollection<GridRowData> _gridDataa;
        private ICommand _RibbonButtonReadExcelCommand;


        public static AuftragsDataViewModel Instance
        {
            get
            {
                if (AuftragsDataViewModel._instance == null)
                {
                    //  Logger.Verbose("Initialize CadPartArchiveController");
                    AuftragsDataViewModel._instance = new AuftragsDataViewModel();
                }
                return AuftragsDataViewModel._instance;
            }
        }
        private DataGrid _grid;
        public List<T> SelectedItems { get; set; }
        private Action<List<T>> _selectionChangedHandler;
        private ICommand _RibbonButtonWriteExcelCommand;
        private FolderFilesList _FolderFilesListSelectedItemProperty;
        private ICommand _RibbonButtonAddAuftragCommand;
        private ICommand _RibbonButtonToggleAuftragCommand;
        private ICommand _ButtonWriteExcel2DCommand;
        private StockAuftragInfo _selectedItem;
        private WpfDataGridController<StockAuftragInfo> _gridHelper;
        public SelectionChangedEventHandler SelectionChanged { get; set; }

        public ObservableCollection<StockAuftragInfo> AuftragsCollectionProperty { get; set; } = new ObservableCollection<StockAuftragInfo>();

        private List<StockAuftragInfo> _filteredByType;
        DependencyObject _DependencyObject;

        public void Initialize(IView view, IJobManagerServiceProvider provider)
        {
            this.View = view;
            _InternJobPool = new InternJobPool();
            _SQLMaterialPool = new SQLMaterialPool();
            _DependencyObject = new DependencyObject();

            _auftragsDataViewModel = this;
            pathToPNdrive = PnPathBuilder.PnDrive;
            _grid = AuftragsDataControl._AuftragsDataControl.GridOrderParts;
            this._provider = provider;
            this._settings = this._provider.FindService<IJobManagerSettings>();
            this._stateManager = this._provider.FindService<IStateManager>();
            this.ArchivesPath = PnPathBuilder.ArDrive + "\\u\\ar\\abasis\\ABASDF";
            this.GridArchiveHeight = this.CreateHeight(this._settings.GridArchiveHeight);
            this.GridFoldersHeight = this.CreateHeight(this._settings.GridFoldersHeight);
            this.GridFilesHeight = this.CreateHeight(this._settings.GridFilesHeight);
            this.Column1Width = new GridLength((double)this._settings.Column1Width, GridUnitType.Pixel);
            _grid.SelectionChanged += DataGrid_SelectionChanged;
            this._stateManager.AttachImageObserver((IPreviewObserver)this);
            ReadArchives();
            this._ArchivesAmount = _archivesAmount;

            this.GridOrders = AuftragsDataControl._AuftragsDataControl.GridOrders;
            this.PartOrders = AuftragsDataControl._AuftragsDataControl.GridOrderParts;

            this.GridMaterialPool = AuftragsDataControl._AuftragsDataControl.GridMaterialPool;

            _InternJobPool.OpenSQLOrderDataConnection(this);

            System.Windows.Style item = (System.Windows.Style)AuftragsDataControl._AuftragsDataControl.Resources["StyleRightAlignedCell"];

            SelectionChanged = new SelectionChangedEventHandler(this.SelectedAuftragChanged);



        }




        public void Edit(ViewAction action)
        {

            AddAuftragControl editControl = new AddAuftragControl()
            {
                DataContext = new AddAuftragControlViewModel(this.OrdersData, null, this.SelectedAuftrag, action, () => this.CloseAction())
            };
            //this._grid.IsEnabled = false;
            this.ActiveView = editControl;
            //this.ActiveView = editControl;
            //MainWindow.Instance.xCControl = editControl;
            //	MainWindow.Instance.dockEdit = new DockPanel();
            AuftragsDataControl._AuftragsDataControl.dockEdit.Children.Clear();
            AuftragsDataControl._AuftragsDataControl.dockEdit.Children.Add(editControl);

            AuftragsDataControl._AuftragsDataControl.dockEdit.Visibility = Visibility.Visible;
        }
        private List<StockAuftragInfo> _materials;
        private ICommand _ToggleTeileCommand;
        private ICommand _Excel2SwiftCommand;
        private ICommand _AbgleichCommand;
        private string _MaterialTextFilter;
        private string _DickeTextFilter;
        private string _MatNrTextFilter;
        private string _AuftragFilterText;

        private void CloseAction()
        {
            Console.WriteLine(ActiveView);
            this.ActiveView = null;
            // this._task = Task.Run<bool>(() => this.Enable(this._grid));
        }
        public OrdersData SelectedAuftrag
        {
            get
            {
                return this._SelectedAuftrag;
            }

            set
            {
                this._SelectedAuftrag = value;
            }
        }




        private void SelectedAuftragChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    OrdersData item = e.AddedItems[0] as OrdersData;
                    Console.WriteLine(item);
                    if (item != null)
                    {
                        string empty = string.Empty;
                        SelectedAuftrag = item;
                    }
                }
                else
                    SelectedAuftrag = OrdersGridSelectedItemProperty;
            }
            catch (Exception exception)
            {
                Logger.Exception(exception);
            }
        }






        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_grid.SelectedItems.Count == 0 || _grid.SelectedItem == null || _grid.SelectedItem.GetType() != typeof(T))
            {
                return;
            }
            SelectedItems.Clear();
            foreach (T selectedItem in _grid.SelectedItems)
            {
                SelectedItems.Add(selectedItem);
                Console.WriteLine(selectedItem.ToString());
            }
            _selectionChangedHandler?.Invoke(SelectedItems);
        }



        public ICommand AddAuftragCommand
        {
            get
            {
                if (this._RibbonButtonAddAuftragCommand == null)
                    this._RibbonButtonAddAuftragCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonAddAuftrag()), (Predicate<object>)(x => true));
                return this._RibbonButtonAddAuftragCommand;
            }
        }

        public void ButtonAddAuftrag()
        {
            Console.WriteLine("_RibbonButtonAddAuftragCommand");
            Logger.Info("_RibbonButtonAddAuftragCommand : {0}", (object)DateTime.Now.ToString("s"));
            this.RibbonButtonAddAuftrag();
        }
        public void RibbonButtonAddAuftrag()
        {
            Console.Write("RibbonButtonAddAuftrag");
            //if (this.SelectedItem != null)
            //{
            //    this.GridOrders.SelectedItems.Clear();
            //}
            //this.SelectedItem = new StockAuftragInfo();
            //TypeInitializer.Initialize(this.SelectedItem);
            //StockAuftragInfo.Initialize(this.SelectedItem, SystemConfiguration.UseInch);
            //this.SelectedAuftrag = this.SelectedItem;
            Edit(ViewAction.Create);
        }



        public ICommand RibbonButtonOrdnerZuExelCommand
        {
            get
            {
                if (this._RibbonButtonOrdnerZuExelCommand == null)
                    this._RibbonButtonOrdnerZuExelCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonOrdnerZuExel()), (Predicate<object>)(x => true));
                return this._RibbonButtonOrdnerZuExelCommand;
            }
        }

        public void ButtonOrdnerZuExel()
        {
            Console.WriteLine("ExcelICommand");
            Logger.Info("RibbonButtonOrdnerZuExel : {0}", (object)DateTime.Now.ToString("s"));
            this.RibbonButtonOrdnerZuExel();
        }

        public void RibbonButtonOrdnerZuExel()
        {
            Console.Write("RibbonButtonOrdnerZuExel_Click");
            MainWindow.mainWindow.jobHelper.CreateDXFexelTab();
        }


        public ICommand ReadExcel2DCommand
        {
            get
            {
                if (this._ButtonReadExcel2DCommand == null)
                    this._ButtonReadExcel2DCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonReadExcel2D()), (Predicate<object>)(x => true));
                return this._ButtonReadExcel2DCommand;
            }
        }

        public void ButtonReadExcel2D()
        {
            Console.WriteLine("Read-Excel-2D");
            Logger.Info("Read-Excel-2D : {0}", (object)DateTime.Now.ToString("s"));
            MainWindow.mainWindow.jobHelper.ReadExcel2D(MainWindow.mainWindow.txtKundenName.Text, MainWindow.mainWindow.txtAuftragsNummer.Text, MainWindow.mainWindow.xDatePicker.Text, MainWindow.mainWindow.txtArchivName.Text);
        }

        public ICommand WriteExcel2DCommand
        {
            get
            {
                if (this._ButtonWriteExcel2DCommand == null)
                    this._ButtonWriteExcel2DCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonWriteExcel2D()), (Predicate<object>)(x => true));
                return this._ButtonWriteExcel2DCommand;
            }
        }


        public void ButtonWriteExcel2D()
        {
            Console.WriteLine("Write-Excel-2D");
            Logger.Info("Write-Excel-2D : {0}", (object)DateTime.Now.ToString("s"));
            MainWindow.mainWindow.jobHelper.WritePN2DExcel(MainWindow.mainWindow.txtKundenName.Text, MainWindow.mainWindow.txtAuftragsNummer.Text, MainWindow.mainWindow.xDatePicker.Text, MainWindow.mainWindow.txtArchivName.Text);
        }

        public ICommand ReadExcel3DCommand
        {
            get
            {
                if (this._RibbonButtonReadExcelCommand == null)
                    this._RibbonButtonReadExcelCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonReadExcel3D()), (Predicate<object>)(x => true));
                return this._RibbonButtonReadExcelCommand;
            }
        }


        public void ButtonReadExcel3D()
        {
            Console.Write("RibbonButtonReadExcel");
            Logger.Info("_RibbonButtonReadExcelCommand : {0}", (object)DateTime.Now.ToString("s"));
            MainWindow.mainWindow.jobHelper.ReadExcel3D(MainWindow.mainWindow.txtKundenName.Text, MainWindow.mainWindow.txtAuftragsNummer.Text, MainWindow.mainWindow.xDatePicker.Text, MainWindow.mainWindow.txtArchivName.Text);
        }

        public ICommand WriteExcel3DCommand
        {
            get
            {
                if (this._RibbonButtonWriteExcelCommand == null)
                    this._RibbonButtonWriteExcelCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonWriteExcel3D()), (Predicate<object>)(x => true));
                return this._RibbonButtonWriteExcelCommand;
            }
        }

        public void ButtonWriteExcel3D()
        {
            Console.Write("ButtonWriteExcel3D");
            Logger.Info("_RibbonButtonWriteExcelCommand : {0}", (object)DateTime.Now.ToString("s"));

            MainWindow.mainWindow.jobHelper.WritePN3DExcel(MainWindow.mainWindow.txtKundenName.Text, MainWindow.mainWindow.txtAuftragsNummer.Text, MainWindow.mainWindow.xDatePicker.Text, MainWindow.mainWindow.txtArchivName.Text);
        }

        public ICommand Excel2SwiftCommand
        {
            get
            {
                if (this._Excel2SwiftCommand == null)
                    this._Excel2SwiftCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonExcel2Swift()), (Predicate<object>)(x => true));
                return this._Excel2SwiftCommand;
            }
        }

        public void ButtonExcel2Swift()
        {
            Console.WriteLine("ButtonExcel2Swift");
            Logger.Info("ButtonExcel2Swift : {0}", (object)DateTime.Now.ToString("s"));
            MainWindow.mainWindow.jobHelper.WriteExcel2Swift(MainWindow.mainWindow.txtKundenName.Text, MainWindow.mainWindow.txtAuftragsNummer.Text, MainWindow.mainWindow.xDatePicker.Text, MainWindow.mainWindow.txtArchivName.Text);

        }

        public ICommand AbgleichCommand
        {
            get
            {
                if (this._AbgleichCommand == null)
                    this._AbgleichCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonAbgleich()), (Predicate<object>)(x => true));
                return this._AbgleichCommand;
            }
        }

        public void ButtonAbgleich()
        {
            Console.WriteLine("AbgleichCommand");
            Logger.Info("AbgleichCommand : {0}", (object)DateTime.Now.ToString("s"));

            _SQLMaterialPool.CreateOrderedParts(MainWindow.mainWindow.jobHelper);
        }

        public ICommand ButtonSelectArchivCommand
        {
            get
            {
                if (this._ButtonSelectArchivCommand == null)
                    this._ButtonSelectArchivCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonSelectArchiv()), (Predicate<object>)(x => true));
                return this._ButtonSelectArchivCommand;
            }
        }

        public void ButtonSelectArchiv()
        {
            Console.WriteLine("ExcelICommand");
            Logger.Info("RibbonButtonOrdnerZuExel : {0}", (object)DateTime.Now.ToString("s"));
            this.RibbonButtonSelectArchiv();
        }

        public void RibbonButtonSelectArchiv()
        {
            Console.Write("ButtonStartAuftrag");
            ReadArchives();
            AuftragsDataControl._AuftragsDataControl.InitializeFileSystemObjects();
        }

        public ICommand ButtonReadMaterialCSVCommand
        {
            get
            {
                if (this._ButtonReadMaterialCSVCommand == null)
                    this._ButtonReadMaterialCSVCommand = (ICommand)new RelayCommand((Action<object>)(x => ReadMaterialCSV("database")));
                return this._ButtonReadMaterialCSVCommand;
            }
        }

        public ICommand ButtonPnArchivBrowserCommand
        {
            get
            {
                if (this._ButtonPnArchivBrowserCommand == null)
                    this._ButtonPnArchivBrowserCommand = (ICommand)new RelayCommand((Action<object>)(x => StartPNbrowser()));
                return this._ButtonPnArchivBrowserCommand;
            }
        }

        public void StartPNbrowser()
        {
            //  System.Diagnostics.Process.Start(System.IO.Path.Combine(pathToPNdrive, "u\\pn\\run\\PnArBrDb.exe"));
            if (File.Exists("P:\\MausiManager.zip"))
                File.Delete("P:\\MausiManager.zip");
            ZipFile.CreateFromDirectory("C:\\Users\\TBraig\\source\\repos\\sensemilli\\MausiManager\\bin\\Debug\\net6.0-windows", "P:\\MausiManager.zip");
        }

        public ICommand ButtonPnArchivBrowserSelectedCommand
        {
            get
            {
                if (this._ButtonPnArchivBrowserSelectedCommand == null)
                    this._ButtonPnArchivBrowserSelectedCommand = (ICommand)new RelayCommand((Action<object>)(x => StartPNbrowserSelected()));
                return this._ButtonPnArchivBrowserSelectedCommand;
            }
        }

        public void StartPNbrowserSelected()
        {
            string arnr;
            if (MainWindow.mainWindow.txtArchivNummer.Text == "")
                arnr = "0001";
            else
                arnr = MainWindow.mainWindow.txtArchivNummer.Text;
            System.Diagnostics.Process.Start(System.IO.Path.Combine(pathToPNdrive, "u\\pn\\run\\PnArBrDb.exe"), "/archive:" + arnr + "/multiselect:%select% /t:%action% /s:%filter%");
        }

     


        public ICommand ToggleAuftragCommand
        {
            get
            {
                if (this._RibbonButtonToggleAuftragCommand == null)
                    this._RibbonButtonToggleAuftragCommand = (ICommand)new RelayCommand((Action<object>)(x => this.ButtonToggleAuftrag()), (Predicate<object>)(x => true));
                return this._RibbonButtonToggleAuftragCommand;
            }
        }

        public void ButtonToggleAuftrag()
        {
            Console.Write("RibbonButtonToggleAuftrag");
            //  AuftragsDataControl._AuftragsDataControl.Edit(ViewAction.Edit);
            if (AuftragsDataControl._AuftragsDataControl.dockAuftragsListe.Visibility == Visibility.Hidden)
            {
                AuftragsDataControl._AuftragsDataControl.dockPartsOrderListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockMaterialListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockAuftragsListe.Visibility = Visibility.Visible;
                AuftragsDataControl._AuftragsDataControl.dockFilterMaterialListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterArchiveListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterAuftragsListe.Visibility = Visibility.Visible;
                _InternJobPool.OpenSQLOrderDataConnection(this);
            }
           
        }


        public ICommand ToggleMaterialCommand
        {
            get
            {
                if (this._ToggleMaterialCommand == null)
                    this._ToggleMaterialCommand = (ICommand)new RelayCommand((Action<object>)(x => ToggleMaterial()));
                return this._ToggleMaterialCommand;
            }
        }

        private void ToggleMaterial()
        {
            Console.Write("ToggleMaterial");
            //  AuftragsDataControl._AuftragsDataControl.Edit(ViewAction.Edit);
            if (AuftragsDataControl._AuftragsDataControl.dockMaterialListe.Visibility == Visibility.Hidden)
            {
                AuftragsDataControl._AuftragsDataControl.dockPartsOrderListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockMaterialListe.Visibility = Visibility.Visible;
                AuftragsDataControl._AuftragsDataControl.dockAuftragsListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterMaterialListe.Visibility = Visibility.Visible;
                AuftragsDataControl._AuftragsDataControl.dockFilterArchiveListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterAuftragsListe.Visibility = Visibility.Hidden;
                _SQLMaterialPool.OpenSQLMaterialDataConnection(this);
            }
          
        }

        public ICommand ToggleTeileCommand
        {
            get
            {
                if (this._ToggleTeileCommand == null)
                    this._ToggleTeileCommand = (ICommand)new RelayCommand((Action<object>)(x => ToggleTeile()));
                return this._ToggleTeileCommand;
            }
        }

        private void ToggleTeile()
        {
            Console.Write("ToggleTeileCommand");
            //  AuftragsDataControl._AuftragsDataControl.Edit(ViewAction.Edit);
            if (AuftragsDataControl._AuftragsDataControl.dockPartsOrderListe.Visibility == Visibility.Hidden)
            {
                AuftragsDataControl._AuftragsDataControl.dockPartsOrderListe.Visibility = Visibility.Visible;
                AuftragsDataControl._AuftragsDataControl.dockMaterialListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockAuftragsListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterMaterialListe.Visibility = Visibility.Hidden;
                AuftragsDataControl._AuftragsDataControl.dockFilterArchiveListe.Visibility = Visibility.Visible;
                AuftragsDataControl._AuftragsDataControl.dockFilterAuftragsListe.Visibility = Visibility.Hidden;
                _SQLMaterialPool.OpenSQLMaterialDataConnection(this);
            }
           
        }



        public void ReadArchives()
        {
            if (GridDataa != null)
            {
                GridDataa.Clear();
            }
            GridDataa = new ObservableCollection<GridRowData>();
            _gridDataa = GridDataa;
            int i = 0;
            int ii = 0;
            part1 = null;
            part2 = null;
            var lineCount = File.ReadAllLines("P:\\u\\ar\\abasis\\ABASDF").Length;
            IProgress<int> progress = new Progress<int>((Action<int>)(x => ++lineCount));
            // this.MaxArchivesAmount = lineCount;

            foreach (string line in File.ReadAllLines("P:\\u\\ar\\abasis\\ABASDF"))
            {
                string linee = line.TrimStart();
                string[] parts = linee.Split(' ');
                foreach (string part in parts)
                {
                    progress?.Report(1);
                    ii++;
                    if (i >= 1)
                    {
                        Console.WriteLine("{0}:{1}", i, part);
                        part1 = parts[0];
                        txtANummer = part1;
                        if (ii == 2)
                        {
                            part2 = parts[1];
                            txtAName = part2;
                        }
                        if (ii == 3)
                        {
                            part2 = parts[1] + " " + parts[2];
                            txtAName = parts[1] + " " + parts[2];
                        }
                        if (ii == 4)
                        {
                            part2 = parts[1] + " " + parts[2] + " " + parts[3];
                            txtAName = parts[1] + " " + parts[2] + " " + parts[3];
                        }
                    }
                }
                /*
                this.listView.Items.Add(new ListViewItem(new[] {
                part1,
              part2
              })); */
                //   xArchivListView.Items.Add(new ListViewItem(new[] {
                // part1,
                //   part2
                // }));

                if (part1 == null)
                    part1 = "0";
                if (part1 == "")
                    part1 = "0";
                if (part1 == "0")
                    part2 = "leer";
                if (part2 != null && int.Parse(part1) != 0)
                    GridDataa.Add(new GridRowData
                    {
                        IsChecked = false,
                        Text = part2,
                        PNEnumValue = PNEnumValues.Options,
                        IntValue = int.Parse(part1)
                    });

                ii = 0;
                _archivesAmount = i;
                i++; // For demonstration.


            }

            AuftragsDataControl.griddi.ItemsSource = GridDataa;
            _gridDataa = GridDataa;
        }

        public void ReadMaterialCSV(string action)
        {
          
            _SQLMaterialPool.ReadMaterialCSV(this , action);

            ////////TODO
        }
        public ICommand ButtonWriteToMaterialDBCommand
        {
            get
            {
                if (this._ButtonWriteToMaterialDBCommand == null)
                    this._ButtonWriteToMaterialDBCommand = (ICommand)new RelayCommand((Action<object>)(x => WriteToMaterialDB()));
                return this._ButtonWriteToMaterialDBCommand;
            }
        }
        private void WriteToMaterialDB()
        {
          //  int i = 0;
            foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     //where item.Text.Contains(TextFilterText.Text)
                                 select item)
            {
                //string[] aa = { item.BestellNummer, item.BestellDatum, item.Bemerk3, item.BestellPos, item.MaterialName, item.BestellDatum, item.Bezeichnung,
                //     item.AuftragsNummer, item.KundenName, item.PlIdentNr, Convert.ToString(item.MaxX), Convert.ToString(item.PlThick), Convert.ToString(item.MaxY), Convert.ToString(item.Amount), "N-"+ item.MaxX + "-" + item.MaxY,  Convert.ToString(item.PlThick) };
                //readablePhrase = string.Join(" ", aa);
                //Console.WriteLine(readablePhrase);
                _SQLMaterialPool.Connect(item);
            }
        }
        public ICommand ButtonCreateMaterialPrintExcelCommand
        {
            get
            {
                if (this._ButtonCreateMaterialPrintExcelCommand == null)
                    this._ButtonCreateMaterialPrintExcelCommand = (ICommand)new RelayCommand((Action<object>)(x => PrintEtiketten()));
                return this._ButtonCreateMaterialPrintExcelCommand;
            }
        }
        private void PrintEtiketten()
        {
           // int i = 0;
            
            foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     //where item.Text.Contains(TextFilterText.Text)
                                 select item)
            {
                string[] aa = { item.BestellNummer, item.BestellDatum, item.Bemerk3, item.BestellPos, item.MaterialName, item.BestellDatum, item.Bezeichnung,
                     item.AuftragsNummer, item.KundenName, item.PlIdentNr, Convert.ToString(item.MaxX), Convert.ToString(item.PlThick), Convert.ToString(item.MaxY), Convert.ToString(item.Amount), "N-"+ item.MaxX + "-" + item.MaxY,  Convert.ToString(item.PlThick) };
                readablePhrase = string.Join(";", aa);
                Console.WriteLine(readablePhrase);

                _SQLMaterialPool.EtikettenPrint(readablePhrase, item);
            }
            _SQLMaterialPool.CreateMatPrintExcel();
        }

 




       

 
        public ObservableCollection<GridRowData> GridDataa
        {
            get { return _gridDataa; }
            set
            {
                _gridDataa = value;
                Console.WriteLine(value + " grid ");
                OnPropertyChanged("GridDataa");
            }
        }



        public GridRowData GridSelectedItemProperty
        {
            get { return _gridSelectedItemProperty; }
            set
            {
                _gridSelectedItemProperty = value;
                if (_gridSelectedItemProperty != null)
                {
                    MainWindow.mainWindow.txtArchivNummer.Text = _gridSelectedItemProperty.IntValue.ToString();
                    MainWindow.mainWindow.txtArchivName.Text = _gridSelectedItemProperty.Text;
                    OnPropertyChanged("GridSelectedItemProperty");
                    Console.WriteLine(value.Text + "     " + value.IntValue + "     " + value.PNEnumValue + "gridval   " + value + "  AdataModel");
                }
            }
        }

        public ObservableCollection<PartOrderData> PartOrderData
        {
            get { return _PartOrderList; }
            set
            {
                _PartOrderList = value;
                Console.WriteLine(value + " grid ");
                OnPropertyChanged("PartsOrderData");
            }
        }

        public PartOrderData PartOrderGridSelectedItemProperty
        {
            get { return _PartOrderGridSelectedItemProperty; }
            set
            {
                _PartOrderGridSelectedItemProperty = value;
                if (_PartOrderGridSelectedItemProperty != null)
                {
                    //MainWindow.mainWindow.txtArchivNummer.Text = _gridSelectedItemProperty.IntValue.ToString();
                   // MainWindow.mainWindow.txtArchivName.Text = _gridSelectedItemProperty.Text;
                    OnPropertyChanged("PartOrderGridSelectedItemProperty");
                    Console.WriteLine(value.OrderPos + "    " + value.PartName + "    " + value.Anzahl  );
                    Console.WriteLine(AuftragsDataControl._AuftragsDataControl.GridOrderParts.SelectedIndex);
                   
                }
            }
        }
        
            public ObservableCollection<OrdersData> OrdersData
        {
            get { return _OrdersData; }
            set
            {
                _OrdersData = value;
                Console.WriteLine(value + " grid ");
                OnPropertyChanged("OrdersData");
            }
        }
        public OrdersData OrdersGridSelectedItemProperty
        {
            get { return _OrdersGridSelectedItemProperty; }
            set
            {
                _OrdersGridSelectedItemProperty = value;
                if (_OrdersGridSelectedItemProperty != null)
                {
                    //MainWindow.mainWindow.txtArchivNummer.Text = _gridSelectedItemProperty.IntValue.ToString();
                    // MainWindow.mainWindow.txtArchivName.Text = _gridSelectedItemProperty.Text;
                    OnPropertyChanged("_OrdersGridSelectedItemProperty");
                    Console.WriteLine(value.auftragsnummer + "     " + value.kundenname + "      " + value.bemerkungen + "      " + value.bearbeiter1);
                    Console.WriteLine(AuftragsDataControl._AuftragsDataControl.GridOrders.SelectedIndex);

                    int count = value.auftragsnummer.Count();
                    string orderYear = value.auftragsnummer.Remove(2, count - 2);
                    string pathTOcadzeich = "S:\\cadzeich\\20" + orderYear + "\\" + value.auftragsnummer.Remove(6, count - 6) + "\\";
                    string pathTOkunden = "X:\\Kunden\\20" + orderYear + "\\" + value.auftragsnummer.Remove(6, count - 6) + "\\";
                    AuftragsDataControl._AuftragsDataControl.PreSelect(pathTOcadzeich);
                    AuftragsDataControl._AuftragsDataControl.PreSelect(pathTOkunden);
                    MainWindow.mainWindow.txtAuftragsNummer.Text = value.auftragsnummer;
                    MainWindow.mainWindow.txtKundenName.Text = value.kundenname;
                    Console.WriteLine(pathTOcadzeich + "    " + pathTOkunden);
                    _InternJobPool.ReadPartsFromOrder(this, value.auftragsnummer);
                }
            }
        }


        public ObservableCollection<FolderFilesList> FolderFilesList
        {
            get { return _FolderFilesList; }
            set
            {
                _FolderFilesList = value;
                Console.WriteLine(value + " grid ");
                OnPropertyChanged("FolderFilesList");
            }
        }

        public FolderFilesList FolderFilesSelectedItemProperty
        {
            get { return _FolderFilesListSelectedItemProperty; }
            set
            {
                _FolderFilesListSelectedItemProperty = value;
                string sPath ="";
                string sFileName = "";
                if (_FolderFilesListSelectedItemProperty != null)
                {
                    
                    //MainWindow.mainWindow.txtArchivNummer.Text = _gridSelectedItemProperty.IntValue.ToString();
                    // MainWindow.mainWindow.txtArchivName.Text = _gridSelectedItemProperty.Text;
                    try
                    {
                        if (AuftragsDataControl._AuftragsDataControl.xDXFprevhost.Visibility == Visibility.Hidden)
                            AuftragsDataControl._AuftragsDataControl.xDXFprevhost.Visibility = Visibility.Visible;
                        sPath = AuftragsDataControl.selectedItem.FileSystemInfo.FullName;
                        sFileName = value.Name;
                        if (sFileName.Contains(".dxf"))
                        {
                            AuftragsDataControl.formsPreview.Open(sPath + "\\" + sFileName);
                        }
                        if (sFileName.Contains(".DXF"))
                        {
                            AuftragsDataControl.formsPreview.Open(sPath + "\\" + sFileName);
                        }

                        if (sFileName.Contains(".xlsx"))
                        {
                            AuftragsDataControl.formsPreview.Open(sPath + "\\" + sFileName);
                        }
                        if (sFileName.Contains(".XLSX"))
                        {
                            AuftragsDataControl.formsPreview.Open(sPath + "\\" + sFileName);
                        }
                        //dxfListStatusText.Text = iFiles.ToString() + " File(s)  " + iDXFfiles.ToString() + "  DXF-files      Selected - " + sFileName + " - Selected";
                        //Process.Start(sPath + "\\" + sFileName);
                    }
                    catch (Exception Exc)
                    {
                        System.Windows.MessageBox.Show(Exc.ToString());
                    }
                    AuftragsDataControl._AuftragsDataControl.dxfListStatusText.Text = "Selected:    " + sPath + "\\" + sFileName;
                    OnPropertyChanged("PartOrderGridSelectedItemProperty");
                    Console.WriteLine(value.Name + " Item " + value.Extension + "  Date  " + value.CreationDate);
                    Console.WriteLine(AuftragsDataControl._AuftragsDataControl.xFilesList.SelectedIndex);
                }
            }
        }

        public GridLength GridPlateHeight
        {
            get
            {
                return this._gridPlateHeight;
            }
            set
            {
                this._gridPlateHeight = value;
                base.NotifyPropertyChanged("GridPlateHeight");
            }
        }

        public DataGrid GridOrders { get;  set; }
        public DataGrid PartOrders { get; set; }

        public DataGrid GridMaterialPool { get; set; }

        public ObservableCollection<PlateData> PlateData
        {
            get { return _PlateData; }
            set
            {
                _PlateData = value;
                Console.WriteLine(value + " PlateData ");
                OnPropertyChanged("PlateData");
            }
        }

        private GridLength CreateHeight(int value) => value <= 0 ? new GridLength(1.0, GridUnitType.Star) : new GridLength((double)value, GridUnitType.Pixel);





        public void PreviewChanged(string path = "")
        {
            foreach (MachineViewInfo machine in this._settings.Machines)
            {
                MachineViewInfo item = machine;
                // this.CalculateJobsInMachine(item, this._originalJobs.FindAll((Predicate<JobInfo>)(x => x.JOB_MACHINE_NO == item.Number)));
            }
        }

        public void SaveSettings()
        {
            this._settings.GridArchiveHeight = (int)this.GridArchiveHeight.Value;
           this._settings.GridFoldersHeight = (int)this.GridFoldersHeight.Value;
            this._settings.GridFilesHeight = (int)this.GridFilesHeight.Value;
        }

        internal void Edit(ViewAction edit, ObservableCollection<OrdersData> data, OrdersData row)
        {
            AddAuftragControl editControl = new AddAuftragControl()
            {
                DataContext = new AddAuftragControlViewModel(data, row, this.SelectedAuftrag, edit, () => this.CloseAction())
            };
            //this._grid.IsEnabled = false;
            this.ActiveView = editControl;
            //MainWindow.Instance.xCControl = editControl;
            //	MainWindow.Instance.dockEdit = new DockPanel();
            AuftragsDataControl._AuftragsDataControl.dockEdit.Children.Clear();
            AuftragsDataControl._AuftragsDataControl.dockEdit.Children.Add(editControl);

            AuftragsDataControl._AuftragsDataControl.dockEdit.Visibility = Visibility.Visible;
        }

        public bool HasToIgnoreEnter { get; set; }

        public double FontSize
        {
            get => this._fontSize;
            set
            {
                this._fontSize = value;
                this.NotifyPropertyChanged(nameof(FontSize));
            }
        }

        public int ArchivesAmount
        {
            get => this._ArchivesAmount;
            set
            {
                Console.WriteLine("Archamount" + this._ArchivesAmount);
                this._ArchivesAmount = value;
                this.NotifyPropertyChanged(nameof(ArchivesAmount));
            }
        }

        public string ArchivesPath
        {
            get => this._ArchivesPath;
            set
            {
                this._ArchivesPath = value;
                this.NotifyPropertyChanged(nameof(ArchivesPath));
            }
        }

        public GridLength Column1Width
        {
            get
            {
                return this._column1Width;
            }
            set
            {
                this._column1Width = value;
                base.NotifyPropertyChanged("Column1Width");
            }
        }

        public GridLength GridArchiveHeight
        {
            get => this._GridArchiveHeight;
            set
            {
                this._GridArchiveHeight = value;
                this.NotifyPropertyChanged(nameof(GridArchiveHeight));
            }
        }

        public GridLength GridFoldersHeight
        {
            get => this._gridFoldersHeight;
            set
            {
                this._gridFoldersHeight = value;
                this.NotifyPropertyChanged(nameof(GridFoldersHeight));
            }
        }

        public GridLength GridFilesHeight
        {
            get => this._gridFilesHeight;
            set
            {
                this._gridFilesHeight = value;
                this.NotifyPropertyChanged(nameof(GridFilesHeight));
            }
        }

        public int MaxJobsAmount
        {
            get => this._maxJobsAmount;
            set
            {
                this._maxJobsAmount = value;
                this.NotifyPropertyChanged(nameof(MaxJobsAmount));
            }
        }

        public int ReadJobsAmount
        {
            get => this._readJobsAmount;
            set
            {
                this._readJobsAmount = value;
                this.NotifyPropertyChanged(nameof(ReadJobsAmount));
            }
        }

        public Visibility ProgressVisibility
        {
            get => this._progressVisibility;
            set
            {
                this._progressVisibility = value;
                this.NotifyPropertyChanged(nameof(ProgressVisibility));
            }
        }

        public int JobsFiltered
        {
            get => this._jobsFiltered;
            set
            {
                this._jobsFiltered = value;
                this.NotifyPropertyChanged(nameof(JobsFiltered));
            }
        }
        public int ReadArchivesAmount
        {
            get => this._ReadArchivesAmount;
            set
            {
                this._ReadArchivesAmount = value;
                this.NotifyPropertyChanged(nameof(ReadArchivesAmount));
            }
        }
        public int MaxArchivesAmount
        {
            get => this._MaxArchivesAmount;
            set
            {
                this._MaxArchivesAmount = value;
                this.NotifyPropertyChanged(nameof(MaxArchivesAmount));
            }
        }

        public string TextFilter
        {
            get => this._TextFilter;
            set
            {
                this._TextFilter = value;
                this.NotifyPropertyChanged(nameof(TextFilter));
            }
        }
        public string TextFilterFiles
        {
            get => this._TextFilterFiles;
            set
            {
                this._TextFilterFiles = value;
                this.NotifyPropertyChanged(nameof(TextFilterFiles));
            }
        }
        public string MaterialTextFilter
        {
            get => this._MaterialTextFilter;
            set
            {
                this._MaterialTextFilter = value;
                this.NotifyPropertyChanged(nameof(MaterialTextFilter));
            }
        }
        public string DickeTextFilter
        {
            get => this._DickeTextFilter;
            set
            {
                this._DickeTextFilter = value;
                this.NotifyPropertyChanged(nameof(DickeTextFilter));
            }
        }
        public string MatNrTextFilter
        {
            get => this._MatNrTextFilter;
            set
            {
                this._MatNrTextFilter = value;
                this.NotifyPropertyChanged(nameof(MatNrTextFilter));
            }
        }
         public string AuftragFilterText
        {
            get => this._AuftragFilterText;
            set
            {
                this._AuftragFilterText = value;
                this.NotifyPropertyChanged(nameof(AuftragFilterText));
            }
        }
        public FrameworkElement ActiveView
        {
            get
            {
                return this._activeView;
            }
            set
            {
                this._activeView = value;
                base.NotifyPropertyChanged("ActiveView");
            }
        }
    }

    public class T 
    {
        public T()
        {
            new WiCAM.Pn4000.JobManager.Classes.TypeInitializer().Initialize(this);
        }
    }
}
