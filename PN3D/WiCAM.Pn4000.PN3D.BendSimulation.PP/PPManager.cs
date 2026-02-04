using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.Reporting;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP;

internal class PPManager : IPpManager
{
    [CompilerGenerated]
    private sealed class _003CSplitProgramms_003Ed__11 : IEnumerable<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)>, 
        IEnumerable, IEnumerator<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)>, IEnumerator, IDisposable
    {
        private int _003C_003E1__state;

        private (string subFolder, List<ICombinedBendDescriptor> bends, int subNo) _003C_003E2__current;

        private int _003C_003El__initialThreadId;

        private IDoc3d doc;

        public IDoc3d _003C_003E3__doc;

        private IEnumerable<ICombinedBendDescriptor> bends;

        public IEnumerable<ICombinedBendDescriptor> _003C_003E3__bends;

        private string tempFolder;

        public string _003C_003E3__tempFolder;

        private IEnumerator<(int subNo, List<ICombinedBendDescriptor> subBends)> _003C_003E7__wrap1;

        (string subFolder, List<ICombinedBendDescriptor> bends, int subNo) IEnumerator<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)>.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return _003C_003E2__current;
            }
        }

        [DebuggerHidden]
        public _003CSplitProgramms_003Ed__11(int _003C_003E1__state)
        {
            this._003C_003E1__state = _003C_003E1__state;
            _003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            int num = _003C_003E1__state;
            if (num == -3 || num == 1)
            {
                try
                {
                }
                finally
                {
                    _003C_003Em__Finally1();
                }
            }
            _003C_003E7__wrap1 = null;
            _003C_003E1__state = -2;
        }

        private bool MoveNext()
        {
            try
            {
                switch (_003C_003E1__state)
                {
                    default:
                        return false;
                    case 0:
                        _003C_003E1__state = -1;
                        _003C_003E7__wrap1 = doc.BendsPerPp(bends).GetEnumerator();
                        _003C_003E1__state = -3;
                        break;
                    case 1:
                        _003C_003E1__state = -3;
                        break;
                }
                if (_003C_003E7__wrap1.MoveNext())
                {
                    (int subNo, List<ICombinedBendDescriptor> subBends) current = _003C_003E7__wrap1.Current;
                    int item = current.subNo;
                    List<ICombinedBendDescriptor> item2 = current.subBends;
                    DirectoryCreate(tempFolder);
                    string text = Path.Combine(tempFolder, $"SubPp{item}");
                    DirectoryCreate(text);
                    _003C_003E2__current = (subFolder: text, bends: item2, subNo: item);
                    _003C_003E1__state = 1;
                    return true;
                }
                _003C_003Em__Finally1();
                _003C_003E7__wrap1 = null;
                return false;
            }
            catch
            {
                //try-fault
                ((IDisposable)this).Dispose();
                throw;
            }
        }

        bool IEnumerator.MoveNext()
        {
            //ILSpy generated this explicit interface implementation from .override directive in MoveNext
            return this.MoveNext();
        }

        private void _003C_003Em__Finally1()
        {
            _003C_003E1__state = -1;
            if (_003C_003E7__wrap1 != null)
            {
                _003C_003E7__wrap1.Dispose();
            }
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        IEnumerator<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)> IEnumerable<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)>.GetEnumerator()
        {
            _003CSplitProgramms_003Ed__11 _003CSplitProgramms_003Ed__;
            if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
            {
                _003C_003E1__state = 0;
                _003CSplitProgramms_003Ed__ = this;
            }
            else
            {
                _003CSplitProgramms_003Ed__ = new _003CSplitProgramms_003Ed__11(0);
            }
            _003CSplitProgramms_003Ed__.doc = _003C_003E3__doc;
            _003CSplitProgramms_003Ed__.bends = _003C_003E3__bends;
            _003CSplitProgramms_003Ed__.tempFolder = _003C_003E3__tempFolder;
            return _003CSplitProgramms_003Ed__;
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<(string, List<ICombinedBendDescriptor>, int)>)this).GetEnumerator();
        }
    }

    private readonly IFactorio _factorio;

    private readonly IAutoMode _autoMode;

    private readonly IPnPathService _pathService;

    private readonly ITranslator _translator;

    private readonly IMessageLogGlobal _messageLog;

    private readonly ILogCenterService _logCenterService;

    private readonly IReportCreator _reportCreator;

    public PPManager(IFactorio factorio, IAutoMode autoMode, IPnPathService pathService, ITranslator translator, IMessageLogGlobal messageLog, ILogCenterService logCenterService, IReportCreator reportCreator)
    {
        _factorio = factorio;
        _autoMode = autoMode;
        _pathService = pathService;
        _translator = translator;
        _messageLog = messageLog;
        _logCenterService = logCenterService;
        _reportCreator = reportCreator;
    }

    public void ClearBendTempDir()
    {
        try
        {
            string userFilePath = _pathService.GetUserFilePath("bendTemp");
            LogFileAccess(userFilePath, "ClearBendTempDir"); // Logging hinzufügen
            if (Directory.Exists(userFilePath))
            {
                Directory.Delete(userFilePath, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logCenterService.Debug($"Failed to clear bend temp directory: {ex.Message}");
        }
    }

    public void CalculatePpInformation(IDoc3d doc)
    {
        throw new NotImplementedException();
    }

    public F2exeReturnCode CreateNC(string fileName, IDoc3d doc, bool createNc, bool createReport, string reportFormat)
    {
        if (doc.EntryModel3D.PartInfo.PartType == PartType.FlatSheetMetal)
        {
            return F2exeReturnCode.OK;
        }
        if (doc.BendSimulation == null)
        {
            return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
        }
        if (!doc.HasRequiredUserComments() && _factorio.Resolve<IConfigProvider>().InjectOrCreate<UserCommentsConfig>().CheckBeforePp)
        {
            doc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.PPGeneration.RequiredCommentsMissing");
            return F2exeReturnCode.CANCEL_BY_USER;
        }
        double currentStep = doc.BendSimulation.State.CurrentStep;
        IBendSelection bendSelection = doc.Factorio.Resolve<IBendSelection>();
        IBendPositioning currentBend = doc.ToolsAndBends.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == doc.BendSimulation.State.ActiveStep.BendInfo.Order);
        if (doc.BendMachine?.PostProcessor == null)
        {
            doc.MessageDisplay.ShowTranslatedErrorMessage("BendMachine.NoPp", "");
            return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
        }
        F2exeReturnCode f2exeReturnCode = _factorio.Resolve<IPN3DBendPipe>().ValidateSimulation(_autoMode.PopupsEnabled, doc, AutoContinueIfOk: true);
        if (f2exeReturnCode != 0 && !doc.FrontCalls.CreatePostprocessorIfErrorsSimValidation())
        {
            bendSelection.CurrentBend = currentBend;
            return f2exeReturnCode;
        }
        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            string bendTempFolder = doc.BendTempFolder;
            LogFileAccess(bendTempFolder, "CreateNC"); // Logging hinzufügen
            
            if (!EnsureDirectoryExists(bendTempFolder)) // Sicherere Verzeichniserstellung
            {
                return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
            }

            List<ICombinedBendDescriptor> bends = doc.CombinedBendDescriptors.OfType<ICombinedBendDescriptor>().ToList();
            List<string> list = doc.NamesPpBase.ToList();
            string namePPSuffix = doc.NamePPSuffix;
            List<(string, List<ICombinedBendDescriptor>, int)> list2 = SplitProgramms(doc, bends, bendTempFolder, doc.BendMachine.PostProcessor).ToList();
            foreach (var item4 in list2)
            {
                string item = item4.Item1;
                List<ICombinedBendDescriptor> item2 = item4.Item2;
                int item3 = item4.Item3;
                string text = ((list.Count >= item3) ? list[item3 - 1] : string.Empty);
                if (string.IsNullOrEmpty(text))
                {
                    text = ((!doc.BendMachine.PressBrakeData.PpNameAuto) ? doc.DiskFile?.Header?.ModelName : doc.GetAutoPpName(list2.Count > 1, item3));
                }
                string text2 = GetSafeFileName(text + namePPSuffix); // Sichere Dateinamenverarbeitung
                List<IPPError> errors;
                IPpData ppData = doc.BendMachine.PostProcessor.CreateNC(doc, _factorio, item, item2, text2, out errors);
                File.WriteAllText(Path.Combine(item, "Name.txt"), text2, Encoding.UTF8);
                if (ppData == null || errors.Count > 0 || !doc.BendMachine.PostProcessor.WriteNC(item, ppData, _factorio))
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                    bendSelection.CurrentBend = currentBend;
                    string message = string.Join(Environment.NewLine, errors.Select((IPPError x) => x.ToString(_translator)));
                    string caption = _translator.Translate("l_popup.PPGeneration.ExceptionMessage");
                    doc.MessageDisplay.ShowWarningMessage(message, caption, notificationStyle: true);
                    return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
                }
            }
            Thread.CurrentThread.CurrentCulture = currentCulture;
            bendSelection.CurrentBend = currentBend;
            if (createReport && (CreateReportConvert(doc, reportFormat, out var errors2) == 0 || errors2.Count > 0))
            {
                doc.MessageDisplay.ShowWarningMessage(_translator.Translate("l_popup.PPGeneration.ReportFailed"), null, notificationStyle: true);
                return F2exeReturnCode.ERROR_NO_REPORT_GENERATED;
            }
            return F2exeReturnCode.OK;
        }
        catch (Exception e)
        {
            _logCenterService.CatchRaport(e);
            doc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.PPGeneration.ExceptionMessage");
            return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
        }
    }

    [IteratorStateMachine(typeof(_003CSplitProgramms_003Ed__11))]
    private IEnumerable<(string subFolder, List<ICombinedBendDescriptor> bends, int subNo)> SplitProgramms(IDoc3d doc, IEnumerable<ICombinedBendDescriptor> bends, string tempFolder, IPostProcessor postprocessor)
    {
        //yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
        return new _003CSplitProgramms_003Ed__11(-2)
        {
            _003C_003E3__doc = doc,
            _003C_003E3__bends = bends,
            _003C_003E3__tempFolder = tempFolder
        };
    }

    private static bool CheckDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            return DirectoryCreate(path);
        }
        return true;
    }

    private static bool DirectoryCreate(string path)
    {
        return new PPManager(null, null, null, null, null, null, null).EnsureDirectoryExists(path); // Erstellen Sie ein neues Objekt, um die Methode aufzurufen
    }

    public F2exeReturnCode CreateReport(IDoc3d doc, string format)
    {
        List<Exception> errors;
        int num = CreateReportConvert(doc, format, out errors);
        if (_autoMode.PopupsEnabled)
        {
            string bendTempFolder = doc.BendTempFolder;
            if (Directory.Exists(bendTempFolder))
            {
                string[] directories = Directory.GetDirectories(bendTempFolder);
                for (int i = 0; i < directories.Length; i++)
                {
                    string text = Path.Combine(directories[i], "TemplateProductionPaper.trdp");
                    if (File.Exists(text))
                    {
                        ReportViewerWindow reportViewerWindow = new ReportViewerWindow();
                        reportViewerWindow.SetSource(text, format);
                        reportViewerWindow.ShowDialog();
                    }
                }
            }
        }
        if (errors.Count > 0 || num == 0)
        {
            doc.MessageDisplay.ShowWarningMessage(_translator.Translate("l_popup.PPGeneration.ReportFailed"), null, notificationStyle: true);
            return F2exeReturnCode.ERROR_NO_REPORT_GENERATED;
        }
        return F2exeReturnCode.OK;
    }

    public int CreateReportConvert(IDoc3d doc, string format, out List<Exception> errors)
    {
        errors = new List<Exception>();
        string bendTempFolder = doc.BendTempFolder;
        LogFileAccess(bendTempFolder, "CreateReportConvert"); // Logging hinzufügen
        
        int num = 0;
        if (Directory.Exists(bendTempFolder))
        {
            string[] directories = Directory.GetDirectories(bendTempFolder);
            foreach (string path in directories)
            {
                string text = Path.Combine(path, "TemplateProductionPaper.trdp");
                LogFileAccess(text, "CreateReportConvert - Template"); // Logging hinzufügen
                
                string filenameOutput = Path.Combine(path, GetSafeFileName("ProductionPaper")); // Sichere Dateinamenverarbeitung
                if (File.Exists(text))
                {
                    if (_reportCreator.Create(text, format, ref filenameOutput, out List<Exception> exceptions))
                    {
                        num++;
                    }
                    errors.AddRange(exceptions);
                }
            }
        }
        foreach (Exception error in errors)
        {
            _logCenterService.CatchRaport(error);
        }
        if (errors.Count == 0 && num > 0)
        {
            doc.ReportGenerated = true;
        }
        return num;
    }

    public void ResetTempFolder(IDoc3d doc)
    {
        string bendTempFolder = doc.BendTempFolder;
        if (Directory.Exists(bendTempFolder))
        {
            Directory.Delete(bendTempFolder, recursive: true);
        }
    }

    public F2exeReturnCode SendPPFiles(IDoc3d doc, bool sendNc, bool sendReport)
    {
        if (doc.EntryModel3D.PartInfo.PartType == PartType.FlatSheetMetal)
        {
            return F2exeReturnCode.OK;
        }
        if (doc.BendMachine?.PostProcessor == null)
        {
            doc.MessageDisplay.ShowTranslatedErrorMessage("BendMachine.NoPp", "");
            return F2exeReturnCode.ERROR_NO_NCFILE_GENERATED;
        }
        F2exeReturnCode f2exeReturnCode = _factorio.Resolve<IPN3DBendPipe>().CheckDocProgress(doc, DocState.FingerCalculated, showErrorMessage: true);
        if (f2exeReturnCode != 0)
        {
            return f2exeReturnCode;
        }
        F2exeReturnCode result = F2exeReturnCode.OK;
        string bendTempFolder = doc.BendTempFolder;
        LogFileAccess(bendTempFolder, "SendPPFiles"); // Logging hinzufügen
        
        if (!Directory.Exists(bendTempFolder))
        {
            _logCenterService.Debug($"Bend temp folder not found: {bendTempFolder}");
            return F2exeReturnCode.ERROR_FILE_NOT_EXIST;
        }
        string[] directories = Directory.GetDirectories(bendTempFolder);
        foreach (string text in directories)
        {
            IBendMachine bendMachine = doc.BendMachine;
            string text2 = bendMachine.PressBrakeData.NCDirectory;
            if (string.IsNullOrEmpty(text2) || bendMachine.PressBrakeData.NCDirectoryUserSelect)
            {
                text2 = doc.FrontCalls.OpenFolderDialog(doc, out var result2, text2);
                if (result2 == false)
                {
                    return F2exeReturnCode.CANCEL_BY_USER;
                }
                if (string.IsNullOrEmpty(text2))
                {
                    doc.MessageDisplay.ShowTranslatedErrorMessage("SendPpNoDestination");
                    return F2exeReturnCode.NOPPDESTINATION;
                }
            }
            string text3 = bendMachine.PressBrakeData.ReportDirectory;
            if (string.IsNullOrEmpty(text3))
            {
                text3 = text2;
            }
            if (!Directory.Exists(text))
            {
                doc.MessageDisplay.ShowTranslatedErrorMessage("SendPpNoFilesExists");
                return F2exeReturnCode.ERROR_FILE_NOT_EXIST;
            }
            Directory.CreateDirectory(text2);
            Directory.CreateDirectory(text3);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            bool flag = false;
            bool flag2 = false;
            string text4 = GetSafeFileName(File.ReadAllText(Path.Combine(text, "Name.txt"), Encoding.UTF8));
            if (sendReport)
            {
                foreach (FileInfo item in from x in Directory.GetFiles(text, "ProductionPaper.*")
                                          select new FileInfo(x))
                {
                    dictionary.Add(item.FullName, Path.Combine(text3, text4 + item.Extension));
                    flag = true;
                }
            }
            if (sendNc)
            {
                foreach (FileInfo item2 in from x in bendMachine.PostProcessor.GetOutputFiles(text, doc)
                                           where !string.IsNullOrEmpty(x)
                                           select new FileInfo(x))
                {
                    dictionary.Add(item2.FullName, Path.Combine(text2, item2.Name));
                    flag2 = true;
                }
            }
            List<KeyValuePair<string, string>> list = dictionary.Where((KeyValuePair<string, string> file) => File.Exists(file.Value)).ToList();
            if (_autoMode.PopupsEnabled)
            {
                ITranslator translator = _translator;
                StringBuilder stringBuilder = new StringBuilder();
                string caption = translator.Translate("SendPpMessageTitle");
                if (sendReport)
                {
                    if (flag)
                    {
                        stringBuilder.AppendLine(translator.Translate("SendPpReportExist"));
                    }
                    else
                    {
                        stringBuilder.AppendLine(translator.Translate("SendPpReportNotExist"));
                    }
                }
                if (sendNc)
                {
                    if (flag2)
                    {
                        stringBuilder.AppendLine(translator.Translate("SendPpBncExist"));
                    }
                    else
                    {
                        stringBuilder.AppendLine(translator.Translate("SendPpBncNotExist"));
                    }
                }
                if ((sendReport && !flag) || (sendNc && !flag2))
                {
                    stringBuilder.AppendLine(translator.Translate("SendPpIgnoreMissingFile"));
                }
                if (list.Count > 0)
                {
                    stringBuilder.AppendLine(translator.Translate("SendPpOverrideFile"));
                    foreach (KeyValuePair<string, string> item3 in list)
                    {
                        stringBuilder.AppendLine(item3.Value);
                    }
                }
                if (MessageBox.Show(stringBuilder.ToString(), caption, MessageBoxButton.OKCancel) != MessageBoxResult.OK)
                {
                    return F2exeReturnCode.CANCEL_BY_USER;
                }
            }
            else
            {
                if (sendNc && !flag2)
                {
                    return F2exeReturnCode.ERROR_FILE_NOT_EXIST_NC;
                }
                if (sendReport && !flag)
                {
                    return F2exeReturnCode.ERROR_FILE_NOT_EXIST_REPORT;
                }
            }
            foreach (KeyValuePair<string, string> item4 in dictionary)
            {
                try
                {
                    File.Copy(item4.Key, item4.Value, overwrite: true);
                    doc.BendMachine?.PostProcessor.EmbedFileName(item4.Value);
                }
                catch (Exception)
                {
                    doc.MessageDisplay.ShowTranslatedErrorMessage("SendPpErrorFile", item4.Key);
                    result = F2exeReturnCode.ERROR_FILE_WRITE_FAILED;
                }
            }
            string ppSendBatch = doc.BendMachine.PpSendBatch;
            if (File.Exists(ppSendBatch))
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c" + ppSendBatch;
                process.Start();
            }
            doc.NcDataSend = true;
        }
        return result;
    }

    public F2exeReturnCode GeneratePpName(IDoc3d doc)
    {
        if (doc.BendMachine == null)
        {
            return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
        }
        try
        {
            doc.NumberPp = doc.BendMachine.GeneratePpNumber();
            List<(int, List<ICombinedBendDescriptor>)> list = doc.BendsPerPp(doc.CombinedBendDescriptors).ToList();
            List<string> list2 = new List<string>();
            using (List<(int, List<ICombinedBendDescriptor>)>.Enumerator enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    list2.Add(doc.GetAutoPpName(subNo: enumerator.Current.Item1, multi: list.Count > 1));
                }
            }
            doc.SetNamesPpBase(list2);
            return F2exeReturnCode.OK;
        }
        catch (Exception exception)
        {
            doc.MessageDisplay.ShowErrorMessage(exception);
        }
        return F2exeReturnCode.ERROR_FATAL;
    }

    // 1. Zusätzliches Logging für Dateizugriffe einführen
    private void LogFileAccess(string filename, string operation)
    {
        Console.WriteLine($"File operation '{operation}' on: {filename}");
        if (!File.Exists(filename))
        {
            Console.WriteLine($"File does not exist: {filename}");
        }
    }

    // 2. Sichere Dateinamenverarbeitung
    private string GetSafeFileName(string baseName)
    {
        if (string.IsNullOrEmpty(baseName))
        {
            Console.WriteLine($"Warning: Empty filename provided");
            return null;
        }
        
        // Entfernen ungültiger Zeichen
        string safeName = string.Join("_", baseName.Split(Path.GetInvalidFileNameChars()));
        Console.WriteLine($"Sanitized filename: {safeName}");
        return safeName;
    }

    // 3. Überprüfung temporärer Verzeichnisse
    private bool EnsureDirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"Creating directory: {path}");
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create directory {path}: {ex.Message}");
            return false;
        }
    }
}
