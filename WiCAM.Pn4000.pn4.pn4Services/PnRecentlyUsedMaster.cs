using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.Extensions;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.pn4.pn4UILib.Ribbon;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.pn4.pn4Services;

public class PnRecentlyUsedMaster
{
	private const string RecentlyBMach = "BMACH";

	private const string RecentlyToolCalc = "ToolCalc";

	private static readonly int _maxCount = 6;

	private List<RecentlyUsedRecord> _recentlyUsedData;

	private readonly Dictionary<object, RecentlyUsedRecord> _connectionButtonRecord;

	private Dictionary<Button, RecentlyUsedRecord> _previewButtonConnection;

	private Image _previewViewer;

	private Label _previewDescription;

	private Style _buttonStyleList;

	private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();

	private static readonly string _recFilename = "RecentlyUsed.xml";

	private BackgroundWorker _worker;

	private RecentlyUsedRecord _currenGenerating;

	private readonly int _pnmajorversion;

	private int _menutype;

	private object _list;

	private string _datatype;

	private int _recordMax;

	private object _prevLastEnter;

	private object _latestBigPreviewButton;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly IFactorio _factorio;

	private readonly IPnToolTipService _pnToolTipService;

	private readonly IPnPathService _pnPathService;

	private readonly IPnIconsService _pnIconsService;

	private readonly ILogCenterService _logCenterService;

	private readonly IConfigProvider _configProvider;

	private readonly IPN3DDocPipe _pn3DDocPipe;

	private readonly IPN3DBendPipe _bendPipe;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly ExeFlow _exeFlow;

	private readonly PnRibbon _ribbon;

	private readonly IRibbonMainWindowConnector _ribbonMainWindowConnector;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IPnCommandsBend _commandsBend;

	private readonly IPnCommandBasics _commandsBasics;

	public PnRecentlyUsedMaster(IFactorio factorio, IPnToolTipService toolTip, IPnPathService pathService, IPnIconsService iconsService, ILogCenterService logCenter, IConfigProvider configProvider, IPN3DDocPipe pn3DDocPipe, IMainWindowDataProvider mainWindowDataProvider, ILanguageDictionary languageDictionary, ExeFlow exeFlow, PnRibbon ribbon, IRibbonMainWindowConnector ribbonMainWindowConnector, IPN3DBendPipe bendPipe, ICurrentDocProvider currentDocProvider, IPnColorsService pnColorsService, IPnCommandsBend commandsBend, IPnCommandBasics commandsBasics)
	{
		_factorio = factorio;
		_pnToolTipService = toolTip;
		_languageDictionary = languageDictionary;
		_pnPathService = pathService;
		_pnIconsService = iconsService;
		_logCenterService = logCenter;
		_configProvider = configProvider;
		_pn3DDocPipe = pn3DDocPipe;
		_mainWindowDataProvider = mainWindowDataProvider;
		_exeFlow = exeFlow;
		_ribbon = ribbon;
		_ribbonMainWindowConnector = ribbonMainWindowConnector;
		_bendPipe = bendPipe;
		_currentDocProvider = currentDocProvider;
		_connectionButtonRecord = new Dictionary<object, RecentlyUsedRecord>();
		_commandsBend = commandsBend;
		_commandsBasics = commandsBasics;
		_ribbonMainWindowConnector.OnFillMenu += FillMenu;
		_ribbonMainWindowConnector.OnUpdateLastMenu += UpdateLastMenu;
		_ribbonMainWindowConnector.OnResetAllPreviewNeeds += ResetAllPreviewNeeds;
		Load();
		_dispatcherTimer.Tick += dispatcherTimer_Tick;
		_dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
		_pnmajorversion = _pnPathService.GetMajorPnVersion();
	}

	public void DelRecentlyUsedRecord(RecentlyUsedRecord rec, bool save = true)
	{
		for (int i = 0; i < _recentlyUsedData.Count; i++)
		{
			if (rec.ArchiveID == _recentlyUsedData[i].ArchiveID && rec.FileName == _recentlyUsedData[i].FileName && rec.FullPath == _recentlyUsedData[i].FullPath && rec.Type == _recentlyUsedData[i].Type)
			{
				_recentlyUsedData.RemoveAt(i);
				if (save)
				{
					Save();
				}
				break;
			}
		}
	}

	public void DelRecentlyUsedRecord(List<string> memoryDisk)
	{
		RecentlyUsedRecord rec = CreateRecentlyUsedRecord(memoryDisk);
		DelRecentlyUsedRecord(rec);
	}

