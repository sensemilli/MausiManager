using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Shapes;
using System.Windows.Shell;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using WiCAM.Apis.Clients.PnApiClient.Models.Request;
using WiCAM.Pn4000.JobManager.AuftragsHelfer;
using WiCAM.Pn4000.TechnoTable;
using Path = System.IO.Path;

namespace WiCAM.Pn4000.JobManager
{
    public class JobHelper
    {
        private Microsoft.Office.Interop.Excel.Range xlrange;
        public ObservableCollection<PartOrderData> PartsOrderData;
        double gesamtlaenge = 0.0;
        double gesamtbreite = 0.0;
        Microsoft.Office.Interop.Excel.Application xlAppPN = null;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbookPN = null;
        Microsoft.Office.Interop.Excel._Worksheet xlWorksheetPN = null;
        string dxfDirectory;
        string dxfpathname;

        public void CreateDXFexelTab()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            
            if (AuftragsDataControl.selectedItem == null)
            {
                dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
            }
            else
                dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName + "\\");

            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
            {
                // Open document

                string dxfDirectory = dlg.SelectedPath;

                //string dxfPath = dlg.Path
                Console.WriteLine("Teileliste =  " + dxfDirectory);

                Microsoft.Office.Interop.Excel.Application xlAppFileNames = new Microsoft.Office.Interop.Excel.Application();
                object misValue = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Workbook xlWorkbookFileNames = xlAppFileNames.Workbooks.Add(misValue);
                Microsoft.Office.Interop.Excel._Worksheet xlWorksheetFileNames = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbookFileNames.Worksheets.get_Item(1);
                //Excel.Workbook xlWorkbookFileNames = xlAppFileNames.Workbooks.Open("C:\\Users\\sense\\Desktop\\dxfFileNamesOrig.xlsx"); 
                //Excel._Worksheet xlWorksheetFileNames = xlWorkbookFileNames.Sheets[1];

                xlAppFileNames.Visible = true;
                Console.WriteLine("Creating Header");
               

                xlWorksheetFileNames.Cells[3, 1] = "Filename";
                xlWorksheetFileNames.Cells[3, 2] = "Pos";
                xlWorksheetFileNames.Cells[3, 3] = "Baugruppe";
                xlWorksheetFileNames.Cells[3, 4] = "Artikel";
                xlWorksheetFileNames.Cells[3, 5] = "Anzahl-Einzelteile";
                xlWorksheetFileNames.Cells[3, 6] = "Anzahl";
                xlWorksheetFileNames.Cells[3, 7] = "Länge";
                xlWorksheetFileNames.Cells[3, 8] = "Breite";
                xlWorksheetFileNames.Cells[3, 9] = "Dicke";
                xlWorksheetFileNames.Cells[3, 10] = "Material";
                xlWorksheetFileNames.Cells[3, 11] = "Oberfläche";
                xlWorksheetFileNames.Cells[3, 12] = "Bemerkungen";
                xlWorksheetFileNames.Cells[3, 13] = "Gravur";
                xlrange = xlWorksheetFileNames.UsedRange;
                xlrange.AutoFilter(1, "", Microsoft.Office.Interop.Excel.XlAutoFilterOperator.xlFilterValues, Type.Missing, true);
                xlWorksheetFileNames.Cells[1, 1] = MainWindow.mainWindow.txtKundenName.Text;
                xlWorksheetFileNames.Cells[1, 4] = MainWindow.mainWindow.txtAuftragsNummer.Text;

                string[] files = Directory.GetFiles(dlg.SelectedPath);
                int iForFiles = 3;
                bool resulting;
                foreach (string file in files)
                {

                    resulting = Path.GetExtension(file).Equals(".dxf");
                    if (resulting == false)
                    {
                        resulting = Path.GetExtension(file).Equals(".DXF");
                    }
                    if (resulting == false)
                    {
                        resulting = Path.GetExtension(file).Equals(".step");
                    }
                    if (resulting == false)
                    {
                        resulting = Path.GetExtension(file).Equals(".STEP");
                    }
                    if (resulting == false)
                    {
                        resulting = Path.GetExtension(file).Equals(".stp");
                    }
                    if (resulting == false)
                    {
                        resulting = Path.GetExtension(file).Equals(".STP");
                    }
                    if (resulting == true)
                    {
                        Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
                        xlWorksheetFileNames.Cells[iForFiles+1, 1] = Path.GetFileNameWithoutExtension(file);
                        iForFiles++;
                    }
                }
                xlWorksheetFileNames.Columns.AutoFit();
                xlWorksheetFileNames.Rows.AutoFit();
                // xlWorkbookFileNames.Save();
                // xlWorkbookFileNames.SaveAs(dxfDirectory + "dxfFileNames.xlsx");
                xlWorkbookFileNames.SaveAs(dxfDirectory + "\\" + MainWindow.mainWindow.txtKundenName.Text + "-" + MainWindow.mainWindow.txtAuftragsNummer.Text + "-TeileListe.xlsx", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //xlWorkbookFileNames.Close(true, misValue, misValue);
  
                Marshal.ReleaseComObject(xlWorksheetFileNames);
                xlWorkbookFileNames.Close();
                Marshal.ReleaseComObject(xlWorkbookFileNames);
                xlAppFileNames.Quit();
            }
        }

