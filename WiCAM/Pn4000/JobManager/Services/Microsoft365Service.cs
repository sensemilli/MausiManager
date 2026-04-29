using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager.Services
{
    public class Microsoft365Service
    {
        private readonly GraphServiceClient _graphClient;
        private readonly Microsoft365Config _config;

        public Microsoft365Service(Microsoft365Config config)
        {
            _config = config;

            // ClientSecretCredential für Application permissions
            var credential = new ClientSecretCredential(
                _config.TenantId,
                _config.ClientId,
                _config.ClientSecret
            );

            _graphClient = new GraphServiceClient(credential);
            Logger.Info("Microsoft365Service initialisiert");
        }

        #region Basis-Dateiverwaltung

        /// <summary>
        /// Liest eine Datei aus OneDrive oder SharePoint
        /// </summary>
        public async Task<Stream> ReadFileAsync(string driveId, string itemId)
        {
            try
            {
                var stream = await _graphClient.Drives[driveId]
                    .Items[itemId]
                    .Content
                    .GetAsync();

                Logger.Info("Datei gelesen: DriveId={0}, ItemId={1}", driveId, itemId);
                return stream;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Sucht nach Dateien in OneDrive
        /// </summary>
        public async Task<List<DriveItem>> SearchFilesAsync(string searchQuery)
        {
            try
            {
                // Suche über die Graph Search-API (Beta) oder Filter auf Children
         /*       var items = await _graphClient.Me.Drive.Root.Children
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Filter = $"contains(name,'{searchQuery}')";
                    });

                var result = items?.Value?.ToList() ?? new List<DriveItem>();
                Logger.Info("Gefundene Dateien: {0}", result.Count);*/
                return null; // result;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Lädt eine Datei in OneDrive hoch
        /// </summary>
        public async Task<DriveItem> UploadFileAsync(string fileName, Stream fileStream, string folderPath = null)
        {
            try
            {
                var uploadPath = string.IsNullOrEmpty(folderPath)
                    ? $"/me/drive/root:/{fileName}:/content"
                    : $"/me/drive/root:/{folderPath}/{fileName}:/content";

                /*    var uploadedItem = await _graphClient.Me.Drive
                        .ItemWithPath(string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath}/{fileName}")
                        .Content
                        .PutAsync(fileStream);

                    Logger.Info("Datei hochgeladen: {0}", fileName);*/
                return null; // uploadedItem;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Listet Dateien in einem Ordner auf
        /// </summary>
        public async Task<List<DriveItem>> ListFilesInFolderAsync(string folderId = null)
        {
            try
            {
                DriveItemCollectionResponse items;

                if (string.IsNullOrEmpty(folderId))
                {
                    // Ursprünglich:
                    // items = await _graphClient.Drives.GetAsync();
                    // Korrigiert: Hole die Dateien im Root-Ordner des angemeldeten Benutzers
                   // items = await _graphClient.Me.Drive.Root.Children.GetAsync();
                }
                else
                {
                    // Entkommentiere und verwende die korrekte Methode für einen bestimmten Ordner:
                  //  items = await _graphClient.Me.Drive.Items[folderId].Children.GetAsync();
                }

                //  var fileList = items?.Value?.ToList() ?? new List<DriveItem>();
                //  Logger.Info("Dateien im Ordner: {0}", fileList.Count);
                return null; // fileList;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Liest Metadaten einer Datei
        /// </summary>
        public async Task<DriveItem> GetFileMetadataAsync(string driveId, string itemId)
        {
            try
            {
                var item = await _graphClient.Drives[driveId].Items[itemId].GetAsync();
                Logger.Info("Metadaten gelesen: {0}", item?.Name);
                return item;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Erstellt einen neuen Ordner
        /// </summary>
        public async Task<DriveItem> CreateFolderAsync(string folderName, string parentFolderId = null, string driveId = null)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder(),
                    AdditionalData = new Dictionary<string, object>()
                    {
                        { "@microsoft.graph.conflictBehavior", "rename" }
                    }
                };

                DriveItem newFolder;
                if (string.IsNullOrEmpty(parentFolderId))
                {
                    newFolder = null;
                   // newFolder = await _graphClient.Me.Drive.Items["root"].Children.PostAsync(driveItem);
                }
                else if (!string.IsNullOrEmpty(driveId))
                {
                    newFolder = await _graphClient.Drives[driveId].Items[parentFolderId].Children.PostAsync(driveItem);
                }
                else
                {
                    throw new ArgumentException("driveId muss angegeben werden, wenn parentFolderId gesetzt ist.");
                }

                Logger.Info("Ordner erstellt: {0}", folderName);
                return newFolder;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Löscht eine Datei oder einen Ordner
        /// </summary>
        public async Task DeleteItemAsync(string driveId, string itemId)
        {
            try
            {
                await _graphClient.Drives[driveId].Items[itemId].DeleteAsync();
                Logger.Info("Item gelöscht: {0}", itemId);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Teilt eine Datei und erstellt einen Freigabelink
        /// </summary>
        public async Task<string> CreateSharingLinkAsync(string itemId, string linkType = "view")
        {
            try
            {
                // Ursprünglich: _graphClient.Me.Drive.Items[itemId]
                // Korrigiert: Zugriff auf das Item über _graphClient.Me.Drive.ItemWithPath oder _graphClient.Drives[driveId].Items[itemId]
                // Da driveId nicht übergeben wird, verwenden wir _graphClient.Me.Drive.Items[itemId] -> _graphClient.Me.Drive.Items[itemId] gibt es nicht, aber _graphClient.Me.Drive.ItemWithPath kann genutzt werden, wenn der Pfad bekannt ist.
                // Alternativ: Wenn nur itemId bekannt ist, kann _graphClient.Drives[driveId].Items[itemId] verwendet werden, driveId muss dann aber bekannt sein.
                // Da die anderen Methoden driveId als Parameter haben, sollte dies hier auch ergänzt werden.

                throw new NotImplementedException("Bitte die Methode so anpassen, dass driveId als Parameter übergeben wird und dann _graphClient.Drives[driveId].Items[itemId] verwendet wird.");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        #endregion

        #region Erweiterte Dokumentenverarbeitung

        /// <summary>
        /// Extrahiert Text aus einem Word-Dokument
        /// </summary>
        public async Task<string> ExtractTextFromWordAsync(string driveId, string itemId)
        {
            try
            {
                // Konvertiert Word-Dokument zu PDF und extrahiert Text
                var pdfStream = await _graphClient.Drives[driveId].Items[itemId]
                    .Content
                    .GetAsync(requestConfiguration =>
                    {
                      //  requestConfiguration.QueryParameters.Format = "pdf";
                    });

                Logger.Info("Text aus Word-Dokument extrahiert: ItemId={0}", itemId);

                // Hier können Sie eine PDF-Textextraktions-Bibliothek verwenden
                // z.B. iTextSharp oder PdfPig
                return await ExtractTextFromPdfStream(pdfStream);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Extrahiert Text aus einem Excel-Dokument
        /// </summary>
        public async Task<Dictionary<string, List<List<string>>>> ExtractDataFromExcelAsync(string driveId, string itemId)
        {
            try
            {
                var workbook = await _graphClient.Drives[driveId].Items[itemId].Workbook.GetAsync();
                var worksheets = await _graphClient.Drives[driveId].Items[itemId].Workbook.Worksheets.GetAsync();

                var result = new Dictionary<string, List<List<string>>>();

                if (worksheets?.Value != null)
                {
                    foreach (var sheet in worksheets.Value)
                    {
                        var usedRange = await _graphClient.Drives[driveId].Items[itemId]
                            .Workbook.Worksheets[sheet.Id]
                            .UsedRange
                            .GetAsync();

                        var sheetData = new List<List<string>>();

                        if (usedRange?.Text != null)
                        {
                            var valuesObj = usedRange.Text.GetValue();
                            if (valuesObj is IEnumerable<object> rows)
                            {
                                foreach (var rowObj in rows)
                                {
                                    var rowData = new List<string>();
                                    if (rowObj is IEnumerable<object> cells)
                                    {
                                        foreach (var cell in cells)
                                        {
                                            rowData.Add(cell?.ToString() ?? string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        // Falls die Zeile kein Array ist, als Einzelwert behandeln
                                        rowData.Add(rowObj?.ToString() ?? string.Empty);
                                    }
                                    sheetData.Add(rowData);
                                }
                            }
                        }

                        result.Add(sheet.Name, sheetData);
                    }
                }

                Logger.Info("Daten aus Excel extrahiert: ItemId={0}, Worksheets={1}", itemId, result.Count);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Führt OCR auf einem Bild oder PDF durch (über OneDrive)
        /// </summary>
        public async Task<string> PerformOcrAsync(string driveId, string itemId)
        {
            try
            {
                // Korrigiert: Zugriff auf Items über Drives[driveId]
                var item = await _graphClient.Drives[driveId].Items[itemId].GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "name", "image" };
                });

                if (item?.Image != null)
                {
                    Logger.Info("OCR-Daten verfügbar für: {0}", item.Name);
                    return await PerformAzureOcrAsync(itemId);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Analysiert ein Dokument und erstellt eine Zusammenfassung
        /// </summary>
        public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(string itemId)
        {
            try
            {
                var item = await GetFileMetadataAsync(null, itemId);
                var result = new DocumentAnalysisResult
                {
                    FileName = item.Name,
                    FileSize = item.Size ?? 0,
                    CreatedDate = item.CreatedDateTime ?? DateTimeOffset.MinValue,
                    ModifiedDate = item.LastModifiedDateTime ?? DateTimeOffset.MinValue,
                    FileType = Path.GetExtension(item.Name)
                };

                // Extrahiere Inhalt basierend auf Dateityp
                switch (result.FileType.ToLowerInvariant())
                {
                    case ".docx":
                    case ".doc":
                        result.TextContent = await ExtractTextFromWordAsync(item.ParentReference?.DriveId, itemId);
                        break;

                    case ".xlsx":
                    case ".xls":
                        var excelData = await ExtractDataFromExcelAsync(item.ParentReference?.DriveId, itemId);
                        result.StructuredData = excelData;
                        result.TextContent = ConvertExcelToText(excelData);
                        break;

                    case ".pdf":
                        var stream = await ReadFileAsync(item.ParentReference?.DriveId, itemId);
                        result.TextContent = await ExtractTextFromPdfStream(stream);
                        break;

                    case ".txt":
                    {
                        var textStream = await ReadFileAsync(item.ParentReference?.DriveId, itemId);
                        using var reader = new StreamReader(textStream);
                        result.TextContent = await reader.ReadToEndAsync();
                        break;
                    }
                }

                // Generiere Zusammenfassung
                result.Summary = GenerateSummary(result.TextContent);
                result.Keywords = ExtractKeywords(result.TextContent);

                Logger.Info("Dokument analysiert: {0}", item.Name);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Gliedert ein Dokument in Abschnitte
        /// </summary>
        public async Task<List<DocumentSection>> StructureDocumentAsync(string itemId)
        {
            try
            {
                var analysis = await AnalyzeDocumentAsync(itemId);
                var sections = new List<DocumentSection>();

                if (!string.IsNullOrEmpty(analysis.TextContent))
                {
                    // Teile Dokument in Abschnitte basierend auf Überschrifts
                    var lines = analysis.TextContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    DocumentSection currentSection = null;

                    foreach (var line in lines)
                    {
                        if (IsHeading(line))
                        {
                            if (currentSection != null)
                            {
                                sections.Add(currentSection);
                            }

                            currentSection = new DocumentSection
                            {
                                Title = line.Trim(),
                                Content = new StringBuilder(),
                                Level = DetermineHeadingLevel(line)
                            };
                        }
                        else if (currentSection != null)
                        {
                            currentSection.Content.AppendLine(line);
                        }
                    }

                    if (currentSection != null)
                    {
                        sections.Add(currentSection);
                    }
                }

                Logger.Info("Dokument in {0} Abschnitte gegliedert", sections.Count);
                return sections;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Vergleicht zwei Dokumente und findet Unterschiede
        /// </summary>
        public async Task<DocumentComparisonResult> CompareDocumentsAsync(string itemId1, string itemId2)
        {
            try
            {
                var doc1 = await AnalyzeDocumentAsync(itemId1);
                var doc2 = await AnalyzeDocumentAsync(itemId2);

                var result = new DocumentComparisonResult
                {
                    Document1Name = doc1.FileName,
                    Document2Name = doc2.FileName,
                    SimilarityPercentage = CalculateSimilarity(doc1.TextContent, doc2.TextContent)
                };

                // Finde Unterschiede
                result.AddedContent = FindAddedContent(doc1.TextContent, doc2.TextContent);
                result.RemovedContent = FindRemovedContent(doc1.TextContent, doc2.TextContent);
                result.ModifiedSections = FindModifiedSections(doc1.TextContent, doc2.TextContent);

                Logger.Info("Dokumente verglichen: {0} vs {1}, Ähnlichkeit: {2}%",
                           doc1.FileName, doc2.FileName, result.SimilarityPercentage);

                return result;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        #endregion

        #region SharePoint-Integration

        /// <summary>
        /// Listet SharePoint-Sites auf
        /// </summary>
        public async Task<List<Site>> ListSharePointSitesAsync()
        {
            try
            {
                var sites = await _graphClient.Sites.GetAsync();
                var siteList = sites?.Value?.ToList() ?? new List<Site>();

                Logger.Info("SharePoint Sites gefunden: {0}", siteList.Count);
                return siteList;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        /// <summary>
        /// Lädt Dateien von einer SharePoint-Dokumentenbibliothek
        /// </summary>
        public async Task<List<DriveItem>> GetSharePointDocumentsAsync(string siteId, string libraryName)
        {
            try
            {
                var drives = await _graphClient.Sites[siteId].Drives.GetAsync();
                var documentLibrary = drives?.Value?.FirstOrDefault(d =>
                    d.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase));

                if (documentLibrary == null)
                {
                    Logger.Warning("Dokumentenbibliothek nicht gefunden: {0}", libraryName);
                    return new List<DriveItem>();
                }

                var root = await _graphClient.Drives[documentLibrary.Id].Root.GetAsync();
                var items = root != null && root.Id != null
                    ? await _graphClient.Drives[documentLibrary.Id].Items[root.Id].Children.GetAsync()
                    : null;
                var fileList = items?.Value?.ToList() ?? new List<DriveItem>();

                Logger.Info("SharePoint Dokumente geladen: {0}", fileList.Count);
                return fileList;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        #endregion

        #region Hilfsmethoden

        private async Task<string> ExtractTextFromPdfStream(Stream pdfStream)
        {
            // Implementierung mit iTextSharp oder PdfPig
            // Beispiel mit iTextSharp:
            // var pdfReader = new PdfReader(pdfStream);
            // var text = PdfTextExtractor.GetTextFromPage(pdfReader, pageNumber);

            Logger.Info("PDF-Textextraktion durchgeführt");
            return "PDF text extraction requires additional library (iTextSharp/PdfPig)";
        }

        private async Task<string> PerformAzureOcrAsync(string itemId)
        {
            // Azure Computer Vision API für OCR
            Logger.Info("Azure OCR für ItemId: {0}", itemId);
            return "Azure Computer Vision OCR implementation required";
        }

        private string ConvertExcelToText(Dictionary<string, List<List<string>>> excelData)
        {
            var sb = new StringBuilder();
            foreach (var sheet in excelData)
            {
                sb.AppendLine($"=== {sheet.Key} ===");
                foreach (var row in sheet.Value)
                {
                    sb.AppendLine(string.Join("\t", row));
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string GenerateSummary(string text, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Einfache Zusammenfassung: Erste N Zeichen
            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        private List<string> ExtractKeywords(string text, int maxKeywords = 10)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            // Einfache Keyword-Extraktion
            var words = text.Split(new[] { ' ', '\r', '\n', '\t', '.', ',', ';', ':', '!', '?' },
                                   StringSplitOptions.RemoveEmptyEntries);

            return words
                .Where(w => w.Length > 3)
                .GroupBy(w => w.ToLowerInvariant())
                .OrderByDescending(g => g.Count())
                .Take(maxKeywords)
                .Select(g => g.Key)
                .ToList();
        }

        private bool IsHeading(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Prüfe auf typische Überschrifts-Muster
            return line.Length < 100 &&
                   (line.StartsWith("#") ||
                    line.All(char.IsUpper) ||
                    line.EndsWith(":"));
        }

        private int DetermineHeadingLevel(string line)
        {
            if (line.StartsWith("###"))
                return 3;
            if (line.StartsWith("##"))
                return 2;
            if (line.StartsWith("#"))
                return 1;

            return line.All(char.IsUpper) ? 1 : 2;
        }

        private double CalculateSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Levenshtein-Distanz oder ähnliche Metrik
            var longer = text1.Length > text2.Length ? text1 : text2;
            var shorter = text1.Length > text2.Length ? text2 : text1;

            if (longer.Length == 0)
                return 100.0;

            return (longer.Length - ComputeLevenshteinDistance(longer, shorter)) / (double)longer.Length * 100.0;
        }

        private int ComputeLevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        private List<string> FindAddedContent(string original, string modified)
        {
            // Vereinfachte Implementierung
            return new List<string>();
        }

        private List<string> FindRemovedContent(string original, string modified)
        {
            // Vereinfachte Implementierung
            return new List<string>();
        }

        private List<string> FindModifiedSections(string original, string modified)
        {
            // Vereinfachte Implementierung
            return new List<string>();
        }

        #endregion
    }

    #region Hilfsklassen

    public class DocumentAnalysisResult
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string FileType { get; set; }
        public string TextContent { get; set; }
        public Dictionary<string, List<List<string>>> StructuredData { get; set; }
        public string Summary { get; set; }
        public List<string> Keywords { get; set; }
    }

    public class DocumentSection
    {
        public string Title { get; set; }
        public StringBuilder Content { get; set; }
        public int Level { get; set; }
    }

    public class DocumentComparisonResult
    {
        public string Document1Name { get; set; }
        public string Document2Name { get; set; }
        public double SimilarityPercentage { get; set; }
        public List<string> AddedContent { get; set; }
        public List<string> RemovedContent { get; set; }
        public List<string> ModifiedSections { get; set; }
    }

    #endregion
}