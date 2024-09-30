using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using WiCAM.Pn4000.Common;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Data;
using System.Collections;
using WiCAM.Pn4000.JobManager.ShellClasses;
using System.Windows.Input;
using Cursors = System.Windows.Input.Cursors;
using WiCAM.Pn4000.TechnoTable;
using System.Windows.Media;
using WiCAM.Pn4000.Gmpool;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media.Animation;
using System.Data.SqlTypes;
using WiCAM.Pn4000.JobManager.AuftragsHelfer;

namespace WiCAM.Pn4000.JobManager
{
    public partial class AuftragsDataControl : UserControl, IView, IComponentConnector
    {
        private int iDirectories;
        private int iFiles;
        private int iDXFfiles;
        private TreeNode startTree;
        public static PreviewHandlerHost formsPreview;
        public static System.Windows.Controls.DataGrid griddi;
        public static System.Windows.Controls.DataGrid partOrderGrid;
        private bool consolevisible;
        private ObservableCollection<GridRowData> _filtered;
        private ObservableCollection<FolderFilesList> _filteredFiles;

        public event PropertyChangedEventHandler PropertyChanged;

        private string pathToPNdrive;
        public static AuftragsDataControl _AuftragsDataControl;
        ObservableCollection<FolderFilesList> _FolderFilesList;

        public static FileSystemObjectInfo selectedItem;
        private List<TimeScale> timeList;
        private string connectionString;
        private string sqlQuery;
        private SqlCommand sqlcmd;
        private SqlDataAdapter sqlda;
        JobHelper jobHelper;
        private SQLMaterialPool _SQLMaterialPool;
        private ObservableCollection<PlateData> _FilteredMaterialList;
        private ObservableCollection<OrdersData> _FilteredOrdersList;

        public AuftragsDataControl()
        {
            this.InitializeComponent();
            pathToPNdrive = PnPathBuilder.PnDrive;
             jobHelper = new JobHelper();
            _SQLMaterialPool = new SQLMaterialPool();
            _AuftragsDataControl = this;
            griddi = Griddi;
            partOrderGrid = GridOrderParts;
            consolevisible = false;

            formsPreview = new PreviewHandlerHost();
            formsPreview.InitializeComponent();
            //  previewHandlerHost = new PreviewHandlerHost();
            //  autoSettingsControl = new AutoSettingsControl();
            this.prevHost.Child = formsPreview;
            formsPreview.Open("X:\\Kunden\\2022\\220339\\iBend\\LFA\\220339LFA1006Z0-001_REV00.dxf");
            FilteredList = new ObservableCollection<GridRowData>();
            FilteredFilesList = new ObservableCollection<FolderFilesList>();
            _FilteredMaterialList = new ObservableCollection<PlateData>();
            _FilteredOrdersList = new ObservableCollection<OrdersData>();


            InitializeFileSystemObjects();
            //     transcont.OnApplyTemplate();
            timeList = new List<TimeScale>
            {
                new TimeScale{Month="Januar", Time7="07,20,30"},
                new TimeScale{Month="Juli", Time23="10,20,30"}
            };

            dgTimeline.StartTime = 01;
            dgTimeline.EndTime = 31;

            dgTimeline.Loaded += new RoutedEventHandler(dgTimeline_Loaded);

            dgTimeline.ItemsSource = timeList;


        }
        private void OnClickOutput(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("OnClickOutput");
            MainWindow.mainWindow.SetFlyoutState();
            if (consolevisible == false)
            {
                ConsoleExtension.Show();
                consolevisible = true;
                return;
            }
            if (consolevisible == true)
            {
                ConsoleExtension.Hide();
                consolevisible = false;
            }
        }

        private void OnClickSwitchPrev(object sender, RoutedEventArgs e)
        {
            if (xPartPrevDock.Visibility == Visibility.Visible)
            {
                xDXFprevhost.Visibility = Visibility.Visible;
                xPartPrevDock.Visibility = Visibility.Hidden;
            }
            else
            {
                xDXFprevhost.Visibility = Visibility.Hidden;
                xPartPrevDock.Visibility = Visibility.Visible;
            }

        }

