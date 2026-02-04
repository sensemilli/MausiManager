using System;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.Contracts.Assembly;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Extensions;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Assembly;
using WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers;
using WiCAM.Pn4000.PN3D.Assembly.PurchasedParts;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.BendSimulation.PP;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Doc.Serializer;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Unfold;
using PartAnalyzer = WiCAM.Pn4000.PN3D.Assembly.DisassemblyWindowHelpers.PartAnalyzer;

namespace WiCAM.Pn4000.PN3D;

public static class DependencyInjection
{
	public static void AddPn3d(this IServiceCollection builder)
	{
		builder.AddSingletonRegisterBoth<IPnCommandRegister, PN3DRootPipe>();
		builder.AddSingletonIntercepted<IPN3DBendPipe, PN3DBendPipe>();
		builder.AddSingletonIntercepted<IPN3DDocPipe, PN3DDocPipe>();
		builder.AddSingletonIntercepted<IPrefabricatedPartsManager, PrefabricatedPartsManager>();
		builder.AddSingleton<IImportMaterialMapper, ImportMaterialMapper>();
		builder.AddSingleton<IAssemblyFactory, AssemblyFactory>();
		builder.AddSingletonIntercepted<IPartAnalyzer, PartAnalyzer>();
		builder.AddSingleton<IUnitConverters, UnitConverters>();
		builder.AddSingleton<IUnitCurrentLanguage, UnitCurrentLanguage>();
		builder.AddSingleton<IBendMachineSimulation, BendMachineSimulation>();
		builder.AddSingleton<IPnBndDocToDocConverter, PnBndDocToDocConverter>();
		builder.AddSingleton<IAnalyzeConfigProvider, ConvertConfig>();
		builder.AddSingleton<IToolsAndBendsSerializer, ToolsAndBendsSerializer>();
		builder.AddSingletonIntercepted<IPpManager, PPManager>();
		builder.AddTransient<IMachineUnfoldSettingsViewModel, MachineUnfoldSettingsViewModel>();
		builder.AddTransient<PopupUnfoldSettingViewModel>();
		builder.AddTransient<PopupMaterialAllianceView>();
		builder.AddTransient<IDocMetadata, DocMetadata>();
	}

	public static void UsePn3d(this IServiceProvider factorio)
	{
		factorio.GetService<IPpManager>().ClearBendTempDir();
	}

	private static void AddSingletonRegisterBoth<I, T>(this IServiceCollection _services) where I : class where T : class, I
	{
		_services.AddSingleton<T>();
		_services.AddSingleton<I, T>((IServiceProvider x) => x.GetService<T>());
	}

	public static IDoc3d Doc3d(this IPnBndDoc doc)
	{
		return doc as IDoc3d;
	}
}
