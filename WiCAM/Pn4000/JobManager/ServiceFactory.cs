using WiCAM.Pn4000.JobManager.Classes;

namespace WiCAM.Pn4000.JobManager;

public class ServiceFactory
{
	public static IJobManagerServiceProvider CreateDefaultProvider()
	{
		SettingsNames names = new SettingsNames();
		SettingsInfo settingsInfo = new SettingsReader(new SettingsMachineReader(), new SettingFinder(names), names).Read();
		return new JobManagerServiceProvider(new IService[2]
		{
			settingsInfo,
			new StateManager(settingsInfo)
		});
	}
}
