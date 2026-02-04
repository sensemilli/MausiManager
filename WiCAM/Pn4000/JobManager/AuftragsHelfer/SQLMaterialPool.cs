using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Gmpool;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.Office.Interop;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using WiCAM.Pn4000.Common;
using ControlzEx.Standard;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Data;
using System.IO;
using System.Windows.Media.Media3D;
using WiCAM.Pn4000.BendModel.BendTools.Macros;

namespace WiCAM.Pn4000.JobManager.AuftragsHelfer
{
    internal class SQLMaterialPool
    {
        public static string readablePhrase;
        PlateData plate;
        private int intCountString;
        private string[] kk;
        private double resultK;
        private string[] mm;
        private double resultM;
        private string[] pp;
        private double resultP;
        private string pathToPNdrive;
        private string connectionString;
        private string sqlQuery;
        private SqlCommand sqlcmd;
        private SqlDataAdapter sqlda;
        private DateTime parsedTime;
        private double dichte;

        public SQLMaterialPool()
        {
            pathToPNdrive = PnPathBuilder.PnDrive;
        }




        internal void ReadMaterialCSV(AuftragsDataViewModel auftragsDataViewModel, string action)
        {
          
            auftragsDataViewModel.PlateData = new ObservableCollection<PlateData>();   

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(path: "C:\\wicam_csv\\");
            dlg.FileName = "MaterialBestellListe"; // Default file name
            dlg.DefaultExt = ".csv"; // Default file extension
            dlg.Filter = "CSV Dateien (.csv,.xlsx)|*.csv;*.xlsx"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                string csvDirectory = System.IO.Path.GetDirectoryName(dlg.FileName);

                //string dxfPath = dlg.Path
                Console.WriteLine("MaterialListe =  " + filename);
                Excel.Application xlMaterialApp = new Excel.Application();
                Excel.Workbook xlMaterialWorkbook = xlMaterialApp.Workbooks.Open(filename);
                Excel._Worksheet xlMaterialWorksheet = (Excel._Worksheet)xlMaterialWorkbook.Sheets[1];
                Excel.Range xlMaterialRange = xlMaterialWorksheet.UsedRange;
                int rowStart = 2;
                int rowCount = xlMaterialRange.Rows.Count;
                int colCount = xlMaterialRange.Columns.Count;
                Console.WriteLine("rows   " + xlMaterialRange.Rows.Count);
                Console.WriteLine("cols   " + xlMaterialRange.Columns.Count);

                for (int i = 2; i <= rowCount; i++)
                {
                    for (int j = 1; j <= colCount; j++)
                    {
                        //new line
                        if (j == 1)
                            Console.Write("\r\n");

                        //write the value to the console
                        if (xlMaterialRange.Cells[i, j] != null && xlMaterialRange.Cells[i, j].Value2 != null)
                        {
                            string bestellNr = xlMaterialWorksheet.Cells[i, 1].Value;
                            string bestellDatum = xlMaterialWorksheet.Cells[i, 2].Value;
                            string lieferant = xlMaterialWorksheet.Cells[i, 3].Value;
                            string position = xlMaterialWorksheet.Cells[i, 4].Value;
                            string material = xlMaterialWorksheet.Cells[i, 5].Value;
                            string bestellTermin = xlMaterialWorksheet.Cells[i, 6].Value;
                            string bezeichnung = xlMaterialWorksheet.Cells[i, 7].Value;
                            string auftrNr = xlMaterialWorksheet.Cells[i, 8].Value;
                            string objekt = xlMaterialWorksheet.Cells[i, 9].Value;
                            string lieferschein = xlMaterialWorksheet.Cells[i, 10].Value;
                            string laenge = xlMaterialWorksheet.Cells[i, 11].Value;
                            string hoehe = xlMaterialWorksheet.Cells[i, 12].Value;
                            string breitee = xlMaterialWorksheet.Cells[i, 13].Value;
                            string anzahl = xlMaterialWorksheet.Cells[i, 14].Value;
                            string nlb = xlMaterialWorksheet.Cells[i, 15].Value;
                            string thick = xlMaterialWorksheet.Cells[i, 16].Value;

                            string[] aa = { bestellNr, bestellDatum, lieferant, position, material, bestellTermin, bezeichnung, auftrNr, objekt, lieferschein, laenge, hoehe, breitee, anzahl, nlb, thick };
                            readablePhrase = string.Join(" ", aa);

                            

                          

                            //    string bestellDatum = xlMaterialWorksheet.Cells[i, 2].Value;
                            //    string lieferant = xlMaterialWorksheet.Cells[i, 3].Value;
                            //    string position = xlMaterialWorksheet.Cells[i, 4].Value;
                            //    string material = xlMaterialWorksheet.Cells[i, 5].Value;
                            //    string bestellTermin = xlMaterialWorksheet.Cells[i, 6].Value;
                            //    string bezeichnung = xlMaterialWorksheet.Cells[i, 7].Value;
                            //    string auftrNr = xlMaterialWorksheet.Cells[i, 8].Value;
                            //    string objekt = xlMaterialWorksheet.Cells[i, 9].Value;
                            //    string lieferschein = xlMaterialWorksheet.Cells[i, 10].Value;
                            //    string laenge = xlMaterialWorksheet.Cells[i, 11].Value;
                            //    string hoehe = xlMaterialWorksheet.Cells[i, 12].Value;
                            //    string breitee = xlMaterialWorksheet.Cells[i, 13].Value;
                            //    string anzahl = xlMaterialWorksheet.Cells[i, 14].Value;
                            //    string nlb = xlMaterialWorksheet.Cells[i, 15].Value;
                            //    string thick = xlMaterialWorksheet.Cells[i, 16].Value;

                            //    string[] aa = { bestellNr, bestellDatum, lieferant, position, material, bestellTermin, bezeichnung, auftrNr, objekt, lieferschein, laenge, hoehe, breitee, anzahl, nlb, thick };
                            //    readablePhrase = string.Join(" ", aa);


                        }
                        Console.WriteLine(xlMaterialRange.Cells[i, j].Value2.ToString() + "\t");


                    }

                    AddToPlateList(readablePhrase);

                    //if (action == "database")
                    //    auftragsDataViewModel.Connect(readablePhrase);
                    //if (action == "print")
                    //    auftragsDataViewModel.EtikettenPrint(readablePhrase);
                }
              
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlMaterialRange);
                Marshal.ReleaseComObject(xlMaterialWorksheet);

                //close and release
                xlMaterialWorkbook.Close();
                Marshal.ReleaseComObject(xlMaterialWorkbook);

                //quit and release
                xlMaterialApp.Quit();
                Marshal.ReleaseComObject(xlMaterialApp);

