using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Strings;
using UnityEngine;

namespace MisterGames.Common.Editor.GoogleSheets {

    [CreateAssetMenu(fileName = nameof(GoogleSheetImporter), menuName = "MisterGames/Google/" + nameof(GoogleSheetImporter))]
    public sealed class GoogleSheetImporter : ScriptableObject {

        [Header("Download Settings")]
        [SerializeField] private TextAsset _credentials;
        [SerializeField] private bool _cancelOnAnyError = true;
        
        public async UniTask DownloadAndParse(IReadOnlyList<string> sheetIds, IGoogleSheetParser parser) {
            var loader = new GoogleSheetLoader(_credentials.text);
            
            var titleResults = await DownloadTitles(loader, sheetIds);
            var titlesDownloadStatus = PrintTitlesDownloadResults(sheetIds, titleResults);

            if (_cancelOnAnyError && titlesDownloadStatus == GoogleSheetLoader.Status.Error) {
                LogWarning("aborted downloading sheets due to errors.");
                return;
            }

            var tableResults = await DownloadAllTables(loader, sheetIds, titleResults);
            var tablesDownloadStatus = PrintTablesDownloadResults(sheetIds, titleResults, tableResults);
            
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

        private static async UniTask<GoogleSheetLoader.Result<IReadOnlyList<string>>[]> DownloadTitles(
            GoogleSheetLoader loader, 
            IReadOnlyList<string> sheetIds) 
        {
            if (sheetIds is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<IReadOnlyList<string>>>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<IReadOnlyList<string>>>>.Shared.Rent(sheetIds.Count);
            
            for (int i = 0; i < sheetIds.Count; i++) {
                string id = sheetIds[i];
                if (string.IsNullOrWhiteSpace(id)) continue;

                tasks[i] = loader.DownloadTitles(id);
            }

            var results = await UniTask.WhenAll(tasks);
            ArrayPool<UniTask<GoogleSheetLoader.Result<IReadOnlyList<string>>>>.Shared.Return(tasks, clearArray: true);
            
            return results;
        }
        
        private static async UniTask<GoogleSheetLoader.Result<SheetTable>[][]> DownloadAllTables(
            GoogleSheetLoader loader, 
            IReadOnlyList<string> sheetIds, 
            GoogleSheetLoader.Result<IReadOnlyList<string>>[] titleResults) 
        {
            if (sheetIds is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<SheetTable>[]>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>[]>>.Shared.Rent(sheetIds.Count);
            
            for (int i = 0; i < sheetIds.Count; i++) {
                var result = titleResults[i];
                string sheetId = sheetIds[i];
                
                switch (result.status) {
                    case GoogleSheetLoader.Status.Success:
                        tasks[i] = DownloadTables(loader, sheetId, result.data);
                        break;

                    case GoogleSheetLoader.Status.Error:
                        break;
                }
            }

            var results = await UniTask.WhenAll(tasks);
            ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>[]>>.Shared.Return(tasks, clearArray: true);
            
            return results;
        }
        
        private static async UniTask<GoogleSheetLoader.Result<SheetTable>[]> DownloadTables(
            GoogleSheetLoader loader, 
            string sheetId, 
            IReadOnlyList<string> titles) 
        {
            if (titles is not { Count: > 0 }) {
                return Array.Empty<GoogleSheetLoader.Result<SheetTable>>();
            }
            
            var tasks = ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>>>.Shared.Rent(titles.Count);
            
            for (int i = 0; i < titles.Count; i++) {
                string title = titles[i];
                if (string.IsNullOrWhiteSpace(title)) continue;

                tasks[i] = loader.DownloadTable(sheetId, title);
            }

            var results = await UniTask.WhenAll(tasks);
            ArrayPool<UniTask<GoogleSheetLoader.Result<SheetTable>>>.Shared.Return(tasks, clearArray: true);
            
            return results;
        }

        private static GoogleSheetLoader.Status PrintTitlesDownloadResults(
            IReadOnlyList<string> sheetIds,
            GoogleSheetLoader.Result<IReadOnlyList<string>>[] results) 
        {
            var sbSuccess = new StringBuilder("sheet titles downloaded successfully: ");
            var sbError = new StringBuilder("error downloading sheet titles: ");
            
            int successCount = 0;
            int errorsCount = 0;
            
            for (int i = 0; i < sheetIds.Count; i++) {
                var result = results[i];

                switch (result.status) {
                    case GoogleSheetLoader.Status.Success:
                        successCount++;
                        sbSuccess.AppendLine($"[{i}] Sheet {sheetIds[i]}: {result.data.AsString()}");
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
            GoogleSheetLoader.Result<IReadOnlyList<string>>[] titleResults, 
            GoogleSheetLoader.Result<SheetTable>[][] tableResults) 
        {
            var sbSuccess = new StringBuilder("tables downloaded successfully: ");
            var sbError = new StringBuilder("error downloading tables: ");
            
            int successCount = 0;
            int errorsCount = 0;
            
            for (int i = 0; i < sheetIds.Count; i++) {
                string sheetId = sheetIds[i];
                var titleResult = titleResults[i];
                var tables = tableResults[i];

                for (int j = 0; j < titleResult.data?.Count; j++) {
                    string title = titleResult.data[j];
                    var tableResult = tables[j];
                    
                    switch (tableResult.status) {
                        case GoogleSheetLoader.Status.Success:
                            successCount++;
                            sbSuccess.AppendLine($"[{i}] Sheet {sheetId}, [{j}] table {title}");
                            break;

                        case GoogleSheetLoader.Status.Error:
                            errorsCount++;
                            sbError.AppendLine($"[{i}] Sheet {sheetId}, [{j}] table {title}: {tableResult.message}");
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