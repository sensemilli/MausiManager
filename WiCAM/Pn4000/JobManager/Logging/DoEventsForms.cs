using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

internal class DoEventsForms : IDoEvents
{
    public void DoEvents(int? msSleep)
    {
        Application.DoEvents();
        if (msSleep.HasValue)
        {
            Task.Delay(msSleep.Value).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter()
                .GetResult();
        }
    }
}