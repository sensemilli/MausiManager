using Microsoft.Extensions.DependencyInjection;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4UILib.Popup;

namespace WiCAM.Pn4000;

public static class Extension
{
	public static void AddCommonUi(this IServiceCollection service)
	{
		service.AddSingleton<Popup>();
		service.AddSingleton<LikeModalMode>();
		service.AddTransient<QuickTable>();
	}
}
