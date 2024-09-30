using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SmartAssembly.Attributes;

namespace WiCAM.Pn4000.JobManager
{
    [DoNotPruneType]
    [DoNotObfuscateType]
    [DataContract]
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        [XmlIgnore]
        public string DisplayName;

        [Browsable(false)]
        [XmlIgnore]
        public bool IsNotifyEnabled { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected ViewModelBase()
        {
            IsNotifyEnabled = true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (IsNotifyEnabled && this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                Console.WriteLine("propchange" + propertyName);

                //if (propertyName == "PartOrderGridSelectedItemProperty")
                //if (AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty == null)
                //  Console.WriteLine("propchangeGridAmodelSelItemNull");

                if (AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty != null)
                {
                    Console.WriteLine("selected   +" + AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty.Text);
                    MainWindow.mainWindow.txtArchivNummer.Text = AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty.IntValue.ToString();
                    MainWindow.mainWindow.txtArchivName.Text = AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty.Text;
                    Console.WriteLine("selected   +" + AuftragsDataViewModel._auftragsDataViewModel.GridSelectedItemProperty.PNEnumValue);
                }
            }
        }

        [Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this).Find(propertyName, ignoreCase: false) == null)
            {
                string.Format(CultureInfo.CurrentCulture, CS.MsgErrorWrongProperty, propertyName, GetType().FullName);
            }
        }
    }
}