using System;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal class MaterialLockedException : Exception
	{
		public StockMaterialInfo Material
		{
			get;
			set;
		}

		public MaterialLockedException(StockMaterialInfo item)
		{
			this.Material = item;
		}
	}
}