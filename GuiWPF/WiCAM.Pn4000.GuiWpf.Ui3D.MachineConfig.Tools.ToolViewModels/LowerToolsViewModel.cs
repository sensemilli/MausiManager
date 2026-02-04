using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class LowerToolsViewModel : ProfilesAndPartsViewModel<LowerToolViewModel, LowerToolPieceViewModel>
{
    [CompilerGenerated]
    private ITranslator _003C_translator_003EP;

    [CompilerGenerated]
    private IUnitConverter _003C_unitConverter_003EP;

    [CompilerGenerated]
    private IGlobalToolStorage _003C_toolStorage_003EP;

    [CompilerGenerated]
    private IMessageLogGlobal _003C_messageLog_003EP;

    [CompilerGenerated]
    private BystronicXMLImporter _003C_bystronicXMLImporter_003EP;

    [CompilerGenerated]
    private DelemXMLImporter _003C_delemXMLImporter_003EP;

    [CompilerGenerated]
    private LvdXMLImporter _003C_lvdXMLImporter_003EP;

    [CompilerGenerated]
    private RadanMDBImporter _003C_radanMDBImporter_003EP;

    [CompilerGenerated]
    private RadanXMLImporter _003C_radanXMLImporter_003EP;

    [CompilerGenerated]
    private TrumpfARVImporter _003C_trumpfARVImporter_003EP;

    [CompilerGenerated]
    private TrumpfMDBImporter _003C_trumpfMDBImporter_003EP;

    [CompilerGenerated]
    private TrumpfBtdImporter _003C_trumpfBtdImporter_003EP;

    [CompilerGenerated]
    private SafanDarleyImporter _003C_safanDarleyImporter_003EP;

    [CompilerGenerated]
    private StepAutomationImporter _003C_stepAutomationImporter_003EP;

    [CompilerGenerated]
    private IDieHelperDeprecated _003C_dieHelper_003EP;

    private ICommand? _checkDieProfile;

    private ICommand? _bystronicXMLCommand;

    private ICommand? _delemXMLCommand;

    private ICommand? _lvdXMLCommand;

    private ICommand? _radanMDBCommand;

    private ICommand? _radanXMLCommand;

    private ICommand? _trumpfARVDiesCommand;

    private ICommand? _trumpfARVHemsCommand;

    private ICommand? _trumpfMDBCommand;

    private ICommand? _trumpfBtdCommand;

    private ICommand? _safanDarleyCommand;

    private ICommand? _dxfCommand;

    private ICommand? _stepAutomationXmaCommand;

    public ICommand CheckDieProfileCommand => _checkDieProfile ?? (_checkDieProfile = new RelayCommand(CheckDieGeometry));

    public ICommand BystronicXMLCommand => _bystronicXMLCommand ?? (_bystronicXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_bystronicXMLImporter_003EP, _003C_bystronicXMLImporter_003EP.ImportDies);
    }));

    public ICommand DelemXMLCommand => _delemXMLCommand ?? (_delemXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_delemXMLImporter_003EP, _003C_delemXMLImporter_003EP.ImportDies);
    }));

    public ICommand LvdXMLCommand => _lvdXMLCommand ?? (_lvdXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_lvdXMLImporter_003EP, _003C_lvdXMLImporter_003EP.ImportDies);
    }));

    public ICommand RadanMDBCommand => _radanMDBCommand ?? (_radanMDBCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanMDBImporter_003EP, _003C_radanMDBImporter_003EP.ImportDies);
    }));

    public ICommand RadanXMLCommand => _radanXMLCommand ?? (_radanXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanXMLImporter_003EP, _003C_radanXMLImporter_003EP.ImportDies);
    }));

    public ICommand TrumpfARVDiesCommand => _trumpfARVDiesCommand ?? (_trumpfARVDiesCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfARVImporter_003EP, _003C_trumpfARVImporter_003EP.ImportDies);
    }));

    public ICommand TrumpfARVHemsCommand => _trumpfARVHemsCommand ?? (_trumpfARVHemsCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfARVImporter_003EP, _003C_trumpfARVImporter_003EP.ImportHems);
    }));

    public ICommand TrumpfMDBCommand => _trumpfMDBCommand ?? (_trumpfMDBCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfMDBImporter_003EP, _003C_trumpfMDBImporter_003EP.ImportDies);
    }));

    public ICommand TrumpfBTDCommand => _trumpfBtdCommand ?? (_trumpfBtdCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfBtdImporter_003EP, _003C_trumpfBtdImporter_003EP.ImportDies);
    }));

    public ICommand StepAutomationXmaCommand => _stepAutomationXmaCommand ?? (_stepAutomationXmaCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_stepAutomationImporter_003EP, _003C_stepAutomationImporter_003EP.ImportDies);
    }));

    public ICommand SafanDarleyCommand => _safanDarleyCommand ?? (_safanDarleyCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_safanDarleyImporter_003EP, _003C_safanDarleyImporter_003EP.ImportDies);
    }));

    public ICommand DXFCommand => _dxfCommand ?? (_dxfCommand = new RelayCommand((Action<object>)delegate
    {
        ImportDXFProfile(base.SelectedProfile.MultiTool);
    }));

    public List<ComboboxEntry<VWidthTypes>> VWidthTypes { get; }

    public override ObservableCollection<LowerToolViewModel> Profiles => base.ToolConfigModel.LowerTools;

    public override ObservableCollection<LowerToolPieceViewModel> Pieces => base.ToolConfigModel.LowerPieces;

    public LowerToolsViewModel(IConfigProvider _configProvider, ITranslator _translator, IUnitConverter _unitConverter, 
        IDrawToolProfiles _drawToolProfiles, IGlobalToolStorage _toolStorage, IModelFactory _modelFactory,
        IMessageLogGlobal _messageLog, BystronicXMLImporter _bystronicXMLImporter, DelemXMLImporter _delemXMLImporter, 
        LvdXMLImporter _lvdXMLImporter, RadanMDBImporter _radanMDBImporter, RadanXMLImporter _radanXMLImporter, 
        TrumpfARVImporter _trumpfARVImporter, TrumpfMDBImporter _trumpfMDBImporter, TrumpfBtdImporter _trumpfBtdImporter,
        SafanDarleyImporter _safanDarleyImporter, StepAutomationImporter _stepAutomationImporter, IDieHelperDeprecated _dieHelper) : base(_configProvider, _translator, _drawToolProfiles, _toolStorage, _modelFactory)
    {
        _003C_translator_003EP = _translator;
        _003C_unitConverter_003EP = _unitConverter;
        _003C_toolStorage_003EP = _toolStorage;
        _003C_messageLog_003EP = _messageLog;
        _003C_bystronicXMLImporter_003EP = _bystronicXMLImporter;
        _003C_delemXMLImporter_003EP = _delemXMLImporter;
        _003C_lvdXMLImporter_003EP = _lvdXMLImporter;
        _003C_radanMDBImporter_003EP = _radanMDBImporter;
        _003C_radanXMLImporter_003EP = _radanXMLImporter;
        _003C_trumpfARVImporter_003EP = _trumpfARVImporter;
        _003C_trumpfMDBImporter_003EP = _trumpfMDBImporter;
        _003C_trumpfBtdImporter_003EP = _trumpfBtdImporter;
        _003C_safanDarleyImporter_003EP = _safanDarleyImporter;
        _003C_stepAutomationImporter_003EP = _stepAutomationImporter;
        _003C_dieHelper_003EP = _dieHelper;
        VWidthTypes = new List<ComboboxEntry<VWidthTypes>>();
        
    }

    public override void DeleteProfile(LowerToolViewModel profile)
    {
        base.ToolConfigModel.DeleteLowerTool(profile);
    }

    public override void DeletePiece(LowerToolPieceViewModel piece)
    {
        base.ToolConfigModel.DeleteLowerPiece(piece);
    }

    public new LowerToolsViewModel Init(MachineToolsViewModel toolModel)
    {
        base.Init(toolModel);
        foreach (VWidthTypes value in System.Enum.GetValues(typeof(VWidthTypes)))
        {
            if (value != 0)
            {
                VWidthTypes.Add(new ComboboxEntry<VWidthTypes>(_003C_translator_003EP.Translate($"l_enum.VWidthTypes.{value}"), value));
            }
        }
        return this;
    }

    public override void AddProfile()
    {
        LowerToolGroupViewModel lowerToolGroupViewModel = base.SelectedProfile?.Group ?? base.ToolConfigModel.LowerGroups.FirstOrDefault();
        if (lowerToolGroupViewModel != null)
        {
            LowerToolViewModel profile = new LowerToolViewModel(WiCAM.Pn4000.Contracts.Tools.VWidthTypes.Undefined, _003C_unitConverter_003EP, _003C_toolStorage_003EP, base.ToolConfigModel.CreateMultiTool(""), lowerToolGroupViewModel);
            AddProfile(profile);
        }
        else
        {
            base.ToolConfigModel.MessageLogGlobal.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.AddToolProfileErrorNoGroup");
        }
    }

    public override void AddPiece()
    {
        if (base.SelectedProfile != null)
        {
            LowerToolPieceViewModel piece = base.ToolConfigModel.CreateLowerToolPiece(base.ToolConfigModel.SelectedToolList, base.SelectedProfile.MultiTool, "");
            AddPiece(piece);
        }
    }

    private void CheckDieGeometry()
    {
        LowerToolViewModel selectedProfile = base.SelectedProfile;
        if (selectedProfile != null)
        {
            string text = _003C_dieHelper_003EP.CheckDieGeometry(selectedProfile.MultiTool.GeometryFileFull, selectedProfile.OffsetInX, selectedProfile.VAngle.Value, selectedProfile.VWidth.Value, 0.0, selectedProfile.CornerRadius.Value, selectedProfile.VWidthType.Value);
            bool num = text != "";
            text = "Checking \"" + base.SelectedProfile.Name + "\" (Non standard die profiles might not be checked correctly):\n" + text;
            text = text.Replace("\n", "\n\n");
            if (num)
            {
                _003C_messageLog_003EP.ShowWarningMessage(text);
            }
            else
            {
                _003C_messageLog_003EP.ShowInformationMessage(text + "All correct");
            }
        }
    }
}
