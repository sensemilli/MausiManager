using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiCAM.Pn4000.Materials;

namespace WiCAM.Pn4000.JobManager.Views
{

    public interface IStockAuftragHelper
    {
        List<StockAuftragInfo> ReadMaterials();

        StockAuftragInfo SelectMaterial(StockAuftragInfo smi);

        bool DeleteMaterial(StockAuftragInfo smi);

        bool DeleteAllMaterials();

        bool InsertMaterial(StockAuftragInfo smi);

        bool UpdateMaterial(StockAuftragInfo smi);

        bool WriteFile(IEnumerable<StockAuftragInfo> list, string path);

        bool SaveAll(IEnumerable<StockAuftragInfo> list);

        List<StockAuftragInfo> ReadStockAuftrags();
    }
}