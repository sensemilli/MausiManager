using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WiCAM.Pn4000.Archive.CAD;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Loader.Loader;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.WpfControls.CadgeoViewer;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class UpperToolsViewModel : ProfilesAndPartsViewModel<UpperToolViewModel, UpperToolPieceViewModel>
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
    private SafanDarleyImporter _003C_safanDarleyImporter_003EP;

    [CompilerGenerated]
    private StepAutomationImporter _003C_stepAutomationImporter_003EP;

    [CompilerGenerated]
    private IPunchHelperDeprecated _003CpunchHelper_003EP;

    private ICommand? _checkPunchProfile;

    private ICommand? _bystronicXMLCommand;

    private ICommand? _delemXMLCommand;

    private ICommand? _lvdXMLCommand;

    private ICommand? _radanMDBCommand;

    private ICommand? _radanXMLCommand;

    private ICommand? _trumpfARVCommand;

    private ICommand? _trumpfMDBCommand;

    private ICommand? _stepAutomationXpuCommand;

    private ICommand? _heelWZGCommand;

    private ICommand? _discWZGCommand;

    private ICommand? _dxfCommand;

    private ICommand? _safanDarleyCommand;

    public ICommand CheckDieProfileCommand => _checkPunchProfile ?? (_checkPunchProfile = new RelayCommand(CheckPunchGeometry));

    public ICommand BystronicXMLCommand => _bystronicXMLCommand ?? (_bystronicXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_bystronicXMLImporter_003EP, _003C_bystronicXMLImporter_003EP.ImportPunches);
    }));

    public ICommand DelemXMLCommand => _delemXMLCommand ?? (_delemXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_delemXMLImporter_003EP, _003C_delemXMLImporter_003EP.ImportPunches);
    }));

    public ICommand LvdXMLCommand => _lvdXMLCommand ?? (_lvdXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_lvdXMLImporter_003EP, _003C_lvdXMLImporter_003EP.ImportPunches);
    }));

    public ICommand RadanMDBCommand => _radanMDBCommand ?? (_radanMDBCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanMDBImporter_003EP, _003C_radanMDBImporter_003EP.ImportPunches);
    }));

    public ICommand RadanXMLCommand => _radanXMLCommand ?? (_radanXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanXMLImporter_003EP, _003C_radanXMLImporter_003EP.ImportPunches);
    }));

    public ICommand TrumpfARVCommand => _trumpfARVCommand ?? (_trumpfARVCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfARVImporter_003EP, _003C_trumpfARVImporter_003EP.ImportPunches);
    }));

    public ICommand TrumpfMDBCommand => _trumpfMDBCommand ?? (_trumpfMDBCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfMDBImporter_003EP, _003C_trumpfMDBImporter_003EP.ImportPunches);
    }));

    public ICommand StepAutomationXpuCommand => _stepAutomationXpuCommand ?? (_stepAutomationXpuCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_stepAutomationImporter_003EP, _003C_stepAutomationImporter_003EP.ImportPunches);
    }));

    public ICommand HeelWZGCommand => _heelWZGCommand ?? (_heelWZGCommand = new RelayCommand((Action<object>)delegate
    {
        ImportWzgSideProfile();
    }));

    public ICommand DiscWZGCommand => _discWZGCommand ?? (_discWZGCommand = new RelayCommand((Action<object>)delegate
    {
        ImportWzgMeasureDisc();
    }));

    public ICommand DXFCommand => _dxfCommand ?? (_dxfCommand = new RelayCommand((Action<object>)delegate
    {
        ImportDXFProfile(base.SelectedProfile.MultiTool);
    }));

    public ICommand SafanDarleyCommand => _safanDarleyCommand ?? (_safanDarleyCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_safanDarleyImporter_003EP, _003C_safanDarleyImporter_003EP.ImportPunches);
    }));

    public override ObservableCollection<UpperToolViewModel> Profiles => base.ToolConfigModel.UpperTools;

    public override ObservableCollection<UpperToolPieceViewModel> Pieces => base.ToolConfigModel.UpperPieces;

    public UpperToolsViewModel(IConfigProvider _configProvider, ITranslator _translator, IUnitConverter _unitConverter, 
        IDrawToolProfiles _drawToolProfiles, IGlobalToolStorage _toolStorage, IModelFactory _modelFactory, 
        IMessageLogGlobal _messageLog, BystronicXMLImporter _bystronicXMLImporter, DelemXMLImporter _delemXMLImporter, 
        LvdXMLImporter _lvdXMLImporter, RadanMDBImporter _radanMDBImporter, RadanXMLImporter _radanXMLImporter,
        TrumpfARVImporter _trumpfARVImporter, TrumpfMDBImporter _trumpfMDBImporter, SafanDarleyImporter _safanDarleyImporter, 
        StepAutomationImporter _stepAutomationImporter, IPunchHelperDeprecated punchHelper) : base(_configProvider, _translator, _drawToolProfiles, _toolStorage, _modelFactory)
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
        _003C_safanDarleyImporter_003EP = _safanDarleyImporter;
        _003C_stepAutomationImporter_003EP = _stepAutomationImporter;
        _003CpunchHelper_003EP = punchHelper;
       
    }

    public override void DeleteProfile(UpperToolViewModel profile)
    {
        base.ToolConfigModel.DeleteUpperTool(profile);
    }

    public override void DeletePiece(UpperToolPieceViewModel piece)
    {
        base.ToolConfigModel.DeleteUpperPiece(piece);
    }

    public new UpperToolsViewModel Init(MachineToolsViewModel toolConfigModel)
    {
        base.Init(toolConfigModel);
        return this;
    }

    public void ImportWzgSideProfile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = _003C_translator_003EP.Translate("l_popup.PopupMachineConfig.l_filter.WzgHeel"),
            Title = _003C_translator_003EP.Translate("l_popup.PopupMachineConfig.l_filter.ImportDialogName"),
            Multiselect = false
        };
        if (openFileDialog.ShowDialog().Value)
        {
            ICadGeo activeGeometry = base.SelectedProfile.MultiTool.GetActiveGeometry();
            CadGeoInfo heelFront = new CadGeoHelper().ReadCadgeo(openFileDialog.FileName);
            WzgLoader.GetHeelTool(activeGeometry, heelFront, out var resultModel, advancedExtrude: true);
            base.SelectedPart.Geometry3D = resultModel;
        }
    }

    public void ImportWzgMeasureDisc()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = _003C_translator_003EP.Translate("l_popup.PopupMachineConfig.l_filter.WzgDisk"),
            Title = _003C_translator_003EP.Translate("l_popup.PopupMachineConfig.l_filter.ImportDialogName"),
            Multiselect = false
        };
        if (openFileDialog.ShowDialog().Value)
        {
            CadGeoInfo discBase = new CadGeoHelper().ReadCadgeo(openFileDialog.FileName);
            Model activeModel = base.SelectedPart.GetActiveModel();
            WzgLoader.AddMeasureDisc(activeModel, discBase);
            base.SelectedPart.Geometry3D = activeModel;
        }
    }

    public override void AddProfile()
    {
        UpperToolGroupViewModel upperToolGroupViewModel = base.SelectedProfile?.Group ?? base.ToolConfigModel.UpperGroups.FirstOrDefault();
        if (upperToolGroupViewModel != null)
        {
            UpperToolViewModel profile = new UpperToolViewModel(_003C_unitConverter_003EP, _003C_toolStorage_003EP, base.ToolConfigModel.CreateMultiTool(""), upperToolGroupViewModel, 0.0, 0.0, 0.0, 0.0);
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
            UpperToolPieceViewModel piece = base.ToolConfigModel.CreateUpperToolPiece(base.ToolConfigModel.SelectedToolList, base.SelectedProfile.MultiTool, "");
            AddPiece(piece);
        }
    }

    private void CheckPunchGeometry()
    {
        string text = _003CpunchHelper_003EP.CheckPunchGeometry(base.SelectedProfile.MultiTool.GeometryFileFull, base.SelectedProfile.Radius.GetValueOrDefault(), base.SelectedProfile.Angle.GetValueOrDefault());
        bool num = text != "";
        text = "Checking \"" + base.SelectedProfile.Name + "\" (Non standard punch profiles might not be checked correctly):\n" + text;
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