	public void DelRecentlyUsedRecordForType(string type)
	{
		foreach (RecentlyUsedRecord item in _recentlyUsedData.Where((RecentlyUsedRecord r) => r.Type == type).ToList())
		{
			_recentlyUsedData.Remove(item);
		}
		Save();
	}

	public void SetAllRecentlyUsedRecordForType(string type, IEnumerable<RecentlyUsedRecord> allRecords)
	{
		foreach (RecentlyUsedRecord item in _recentlyUsedData.Where((RecentlyUsedRecord r) => r.Type == type).ToList())
		{
			_recentlyUsedData.Remove(item);
		}
		_recentlyUsedData.InsertRange(0, allRecords);
		Save();
	}

	public void AddRecentlyUsedRecord(int archive, string partName, string path, string type)
	{
		RecentlyUsedRecord rec = new RecentlyUsedRecord
		{
			FileName = partName,
			FullPath = path,
			ArchiveID = archive,
			Type = type.ToUpper()
		};
		AddRecentlyUsedRecord(rec);
	}

	public void AddRecentlyUsedRecord(List<string> memoryDisk)
	{
		if (memoryDisk.Count == 4)
		{
			AddRecentlyUsedRecord(CreateRecentlyUsedRecord(memoryDisk));
		}
	}

	public void AddRecentlyUsedRecord(RecentlyUsedRecord rec, bool save = true)
	{
		_recentlyUsedData.Insert(0, rec);
		CheckFirstDuplicate();
		CheckMaxForType(rec.Type);
		if (save)
		{
			Save();
		}
	}

	private RecentlyUsedRecord CreateRecentlyUsedRecord(List<string> memoryDisk)
	{
		try
		{
			return new RecentlyUsedRecord
			{
				FileName = memoryDisk[1],
				FullPath = memoryDisk[2],
				ArchiveID = Convert.ToInt32(memoryDisk[0]),
				Type = memoryDisk[3].ToUpper()
			};
		}
		catch
		{
			return null;
		}
	}

	private void CheckFirstDuplicate()
	{
		RecentlyUsedRecord recentlyUsedRecord = _recentlyUsedData[0];
		for (int i = 1; i < _recentlyUsedData.Count; i++)
		{
			if (recentlyUsedRecord.ArchiveID == _recentlyUsedData[i].ArchiveID && recentlyUsedRecord.FileName == _recentlyUsedData[i].FileName && recentlyUsedRecord.FullPath == _recentlyUsedData[i].FullPath && recentlyUsedRecord.Type == _recentlyUsedData[i].Type)
			{
				_recentlyUsedData.RemoveAt(i);
				break;
			}
		}
	}

	private void UpdateLastMenu()
	{
		if (_list is StackPanel)
		{
			(_list as StackPanel).Children.Clear();
		}
		FillMenu(1, _list, _datatype, _recordMax, _previewViewer, _previewDescription);
	}

