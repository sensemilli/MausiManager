using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiCAM.Pn4000.Archives
{


    public interface IArchiveReader
    {
        public string ConnectionString { get; }

        string DbFilter { get; set; }

        void ReadAsyncron(int archiveNumber, Action onReadyAction);
    }
}