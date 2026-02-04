using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WiCAM.Pn4000.PnControl;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;

public class UpperAdaptersViewModel : ProfilesAndPartsViewModel<UpperAdapterViewModel, LowerToolPieceViewModel>
{
    [CompilerGenerated]
    private IUnitConverter _003C_unitConverter_003EP;

    [CompilerGenerated]
    private IGlobalToolStorage _003C_toolStorage_003EP;

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

    private ICommand? _bystronicXMLAdapterCommand;

    private ICommand? _bystronicXMLHolderCommand;

    private ICommand? _delemXMLCommand;

    private ICommand? _lvdXMLCommand;

    private ICommand? _radanMDBCommand;

    private ICommand? _radanXMLCommand;

    private ICommand? _trumpfARVAdapterCommand;

    private ICommand? _trumpfARVHolderCommand;

    private ICommand? _trumpfMDBAdapterCommand;

    private ICommand? _trumpfMDBHolderCommand;

    private ICommand? _stepAutomationAdapterCommand;

    private ICommand? _stepAutomationHolderCommand;

    private ICommand? _dxfCommand;

    private ICommand? _safanDarleyCommand;

    public ICommand BystronicXMLAdapterCommand => _bystronicXMLAdapterCommand ?? (_bystronicXMLAdapterCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_bystronicXMLImporter_003EP, _003C_bystronicXMLImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand BystronicXMLHolderCommand => _bystronicXMLHolderCommand ?? (_bystronicXMLHolderCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_bystronicXMLImporter_003EP, _003C_bystronicXMLImporter_003EP.ImportUpperHolders);
    }));

    public ICommand DelemXMLCommand => _delemXMLCommand ?? (_delemXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_delemXMLImporter_003EP, _003C_delemXMLImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand LVDXMLCommand => _lvdXMLCommand ?? (_lvdXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_lvdXMLImporter_003EP, _003C_lvdXMLImporter_003EP.ImportUpperHolders);
    }));

    public ICommand RadanMDBCommand => _radanMDBCommand ?? (_radanMDBCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanMDBImporter_003EP, _003C_radanMDBImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand RadanXMLCommand => _radanXMLCommand ?? (_radanXMLCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_radanXMLImporter_003EP, _003C_radanXMLImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand TrumpfARVAdapterCommand => _trumpfARVAdapterCommand ?? (_trumpfARVAdapterCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfARVImporter_003EP, _003C_trumpfARVImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand TrumpfARVHolderCommand => _trumpfARVHolderCommand ?? (_trumpfARVHolderCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfARVImporter_003EP, _003C_trumpfARVImporter_003EP.ImportUpperHolders);
    }));

    public ICommand TrumpfMDBAdapterCommand => _trumpfMDBAdapterCommand ?? (_trumpfMDBAdapterCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfMDBImporter_003EP, _003C_trumpfMDBImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand TrumpfMDBHolderCommand => _trumpfMDBHolderCommand ?? (_trumpfMDBHolderCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_trumpfMDBImporter_003EP, _003C_trumpfMDBImporter_003EP.ImportUpperHolders);
    }));

    public ICommand StepAutomationAdapterCommand => _stepAutomationAdapterCommand ?? (_stepAutomationAdapterCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_stepAutomationImporter_003EP, _003C_stepAutomationImporter_003EP.ImportUpperAdapters);
    }));

    public ICommand StepAutomationHolderCommand => _stepAutomationHolderCommand ?? (_stepAutomationHolderCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_stepAutomationImporter_003EP, _003C_stepAutomationImporter_003EP.ImportUpperHolders);
    }));

    public ICommand DXFCommand => _dxfCommand ?? (_dxfCommand = new RelayCommand((Action<object>)delegate
    {
        ImportDXFProfile(base.SelectedProfile.MultiTool);
    }));

    public ICommand SafanDarleyCommand => _safanDarleyCommand ?? (_safanDarleyCommand = new RelayCommand((Action<object>)delegate
    {
        ImportTools(_003C_safanDarleyImporter_003EP, _003C_safanDarleyImporter_003EP.ImportPunches);
    }));

    public override ObservableCollection<UpperAdapterViewModel> Profiles => base.ToolConfigModel.UpperAdapters;

    public override ObservableCollection<LowerToolPieceViewModel> Pieces => base.ToolConfigModel.LowerPieces;

    public List<ComboboxEntry<AdapterDirections>> AllAdapterDirections => base.ToolConfigModel.AllAdapterDirections;

    public List<ComboboxEntry<AdapterXPositions>> AllAdapterXPositions => base.ToolConfigModel.AllAdapterXPositions;

    public UpperAdaptersViewModel(IConfigProvider _configProvider, Contracts.Common.ITranslator _translator, IUnitConverter _unitConverter, 
        IDrawToolProfiles _drawToolProfiles, IGlobalToolStorage _toolStorage, IModelFactory _modelFactory,
        BystronicXMLImporter _bystronicXMLImporter, DelemXMLImporter _delemXMLImporter, LvdXMLImporter _lvdXMLImporter, 
        RadanMDBImporter _radanMDBImporter, RadanXMLImporter _radanXMLImporter, TrumpfARVImporter _trumpfARVImporter, 
        TrumpfMDBImporter _trumpfMDBImporter, SafanDarleyImporter _safanDarleyImporter, StepAutomationImporter _stepAutomationImporter) : base(_configProvider, _translator, _drawToolProfiles, _toolStorage, _modelFactory)
    {
        _003C_unitConverter_003EP = _unitConverter;
        _003C_toolStorage_003EP = _toolStorage;
        _003C_bystronicXMLImporter_003EP = _bystronicXMLImporter;
        _003C_delemXMLImporter_003EP = _delemXMLImporter;
        _003C_lvdXMLImporter_003EP = _lvdXMLImporter;
        _003C_radanMDBImporter_003EP = _radanMDBImporter;
        _003C_radanXMLImporter_003EP = _radanXMLImporter;
        _003C_trumpfARVImporter_003EP = _trumpfARVImporter;
        _003C_trumpfMDBImporter_003EP = _trumpfMDBImporter;
        _003C_safanDarleyImporter_003EP = _safanDarleyImporter;
        _003C_stepAutomationImporter_003EP = _stepAutomationImporter;
       
    }

    public override void DeleteProfile(UpperAdapterViewModel profile)
    {
        base.ToolConfigModel.DeleteUpperAdapter(profile);
    }

    public override void DeletePiece(LowerToolPieceViewModel piece)
    {
        base.ToolConfigModel.DeleteLowerPiece(piece);
    }

    public override void AddProfile()
    {
        UpperAdapterViewModel profile = new UpperAdapterViewModel(_003C_unitConverter_003EP, _003C_toolStorage_003EP, base.ToolConfigModel.CreateMultiTool(""));
        AddProfile(profile);
    }

    public override void AddPiece()
    {
        if (base.SelectedProfile != null)
        {
            LowerToolPieceViewModel piece = base.ToolConfigModel.CreateLowerToolPiece(base.ToolConfigModel.SelectedToolList, base.SelectedProfile.MultiTool, "");
            AddPiece(piece);
        }
    }
}
