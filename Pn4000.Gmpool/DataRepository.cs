using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Database;
using WiCAM.Pn4000.JobManager.Views;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.Gmpool
{
	internal class DataRepository
	{
		private static DataRepository _instance;

		private string _connectionString;

		private string _filter;

		public static DataRepository Instance
		{
			get
			{
				if (DataRepository._instance == null)
				{
					Logger.Verbose("Initialize DataRepository");
					DataRepository._instance = new DataRepository();
				}
				return DataRepository._instance;
			}
		}

		public bool IsDatabase
		{
			get;
			private set;
		}

		public List<MaterialArtInfo> MaterialArts
		{
			get;
			set;
		}

		public List<StockMaterialInfo> Materials
		{
			get;
			set;
		}
        public List<MaterialArtInfo> AuftragArts
        {
            get;
            set;
        }
        public List<StockAuftragInfo> Auftragials
        {
            get;
            set;
        }

        private DataRepository()
		{
			DataManager instance = DataManager.Instance;
			this.IsDatabase = instance.IsInitialised;
			if (this.IsDatabase)
			{
				PnDatabaseConnectionsManager pnDatabaseConnectionsManager = new PnDatabaseConnectionsManager();
				this._connectionString = pnDatabaseConnectionsManager.GMPOOL;
				this._filter = pnDatabaseConnectionsManager.GmpoolFilter;
			}
			Task<List<StockMaterialInfo>> task = Task.Run<List<StockMaterialInfo>>(() => instance.ReadStockMaterials());
			this.MaterialArts = new List<MaterialArtInfo>((new MaterialArtManager()).MaterialArts());
			Task.WhenAll<List<StockMaterialInfo>>(new Task<List<StockMaterialInfo>>[] { task });
			this.Materials = task.Result;
			this.UpdateMaterialNameParallel();

		}

      


        public void CalculateValues(StockMaterialInfo material)
		{
			material.Res2 = material.MaxX * material.MaxY / 1000000;
			MaterialArtInfo materialArtInfo = this.FindMaterialArt(material.MatNumber);
			if (materialArtInfo != null)
			{
				material.MaterialName = materialArtInfo.Name;
				if (material.PlThick > 0)
				{
					material.Res3 = material.Res2 * material.PlThick * materialArtInfo.Density;
				}
			}
		}

		private void CalculateValues(StockMaterialInfo material, double density)
		{
			if (material.PlTyp == 1)
			{
				if (material.Res2 <= 0.1)
				{
					material.Res2 = material.MaxX * material.MaxY / 1000000;
				}
				if (material.Res3 <= 0.1 && material.PlThick > 0)
				{
					material.Res3 = material.Res2 * material.PlThick * density;
				}
			}
		}

		public MaterialArtInfo FindMaterialArt(int materialNumber)
		{
			return this.MaterialArts.Find((MaterialArtInfo x) => x.Number == materialNumber);
		}

		public StockMaterialInfo Read(StockMaterialInfo item)
		{
			if (!this.IsDatabase)
			{
				return item;
			}
			return (new StockMaterialDatabaseHelper(this._connectionString, this._filter)).StockMaterialSelectOne(item.Mpid);
		}

		private void UpdateMaterialNameParallel()
		{
			if (EnumerableHelper.IsNullOrEmpty(this.Materials))
			{
				return;
			}
			if (EnumerableHelper.IsNullOrEmpty(this.MaterialArts))
			{
				return;
			}
			Parallel.For(0, this.MaterialArts.Count, (int i) => {
				MaterialArtInfo item = this.MaterialArts[i];
				List<StockMaterialInfo> stockMaterialInfos = this.Materials.FindAll((StockMaterialInfo x) => x.MatNumber == item.Number);
				if (!EnumerableHelper.IsNullOrEmpty(stockMaterialInfos))
				{
					foreach (StockMaterialInfo name in stockMaterialInfos)
					{
						name.MaterialName = item.Name;
						this.CalculateValues(name, item.Density);
					}
				}
			});
			List<StockMaterialInfo> stockMaterialInfos1 = this.Materials.FindAll((StockMaterialInfo x) => x.MatNumber == 0);
			if (!EnumerableHelper.IsNullOrEmpty(stockMaterialInfos1))
			{
				foreach (StockMaterialInfo stockMaterialInfo in stockMaterialInfos1)
				{
					this.CalculateValues(stockMaterialInfo, 0);
				}
			}
		}
	}
}