using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;

namespace WiCAM.Pn4000.JobManager
{
    public interface ICommonCutViewModel :
      IViewModel
   
    {
        public ICommand MultiShearCommand { get; }
        public ICommand StandartCommand { get; }
        public ICommand SechsundfuenzigCommand { get; }
        public ICommand SechsundvierzigCommand { get; }
        public ICommand DreissigCommand { get; }
        public ICommand SortXCommand { get; }

        public ICommand SelectVerticalTrimTool762x5Command { get; }
        public ICommand SelectVerticalTrimTool76x5Command { get; }
        public ICommand SelectVerticalTrimTool56x5Command { get; }
        public ICommand SelectVerticalTrimTool46x5Command { get; }
        public ICommand SelectVerticalTrimTool30x5Command { get; }
        public ICommand SelectVerticalTrimTool15x5Command { get; }
        public ICommand SelectTrimTool762x5Command { get; }
        public ICommand SelectTrimTool76x5Command { get; }
        public ICommand SelectTrimTool56x5Command { get; }
        public ICommand SelectTrimTool46x5Command { get; }
        public ICommand SelectTrimTool30x5Command { get; }
        public ICommand SechundsiebzigzusechsundsiebzigzweiCommand { get; }
        public ICommand WLASTCommand { get; }

        public ICommand ShowLeftFlyoutCommandMain { get; }

    }
}
