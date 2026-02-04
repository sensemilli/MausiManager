using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Zipper;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.F7;

public class F7ViewModel : ViewModelBase
{
	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	private IDoc3d _currentDoc;

	private string _desc;

	public string Desc
	{
		get
		{
			return _desc;
		}
		set
		{
			if (_desc != value)
			{
				_desc = value;
				NotifyPropertyChanged("Desc");
			}
		}
	}

	public F7Zip F7Zipper { get; private set; }

	public ICommand CmdCancel { get; }

	public ICommand CmdCreateLocal { get; }

	public ICommand CmdSendData { get; }

	public event Action OnClosing;

	private void Cancel()
	{
		this.OnClosing?.Invoke();
	}

	public F7ViewModel(IPnPathService pathService, IConfigProvider configProvider)
	{
		_pathService = pathService;
		_configProvider = configProvider;
		CmdCreateLocal = new RelayCommand(CreateLocal, HasDescription);
		CmdSendData = new RelayCommand(SendData, () => false);
		CmdCancel = new RelayCommand(Cancel);
	}

	public void Init(IDoc3d currentDoc)
	{
		_currentDoc = currentDoc;
	}

	private bool HasDescription()
	{
		return !string.IsNullOrEmpty(Desc?.Trim());
	}

	private void SendData()
	{
		CreateZip();
		F7Zipper.Upload();
		string bundleId = F7Zipper.BundleId;
		if (MessageBox.Show("Upload erfolgreich unter " + bundleId + Environment.NewLine + Environment.NewLine + "In die Zwischenablage kopieren?", "F7 Sicherung erfolgreich.", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
		{
			Clipboard.SetText(bundleId);
		}
		this.OnClosing?.Invoke();
	}

	private void CreateLocal()
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "WF7 Sicherung|*.wf7",
			DefaultExt = "wf7",
			AddExtension = true,
			Title = "Lokale F7 Sicherung"
		};
		if (saveFileDialog.ShowDialog() == true)
		{
			CreateZip(saveFileDialog.FileName);
			this.OnClosing?.Invoke();
		}
	}

	private void CreateZip(string zipName = null)
	{
		string path = _pathService.PNHOMEDRIVE + _pathService.PNHOMEPATH;
		string path2 = string.Format("doc.c3do", DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
		string zipFilepath = zipName ?? Path.Combine(path, Path.GetFileNameWithoutExtension(path2) + ".wf7");
		F7Zipper = new F7Zip(zipFilepath, _pathService, _configProvider, _currentDoc);
		F7Zipper.Create(Desc);
	}
}
