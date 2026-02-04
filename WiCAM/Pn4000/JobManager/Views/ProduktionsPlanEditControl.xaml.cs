using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
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
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows.Media;

//using System.Runtime.InteropServices.WindowsRuntime;
using File = System.IO.File;
using WiCAM.Pn4000.TechnoTable;
using MessageBox = System.Windows.MessageBox;
using Range = Microsoft.Office.Interop.Excel.Range;
using static WiCAM.Pn4000.JobManager.ProduktionsPlanEditViewModel;


namespace WiCAM.Pn4000.JobManager
{
    public partial class ProduktionsPlanEditControl : UserControl, IView, IComponentConnector
    {
        public static ProduktionsPlanEditControl _ProduktionsPlanEditControl;
        private Microsoft.Office.Interop.Excel.Application oExcelApp = null;
        private Workbook oWorkbook = null;
        private Worksheet oWorksheet = null;
        private DataGrid produktionsPlanViewExcel;
        private ObservableCollection<AuftragsListeData> _AuftragsListeData;
        private string _ProduktionsPlanPfad;

        public ProduktionsPlanEditControl()
        {
            this.InitializeComponent();
            _ProduktionsPlanEditControl = this;
          //  pathToPNdrive = PnPathBuilder.PnDrive;
          //  CheckToolBar();
 

        }



     

        //****************************************************************
        // RichEditBox Open/Save File Button Click Events
        //****************************************************************
        private async void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            //// Open a text file.
            //Windows.Storage.Pickers.FileOpenPicker open =
            //    new Windows.Storage.Pickers.FileOpenPicker();
            //open.SuggestedStartLocation =
            //    Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            //open.FileTypeFilter.Add(".rtf");
            //open.FileTypeFilter.Add(".txt");

            //Windows.Storage.StorageFile file = await open.PickSingleFileAsync();
            //if (file != null)
            //{
            //    try
            //    {
            //        Windows.Storage.Streams.IRandomAccessStream randAccStream =
            //    await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            //        //// Load the file into the Document property of the RichEditBox.
            //        if (file.FileType == ".rtf")
            //            editor.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
            //        else if(file.FileType == ".txt")
            //            editor.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.None, randAccStream);
            //    }
            //    catch (Exception)
            //    {
            //        ContentDialog errorDialog = new ContentDialog()
            //        {
            //            Title = "File open error",
            //            Content = "Sorry, I couldn't open the file.",
            //            PrimaryButtonText = "Ok"
            //        };

            //        await errorDialog.ShowAsync();
            //    }
            //}
            /*
            // Open a text file.
            Windows.Storage.Pickers.FileOpenPicker open =
                new Windows.Storage.Pickers.FileOpenPicker();
            open.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            open.FileTypeFilter.Add(".rtf");
            open.FileTypeFilter.Add(".txt");
            open.FileTypeFilter.Add(".docx");

            Windows.Storage.StorageFile file = await open.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    IBuffer buffer = await FileIO.ReadBufferAsync(file);
                    var reader = DataReader.FromBuffer(buffer);
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string text = reader.ReadString(buffer.Length);
                    //// Load the file into the Document property of the RichEditBox.
                    editor.Document.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, text);
                }
                catch (Exception)
                {
                    ContentDialog errorDialog = new ContentDialog()
                    {
                        Title = "File open error",
                        Content = "Sorry, I couldn't open the file.",
                        PrimaryButtonText = "Ok"
                    };
                    await errorDialog.ShowAsync();
                }
            }
            */
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

