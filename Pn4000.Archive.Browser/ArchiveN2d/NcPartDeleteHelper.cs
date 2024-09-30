using System;
using WiCAM.Pn4000.Archive;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.Archive.Browser.ArchiveN2d
{
	internal class NcPartDeleteHelper
	{
		private readonly ArchiveStructureHelper _structureHelper;

		public NcPartDeleteHelper(ArchiveStructureHelper structureHelper)
		{
			this._structureHelper = structureHelper;
		}

		public void DeleteFiles(NcPartInfo part)
		{
			string str = this.PathByType(part, ArchiveFolderType.n2d);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("NC program '{0}' can not be deleted in file system!", new object[] { str });
			}
			str = this.PathByType(part, ArchiveFolderType.c2d);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("NC program '{0}' can not be deleted in file system!", new object[] { str });
			}
			str = this.PathByType(part, ArchiveFolderType.c2t);
			if (!IOHelper.FileDelete(str))
			{
				Logger.Error("NC program '{0}' can not be deleted in file system!", new object[] { str });
			}
		}

		public string PathByType(NcPartInfo item, ArchiveFolderType folderType)
		{
			return this._structureHelper.PathByType(item.PartName, item.ArchiveNumber, folderType);
		}
	}
}