using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Microsoft.Extensions.Options;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Configuration;
using WiCAM.Pn4000.PKernelFlow.WrapC;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;

namespace WiCAM.Pn4000.DependencyInjection.Implementations;

internal sealed class AutoMode : IAutoMode
{
    private readonly IOptions<StartupOptions> _options;

    private bool _pKernelInitialized;

    private readonly bool _popupsEnabled;

    public bool PopupsEnabled
    {
        get
        {
            if (HasGui)
            {
                if (!_pKernelInitialized)
                {
                    return _popupsEnabled;
                }

                if (GetPkernelPopupVisibleMode())
                {
                    return !GetEngineMode();
                }

                return false;
            }

            return false;
        }
    }

    public bool ImportantPopupsEnabled { get; private set; }

    public bool HasGui { get; private set; }

    public AutoMode(IOptions<StartupOptions> options)
    {
        _options = options;
        StartupOptions value = _options.Value;
        _popupsEnabled = value != null && !value.InitializeConsole && value.ShowPopupsAtInitialize;
        ImportantPopupsEnabled = !_options.Value.InitializeConsole;
        HasGui = !_options.Value.InitializeConsole;
    }

    public void PkernelInitialized()
    {
        _pKernelInitialized = true;
    }

    public void DeactivatePopups()
    {
        int luwrit = 0;
        int luread = 0;
        PPO.ppo130_(ref luwrit, ref luread);
    }

    public void ActivatePopups()
    {
        int luwrit = 0;
        int luread = 0;
        PPO.ppo129_(ref luwrit, ref luread);
    }

    public void SetEngineMode(bool active)
    {
        WINDOW.window_set_ieauto(active ? 1 : 0);
    }

    private bool GetEngineMode()
    {
        return WINDOW.window_get_ieauto() == 1;
    }

    private bool GetPkernelPopupVisibleMode()
    {
        return WINDOW.window_get_ipauto() == 0;
    }
}