        public void CreateJobData(string kundenName, string auftragsNummer, string datePicker, string archName)
        {
            string txtKunde = kundenName;
            string txtAuftrag = auftragsNummer;
            //string txtDatum = txtDatumFeld.Text;
            string txtDatum = datePicker;
            var fileContent = string.Empty;
            var filePath = string.Empty;
            string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
            dlg.FileName = "Teileliste"; // Default file name
            dlg.DefaultExt = ".dxf"; // Default file extension
            dlg.Filter = "CAD dxf Dateien (.dxf,.xlsx)|*.dxf;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                string dxfDirectory = Path.GetDirectoryName(dlg.FileName);

                //string dxfPath = dlg.Path
                Console.WriteLine("Teileliste =  " + filename);

                Microsoft.Office.Interop.Excel.Application xlAppFileNames = new Microsoft.Office.Interop.Excel.Application();
                object misValue = System.Reflection.Missing.Value;
                Microsoft.Office.Interop.Excel.Workbook xlWorkbookFileNames = xlAppFileNames.Workbooks.Add(misValue);
                Microsoft.Office.Interop.Excel._Worksheet xlWorksheetFileNames = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkbookFileNames.Worksheets.get_Item(1);
                //Excel.Workbook xlWorkbookFileNames = xlAppFileNames.Workbooks.Open("C:\\Users\\sense\\Desktop\\dxfFileNamesOrig.xlsx"); 
                //Excel._Worksheet xlWorksheetFileNames = xlWorkbookFileNames.Sheets[1];
              

                xlAppFileNames.Visible = true;
                Console.WriteLine("Creating Header");
                xlWorksheetFileNames.Cells[1, 1] = "Pos";
                xlWorksheetFileNames.Cells[1, 2] = "Artikel";
                xlWorksheetFileNames.Cells[1, 3] = "Anzahl";
                xlWorksheetFileNames.Cells[1, 4] = "Länge";
                xlWorksheetFileNames.Cells[1, 5] = "Breite";
                xlWorksheetFileNames.Cells[1, 6] = "Dicke";
                xlWorksheetFileNames.Cells[1, 7] = "Material";
                xlWorksheetFileNames.Cells[1, 8] = "Oberfläche";
                xlWorksheetFileNames.Cells[1, 9] = "Gravur";
                xlrange = xlWorksheetFileNames.UsedRange;
                xlrange.AutoFilter(2, "", Microsoft.Office.Interop.Excel.XlAutoFilterOperator.xlFilterValues, Type.Missing, true);
                              

                string[] files = Directory.GetFiles(dxfDirectory);
                int iForFiles = 1;
                bool resulting;
                foreach (string file in files)
                {
                    Console.WriteLine("Nr " + iForFiles + "  " + Path.GetFileName(file));
                    resulting = Path.GetExtension(file).Equals(".dxf");
                    if (resulting == true)
                    {
                        xlWorksheetFileNames.Cells[iForFiles+1, 2] = Path.GetFileNameWithoutExtension(file);
                        iForFiles++;
                    }
                }
                xlWorksheetFileNames.Columns.AutoFit();
                xlWorksheetFileNames.Rows.AutoFit();
                // xlWorkbookFileNames.Save();
                // xlWorkbookFileNames.SaveAs(dxfDirectory + "dxfFileNames.xlsx");
                xlWorkbookFileNames.SaveAs(dxfDirectory + "\\dxfFileNames.xlsx", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                //xlWorkbookFileNames.Close(true, misValue, misValue);
                Marshal.ReleaseComObject(xlWorksheetFileNames);
                xlWorkbookFileNames.Close();
                Marshal.ReleaseComObject(xlWorkbookFileNames);
                xlAppFileNames.Quit();

                Marshal.ReleaseComObject(xlAppFileNames);

                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filename);
                Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];
                Microsoft.Office.Interop.Excel.Application xlAppPN = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook xlWorkbookPN = xlAppPN.Workbooks.Open(desktoppath + "\\LoopCustomerFiles.xlsm");
                Microsoft.Office.Interop.Excel._Worksheet xlWorksheetPN = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbookPN.Sheets[1];
                Microsoft.Office.Interop.Excel.Range xlRange = xlWorksheet.UsedRange;
                xlApp.Visible = false;
                xlApp.UserControl = false;
                xlAppPN.Visible = true;

