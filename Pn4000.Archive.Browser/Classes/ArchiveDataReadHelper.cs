using System;
using WiCAM.Pn4000.Archive.Browser.Helpers;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.Classes
{
	internal class ArchiveDataReadHelper : IDisposable
	{
		private static ArchiveDataReadHelper _instance;

		public static ArchiveDataReadHelper Instance
		{
			get
			{
				if (ArchiveDataReadHelper._instance == null)
				{
					Logger.Verbose("Initialize ArchiveDataReadHelper");
					ArchiveDataReadHelper._instance = new ArchiveDataReadHelper();
				}
				return ArchiveDataReadHelper._instance;
			}
		}

		private ArchiveDataReadHelper()
		{
			DataManagerBase.CancelButtonText = StringResourceHelper.Instance.FindString("ButtonCancel");
		}

		public void Dispose()
		{
		}
	}
}