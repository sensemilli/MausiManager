using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public class StopCombinationViewModel
{
    [CompilerGenerated]
    private IFingerStopCombinationData _003C_data_003EP;

    public int Id { get; }

    public string? PpId { get; set; }

    public string ActualIconPath
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(IconPath))
            {
                return IconPath;
            }
            return DefaultIconPath;
        }
    }

    public string IconPath { get; set; }

    public string DefaultIconPath { get; }

    public int FaceCount => FaceNames.Count;

    public List<string> FaceNames { get; }

    public string Faces => string.Join(Environment.NewLine, FaceNames);

    public StopCombinationViewModel(int id, IFingerStopCombinationData _data)
    {
        _003C_data_003EP = _data;
        Id = id;
        PpId = _003C_data_003EP.PpId;
        IconPath = _003C_data_003EP.CustomIconPath ?? string.Empty;
        DefaultIconPath = _003C_data_003EP.DefaultIconPath;
        FaceNames = _003C_data_003EP.FaceNames;
        //base._002Ector();
    }

    public void Save()
    {
        _003C_data_003EP.PpId = PpId;
        if (!string.IsNullOrEmpty(IconPath))
        {
            _003C_data_003EP.CustomIconPath = IconPath;
        }
    }
}
