using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using WiCAM.Pn4000.Autoloop;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Converters;
using WiCAM.Pn4000.Database;
using WiCAM.Pn4000.Gmpool.Classes;
using WiCAM.Pn4000.Gmpool.Controls;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	public class AddAuftragControlViewModel : WiCAM.Pn4000.JobManager.ViewModelBase
    {
		private readonly ObservableCollection<StockAuftragInfo> _auftragsCollection;

		private readonly List<StockAuftragInfo> _materialsList;

		private readonly ViewAction _action;
        private OrdersData _row;
        private readonly Action _closeAction;

		private readonly StockMaterialDatabaseHelper _dbHelper;

		private readonly StockAuftragInfo _originalItem;

		private Collection<PlateTypeInfo> _plateTypes;

		private FrameworkElement _activeView;

		private StockAuftragViewModel _selectedItem;

		private ICommand _selectMaterialCommand;

		private ICommand _SelectColorsCommand;

		private ICommand _okCommand;

		private ICommand _cancelCommand;
        public static AddAuftragControlViewModel _AddAuftragControlViewModel;
        private string pathToPNdrive;
        private SqlServerHelper _helper;
        private string connectionString;
        private DateTimePicker oDateTimePicker;
        private readonly IStockAuftragHelper _materialsHelper;


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

		public ICommand CancelCommand
		{
			get
			{
				if (this._cancelCommand == null)
				{
					this._cancelCommand = new RelayCommand((object x) => this.Cancel(), (object x) => this.CanCancel());
				}
				return this._cancelCommand;
			}
		}

		public string DoubleFormat
		{
			get;
			set;
		}

		public ICommand OkCommand
		{
			get
			{
				if (this._okCommand == null)
				{
					this._okCommand = new RelayCommand((object x) => this.Ok(), (object x) => this.CanOkCommand());
				}
				return this._okCommand;
			}
		}

		public Collection<PlateTypeInfo> PlateTypes
		{
			get
			{
				return this._plateTypes;
			}
			set
			{
				this._plateTypes = value;
				base.NotifyPropertyChanged("PlateTypes");
			}
		}

		public StockAuftragViewModel SelectedItem
		{
			get
			{
				return this._selectedItem;
			}
			set
			{
				this._selectedItem = value;
				base.NotifyPropertyChanged("SelectedItem");
			}
		}

		public ICommand SelectColorsCommand
        {
			get
			{
				if (this._SelectColorsCommand == null)
				{
					this._SelectColorsCommand = new RelayCommand((object x) => this.SelectColors(), (object x) => true);
				}
				return this._SelectColorsCommand;
			}
		}

		public ICommand SelectMaterialCommand
		{
			get
			{
				if (this._selectMaterialCommand == null)
				{
					this._selectMaterialCommand = new RelayCommand((object x) => this.SelectMaterial(), (object x) => true);
				}
				return this._selectMaterialCommand;
			}
		}

        public AddAuftragControlViewModel(ObservableCollection<OrdersData> orders, OrdersData row, OrdersData selectedItem, ViewAction action, Action closeAction)
        {
            _AddAuftragControlViewModel = this;
            pathToPNdrive = PnPathBuilder.PnDrive;
            Console.WriteLine(ActiveView);
            this._action = action;
           
            this._row = row;
            if (action == ViewAction.Edit)
            {
               
                fillEdit(row);
            }
            if (action == ViewAction.Create)
            {

                fillCreate(row);
            }
        }

        private void fillEdit(OrdersData row)
        {
            AuftragsNummer = row.auftragsnummer;
            KundenName = row.kundenname;
            Bemerkungen = row.bemerkungen;
            Bearbeiter1 = row.bearbeiter1;
            Bearbeiter2 = row.bearbeiter2;
            Bearbeiter3 = row.bearbeiter3;
            Bearbeiter4 = row.bearbeiter4;
            Bearbeiter5 = row.bearbeiter5;
            Bearbeiter6 = row.bearbeiter6;
            Bearbeiter7 = row.bearbeiter7;
            Bearbeiter8 = row.bearbeiter8;
            Bearbeiter9 = row.bearbeiter9;
            Liefertermin = row.liefertermin;
            AVtermin = row.avtermin;
            Produktion = row.produktion;
            Oberflaeche = row.oberflaeche;
            Verpacken = row.verpacken;
            Komplettieren = row.komplettieren;

            MPiD = row.mpid;
        }

        private void fillCreate(OrdersData row)
        {
            Bearbeiter1 = "Franz";
            Bearbeiter2 = "Maciej";
            Bearbeiter3 = "Jakob-Emin";
            Bearbeiter4 = "Tommy";
            Bearbeiter5 = "Carlos";
            Bearbeiter6 = "Kathrin";
            Bearbeiter7 = "???";
            Bearbeiter8 = " ";
            Bearbeiter9 = " ";
            AVtermin = DateTime.Today;
            Produktion = DateTime.Today;
            Oberflaeche = DateTime.Today;
            Verpacken = DateTime.Today;
            Komplettieren = DateTime.Today;
            
            //this._materialsCollection = materials;
            //this._materialsList = materialsList;
            //this._action = action;
            //this._closeAction = closeAction;
            //this._plateTypes = PlateTypeInfo.Types();
            //foreach (PlateTypeInfo _plateType in this._plateTypes)
            //{
            //	StringResourceHelper instance = StringResourceHelper.Instance;
            //	StockMaterialType type = _plateType.Type;
            //	_plateType.Text = instance.FindString(type.ToString());
            //}
            //this.DoubleFormat = SystemConfiguration.WpfFormatDouble;
            //if (ApplicationConfigurationInfo.Instance.TracingEnabled)
            //{
            //	DataManager.Instance.ShowTracing();
            //}

            //if (string.IsNullOrEmpty(plate.MachineNummer))
            //{
            //	plate.MachineNummer = "0;";
            //}
            //this._originalItem = plate;
            //plate.MaterialName = this.FindMaterialName(plate.MatNumber);
            //this.SelectedItem = (new StockMaterialConverter()).Convert(plate);
            //this.SelectedItem.OriginalMaterial = plate;
        }

        private bool CanCancel()
		{
			return true;
		}

		private void Cancel()
		{
            AuftragsDataControl._AuftragsDataControl.dockEdit.Visibility = Visibility.Hidden;
   //         this._originalItem.Locked = 0;
			//this._originalItem.PcName = string.Empty;
			//if (!DataManager.Instance.UpdateMaterial(this._originalItem))
			//{
			//	Logger.Error("Problems saving material {0}", new object[] { this._originalItem.PlName });
			//}
			Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
			action();
   //         MainWindow.Instance.dockEdit.Visibility = Visibility.Hidden;

        }

        private bool CanOkCommand()
		{
			return true;
		}

		

		private void MaterialSelectClosed()
		{
			this.ActiveView = null;
		}

		public string AuftragsNummer { get; set; }
        public string KundenName { get; set; }
        public string Bemerkungen { get; set; }
        public string Bearbeiter1 { get; set; }
        public string Bearbeiter2 { get; set; }
        public string Bearbeiter3 { get; set; }
        public string Bearbeiter4 { get; set; }
        public string Bearbeiter5 { get; set; }
        public string Bearbeiter6 { get; set; }
        public string Bearbeiter7 { get; set; }
        public string Bearbeiter8 { get; set; }
        public string Bearbeiter9 { get; set; }
        public DateTime? Liefertermin { get; set; }
        public DateTime? AVtermin { get; set; }
        public DateTime? Produktion { get; set; }
        public DateTime? Oberflaeche { get; set; }
        public DateTime? Verpacken { get; set; }
        public DateTime? Komplettieren { get; set; }

        public int? MPiD { get; private set; }

        private void Ok()
		{
            if (this._action == ViewAction.Edit)
            {
                fillEditChanged();
                Console.WriteLine(AuftragsNummer);
                if (PnPathBuilder.ArDrive == "P:")
                    connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                if (PnPathBuilder.ArDrive == "C:")
                    connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();
                string sql = "UPDATE wicam.dbo.w_auftragspool SET auftragsnummer = @auftragsnummer, kundenname = @kundenname, bemerkungen = @bemerkungen, bearbeiter1 = @bearbeiter1, bearbeiter2 = @bearbeiter2, " +
                    "bearbeiter3 = @bearbeiter3, bearbeiter4 = @bearbeiter4, bearbeiter5 = @bearbeiter5, bearbeiter6 = @bearbeiter6, bearbeiter7 = @bearbeiter7, bearbeiter8 = @bearbeiter8, bearbeiter9 = @bearbeiter9, " +
                    "avtermin = @avtermin, produktion = @produktion, verpacken = @verpacken, oberflaeche = @oberflaeche, komplettieren = @komplettieren, liefertermin = @liefertermin WHERE mpid = " + MPiD;
                SqlCommand cmd = new SqlCommand(sql, connection);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.UpdateCommand = new SqlCommand(sql, connection); // Insert (erstellen/einfügen) Command erstellen
                adapter.UpdateCommand.Parameters.AddWithValue("@auftragsnummer", AuftragsNummer);
                adapter.UpdateCommand.Parameters.AddWithValue("@kundenname", KundenName);
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerkungen", Bemerkungen);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter1", Bearbeiter1);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter2", Bearbeiter2);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter3", Bearbeiter3);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter4", Bearbeiter4);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter5", Bearbeiter5);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter6", Bearbeiter6);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter7", Bearbeiter7);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter8", Bearbeiter8);
                adapter.UpdateCommand.Parameters.AddWithValue("@bearbeiter9", Bearbeiter9);
                adapter.UpdateCommand.Parameters.AddWithValue("@avtermin", AVtermin);
                adapter.UpdateCommand.Parameters.AddWithValue("@produktion", Produktion);
                adapter.UpdateCommand.Parameters.AddWithValue("@verpacken", Verpacken);
                adapter.UpdateCommand.Parameters.AddWithValue("@oberflaeche", Oberflaeche);
                adapter.UpdateCommand.Parameters.AddWithValue("@komplettieren", Komplettieren);
                adapter.UpdateCommand.Parameters.AddWithValue("@liefertermin", Liefertermin);
                adapter.UpdateCommand.Parameters.AddWithValue("@material1", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@material2", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@material3", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@material4", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@material5", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@material6", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@material7", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@material8", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@material9", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe1", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe2", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe3", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe4", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe5", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe6", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe7", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe8", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@farbe9", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_1", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_2", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_3", "");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_4", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_5", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_6", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_7", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_8", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_9", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_10", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_11", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_12", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_13", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_14", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_15", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_16", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@bemerk_17", "abc");
                adapter.UpdateCommand.Parameters.AddWithValue("@freigabe", 1);
                adapter.UpdateCommand.Parameters.AddWithValue("@ErstellungsDatum", DateTime.Now.ToString("MMddyyyy"));
                adapter.UpdateCommand.Parameters.AddWithValue("@IsDeleted", 0);
                adapter.UpdateCommand.Parameters.AddWithValue("@DeleteDate", 2023);
                adapter.UpdateCommand.Parameters.AddWithValue("@ModifiedDate", 2023);
                adapter.UpdateCommand.Parameters.AddWithValue("@CreationDate", 2023);

                adapter.UpdateCommand.ExecuteNonQuery(); // Command ausführen

                // adapter.DeleteCommand
                // adapter.UpdateCommand

                // Alle Datenbank zugehörigen Objekte schließen

                //reader.Close();
                cmd.Dispose();
                connection.Close();
            }


                if (this._action == ViewAction.Create)
            {
                Console.WriteLine(AuftragsNummer);
                if (PnPathBuilder.ArDrive == "P:")
                    connectionString = @"Data Source=MUNDAL-APP02\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                if (PnPathBuilder.ArDrive == "C:")
                    connectionString = @"Data Source=TOMMY\WSQL;Initial Catalog=wicam; User ID=wicam; Password=wicamLeitstand";
                SqlConnection connection = new SqlConnection(connectionString);

                connection.Open();
                string sql = "INSERT INTO wicam.dbo.w_auftragspool (auftragsnummer, kundenname, bemerkungen, bearbeiter1, bearbeiter2, bearbeiter3 , bearbeiter4, bearbeiter5, bearbeiter6, bearbeiter7, bearbeiter8, bearbeiter9, " +
                             "avtermin , produktion, verpacken, oberflaeche, komplettieren, liefertermin, material1, material2, material3, material4, material5, material6, material7, material8, material9, " +
                             "farbe1, farbe2, farbe3, farbe4, farbe5, farbe6, farbe7, farbe8, farbe9, bemerk_1, bemerk_2, bemerk_3, bemerk_4, bemerk_5, bemerk_6, bemerk_7, bemerk_8, bemerk_9, bemerk_10, bemerk_11, " +
                             "bemerk_12, bemerk_13, bemerk_14, bemerk_15, bemerk_16, bemerk_17, freigabe, ErstellungsDatum, IsDeleted, DeleteDate, ModifiedDate, CreationDate) VALUES (@auftragsnummer,@kundenname,@bemerkungen," +
                   " @bearbeiter1, @bearbeiter2, @bearbeiter3,@bearbeiter4,@bearbeiter5,@bearbeiter6,@bearbeiter7,@bearbeiter8,@bearbeiter9,@avtermin,@produktion,@verpacken,@oberflaeche,@komplettieren,@liefertermin,@material1,@material2,@material3,@material4,@material5,@material6," +
                   " @material7, @material8, @material9,@farbe1,@farbe2,@farbe3,@farbe4,@farbe5, @farbe6,@farbe7,@farbe8,@farbe9,@bemerk_1,@bemerk_2,@bemerk_3,@bemerk_4,@bemerk_5, @bemerk_6,@bemerk_7,@bemerk_8,@bemerk_9,@bemerk_10,@bemerk_11, @bemerk_12, @bemerk_13, @bemerk_14," +
                   " @bemerk_15,@bemerk_16,@bemerk_17, @freigabe, @ErstellungsDatum, @IsDeleted,@DeleteDate, @ModifiedDate, @CreationDate)";
                SqlCommand cmd = new SqlCommand(sql, connection);

                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.InsertCommand = new SqlCommand(sql, connection); // Insert (erstellen/einfügen) Command erstellen
                adapter.InsertCommand.Parameters.AddWithValue("@auftragsnummer", AuftragsNummer);
                adapter.InsertCommand.Parameters.AddWithValue("@kundenname", KundenName);
                adapter.InsertCommand.Parameters.AddWithValue("@bemerkungen", Bemerkungen);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter1", Bearbeiter1);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter2", Bearbeiter2);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter3", Bearbeiter3);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter4", Bearbeiter4);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter5", Bearbeiter5);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter6", Bearbeiter6);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter7", Bearbeiter7);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter8", Bearbeiter8);
                adapter.InsertCommand.Parameters.AddWithValue("@bearbeiter9", Bearbeiter9);
                adapter.InsertCommand.Parameters.AddWithValue("@avtermin", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@produktion", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@verpacken", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@oberflaeche", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@komplettieren", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@liefertermin", AVtermin);
                adapter.InsertCommand.Parameters.AddWithValue("@material1", "");
                adapter.InsertCommand.Parameters.AddWithValue("@material2", "");
                adapter.InsertCommand.Parameters.AddWithValue("@material3", "");
                adapter.InsertCommand.Parameters.AddWithValue("@material4", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@material5", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@material6", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@material7", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@material8", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@material9", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe1", "");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe2", "");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe3", "");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe4", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe5", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe6", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe7", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe8", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@farbe9", "abc");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_1", "");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_2", "");
                adapter.InsertCommand.Parameters.AddWithValue("@bemerk_3", "");
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
                adapter.InsertCommand.Parameters.AddWithValue("@freigabe", 1);
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
            AuftragsDataViewModel._auftragsDataViewModel._InternJobPool.OpenSQLOrderDataConnection(AuftragsDataViewModel._auftragsDataViewModel);
            AuftragsDataControl._AuftragsDataControl.dockEdit.Visibility = Visibility.Hidden;
                Action action = this._closeAction;
			if (action == null)
			{
				return;
			}
          

        }

        private void fillEditChanged()
        {
            AuftragsNummer = AddAuftragControl._AddAuftragControl.txtFirst.Text;
            KundenName = AddAuftragControl._AddAuftragControl.xKunde.Text;
            Bemerkungen = AddAuftragControl._AddAuftragControl.xBemerkungen.Text;
            Bearbeiter1 = AddAuftragControl._AddAuftragControl.xBearbeiter1.Text;
            Bearbeiter2 = AddAuftragControl._AddAuftragControl.xBearbeiter2.Text;
            Bearbeiter3 = AddAuftragControl._AddAuftragControl.xBearbeiter3.Text;
            Bearbeiter4 = AddAuftragControl._AddAuftragControl.xBearbeiter4.Text;
            Bearbeiter5 = AddAuftragControl._AddAuftragControl.xBearbeiter5.Text;
            Bearbeiter6 = AddAuftragControl._AddAuftragControl.xBearbeiter6.Text;
            Bearbeiter7 = AddAuftragControl._AddAuftragControl.xBearbeiter7.Text;
            Bearbeiter8 = AddAuftragControl._AddAuftragControl.xBearbeiter8.Text;
            Bearbeiter9 = AddAuftragControl._AddAuftragControl.xBearbeiter9.Text;
            Liefertermin = AddAuftragControl._AddAuftragControl.xLiefertermin.SelectedDate;
            AVtermin = AddAuftragControl._AddAuftragControl.xAVtermin.SelectedDate;
            Produktion = AddAuftragControl._AddAuftragControl.xProduktion.SelectedDate;
            Oberflaeche = AddAuftragControl._AddAuftragControl.xOberflaeche.SelectedDate;
            Verpacken = AddAuftragControl._AddAuftragControl.xVerpacken.SelectedDate;
            Komplettieren = AddAuftragControl._AddAuftragControl.xKomplettieren.SelectedDate;
        }

        private void SelectColors()
		{
            //MachinesSelectControl machinesSelectControl = new MachinesSelectControl();
            //MachinesSelectControlViewModel machinesSelectControlViewModel = new MachinesSelectControlViewModel(this.SelectedItem, () => this.ActiveView = null);
            //machinesSelectControl.DataContext = machinesSelectControlViewModel;
            //this.ActiveView = machinesSelectControl;
            Console.WriteLine("colors");
        }

		private void SelectMaterial()
		{
			//WiCAM.Pn4000.Gmpool.MaterialSelectControl materialSelectControl = new WiCAM.Pn4000.Gmpool.MaterialSelectControl();
			//MaterialSelectControlViewModel materialSelectControlViewModel = new MaterialSelectControlViewModel(this.SelectedItem, materialSelectControl, new Action(this.MaterialSelectClosed));
			//materialSelectControl.DataContext = materialSelectControlViewModel;
			//this.ActiveView = materialSelectControl;
		}
	}
}