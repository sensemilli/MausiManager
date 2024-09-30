using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Materials;
using System.Globalization;
using System.Reflection;

namespace WiCAM.Pn4000.JobManager.Views
{
    [Serializable]
    public class StockAuftragInfo : BindableObject, IAuftragializeItem

    {
        private static SqlCrudInfo _crud;

        public const string csAuftragsNummer = "AuftragsNummer";

        public const string PkName = "Mpid";

        public const string csKundenName = "KundenName";

        public const string csIdentNr = "IdentNr";

        public const string csBezeichnung = "Bezeichnung";

        public const string csBemerk1 = "Bemerk1";

        public const string csBemerk2 = "Bemerk2";

        public const string csBemerk3 = "Bemerk3";

        public const string csPlThick = "PlThick";

        public const string csMaxX = "MaxX";

        public const string csMaxY = "MaxY";

        public const string csMatNumber = "MatNumber";

        public const string csPlTyp = "PlTyp";

        public const string csArNumber = "ArNumber";

        public const string csRes1 = "Res1";

        public const string csRes2 = "Res2";

        public const string csRes3 = "Res3";

        public const string csAmount = "Amount";

        public const string csFreigabe = "Freigabe";

        public const string csLagerplatz = "Lagerplatz";

        public const string csBevorzug = "Bevorzug";

        public const string csRes4 = "Res4";

        public const string csRes5 = "Res5";

        public const string csMachine = "Machine";

        public const string csBewFactor = "BewFactor";

        public const string csMinX = "MinX";

        public const string csMinY = "MinY";

        public const string csStepX = "StepX";

        public const string csStepY = "StepY";

        public const string csAmountRes = "AmountRes";

        public const string csLocked = "Locked";

        public const string csPcName = "PcName";

        public const string csFilter = "Filter";

        public const string csBemerk4 = "Bemerk4";

        public const string csBemerk5 = "Bemerk5";

        public const string csBemerk6 = "Bemerk6";

        public const string csBemerk7 = "Bemerk7";

        public const string csBemerk8 = "Bemerk8";

        public const string csBemerk9 = "Bemerk9";

        public const string csBemerk10 = "Bemerk10";

        public const string csBemerk11 = "Bemerk11";

        public const string csBemerk12 = "Bemerk12";

        public const string csBemerk13 = "Bemerk13";

        public const string csBemerk14 = "Bemerk14";

        public const string csBemerk15 = "Bemerk15";

        public const string csBemerk16 = "Bemerk16";

        public const string csBemerk17 = "Bemerk17";

        public const string csBemerk18 = "Bemerk18";

        public const string csBemerk19 = "Bemerk19";

        public const string csBemerk20 = "Bemerk20";

        public const string csReferenceNr = "ReferenceNr";

        public const string csLagerplatzString = "LagerplatzString";

        public const string csMachineNummer = "MachineNummer";

        public const string csUeberbuchungErlaubt = "UeberbuchungErlaubt";

        public const string csMeldeBestand = "MeldeBestand";

        public const string csErstellungsDatum = "ErstellungsDatum";

        public const string csIsDeleted = "IsDeleted";

        public const string csDeleteDate = "DeleteDate";

        public const string SqlTableName = "[dbo].[w_matpool]";

        public const string csSelectSingleMaterial = "SelectSingleMaterial";

        public const string csSelectId = "SelectId";

        public const string csDeleteFiltered = "DeleteFiltered";

        private static readonly string __defaultMachineNumberText = "0;";

        private static readonly string SqlSelectId = "SELECT [mpid] FROM [dbo].[w_matpool]";

        private static readonly string SqlMaterialSelectWhere = " WHERE ISNULL([pl_name], N'')=@PlName AND ISNULL([ident_nr], N'')=@IdentNr \r\nAND ISNULL([bezeichnung], N'')=@Bezeichnung AND [lagerplatz]=@Lagerplatz AND RTRIM(ISNULL([filter], N''))=ISNULL(@Filter, N'');";


        private string _identNr;

        private string _bezeichnung;

        private string _bemerk1;

        private string _bemerk2;

        private string _bemerk3;

        private double _plThick;

        private double _maxX;

        private double _maxY;

        private int _matNumber;

        private int _plTyp;

        private int _arNumber;

        private double _res1;

        private double _res2;

        private double _res3;

        private int _amount;

        private int _freigabe;

        private int _lagerplatz;

        private double _bevorzug;

        private double _res4;

        private double _res5;

        private int _machine;

        private double _bewFactor;

        private double _minX;

        private double _minY;

        private double _stepX;

        private double _stepY;

        private int _amountRes;

        private int _locked;

        private string _pcName;

        private string _filter;

        private string _bemerk4;

        private string _bemerk5;

        private string _bemerk6;

        private string _bemerk7;

        private string _bemerk8;

        private string _bemerk9;

        private string _bemerk10;

        private string _bemerk11;

        private string _bemerk12;

        private string _bemerk13;

        private string _bemerk14;

        private string _bemerk15;

        private string _bemerk16;

        private string _bemerk17;

        private string _bemerk18;

        private string _bemerk19;

        private string _bemerk20;

        private string _referenceNr;

        private string _lagerplatzString;

        private string _machineNummer;

        private bool _ueberbuchungErlaubt;

        private int _meldeBestand;

        private int _erstellungsDatum;

        private bool _isDeleted;

