using System;
using System.IO;
using System.Text.Json;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class ToolSetupMemory : IToolSetupMemory
{
	private readonly IDocManager _docManager;

	private readonly IToolsAndBendsSerializer _toolsAndBendsSerializer;

	private readonly IPopupService _popupService;

	private readonly IFactorio _factorio;

	public ToolSetupMemory(ICurrentDocProvider currentDocProvider, IDocManager docManager, IToolsAndBendsSerializer toolsAndBendsSerializer, IPopupService popupService, IFactorio factorio)
	{
		_docManager = docManager;
		_toolsAndBendsSerializer = toolsAndBendsSerializer;
		_popupService = popupService;
		_factorio = factorio;
		currentDocProvider.CurrentDocChanged += CurrentDocProvider_CurrentDocChanged;
	}

	private void CurrentDocProvider_CurrentDocChanged(IDoc3d docOld, IDoc3d docNew)
	{
		if (docOld != null && docOld.ToolsAndBends?.ToolSetups.Count > 0)
		{
			IEditToolsSelection editToolsSelection = _docManager.GetScope(docOld).Resolve<IEditToolsSelection>();
			IToolSetups toolSetups = editToolsSelection?.CurrentSetups;
			if (toolSetups != null)
			{
				string setup = _toolsAndBendsSerializer.Convert(_toolsAndBendsSerializer.Convert(toolSetups));
				AutoSave(editToolsSelection.CurrentMachine.MachineNo, setup, docOld.DiskFile.Header.ModelName);
			}
		}
	}

	public IToolSetupMemory.Memory? Load(int machineNumber)
	{
		ToolSetupMemoryViewModel toolSetupMemoryViewModel = _popupService.ShowDialog(delegate(ToolSetupMemoryViewModel x)
		{
			x.Init(saveMode: false, machineNumber);
		});
		if (toolSetupMemoryViewModel.Result == ToolSetupMemoryViewModel.DialogResults.Ok)
		{
			IToolSetupMemory.Memory memory = JsonSerializer.Deserialize<IToolSetupMemory.Memory>(File.ReadAllText(toolSetupMemoryViewModel.ResultFilename));
			if (memory != null)
			{
				memory.SavedName = new FileInfo(toolSetupMemoryViewModel.ResultFilename).Name;
			}
			return memory;
		}
		return null;
	}

	public void Save(int machineNumber, string setup, string docName)
	{
		ToolSetupMemoryViewModel toolSetupMemoryViewModel = _popupService.ShowDialog(delegate(ToolSetupMemoryViewModel x)
		{
			x.Init(saveMode: true, machineNumber);
		});
		if (toolSetupMemoryViewModel.Result == ToolSetupMemoryViewModel.DialogResults.Ok)
		{
			Save(machineNumber, setup, docName, toolSetupMemoryViewModel.ResultFilename);
		}
	}

	public void AutoSave(int machineNumber, string setup, string docName)
	{
		ToolSetupMemoryViewModel toolSetupMemoryViewModel = _factorio.Resolve<ToolSetupMemoryViewModel>();
		toolSetupMemoryViewModel.Init(saveMode: true, machineNumber);
		toolSetupMemoryViewModel.SelectedDescription = "AutoSave";
		Save(machineNumber, setup, docName, toolSetupMemoryViewModel.ResultFilename);
	}

	private void Save(int machineNumber, string setup, string docName, string resultFilename)
	{
		IToolSetupMemory.Memory memory = new IToolSetupMemory.Memory(setup, DateTime.Now, docName, machineNumber);
		string contents = MemToString(memory);
		Directory.CreateDirectory(new FileInfo(resultFilename).Directory.FullName);
		File.WriteAllText(resultFilename, contents);
	}

	private string MemToString(IToolSetupMemory.Memory memory)
	{
		return JsonSerializer.Serialize(memory);
	}
}
