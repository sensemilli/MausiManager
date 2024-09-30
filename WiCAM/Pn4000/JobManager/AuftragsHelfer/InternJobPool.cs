using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.AuftragsHelfer
{
    public class InternJobPool
    {
        private string pathToPNdrive;
        private SQLMaterialPool _SQLMaterialPool;
        private string connectionString;
        private string sqlQuery;
        private SqlCommand sqlcmd;
        private SqlDataAdapter sqlda;
        private DateTime parsedTime;

        public InternJobPool() {
            pathToPNdrive = PnPathBuilder.PnDrive;
            _SQLMaterialPool = new SQLMaterialPool();
        }

        public void OpenSQLOrderDataConnection(AuftragsDataViewModel auftragsDataViewModel)
        {
            auftragsDataViewModel.OrdersData = new ObservableCollection<OrdersData>();
            CultureInfo provider = CultureInfo.InvariantCulture;

            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            //if (PnPathBuilder.ArDrive == "H:")
            //    connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();
            sqlQuery = "Select mpid, auftragsnummer, kundenname, bemerkungen, bearbeiter1, bearbeiter2, bearbeiter3, bearbeiter4, bearbeiter5, bearbeiter6, bearbeiter7, bearbeiter8, bearbeiter9, " +
                "liefertermin, avtermin, produktion, verpacken, oberflaeche, komplettieren from dbo.w_auftragspool";
            sqlcmd = new SqlCommand(sqlQuery, connection);
            sqlda = new SqlDataAdapter(sqlcmd);
            using (SqlDataReader reader = sqlcmd.ExecuteReader())
            {
                var ordinals = new
                {
                    mpid = reader.GetOrdinal("mpid"),
                    auftragsnummer = reader.GetOrdinal("auftragsnummer"),
                    kundenname = reader.GetOrdinal("kundenname"),
                    bemerkungen = reader.GetOrdinal("bemerkungen"),
                    bearbeiter1 = reader.GetOrdinal("bearbeiter1"),
                    bearbeiter2 = reader.GetOrdinal("bearbeiter2"),
                    bearbeiter3 = reader.GetOrdinal("bearbeiter3"),
                    bearbeiter4 = reader.GetOrdinal("bearbeiter4"),
                    bearbeiter5 = reader.GetOrdinal("bearbeiter5"),
                    bearbeiter6 = reader.GetOrdinal("bearbeiter6"),
                    bearbeiter7 = reader.GetOrdinal("bearbeiter7"),
                    bearbeiter8 = reader.GetOrdinal("bearbeiter8"),
                    bearbeiter9 = reader.GetOrdinal("bearbeiter9"),
                    liefertermin = reader.GetOrdinal("liefertermin"),
                    avtermin = reader.GetOrdinal("avtermin"),
                    produktion = reader.GetOrdinal("produktion"),
                    verpacken = reader.GetOrdinal("verpacken"),
                    oberflaeche = reader.GetOrdinal("oberflaeche"),
                    komplettieren = reader.GetOrdinal("komplettieren")
                };

                while (reader.Read() == true)
                {
                    var temp = new OrdersData();
                    temp.mpid = reader.GetInt32(ordinals.mpid);
                    temp.auftragsnummer = reader.GetString(ordinals.auftragsnummer);
                    temp.kundenname = reader.GetString(ordinals.kundenname);
                    temp.bemerkungen = reader.GetString(ordinals.bemerkungen);
                    if (!reader.IsDBNull(ordinals.bearbeiter1))
                        temp.bearbeiter1 = reader.GetString(ordinals.bearbeiter1);
                    else temp.bearbeiter1 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter2))
                        temp.bearbeiter2 = reader.GetString(ordinals.bearbeiter2);
                    else temp.bearbeiter2 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter3))
                        temp.bearbeiter3 = reader.GetString(ordinals.bearbeiter3);
                    else temp.bearbeiter3 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter4))
                        temp.bearbeiter4 = reader.GetString(ordinals.bearbeiter4);
                    else temp.bearbeiter4 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter5))
                        temp.bearbeiter5 = reader.GetString(ordinals.bearbeiter5);
                    else temp.bearbeiter5 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter6))
                        temp.bearbeiter6 = reader.GetString(ordinals.bearbeiter6);
                    else temp.bearbeiter6 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter7))
                        temp.bearbeiter7 = reader.GetString(ordinals.bearbeiter7);
                    else temp.bearbeiter7 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter8))
                        temp.bearbeiter8 = reader.GetString(ordinals.bearbeiter8);
                    else temp.bearbeiter8 = string.Empty;
                    if (!reader.IsDBNull(ordinals.bearbeiter9))
                        temp.bearbeiter9 = reader.GetString(ordinals.bearbeiter9);
                    else temp.bearbeiter9 = string.Empty;
                    if (!reader.IsDBNull(ordinals.liefertermin))
                    {
                        string parsedate = reader.GetString(ordinals.liefertermin);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.liefertermin = parsedTime;
                    }
                    else temp.liefertermin = DateTime.Today;

                    if (!reader.IsDBNull(ordinals.avtermin))
                    {
                        string parsedate = reader.GetString(ordinals.avtermin);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.avtermin = parsedTime;
                    }
                    else temp.avtermin = DateTime.Today;

                    if (!reader.IsDBNull(ordinals.produktion))
                    {
                        string parsedate = reader.GetString(ordinals.produktion);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.produktion = parsedTime;
                    }
                    else temp.produktion = DateTime.Today;

                    if (!reader.IsDBNull(ordinals.verpacken))
                    {
                        string parsedate = reader.GetString(ordinals.verpacken);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.verpacken = parsedTime;
                    }
                    else temp.verpacken = DateTime.Today;

                    if (!reader.IsDBNull(ordinals.komplettieren))
                    {
                        string parsedate = reader.GetString(ordinals.komplettieren);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.komplettieren = parsedTime;
                    }
                    else temp.komplettieren = DateTime.Today;

                    if (!reader.IsDBNull(ordinals.oberflaeche))
                    {
                        string parsedate = reader.GetString(ordinals.oberflaeche);
                        DateTime.TryParse(parsedate.Replace(".", "-"), out parsedTime);
                        temp.oberflaeche = parsedTime;
                    }
                    else temp.oberflaeche = DateTime.Today;
                    temp.ResultThumb = "/Images/maus.ico";
                    //AuftragsDataControl._AuftragsDataControl.caltemplate.calendar.SelectedDate = avtermindate;
                    auftragsDataViewModel.OrdersData.Add(temp);
                    //AuftragsDataControl._AuftragsDataControl.calendar.SelectedDate = "25.07.2024";
                }


            }


            auftragsDataViewModel.GridOrders.SelectedValuePath = "auftragsnummer";
            auftragsDataViewModel.GridOrders.DisplayMemberPath = "kundenname";
            auftragsDataViewModel.GridOrders.ItemsSource = auftragsDataViewModel.OrdersData;

            //System.Data.DataTable dt = new System.Data.DataTable("auftragsnummer");
            //sqlda.Fill(dt);
            //GridOrders.ItemsSource = dt.DefaultView;
            connection.Close();
        }

        internal void ReadPartsFromOrder(AuftragsDataViewModel auftragsDataViewModel, string auftragsnummer)
        {
            auftragsDataViewModel.PartOrderData = new ObservableCollection<PartOrderData>();
            CultureInfo provider = CultureInfo.InvariantCulture;
            //int count = auftragsnummer.Count();
           // auftragsnummer = auftragsnummer.Remove(6, count - 6);
            if (PnPathBuilder.ArDrive == "P:")
                connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            //if (PnPathBuilder.ArDrive == "H:")
            //    connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            if (PnPathBuilder.ArDrive == "C:")
                connectionString = @"Data Source=DESKTOP-8M8J1J0\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();
            sqlQuery = "Select IDB050, NC_FILE, POSITION, NUMBER, REMARK ,AMOUNT ,NESTING_ROT ,NC_EXIST ,MATERIAL_NO ,MACHINE_NO ," +
                "NC_ARCHIV ,PRIORITY ,RELEASE ,STATUS ,THICKNESS ,DIMENSION_X ,DIMENSION_Y ,ORDER_DATE ,RELEASED_AMOUNT ,PRODUCED_AMOUNT " +
                ",REJECTED_AMOUNT ,MAX_AMOUNT ,TABLE_NO ,STACK_NO ,NEST_MIRR ,COMMON_CUT ,RESERVE_ID ,RESERVED_1 ,RESERVED_2 ,PART_TIME " +
                ",PLAT_TIME ,MACH_TIME ,PART_AREA ,PLAT_AREA ,MACH_AREA ,REMARK_1 ,REMARK_2 ,REMARK_3 ,REMARK_4 ,REMARK_5 ,REMARK_6 ,REMARK_7 " +
                ",REMARK_8 ,REMARK_9 ,REMARK_10 ,REMARK_11 ,REMARK_12 ,REMARK_13 ,REMARK_14 ,REMARK_15 ,REMARK_16 ,REMARK_17 ,REMARK_18 ,REMARK_19 " +
                ",REMARK_20 ,locked ,pc_name ,filter ,PLATE_IDENT ,PLATE_CHARGE ,PLATE_DESCRIPTION ,PLATE_STORAGE_NR ,BAUGRUPPE ,COMMISSION_NR " +
                ",COLOR ,CLEANED_UP ,StdNestingName ,StdNestingArchive ,RotationAngle ,ModifiedDate ,CreationDate " +
                " from dbo.OrderedParts WHERE NUMBER LIKE " + MainWindow.mainWindow.txtAuftragsNummer.Text;
            Console.WriteLine(sqlQuery);
            sqlcmd = new SqlCommand(sqlQuery, connection);
            sqlda = new SqlDataAdapter(sqlcmd);
            try
            {
                using (SqlDataReader reader = sqlcmd.ExecuteReader())
                {
                    var ordinals = new
                    {
                        mpid = reader.GetOrdinal("IDB050"),
                        ncfile = reader.GetOrdinal("NC_FILE"),
                        position = reader.GetOrdinal("POSITION"),
                        number = reader.GetOrdinal("NUMBER"),
                        //remark = reader.GetOrdinal("REMARK"),
                        amount = reader.GetOrdinal("AMOUNT"),
                        dimx = reader.GetOrdinal("DIMENSION_X"),
                        dimy = reader.GetOrdinal("DIMENSION_Y"),
                        thick = reader.GetOrdinal("THICKNESS"),
                        bemerkungen = reader.GetOrdinal("REMARK_1"),
                        artikel = reader.GetOrdinal("REMARK_3"),
                        material = reader.GetOrdinal("MATERIAL_NO"),
                        materialname = reader.GetOrdinal("REMARK_2"),
                        oberflaeche = reader.GetOrdinal("REMARK_4"),
                        baugruppe = reader.GetOrdinal("REMARK_5"),
                        gravur = reader.GetOrdinal("REMARK_6"),
                        release = reader.GetOrdinal("RELEASE"),
                        status = reader.GetOrdinal("STATUS")

                        //liefertermin = reader.GetOrdinal("ModifiedDate"),
                        //avtermin = reader.GetOrdinal("CreationDate")

                    };

                    while (reader.Read() == true)
                    {
                        var temp = new PartOrderData();
                        temp.IDB050 = reader.GetInt32(ordinals.mpid);
                        temp.Auftrag = reader.GetString(ordinals.number);
                        temp.PartName = reader.GetString(ordinals.ncfile);
                        temp.OrderPos = reader.GetString(ordinals.position);
                        temp.AnzahlInt = reader.GetInt32(ordinals.amount);
                        temp.Anzahl = temp.AnzahlInt.ToString();
                        temp.LaengeDouble = reader.GetDouble(ordinals.dimx);
                        temp.Laenge = temp.LaengeDouble.ToString();
                        temp.BreiteDouble = reader.GetDouble(ordinals.dimy);
                        temp.Breite = temp.BreiteDouble.ToString();
                        temp.DickeDouble = reader.GetDouble(ordinals.thick);
                        temp.Dicke = temp.DickeDouble.ToString();
                        temp.MaterialInt = reader.GetInt32(ordinals.material);
                        temp.Material = reader.GetString(ordinals.materialname);//_SQLMaterialPool.switchMaterials(temp.MaterialInt);
                        temp.Artikel = reader.GetString(ordinals.artikel);
                        temp.Oberflaeche = reader.GetString(ordinals.oberflaeche);
                        if (!reader.IsDBNull(ordinals.bemerkungen))
                            temp.Bemerkungen = reader.GetString(ordinals.bemerkungen);
                        if (!reader.IsDBNull(ordinals.baugruppe))
                            temp.AssemblyName = reader.GetString(ordinals.baugruppe);
                        if (!reader.IsDBNull(ordinals.gravur))
                            temp.Gravur = reader.GetString(ordinals.gravur);
                        if (!reader.IsDBNull(ordinals.release))
                            temp.Release = reader.GetInt32(ordinals.release);
                        if (!reader.IsDBNull(ordinals.status))
                            temp.Status = reader.GetInt32(ordinals.status);
                        Console.WriteLine(reader.GetString(ordinals.ncfile));
                        auftragsDataViewModel.PartOrderData.Add(temp);

                    }
                }
            }
            catch
            {
                Console.WriteLine("errorpartsorder  " + auftragsnummer);
                connection.Close();

            }
            


         //   auftragsDataViewModel.PartOrders.SelectedValuePath = "NUMBER";
          //  auftragsDataViewModel.PartOrders.DisplayMemberPath = "NC_FILE";
            auftragsDataViewModel.PartOrders.ItemsSource = auftragsDataViewModel.PartOrderData;

            //System.Data.DataTable dt = new System.Data.DataTable("auftragsnummer");
            //sqlda.Fill(dt);
            //GridOrders.ItemsSource = dt.DefaultView;
            connection.Close();
        }
    }
}
