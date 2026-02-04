using System;
using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.BendDoc.Contracts;
using WiCAM.Pn4000.BendDoc.Services;
using WiCAM.Pn4000.BendDoc.UndoRedo;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Extensions;
using WiCAM.Pn4000.Contracts.FingerCalculation;
using WiCAM.Pn4000.FingerStopCalculation;
using WiCAM.Pn4000.FingerStopCalculationMediator;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.BendDoc;

public static class Extensions
{
	public static void AddPnBndDoc(this IServiceCollection builder)
	{
		builder.AddSingletonIntercepted<IPnBndDocImporter, PnBndDocImporter>();
		builder.AddSingletonIntercepted<IDocSerializer, DocSerializer>();
		builder.AddTransientIntercepted<IDocConverter, DocConverter>();
		builder.AddTransientIntercepted<IPnBndDocCopyOptions, PnBndDocCopyOptions>();
		builder.AddSingleton<ICurrentDocProvider, CurrentDocProvider>();
		builder.AddSingleton((IServiceProvider x) => (ICurrentBusyState)x.GetService<ICurrentDocProvider>());
		builder.AddSingleton<IDocManagerInternal, DocManager>();
		builder.AddSingleton((Func<IServiceProvider, IDocManager>)((IServiceProvider x) => x.GetRequiredService<IDocManagerInternal>()));
		builder.AddSingleton<IInternalDocFactory, Doc3dFactory>();
		builder.AddSingleton((Func<IServiceProvider, IDoc3dFactory>)((IServiceProvider x) => x.GetRequiredService<IInternalDocFactory>()));
		builder.AddSingleton<ISpatialImport, SpatialImport>();
		builder.AddScoped<IFingerStopCalculationMediator, global::WiCAM.Pn4000.FingerStopCalculationMediator.FingerStopCalculationMediator>();
		builder.AddScoped<IFingerStopModifier, FingerStopModifier>();
		builder.AddScoped<Doc3d>();
		builder.AddScoped((Func<IServiceProvider, IDoc3d>)((IServiceProvider x) => x.GetRequiredService<Doc3d>()));
		builder.AddScoped((Func<IServiceProvider, IPnBndDoc>)((IServiceProvider x) => x.GetRequiredService<IDoc3d>()));
		builder.AddScoped<IUndo3dDocService, Undo3dDocService>();
		builder.AddSingleton<IUndo3dService, Undo3dService>();
	}

}