	public static void DoEvents()
	{
		Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate
		{
		});
	}

	private void FillMenu(int menutype, object list, string datatype, int recordMax, Image previewViewer, Label previewDescription)
	{
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		ResetAllPreviewNeeds();
		_previewViewer = previewViewer;
		_previewDescription = previewDescription;
		_prevLastEnter = null;
		if (menutype == 1)
		{
			_list = list;
			_datatype = datatype;
			_recordMax = recordMax;
		}
		_menutype = menutype;
		_connectionButtonRecord.Clear();
		int num = 0;
		int num2 = _maxCount;
		if (recordMax != 0)
		{
			num2 = recordMax;
		}
		if (datatype == "ToolCalc")
		{
			num2 = int.MaxValue;
		}
		_previewButtonConnection = new Dictionary<Button, RecentlyUsedRecord>();
		foreach (RecentlyUsedRecord recentlyUsedDatum in _recentlyUsedData)
		{
			if ((!datatype.ToUpper().Contains(recentlyUsedDatum.Type.ToUpper()) && !datatype.Contains("ALL")) || num >= num2)
			{
				continue;
			}
			num++;
			PnRibbonButton pnRibbonButton = new PnRibbonButton();
			pnRibbonButton.LargeImage = _pnIconsService.GetBigIcon($"FILE_{recentlyUsedDatum.Type.ToUpper()}");
			PnRibbonButton pnRibbonButton2 = pnRibbonButton;
			if (pnRibbonButton2.LargeImage == null)
			{
				ImageSource imageSource = (pnRibbonButton2.LargeImage = _pnIconsService.GetBigIcon("FILE"));
			}
			pnRibbonButton.Label = recentlyUsedDatum.FileName;
			string text = null;
			if (recentlyUsedDatum.ArchiveID > 0)
			{
				text = $"{_pnPathService.GetArchiveName(recentlyUsedDatum.ArchiveID).Trim()} ({recentlyUsedDatum.ArchiveID})";
				if (recentlyUsedDatum.Type == "BMACH")
				{
					text = recentlyUsedDatum.ArchiveID.ToString();
				}
				pnRibbonButton.LabelSub1 = text;
				pnRibbonButton.LabelSub2 = recentlyUsedDatum.FullPath;
			}
			else
			{
				pnRibbonButton.LabelSub1 = recentlyUsedDatum.FullPath;
				pnRibbonButton.LabelSub2 = string.Empty;
			}
			if (menutype == 0)
			{
				pnRibbonButton.Width = 220.0;
			}
			if (_buttonStyleList == null)
			{
				_buttonStyleList = (Style)(list as FrameworkElement).TryFindResource("PnRibbonButtonForRecentlyUsed");
			}
			pnRibbonButton.Style = _buttonStyleList;
			pnRibbonButton.Click += Btn_Click;
			pnRibbonButton.PreviewReady = false;
			if (!generalUserSettingsConfig.HideToolTips && menutype != 1)
			{
				string l = $"{recentlyUsedDatum.FileName} ({recentlyUsedDatum.Type})";
				string empty = string.Empty;
				string dectription = ((text == null) ? recentlyUsedDatum.FullPath : $"{text}\n{recentlyUsedDatum.FullPath}");
				empty = FileDescription(recentlyUsedDatum);
				_pnToolTipService.SetTooltip(pnRibbonButton, l, empty, dectription, pnRibbonButton.LargeImage);
			//	generalUserSettingsConfig.SetWpfToolTipTimes(pnRibbonButton);
			}
			if (list is WrapPanel)
			{
				(list as WrapPanel).Children.Add(pnRibbonButton);
			}
			if (menutype == 1)
			{
				(list as StackPanel).Children.Add(pnRibbonButton);
				pnRibbonButton.MouseEnter += Btn_MouseEnter;
				pnRibbonButton.MouseLeave += Btn_MouseLeave;
			}
			_connectionButtonRecord.Add(pnRibbonButton, recentlyUsedDatum);
		}
		if (_pnmajorversion >= 25 && generalUserSettingsConfig.PreviewRecentlyUsed)
		{
			RecognizeCurrentPreviewNeeds();
		}
	}

	public string FileDescription(RecentlyUsedRecord r)
	{
		string result = string.Empty;
		try
		{
			string text = r.FullPath + r.FileName;
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				result = $"{fileInfo.LastWriteTime} ({(fileInfo.Length / 1024).ToString()} kb)";
			}
		}
		catch
		{
		}
		return result;
	}

	private void ResetAllPreviewNeeds()
	{
		foreach (RecentlyUsedRecord recentlyUsedDatum in _recentlyUsedData)
		{
			recentlyUsedDatum.NeedPreview = false;
		}
		while (_worker != null)
		{
			Thread.Sleep(10);
			DoEvents();
		}
	}

	private void RecognizeCurrentPreviewNeeds()
	{
		ResetAllPreviewNeeds();
		foreach (RecentlyUsedRecord value in _connectionButtonRecord.Values)
		{
			if (value.Drawer == null)
			{
				value.NeedPreview = true;
				continue;
			}
			string text = value.FullPath + value.FileName;
			bool flag = false;
			try
			{
				if (File.Exists(text))
				{
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.LastWriteTime != value.PreviewDate)
					{
						flag = true;
						value.PreviewDate = fileInfo.LastWriteTime;
					}
				}
			}
			catch
			{
			}
			if (flag)
			{
				value.NeedPreview = true;
			}
			else
			{
				ShowPreviewForRecord(value);
			}
		}
		GenerateOnePreview();
	}

	private void GenerateOnePreview()
	{
		_currenGenerating = GetRecordForPreview();
		if (_currenGenerating != null)
		{
			_worker = new BackgroundWorker();
			_worker.DoWork += worker_DoWork;
			_worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
			_worker.RunWorkerAsync();
		}
	}

	private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		_worker = null;
		ShowPreviewForRecord(_currenGenerating);
		GenerateOnePreview();
	}

	private void ShowPreviewForRecord(RecentlyUsedRecord currenGenerating)
	{
		currenGenerating.NeedPreview = false;
		if (!_connectionButtonRecord.Values.Contains(currenGenerating) || currenGenerating.Drawer == null)
		{
			return;
		}
		PnRibbonButton buttonConnectTo = GetButtonConnectTo(currenGenerating);
		if (buttonConnectTo == null)
		{
			return;
		}
		buttonConnectTo.LargeImage = currenGenerating.Drawer.DrawCadGeo(32, 32, -2);
		if (buttonConnectTo.ToolTip != null)
		{
			_pnToolTipService.UpdateTooltipWithPreview(buttonConnectTo, currenGenerating.Drawer.DrawCadGeo(300, 300, -2));
		}
		try
		{
			string text = currenGenerating.FullPath + currenGenerating.FileName;
			if (File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				currenGenerating.PreviewDate = fileInfo.LastWriteTime;
			}
		}
		catch
		{
		}
		if (_latestBigPreviewButton == buttonConnectTo)
		{
			Preview_Click(buttonConnectTo, null);
		}
	}

	private PnRibbonButton GetButtonConnectTo(RecentlyUsedRecord currenGenerating)
	{
		foreach (object key in _connectionButtonRecord.Keys)
		{
			if (_connectionButtonRecord[key] == currenGenerating)
			{
				return key as PnRibbonButton;
			}
		}
		return null;
	}

	private void worker_DoWork(object sender, DoWorkEventArgs e)
	{
		try
		{
			switch (_currenGenerating.Type)
			{
			case "VAR":
			case "MPL":
			case "N2D":
			case "M2D":
				_currenGenerating.Drawer = _factorio.Resolve<GadGeoDrawMultiOutput>();
				_currenGenerating.Drawer.Init(GetRealPreviewPath(_currenGenerating, _pnPathService));
				break;
			case "DXF":
			case "DWG":
			{
				IAcad2PNLauncher acad2PNLauncher = _factorio.Resolve<IAcad2PNLauncher>();
				if (acad2PNLauncher.ImportAutoCadFile(_currenGenerating.FullPath + _currenGenerating.FileName, force_text2geometry: true, force_keeporgin: false, 5000))
				{
					IAcad2PNWpf acad2PNWpf = _factorio.Resolve<IAcad2PNWpf>();
					_currenGenerating.Drawer = (GadGeoDrawMultiOutput)acad2PNWpf.GetDrawer(acad2PNLauncher);
				}
				break;
			}
			}
		}
		catch
		{
		}
	}

	private RecentlyUsedRecord GetRecordForPreview()
	{
		foreach (RecentlyUsedRecord value in _connectionButtonRecord.Values)
		{
			if (value.NeedPreview)
			{
				return value;
			}
		}
		return null;
	}

	private void Btn_MouseLeave(object sender, MouseEventArgs e)
	{
		_dispatcherTimer.Stop();
	}

	private void Btn_MouseEnter(object sender, MouseEventArgs e)
	{
		_dispatcherTimer.Stop();
		_prevLastEnter = sender;
		_dispatcherTimer.Start();
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		_dispatcherTimer.Stop();
		if (_prevLastEnter != null)
		{
			Preview_Click(_prevLastEnter, null);
		}
	}

	private void Preview_Click(object sender, RoutedEventArgs e)
	{
		if (sender == null)
		{
			return;
		}
		_latestBigPreviewButton = sender;
		if (_previewViewer == null || !_connectionButtonRecord.ContainsKey(sender))
		{
			return;
		}
		_previewViewer.Visibility = Visibility.Collapsed;
		_previewDescription.Content = string.Empty;
		RecentlyUsedRecord recentlyUsedRecord = _connectionButtonRecord[sender];
		if (recentlyUsedRecord.Drawer != null)
		{
			GetViewerWidth(_previewViewer, out var vx, out var vy);
			if (vx > 0 && vy > 0)
			{
				_previewViewer.Visibility = Visibility.Visible;
				_previewViewer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
				_previewViewer.Source = recentlyUsedRecord.Drawer.DrawCadGeo(vx, vy, 0);
			}
			else
			{
				_previewViewer.Source = null;
			}
			if (_previewDescription != null)
			{
				_previewDescription.Content = FileDescription(recentlyUsedRecord);
			}
		}
	}

	private void GetViewerWidth(Image viewer, out int vx, out int vy)
	{
		Grid grid = viewer.Parent as Grid;
		vx = (int)grid.ActualWidth;
		vy = (int)grid.RowDefinitions[Grid.GetColumn(viewer)].ActualHeight;
	}

	private static string GetRealPreviewPath(RecentlyUsedRecord r, IPnPathService pnPathService)
	{
		string type = r.Type;
		if (!(type == "MPL"))
		{
			if (type == "N2D")
			{
				return r.FullPath.Replace("n2d", "c2d") + r.FileName;
			}
			return r.FullPath + r.FileName;
		}
		return pnPathService.GetArchivePath(r.ArchiveID, 16) + r.FileName;
	}

	private void Btn_Click(object sender, RoutedEventArgs e)
	{
		if (_menutype == 1)
		{
			_ribbonMainWindowConnector.SetMainScreenLayout(0);
		}
		_ribbon.CloseCodePopup();
		if (_connectionButtonRecord != null && _connectionButtonRecord.ContainsKey(sender))
		{
			RecentlyUsedRecord recentlyUsedRecord = _connectionButtonRecord[sender];
			if (recentlyUsedRecord.Type == "PN3D")
			{
				_factorio.Resolve<IPnCommandsOther>().ImportUniversal(new PnCommandArg(null, null)
				{
					CommandStr = "RecentlyUsed"
				}, new ImportArg
				{
					Filename = recentlyUsedRecord.FullPath + recentlyUsedRecord.FileName,
					CheckLicense = true,
					MoveToCenter = true
				});
			}
			else if (recentlyUsedRecord.Type == "VIEW3D")
			{
				_factorio.Resolve<IPnCommandsOther>().ImportUniversal(new PnCommandArg(null, null)
				{
					CommandStr = "RecentlyUsed"
				}, new ImportArg
				{
					Filename = recentlyUsedRecord.FullPath + recentlyUsedRecord.FileName,
					CheckLicense = true,
					MoveToCenter = true,
					UseHd = false
				});
			}
			else if (recentlyUsedRecord.Type == "P3D")
			{
				_factorio.Resolve<IPnCommandsOther>().ImportUniversal(new PnCommandArg(null, null)
				{
					CommandStr = "RecentlyUsed"
				}, new ImportArg
				{
					Filename = recentlyUsedRecord.FullPath + recentlyUsedRecord.FileName
				});
				_mainWindowDataProvider.AddRecentlyUsedRecord(recentlyUsedRecord);
			}
			else if (recentlyUsedRecord.Type == "BMACH")
			{
				_bendPipe.SelectMachineRecentlyUsed(recentlyUsedRecord.FileName, recentlyUsedRecord.FullPath, recentlyUsedRecord.ArchiveID, _currentDocProvider.CurrentDoc);
			}
			else if (recentlyUsedRecord.Type == "ToolCalc")
			{
				_commandsBend.SetTools(_commandsBasics.CreateCommandArg(_currentDocProvider.CurrentDoc), recentlyUsedRecord.ArchiveID);
			}
			else
			{
				List<string> list = new List<string>();
				list.Add(recentlyUsedRecord.ArchiveID.ToString());
				list.Add(recentlyUsedRecord.FileName);
				list.Add(recentlyUsedRecord.FullPath);
				list.Add(recentlyUsedRecord.Type);
				_exeFlow.CallRecenlty(list);
			}
		}
	}

	private void CheckMaxForType(string type)
	{
		int num = 0;
		List<RecentlyUsedRecord> list = new List<RecentlyUsedRecord>();
		foreach (RecentlyUsedRecord recentlyUsedDatum in _recentlyUsedData)
		{
			if (recentlyUsedDatum.Type == type)
			{
				num++;
				if (num > GetMaxForType(type))
				{
					list.Add(recentlyUsedDatum);
				}
			}
		}
		foreach (RecentlyUsedRecord item in list)
		{
			_recentlyUsedData.Remove(item);
		}
	}

	private int GetMaxForType(string type)
	{
		if (type == "ToolCalc")
		{
			return int.MaxValue;
		}
		return _maxCount;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public void Load()
	{
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RecentlyUsedRecord>));
			if (File.Exists(_recFilename))
			{
				using FileStream stream = new FileStream(_recFilename, FileMode.Open, FileAccess.Read, FileShare.Read);
				_recentlyUsedData = (List<RecentlyUsedRecord>)xmlSerializer.Deserialize(stream);
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
		if (_recentlyUsedData == null)
		{
			_recentlyUsedData = new List<RecentlyUsedRecord>();
		}
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public void Save()
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RecentlyUsedRecord>));
		try
		{
			using TextWriter textWriter = new StreamWriter(_recFilename);
			xmlSerializer.Serialize(textWriter, _recentlyUsedData);
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMachineRecords()
	{
		return _recentlyUsedData.Where((RecentlyUsedRecord r) => r.Type == "BMACH");
	}

	public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedMaterialRecords()
	{
		return _recentlyUsedData.Where((RecentlyUsedRecord r) => r.Type == "Mat3D");
	}

	public IEnumerable<RecentlyUsedRecord> GetRecentlyUsedImportRecords()
	{
		return _recentlyUsedData.Where((RecentlyUsedRecord r) => r.Type == "Imp3D");
	}
}
