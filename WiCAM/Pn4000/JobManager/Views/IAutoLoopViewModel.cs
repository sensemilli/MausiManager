using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

namespace WiCAM.Pn4000.JobManager
{
    public interface IAutoLoopViewModel :
      IViewModel
   
    {
     

     
        ICommand SaveFPRedit { get; }
        ICommand FPR996toggleCommand { get; }
        ICommand FPR997toggleCommand { get; }

        ICommand FPR998toggleCommand { get; }

        ICommand FPR999toggleCommand { get; }

        public ICommand LageCommand { get; }
        public ICommand XrichtenCommand { get; }
        public ICommand YrichtenCommand { get; }
        public ICommand TrimmenCommand { get; }
        public ICommand GleicheDCommand { get; }
        public ICommand CADdeleteCommand { get; }
        public ICommand GravurCommand { get; }
        public ICommand StanzenInnenCommand { get; }
        public ICommand KonturAussenCommand { get; }
        public ICommand FlaecheAussenCommand { get; }
        public ICommand StanzenAutoCommand { get; }
        public ICommand FlaecheStanzenCommand { get; }
        public ICommand WKZaendernCommand { get; }
        public ICommand SortierenCommand { get; }
        public ICommand NCdeleteCommand { get; }

        public ICommand ShowLeftFlyoutCommandMain { get; }

    }
}
