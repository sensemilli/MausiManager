using SmartAssembly.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.JobManager.Views
{


    [DoNotPruneType]
    [DoNotObfuscateType]
    public interface IAuftragializeItem
    {
        void Auftragialize(IDataReader rd);
    }
}