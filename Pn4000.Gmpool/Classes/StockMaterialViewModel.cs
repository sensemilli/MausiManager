using System;
using System.Runtime.CompilerServices;
using System.Threading;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	public class StockMaterialViewModel : ViewModelBase
	{
		private int _mpid;

		private string _plName;

		private string _identNr;

		private string _bezeichnung;

		private string _bemerk1;

		private string _bemerk2;

		private string _bemerk3;

		private string _plThick;

		private string _maxX;

		private string _maxY;

		private int _matNumber;

		private int _plTyp;

		private int _arNumber;

		private string _res1;

		private string _res2;

		private string _res3;

		private int _amount;

		private int _freigabe;

		private int _lagerplatz;

		private string _bevorzug;

		private string _res4;

		private string _res5;

		private int _machine;

		private string _bewFactor;

		private string _minX;

		private string _minY;

		private string _stepX;

		private string _stepY;

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

		private DateTime? _ModifiedDate;

		private DateTime _CreationDate;

		public int Amount
		{
			get
			{
				return this._amount;
			}
			set
			{
				if (value != this._amount)
				{
					this._amount = value;
					base.NotifyPropertyChanged("Amount");
				}
			}
		}

		public int AmountRes
		{
			get
			{
				return this._amountRes;
			}
			set
			{
				if (value != this._amountRes)
				{
					this._amountRes = value;
					base.NotifyPropertyChanged("AmountRes");
				}
			}
		}

		public int ArNumber
		{
			get
			{
				return this._arNumber;
			}
			set
			{
				if (value != this._arNumber)
				{
					this._arNumber = value;
					base.NotifyPropertyChanged("ArNumber");
				}
			}
		}

		public string Bemerk1
		{
			get
			{
				return this._bemerk1;
			}
			set
			{
				if (value != this._bemerk1)
				{
					this._bemerk1 = value;
					base.NotifyPropertyChanged("Bemerk1");
				}
			}
		}

		public string Bemerk10
		{
			get
			{
				return this._bemerk10;
			}
			set
			{
				if (value != this._bemerk10)
				{
					this._bemerk10 = value;
					base.NotifyPropertyChanged("Bemerk10");
				}
			}
		}

		public string Bemerk11
		{
			get
			{
				return this._bemerk11;
			}
			set
			{
				if (value != this._bemerk11)
				{
					this._bemerk11 = value;
					base.NotifyPropertyChanged("Bemerk11");
				}
			}
		}

		public string Bemerk12
		{
			get
			{
				return this._bemerk12;
			}
			set
			{
				if (value != this._bemerk12)
				{
					this._bemerk12 = value;
					base.NotifyPropertyChanged("Bemerk12");
				}
			}
		}

		public string Bemerk13
		{
			get
			{
				return this._bemerk13;
			}
			set
			{
				if (value != this._bemerk13)
				{
					this._bemerk13 = value;
					base.NotifyPropertyChanged("Bemerk13");
				}
			}
		}

		public string Bemerk14
		{
			get
			{
				return this._bemerk14;
			}
			set
			{
				if (value != this._bemerk14)
				{
					this._bemerk14 = value;
					base.NotifyPropertyChanged("Bemerk14");
				}
			}
		}

		public string Bemerk15
		{
			get
			{
				return this._bemerk15;
			}
			set
			{
				if (value != this._bemerk15)
				{
					this._bemerk15 = value;
					base.NotifyPropertyChanged("Bemerk15");
				}
			}
		}

		public string Bemerk16
		{
			get
			{
				return this._bemerk16;
			}
			set
			{
				if (value != this._bemerk16)
				{
					this._bemerk16 = value;
					base.NotifyPropertyChanged("Bemerk16");
				}
			}
		}

		public string Bemerk17
		{
			get
			{
				return this._bemerk17;
			}
			set
			{
				if (value != this._bemerk17)
				{
					this._bemerk17 = value;
					base.NotifyPropertyChanged("Bemerk17");
				}
			}
		}

		public string Bemerk18
		{
			get
			{
				return this._bemerk18;
			}
			set
			{
				if (value != this._bemerk18)
				{
					this._bemerk18 = value;
					base.NotifyPropertyChanged("Bemerk18");
				}
			}
		}

		public string Bemerk19
		{
			get
			{
				return this._bemerk19;
			}
			set
			{
				if (value != this._bemerk19)
				{
					this._bemerk19 = value;
					base.NotifyPropertyChanged("Bemerk19");
				}
			}
		}

		public string Bemerk2
		{
			get
			{
				return this._bemerk2;
			}
			set
			{
				if (value != this._bemerk2)
				{
					this._bemerk2 = value;
					base.NotifyPropertyChanged("Bemerk2");
				}
			}
		}

		public string Bemerk20
		{
			get
			{
				return this._bemerk20;
			}
			set
			{
				if (value != this._bemerk20)
				{
					this._bemerk20 = value;
					base.NotifyPropertyChanged("Bemerk20");
				}
			}
		}

		public string Bemerk3
		{
			get
			{
				return this._bemerk3;
			}
			set
			{
				if (value != this._bemerk3)
				{
					this._bemerk3 = value;
					base.NotifyPropertyChanged("Bemerk3");
				}
			}
		}

		public string Bemerk4
		{
			get
			{
				return this._bemerk4;
			}
			set
			{
				if (value != this._bemerk4)
				{
					this._bemerk4 = value;
					base.NotifyPropertyChanged("Bemerk4");
				}
			}
		}

		public string Bemerk5
		{
			get
			{
				return this._bemerk5;
			}
			set
			{
				if (value != this._bemerk5)
				{
					this._bemerk5 = value;
					base.NotifyPropertyChanged("Bemerk5");
				}
			}
		}

		public string Bemerk6
		{
			get
			{
				return this._bemerk6;
			}
			set
			{
				if (value != this._bemerk6)
				{
					this._bemerk6 = value;
					base.NotifyPropertyChanged("Bemerk6");
				}
			}
		}

		public string Bemerk7
		{
			get
			{
				return this._bemerk7;
			}
			set
			{
				if (value != this._bemerk7)
				{
					this._bemerk7 = value;
					base.NotifyPropertyChanged("Bemerk7");
				}
			}
		}

		public string Bemerk8
		{
			get
			{
				return this._bemerk8;
			}
			set
			{
				if (value != this._bemerk8)
				{
					this._bemerk8 = value;
					base.NotifyPropertyChanged("Bemerk8");
				}
			}
		}

		public string Bemerk9
		{
			get
			{
				return this._bemerk9;
			}
			set
			{
				if (value != this._bemerk9)
				{
					this._bemerk9 = value;
					base.NotifyPropertyChanged("Bemerk9");
				}
			}
		}

		public string Bevorzug
		{
			get
			{
				return this._bevorzug;
			}
			set
			{
				if (value != this._bevorzug)
				{
					this._bevorzug = value;
					base.NotifyPropertyChanged("Bevorzug");
				}
			}
		}

		public bool Bevorzugt
		{
			get
			{
				return StringHelper.ToInt(this._bevorzug) > 0;
			}
			set
			{
				this._bevorzug = (value ? "1.0" : "0.0");
				base.NotifyPropertyChanged("Bevorzugt");
			}
		}

		public string BewFactor
		{
			get
			{
				return this._bewFactor;
			}
			set
			{
				if (value != this._bewFactor)
				{
					this._bewFactor = value;
					base.NotifyPropertyChanged("BewFactor");
				}
			}
		}

		public string Bezeichnung
		{
			get
			{
				return this._bezeichnung;
			}
			set
			{
				if (value != this._bezeichnung)
				{
					this._bezeichnung = value;
					base.NotifyPropertyChanged("Bezeichnung");
				}
			}
		}

		public DateTime CreationDate
		{
			get;
			set;
		}

		public DateTime DeleteDate
		{
			get
			{
				return this._deleteDate;
			}
			set
			{
				if (value != this._deleteDate)
				{
					this._deleteDate = value;
					base.NotifyPropertyChanged("DeleteDate");
				}
			}
		}

		public int ErstellungsDatum
		{
			get
			{
				return this._erstellungsDatum;
			}
			set
			{
				if (value != this._erstellungsDatum)
				{
					this._erstellungsDatum = value;
					base.NotifyPropertyChanged("ErstellungsDatum");
				}
			}
		}

		public string Filter
		{
			get
			{
				return this._filter;
			}
			set
			{
				if (value != this._filter)
				{
					this._filter = value;
					base.NotifyPropertyChanged("Filter");
				}
			}
		}

		public int Freigabe
		{
			get
			{
				return this._freigabe;
			}
			set
			{
				if (value != this._freigabe)
				{
					this._freigabe = value;
					base.NotifyPropertyChanged("Freigabe");
				}
			}
		}

		public string IdentNr
		{
			get
			{
				return this._identNr;
			}
			set
			{
				if (value != this._identNr)
				{
					this._identNr = value;
					base.NotifyPropertyChanged("IdentNr");
				}
			}
		}

		public bool IsDeleted
		{
			get
			{
				return this._isDeleted;
			}
			set
			{
				if (value != this._isDeleted)
				{
					this._isDeleted = value;
					base.NotifyPropertyChanged("IsDeleted");
				}
			}
		}

		public int Lagerplatz
		{
			get
			{
				return this._lagerplatz;
			}
			set
			{
				if (value != this._lagerplatz)
				{
					this._lagerplatz = value;
					base.NotifyPropertyChanged("Lagerplatz");
				}
			}
		}

		public string LagerplatzString
		{
			get
			{
				return this._lagerplatzString;
			}
			set
			{
				if (value != this._lagerplatzString)
				{
					this._lagerplatzString = value;
					base.NotifyPropertyChanged("LagerplatzString");
				}
			}
		}

		public int Locked
		{
			get
			{
				return this._locked;
			}
			set
			{
				if (value != this._locked)
				{
					this._locked = value;
					base.NotifyPropertyChanged("Locked");
				}
			}
		}

		public int Machine
		{
			get
			{
				return this._machine;
			}
			set
			{
				if (value != this._machine)
				{
					this._machine = value;
					base.NotifyPropertyChanged("Machine");
				}
			}
		}

		public string MachineNummer
		{
			get
			{
				return this._machineNummer;
			}
			set
			{
				if (value != this._machineNummer)
				{
					this._machineNummer = value;
					base.NotifyPropertyChanged("MachineNummer");
				}
			}
		}

		public string MaterialName
		{
			get
			{
				return this._materialName;
			}
			set
			{
				if (value != this._materialName)
				{
					this._materialName = value;
					base.NotifyPropertyChanged("MaterialName");
				}
			}
		}

		public int MatNumber
		{
			get
			{
				return this._matNumber;
			}
			set
			{
				if (value != this._matNumber)
				{
					this._matNumber = value;
					base.NotifyPropertyChanged("MatNumber");
				}
			}
		}

		public string MaxX
		{
			get
			{
				return this._maxX;
			}
			set
			{
				if (value != this._maxX)
				{
					this._maxX = value;
					base.NotifyPropertyChanged("MaxX");
				}
			}
		}

		public string MaxY
		{
			get
			{
				return this._maxY;
			}
			set
			{
				if (value != this._maxY)
				{
					this._maxY = value;
					base.NotifyPropertyChanged("MaxY");
				}
			}
		}

		public int MeldeBestand
		{
			get
			{
				return this._meldeBestand;
			}
			set
			{
				if (value != this._meldeBestand)
				{
					this._meldeBestand = value;
					base.NotifyPropertyChanged("MeldeBestand");
				}
			}
		}

		public string MinX
		{
			get
			{
				return this._minX;
			}
			set
			{
				if (value != this._minX)
				{
					this._minX = value;
					base.NotifyPropertyChanged("MinX");
				}
			}
		}

		public string MinY
		{
			get
			{
				return this._minY;
			}
			set
			{
				if (value != this._minY)
				{
					this._minY = value;
					base.NotifyPropertyChanged("MinY");
				}
			}
		}

		public DateTime ModifiedDate
		{
			get;
			set;
		}

		public int Mpid
		{
			get
			{
				return this._mpid;
			}
			set
			{
				if (value != this._mpid)
				{
					this._mpid = value;
				}
			}
		}

		public bool OrderNew
		{
			get
			{
				return StringHelper.ToInt(this._res1) > 0;
			}
			set
			{
				this._res1 = (value ? "1.0" : "0.0");
				base.NotifyPropertyChanged("OrderNew");
			}
		}

		public StockMaterialInfo OriginalMaterial
		{
			get;
			set;
		}

		public string PcName
		{
			get
			{
				return this._pcName;
			}
			set
			{
				if (value != this._pcName)
				{
					this._pcName = value;
					base.NotifyPropertyChanged("PcName");
				}
			}
		}

		public string PlName
		{
			get
			{
				return this._plName;
			}
			set
			{
				if (value != this._plName)
				{
					this._plName = value;
					base.NotifyPropertyChanged("PlName");
				}
			}
		}

		public string PlThick
		{
			get
			{
				return this._plThick;
			}
			set
			{
				if (value != this._plThick)
				{
					this._plThick = value;
					base.NotifyPropertyChanged("PlThick");
				}
			}
		}

		public int PlTyp
		{
			get
			{
				return this._plTyp;
			}
			set
			{
				if (value != this._plTyp)
				{
					this._plTyp = value;
					Action<int> action = this.PlateTypeChanged;
					if (action != null)
					{
						action(this._plTyp);
					}
					else
					{
					}
					base.NotifyPropertyChanged("PlTyp");
				}
			}
		}

		public string ReferenceNr
		{
			get
			{
				return this._referenceNr;
			}
			set
			{
				if (value != this._referenceNr)
				{
					this._referenceNr = value;
					base.NotifyPropertyChanged("ReferenceNr");
				}
			}
		}

		public string Res1
		{
			get
			{
				return this._res1;
			}
			set
			{
				if (value != this._res1)
				{
					this._res1 = value;
					base.NotifyPropertyChanged("Res1");
				}
			}
		}

		public string Res2
		{
			get
			{
				return this._res2;
			}
			set
			{
				if (value != this._res2)
				{
					this._res2 = value;
					base.NotifyPropertyChanged("Res2");
				}
			}
		}

		public string Res3
		{
			get
			{
				return this._res3;
			}
			set
			{
				if (value != this._res3)
				{
					this._res3 = value;
					base.NotifyPropertyChanged("Res3");
				}
			}
		}

		public string Res4
		{
			get
			{
				return this._res4;
			}
			set
			{
				if (value != this._res4)
				{
					this._res4 = value;
					base.NotifyPropertyChanged("Res4");
				}
			}
		}

		public string Res5
		{
			get
			{
				return this._res5;
			}
			set
			{
				if (value != this._res5)
				{
					this._res5 = value;
					base.NotifyPropertyChanged("Res5");
				}
			}
		}

		public string StepX
		{
			get
			{
				return this._stepX;
			}
			set
			{
				if (value != this._stepX)
				{
					this._stepX = value;
					base.NotifyPropertyChanged("StepX");
				}
			}
		}

		public string StepY
		{
			get
			{
				return this._stepY;
			}
			set
			{
				if (value != this._stepY)
				{
					this._stepY = value;
					base.NotifyPropertyChanged("StepY");
				}
			}
		}

		public bool UeberbuchungErlaubt
		{
			get
			{
				return this._ueberbuchungErlaubt;
			}
			set
			{
				if (value != this._ueberbuchungErlaubt)
				{
					this._ueberbuchungErlaubt = value;
					base.NotifyPropertyChanged("UeberbuchungErlaubt");
				}
			}
		}

		public StockMaterialViewModel()
		{
		}

		public event Action<int> PlateTypeChanged;
	}
}