                //if (action == "print")
                //    auftragsDataViewModel.CreateMatPrintExcel();
            }
        }

        public void AddToPlateList(string readable)
        {
            string bestelldatum = ""; string lieferant = ""; string posdesc = "";
            string matdesc = ""; string datedesc = ""; string material = "";
            string auftragsnummerdesc = ""; string kundennamedesc = ""; string lsnummer = "";
            string laenge = ""; string l = ""; string breite = "";
            string amountdesc = ""; string platedesc = ""; string dicke = ""; string q = ""; string bestellnummer = "";
            intCountString = 1;

            string[] dataFieldsList = readable.Split(';');
            foreach (string field in dataFieldsList)
            {
                Console.WriteLine(field);
                if (intCountString == 1)
                    bestellnummer = field;
                if (intCountString == 2)
                    bestelldatum = field;
                if (intCountString == 3)
                    lieferant = field;
                if (intCountString == 4)
                    posdesc = field;
                if (intCountString == 5)
                    matdesc = field;
                if (intCountString == 6)
                    datedesc = field;
                if (intCountString == 7)
                    material = field;
                if (intCountString == 8)
                    auftragsnummerdesc = field;
                if (intCountString == 9)
                    kundennamedesc = field;
                if (intCountString == 10)
                    lsnummer = field;

                if (intCountString == 11)
                {
                    laenge = field;
                    List<string> listk = new List<string>();
                    for (int counter = 0; counter < 2; counter++)
                    {
                        Console.WriteLine(laenge[counter].ToString());
                        if (counter == 0)
                        {
                            listk.Add(laenge[counter].ToString() + ".");
                        }
                        else
                            listk.Add(laenge[counter].ToString());
                    }
                    kk = listk.ToArray();
                    resultK = double.Parse(String.Join("", kk), System.Globalization.CultureInfo.InvariantCulture);
                    Console.WriteLine(resultK);
                }

                if (intCountString == 13)
                {
                    breite = field;
                    List<string> listm = new List<string>();
                    for (int counter = 0; counter < 2; counter++)
                    {
                        Console.WriteLine(breite[counter].ToString());
                        if (counter == 0)
                        {
                            listm.Add(breite[counter].ToString() + ".");
                        }
                        else
                            listm.Add(breite[counter].ToString());
                    }
                    mm = listm.ToArray();
                    resultM = double.Parse(String.Join("", mm), System.Globalization.CultureInfo.InvariantCulture);
                    Console.WriteLine(resultM);
                }

                if (intCountString == 14)
                    amountdesc = field;
                if (intCountString == 15)
                    platedesc = field;
                if (intCountString == 16)
                {
                    dicke = field;
                    List<string> listp = new List<string>();
                    for (int counter = 0; counter < 1; counter++)
                    {
                        Console.WriteLine(dicke[counter].ToString());
                        if (counter == 0)
                        {
                            listp.Add(dicke[counter].ToString() + ".");
                        }
                        else
                            listp.Add(dicke[counter].ToString());
                    }
                    pp = listp.ToArray();
                    resultP = double.Parse(String.Join("", pp), System.Globalization.CultureInfo.InvariantCulture);
                    Console.WriteLine(resultP);
                }
                intCountString++;
            }

            plate = new PlateData();
            plate.PlName = platedesc;
            plate.PlTyp = 1;
            plate.Amount = int.Parse(amountdesc);
            plate.ArNumber = 90;
            plate.CreationDate = DateTime.Now;
            DateTime today = DateTime.Today;
            plate.ErstellungsDatum = StringHelper.ToInt(today.ToString("yyyyMMdd"));
            plate.MaterialName = material;
            plate.Bezeichnung = kundennamedesc;
            plate.Bemerk3 = lieferant;
            plate.PlThick = resultP;
            plate.PlThickString = dicke;
            plate.ReferenceNr = ApplicationConfigurationInfo.Instance.NcNumber;
            string[] userName = new string[2];
            today = DateTime.Today;
            userName[0] = today.ToString("yyyyMMdd");
            userName[1] = ApplicationConfigurationInfo.Instance.UserName;
            plate.Bemerk2 = string.Join("; ", userName);
            plate.MachineNummer = ApplicationConfigurationInfo.Instance.RestMachineNumber;
            plate.BestellNummer = bestellnummer;
            plate.BestellDatum = bestelldatum;
            plate.Bemerk3 = lieferant;
            plate.BestellPos = posdesc;
            plate.AuftragsNummer = auftragsnummerdesc;
            plate.KundenName = kundennamedesc;
            plate.LSnummer = lsnummer;
            plate.MaxX = resultK;
            plate.MaxY = resultM;
            plate.MaxXString = laenge;
            plate.MaxYString = breite;
            plate.MatDesc = matdesc;
            

            //plate.ResultThumb = "/Images/maus.ico";
            Console.WriteLine("platename  " + material);
            Console.WriteLine("connect  " + readablePhrase);

            AuftragsDataViewModel._auftragsDataViewModel.PlateData.Add(plate);
            AuftragsDataViewModel._auftragsDataViewModel.GridMaterialPool.ItemsSource = AuftragsDataViewModel._auftragsDataViewModel.PlateData;

        }


        internal void OpenSQLMaterialDataConnection(AuftragsDataViewModel auftragsDataViewModel)
        {
            auftragsDataViewModel.PlateData = new ObservableCollection<PlateData>();
            CultureInfo provider = CultureInfo.InvariantCulture;

            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "H:")
                connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();
                if (AuftragsDataControl._AuftragsDataControl.xHideMuster.IsChecked == true)
                {
                    sqlQuery = "Select TOP 1000 mpid, pl_name, ident_nr , bezeichnung, bemerk_1, bemerk_2, bemerk_3, pl_thick, max_x , max_y , mat_number, pl_typ, ar_number, res1 , res2 , res3 , amount, freigabe, lagerplatz, bevorzug, res4, res5, machine, bew_factor," +
                        " min_x, min_y, step_x, step_y, amount_res, locked, pc_name, filter, bemerk_4, bemerk_5, bemerk_6, bemerk_7, bemerk_8, bemerk_9, bemerk_10, bemerk_11, bemerk_12, bemerk_13, bemerk_14, bemerk_15, bemerk_16, bemerk_17, bemerk_18, bemerk_19, bemerk_20," +
                        " referenceNr, LagerplatzString, MachineNummer, UeberbuchungErlaubt, MeldeBestand, ErstellungsDatum, IsDeleted, DeleteDate, ModifiedDate, CreationDate, (max_x / 1000) * (max_y / 1000) * amount as plflaeche," +
                        " (max_x / 1000) * (max_y / 1000) * pl_thick * amount  as plgewicht, (max_x / 1000) * (max_y / 1000) * amount / 100 * 80 as plnutzfl from dbo.w_matpool where amount NOT LIKE 999999";
                }
                else if (AuftragsDataControl._AuftragsDataControl.xHideMuster.IsChecked == false)
                {
                    sqlQuery = "Select TOP 1000 mpid, pl_name, ident_nr , bezeichnung, bemerk_1, bemerk_2, bemerk_3, pl_thick, max_x , max_y , mat_number, pl_typ, ar_number, res1 , res2 , res3 , amount, freigabe, lagerplatz, bevorzug, res4, res5, machine, bew_factor," +
                    " min_x, min_y, step_x, step_y, amount_res, locked, pc_name, filter, bemerk_4, bemerk_5, bemerk_6, bemerk_7, bemerk_8, bemerk_9, bemerk_10, bemerk_11, bemerk_12, bemerk_13, bemerk_14, bemerk_15, bemerk_16, bemerk_17, bemerk_18, bemerk_19, bemerk_20," +
                    " referenceNr, LagerplatzString, MachineNummer, UeberbuchungErlaubt, MeldeBestand, ErstellungsDatum, IsDeleted, DeleteDate, ModifiedDate, CreationDate, (max_x / 1000) * (max_y / 1000) * amount as plflaeche," +
                    " (max_x / 1000) * (max_y / 1000) * pl_thick * amount  as plgewicht, (max_x / 1000) * (max_y / 1000) * amount / 100 * 80 as plnutzfl  from dbo.w_matpool";
                }
                sqlcmd = new SqlCommand(sqlQuery, connection);
                sqlda = new SqlDataAdapter(sqlcmd);
                using (SqlDataReader reader = sqlcmd.ExecuteReader())
                {
                    var ordinals = new
                    {
                        mpid = reader.GetOrdinal("mpid"),
                        PlName = reader.GetOrdinal("pl_name"),
                        IdentNr = reader.GetOrdinal("ident_nr"),
                        Bezeichnung = reader.GetOrdinal("bezeichnung"),
                        Bemerk1 = reader.GetOrdinal("bemerk_1"),
                        Bemerk2 = reader.GetOrdinal("bemerk_2"),
                        Bemerk3 = reader.GetOrdinal("bemerk_3"),
                        PlThick = reader.GetOrdinal("pl_thick"),
                        MaxX = reader.GetOrdinal("max_x"),
                        MaxY = reader.GetOrdinal("max_y"),
                        MatNr = reader.GetOrdinal("mat_number"),
                        PlTyp = reader.GetOrdinal("pl_typ"),
                        ArNumber = reader.GetOrdinal("ar_number"),
                        Res1 = reader.GetOrdinal("res1"),
                        Res2 = reader.GetOrdinal("res2"),
                        Res3 = reader.GetOrdinal("res3"),
                        Amount = reader.GetOrdinal("amount"),
                        Freigabe = reader.GetOrdinal("freigabe"),
                        Lagerplatz = reader.GetOrdinal("lagerplatz"),
                        Bevorzug = reader.GetOrdinal("bevorzug"),
                        DeleteDate = reader.GetOrdinal("DeleteDate"),
                        ModifiedDate = reader.GetOrdinal("ModifiedDate"),
                        CreationDate = reader.GetOrdinal("CreationDate"),
                        ErstellungsDatum = reader.GetOrdinal("ErstellungsDatum"),
                        PlFlaeche = reader.GetOrdinal("plflaeche"),
                        PlNutzFl = reader.GetOrdinal("plnutzfl"),
                        PlGewicht = reader.GetOrdinal("plgewicht")

                    };

                    while (reader.Read() == true)
                    {
                        var temp = new PlateData();
                        temp.mpid = reader.GetInt32(ordinals.mpid);
                        temp.PlName = reader.GetString(ordinals.PlName);
                        temp.PlIdentNr = reader.GetString(ordinals.IdentNr);
                        temp.Bezeichnung = reader.GetString(ordinals.Bezeichnung);
                        if (!reader.IsDBNull(ordinals.Bemerk1))
                            temp.Bemerk1 = reader.GetString(ordinals.Bemerk1);
                        else temp.Bemerk1 = string.Empty;
                        if (!reader.IsDBNull(ordinals.Bemerk2))
                            temp.Bemerk2 = reader.GetString(ordinals.Bemerk2);
                        else temp.Bemerk2 = string.Empty;
                        if (!reader.IsDBNull(ordinals.Bemerk3))
                            temp.Bemerk3 = reader.GetString(ordinals.Bemerk3);
                        else temp.Bemerk3 = string.Empty;
                        if (!reader.IsDBNull(ordinals.PlThick))
                            temp.PlThick = reader.GetDouble(ordinals.PlThick);
                        else temp.PlThick = 2.00;
                        if (!reader.IsDBNull(ordinals.Lagerplatz))
                            temp.Lagerplatz = reader.GetInt32(ordinals.Lagerplatz);
                        else temp.Lagerplatz = 99;
                        if (!reader.IsDBNull(ordinals.MaxX))
                            temp.MaxX = reader.GetDouble(ordinals.MaxX);
                        else temp.MaxX = 2500.00;
                        if (!reader.IsDBNull(ordinals.MaxY))
                            temp.MaxY = reader.GetDouble(ordinals.MaxY);
                        else temp.MaxY = 1250.00;
                        if (!reader.IsDBNull(ordinals.MatNr))
                            temp.MatNr = reader.GetInt32(ordinals.MatNr);
                        else temp.MatNr = 20;
                        temp.MaterialName = switchMaterials(temp.MatNr);

                        if (!reader.IsDBNull(ordinals.PlTyp))
                            temp.PlTyp = reader.GetInt32(ordinals.PlTyp);
                        else temp.PlTyp = 1;
                        if (!reader.IsDBNull(ordinals.ArNumber))
                            temp.ArNumber = reader.GetInt32(ordinals.ArNumber);
                        else temp.ArNumber = 90;

                        if (!reader.IsDBNull(ordinals.Amount))
                            temp.Amount = reader.GetInt32(ordinals.Amount);
                        else temp.Amount = 0;

                        if (!reader.IsDBNull(ordinals.ErstellungsDatum))
                            temp.ErstellungsDatum = reader.GetInt32(ordinals.ErstellungsDatum);
                        else temp.ErstellungsDatum = 2024;

                        if (!reader.IsDBNull(ordinals.PlFlaeche))
                        {
                            temp.PlFlaeche = reader.GetDouble(ordinals.PlFlaeche);
                            if (temp.Amount == 999999)
                                temp.PlFlaeche = 0.0;
                        }
                        else temp.PlFlaeche = 0.0;

                        if (!reader.IsDBNull(ordinals.PlNutzFl))
                        {
                            temp.PlNutzFl = reader.GetDouble(ordinals.PlNutzFl);
                            if (temp.Amount == 999999)
                                temp.PlNutzFl = 0.0;
                        }
                        else temp.PlNutzFl = 0.0;

                        if (!reader.IsDBNull(ordinals.PlGewicht))
                        {
                            temp.PlGewicht = reader.GetDouble(ordinals.PlGewicht);
                            if (temp.MaterialName.Contains("mg"))
                                temp.PlGewicht = Truncate(temp.PlGewicht * 2.75, 8);
                            if (temp.MaterialName.Contains("38") || temp.MaterialName.Contains("33") || temp.MaterialName.Contains("35") || temp.MaterialName.Contains("1.4"))
                                temp.PlGewicht = Truncate(temp.PlGewicht * 7.85, 8);
                            if (temp.Amount == 999999)
                                temp.PlGewicht = 0;
                        }
                        else temp.PlGewicht = 0;

                        if (!reader.IsDBNull(ordinals.DeleteDate))
                        {
                            temp.DeleteDate = reader.GetDateTime(ordinals.DeleteDate);
                        }
                        else temp.DeleteDate = DateTime.Today;

                        if (!reader.IsDBNull(ordinals.ModifiedDate))
                        {
                            temp.ModifiedDate = reader.GetDateTime(ordinals.ModifiedDate);
                        }
                        else temp.ModifiedDate = DateTime.Today;

                        if (!reader.IsDBNull(ordinals.CreationDate))
                        {
                            temp.CreationDate = reader.GetDateTime(ordinals.CreationDate);
                        }
                        else temp.CreationDate = DateTime.Today;

                        //if (!reader.IsDBNull(ordinals.verpacken))
                        //{
                        //    string parsedate = reader.GetString(ordinals.verpacken);
                        //    DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        //    temp.verpacken = parsedTime;
                        //}
                        //else temp.verpacken = DateTime.Today;

                        //if (!reader.IsDBNull(ordinals.komplettieren))
                        //{
                        //    string parsedate = reader.GetString(ordinals.komplettieren);
                        //    DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        //    temp.komplettieren = parsedTime;
                        //}
                        //else temp.komplettieren = DateTime.Today;

                        //if (!reader.IsDBNull(ordinals.oberflaeche))
                        //{
                        //    string parsedate = reader.GetString(ordinals.oberflaeche);
                        //    DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        //    temp.oberflaeche = parsedTime;
                        //}
                        //else temp.oberflaeche = DateTime.Today;
                        //       temp.ResultThumb = "/Images/maus.ico";
                        //AuftragsDataControl._AuftragsDataControl.caltemplate.calendar.SelectedDate = avtermindate;
                        auftragsDataViewModel.PlateData.Add(temp);
                        //AuftragsDataControl._AuftragsDataControl.calendar.SelectedDate = "25.07.2024";
                    }


                }


                auftragsDataViewModel.GridMaterialPool.SelectedValuePath = "PlName";
                auftragsDataViewModel.GridMaterialPool.DisplayMemberPath = "PlIdentNr";
                auftragsDataViewModel.GridMaterialPool.ItemsSource = auftragsDataViewModel.PlateData;

                //System.Data.DataTable dt = new System.Data.DataTable("auftragsnummer");
                //sqlda.Fill(dt);
                //GridOrders.ItemsSource = dt.DefaultView;
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Connect(PlateData item)
        {
            string bestelldatum = ""; string lieferant = ""; string posdesc = "";
            string matdesc = ""; string datedesc = ""; string material = "";
            string auftragsnummerdesc = ""; string kundennamedesc = ""; string lsnummer = "";
            string laenge = ""; string l = ""; string breite = "";
            string amountdesc = ""; string platedesc = ""; string dicke = ""; string q = "";
            DateTime dToday = DateTime.Today;

            lieferant = item.Bemerk3; lsnummer = item.LSnummer; material = item.MaterialName; dicke = item.PlThickString; laenge = item.MaxXString; breite = item.MaxYString;
            datedesc = dToday.ToString(); matdesc = item.MatDesc; kundennamedesc = item.KundenName; platedesc = item.PlName; posdesc = item.BestellPos; auftragsnummerdesc = item.AuftragsNummer;
            amountdesc = item.Amount.ToString(); bestelldatum = item.BestellDatum;

            //string[] dataFieldsList = aa.Split(';');
            //foreach (string field in dataFieldsList)
            //{
            //    Console.WriteLine(field);

            //    if (iiii == 2)
            //        bestelldatum = field;
            //    if (iiii == 3)
            //        lieferant = field;
            //    if (iiii == 4)
            //        posdesc = field;
            //    if (iiii == 5)
            //        matdesc = field;
            //    if (iiii == 6)
            //        datedesc = field;
            //    if (iiii == 7)
            //        material = field;
            //    if (iiii == 8)
            //        auftragsnummerdesc = field;
            //    if (iiii == 9)
            //        kundennamedesc = field;
            //    if (iiii == 10)
            //        lsnummer = field;

            //    if (iiii == 11)
            //    {
            //        laenge = field;
            //        List<string> listk = new List<string>();
            //        for (int counter = 0; counter < 2; counter++)
            //        {
            //            Console.WriteLine(laenge[counter].ToString());
            //            if (counter == 0)
            //            {
            //                listk.Add(laenge[counter].ToString() + ".");
            //            }
            //            else
            //                listk.Add(laenge[counter].ToString());
            //        }
            //        kk = listk.ToArray();
            //        resultK = double.Parse(String.Join("", kk), System.Globalization.CultureInfo.InvariantCulture);
            //        Console.WriteLine(resultK);
            //    }

            //    if (iiii == 13)
            //    {
            //        breite = field;
            //        List<string> listm = new List<string>();
            //        for (int counter = 0; counter < 2; counter++)
            //        {
            //            Console.WriteLine(breite[counter].ToString());
            //            if (counter == 0)
            //            {
            //                listm.Add(breite[counter].ToString() + ".");
            //            }
            //            else
            //                listm.Add(breite[counter].ToString());
            //        }
            //        mm = listm.ToArray();
            //        resultM = double.Parse(String.Join("", mm), System.Globalization.CultureInfo.InvariantCulture);
            //        Console.WriteLine(resultM);
            //    }

            //    if (iiii == 14)
            //        amountdesc = field;
            //    if (iiii == 15)
            //        platedesc = field;
            //    if (iiii == 16)
            //    {
            //        dicke = field;
            //        List<string> listp = new List<string>();
            //        for (int counter = 0; counter < 1; counter++)
            //        {
            //            Console.WriteLine(dicke[counter].ToString());
            //            if (counter == 0)
            //            {
            //                listp.Add(dicke[counter].ToString() + ".");
            //            }
            //            else
            //                listp.Add(dicke[counter].ToString());
            //        }
            //        pp = listp.ToArray();
            //        resultP = double.Parse(String.Join("", pp), System.Globalization.CultureInfo.InvariantCulture);
            //        Console.WriteLine(resultP);
            //    }
            //    iiii++;
            //}
            Console.WriteLine(lieferant + " + " + datedesc + "  +  " + lsnummer + "  +  " + material + " + " + laenge + " + " + breite + " + " + dicke + " pos " + posdesc + " + " + matdesc);
            string[] blechbestellung = { lieferant, lsnummer + "-" + posdesc, material, laenge + "x" + breite + "x" + dicke, datedesc, matdesc, kundennamedesc };

            if (material.Contains("DX51"))
            {
                material = "1";
                dichte = 7.85;
                Console.WriteLine("stahlverzinkt" + blechbestellung);
                //     File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "StVerzinkt.txt", blechbestellung);
            }
            if (material.Contains("1.4301"))
            {
                material = "10";
                dichte = 7.85;
                Console.WriteLine("vaqual " + blechbestellung);
                //     File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4401"))
            {
                material = "11";
                dichte = 7.85;
                Console.WriteLine("vaqual " + blechbestellung);
                //    File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4404"))
            {
                material = "13";
                dichte = 7.85;
                Console.WriteLine("vaqual " + blechbestellung);
                //    File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4571"))
            {
                material = "14";
                dichte = 7.85;
                Console.WriteLine("vaqual " + blechbestellung);
                //   File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "VA.txt", blechbestellung);
            }
            if (material.Contains("Almg1") && material.Contains("Normalqual") && !matdesc.Contains("E6/EV1"))
            {
                material = "20";
                dichte = 2.7;
                Console.WriteLine("normalqual " + blechbestellung);
                //   File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "NQ.txt", blechbestellung);
            }
            if (material.Contains("ALMG1") && material.Contains("Normalqual") && !matdesc.Contains("E6/EV1"))
            {
                material = "20";
                dichte = 2.7;
                Console.WriteLine("normalqual " + blechbestellung);
                //    File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "NQ.txt", blechbestellung);
            }
            if (material.Contains("Almg1") && material.Contains("Eloxalqual") && !matdesc.Contains("E6/EV1"))
            {
                material = "21";
                dichte = 2.7;
                Console.WriteLine("eloxalqual " + blechbestellung);
                //   File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "EQ.txt", blechbestellung);
            }
            if (material.Contains("ALMG1") && material.Contains("Eloxalqual") && !matdesc.Contains("E6/EV1"))
            {
                material = "21";
                dichte = 2.7;
                Console.WriteLine("eloxalqual " + blechbestellung);
                //  File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "EQ.txt", blechbestellung);
            }
            if (matdesc.Contains("E6/EV1"))
            {
                material = "92";
                dichte = 2.7;
                Console.WriteLine("BANDELOX " + blechbestellung);
                // File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "Elox.txt", blechbestellung);
            }
            if (material.Contains("bandeloxiert"))
            {
                material = "92";
                dichte = 2.7;
                Console.WriteLine("BANDELOX " + blechbestellung);
                // File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + d + "Elox.txt", blechbestellung);
            }
            double weight = resultM * resultK * resultP * dichte;
            double flaeche = resultM * resultK;

            Console.WriteLine(platedesc + lsnummer + auftragsnummerdesc + kundennamedesc + bestelldatum + datedesc + lieferant + dicke + laenge + .000 + breite + .000 + material + amountdesc);
            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();

                string sql = "INSERT INTO dbo.w_matpool (pl_name, ident_nr , bezeichnung, bemerk_1, bemerk_2, bemerk_3, pl_thick, max_x , max_y , mat_number, pl_typ, ar_number, res1 , res2 , res3 , amount, freigabe, lagerplatz, bevorzug, res4, res5, machine, bew_factor," +
                    " min_x, min_y, step_x, step_y, amount_res, locked, pc_name, filter, bemerk_4, bemerk_5, bemerk_6, bemerk_7, bemerk_8, bemerk_9, bemerk_10, bemerk_11, bemerk_12, bemerk_13, bemerk_14, bemerk_15, bemerk_16, bemerk_17, bemerk_18, bemerk_19, bemerk_20," +
                    " referenceNr, LagerplatzString, MachineNummer, UeberbuchungErlaubt, MeldeBestand, ErstellungsDatum, IsDeleted, DeleteDate, ModifiedDate, CreationDate) VALUES (@pl_name,@ident_nr,@bezeichnung,@bemerk_1,@bemerk_2," +
                    " @bemerk_3,@pl_thick, @max_x, @max_y,@mat_number,@pl_typ,@ar_number,@res1,@res2,@res3,@amount,@freigabe,@lagerplatz,@bevorzug,@res4,@res5,@machine,@bew_factor,@min_x,@min_y,@step_x,@step_y,@amount_res,@locked, @pc_name," +
                    " @filter,@bemerk_4,@bemerk_5, @bemerk_6,@bemerk_7,@bemerk_8,@bemerk_9,@bemerk_10,@bemerk_11, @bemerk_12, @bemerk_13, @bemerk_14, @bemerk_15,@bemerk_16,@bemerk_17, @bemerk_18,@bemerk_19,@bemerk_20, @referenceNr, @LagerplatzString," +
                    " @MachineNummer, @UeberbuchungErlaubt, @MeldeBestand, @ErstellungsDatum, @IsDeleted,@DeleteDate, @ModifiedDate, @CreationDate)";
                SqlCommand cmd = new SqlCommand(sql, connection);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.InsertCommand = new SqlCommand(sql, connection); // Insert (erstellen/einfügen) Command erstellen
                adapter.InsertCommand.Parameters.AddWithValue("@pl_name", platedesc);
                adapter.InsertCommand.Parameters.AddWithValue("@ident_nr", lsnummer + "-" + posdesc);
                adapter.InsertCommand.Parameters.AddWithValue("@bezeichnung", auftragsnummerdesc + "-" + kundennamedesc);
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_1", bestelldatum);
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_2", datedesc);
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_3", lieferant);
                adapter.InsertCommand.Parameters.AddWithValue("@pl_thick", dicke);
                adapter.InsertCommand.Parameters.AddWithValue("@max_x", laenge + .000);
                adapter.InsertCommand.Parameters.AddWithValue("@max_y", breite + .000);
                adapter.InsertCommand.Parameters.AddWithValue("@mat_number", material);
                adapter.InsertCommand.Parameters.AddWithValue("@pl_typ", 1);
                adapter.InsertCommand.Parameters.AddWithValue("@ar_number", 90);
                adapter.InsertCommand.Parameters.AddWithValue("@res1", 0.0);
                adapter.InsertCommand.Parameters.AddWithValue("@res2", flaeche);
                adapter.InsertCommand.Parameters.AddWithValue("@res3", weight);
                adapter.InsertCommand.Parameters.AddWithValue("@amount", amountdesc);
                adapter.InsertCommand.Parameters.AddWithValue("@freigabe", 1);
                adapter.InsertCommand.Parameters.AddWithValue("@lagerplatz", 99);
                adapter.InsertCommand.Parameters.AddWithValue("@bevorzug", 0.0);
                adapter.InsertCommand.Parameters.AddWithValue("@res4", 1.1);
                adapter.InsertCommand.Parameters.AddWithValue("@res5", 1.1);
                adapter.InsertCommand.Parameters.AddWithValue("@machine", 0001);
                adapter.InsertCommand.Parameters.AddWithValue("@bew_factor", 1.000);
                adapter.InsertCommand.Parameters.AddWithValue("@min_x", 0.000);
                adapter.InsertCommand.Parameters.AddWithValue("@min_y", 0.000);
                adapter.InsertCommand.Parameters.AddWithValue("@step_x", 0.000);
                adapter.InsertCommand.Parameters.AddWithValue("@step_y", 0.000);
                adapter.InsertCommand.Parameters.AddWithValue("@amount_res", 0);
                adapter.InsertCommand.Parameters.AddWithValue("@locked", 0);
                adapter.InsertCommand.Parameters.AddWithValue("@pc_name", "default");
                adapter.InsertCommand.Parameters.AddWithValue("@filter", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_4", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_5", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_6", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_7", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_8", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_9", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_10", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_11", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_12", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_13", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_14", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_15", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_16", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_17", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_18", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_19", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_20", "abc");

                adapter.InsertCommand.Parameters.AddWithValue("@referenceNr", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@LagerplatzString", "Bestellung");
                adapter.InsertCommand.Parameters.AddWithValue("@MachineNummer", "1;2");
                adapter.InsertCommand.Parameters.AddWithValue("@UeberbuchungErlaubt", 0);
                adapter.InsertCommand.Parameters.AddWithValue("@MeldeBestand", 0);
                adapter.InsertCommand.Parameters.AddWithValue("@ErstellungsDatum", DateTime.Now.ToString("MMddyyyy"));
                adapter.InsertCommand.Parameters.AddWithValue("@IsDeleted", 0);
                adapter.InsertCommand.Parameters.AddWithValue("@DeleteDate", 2023);
                adapter.InsertCommand.Parameters.AddWithValue("@ModifiedDate", 2023);
                adapter.InsertCommand.Parameters.AddWithValue("@CreationDate", 2023);

                adapter.InsertCommand.ExecuteNonQuery(); // Command ausführen

                // adapter.DeleteCommand
                // adapter.UpdateCommand

                // Alle Datenbank zugehörigen Objekte schließen

                //reader.Close();
                cmd.Dispose();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void ChangeOrderedParts(JobHelper jobHelper)
        {
            int partsCount = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData.Count;

            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=.\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";

            string _bemerkungen = "null";
            string _orderpos = "0";
            string _assembly = "Keine";
            string _gravur = "Keine";
            string _oberflaeche = "Keine";
            string _number = "0";
            string artikel = "0";
            string ncfile = "0";
            string material = "0";
            string kunde = "";

            int? matnr = 0;
            int? anzahl = 0;
            int count = 0;
            double? laenge = 0.0;
            double? breite = 0.0;
            double? dicke = 0.0;

            for (int j = 0; j <= partsCount - 1; j++)
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Auftrag != null)
                    count = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Auftrag.ToString().Count();
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Bemerkungen != null)
                    _bemerkungen = Convert.ToString(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Bemerkungen);
                else
                    _bemerkungen = "Keine";

                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].OrderPos != null)
                    _orderpos = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].OrderPos;
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].AssemblyName != null)
                    _assembly = Convert.ToString(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].AssemblyName);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Gravur != null)
                    _gravur = Convert.ToString(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Gravur);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Oberflaeche != null)
                    _oberflaeche = Convert.ToString(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Oberflaeche);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Auftrag != null)
                    _number = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Auftrag.ToString().Remove(6, count - 6);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Material != null)
                    material = Convert.ToString(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Material);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].MaterialInt != null)
                    matnr = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].MaterialInt; //switchStringMaterials(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Material);

                int? idb050 = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].IDB050;
                int? release = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Release;
                int? status = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Status;
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].PartName != null)
                    ncfile = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].PartName;
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Anzahl != null)
                    anzahl = int.Parse(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Anzahl);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Artikel != null)
                    artikel = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Artikel;
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Laenge != null)
                    laenge = double.Parse(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Laenge);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Breite != null)
                    breite = double.Parse(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Breite);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Dicke != null)
                    dicke = double.Parse(AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Dicke);
                if (AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Kunde != null)
                    kunde = AuftragsDataViewModel._auftragsDataViewModel.PartOrderData[j].Kunde;

                string sql = "UPDATE wicam.dbo.OrderedParts SET NC_FILE=@ncfile, POSITION=@orderpos, NUMBER=@number, Amount=@amount, MATERIAL_NO=@matnr, RELEASE=@release, STATUS=@status, " +
                    "THICKNESS=@dicke, DIMENSION_X=@laenge, DIMENSION_Y=@breite, REMARK_1=@bemerkungen, REMARK_2=@material, REMARK_3=@artikel, REMARK_4=@oberflaeche, REMARK_5=@assembly, REMARK_6=@gravur, REMARK_7=@kunde   WHERE IDB050=@idb050";
                Console.WriteLine("ID  " + idb050 + " Auftrag  " + _number + "  NCFILE  " + ncfile + "  Pos  " + _orderpos + "  BG  " + _assembly + "  Artikel  " + artikel + "  Anzahl  " + anzahl + "  L  " + laenge + "  B  " + breite + "  D  " + dicke + "  OF  " + _oberflaeche + "  Gravur  " + _gravur + "  NC  " + release + "  Tru   " + status);

                //  "SET  , VALUES (@ WHERE IDB050 = " + _PartOrderGridSelectedItemProperty.IDB050;
                using (SqlCommand cmd =
                      new SqlCommand(sql, connection))
                    try
                    {
                        cmd.Parameters.AddWithValue("@idb050", idb050);
                        cmd.Parameters.AddWithValue("@ncfile", ncfile);
                        cmd.Parameters.AddWithValue("@orderpos", _orderpos);
                        cmd.Parameters.AddWithValue("@number", _number);
                        cmd.Parameters.AddWithValue("@amount", anzahl);
                        cmd.Parameters.AddWithValue("@matnr", matnr);

                        cmd.Parameters.AddWithValue("@release", release);
                        cmd.Parameters.AddWithValue("@status", status);

                        cmd.Parameters.AddWithValue("@dicke", dicke);
                        cmd.Parameters.AddWithValue("@laenge", laenge);
                        cmd.Parameters.AddWithValue("@breite", breite);
                        cmd.Parameters.AddWithValue("@bemerkungen", _bemerkungen);
                        cmd.Parameters.AddWithValue("@material", material);
                        cmd.Parameters.AddWithValue("@artikel", artikel);
                        cmd.Parameters.AddWithValue("@oberflaeche", _oberflaeche);
                        cmd.Parameters.AddWithValue("@assembly", _assembly);
                        cmd.Parameters.AddWithValue("@gravur", _gravur);
                        cmd.Parameters.AddWithValue("@kunde", kunde);

                        //add whatever parameters you required to update here
                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine("ID  " + AuftragsDataViewModel._auftragsDataViewModel.PartOrderGridSelectedItemProperty.IDB050);
                    }
                    catch (SqlException ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
            }
            AuftragsDataControl._AuftragsDataControl.xFeedBack.Text = "Zu Auftrag " + _number + " wurden " + partsCount + " - Teile in der Datenbank aktualisiert.";
        }


        public void CreateOrderedParts(JobHelper jobHelper)
        {
            int partsCount = jobHelper.PartsOrderData.Count;
            Console.WriteLine(partsCount);
            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";

            string _bemerkungen = "null";
            string _orderpos = "0";
            string _assembly = "Keine";
            string _gravur = "Keine";
            string _oberflaeche = "Keine";

            for (int j = 0; j <= partsCount - 1; j++)
            {

                if (jobHelper.PartsOrderData[j].Bemerkungen != null)
                    _bemerkungen = Convert.ToString(jobHelper.PartsOrderData[j].Bemerkungen);
                else
                    _bemerkungen = "Keine";

                if (jobHelper.PartsOrderData[j].OrderPos != null)
                    _orderpos = jobHelper.PartsOrderData[j].OrderPos;
                if (jobHelper.PartsOrderData[j].AssemblyName != null)
                    _assembly = Convert.ToString(jobHelper.PartsOrderData[j].AssemblyName);
                if (jobHelper.PartsOrderData[j].Gravur != null)
                    _gravur = Convert.ToString(jobHelper.PartsOrderData[j].Gravur);
                if (jobHelper.PartsOrderData[j].Oberflaeche != null)
                    _oberflaeche = Convert.ToString(jobHelper.PartsOrderData[j].Oberflaeche);
                int matnr = switchStringMaterials(jobHelper.PartsOrderData[j].Material);
                try
                {
                    SqlConnection connection = new SqlConnection(connectionString);

                    connection.Open();

                    string sql = "INSERT INTO dbo.OrderedParts ([NC_FILE] ,[POSITION] ,[NUMBER] ,[REMARK] ,[AMOUNT] ,[NESTING_ROT] ,[NC_EXIST] ,[MATERIAL_NO] ,[MACHINE_NO] ,[NC_ARCHIV] ,[PRIORITY] ,[RELEASE] ,[STATUS] " +
                        ",[THICKNESS] ,[DIMENSION_X] ,[DIMENSION_Y] ,[ORDER_DATE] ,[RELEASED_AMOUNT] ,[PRODUCED_AMOUNT] ,[REJECTED_AMOUNT] ,[MAX_AMOUNT] ,[TABLE_NO] ,[STACK_NO] ,[NEST_MIRR] ,[COMMON_CUT] ,[RESERVE_ID] ,[RESERVED_1] ,[RESERVED_2] " +
                        ",[PART_AREA] , [REMARK_1], [REMARK_2], [REMARK_3], [REMARK_4], [REMARK_5], [REMARK_6], [REMARK_7], [locked] ,[pc_name] ,[filter] ,[BAUGRUPPE] ,[COMMISSION_NR] ,[COLOR] ,[CLEANED_UP] ,[StdNestingName] ,[StdNestingArchive] ,[RotationAngle] ,[ModifiedDate] ,[CreationDate])  VALUES (@nc_file,@position,@number,@remark,@amount," +
                        " @nesting_rot,@nc_exist, @material_no, @machine_no,@ncarchiv,@priority,@release,@status,@thickness,@dim_x,@dim_y,@order_date,@released_amount,@produced_amount,@rejected_amount,@max_amount,@table_no,@stack_no,@nest_mirr,@common_cut,@reserve_id," +
                        "@res_1,@res_2,@part_area, @remark_1, @remark_2, @remark_3, @remark_4, @remark_5, @remark_6, @remark_7, @locked, @pc_name,@filter, @baugruppe,@com_nr,@color,@cleaned_up,@nest_name,@nest_arch, @rot_angle, @ModifiedDate, @CreationDate)";
                    SqlCommand cmd = new SqlCommand(sql, connection);

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.InsertCommand = new SqlCommand(sql, connection); // Insert (erstellen/einfügen) Command erstellen
                    adapter.InsertCommand.Parameters.AddWithValue("@nc_file", jobHelper.PartsOrderData[j].PartName);
                    adapter.InsertCommand.Parameters.AddWithValue("@position", _orderpos);
                    adapter.InsertCommand.Parameters.AddWithValue("@number", jobHelper.PartsOrderData[j].Auftrag);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@amount", int.Parse(jobHelper.PartsOrderData[j].Anzahl));
                    adapter.InsertCommand.Parameters.AddWithValue("@nesting_rot", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@nc_exist", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@material_no", matnr);
                    adapter.InsertCommand.Parameters.AddWithValue("@machine_no", 1);
                    adapter.InsertCommand.Parameters.AddWithValue("@ncarchiv", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@priority", 1);
                    adapter.InsertCommand.Parameters.AddWithValue("@release", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@status", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@thickness", double.Parse(jobHelper.PartsOrderData[j].Dicke));
                    adapter.InsertCommand.Parameters.AddWithValue("@dim_x", double.Parse(jobHelper.PartsOrderData[j].Laenge));
                    adapter.InsertCommand.Parameters.AddWithValue("@dim_y", double.Parse(jobHelper.PartsOrderData[j].Breite));
                    adapter.InsertCommand.Parameters.AddWithValue("@order_date", 2023);
                    adapter.InsertCommand.Parameters.AddWithValue("@released_amount", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@produced_amount", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@rejected_amount", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@max_amount", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@table_no", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@stack_no", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@nest_mirr", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@common_cut", 1);
                    adapter.InsertCommand.Parameters.AddWithValue("@reserve_id", 0.0);
                    adapter.InsertCommand.Parameters.AddWithValue("@res_1", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@res_2", 0.0);
                    adapter.InsertCommand.Parameters.AddWithValue("@part_area", 0.0);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_1", _bemerkungen);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_2", Convert.ToString(jobHelper.PartsOrderData[j].Material));
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_3", Convert.ToString(jobHelper.PartsOrderData[j].Artikel));
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_4", _oberflaeche);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_5", _assembly);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_6", _gravur);
                    adapter.InsertCommand.Parameters.AddWithValue("@remark_7", jobHelper.PartsOrderData[j].Kunde);
                    adapter.InsertCommand.Parameters.AddWithValue("@locked", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@pc_name", "abc");
                    adapter.InsertCommand.Parameters.AddWithValue("@filter", "abc");
                    adapter.InsertCommand.Parameters.AddWithValue("@baugruppe", "abc");
                    adapter.InsertCommand.Parameters.AddWithValue("@com_nr", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@color", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@cleaned_up", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@nest_name", "abc");
                    adapter.InsertCommand.Parameters.AddWithValue("@nest_arch", 0);
                    adapter.InsertCommand.Parameters.AddWithValue("@rot_angle", 0.0);
                    adapter.InsertCommand.Parameters.AddWithValue("@ModifiedDate", 2023);
                    adapter.InsertCommand.Parameters.AddWithValue("@CreationDate", 2023);

                    adapter.InsertCommand.ExecuteNonQuery(); // Command ausführen

                    // adapter.DeleteCommand
                    // adapter.UpdateCommand

                    // Alle Datenbank zugehörigen Objekte schließen

                    //reader.Close();
                    cmd.Dispose();
                    connection.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        double Truncate(double value, int places)
        {
            // not sure if you care to handle negative numbers...       
            var f = Math.Pow(10, places);
            return Math.Truncate(value * f) / f;
        }
        public string switchMaterials(int? matNr)
        {
            string _Material = "Almg1";
         switch (matNr) {
                case 1:
                    _Material = "1.0033";
                    return _Material;
                case 2:
                    _Material = "1.0038";
                    return _Material;
                case 4:
                    _Material = "ST37 S 350";
                    return _Material;
                case 5:
                    _Material = "1.0355";
                    return _Material;
                case 10:
                    _Material = "1.4301";
                    return _Material;
                case 11:
                    _Material = "1.4401";
                    return _Material;
                case 12:
                    _Material = "1.4404";
                    return _Material;
                case 13:
                    _Material = "1.4571";
                    return _Material;
                case 14:
                    _Material = "1.4404 Rimex";
                    return _Material;
                case 15:
                    _Material = "1.4404 Korn 240";
                    return _Material;
                case 16:
                    _Material = "1.4301 Korn 240";
                    return _Material;
                case 17:
                    _Material = "1.4404 Korn 320";
                    return _Material;
                case 18:
                    _Material = "1.4462";
                    return _Material;
                case 20:
                    _Material = "Almg1";
                    return _Material;
                case 21:
                    _Material = "Almg3";
                    return _Material;
                case 22:
                    _Material = "Almg4 E6/C0";
                    return _Material;
                case 23:
                    _Material = "Almg 2 Ral";
                    return _Material;
                case 24:
                    _Material = "Almg1 Lochblech";
                    return _Material;
                case 25:
                    _Material = "Almg4 E6/C34";
                    return _Material;
                case 26:
                    _Material = "Almg4 E6/C35";
                    return _Material;
                case 27:
                    _Material = "Almg4 E6/C31";
                    return _Material;
                case 60:
                    _Material = "MESSING";
                    return _Material;
                case 70:
                    _Material = "PVC";
                    return _Material;
                case 91:
                    _Material = "S 350 GD";
                    return _Material;
                case 92:
                    _Material = "Almg4 E0/C0";
                    return _Material;
                case 93:
                    _Material = "1.4301 K240 Elektropoliert";
                    return _Material;
                case 95:
                    _Material = "Almg4 E6/C32";
                    return _Material;
                case 96:
                    _Material = "Almg 2 Ral 7016";
                    return _Material;
                case 97:
                    _Material = "Almg 2 Ral 9004";
                    return _Material;
                case 98:
                    _Material = "1.4404 Glasperl gestrahlt";
                    return _Material;
                case 99:
                    _Material = "Almg 2 Ral 9007";
                    return _Material;
                case 100:
                    _Material = "Almg1 DUETTWARZENBLECH";
                    return _Material;
                case 101:
                    _Material = "Almg 2 Ral 9016";
                    return _Material;
                case 102:
                    _Material = "Almg 2 Ral 9006";
                    return _Material;
                case 103:
                    _Material = "AlmAlmg 2 Ral 8022";
                    return _Material;
                case 104:
                    _Material = "Almg 2 Ral 9010";
                    return _Material;
                case 105:
                    _Material = "Almg 2 Ral 9003";
                    return _Material;
                case 106:
                    _Material = "1.4404 2B R11";
                    return _Material;
                case 107:
                    _Material = "Stahl DX53D";
                    return _Material;
                case 108:
                    _Material = "PVC Schwarz";
                    return _Material;
                case 109:
                    _Material = "ISOLIERPLATTE";
                    return _Material;
                case 110:
                    _Material = "Almg 2 Ral 9003";
                    return _Material;
                case 111:
                    _Material = "Almg 2 Ral 7038";
                    return _Material;
                case 112:
                    _Material = "Almg 2 Ral 7012";
                    return _Material;
                case 113:
                    _Material = "Stahl verz Ral 9003";
                    return _Material;
                case 114:
                    _Material = "1.4404 Spiegelpoliert";
                    return _Material;
                case 115:
                    _Material = "Almg 2 Ral 7044";
                    return _Material;
                case 116:
                    _Material = "PTFE";
                    return _Material;
                case 117:
                    _Material = "RAL WEIß NCS S0502-R50 B";
                    return _Material;
                case 118:
                    _Material = "1.4404 R10";
                    return _Material;
                default:
                    //what you want when nothing is selected
                    return _Material;
            }
        }

        public int switchStringMaterials(string? matNr)
        {
            int _Material = 20;
            switch (matNr)
            {
                case "1.0033":
                    _Material = 1;
                    return _Material;
                case "1.0038":
                    _Material = 2;
                    return _Material;
                case "ST37 S 350":
                    _Material = 4;
                    return _Material;
                case "1.0355":
                    _Material = 5;
                    return _Material;
                case "1.4301":
                    _Material = 10;
                    return _Material;
                case "1.4401":
                    _Material = 11;
                    return _Material;
                case "1.4404":
                    _Material = 12;
                    return _Material;
                case "1.4571":
                    _Material = 13;
                    return _Material;
                case "1.4404 Rimex":
                    _Material = 14;
                    return _Material;
                case "1.4404 Korn 240":
                    _Material = 15;
                    return _Material;
                case "1.4301 Korn 240":
                    _Material = 16;
                    return _Material;
                case "1.4404 Korn 320":
                    _Material = 17;
                    return _Material;
                case "1.4462":
                    _Material = 18;
                    return _Material;
                case "Almg1":
                    _Material = 20;
                    return _Material;
                case "Almg3":
                    _Material = 21;
                    return _Material;
                case "Almg4 E6/C0":
                    _Material = 22;
                    return _Material;
                case "Almg 2 Ral":
                    _Material = 23;
                    return _Material;
                case "Almg1 Lochblech":
                    _Material = 24;
                    return _Material;
                case "Almg4 E6/C34":
                    _Material = 25;
                    return _Material;
                case "Almg4 E6/C35":
                    _Material = 26;
                    return _Material;
                case "Almg4 E6/C31":
                    _Material = 27;
                    return _Material;
                case "MESSING":
                    _Material = 60;
                    return _Material;
                case "PVC":
                    _Material = 70;
                    return _Material;
                case "S 350 GD":
                    _Material = 91;
                    return _Material;
                case "Almg4 E0/C0":
                    _Material = 92;
                    return _Material;
                case "1.4301 K240 Elektropoliert":
                    _Material = 93;
                    return _Material;
                case "Almg4 E6/C32":
                    _Material = 95;
                    return _Material;
                case "Almg 2 Ral 7016":
                    _Material = 96;
                    return _Material;
                case "Almg 2 Ral 9004":
                    _Material = 97;
                    return _Material;
                case "1.4404 Glasperl gestrahlt":
                    _Material = 98;
                    return _Material;
                case "Almg 2 Ral 9007":
                    _Material = 99;
                    return _Material;
                case "Almg1 DUETTWARZENBLECH":
                    _Material = 100;
                    return _Material;
                case "Almg 2 Ral 9016":
                    _Material = 101;
                    return _Material;
                case "Almg 2 Ral 9006":
                    _Material = 102;
                    return _Material;
                case "AlmAlmg 2 Ral 8022":
                    _Material = 103;
                    return _Material;
                case "Almg 2 Ral 9010":
                    _Material = 104;
                    return _Material;
                case "Almg 2 Ral 9003":
                    _Material = 105;
                    return _Material;
                case "1.4404 2B R11":
                    _Material = 106;
                    return _Material;
                case "Stahl DX53D":
                    _Material = 107;
                    return _Material;
                case "PVC Schwarz":
                    _Material = 108;
                    return _Material;
                case "ISOLIERPLATTE":
                    _Material = 109;
                    return _Material;
                case "Almg 2 Ral 90033":
                    _Material = 110;
                    return _Material;
                case "Almg 2 Ral 7038":
                    _Material = 111;
                    return _Material;
                case "Almg 2 Ral 7012":
                    _Material = 112;
                    return _Material;
                case "Stahl verz Ral 9003":
                    _Material = 113;
                    return _Material;
                case "1.4404 Spiegelpoliert":
                    _Material = 114;
                    return _Material;
                case "Almg 2 Ral 7044":
                    _Material = 115;
                    return _Material;
                case "PTFE":
                    _Material = 116;
                    return _Material;
                case "RAL WEIß NCS S0502-R50 B":
                    _Material = 117;
                    return _Material;
                case "1.4404 R10":
                    _Material = 118;
                    return _Material;
                default:
                    //what you want when nothing is selected
                    return _Material;
            }
        }

        public void EtikettenPrint(string print, PlateData item)
        {
            string bestelldatum = ""; string lieferant = ""; string posdesc = "";
            string matdesc = ""; string datedesc = ""; string material = "";
            string auftragsnummerdesc = ""; string kundennamedesc = ""; string lsnummer = "";
            string laenge = ""; string l = ""; string breite = "";
            string amountdesc = ""; string platedesc = ""; string dicke = ""; string q = ""; 

            //  string[] blechbestellung = { lieferant, lsnummer + "-" + posdesc, material, laenge + "x" + breite + "x" + dicke, datedesc, matdesc, kundennamedesc };
            lieferant = item.Bemerk3; lsnummer = item.LSnummer; material = item.MaterialName; dicke = item.PlThickString; laenge = item.MaxXString; breite = item.MaxYString;
            datedesc = item.BestellDatum; matdesc = item.MatDesc; kundennamedesc = item.KundenName; platedesc = item.PlName; posdesc = item.BestellPos;
            Console.WriteLine(lieferant + " + " + datedesc + "  +  " + lsnummer + "  +  " + material + " + " + laenge + " + " + breite + " + " + dicke + " pos " + posdesc + " + " + matdesc);

            string[] blechbestellung = { lieferant, lsnummer + "-" + posdesc, material, laenge + "x" + breite + "x" + dicke, datedesc, matdesc, kundennamedesc };

            if (material.Contains("Eloxalqual") && !matdesc.Contains("E6/EV1"))
            {
                Console.WriteLine("eloxalqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "EQ.txt", blechbestellung);
            }
            if (material.Contains("Normalqual") && !matdesc.Contains("E6/EV1"))
            {
                Console.WriteLine("normalqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "NQ.txt", blechbestellung);
            }
            if (material.Contains("DX51"))
            {
                Console.WriteLine("stahlverzinkt" + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "StVerzinkt.txt", blechbestellung);
            }
            if (material.Contains("1.4301"))
            {
                Console.WriteLine("vaqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4401"))
            {
                Console.WriteLine("vaqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4404"))
            {
                Console.WriteLine("vaqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "VA.txt", blechbestellung);
            }
            if (material.Contains("1.4571"))
            {
                Console.WriteLine("vaqual " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "VA.txt", blechbestellung);
            }
            if (matdesc.Contains("E6/EV1"))
            {
                Console.WriteLine("bandeloxiert " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "Elox.txt", blechbestellung);
            }
            if (material.Contains("bandeloxiert"))
            {
                Console.WriteLine("bandeloxiert " + blechbestellung);
                File.WriteAllLines("P:\\MaterialBestellung\\" + lsnummer + "-" + posdesc + "Elox.txt", blechbestellung);
            }
        }

        public void CreateMatPrintExcel()
        {
            Console.WriteLine("CreateMatPrintExcel");

            Excel.Application xlAppNQ = null;
            Excel.Workbook xlWorkbookNQ = null;
            Excel._Worksheet xlWorksheetNQ = null;
            Excel.Application xlAppEQ = null;
            Excel.Workbook xlWorkbookEQ = null;
            Excel._Worksheet xlWorksheetEQ = null;
            Excel.Application xlAppVA = null;
            Excel.Workbook xlWorkbookVA = null;
            Excel._Worksheet xlWorksheetVA = null;
            Excel.Application xlAppStV = null;
            Excel.Workbook xlWorkbookStV = null;
            Excel._Worksheet xlWorksheetStV = null;
            Excel.Application xlAppElox = null;
            Excel.Workbook xlWorkbookElox = null;
            Excel._Worksheet xlWorksheetElox = null;

            var NQfiles = Directory.EnumerateFiles("P:\\MaterialBestellung\\", "*NQ.txt");
            var EQfiles = Directory.EnumerateFiles("P:\\MaterialBestellung\\", "*EQ.txt");
            var EloXfiles = Directory.EnumerateFiles("P:\\MaterialBestellung\\", "*Elox.txt");
            var VAfiles = Directory.EnumerateFiles("P:\\MaterialBestellung\\", "*VA.txt");
            var STVfiles = Directory.EnumerateFiles("P:\\MaterialBestellung\\", "*StVerzinkt.txt");

            int i = 0;
            int iNQ = 0;
            int iEQ = 0;
            int iVA = 0;
            int iStV = 0;
            int iElox = 0;

            foreach (string currentFile in EloXfiles)
            {
                if (currentFile.Contains("1Elox"))
                {
                    xlAppElox = new Excel.Application();
                    xlWorkbookElox = xlAppElox.Workbooks.Open("P:\\Blechbeschriftung\\Blechbeschriftung_Neu\\AL_vorbesch.xls");
                    xlWorksheetElox = (Excel._Worksheet)xlWorkbookElox.Sheets[1];
                    xlAppElox.Visible = true;
                }
                if (currentFile.Contains("4Elox"))
                {
                    xlWorksheetElox = (Excel._Worksheet)xlWorkbookElox.Sheets[2];
                    xlAppElox.Visible = true;
                }
                if (currentFile.Contains("1Elox") || currentFile.Contains("4Elox"))
                {
                    iElox++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetElox.Cells[2, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetElox.Cells[3, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetElox.Cells[2, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetElox.Cells[3, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetElox.Cells[3, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetElox.Cells[3, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("2Elox") || currentFile.Contains("5Elox"))
                {
                    iElox++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetElox.Cells[6, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetElox.Cells[7, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetElox.Cells[6, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetElox.Cells[7, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetElox.Cells[7, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetElox.Cells[7, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("3Elox") || currentFile.Contains("6Elox"))
                {
                    iElox++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetElox.Cells[10, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetElox.Cells[11, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetElox.Cells[10, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetElox.Cells[11, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetElox.Cells[11, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetElox.Cells[11, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                    iElox = 0;

                    //   xlWorksheetElox.PrintOutEx(1, 2, 1, true);
                }
            }

            foreach (string currentFile in EQfiles)
            {
                i++;
                Console.WriteLine(currentFile);
                if (currentFile.Contains("1EQ"))
                {
                    xlAppEQ = new Excel.Application();
                    xlWorkbookEQ = xlAppEQ.Workbooks.Open("P:\\Blechbeschriftung\\Blechbeschriftung_Neu\\AL_EQ.xls");
                    xlWorksheetEQ = (Excel._Worksheet)xlWorkbookEQ.Sheets[1];
                    xlAppEQ.Visible = true;
                }
                if (currentFile.Contains("4EQ"))
                {
                    xlWorksheetEQ = (Excel._Worksheet)xlWorkbookEQ.Sheets[2];
                    xlAppEQ.Visible = true;
                }
                if (currentFile.Contains("1EQ") || currentFile.Contains("4EQ"))
                {
                    iEQ++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetEQ.Cells[2, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetEQ.Cells[3, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetEQ.Cells[2, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetEQ.Cells[3, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetEQ.Cells[3, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetEQ.Cells[3, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("2EQ") || currentFile.Contains("5EQ"))
                {
                    iEQ++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetEQ.Cells[6, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetEQ.Cells[7, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetEQ.Cells[6, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetEQ.Cells[7, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetEQ.Cells[7, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetEQ.Cells[7, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("3EQ") || currentFile.Contains("6EQ"))
                {
                    iEQ++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetEQ.Cells[10, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetEQ.Cells[11, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetEQ.Cells[10, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetEQ.Cells[11, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetEQ.Cells[11, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetEQ.Cells[11, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                    iEQ = 0;
                }
            }

            foreach (string currentFile in VAfiles)
            {
                i++;
                Console.WriteLine(currentFile);
                if (currentFile.Contains("1VA"))
                {
                    xlAppVA = new Excel.Application();
                    xlWorkbookVA = xlAppVA.Workbooks.Open("P:\\Blechbeschriftung\\Blechbeschriftung_Neu\\VA_Blech.xls");
                    xlWorksheetVA = (Excel._Worksheet)xlWorkbookVA.Sheets[1];
                    xlAppVA.Visible = true;
                }
                if (currentFile.Contains("4VA"))
                {
                    xlWorksheetVA = (Excel._Worksheet)xlWorkbookVA.Sheets[2];
                    xlAppVA.Visible = true;
                }
                if (currentFile.Contains("1VA") || currentFile.Contains("4VA"))
                {
                    iVA++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetVA.Cells[2, 4] = line.Trim();
                            }
                            if (ii == 3)
                            {
                                if (line.Contains("1.4301"))
                                    xlWorksheetVA.Cells[2, 2] = "1.4301";
                                if (line.Contains("1.4401"))
                                    xlWorksheetVA.Cells[2, 2] = "1.4401";
                                if (line.Contains("1.4404"))
                                    xlWorksheetVA.Cells[2, 2] = "1.4404";
                                if (line.Contains("1.4571"))
                                    xlWorksheetVA.Cells[2, 2] = "1.4571";
                            }
                            if (ii == 4)
                            {
                                xlWorksheetVA.Cells[3, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetVA.Cells[2, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetVA.Cells[3, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetVA.Cells[3, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetVA.Cells[3, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }

                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("2VA") || currentFile.Contains("5VA"))
                {
                    iVA++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetVA.Cells[6, 4] = line.Trim();
                            }
                            if (ii == 3)
                            {
                                if (line.Contains("1.4301"))
                                    xlWorksheetVA.Cells[6, 2] = "1.4301";
                                if (line.Contains("1.4401"))
                                    xlWorksheetVA.Cells[6, 2] = "1.4401";
                                if (line.Contains("1.4404"))
                                    xlWorksheetVA.Cells[6, 2] = "1.4404";
                                if (line.Contains("1.4571"))
                                    xlWorksheetVA.Cells[6, 2] = "1.4571";
                            }
                            if (ii == 4)
                            {
                                xlWorksheetVA.Cells[7, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetVA.Cells[6, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetVA.Cells[7, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetVA.Cells[7, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetVA.Cells[7, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("3VA") || currentFile.Contains("6VA"))
                {
                    iVA++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetVA.Cells[10, 4] = line.Trim();
                            }
                            if (ii == 3)
                            {
                                if (line.Contains("1.4301"))
                                    xlWorksheetVA.Cells[10, 2] = "1.4301";
                                if (line.Contains("1.4401"))
                                    xlWorksheetVA.Cells[10, 2] = "1.4401";
                                if (line.Contains("1.4404"))
                                    xlWorksheetVA.Cells[10, 2] = "1.4404";
                                if (line.Contains("1.4571"))
                                    xlWorksheetVA.Cells[10, 2] = "1.4571";
                            }
                            if (ii == 4)
                            {
                                xlWorksheetVA.Cells[11, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetVA.Cells[10, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetVA.Cells[11, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetVA.Cells[11, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetVA.Cells[11, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                    iVA = 0;
                }
            }

            foreach (string currentFile in STVfiles)
            {
                i++;
                Console.WriteLine(currentFile);
                if (currentFile.Contains("1StVerzinkt"))
                {
                    xlAppStV = new Excel.Application();
                    xlWorkbookStV = xlAppStV.Workbooks.Open("P:\\Blechbeschriftung\\Blechbeschriftung_Neu\\Stahl_verz.xls");
                    xlWorksheetStV = (Excel._Worksheet)xlWorkbookStV.Sheets[1];
                    xlAppStV.Visible = true;
                }
                if (currentFile.Contains("4StVerzinkt"))
                {
                    xlWorksheetStV = (Excel._Worksheet)xlWorkbookStV.Sheets[2];
                    xlAppStV.Visible = true;
                }
                if (currentFile.Contains("1StVerzinkt") || currentFile.Contains("4StVerzinkt"))
                {
                    iStV++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetStV.Cells[2, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetStV.Cells[3, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetStV.Cells[2, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetStV.Cells[3, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetStV.Cells[3, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetStV.Cells[3, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("2StVerzinkt") || currentFile.Contains("5StVerzinkt"))
                {
                    iStV++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetStV.Cells[6, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetStV.Cells[7, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetStV.Cells[6, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetStV.Cells[7, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetStV.Cells[7, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetStV.Cells[7, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("3StVerzinkt") || currentFile.Contains("6StVerzinkt"))
                {
                    iStV++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetStV.Cells[10, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetStV.Cells[11, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetStV.Cells[10, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetStV.Cells[11, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetStV.Cells[11, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetStV.Cells[11, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                    iStV = 0;
                }
            }

            foreach (string currentFile in NQfiles)
            {
                i++;
                Console.WriteLine(currentFile);

                if (currentFile.Contains("1NQ"))
                {
                    xlAppNQ = new Excel.Application();
                    xlWorkbookNQ = xlAppNQ.Workbooks.Open("P:\\Blechbeschriftung\\Blechbeschriftung_Neu\\AL_NQ.xls");
                    xlWorksheetNQ = (Excel._Worksheet)xlWorkbookNQ.Sheets[1];
                    xlAppNQ.Visible = true;
                }

                if (currentFile.Contains("4NQ"))
                {
                    xlWorksheetNQ = (Excel._Worksheet)xlWorkbookNQ.Sheets[2];
                    xlAppNQ.Visible = true;
                }

                if (currentFile.Contains("1NQ") || currentFile.Contains("4NQ"))
                {
                    iNQ++;
                    xlWorksheetNQ = (Excel._Worksheet)xlWorkbookNQ.Sheets[1];
                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetNQ.Cells[2, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetNQ.Cells[3, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetNQ.Cells[2, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetNQ.Cells[3, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetNQ.Cells[3, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetNQ.Cells[3, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("2NQ") || currentFile.Contains("5NQ"))
                {
                    iNQ++;
                    xlWorksheetNQ = (Excel._Worksheet)xlWorkbookNQ.Sheets[1];
                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetNQ.Cells[6, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetNQ.Cells[7, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetNQ.Cells[6, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetNQ.Cells[7, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetNQ.Cells[7, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetNQ.Cells[7, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                }
                if (currentFile.Contains("3NQ") || currentFile.Contains("6NQ"))
                {
                    iNQ++;

                    const Int32 BufferSize = 128;
                    using (var fileStream = File.OpenRead(currentFile))
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                    {
                        String line;
                        int ii = 0;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            ii++;
                            if (ii == 2)
                            {
                                xlWorksheetNQ.Cells[10, 4] = line.Trim();
                            }
                            if (ii == 4)
                            {
                                xlWorksheetNQ.Cells[11, 3] = line.Trim();
                            }
                            if (ii == 5)
                            {
                                xlWorksheetNQ.Cells[10, 5] = line.Trim();
                            }
                            if (ii == 7)
                            {
                                xlWorksheetNQ.Cells[11, 1] = line.Trim();
                                if (line.Trim() != "MUNDAL")
                                {
                                    xlWorksheetNQ.Cells[11, 1].Interior.Color = XlRgbColor.rgbSkyBlue;
                                    xlWorksheetNQ.Cells[11, 1].Font.Color = XlRgbColor.rgbRed;
                                }
                            }
                            // Process line
                        }
                    }
                    File.Delete(currentFile);
                    iNQ = 0;
                }



            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad
            if (xlAppNQ != null)
            {
                Marshal.ReleaseComObject(xlWorksheetNQ);
                Marshal.ReleaseComObject(xlWorkbookNQ);
                Marshal.ReleaseComObject(xlAppNQ);
            }
            if (xlAppEQ != null)
            {
                Marshal.ReleaseComObject(xlWorksheetEQ);
                Marshal.ReleaseComObject(xlWorkbookEQ);
                Marshal.ReleaseComObject(xlAppEQ);
            }
            if (xlAppElox != null)
            {
                Marshal.ReleaseComObject(xlWorksheetElox);
                Marshal.ReleaseComObject(xlWorkbookElox);
                Marshal.ReleaseComObject(xlAppElox);
            }
            if (xlAppVA != null)
            {
                Marshal.ReleaseComObject(xlWorksheetVA);
                Marshal.ReleaseComObject(xlWorkbookVA);
                Marshal.ReleaseComObject(xlAppVA);
            }
            if (xlAppStV != null)
            {
                Marshal.ReleaseComObject(xlWorksheetStV);
                Marshal.ReleaseComObject(xlWorkbookStV);
                Marshal.ReleaseComObject(xlAppStV);
            }

        }


    }
}

