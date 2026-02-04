using AnyCAD.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WiCAM.Pn4000.ScreenD3D.Controls.Base;

namespace WiCAM.Pn4000.JobManager
{
    public interface IPartEditViewModel :
      IViewModel
   
    {
     

     
        ICommand SavePartEdit { get; }
        ICommand PartEditCmd1 { get; }
        ICommand FPR997toggleCommand { get; }

        ICommand FPR998toggleCommand { get; }

        ICommand FPR999toggleCommand { get; }

       

        public ICommand ShowLeftFlyoutCommandMain { get; }
        ICommand MessenCommand { get; }
        public string MousePosition { get; }
        ICommand ClickLeft { get; }
        ICommand ClickRight { get; }
        ICommand ClickTop { get; }
        ICommand ClickBottom { get; }
        ICommand ClickFront { get; }

        ICommand ClickBack { get; }
        ICommand ShowAll { get; }
        ICommand ShowWires { get; }
        ICommand DXFtxtAnpassenCommand { get; }
         ObservableCollection<UserStepFileTree> UserStepFileTree { get; set; }

    }
}