        private DateTime _deleteDate;

        private string _materialName;

        private DateTime? _modifiedDate;

        private DateTime _creationDate;
        private string _AuftragsNummer;
        private string _KundenName;
        private string _Bemerkung;
        private int _mpid;

        [XmlElement]
        [DataMember]
        [DataObjectField(true, true, false)]
        [Browsable(false)]
        [Column(Name = "mpid", Storage = "Mpid", CanBeNull = false, IsPrimaryKey = true)]
        public int Mpid
        {
            get
            {
                return _mpid;
            }
            set
            {
                if (value != _mpid)
                {
                    _mpid = value;
                    NotifyPropertyChanged("Mpid");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(true, true, false)]
        [Browsable(false)]
        [Column(Name = "AuftragsNummer", Storage = "AuftragsNummer", CanBeNull = false, IsPrimaryKey = true)]
        public string AuftragsNummer
        {
            get
            {
                return _AuftragsNummer;
            }
            set
            {
                if (value != _AuftragsNummer)
                {
                    _AuftragsNummer = value;
                    NotifyPropertyChanged("AuftragsNummer");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(true, true, false)]
        [Browsable(false)]
        [Column(Name = "KundenName", Storage = "KundenName", CanBeNull = false, IsPrimaryKey = true)]
        public string KundenName
        {
            get
            {
                return _KundenName;
            }
            set
            {
                if (value != _KundenName)
                {
                    _KundenName = value;
                    NotifyPropertyChanged("KundenName");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "Bemerkung", Storage = "Bemerkung", CanBeNull = false)]
        [TranslationKey("Bemerkung")]
        [StringLength(8000)]
        public string Bemerkung
        {
            get
            {
                return _Bemerkung;
            }
            set
            {
                if (value != _Bemerkung)
                {
                    _Bemerkung = value;
                    NotifyPropertyChanged("Bemerkung");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "ident_nr", Storage = "IdentNr", CanBeNull = true)]
        [TranslationKey("PLATE_IDENT_NO")]
        [StringLength(80)]
        public string IdentNr
        {
            get
            {
                return _identNr;
            }
            set
            {
                if (value != _identNr)
                {
                    _identNr = value;
                    NotifyPropertyChanged("IdentNr");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bezeichnung", Storage = "Bezeichnung", CanBeNull = true)]
        [TranslationKey("PLATE_DESCRIPTION")]
        [StringLength(80)]
        public string Bezeichnung
        {
            get
            {
                return _bezeichnung;
            }
            set
            {
                if (value != _bezeichnung)
                {
                    _bezeichnung = value;
                    NotifyPropertyChanged("Bezeichnung");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_1", Storage = "Bemerk1", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_1")]
        [StringLength(80)]
        public string Bemerk1
        {
            get
            {
                return _bemerk1;
            }
            set
            {
                if (value != _bemerk1)
                {
                    _bemerk1 = value;
                    NotifyPropertyChanged("Bemerk1");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_2", Storage = "Bemerk2", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_2")]
        [StringLength(80)]
        public string Bemerk2
        {
            get
            {
                return _bemerk2;
            }
            set
            {
                if (value != _bemerk2)
                {
                    _bemerk2 = value;
                    NotifyPropertyChanged("Bemerk2");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_3", Storage = "Bemerk3", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_3")]
        [StringLength(80)]
        public string Bemerk3
        {
            get
            {
                return _bemerk3;
            }
            set
            {
                if (value != _bemerk3)
                {
                    _bemerk3 = value;
                    NotifyPropertyChanged("Bemerk3");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "pl_thick", Storage = "PlThick", CanBeNull = false)]
        [TranslationKey("PLATE_THICKNESS")]
        public double PlThick
        {
            get
            {
                return _plThick;
            }
            set
            {
                if (value != _plThick)
                {
                    _plThick = value;
                    NotifyPropertyChanged("PlThick");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "max_x", Storage = "MaxX", CanBeNull = false)]
        [TranslationKey("PLATE_MAX_SIZE_X")]
        public double MaxX
        {
            get
            {
                return _maxX;
            }
            set
            {
                if (value != _maxX)
                {
                    _maxX = value;
                    NotifyPropertyChanged("MaxX");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "max_y", Storage = "MaxY", CanBeNull = false)]
        [TranslationKey("PLATE_MAX_SIZE_Y")]
        public double MaxY
        {
            get
            {
                return _maxY;
            }
            set
            {
                if (value != _maxY)
                {
                    _maxY = value;
                    NotifyPropertyChanged("MaxY");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "mat_number", Storage = "MatNumber", CanBeNull = false)]
        [TranslationKey("PLATE_MATERIAL_NO")]
        public int MatNumber
        {
            get
            {
                return _matNumber;
            }
            set
            {
                if (value != _matNumber)
                {
                    _matNumber = value;
                    NotifyPropertyChanged("MatNumber");
                }
            }
        }

        [TranslationKey("PLATE_MATERIAL_NAME")]
        public string MaterialName
        {
            get
            {
                return _materialName;
            }
            set
            {
                if (value != _materialName)
                {
                    _materialName = value;
                    NotifyPropertyChanged("MaterialName");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "pl_typ", Storage = "PlTyp", CanBeNull = false)]
        [TranslationKey("PLATE_TYP")]
        public int PlTyp
        {
            get
            {
                return _plTyp;
            }
            set
            {
                if (value != _plTyp)
                {
                    _plTyp = value;
                    NotifyPropertyChanged("PlTyp");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "ar_number", Storage = "ArNumber", CanBeNull = false)]
        [TranslationKey("PLATE_ARCHIV_NO")]
        public int ArNumber
        {
            get
            {
                return _arNumber;
            }
            set
            {
                if (value != _arNumber)
                {
                    _arNumber = value;
                    NotifyPropertyChanged("ArNumber");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "res1", Storage = "Res1", CanBeNull = false)]
        [TranslationKey("PLATE_ORDER_MATERIAL")]
        public double Res1
        {
            get
            {
                return _res1;
            }
            set
            {
                if (value != _res1)
                {
                    _res1 = value;
                    NotifyPropertyChanged("Res1");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "res2", Storage = "Res2", CanBeNull = false)]
        [TranslationKey("PLATE_AREA")]
        public double Res2
        {
            get
            {
                return _res2;
            }
            set
            {
                if (value != _res2)
                {
                    _res2 = value;
                    NotifyPropertyChanged("Res2");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "res3", Storage = "Res3", CanBeNull = false)]
        [TranslationKey("PLATE_WEIGHT")]
        public double Res3
        {
            get
            {
                return _res3;
            }
            set
            {
                if (value != _res3)
                {
                    _res3 = value;
                    NotifyPropertyChanged("Res3");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "amount", Storage = "Amount", CanBeNull = false)]
        [TranslationKey("PLATE_AMOUNT")]
        public int Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (value != _amount)
                {
                    _amount = value;
                    NotifyPropertyChanged("Amount");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "freigabe", Storage = "Freigabe", CanBeNull = false)]
        [TranslationKey("PLATE_RELEASE_STATUS")]
        public int Freigabe
        {
            get
            {
                return _freigabe;
            }
            set
            {
                if (value != _freigabe)
                {
                    _freigabe = value;
                    NotifyPropertyChanged("Freigabe");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "lagerplatz", Storage = "Lagerplatz", CanBeNull = false)]
        [TranslationKey("PLATE_STOCK_LOCATION")]
        public int Lagerplatz
        {
            get
            {
                return _lagerplatz;
            }
            set
            {
                if (value != _lagerplatz)
                {
                    _lagerplatz = value;
                    NotifyPropertyChanged("Lagerplatz");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "bevorzug", Storage = "Bevorzug", CanBeNull = false)]
        [TranslationKey("PLATE_FAVOURITE")]
        public double Bevorzug
        {
            get
            {
                return _bevorzug;
            }
            set
            {
                if (value != _bevorzug)
                {
                    _bevorzug = value;
                    NotifyPropertyChanged("Bevorzug");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "res4", Storage = "Res4", CanBeNull = false)]
        [TranslationKey("RES4")]
        public double Res4
        {
            get
            {
                return _res4;
            }
            set
            {
                if (value != _res4)
                {
                    _res4 = value;
                    NotifyPropertyChanged("Res4");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "res5", Storage = "Res5", CanBeNull = false)]
        [TranslationKey("RES5")]
        public double Res5
        {
            get
            {
                return _res5;
            }
            set
            {
                if (value != _res5)
                {
                    _res5 = value;
                    NotifyPropertyChanged("Res5");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "machine", Storage = "Machine", CanBeNull = false)]
        [TranslationKey("PLATE_MACHINE_NO")]
        public int Machine
        {
            get
            {
                return _machine;
            }
            set
            {
                if (value != _machine)
                {
                    _machine = value;
                    NotifyPropertyChanged("Machine");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "bew_factor", Storage = "BewFactor", CanBeNull = false)]
        [TranslationKey("PLATE_OPT_FACTOR")]
        public double BewFactor
        {
            get
            {
                return _bewFactor;
            }
            set
            {
                if (value != _bewFactor)
                {
                    _bewFactor = value;
                    NotifyPropertyChanged("BewFactor");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "min_x", Storage = "MinX", CanBeNull = false)]
        [TranslationKey("PLATE_MIN_SIZE_X")]
        public double MinX
        {
            get
            {
                return _minX;
            }
            set
            {
                if (value != _minX)
                {
                    _minX = value;
                    NotifyPropertyChanged("MinX");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "min_y", Storage = "MinY", CanBeNull = false)]
        [TranslationKey("PLATE_MIN_SIZE_Y")]
        public double MinY
        {
            get
            {
                return _minY;
            }
            set
            {
                if (value != _minY)
                {
                    _minY = value;
                    NotifyPropertyChanged("MinY");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "step_x", Storage = "StepX", CanBeNull = false)]
        [TranslationKey("PLATE_SIZE_STEP_X")]
        public double StepX
        {
            get
            {
                return _stepX;
            }
            set
            {
                if (value != _stepX)
                {
                    _stepX = value;
                    NotifyPropertyChanged("StepX");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [InchConversion]
        [Column(Name = "step_y", Storage = "StepY", CanBeNull = false)]
        [TranslationKey("PLATE_SIZE_STEP_Y")]
        public double StepY
        {
            get
            {
                return _stepY;
            }
            set
            {
                if (value != _stepY)
                {
                    _stepY = value;
                    NotifyPropertyChanged("StepY");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "amount_res", Storage = "AmountRes", CanBeNull = false)]
        [TranslationKey("PLATE_RES_AMOUNT")]
        public int AmountRes
        {
            get
            {
                return _amountRes;
            }
            set
            {
                if (value != _amountRes)
                {
                    _amountRes = value;
                    NotifyPropertyChanged("AmountRes");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "locked", Storage = "Locked", CanBeNull = false)]
        public int Locked
        {
            get
            {
                return _locked;
            }
            set
            {
                if (value != _locked)
                {
                    _locked = value;
                    NotifyPropertyChanged("Locked");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [Column(Name = "pc_name", Storage = "PcName", CanBeNull = false)]
        [StringLength(20)]
        public string PcName
        {
            get
            {
                return _pcName;
            }
            set
            {
                if (value != _pcName)
                {
                    _pcName = value;
                    NotifyPropertyChanged("PcName");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "filter", Storage = "Filter", CanBeNull = true)]
        [StringLength(20)]
        public string Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                if (value != _filter)
                {
                    _filter = value;
                    NotifyPropertyChanged("Filter");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_4", Storage = "Bemerk4", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_4")]
        [StringLength(80)]
        public string Bemerk4
        {
            get
            {
                return _bemerk4;
            }
            set
            {
                if (value != _bemerk4)
                {
                    _bemerk4 = value;
                    NotifyPropertyChanged("Bemerk4");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_5", Storage = "Bemerk5", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_5")]
        [StringLength(80)]
        public string Bemerk5
        {
            get
            {
                return _bemerk5;
            }
            set
            {
                if (value != _bemerk5)
                {
                    _bemerk5 = value;
                    NotifyPropertyChanged("Bemerk5");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_6", Storage = "Bemerk6", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_6")]
        [StringLength(80)]
        public string Bemerk6
        {
            get
            {
                return _bemerk6;
            }
            set
            {
                if (value != _bemerk6)
                {
                    _bemerk6 = value;
                    NotifyPropertyChanged("Bemerk6");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_7", Storage = "Bemerk7", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_7")]
        [StringLength(80)]
        public string Bemerk7
        {
            get
            {
                return _bemerk7;
            }
            set
            {
                if (value != _bemerk7)
                {
                    _bemerk7 = value;
                    NotifyPropertyChanged("Bemerk7");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_8", Storage = "Bemerk8", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_8")]
        [StringLength(80)]
        public string Bemerk8
        {
            get
            {
                return _bemerk8;
            }
            set
            {
                if (value != _bemerk8)
                {
                    _bemerk8 = value;
                    NotifyPropertyChanged("Bemerk8");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_9", Storage = "Bemerk9", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_9")]
        [StringLength(80)]
        public string Bemerk9
        {
            get
            {
                return _bemerk9;
            }
            set
            {
                if (value != _bemerk9)
                {
                    _bemerk9 = value;
                    NotifyPropertyChanged("Bemerk9");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_10", Storage = "Bemerk10", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_10")]
        [StringLength(80)]
        public string Bemerk10
        {
            get
            {
                return _bemerk10;
            }
            set
            {
                if (value != _bemerk10)
                {
                    _bemerk10 = value;
                    NotifyPropertyChanged("Bemerk10");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_11", Storage = "Bemerk11", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_11")]
        [StringLength(80)]
        public string Bemerk11
        {
            get
            {
                return _bemerk11;
            }
            set
            {
                if (value != _bemerk11)
                {
                    _bemerk11 = value;
                    NotifyPropertyChanged("Bemerk11");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_12", Storage = "Bemerk12", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_12")]
        [StringLength(80)]
        public string Bemerk12
        {
            get
            {
                return _bemerk12;
            }
            set
            {
                if (value != _bemerk12)
                {
                    _bemerk12 = value;
                    NotifyPropertyChanged("Bemerk12");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_13", Storage = "Bemerk13", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_13")]
        [StringLength(80)]
        public string Bemerk13
        {
            get
            {
                return _bemerk13;
            }
            set
            {
                if (value != _bemerk13)
                {
                    _bemerk13 = value;
                    NotifyPropertyChanged("Bemerk13");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_14", Storage = "Bemerk14", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_14")]
        [StringLength(80)]
        public string Bemerk14
        {
            get
            {
                return _bemerk14;
            }
            set
            {
                if (value != _bemerk14)
                {
                    _bemerk14 = value;
                    NotifyPropertyChanged("Bemerk14");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_15", Storage = "Bemerk15", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_15")]
        [StringLength(80)]
        public string Bemerk15
        {
            get
            {
                return _bemerk15;
            }
            set
            {
                if (value != _bemerk15)
                {
                    _bemerk15 = value;
                    NotifyPropertyChanged("Bemerk15");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_16", Storage = "Bemerk16", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_16")]
        [StringLength(80)]
        public string Bemerk16
        {
            get
            {
                return _bemerk16;
            }
            set
            {
                if (value != _bemerk16)
                {
                    _bemerk16 = value;
                    NotifyPropertyChanged("Bemerk16");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_17", Storage = "Bemerk17", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_17")]
        [StringLength(80)]
        public string Bemerk17
        {
            get
            {
                return _bemerk17;
            }
            set
            {
                if (value != _bemerk17)
                {
                    _bemerk17 = value;
                    NotifyPropertyChanged("Bemerk17");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_18", Storage = "Bemerk18", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_18")]
        [StringLength(80)]
        public string Bemerk18
        {
            get
            {
                return _bemerk18;
            }
            set
            {
                if (value != _bemerk18)
                {
                    _bemerk18 = value;
                    NotifyPropertyChanged("Bemerk18");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_19", Storage = "Bemerk19", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_19")]
        [StringLength(80)]
        public string Bemerk19
        {
            get
            {
                return _bemerk19;
            }
            set
            {
                if (value != _bemerk19)
                {
                    _bemerk19 = value;
                    NotifyPropertyChanged("Bemerk19");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "bemerk_20", Storage = "Bemerk20", CanBeNull = true)]
        [TranslationKey("PLATE_REMARK_20")]
        [StringLength(80)]
        public string Bemerk20
        {
            get
            {
                return _bemerk20;
            }
            set
            {
                if (value != _bemerk20)
                {
                    _bemerk20 = value;
                    NotifyPropertyChanged("Bemerk20");
                }
            }
        }

        [XmlElement]
        [DataMember]
        [DataObjectField(false, false, true)]
        [Column(Name = "referenceNr", Storage = "ReferenceNr", CanBeNull = true)]
        [TranslationKey("PLATE_REFERENCE_NR")]
        [StringLength(80)]
        public string ReferenceNr
        {
            get
            {
                return _referenceNr;
            }
            set
            {
                if (value != _referenceNr)
                {
                    _referenceNr = value;
                    NotifyPropertyChanged("ReferenceNr");
                }
            }
        }

        [XmlElement]
        [TranslationKey("PLATE_STOCK_PLACE")]
        [DataObjectField(false, false, true)]
        [StringLength(81)]
        [Column(Name = "LagerplatzString", Storage = "LagerplatzString", CanBeNull = true)]
        public string LagerplatzString
        {
            get
            {
                return _lagerplatzString;
            }
            set
            {
                if (value != _lagerplatzString)
                {
                    _lagerplatzString = value;
                    NotifyPropertyChanged("LagerplatzString");
                }
            }
        }

        [XmlElement]
        [TranslationKey("PLATE_MACHINE_NR_TEXT")]
        [DataObjectField(false, false, true)]
        [StringLength(81)]
        [Column(Name = "MachineNummer", Storage = "MachineNummer", CanBeNull = true)]
        public string MachineNummer
        {
            get
            {
                return _machineNummer;
            }
            set
            {
                if (value != _machineNummer)
                {
                    _machineNummer = value;
                    NotifyPropertyChanged("MachineNummer");
                }
            }
        }

        [XmlElement]
        [TranslationKey("PLATE_ALLOW_MORE")]
        [Column(Name = "UeberbuchungErlaubt", Storage = "UeberbuchungErlaubt", CanBeNull = false)]
        public bool UeberbuchungErlaubt
        {
            get
            {
                return _ueberbuchungErlaubt;
            }
            set
            {
                if (value != _ueberbuchungErlaubt)
                {
                    _ueberbuchungErlaubt = value;
                    NotifyPropertyChanged("UeberbuchungErlaubt");
                }
            }
        }

        [XmlElement]
        [TranslationKey("PLATE_CRITICAL_AMOUNT")]
        [Column(Name = "MeldeBestand", Storage = "MeldeBestand", CanBeNull = false)]
        public int MeldeBestand
        {
            get
            {
                return _meldeBestand;
            }
            set
            {
                if (value != _meldeBestand)
                {
                    _meldeBestand = value;
                    NotifyPropertyChanged("MeldeBestand");
                }
            }
        }

        [XmlElement]
        [TranslationKey("PLATE_CREATION_DATE")]
        [Column(Name = "ErstellungsDatum", Storage = "ErstellungsDatum", CanBeNull = false)]
        public int ErstellungsDatum
        {
            get
            {
                return _erstellungsDatum;
            }
            set
            {
                if (value != _erstellungsDatum)
                {
                    _erstellungsDatum = value;
                    NotifyPropertyChanged("ErstellungsDatum");
                }
            }
        }

        [XmlElement]
        [TranslationKey("IsDeleted")]
        [Column(Name = "IsDeleted", Storage = "IsDeleted", CanBeNull = false)]
        public bool IsDeleted
        {
            get
            {
                return _isDeleted;
            }
            set
            {
                if (value != _isDeleted)
                {
                    _isDeleted = value;
                    NotifyPropertyChanged("IsDeleted");
                }
            }
        }

        [XmlElement(IsNullable = false)]
        [TranslationKey("DeleteDate")]
        [DataObjectField(false, false, true)]
        [Column(Name = "DeleteDate", Storage = "DeleteDate", CanBeNull = true)]
        public DateTime DeleteDate
        {
            get
            {
                return _deleteDate;
            }
            set
            {
                if (value != _deleteDate)
                {
                    _deleteDate = value;
                    NotifyPropertyChanged("DeleteDate");
                }
            }
        }

        [TranslationKey("ModifiedDate")]
        [DataObjectField(false, false, true)]
        [Column(Name = "ModifiedDate", Storage = "ModifiedDate", CanBeNull = true)]
        public DateTime? ModifiedDate
        {
            get
            {
                return _modifiedDate;
            }
            set
            {
                _modifiedDate = value;
            }
        }

        [TranslationKey("CreationDate")]
        [Column(Name = "CreationDate", Storage = "CreationDate", CanBeNull = false)]
        public DateTime CreationDate
        {
            get
            {
                return _creationDate;
            }
            set
            {
                _creationDate = value;
            }
        }

        public static SqlCrudInfo CrudData()
        {
            if (_crud == null)
            {
                WiCAM.Pn4000.Database.SqlCommandByAttributeBuilder instance = WiCAM.Pn4000.Database.SqlCommandByAttributeBuilder.Instance;
                _crud = new SqlCrudInfo(typeof(StockAuftragInfo));
                _crud.SqlCommands.Add("SqlSelect", SelectCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SqlSelectOne", SelectOneCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SqlDelete", DeleteCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SqlDeleteOne", DeleteOneCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SqlInsert", InsertCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SqlUpdate", UpdateCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", string.Empty));
                _crud.SqlCommands.Add("SelectSingleMaterial", SelectOneCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", SqlMaterialSelectWhere));
                _crud.SqlCommands.Add("SelectId", SqlSelectId + SqlMaterialSelectWhere);
                _crud.SqlCommands.Add("DeleteFiltered", DeleteOneCommand<StockAuftragInfo>("[dbo].[w_auftragspool]", " WHERE RTRIM(ISNULL([filter], N''))=@Filter;"));
            }

            return _crud;
        }
        public static string DeleteCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlDelete);
            stringBuilder.Append(fullTableName);
            AddWhereSql(stringBuilder, string.Empty, whereSql);
            return stringBuilder.ToString();
        }

        public static string DeleteOneCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlDelete);
            stringBuilder.Append(fullTableName);
            AddWhereSql(stringBuilder, _primaryKeyName, whereSql);
            return stringBuilder.ToString();
        }

        public static string InsertCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlInsert);
            stringBuilder.Append(fullTableName);
            AddInsertFields(stringBuilder, _fieldNames, _propertyNames, _primaryKeyName);
            return stringBuilder.ToString();
        }

        public static string UpdateCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlUpdate);
            stringBuilder.Append(fullTableName);
            stringBuilder.Append(SqlSet);
            AddUpdateFields(stringBuilder, _fieldNames, _propertyNames, _primaryKeyName);
            AddWhereSql(stringBuilder, _primaryKeyName, whereSql);
            return stringBuilder.ToString();
        }
        internal static void AddInsertFields(StringBuilder sb, string[] fieldNames, string[] propertyNames, string primaryKeyName)
        {
            sb.Append(SqlBracketRoundOpen);
            AddInsertFields(sb, SqlFieldFormat, fieldNames, primaryKeyName);
            sb.Append(SqlBracketRoundClose);
            sb.Append(SqlValues);
            sb.Append(SqlBracketRoundOpen);
            AddInsertParameters(sb, SqlFieldParameterFormat, propertyNames, primaryKeyName);
            sb.Append(SqlBracketRoundClose);
        }
        internal static void AddInsertFields(StringBuilder sb, string format, string[] fieldNames, string primaryKeyName)
        {
            int length = fieldNames.Length - 1;
            if (!string.IsNullOrEmpty(primaryKeyName))
            {
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    if (!primaryKeyName.Equals(fieldNames[i], StringComparison.OrdinalIgnoreCase))
                    {
                        AppendSelectField(sb, format, fieldNames[i], i, length);
                    }
                }
            }
            else
            {
                for (int j = 0; j < fieldNames.Length; j++)
                {
                    AppendSelectField(sb, format, fieldNames[j], j, length);
                }
            }
        }

        internal static void AddInsertParameters(StringBuilder sb, string format, string[] propertyNames, string primaryKeyName)
        {
            int length = propertyNames.Length - 1;
            if (!string.IsNullOrEmpty(primaryKeyName))
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    if (primaryKeyName != propertyNames[i])
                    {
                        AppendSelectField(sb, format, propertyNames[i], i, length);
                    }
                }
            }
            else
            {
                for (int j = 0; j < propertyNames.Length; j++)
                {
                    AppendSelectField(sb, format, propertyNames[j], j, length);
                }
            }
        }

        internal static void AddUpdateFields(StringBuilder sb, string[] fieldNames, string[] propertyNames, string primaryKeyName)
        {
            int length = propertyNames.Length - 1;
            if (!string.IsNullOrEmpty(primaryKeyName))
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    if (primaryKeyName != propertyNames[i])
                    {
                        AppendUpdateField(sb, fieldNames[i], propertyNames[i], i, length);
                    }
                }
            }
            else
            {
                for (int j = 0; j < propertyNames.Length; j++)
                {
                    AppendUpdateField(sb, fieldNames[j], propertyNames[j], j, length);
                }
            }
        }

        public static string SelectCommand<T>(string fullTableName) where T : class, IAuftragializeItem
        {
            return SelectCommand<T>(fullTableName, string.Empty);
        }
        public static string SelectOneCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlSelect);
            AddSelectFields(stringBuilder, _fieldNames, _propertyNames, _primaryKeyName);
            stringBuilder.Append(SqlFrom);
            stringBuilder.Append(fullTableName);
            AddWhereSql(stringBuilder, _primaryKeyName, whereSql);
            return stringBuilder.ToString();
        }
        internal static void AddWhereSql(StringBuilder sb, string primaryKeyName, string whereSql)
        {
            if (string.IsNullOrEmpty(whereSql))
            {
                sb.Append(WhereString(primaryKeyName));
            }
            else
            {
                sb.Append(whereSql);
            }
        }
        internal static string WhereString(string primaryKeyName)
        {
            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlWhere);
            AppendUpdateField(stringBuilder, primaryKeyName, primaryKeyName, 0, 0);
            return stringBuilder.ToString();
        }
        internal static void AppendUpdateField(StringBuilder sb, string fieldName, string propertyName, int index, int length)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, SqlFieldUpdateFormat, fieldName, propertyName);
            if (index < length)
            {
                sb.Append(WiCAM.Pn4000.Common.CS.Comma);
            }
        }

        private static PropertyInfo[] _properties;

        private static string[] _fieldNames;

        private static string[] _propertyNames;

        private static string _primaryKeyName;

        public static readonly string SqlSelect = "SELECT ";

        public static readonly string SqlSelectTop = "SELECT TOP ({0}) ";

        public static readonly string SqlInsert = "INSERT INTO ";

        public static readonly string SqlFrom = " FROM ";

        public static readonly string SqlDelete = "DELETE " + SqlFrom;

        public static readonly string SqlUpdate = "UPDATE ";

        public static readonly string SqlWhere = " WHERE ";

        public static readonly string SqlAnd = " AND ";

        public static readonly string SqlOr = " OR ";

        public static readonly string SqlSet = " SET ";

        public static readonly string SqlValues = " VALUES ";

        public static readonly string SqlFieldFormat = "[{0}]";
        public static readonly string SqlFieldUpdateFormat = "[{0}]=@{1}";
        public static readonly string SqlFieldParameterFormat = "@{0}";

        public static readonly string SqlBracketRoundOpen = " (";

        public static readonly string SqlBracketRoundClose = ") ";

        public static readonly string SqlDateFormat = "CONVERT(datetime,N'{0}', 126)";

        public static readonly string SqlStringLikeFormat = "{0} LIKE '%{1}%'";

        public static readonly string SqlConvertToStringLikeFormat = "CONVERT({0}, 'System.String') LIKE '%{1}%'";

        public static readonly string SqlConnectionTest = "SELECT 1;";

        private static Type _objectType;


        public static string SelectCommand<T>(string fullTableName, string whereSql) where T : class, IAuftragializeItem
        {
            CheckObjectType<T>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(SqlSelect);
            AddSelectFields(stringBuilder, _fieldNames, _propertyNames, _primaryKeyName);
            stringBuilder.Append(SqlFrom);
            stringBuilder.Append(fullTableName);
            if (!string.IsNullOrEmpty(whereSql))
            {
                stringBuilder.Append(whereSql);
            }

            return stringBuilder.ToString();
        }
        private static void CheckObjectType<T>() where T : class, IAuftragializeItem
        {
            if (!(_objectType != typeof(T)))
            {
                return;
            }

            _objectType = typeof(T);
            _properties = typeof(T).GetProperties();
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            _primaryKeyName = null;
            PropertyInfo[] properties = _properties;
            for (int i = 0; i < properties.Length; i++)
            {
                object obj = CustomAttributeHelper.FindCustomAttribute(properties[i], typeof(ColumnAttribute));
                if (obj != null)
                {
                    ColumnAttribute columnAttribute = obj as ColumnAttribute;
                    list.Add(columnAttribute.Name);
                    list2.Add(StringHelper.FirstToUpper(columnAttribute.Storage));
                    if (columnAttribute.IsPrimaryKey)
                    {
                        _primaryKeyName = StringHelper.FirstToUpper(columnAttribute.Name);
                    }
                }
            }

            _fieldNames = list.ToArray();
            _propertyNames = list2.ToArray();
        }

        internal static void AddSelectFields(StringBuilder sb, string[] fieldNames, string[] propertyNames, string primaryKeyName)
        {
            AddSelectFields(sb, SqlFieldFormat, fieldNames);
        }

        internal static void AddSelectFields(StringBuilder sb, string format, string[] fieldNames)
        {
            int length = fieldNames.Length - 1;
            for (int i = 0; i < fieldNames.Length; i++)
            {
                AppendSelectField(sb, format, fieldNames[i], i, length);
            }
        }

        internal static void AppendSelectField(StringBuilder sb, string format, string fieldName, int index, int length)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, format, fieldName);
            if (index < length)
            {
                sb.Append(WiCAM.Pn4000.Common.CS.Comma);
            }
        }

        public static void Initialize(StockAuftragInfo smi, bool isInch)
        {
            smi._AuftragsNummer = "240000";
            smi._plThick = (isInch ? 0.05906 : 1.5);
            smi._maxX = (isInch ? 98.4252 : 2500.0);
            smi._maxY = (isInch ? 49.2126 : 1250.0);
            smi._amount = 999999;
            smi._freigabe = 1;
            smi._bevorzug = 1.0;
            smi._plTyp = 1;
        }

        public bool IsEqual(StockAuftragInfo smi)
        {
            if (smi.Mpid == Mpid)
            {
                return true;
            }

            if (IsEqual(smi.AuftragsNummer, AuftragsNummer) && IsEqual(smi.IdentNr, IdentNr) && IsEqual(smi.Bezeichnung, Bezeichnung) && IsEqual(smi.Filter, Filter))
            {
                return smi.Lagerplatz == Lagerplatz;
            }

            return false;
        }

        public void ControlMachinesString()
        {
            string text = ";";
            if (!string.IsNullOrEmpty(_machineNummer))
            {
                string[] value = _machineNummer.Split(text.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                _machineNummer = string.Join(text, value) + text;
            }
        }

        private static bool IsEqual(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                s1 = string.Empty;
            }

            if (string.IsNullOrEmpty(s2))
            {
                s2 = string.Empty;
            }

            return s1.Equals(s2, StringComparison.OrdinalIgnoreCase);
        }

        public StockAuftragInfo()
        {
            _pcName = string.Empty;
            _machineNummer = __defaultMachineNumberText;
        }

        public void Auftragialize(IDataReader rd)
        {
            try
            {
                _mpid = rd.GetInt32(0);
                _AuftragsNummer = rd.GetString(1);
                _identNr = ReadString(rd, 2);
                _bezeichnung = ReadString(rd, 3);
                if (!rd.IsDBNull(4))
                {
                    _bemerk1 = rd.GetString(4);
                }

                if (!rd.IsDBNull(5))
                {
                    _bemerk2 = rd.GetString(5);
                }

                if (!rd.IsDBNull(6))
                {
                    _bemerk3 = rd.GetString(6);
                }

                _plThick = rd.GetDouble(7);
                _maxX = rd.GetDouble(8);
                _maxY = rd.GetDouble(9);
                _matNumber = rd.GetInt32(10);
                _plTyp = rd.GetInt32(11);
                _arNumber = rd.GetInt32(12);
                _res1 = rd.GetDouble(13);
                _res2 = rd.GetDouble(14);
                _res3 = rd.GetDouble(15);
                _amount = rd.GetInt32(16);
                _freigabe = rd.GetInt32(17);
                _lagerplatz = rd.GetInt32(18);
                _bevorzug = rd.GetDouble(19);
                _res4 = rd.GetDouble(20);
                _res5 = rd.GetDouble(21);
                _machine = rd.GetInt32(22);
                _bewFactor = rd.GetDouble(23);
                _minX = rd.GetDouble(24);
                _minY = rd.GetDouble(25);
                _stepX = rd.GetDouble(26);
                _stepY = rd.GetDouble(27);
                _amountRes = rd.GetInt32(28);
                _locked = rd.GetInt32(29);
                _pcName = rd.GetString(30);
                if (!rd.IsDBNull(31))
                {
                    _filter = rd.GetString(31);
                }

                if (!rd.IsDBNull(32))
                {
                    _bemerk4 = rd.GetString(32);
                }

                if (!rd.IsDBNull(33))
                {
                    _bemerk5 = rd.GetString(33);
                }

                if (!rd.IsDBNull(34))
                {
                    _bemerk6 = rd.GetString(34);
                }

                if (!rd.IsDBNull(35))
                {
                    _bemerk7 = rd.GetString(35);
                }

                if (!rd.IsDBNull(36))
                {
                    _bemerk8 = rd.GetString(36);
                }

                if (!rd.IsDBNull(37))
                {
                    _bemerk9 = rd.GetString(37);
                }

                if (!rd.IsDBNull(38))
                {
                    _bemerk10 = rd.GetString(38);
                }

                if (!rd.IsDBNull(39))
                {
                    _bemerk11 = rd.GetString(39);
                }

                if (!rd.IsDBNull(40))
                {
                    _bemerk12 = rd.GetString(40);
                }

                if (!rd.IsDBNull(41))
                {
                    _bemerk13 = rd.GetString(41);
                }

                if (!rd.IsDBNull(42))
                {
                    _bemerk14 = rd.GetString(42);
                }

                if (!rd.IsDBNull(43))
                {
                    _bemerk15 = rd.GetString(43);
                }

                if (!rd.IsDBNull(44))
                {
                    _bemerk16 = rd.GetString(44);
                }

                if (!rd.IsDBNull(45))
                {
                    _bemerk17 = rd.GetString(45);
                }

                if (!rd.IsDBNull(46))
                {
                    _bemerk18 = rd.GetString(46);
                }

                if (!rd.IsDBNull(47))
                {
                    _bemerk19 = rd.GetString(47);
                }

                if (!rd.IsDBNull(48))
                {
                    _bemerk20 = rd.GetString(48);
                }

                if (!rd.IsDBNull(49))
                {
                    _referenceNr = rd.GetString(49);
                }

                if (!rd.IsDBNull(50))
                {
                    _lagerplatzString = rd.GetString(50);
                }

                if (!rd.IsDBNull(51))
                {
                    _machineNummer = rd.GetString(51);
                }

                _ueberbuchungErlaubt = rd.GetBoolean(52);
                _meldeBestand = rd.GetInt32(53);
                _erstellungsDatum = rd.GetInt32(54);
                _isDeleted = rd.GetBoolean(55);
                if (!rd.IsDBNull(56))
                {
                    _deleteDate = rd.GetDateTime(56);
                }
                else
                {
                    _deleteDate = DateTime.Today;
                }

                if (!rd.IsDBNull(57))
                {
                    _modifiedDate = rd.GetDateTime(57);
                }

                _creationDate = rd.GetDateTime(58);
                if (_machineNummer == null)
                {
                    _machineNummer = "0;";
                    return;
                }

                _machineNummer = _machineNummer.Trim();
                if (string.IsNullOrEmpty(_machineNummer))
                {
                    _machineNummer = "0;";
                }
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
        }

        private string ReadString(IDataReader rd, int index)
        {
            if (rd.IsDBNull(index))
            {
                return string.Empty;
            }

            return rd.GetString(index);
        }
    }
}
