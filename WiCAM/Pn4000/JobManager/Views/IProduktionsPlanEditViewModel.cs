using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

namespace WiCAM.Pn4000.JobManager
{
    public interface IProduktionsPlanEditViewModel :
      IViewModel
   
    {
     
        public ObservableCollection<AuftragsListeData> ProduktionsPlanDataGrid { get; set; }
        public AuftragsListeData ProduktionsPlanDataGridSelectedItemProperty { get; set; }
     
        ICommand ProduktionsPlanReadCmd { get;  }
        ICommand FilterVerpacktCmd { get; }
        ICommand FilterBiegenCmd { get; }
    }
}
