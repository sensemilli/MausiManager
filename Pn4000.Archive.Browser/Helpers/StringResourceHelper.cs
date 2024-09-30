using System;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Helpers
{
	internal class StringResourceHelper : ResourceHelperBase
	{
		private static StringResourceHelper _instance;

		public static StringResourceHelper Instance
		{
			get
			{
				if (StringResourceHelper._instance == null)
				{
					Logger.Verbose("Initialize StringResourceHelper");
					StringResourceHelper._instance = new StringResourceHelper();
				}
				return StringResourceHelper._instance;
			}
		}

		private StringResourceHelper() : base("..\\Resources\\StringResources")
		{
		}
	}
}