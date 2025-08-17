using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Lists;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {

    [CreateAssetMenu(fileName = nameof(GoogleSheetImporter), menuName = "MisterGames/Google/" + nameof(GoogleSheetImporter))]
    public sealed class GoogleSheetImporter : ScriptableObject {

        [Header("Download Settings")]
        [SerializeField] private TextAsset _credentials;
        [SerializeField] private bool _cancelOnAnyError = true;
        
        public async UniTask DownloadAndParse(IReadOnlyList<string> sheetIds, IGoogleSheetParser parser, CancellationToken cancellationToken) {
            if (sheetIds is not { Count: > 0 }) {
                LogInfo($"nothing to download.");    
                return;
            }
            
            var loader = new GoogleSheetLoader(_credentials.text);
            
            LogInfo($"starting to download {sheetIds.Count} sheets...");
            
            var metaResults = await DownloadMeta(loader, sheetIds);
            var metaDownloadStatus = PrintMetaDownloadResults(sheetIds, metaResults);

            if (cancellationToken.IsCancellationRequested) {
                LogWarning("aborted downloading sheets due to cancellation.");
                return;
            }
            
            if (_cancelOnAnyError && metaDownloadStatus == GoogleSheetLoader.Status.Error) {
                LogWarning("aborted downloading sheets meta due to errors.");
                return;
            }

            var tableResults = await DownloadAllTables(loader, sheetIds, metaResults);
            var tablesDownloadStatus = PrintTablesDownloadResults(sheetIds, metaResults, tableResults);
            
            if (cancellationToken.IsCancellationRequested) {
                LogWarning("aborted downloading tables due to cancellation.");
                return;
            }
            
            if (_cancelOnAnyError && tablesDownloadStatus == GoogleSheetLoader.Status.Error) {
                LogWarning("aborted downloading tables due to errors.");
                return;
            }

            parser.Parse(ConvertTableResults(tableResults));
        }

        private static IReadOnlyList<SheetTable> ConvertTableResults(GoogleSheetLoader.Result<SheetTable>[][] tableResults) {
            var tables = new List<SheetTable>();

            for (int i = 0; i < tableResults?.Length; i++) {
                var results = tableResults[i];
                
                for (int j = 0; j < results?.Length; j++) {
                    var result = results[j];

                    switch (result.status) {
                        case GoogleSheetLoader.Status.Error:
                            break;

                        case GoogleSheetLoader.Status.Success:
                            tables.Add(result.data);
                            break;
                    }
                }
            }

            return tables;
        }

        private static async UniTask<GoogleSheetLoader.Result<SheetMeta>[]> DownloadMeta(
            GoogleSheetLoader loader, 
            IReadOnlyList<string> sheetIds) 
        {
            if (sheetIds is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<SheetMeta>>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<SheetMeta>>>.Shared.Rent(sheetIds.Count);
            
            for (int i = 0; i < sheetIds.Count; i++) {
                string id = sheetIds[i];
                if (string.IsNullOrWhiteSpace(id)) continue;

                tasks[i] = loader.DownloadMeta(id);
            }

            var results = await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask<GoogleSheetLoader.Result<SheetMeta>>>.Shared.Return(tasks);
            
            return results;
        }
        
        private static async UniTask<GoogleSheetLoader.Result<SheetTable>[][]> DownloadAllTables(
            GoogleSheetLoader loader, 
            IReadOnlyList<string> sheetIds, 
            GoogleSheetLoader.Result<SheetMeta>[] metaResults) 
        {
            if (sheetIds is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<SheetTable>[]>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>[]>>.Shared.Rent(sheetIds.Count);
            
            for (int i = 0; i < sheetIds.Count; i++) {
                var result = metaResults[i];
                string sheetId = sheetIds[i];
                
                switch (result.status) {
                    case GoogleSheetLoader.Status.Success:
                        tasks[i] = DownloadTables(loader, sheetId, result.data.sheetTitle, result.data.tables);
                        break;

                    case GoogleSheetLoader.Status.Error:
                        break;
                }
            }

            var results = await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>[]>>.Shared.Return(tasks);
            
            return results;
        }
        
        private static async UniTask<GoogleSheetLoader.Result<SheetTable>[]> DownloadTables(
            GoogleSheetLoader loader, 
            string sheetId,
            string sheetTitle,
            IReadOnlyList<string> tables) 
        {
            if (tables is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<SheetTable>>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>>>.Shared.Rent(tables.Count);
            
            for (int i = 0; i < tables.Count; i++) {
                string title = tables[i];
                if (string.IsNullOrWhiteSpace(title)) continue;

                tasks[i] = loader.DownloadTable(sheetId, sheetTitle, title);
            }

            var results = await UniTask.WhenAll(tasks);
            
            tasks.ResetArrayElements();
            ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>>>.Shared.Return(tasks, clearArray: true);
            
            return results;
        }

        private static GoogleSheetLoader.Status PrintMetaDownloadResults(
            IReadOnlyList<string> sheetIds,
            GoogleSheetLoader.Result<SheetMeta>[] results) 
        {
            var sbSuccess = new StringBuilder("sheets meta downloaded successfully: ");
            var sbError = new StringBuilder("error downloading sheets meta: ");
            
            int successCount = 0;
            int errorsCount = 0;
            
            for (int i = 0; i < sheetIds.Count; i++) {
                var result = results[i];

                switch (result.status) {
                    case GoogleSheetLoader.Status.Success:
                        successCount++;
                        sbSuccess.AppendLine($"[{i}] Sheet {result.data.sheetTitle}: {result.data.tables.AsString()}");
                        break;

                    case GoogleSheetLoader.Status.Error:
                        errorsCount++;
                        sbError.AppendLine($"[{i}] Sheet {sheetIds[i]}: {result.message}");
                        break;
                }
            }
            
            if (successCount > 0) LogInfo(sbSuccess.ToString());
            if (errorsCount > 0) LogWarning(sbError.ToString());

            if (successCount <= 0 && errorsCount <= 0) {
                LogInfo("no sheets to download");
            }
            
            return errorsCount > 0 ? GoogleSheetLoader.Status.Error : GoogleSheetLoader.Status.Success;
        }
        
        private static GoogleSheetLoader.Status PrintTablesDownloadResults(
            IReadOnlyList<string> sheetIds, 
            GoogleSheetLoader.Result<SheetMeta>[] metaResults, 
            GoogleSheetLoader.Result<SheetTable>[][] tableResults) 
        {
            var sbSuccess = new StringBuilder("tables downloaded successfully: ");
            var sbError = new StringBuilder("error downloading tables: ");
            
            int successCount = 0;
            int errorsCount = 0;
            
            for (int i = 0; i < sheetIds.Count; i++) {
                var metaResult = metaResults[i];
                var tables = tableResults[i];

                for (int j = 0; j < metaResult.data.tables?.Count; j++) {
                    string title = metaResult.data.tables[j];
                    var tableResult = tables[j];
                    
                    switch (tableResult.status) {
                        case GoogleSheetLoader.Status.Success:
                            successCount++;
                            sbSuccess.AppendLine($"[{i}] Sheet {metaResult.data.sheetTitle}, [{j}] table {title}");
                            break;

                        case GoogleSheetLoader.Status.Error:
                            errorsCount++;
                            sbError.AppendLine($"[{i}] Sheet {metaResult.data.sheetTitle}, [{j}] table {title}: {tableResult.message}");
                            break;
                    }
                }
            }
            
            if (successCount > 0) LogInfo(sbSuccess.ToString());
            if (errorsCount > 0) LogWarning(sbError.ToString());

            if (successCount <= 0 && errorsCount <= 0) {
                LogInfo("no tables to download");
            }
            
            return errorsCount > 0 ? GoogleSheetLoader.Status.Error : GoogleSheetLoader.Status.Success;
        }

        private static void LogInfo(string message) {
            Debug.Log($"{nameof(GoogleSheetImporter).FormatColorOnlyForEditor(Color.white)}: {message}");
        }
        
        private static void LogWarning(string message) {
            Debug.LogWarning($"{nameof(GoogleSheetImporter).FormatColorOnlyForEditor(Color.white)}: {message}");
        }
    }
    
}