using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services;

public partial class PnDebug : Window, IPnDebug, IComponentConnector
{
	private readonly ObservableCollection<object> _collection = new ObservableCollection<object>();

	private readonly IConfigProvider _configProvider;

	private readonly ILogCenterService _logCenterService;

	private int _globalCount;

	private ScrollViewer _sv;

	private DispatcherTimer _dispatcherTimer;

	private string _mCfgFilename = "debug_cfg.cfg";

	private bool _v1;

	private bool _v2;

	private bool _v3;

	private bool _v4;

	private Stopwatch stopWatch = new Stopwatch();

	private long _initialParam1;

	private long _initialParam2;

	private int _clStyle = 1;

	public bool WasUsing { get; set; }

	public PnDebug(IConfigProvider configProvider, ILogCenterService logCenterService)
	{
		this._configProvider = configProvider;
		this._logCenterService = logCenterService;
		this.InitializeComponent();
	}

	public void DebugThat(string str)
	{
		if (!this._configProvider.InjectOrCreate<GeneralUserSettingsConfig>().ActiveDebugWindow)
		{
			return;
		}
		base.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			this.AddToList(str);
			if (str.Contains("[STOPWATCH.START]"))
			{
				this.AddToList("STOPWATCH.START");
				this.stopWatch.Start();
			}
			if (str.Contains("[STOPWATCH.STOP]"))
			{
				this.stopWatch.Stop();
				this.AddToList($"STOPWATCH.STOP: {this.stopWatch.ElapsedMilliseconds} ms");
				this.stopWatch.Reset();
			}
			this.Flush();
		});
	}

	private void AddToList(string s)
	{
		if (this._collection.Count > 9999)
		{
			this._collection.RemoveAt(0);
		}
		this._collection.Add(new TextBlock
		{
			Text = s
		});
		this._globalCount++;
		this.UpdateCountLabel();
	}

	private void UpdateCountLabel()
	{
		this.count_label.Content = $"{this._collection.Count}/{this._globalCount}";
	}

	private void FindSv()
	{
		int childrenCount = VisualTreeHelper.GetChildrenCount(this.list1);
		for (int i = 0; i < childrenCount; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(this.list1, i);
			VisualTreeHelper.GetChildrenCount(child);
			for (int j = 0; j < childrenCount; j++)
			{
				DependencyObject child2 = VisualTreeHelper.GetChild(child, j);
				if (child2 is ScrollViewer)
				{
					this._sv = (ScrollViewer)child2;
					return;
				}
			}
		}
	}

	public void ScrollToEnd()
	{
		if (this._sv == null)
		{
			this.FindSv();
		}
		if (this._sv != null)
		{
			this._sv.ScrollToEnd();
		}
	}

	public void Flush()
	{
		this.ScrollToEnd();
	}

	public void AddString(string str)
	{
		this.AddToList(str);
		this.ScrollToEnd();
		this.WasUsing = true;
	}

	public void Clear()
	{
		base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ClearInternal));
	}

	public void ShowPlus()
	{
		base.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			base.Show();
			this.ScrollToEnd();
			this.WasUsing = true;
		});
	}

	public void HidePlus()
	{
		base.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(base.Hide));
	}

	public void ClearInternal()
	{
		this._collection.Clear();
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		this.SaveConfig();
		base.Hide();
		e.Cancel = true;
	}

	public void SaveConfig()
	{
		base.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			if (!this.WasUsing)
			{
				return;
			}
			using StreamWriter streamWriter = new StreamWriter(this._mCfgFilename);
			streamWriter.WriteLine(((int)base.Left).ToString());
			streamWriter.WriteLine(((int)base.Top).ToString());
			streamWriter.WriteLine(((int)base.Width + (int)base.Left).ToString());
			streamWriter.WriteLine(((int)base.Height + (int)base.Top).ToString());
			streamWriter.WriteLine(this._clStyle.ToString());
		});
	}

	private void button1_Click(object sender, RoutedEventArgs e)
	{
		this.ClearInternal();
		this._globalCount = 0;
		this.UpdateCountLabel();
	}

	private void button2_Click(object sender, RoutedEventArgs e)
	{
		base.Hide();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		try
		{
			if (File.Exists(this._mCfgFilename))
			{
				string[] array = File.ReadAllLines(this._mCfgFilename);
				if (array.Length < 4)
				{
					return;
				}
				base.Left = Convert.ToInt32(array[0]);
				base.Top = Convert.ToInt32(array[1]);
				base.Width = (double)Convert.ToInt32(array[2]) - base.Left;
				base.Height = (double)Convert.ToInt32(array[3]) - base.Top;
				this.CorectionForDebugWindowLocation();
				if (array.Length >= 5)
				{
					this._clStyle = Convert.ToInt32(array[4]);
				}
			}
		}
		catch (Exception e2)
		{
			this._logCenterService.CatchRaport(e2);
		}
		this.list1.ItemsSource = this._collection;
		this.ColorUpdate();
		this.UpdateCountLabel();
	}

	private void CorectionForDebugWindowLocation()
	{
		if (base.Top + base.Height / 2.0 > SystemParameters.VirtualScreenHeight)
		{
			base.Top = SystemParameters.VirtualScreenHeight - base.Height;
		}
		if (base.Left + base.Width / 2.0 > SystemParameters.VirtualScreenWidth)
		{
			base.Left = SystemParameters.VirtualScreenWidth - base.Width;
		}
		if (base.Left < SystemParameters.VirtualScreenLeft)
		{
			base.Left = SystemParameters.VirtualScreenLeft;
		}
		if (base.Top < SystemParameters.VirtualScreenTop)
		{
			base.Top = SystemParameters.VirtualScreenTop;
		}
	}

	private void button3_Click(object sender, RoutedEventArgs e)
	{
		this.Copy();
	}

	private void Copy()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (this.list1.SelectedItems.Count > 0)
		{
			foreach (TextBlock selectedItem in this.list1.SelectedItems)
			{
				stringBuilder.AppendLine(selectedItem.Text);
			}
		}
		else
		{
			foreach (TextBlock item in (IEnumerable)this.list1.Items)
			{
				stringBuilder.AppendLine(item.Text);
			}
		}
		Clipboard.Clear();
		Clipboard.SetText(stringBuilder.ToString());
	}

	private void Window_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
		{
			this.Copy();
		}
	}

	private void button4_Click(object sender, RoutedEventArgs e)
	{
		this.AddToList("----------------------------------");
		this.AddToList(DateTime.Now.ToString());
		this.AddToList(AppDomain.CurrentDomain.FriendlyName);
		this.AddToList(AppDomain.CurrentDomain.BaseDirectory);
		this.AddToList($"PRV MEM SIZE {Process.GetCurrentProcess().PrivateMemorySize64 / 1024} KB");
		this.AddToList("----------------------------------");
		this.Flush();
	}

	private void CheckBox_Click(object sender, RoutedEventArgs e)
	{
		if (this.monitoring_active_checkbox.IsChecked.Value)
		{
			this.MonitoringColumn.Width = new GridLength(200.0);
			this.ResetPlusMinus();
			this._dispatcherTimer = new DispatcherTimer();
			this._dispatcherTimer.Tick += dispatcherTimer_Tick;
			this._dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
			this._dispatcherTimer.Start();
		}
		else
		{
			this.MonitoringColumn.Width = new GridLength(0.0);
			this._dispatcherTimer.Stop();
			this._dispatcherTimer = null;
		}
	}

	private void ResetPlusMinus()
	{
		this._initialParam1 = Process.GetCurrentProcess().PrivateMemorySize64;
		this._initialParam2 = GC.GetTotalMemory(forceFullCollection: true);
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		long privateMemorySize = Process.GetCurrentProcess().PrivateMemorySize64;
		long num = privateMemorySize - this._initialParam1;
		this.totmem_label.Content = $"{(double)privateMemorySize / 1048576.0:0.0}MB ({(double)num / 1048576.0:0.0}MB)";
		privateMemorySize = GC.GetTotalMemory(forceFullCollection: true);
		num = privateMemorySize - this._initialParam2;
		this.menmem_label.Content = $"{(double)privateMemorySize / 1048576.0:0.0}MB ({(double)num / 1048576.0:0.0}MB)";
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		this.ResetPlusMinus();
	}

	private void clbutton_Click(object sender, RoutedEventArgs e)
	{
		this._clStyle++;
		if (this._clStyle > 3)
		{
			this._clStyle = 1;
		}
		this.ColorUpdate();
		this.DebugThat("Debug window colors change.");
	}

	private void ColorUpdate()
	{
		switch (this._clStyle)
		{
		case 1:
			this.list1.Background = new SolidColorBrush(Colors.White);
			this.list1.Foreground = new SolidColorBrush(Colors.Black);
			break;
		case 2:
			this.list1.Background = new SolidColorBrush(Colors.Black);
			this.list1.Foreground = new SolidColorBrush(Colors.Green);
			break;
		case 3:
			this.list1.Background = new SolidColorBrush(Colors.Black);
			this.list1.Foreground = new SolidColorBrush(Colors.White);
			break;
		}
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
	}

	private void buttont1_Click(object sender, RoutedEventArgs e)
	{
	}

	private void buttont2_Click(object sender, RoutedEventArgs e)
	{
	}

	private void buttont3_Click(object sender, RoutedEventArgs e)
	{
	}

	private void Button_Click_2(object sender, RoutedEventArgs e)
	{
	}

	public void DebugThatOptionally(int v, string str)
	{
		switch (v)
		{
		case 1:
			if (this._v1)
			{
				this.DebugThat(str);
			}
			break;
		case 2:
			if (this._v2)
			{
				this.DebugThat(str);
			}
			break;
		case 3:
			if (this._v3)
			{
				this.DebugThat(str);
			}
			break;
		case 4:
			if (this._v4)
			{
				this.DebugThat(str);
			}
			break;
		}
	}

	private void optional_debug1_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			this._v1 = !this._v1;
		});
	}

	private void optional_debug2_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			this._v2 = !this._v2;
		});
	}

	private void optional_debug3_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			this._v3 = !this._v3;
		});
	}

	private void optional_debug4_Click(object sender, RoutedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate
		{
			this._v4 = !this._v4;
		});
	}

    bool IPnDebug.IsVisible => base.IsVisible;
}
