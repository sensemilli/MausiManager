namespace WiCAM.Pn4000.Contracts.Configuration;

public class StartupOptions
{
	public string[] Arguments { get; set; }

	public bool Minimized { get; set; }

	public bool FastStart { get; set; }

	public bool ShowHelp { get; set; }

	public bool ShowStartWindow { get; set; } = true;

	public bool RecentlyUsed { get; set; }

	public bool ShowPopupsAtInitialize { get; set; }

	public bool IsLogActive { get; set; }

	public bool InitializeConsole { get; set; }
}