                int rowStart = 2;
                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;
                Console.WriteLine("rows   " + xlRange.Rows.Count);
                Console.WriteLine("cols   " + xlRange.Columns.Count);
               
                AuftragsDataViewModel._auftragsDataViewModel._PartOrderList = PartsOrderData;

                if (PartsOrderData == null)
                    PartsOrderData = new ObservableCollection<PartOrderData>();
               // PartsOrderData.Clear();


                for (int i = 2; i <= rowCount; i++)
                {
                    Console.Write(PartsOrderData);
                    PartsOrderData.Add(new PartOrderData
                    {
                        OrderPos =  Convert.ToString(xlWorksheet.Cells[i, 1].Value),
                        PartName = Convert.ToString(xlWorksheet.Cells[i, 2].Value),
                        Anzahl = Convert.ToString(xlWorksheet.Cells[i, 3].Value),
                        Laenge = Convert.ToString(xlWorksheet.Cells[i, 4].Value),
                        Breite = Convert.ToString(xlWorksheet.Cells[i, 5].Value),
                        Dicke = Convert.ToString(xlWorksheet.Cells[i, 6].Value),
                        Material = Convert.ToString(xlWorksheet.Cells[i, 7].Value),
                        Oberflaeche = Convert.ToString(xlWorksheet.Cells[i, 8].Value),
                        Gravur = Convert.ToString(xlWorksheet.Cells[i, 9].Value)

                    });
                    for (int j = 1; j <= colCount; j++)
                    {
                        //new line
                        if (j == 1)
                            Console.Write("\r\n");

                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        {
                         //   Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");

                            xlWorksheetPN.Cells[rowStart + i, 1] = xlWorksheet.Cells[i, 2].Value;
                            xlWorksheetPN.Cells[rowStart + i, 2] = txtAuftrag;
                            xlWorksheetPN.Cells[rowStart + i, 3] = txtDatum;
                            xlWorksheetPN.Cells[rowStart + i, 4] = xlWorksheet.Cells[i, 7].Value;
                            xlWorksheetPN.Cells[rowStart + i, 5] = xlWorksheet.Cells[i, 6].Value;
                            xlWorksheetPN.Cells[rowStart + i, 6] = xlWorksheet.Cells[i, 3].Value;
                            xlWorksheetPN.Cells[rowStart + i, 7] = xlWorksheet.Cells[i, 4].Value;
                            xlWorksheetPN.Cells[rowStart + i, 8] = xlWorksheet.Cells[i, 5].Value;
                            xlWorksheetPN.Cells[rowStart + i, 9] = "0001";
                            xlWorksheetPN.Cells[rowStart + i, 10] = xlWorksheet.Cells[i, 8].Value;
                            xlWorksheetPN.Cells[rowStart + i, 11] = "Info";
                            xlWorksheetPN.Cells[rowStart + i, 12] = archName;
                            xlWorksheetPN.Cells[rowStart + i, 13] = 1;
                            xlWorksheetPN.Cells[rowStart + i, 14] = dxfDirectory;
                            xlWorksheetPN.Cells[rowStart + i, 15] = "DXF";
                            xlWorksheetPN.Cells[rowStart + i, 16] = ".DXF";
                            xlWorksheetPN.Cells[rowStart + i, 17] = txtKunde;
                            xlWorksheetPN.Cells[rowStart + i, 18] = 00;
                            xlWorksheetPN.Cells[rowStart + i, 19] = xlWorksheet.Cells[i, 1].Value;

                            //   int resultAnzahl;
                            //int.TryParse(xlWorksheet.Cells[i, 3].Value, out resultAnzahl);
                            //    double resultLaenge;
                            //double.TryParse(xlWorksheet.Cells[i, 7].Value, out resultLaenge);
                            //    double resultBreite;
                            //double.TryParse(xlWorksheet.Cells[i, 8].Value, out resultBreite);
                            //  double resultDicke;
                            //double.TryParse(xlWorksheet.Cells[i, 5].Value, out resultDicke);
                          

                            //add useful things here!

                        }
                  
                    }



                    //if (AuftragsDataViewModel._auftragsDataViewModel._PartOrderList != null)
                    //{
                    //    AuftragsDataViewModel._auftragsDataViewModel._PartOrderList.Clear();
                    //}
                    //ObservableCollection<PartOrderData> PartsOrderData = new ObservableCollection<PartOrderData>();
                    //AuftragsDataViewModel._auftragsDataViewModel._PartOrderList = PartsOrderData;
                    //PartsOrderData.Add(new PartOrderData
                    //{
                    //    OrderPos = xlWorksheet.Cells[i, 1].Value,
                    //    PartName = xlWorksheet.Cells[i, 2].Value,
                    //    Anzahl = xlWorksheet.Cells[i, 3].Value.ToString(),
                    //    Laenge = xlWorksheet.Cells[i, 4].Value.ToString(),
                    //    Breite = xlWorksheet.Cells[i, 5].Value.ToString(),
                    //    Dicke = xlWorksheet.Cells[i, 6].Value.ToString(),
                    //    Material = xlWorksheet.Cells[i, 7].Value,
                    //    Oberflaeche = xlWorksheet.Cells[i, 8].Value
                    //});

                }
                AuftragsDataControl._AuftragsDataControl.GridOrderParts.ItemsSource = PartsOrderData;