        private void CloseAction()
        {

        }
        #region Methods

        private void InitializeFileSystemObjects()
        {
            var drives = DriveInfo.GetDrives();
            DriveInfo
                .GetDrives()
                .ToList()
                .ForEach(drive =>
                {
                    var fileSystemObject = new FileSystemObjectInfo(drive);
                    fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
                    fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
                    treeView.Items.Add(fileSystemObject);

                });
            PreSelect("X:\\Kunden\\");//Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            PreSelect("S:\\cadzeich\\");
        }

        public void PreSelect(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("error path");
                return;
            }
            Console.WriteLine(path);
            var driveFileSystemObjectInfo = GetDriveFileSystemObjectInfo(path);
            driveFileSystemObjectInfo.IsExpanded = true;
            PreSelect(driveFileSystemObjectInfo, path);
        }

        private void PreSelect(FileSystemObjectInfo fileSystemObjectInfo, string path)
        {
            foreach (var childFileSystemObjectInfo in fileSystemObjectInfo.Children)
            {
                var isParentPath = IsParentPath(path, childFileSystemObjectInfo.FileSystemInfo.FullName);
                if (isParentPath)
                {
                    if (string.Equals(childFileSystemObjectInfo.FileSystemInfo.FullName, path))
                    {

                        /* We found the item for pre-selection */
                    }
                    else
                    {
                        childFileSystemObjectInfo.IsExpanded = true;
                        PreSelect(childFileSystemObjectInfo, path);
                    }
                }
            }
        }

        #endregion

        #region Helpers

        public FileSystemObjectInfo GetDriveFileSystemObjectInfo(string path)
        {
            var directory = new DirectoryInfo(path);
            var drive = DriveInfo
                .GetDrives()
                .Where(d => d.RootDirectory.FullName == directory.Root.FullName)
                .FirstOrDefault();
            return GetDriveFileSystemObjectInfo(drive);
        }

        private FileSystemObjectInfo GetDriveFileSystemObjectInfo(DriveInfo drive)
        {
            foreach (var fso in treeView.Items.OfType<FileSystemObjectInfo>())
            {
                if (fso.FileSystemInfo.FullName == drive.RootDirectory.FullName)
                {
                    return fso;
                }
            }
            return null;
        }

        private bool IsParentPath(string path,
            string targetPath)
        {
            return path.StartsWith(targetPath);
        }

        #endregion