        private void ProduktionsPlanDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ProduktionsPlanRead()
        {
         
			if (_AuftragsListeData == null)
				_AuftragsListeData = new ObservableCollection<AuftragsListeData>();
			_AuftragsListeData.Clear();
			try
			{
				//string fileName = "C:\\Users\\Tommy\\Desktop\\Test.xlsx";
				//string fileName = "C:\\Users\\Tommy\\Desktop\\Test.xls";
				string fileName =  "C:\\Users\\Tommy\\Desktop\\Fertigungsliste.xlsm";
			  Range oRange;
            string sSpaltenName;
            object oValue;
            OpenFileDialog openFileDialogExcel = new OpenFileDialog();  //create openfileDialog Object
            if (openFileDialogExcel.ShowDialog() == DialogResult.OK)
            {
                // Datei öffnen und aktives Blatt und benutzten Bereich auswählen
                _ProduktionsPlanPfad = openFileDialogExcel.FileName;
                oExcelApp = new Microsoft.Office.Interop.Excel.Application();
                oWorkbook = oExcelApp.Workbooks.Open(openFileDialogExcel.FileName);
                oWorksheet = (Worksheet)oWorkbook.ActiveSheet;
                oRange = oWorksheet.UsedRange;
                int rowCount = oRange.Rows.Count;

              //  dataGridViewExcel = _grid;
                // Zellen vom Dokument in die Ansicht laden
          //      for (int i = 0; i < oRange.Columns.Count; i++)
            //    {
   /*                 if (i >= dataGridViewExcel.Columns.Count)
                    {
                        // bilden der Spalten-Namen: A, B, C, D, E, ..., A1, B1, C1, ..., A2, B2, ...
                        sSpaltenName = ((char)('A' + i % 26)).ToString();
                        if (Math.Floor(i / 26.0) != 0)
                            sSpaltenName += Math.Floor(i / 26.0).ToString();
                        Console.WriteLine(sSpaltenName);
                      //  dataGridViewExcel.Columns.Add(sSpaltenName, sSpaltenName);
                    }
            */
				  for (int i = 5; i <= rowCount; i++)
				  {
                        // Zeilen werden nur beim 1. Durchlauf hinzugefügt
                        if (i == 5)
						{
							//  dataGridViewExcel.Rows.Add();  add header
						}
                        else
	                  //      Console.WriteLine(j);
                          //  dataGridViewExcel.Rows.Add();
                    //    oValue = (oRange.Cells[j + 1, i + 1] as Range).Value;
                        // muss unbedingt abgefangen werden
                  //      if (oValue != null)
	                //        Console.WriteLine(oValue.ToString());
                        _AuftragsListeData.Add(new AuftragsListeData
                        {
	                        Auslieferung = Convert.ToString(oWorksheet.Cells[i, 1].Value),
	                        Object = Convert.ToString(oWorksheet.Cells[i, 2].Value),
	                        Vorgangsname = Convert.ToString(oWorksheet.Cells[i, 3].Value),
	                        Auftrag = Convert.ToString(oWorksheet.Cells[i, 4].Value),
	                        Bestellnummer = Convert.ToString(oWorksheet.Cells[i, 5].Value),
	                        Artikel = Convert.ToString(oWorksheet.Cells[i, 6].Value),
	                        Anfang = Convert.ToString(oWorksheet.Cells[i, 7].Value),
	                        Fertig = Convert.ToString(oWorksheet.Cells[i, 8].Value),
	                        Oberflaeche = Convert.ToString(oWorksheet.Cells[i, 9].Value),
	                        Infos = Convert.ToString(oWorksheet.Cells[i, 10].Value),
	                        Zustaendig = Convert.ToString(oWorksheet.Cells[i, 11].Value),
	                        AV = Convert.ToString(oWorksheet.Cells[i, 12].Value),
	                        MaterialBestellt = Convert.ToString(oWorksheet.Cells[i, 13].Value),
	                        Programmiert = Convert.ToString(oWorksheet.Cells[i, 14].Value),
	                        Abgewickelt = Convert.ToString(oWorksheet.Cells[i, 15].Value),
	                        Stanzen = Convert.ToString(oWorksheet.Cells[i, 16].Value),
	                        Biegen = Convert.ToString(oWorksheet.Cells[i, 17].Value),
	                        Schweissen = Convert.ToString(oWorksheet.Cells[i, 18].Value),
	                        Verpacken = Convert.ToString(oWorksheet.Cells[i, 19].Value),

	                        RowNumber = i,
                        });
                        if (oWorksheet.Cells[i, 3].Value != null)
							Console.WriteLine(oWorksheet.Cells[i, 3].Value.ToString());
                           //  dataGridViewExcel.Cells[i].Value = oValue.ToString();
                           if (oWorksheet.Cells[i, 3].Value == null)
	                           break;
				  }
					MainWindow._instance.produktionsPlanEditControl.xProduktionsPlanDataGrid.ItemsSource = _AuftragsListeData;
					GC.Collect();
					GC.WaitForPendingFinalizers();

					//release com objects to fully kill excel process from running in the background
					Marshal.ReleaseComObject(oRange);
					Marshal.ReleaseComObject(oWorksheet);

					//close and release
					oWorkbook.Close(false, System.Reflection.Missing.Value, System.Reflection.Missing.Value);
					Marshal.ReleaseComObject(oWorkbook);
      
					//quit and release
					oExcelApp.Quit();
					Marshal.ReleaseComObject(oExcelApp);
                   // dataGridViewExcel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                   // dataGridViewExcel.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                   // dataGridViewExcel.AllowUserToResizeColumns = true;
                   // dataGridViewExcel.AllowUserToResizeRows = true;
                   // dataGridViewExcel.AllowUserToAddRows = false;
                   // dataGridViewExcel.AllowUserToDeleteRows = false;
                   // dataGridViewExcel.ReadOnly = true;
                   // dataGridViewExcel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            }
           // else // Bei Abbruch, Fenster schließen
              //  Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

        private void CellValueChanged(object sender, TextChangedEventArgs e)
        {
			var cell = xProduktionsPlanDataGrid.CurrentCell;
			if (cell != null)
			{
				var row = cell.Item as AuftragsListeData;
				if (row != null)
				{
					Console.WriteLine(row.RowNumber +  "  "  + row.AV);
				//	WriteProduktionsPlanExcel(row.RowNumber, row.AV);
					// Hier können Sie den Wert der Zelle bearbeiten
					// Beispiel: row.SomeProperty = newValue;
				}
			}
		}
        
           public void WriteProduktionsPlanExcel(int? rowNumber, string avfield)
        {
	        oExcelApp = new Microsoft.Office.Interop.Excel.Application();
	        oExcelApp.Visible = true;
	        oWorkbook = oExcelApp.Workbooks.Open(_ProduktionsPlanPfad);
	        oWorksheet = (Worksheet)oWorkbook.ActiveSheet;

	        var cell = xProduktionsPlanDataGrid.CurrentCell;
		        var row = cell.Item as AuftragsListeData;
		        
	        oWorksheet.Cells[rowNumber, 12] = row.AV;
                
	        oWorkbook.SaveAs(_ProduktionsPlanPfad,
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
                //cleanup
               // GC.Collect();
             //   GC.WaitForPendingFinalizers();

                //release com objects to fully kill excel process from running in the background
             //   Marshal.ReleaseComObject(oWorksheet);

                //close and release
                // xlWorkbookPN.Close();
            //    Marshal.ReleaseComObject(oWorkbook);

                //quit and release
                //    xlAppPN.Quit();
            //    Marshal.ReleaseComObject(oExcelApp);

                Console.WriteLine("finish");
                
        
            //" + PartsOrderData[1].Kunde + "-" + PartsOrderData[1].Auftrag + "-
          
        }

        public void ProduktionsPlanReadFilteredVerpackt()
        {
            Console.WriteLine("ChangedEvent   " + produktionsPlanViewExcel + "  " + _AuftragsListeData);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            ObservableCollection<AuftragsListeData> TempFiltered = new ObservableCollection<AuftragsListeData>();
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData, (object)DateTime.Now.ToString("s"));
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData.Count, (object)DateTime.Now.ToString("s"));
            
            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            TempFiltered.Clear();
            if (MainWindow._instance.xFilterVerpackt.IsChecked == false)
            {
                MainWindow._instance.produktionsPlanEditControl.xProduktionsPlanDataGrid.ItemsSource = _AuftragsListeData;
                return;
            }


            else
            {
               for (int i = 0; i <= _AuftragsListeData.Count - 1; i++)
                //  if (_AuftragsListeData[i].Verpacken != null)

                /*    foreach (var item in from item in _AuftragsListeData
                                     where (item.Vorgangsname.Contains("2"))
                                     select item) */
                {
                    if (_AuftragsListeData[i].Verpacken == "x" || _AuftragsListeData[i].Verpacken == "X")
                    {
                        Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData[i].Verpacken, (object)DateTime.Now.ToString("s"));
                    }
                    else
                        TempFiltered.Add(_AuftragsListeData[i]);

                }
            }

            MainWindow._instance.produktionsPlanEditControl.xProduktionsPlanDataGrid.ItemsSource = TempFiltered;
        }

        public void ProduktionsPlanReadFilteredBiegen()
        {
            Console.WriteLine("ChangedEvent   " + produktionsPlanViewExcel + "  " + _AuftragsListeData);
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            ObservableCollection<AuftragsListeData> TempFiltered = new ObservableCollection<AuftragsListeData>();
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData, (object)DateTime.Now.ToString("s"));
            Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData.Count, (object)DateTime.Now.ToString("s"));

            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            TempFiltered.Clear();
            if (MainWindow._instance.xFilterBiegen.IsChecked == false)
            {
                MainWindow._instance.produktionsPlanEditControl.xProduktionsPlanDataGrid.ItemsSource = _AuftragsListeData;
                return;
            }


            else
            {
                for (int i = 0; i <= _AuftragsListeData.Count - 1; i++)
                //  if (_AuftragsListeData[i].Verpacken != null)

                /*    foreach (var item in from item in _AuftragsListeData
                                     where (item.Vorgangsname.Contains("2"))
                                     select item) */
                {
                    if (_AuftragsListeData[i].Biegen == "x" || _AuftragsListeData[i].Biegen == "X")
                    {
                        Logger.Info("ChckBoxFilterVerpacktCmd : {0}" + _AuftragsListeData[i].Biegen, (object)DateTime.Now.ToString("s"));
                    }
                    else
                        TempFiltered.Add(_AuftragsListeData[i]);

                }
            }

            MainWindow._instance.produktionsPlanEditControl.xProduktionsPlanDataGrid.ItemsSource = TempFiltered;
        }



        private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			// Save the file
			try
			{
				oWorkbook.Save();
				MessageBox.Show("File saved successfully.");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error saving file: " + ex.Message);
			}        }
    }
    }
