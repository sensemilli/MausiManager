using System;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	internal class AppArguments : ApplicationArgumentsBase
	{
		internal const int __currentErg0Version = 2;

		private static AppArguments _instance;

		private string __keyArNumber = "archive";

		private string __keyMultiSel = "multiselect";

		private string __keyNameFilter = "filter";

		private string __keyArType = "type";

		private string __keyErg0Ver = "erg0";

		public bool HasToExpandAll
		{
			get;
			private set;
		}

		public static AppArguments Instance
		{
			get
			{
				if (AppArguments._instance == null)
				{
					Logger.Verbose("Initialize AppArguments");
					AppArguments._instance = new AppArguments();
				}
				return AppArguments._instance;
			}
		}

		public bool IsMultiselect
		{
			get;
			private set;
		}

		public int MultiselectValue
		{
			get;
			private set;
		}

		public string SearchString
		{
			get;
			set;
		}

		private AppArguments()
		{
			base.Initialize(Environment.CommandLine);
			this.IsMultiselect = this.CheckIsMultiselect();
			this.SearchString = base.AgrumentByName(this.__keyNameFilter);
			this.HasToExpandAll = false;
			string str = base.AgrumentByName("expand");
			if (!string.IsNullOrEmpty(str))
			{
				this.HasToExpandAll = str.Equals("1", StringComparison.CurrentCultureIgnoreCase);
			}
		}

		public int ArchiveNumber()
		{
			string str = base.AgrumentByName(this.__keyArNumber);
			if (string.IsNullOrEmpty(str))
			{
				return 1;
			}
			return Convert.ToInt32(str);
		}

		public ArchiveFileType ArchiveType()
		{
			string str = base.AgrumentByName(this.__keyArType);
			if (string.IsNullOrEmpty(str))
			{
				return ArchiveFileType.N2D;
			}
			return (ArchiveFileType)System.Enum.Parse(typeof(ArchiveFileType), str.ToUpper());
		}

		private bool CheckIsMultiselect()
		{
			string str = base.AgrumentByName(this.__keyMultiSel);
			if (string.IsNullOrEmpty(str))
			{
				return false;
			}
			this.MultiselectValue = StringHelper.ToInt(str);
			return this.MultiselectValue > 0;
		}

		public int Erg0Version()
		{
			string str = base.AgrumentByName(this.__keyErg0Ver);
			if (string.IsNullOrEmpty(str))
			{
				return 2;
			}
			return StringHelper.ToInt(str);
		}
	}
}