        private void FileSystemObject_AfterExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Arrow;

        }

        private void FileSystemObject_BeforeExplore(object sender, System.EventArgs e)
        {
            Cursor = Cursors.Wait;
        }



        private void FilteredTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + griddi + "  " + AuftragsDataViewModel.Instance._gridDataa);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            ObservableCollection<GridRowData> TempFiltered;

            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _filtered.Clear();
            foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._gridDataa
                                 where item.Text.Contains(TextFilterText.Text)
                                 select item)
            {
                _filtered.Add(item);
            }
            this.Griddi.ItemsSource = _filtered;
        }

        public ObservableCollection<GridRowData> FilteredList
        {
            get
            {
                return _filtered;
            }
            set
            {
                if (_filtered == value)
                    return;

                _filtered = value;
            }
        }

        public ObservableCollection<FolderFilesList> FilteredFilesList
        {
            get
            {
                return _filteredFiles;
            }
            set
            {
                if (_filteredFiles == value)
                    return;

                _filteredFiles = value;
            }
        }

        public ObservableCollection<PlateData> FilteredMaterialList
        {
            get
            {
                return _FilteredMaterialList;
            }
            set
            {
                if (_FilteredMaterialList == value)
                    return;

                _FilteredMaterialList = value;
            }
        }
        public ObservableCollection<OrdersData> FilteredOrdersList
        {
            get
            {
                return _FilteredOrdersList;
            }
            set
            {
                if (_FilteredOrdersList == value)
                    return;

                _FilteredOrdersList = value;
            }
        }


        /*

        public void LoadDXFlist()
        {
            string[] aDrives = Environment.GetLogicalDrives();

            dxfFolderTree.BeginUpdate();

            foreach (string strDrive in aDrives)
            {
                TreeNode dnMyDrives = new TreeNode(strDrive.Remove(2, 1));
                Console.WriteLine(strDrive);
                switch (strDrive)
                {
                    case "A:\\":
                        dnMyDrives.SelectedImageIndex = 0;
                        dnMyDrives.ImageIndex = 0;
                        break;
                    case "C:\\":

                        // The next statement causes the treeView1_AfterSelect Event to fire once on startup.
                        // This effect can be seen just after intial program load. C:\ node is selected
                        // Automatically on program load, expanding the C:\ treeView1 node.
                        dxfFolderTree.SelectedNode = dnMyDrives;
                        startTree = dnMyDrives;
                        dnMyDrives.SelectedImageIndex = 1;
                        dnMyDrives.ImageIndex = 1;

                        break;
                    case "D:\\":
                        dnMyDrives.SelectedImageIndex = 2;
                        dnMyDrives.ImageIndex = 2;
                        break;
                    case "X:\\":
                        dxfFolderTree.SelectedNode = dnMyDrives;
                        startTree = dnMyDrives;
                        dnMyDrives.SelectedImageIndex = 3;
                        dnMyDrives.ImageIndex = 3;
                        break;
                    case "S:\\":
                        dnMyDrives.SelectedImageIndex = 4;
                        dnMyDrives.ImageIndex = 4;
                        break;
                    default:
                        dnMyDrives.SelectedImageIndex = 5;
                        dnMyDrives.ImageIndex = 5;
                        break;
                }

                dxfFolderTree.Nodes.Add(dnMyDrives);
            }
            dxfFolderTree.EndUpdate();

        }
        */
        /*
        private void dxfListView_ItemActivate(object sender, EventArgs e)
        {
            try
            {
                string sPath = dxfFolderTree.SelectedNode.FullPath;
                string sFileName = dxfListView.FocusedItem.Text;
                if (sFileName.Contains(".dxf"))
                {
                    formsPreview.Open(sPath + "\\" + sFileName);
                }
                if (sFileName.Contains(".DXF"))
                {
                    formsPreview.Open(sPath + "\\" + sFileName);
                }

                if (sFileName.Contains(".xlsx"))
                {
                    formsPreview.Open(sPath + "\\" + sFileName);
                }
                if (sFileName.Contains(".XLSX"))
                {
                    formsPreview.Open(sPath + "\\" + sFileName);
                }
                dxfListStatusText.Text = iFiles.ToString() + " File(s)  " + iDXFfiles.ToString() + "  DXF-files      Selected - " + sFileName + " - Selected";
                //Process.Start(sPath + "\\" + sFileName);
            }
            catch (Exception Exc)
            {
                System.Windows.MessageBox.Show(Exc.ToString());
            }
        }
        */
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Console.WriteLine("rowdoubleclickAdata");
            //Edit(ViewAction.Create);
            //PartOrderData selectedPartOrder = (PartOrderData)GridOrderParts.SelectedItem;
            //Console.WriteLine(selectedPartOrder.PartName);
            ////// xPartGrid.DefaultCellStyle.BackColor = Color.Maroon;
            //PartOrderData row = (PartOrderData)GridOrderParts.SelectedItems[0];

            //ObservableCollection<PartOrderData> data = (ObservableCollection<PartOrderData>)GridOrderParts.ItemsSource;
            //data.Remove(row);

            //  DataRowView dr = selectedPartOrder;
            //  DataRow dr1 = dr.Row;
            //GridOrderParts.CurrentRow.Cells[columnIndex].Style.BackColor = Color.Red;
            //sender.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Beige;
            //PartCell.Foreground = System.Windows.Media.Brushes.Violet;
            //     AuftragsDataControl._AuftragsDataControl.GridOrderParts.Items.RemoveAt(AuftragsDataControl._AuftragsDataControl.GridOrderParts.SelectedIndex);

            //      GridOrderParts.RowBackground = System.Windows.Media.Brush;

        }


        /*
        private void dxfFolderTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            // Get subdirectories from disk, add to treeView1 control
            //    Console.WriteLine("folder click");
            //      formsPreview.UnloadPreviewHandler();

            AddDirectories(e.Node);

            // if node is collapsed, expand it. This allows single click to open folders.
            dxfFolderTree.SelectedNode.Expand();

            // Get files from disk, add to listView1 control
            AddFiles(e.Node.FullPath.ToString());
            dxfTreeStatusText.Text = iDirectories.ToString() + " Folder(s)" + "  ---  " + e.Node.FullPath.ToString();
            _SelectedFilesFolder = e.Node.FullPath.ToString();
            dxfListStatusText.Text = iFiles.ToString() + " File(s)" + "----" + iDXFfiles.ToString() + "  DXF-files";
            char[] charsToTrim1 = {'T', 'r', 'e', 'e', 'N',
                               'o', 'd', 'e', ':', ' '};
            MainWindow.mainWindow.txtAuftragsNummer.Text = e.Node.ToString().Trim(charsToTrim1);
        }

        private void AddDirectories(System.Windows.Forms.TreeNode tnSubNode)
        {
            // This method is used to get directories (from disks, or from other directories)
            Console.WriteLine(tnSubNode);

            dxfFolderTree.BeginUpdate();
            iDirectories = 0;

            try
            {
                DirectoryInfo diRoot;

                // If drive, get directories from drives
                if (tnSubNode.SelectedImageIndex < 11)
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath + "\\");
                }

                //  Else, get directories from directories
                else
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath);
                }
                DirectoryInfo[] dirs = diRoot.GetDirectories();

                // Must clear this first, else the directories will get duplicated in treeview
                tnSubNode.Nodes.Clear();

                // Add the sub directories to the treeView1
                foreach (DirectoryInfo dir in dirs)
                {
                    iDirectories++;
                    System.Windows.Forms.TreeNode subNode = new System.Windows.Forms.TreeNode(dir.Name);
                    subNode.ImageIndex = 11;
                    subNode.SelectedImageIndex = 12;
                    tnSubNode.Nodes.Add(subNode);
                }

            }
            // Throw Exception when accessing directory: C:\System Volume Information	 // do nothing
            catch {; }

            dxfFolderTree.EndUpdate();
        }
        */
        private void AddFiles(string strPath)
        {
            _FolderFilesList = new ObservableCollection<FolderFilesList>();
            Console.WriteLine(strPath);

            iFiles = 0;
            iDXFfiles = 0;
            ImageSource fileimg = null;

            try
            {
                DirectoryInfo di = new DirectoryInfo(strPath + "\\");
                FileInfo[] theFiles = di.GetFiles();
                foreach (FileInfo theFile in theFiles)
                {
                    iFiles++;
                    if (theFile.Extension.Contains("dxf"))
                        iDXFfiles++;
                    if (theFile.Extension.Contains("DXF"))
                        iDXFfiles++;
                    fileimg = FileManager.GetImageSource(theFile.FullName);

                    _FolderFilesList.Add(new FolderFilesList
                    {
                        FileIcon = fileimg,
                        Name = theFile.Name,
                        Extension = theFile.Extension,
                        Size = theFile.Length.ToString(),
                        CreationDate = theFile.LastWriteTime.ToShortDateString()
                    });

                }
            }
            catch (Exception Exc) { dxfListStatusText.Text = Exc.ToString(); }

            xFilesList.ItemsSource = _FolderFilesList;

        }



        [SpecialName]
        object IView.DataContext()
        {
            return this.DataContext;
        }

        [SpecialName]
        void IView.DataContext(object value)
        {
            Console.WriteLine(value);
            this.DataContext = value;
        }

        private void dxfListView_Resize(object sender, EventArgs e)
        {
            // resizeControl(buttonOriginalRectangle, dxfListView);

        }

        private void dxfFolderTree_SizeChanged(object sender, EventArgs e)
        {

        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            System.Windows.Controls.TreeView treeView = sender as System.Windows.Controls.TreeView;
            selectedItem = treeView.SelectedItem as FileSystemObjectInfo;
            int count = selectedItem.FileSystemInfo.FullName.ToString().Count();
          //  MainWindow.mainWindow.txtAuftragsNummer.Text = selectedItem.FileSystemInfo.FullName.ToString().Remove(0, count - 6);
            Console.WriteLine(selectedItem.FileSystemInfo.FullName);
            AddFiles(selectedItem.FileSystemInfo.FullName.ToString());

        }

        private void FilteredFilesTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + xFilesList);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */

            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _filteredFiles.Clear();
            foreach (var item in from item in _FolderFilesList
                                 where item.Name.Contains(TextFilterFilesText.Text)
                                 select item)
            {
                _filteredFiles.Add(item);
            }
            this.xFilesList.ItemsSource = _filteredFiles;
        }

        private void RowFiles_DoubleClick(object sender, MouseButtonEventArgs e)
        {

            FolderFilesList selectedFile = (FolderFilesList)xFilesList.SelectedItem;
            Console.WriteLine(selectedFile.Name);
            //// xPartGrid.DefaultCellStyle.BackColor = Color.Maroon;
            FolderFilesList row = (FolderFilesList)xFilesList.SelectedItems[0];

            ObservableCollection<FolderFilesList> data = (ObservableCollection<FolderFilesList>)xFilesList.ItemsSource;
            //data.Remove(row);
            //   AuftragsDataViewModel._auftragsDataViewModel.Edit(ViewAction.Edit, data, row);
        }

        #region Datagrid Loaded
        void dgTimeline_Loaded(object sender, RoutedEventArgs e)
        {
            LoadColumns(dgTimeline.StartTime, dgTimeline.EndTime);
        }
        #endregion

        #region LoadColumns(int,int)
        private void LoadColumns(int startTime, int endTime)
        {
            if (dgTimeline.Columns.Count != 0)
            {
                dgTimeline.Columns.Clear();
            }
            int sTime = startTime;
            int eTime = endTime;

            DataGridTextColumn dgTextCol = new DataGridTextColumn();
            dgTextCol.Header = "Month / Days";
            dgTextCol.Width = 150;
            dgTextCol.Binding = new System.Windows.Data.Binding("Month");
            dgTimeline.Columns.Add(dgTextCol);

            if (sTime < 31 && sTime > 00)
            {
                for (int i = sTime; i < 32; i++)
                {
                    string header = (i + 1).ToString() + " ";
                    DataGridTemplateColumn dgTempCol = new DataGridTemplateColumn();
                    dgTempCol.Header = header;
                    dgTempCol.Width = 120;

                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TimelineControl));
                    System.Windows.Data.Binding bTemp = new System.Windows.Data.Binding("Time" + i.ToString());
                    bTemp.Mode = BindingMode.TwoWay;
                    fef.SetValue(TimelineControl.DaysProperty, bTemp);
                    DataTemplate dt = new DataTemplate();
                    dt.VisualTree = fef;
                    dgTempCol.CellTemplate = dt;

                    dgTimeline.Columns.Add(dgTempCol);
                }
                sTime = 31;
            }

            if (eTime > sTime)
            {
                for (int i = 0; i <= eTime; i++)
                {
                    string header = string.Empty;
                    if (i == 0)
                    {
                        header = (i).ToString() + " ";
                    }
                    else
                    {
                        header = (i).ToString() + " ";
                    }
                    DataGridTemplateColumn dgTempCol = new DataGridTemplateColumn();
                    dgTempCol.Header = header;
                    dgTempCol.Width = 120;

                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(TimelineControl));
                    System.Windows.Data.Binding bTemp = new System.Windows.Data.Binding("Time" + i.ToString());
                    bTemp.Mode = BindingMode.TwoWay;
                    fef.SetValue(TimelineControl.DaysProperty, bTemp);
                    DataTemplate dt = new DataTemplate();
                    dt.VisualTree = fef;
                    dgTempCol.CellTemplate = dt;

                    dgTimeline.Columns.Add(dgTempCol);
                }
            }
        }
        #endregion

        private void OrdersRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OrdersData selectedOrder = (OrdersData)GridOrders.SelectedItem;
            Console.WriteLine(selectedOrder.auftragsnummer);
            //// xPartGrid.DefaultCellStyle.BackColor = Color.Maroon;
            OrdersData row = (OrdersData)GridOrders.SelectedItems[0];

            ObservableCollection<OrdersData> data = (ObservableCollection<OrdersData>)GridOrders.ItemsSource;
            //data.Remove(row);
            AuftragsDataViewModel._auftragsDataViewModel.Edit(ViewAction.Edit, data, row);
        }



     
        private void Image_JobDeleteMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
                OrdersData selectedOrder = (OrdersData)GridOrders.SelectedItem;
                Console.WriteLine(selectedOrder.auftragsnummer);
                //// xPartGrid.DefaultCellStyle.BackColor = Color.Maroon;
                OrdersData row = (OrdersData)GridOrders.SelectedItems[0];

                ObservableCollection<OrdersData> data = (ObservableCollection<OrdersData>)GridOrders.ItemsSource;

                if (PnPathBuilder.ArDrive == "P:")
                    connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                if (PnPathBuilder.ArDrive == "H:")
                    connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                if (PnPathBuilder.ArDrive == "C:")
                    connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            data.Remove(row);
            deleteRow("dbo.w_auftragspool", row.mpid);
               

        }
        
        public void deleteRow(string table, int? IDNumber)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        using (SqlCommand command = new SqlCommand("DELETE FROM " + table + " WHERE mpid = " + IDNumber, con))
                        {
                            command.ExecuteNonQuery();
                        }
                        con.Close();
                    }
                }
                catch (SystemException ex)
                {
                    Console.WriteLine(string.Format("An error occurred: {0}", ex.Message));
                }
            }

        private void Image_OpenFolderMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            WindowsExplorerOpen(selectedItem.FileSystemInfo.FullName.ToString());
        }
        public  void WindowsExplorerOpen(string path)
        {
            CommandLine(path, $"start {path}");
        }

        private  void CommandLine(string workingDirectory, string Command)
        {
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/K " + Command + " && exit");
            ProcessInfo.WorkingDirectory = workingDirectory;
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = true;
            ProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process = Process.Start(ProcessInfo);
            Process.WaitForExit();
        }


        private void MenuItem2DTeileListeLaden_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
         
                dlg.InitialDirectory = Path.GetDirectoryName(path: selectedItem.FileSystemInfo.FullName + "\\");
            dlg.FileName = "Teileliste"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Dateien (.xlsm,.xlsx)|*.xlsm;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
               
                jobHelper.ReadExcel2DResult(dlg);
            }
            }

        private void MenuItem3DTeileListeLaden_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = Path.GetDirectoryName(path: selectedItem.FileSystemInfo.FullName + "\\");
            dlg.FileName = "Teileliste"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "Excel Dateien (.xlsm,.xlsx)|*.xlsm;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                jobHelper.ReadExel3DResult(dlg);
            }
        }

        private void MaterialPoolRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(string.Format("MaterialPoolRow_DoubleClick: {0}"));

        }

        private void MaterialFilteredTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + GridMaterialPool);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _FilteredMaterialList.Clear();
            if (DickeFilterText.Text == "")
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MaterialName.Contains(MaterialFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            else
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MaterialName.Contains(MaterialFilterText.Text) && item.PlThick == double.Parse(DickeFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            this.GridMaterialPool.ItemsSource = _FilteredMaterialList;
        }

        private void AuftragFilteredTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + GridOrders);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _FilteredOrdersList.Clear();
            foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._OrdersData
                                 where item.auftragsnummer.Contains(AuftragFilterText.Text)
                                 select item)
            {
                _FilteredOrdersList.Add(item);
            }
           
                this.GridOrders.ItemsSource = _FilteredOrdersList;
        }

        private void DickeFilteredTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + GridMaterialPool);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _FilteredMaterialList.Clear();
            if (DickeFilterText.Text == "")
            {
                MaterialFilteredTextChanged(sender, e);
                return;
            }
            if ((MaterialFilterText.Text == "") && (MatNrFilterText.Text == ""))
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.PlThick == double.Parse(DickeFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            if ((MaterialFilterText.Text == "") && (MatNrFilterText.Text != ""))
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MatNr == Int32.Parse(MatNrFilterText.Text) && item.PlThick == double.Parse(DickeFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            if ((MaterialFilterText.Text != "") && (MatNrFilterText.Text == ""))
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MaterialName.Contains(MaterialFilterText.Text) && item.PlThick == double.Parse(DickeFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }

            this.GridMaterialPool.ItemsSource = _FilteredMaterialList;
        }

        private void MatNrFilteredTextChanged(object sender, TextChangedEventArgs e)
        {
            Console.WriteLine("ChangedEvent   " + GridMaterialPool);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            _FilteredMaterialList.Clear();
            if (MatNrFilterText.Text == "")
            {
                MaterialFilteredTextChanged(sender, e);
                return;
            }
            if (DickeFilterText.Text == "")
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MatNr == Int32.Parse(MatNrFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            else
            {
                foreach (var item in from item in AuftragsDataViewModel._auftragsDataViewModel._PlateData
                                     where item.MatNr == Int32.Parse(MatNrFilterText.Text) && item.PlThick == double.Parse(DickeFilterText.Text)
                                     select item)
                {
                    _FilteredMaterialList.Add(item);
                }
            }
            this.GridMaterialPool.ItemsSource = _FilteredMaterialList;
        }

        private void PartsRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                PartOrderData selectedPartOrder = (PartOrderData)GridOrderParts.SelectedItem;
                Console.WriteLine(selectedPartOrder.PartName);
                //// xPartGrid.DefaultCellStyle.BackColor = Color.Maroon;
                PartOrderData row = (PartOrderData)GridOrderParts.SelectedItems[0];

                ObservableCollection<PartOrderData> data = (ObservableCollection<PartOrderData>)GridOrderParts.ItemsSource;
                data.Remove(row);
            }
            else
            {
                return;
            }
            
        }

        private void UpdatePartsDB(object sender, RoutedEventArgs e)
        {
            _SQLMaterialPool.ChangeOrderedParts(jobHelper.PartsOrderData);
        }
    }

    public enum PNEnumValues
    {
        Options, ExportArchive, UnterArchiv, AddArchiv, Umbenennen, Sortieren, Loeschen
    }

    public class GridRowData
    {
        public bool IsChecked { get; set; }
        public string? Text { get; set; }
        public PNEnumValues PNEnumValue { get; set; }
        public PNEnumValues PNEnumValuee { get; set; }
        public int IntValue { get; set; }
    }

    public class PartOrderData
    {
        public int? IDB050 { get; set; }

        public string? PartName { get; set; }
        public string? OrderPos { get; set; }
        public string? AssemblyName { get; set; }
        public string? Artikel { get; set; }
        public string? AnzahlEinzel { get; set; }
        public string? Anzahl { get; set; }
        public int? AnzahlInt { get; set; }

        public string? Laenge { get; set; }
        public double? LaengeDouble { get; set; }

        public string? Breite { get; set; }
        public double? BreiteDouble { get; set; }

        public string? Dicke { get; set; }
        public double? DickeDouble { get; set; }

        public string? Material { get; set; }
        public int? MaterialInt { get; set; }
        public int? Release { get; set; }
        public int? Status { get; set; }

        public string? Oberflaeche { get; set; }
        public string? Bemerkungen { get; set; }
        public string? Gravur { get; set; }
        public string? Auftrag { get; set; }
        public string? Kunde { get;  set; }
    }

    public class OrdersData
    {
        public int? mpid { get; set; }

        public string? auftragsnummer { get; set; }
        public string? kundenname { get; set; }
        public string? bemerkungen { get; set; }
        public string? bearbeiter1 { get; set; }
        public string? bearbeiter2 { get; set; }
        public string? bearbeiter3 { get; set; }
        public string? bearbeiter4 { get; set; }
        public string? bearbeiter5 { get; set; }
        public string? bearbeiter6 { get; set; }
        public string? bearbeiter7 { get; set; }
        public string? bearbeiter8 { get; set; }
        public string? bearbeiter9 { get; set; }
        public DateTime? avtermin { get; set; }
        public DateTime? produktion { get; set; }
        public DateTime? liefertermin { get; set; }
        public DateTime? oberflaeche { get; set; }
        public DateTime? verpacken { get; set; }
        public DateTime? komplettieren { get; set; }
        public string? ResultThumb { get; set; }
        public string? bemerk_1 { get; set; }
        public string? material1 { get; set; }
        public string? farbe1 { get; set; }
    }


    public class PlateData
    {
        public double? PlNutzFl { get; set; }
        public double PlGewicht { get; set; }

        public int? mpid { get; set; }

        public string? PlName { get; set; }
        public int? PlTyp { get; set; }
        public string? PlIdentNr { get; set; }

        public int? Amount { get; set; }
        public int? ArNumber { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public int? ErstellungsDatum { get; set; }
        public string? MaterialName { get; set; }
        public string? ReferenceNr { get; set; }
       
        public string? MachineNummer { get; set; }
        public string? Bezeichnung { get; set; }
        public int? Lagerplatz { get; set; }

        public string? Bemerk1 { get; set; }
        public string? Bemerk2 { get; set; }
        public string? Bemerk3 { get; set; }
        public string? BestellNummer { get; set; }
        public string? BestellDatum { get; set; }
        public string? BestellPos { get; set; }
        public string? AuftragsNummer { get; set; }
        public string? KundenName { get; set; }
        public string? LSnummer { get; set; }
        public string? PlThickString { get; set; }
        public string? MaxXString { get; set; }
        public string? MaxYString { get; set; }
        public string? MatDesc { get; set; }

        public double? PlThick { get; set; }
        public double? MaxX { get; set; }
        public double? MaxY { get; set; }
        public int? MatNr { get; set; }
        public double? PlFlaeche { get; set; }

        public DateTime? avtermin { get; set; }
        public DateTime? produktion { get; set; }
        public DateTime? liefertermin { get; set; }
        public DateTime? oberflaeche { get; set; }
        public DateTime? verpacken { get; set; }
        public DateTime? komplettieren { get; set; }
 
    }

    public class FolderFilesList
    {
        public ImageSource FileIcon { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string CreationDate { get; set; }
        public string Extension { get; set; }

    }



    public class IsRALOrNotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            string _Oberflaeche = value.ToString();
            Console.WriteLine(_Oberflaeche);
            if (_Oberflaeche.Contains("RAL"))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsEqualOrMoreThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            string[] split = value.ToString().Split(",");
            int intValue = Int32.Parse(split[0]);
            int compareToValue = Int32.Parse(parameter.ToString() ?? string.Empty);

            return intValue >= compareToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