                xlWorkbookPN.Save();
                //xlWorkbookPN.SaveAs(dxfDirectory + "\\LoopCustomerFiles.xlsm", Excel.XlFileFormat.xlIntlMacro, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
                File.Copy(Path.Combine(desktoppath, "LoopCustomerFiles.xlsm"), Path.Combine(dxfDirectory, "LoopCustomerFiles.xlsm"), true);
                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad


                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheetPN);

                //close and release
                xlWorkbook.Close();
                xlWorkbookPN.Close();
                Marshal.ReleaseComObject(xlWorkbook);
                Marshal.ReleaseComObject(xlWorkbookPN);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
                //xlAppPN.Quit();
                Marshal.ReleaseComObject(xlAppPN);


                Console.WriteLine("finish");
            }
        }

        Microsoft.Office.Interop.Excel.Application xlApp = null;
        Microsoft.Office.Interop.Excel.Workbook xlWorkbook =null;
        Microsoft.Office.Interop.Excel._Worksheet xlWorksheet = null;
        Microsoft.Office.Interop.Excel.Range xlRange;

        public void ReadExcel3D(string kundenName, string auftragsNummer, string datePicker, string archName)
        {
            string txtKunde = kundenName;
            string txtAuftrag = auftragsNummer;
            //string txtDatum = txtDatumFeld.Text;
            string txtDatum = datePicker;
            var fileContent = string.Empty;
            var filePath = string.Empty;
            string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (!AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString().Contains("\\"))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(path: "S:\\cadzeich\\");
            }
            else
                dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName + "\\");
            dlg.FileName = "Teileliste"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "CAD dxf Dateien (.dxf,.xlsx)|*.dxf;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ReadExel3DResult(dlg);
            }
        }

        public void ReadExel3DResult(Microsoft.Win32.OpenFileDialog dlg)
        {
            SQLMaterialPool _SQLMaterialPool = new SQLMaterialPool();

            // Open document
            string filename = dlg.FileName;
            string dxfDirectory = Path.GetDirectoryName(dlg.FileName);

            //string dxfPath = dlg.Path
            Console.WriteLine("Teileliste =  " + filename);

            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlWorkbook = xlApp.Workbooks.Open(filename);
            xlWorksheet = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];

            xlRange = xlWorksheet.UsedRange;
            xlApp.Visible = false;
            xlApp.UserControl = false;

            int rowStart = 0;
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            Console.WriteLine("rows   " + xlRange.Rows.Count);
            Console.WriteLine("cols   " + xlRange.Columns.Count);

            AuftragsDataViewModel._auftragsDataViewModel._PartOrderList = PartsOrderData;

            if (PartsOrderData == null)
                PartsOrderData = new ObservableCollection<PartOrderData>();
            PartsOrderData.Clear();


            for (int i = 4; i <= rowCount; i++)
            {
                if (Convert.ToString(xlWorksheet.Cells[i, 4].Value).Contains("Summe"))
                {
                    AuftragsDataControl._AuftragsDataControl.TextAuftrag.Text = PartsOrderData[0].Auftrag;
                    AuftragsDataControl._AuftragsDataControl.TextKunde.Text = PartsOrderData[0].Kunde;
                    MainWindow.mainWindow.txtAuftragsNummer.Text = PartsOrderData[0].Auftrag;
                    MainWindow.mainWindow.txtKundenName.Text = PartsOrderData[0].Kunde;
                    Console.WriteLine(gesamtlaenge + " * " + gesamtbreite);
                    finishReading();
                    return;
                }
             //   if (xlWorksheet.Cells[i, 4].Value != previouscell)
               // {
                    PartsOrderData.Add(new PartOrderData
                    {
                        PartName = Convert.ToString(xlWorksheet.Cells[i, 1].Value),
                        OrderPos = Convert.ToString(xlWorksheet.Cells[i, 2].Value),
                        AssemblyName = Convert.ToString(xlWorksheet.Cells[i, 3].Value),
                        Artikel = Convert.ToString(xlWorksheet.Cells[i, 4].Value),
                        Anzahl = Convert.ToString(xlWorksheet.Cells[i, 6].Value),
                        Laenge = Convert.ToString(xlWorksheet.Cells[i, 7].Value),
                        Breite = Convert.ToString(xlWorksheet.Cells[i, 8].Value),
                        Dicke = Convert.ToString(xlWorksheet.Cells[i, 9].Value),
                        Material = Convert.ToString(xlWorksheet.Cells[i, 10].Value),
                        MaterialInt = _SQLMaterialPool.switchStringMaterials(Convert.ToString(xlWorksheet.Cells[i, 10].Value)),
                        Oberflaeche = Convert.ToString(xlWorksheet.Cells[i, 11].Value),
                        Bemerkungen = Convert.ToString(xlWorksheet.Cells[i, 12].Value),
                        Gravur = Convert.ToString(xlWorksheet.Cells[i, 13].Value),
                        Auftrag = Convert.ToString(xlWorksheet.Cells[1, 4].Value),
                        Kunde = Convert.ToString(xlWorksheet.Cells[1, 1].Value)
                    });
                  //  Console.WriteLine(Convert.ToString(xlWorksheet.Cells[i, 1].Value));
                if (xlWorksheet.Cells[i, 7].Value != null)
                {
                  //  gesamtlaenge = gesamtlaenge + xlWorksheet.Cells[i, 7].Value;
                }
                if (xlWorksheet.Cells[i, 8].Value != null)
                {
                  //  gesamtbreite = gesamtbreite + xlWorksheet.Cells[i, 8].Value;
                }
                //   previouscell = xlWorksheet.Cells[i, 4].Value;

                //}

            }

        }

        public void ReadExcel2D(string kundenName, string auftragsNummer, string datePicker, string archName)
        {

            string txtKunde = kundenName;
            string txtAuftrag = auftragsNummer;
            //string txtDatum = txtDatumFeld.Text;
            string txtDatum = datePicker;
            if (string.IsNullOrEmpty(datePicker))
            {              
                Console.WriteLine(DateTime.Today);
                txtDatum = DateTime.Today.ToString();
            }
            var fileContent = string.Empty;
            var filePath = string.Empty;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (AuftragsDataControl.selectedItem.FileSystemInfo == null)
                dlg.InitialDirectory = Path.GetDirectoryName(path: "S:\\cadzeich\\");
            
            else
                dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName + "\\");
            dlg.FileName = "Teileliste"; // Default file name
            dlg.DefaultExt = ".xlsx"; // Default file extension
            dlg.Filter = "CAD dxf Dateien (.dxf,.xlsx)|*.dxf;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ReadExcel2DResult(dlg);
            }
        }
     

        public void ReadExcel2DResult(Microsoft.Win32.OpenFileDialog dlg)
        {
            SQLMaterialPool _SQLMaterialPool = new SQLMaterialPool();

            // Open document
            string filename = dlg.FileName;
            string dxfDirectory = Path.GetDirectoryName(dlg.FileName);

            //string dxfPath = dlg.Path
            Console.WriteLine("Teileliste =  " + filename);
            

            xlApp = new Microsoft.Office.Interop.Excel.Application();
            xlWorkbook = xlApp.Workbooks.Open(filename);
            xlWorksheet = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbook.Sheets[1];

            xlRange = xlWorksheet.UsedRange;
            xlApp.Visible = false;
            xlApp.UserControl = false;

            int rowStart = 0;
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;
            Console.WriteLine("rows   " + xlRange.Rows.Count);
            Console.WriteLine("cols   " + xlRange.Columns.Count);

            AuftragsDataViewModel._auftragsDataViewModel._PartOrderList = PartsOrderData;

            if (PartsOrderData == null)
                PartsOrderData = new ObservableCollection<PartOrderData>();
            PartsOrderData.Clear();


            for (int i = 4; i <= rowCount; i++)
            {
                if (Convert.ToString(xlWorksheet.Cells[i, 4].Value).Contains("Summe"))
                {
                    AuftragsDataControl._AuftragsDataControl.TextAuftrag.Text = PartsOrderData[0].Auftrag;
                    AuftragsDataControl._AuftragsDataControl.TextKunde.Text = PartsOrderData[0].Kunde;
                    MainWindow.mainWindow.txtAuftragsNummer.Text = PartsOrderData[0].Auftrag;
                    MainWindow.mainWindow.txtKundenName.Text = PartsOrderData[0].Kunde;
                    Console.WriteLine(gesamtlaenge + " * " + gesamtbreite);
                    finishReading();
                    return;
                }
              // if (xlWorksheet.Cells[i, 4].Value != previouscell)
                //{
                    PartsOrderData.Add(new PartOrderData
                    {
                        PartName = Convert.ToString(xlWorksheet.Cells[i, 1].Value),
                        OrderPos = Convert.ToString(xlWorksheet.Cells[i, 2].Value),
                        AssemblyName = Convert.ToString(xlWorksheet.Cells[i, 3].Value),
                        Artikel = Convert.ToString(xlWorksheet.Cells[i, 4].Value),
                        Anzahl = Convert.ToString(xlWorksheet.Cells[i, 6].Value),
                        Laenge = Convert.ToString(xlWorksheet.Cells[i, 7].Value),
                        Breite = Convert.ToString(xlWorksheet.Cells[i, 8].Value),
                        Dicke = Convert.ToString(xlWorksheet.Cells[i, 9].Value),
                        Material = Convert.ToString(xlWorksheet.Cells[i, 10].Value),
                        MaterialInt = _SQLMaterialPool.switchStringMaterials(Convert.ToString(xlWorksheet.Cells[i, 10].Value)),
                        Oberflaeche = Convert.ToString(xlWorksheet.Cells[i, 11].Value),
                        Bemerkungen = Convert.ToString(xlWorksheet.Cells[i, 12].Value),
                        Gravur = Convert.ToString(xlWorksheet.Cells[i, 13].Value),
                        Auftrag = Convert.ToString(xlWorksheet.Cells[1, 4].Value),
                        Kunde = Convert.ToString(xlWorksheet.Cells[1, 1].Value)
                    });
                    Console.WriteLine(Convert.ToString(xlWorksheet.Cells[i, 1].Value));
                if (xlWorksheet.Cells[i, 7].Value != null)
                gesamtlaenge = gesamtlaenge + xlWorksheet.Cells[i, 7].Value / 1000;
                if (xlWorksheet.Cells[i, 8].Value != null)
                    gesamtbreite = gesamtbreite + xlWorksheet.Cells[i, 8].Value / 1000;
                //   previouscell = xlWorksheet.Cells[i, 4].Value;

                // }

            }
        }

        public void finishReading()
        {
            AuftragsDataControl._AuftragsDataControl.GridOrderParts.ItemsSource = PartsOrderData;

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
            Marshal.ReleaseComObject(xlWorkbook);
      
            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);
      
            Console.WriteLine("finish");
            int count = PartsOrderData[1].Auftrag.Count();
            if (count >= 5)
            {
                AuftragsDataControl._AuftragsDataControl.TextFilterText.Text = PartsOrderData[1].Auftrag.Remove(5, count - 5);
            }
        }

       
        public void WritePN3DExcel(string kundenName, string auftragsNummer, string datePicker, string archName)
        {
            PartsOrderData = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData;

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (AuftragsDataControl.selectedItem == null)
                dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
            else
                dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString() + "\\");
            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Open document
                dxfpathname = dlg.SelectedPath;
                dxfDirectory = Path.GetDirectoryName(dlg.SelectedPath);

                string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                String today = datePicker;

                xlAppPN = new Microsoft.Office.Interop.Excel.Application();
                xlWorkbookPN = xlAppPN.Workbooks.Open(desktoppath + "\\AutoloopBend.xlsm");
                xlWorksheetPN = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbookPN.Sheets[1];
                xlAppPN.Visible = true;

                int partsCount = PartsOrderData.Count;
                int rowStart = 4;

                for (int j = 0; j <= partsCount - 1; j++)
                {
                    xlWorksheetPN.Cells[rowStart + j, 1] = PartsOrderData[j].PartName;
                    xlWorksheetPN.Cells[rowStart + j, 2] = PartsOrderData[j].Auftrag;
                    xlWorksheetPN.Cells[rowStart + j, 3] = today;
                    xlWorksheetPN.Cells[rowStart + j, 4] = PartsOrderData[j].Material;
                    xlWorksheetPN.Cells[rowStart + j, 5] = 0;
                    xlWorksheetPN.Cells[rowStart + j, 6] = PartsOrderData[j].Anzahl;
                    xlWorksheetPN.Cells[rowStart + j, 9] = 0001;
                    xlWorksheetPN.Cells[rowStart + j, 10] = PartsOrderData[j].Bemerkungen;
                    xlWorksheetPN.Cells[rowStart + j, 11] = 1;
                    xlWorksheetPN.Cells[rowStart + j, 12] = MainWindow.mainWindow.txtArchivName.Text;
                    xlWorksheetPN.Cells[rowStart + j, 13] = 1;
                    xlWorksheetPN.Cells[rowStart + j, 14] = dxfpathname;
                    xlWorksheetPN.Cells[rowStart + j, 15] = "STP";
                    xlWorksheetPN.Cells[rowStart + j, 16] = PartsOrderData[j].Kunde;
                    xlWorksheetPN.Cells[rowStart + j, 17] = 0;
                    xlWorksheetPN.Cells[rowStart + j, 18] = PartsOrderData[j].Gravur;
                }
                xlWorkbookPN.SaveAs(dxfpathname + "\\" + PartsOrderData[0].Kunde + "-" + PartsOrderData[0].Auftrag + "-PNloop.xlsm",
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         false,
         false,
         Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
         false,
         false,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value);
                //     xlWorkbookPN.SaveAs(string.Format("{0}.xlsm", dxfDirectory, 0) + dxfDirectory, Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);

                //   xlWorkbookPN.SaveAs(dxfDirectory + "\\PNloop{0}.xlsm", Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                finishWriting();
            }
            //" + PartsOrderData[1].Kunde + "-" + PartsOrderData[1].Auftrag + "-
          
        }

        public void WritePN2DExcel(string kundenName, string auftragsNummer, string datePicker, string archName)
        {
            PartsOrderData = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData;

            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (AuftragsDataControl.selectedItem == null)
                dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");  
        
            else
                dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString() + "\\");
            // Show open file dialog box
            DialogResult result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Open document
                dxfpathname = dlg.SelectedPath;
                dxfDirectory = Path.GetDirectoryName(dlg.SelectedPath);

                string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                String today = datePicker;

                xlAppPN = new Microsoft.Office.Interop.Excel.Application();
                xlWorkbookPN = xlAppPN.Workbooks.Open(desktoppath + "\\Autoloop2D.xlsm");
                xlWorksheetPN = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbookPN.Sheets[1];
                xlAppPN.Visible = true;

                int partsCount = PartsOrderData.Count;
                int rowStart = 4;

                for (int j = 0; j <= partsCount - 1; j++)
                {
                    xlWorksheetPN.Cells[rowStart + j, 1] = PartsOrderData[j].PartName;
                    xlWorksheetPN.Cells[rowStart + j, 2] = PartsOrderData[j].Auftrag;
                    xlWorksheetPN.Cells[rowStart + j, 3] = today;
                    xlWorksheetPN.Cells[rowStart + j, 4] = PartsOrderData[j].Material;
                    xlWorksheetPN.Cells[rowStart + j, 5] = PartsOrderData[j].Dicke;
                    xlWorksheetPN.Cells[rowStart + j, 6] = PartsOrderData[j].Anzahl;
                    xlWorksheetPN.Cells[rowStart + j, 7] = PartsOrderData[j].Laenge;
                    xlWorksheetPN.Cells[rowStart + j, 8] = PartsOrderData[j].Breite;
                    xlWorksheetPN.Cells[rowStart + j, 9] = 0001;
                    xlWorksheetPN.Cells[rowStart + j, 10] = PartsOrderData[j].Oberflaeche;
                    xlWorksheetPN.Cells[rowStart + j, 11] = PartsOrderData[j].Bemerkungen;
                    xlWorksheetPN.Cells[rowStart + j, 12] = MainWindow.mainWindow.txtArchivName.Text;
                    xlWorksheetPN.Cells[rowStart + j, 13] = 1;
                    xlWorksheetPN.Cells[rowStart + j, 14] = dxfpathname + "\\";
                    xlWorksheetPN.Cells[rowStart + j, 15] = "DXF";
                    xlWorksheetPN.Cells[rowStart + j, 16] = ".dxf";
                    xlWorksheetPN.Cells[rowStart + j, 17] = PartsOrderData[j].Kunde;
                    xlWorksheetPN.Cells[rowStart + j, 18] = 0;
                    xlWorksheetPN.Cells[rowStart + j, 19] = PartsOrderData[j].Gravur;
                }
                xlWorkbookPN.SaveAs(dxfpathname + "\\" + PartsOrderData[0].Kunde + "-" + PartsOrderData[0].Auftrag + "-PN2Dloop.xlsm",
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         false,
         false,
         Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
         false,
         false,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value,
         System.Reflection.Missing.Value);
                //     xlWorkbookPN.SaveAs(string.Format("{0}.xlsm", dxfDirectory, 0) + dxfDirectory, Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);

                //   xlWorkbookPN.SaveAs(dxfDirectory + "\\PNloop{0}.xlsm", Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                finishWriting();
            }
            //" + PartsOrderData[1].Kunde + "-" + PartsOrderData[1].Auftrag + "-

        }


        public void finishWriting()
        {
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlWorksheetPN);

            //close and release
           // xlWorkbookPN.Close();
            Marshal.ReleaseComObject(xlWorkbookPN);

            //quit and release
        //    xlAppPN.Quit();
            Marshal.ReleaseComObject(xlAppPN);

            Console.WriteLine("finish");

        }

        public void WriteExcel2Swift(string kundenName, string auftragsNummer, string datePicker, string archName)
        {
            //FolderBrowserDialog dlg = new FolderBrowserDialog();
            //if (!AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString().Contains("X:"))
            //{
            //    dlg.InitialDirectory = Path.GetDirectoryName(path: "X:\\Kunden\\");
            //}
            //else
            //    dlg.InitialDirectory = Path.GetDirectoryName(path: AuftragsDataControl.selectedItem.FileSystemInfo.FullName.ToString() + "\\");
            //// Show open file dialog box
            //DialogResult result = dlg.ShowDialog();
            //if (result == DialogResult.OK)
            //{
            //    // Open document
            //    dxfpathname = dlg.SelectedPath;
            //    dxfDirectory = Path.GetDirectoryName(dlg.SelectedPath);

                string Excel2Swiftpath = "C:\\EXCEL2SWIFT";
                String today = datePicker;

                xlAppPN = new Microsoft.Office.Interop.Excel.Application();
                xlWorkbookPN = xlAppPN.Workbooks.Open(Excel2Swiftpath + "\\Excel2Swift.xlsm");
                xlWorksheetPN = (Microsoft.Office.Interop.Excel._Worksheet)xlWorkbookPN.Sheets[1];
                xlAppPN.Visible = true;

                int partsCount = PartsOrderData.Count;
                int rowStart = 3;
            xlWorksheetPN.Cells[1, 2] = PartsOrderData[0].Auftrag;
            xlWorksheetPN.Cells[1, 6] = PartsOrderData[0].Kunde;

            for (int j = 0; j <= partsCount - 1; j++)
                {
                    xlWorksheetPN.Cells[rowStart + j, 1] = PartsOrderData[j].PartName;
                xlWorksheetPN.Cells[rowStart + j, 2] = PartsOrderData[j].Anzahl;
                xlWorksheetPN.Cells[rowStart + j, 3] = PartsOrderData[j].Laenge;
                xlWorksheetPN.Cells[rowStart + j, 4] = PartsOrderData[j].Breite;
                xlWorksheetPN.Cells[rowStart + j, 5] = PartsOrderData[j].Dicke;
                xlWorksheetPN.Cells[rowStart + j, 6] = PartsOrderData[j].Material;
                xlWorksheetPN.Cells[rowStart + j, 7] = PartsOrderData[j].Bemerkungen;
                
                }

         //       xlWorkbookPN.SaveAs(dxfpathname + "\\" + PartsOrderData[1].Kunde + "-" + PartsOrderData[1].Auftrag + "-PNloop.xlsm",
         //System.Reflection.Missing.Value,
         //System.Reflection.Missing.Value,
         //System.Reflection.Missing.Value,
         //false,
         //false,
         //Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
         //false,
         //false,
         //System.Reflection.Missing.Value,
         //System.Reflection.Missing.Value,
         //System.Reflection.Missing.Value);
                //     xlWorkbookPN.SaveAs(string.Format("{0}.xlsm", dxfDirectory, 0) + dxfDirectory, Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value, System.Reflection.Missing.Value);

                //   xlWorkbookPN.SaveAs(dxfDirectory + "\\PNloop{0}.xlsm", Microsoft.Office.Interop.Excel.XlFileFormat.xlIntlMacro, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                finishWriting();
            
            //" + PartsOrderData[1].Kunde + "-" + PartsOrderData[1].Auftrag + "-

        }
    }
}
