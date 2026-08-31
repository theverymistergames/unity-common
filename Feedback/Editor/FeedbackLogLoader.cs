using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using UnityEngine;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Downloads the feedback table with the same service account the other Google Sheets of the project
    /// are downloaded with, and keeps the last download in the Library folder, so the window has something
    /// to show without the network.
    /// </summary>
    public static class FeedbackLogLoader {

        public readonly struct Result {

            public readonly bool ok;
            public readonly FeedbackLogEntry[] entries;
            public readonly string message;

            public Result(bool ok, FeedbackLogEntry[] entries, string message) {
                this.ok = ok;
                this.entries = entries;
                this.message = message;
            }

            public static Result Success(FeedbackLogEntry[] entries) => new(true, entries, message: null);
            public static Result Error(string message) => new(false, Array.Empty<FeedbackLogEntry>(), message);
        }

        [Serializable]
        private sealed class Cache {
            public string downloadedAt;
            public FeedbackLogEntry[] entries;
        }

        private const string CacheFolder = "Library/MisterGames";
        private const string CacheFile = "FeedbackLogs.json";
        private const string Range = "!A1:Z100000";

        /// <summary>
        /// Columns of the table, in the order the Apps Script writes them.
        /// A header row is used when there is one, so a changed order does not break the parsing.
        /// </summary>
        private static readonly string[] ColumnNames = {
            "receivedat", "playerid", "sessionid", "build", "platform", "device", "timeutc", "type", "message", "stack",
        };

        /// <summary>
        /// Layout of the table without a header row and without the player and device columns.
        /// </summary>
        private static readonly string[] LegacyColumnNames = {
            "receivedat", "sessionid", "build", "platform", "timeutc", "type", "message", "stack",
        };

        public static async UniTask<Result> Download(
            string credentialsJson,
            string spreadsheetId,
            string sheetName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(credentialsJson)) return Result.Error("credentials are not set");
            if (string.IsNullOrWhiteSpace(spreadsheetId)) return Result.Error("spreadsheet id is not set");
            if (string.IsNullOrWhiteSpace(sheetName)) return Result.Error("sheet name is not set");

            IList<IList<object>> values;

            try {
                var service = new SheetsService(new BaseClientService.Initializer {
                    HttpClientInitializer = GoogleCredential
                        .FromJson(credentialsJson)
                        .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly),
                });

                var request = service.Spreadsheets.Values.Get(spreadsheetId, sheetName + Range);
                var response = await request.ExecuteAsync(cancellationToken);

                values = response?.Values;
            }
            catch (Exception e) {
                return Result.Error(e.Message);
            }

            await UniTask.SwitchToMainThread();

            if (cancellationToken.IsCancellationRequested) return Result.Error("download is cancelled");

            var entries = Parse(values);
            WriteCache(entries);

            return Result.Success(entries);
        }

        public static FeedbackLogEntry[] ReadCache(out string downloadedAt) {
            downloadedAt = null;

            try {
                string path = GetCachePath();
                if (!File.Exists(path)) return Array.Empty<FeedbackLogEntry>();

                var cache = JsonUtility.FromJson<Cache>(File.ReadAllText(path));
                if (cache?.entries == null) return Array.Empty<FeedbackLogEntry>();

                downloadedAt = cache.downloadedAt;

                for (int i = 0; i < cache.entries.Length; i++) {
                    cache.entries[i].ParseTime();
                }

                return cache.entries;
            }
            catch (Exception e) {
                Debug.LogWarning($"{nameof(FeedbackLogLoader)}: can not read cached logs: {e.Message}.");
                return Array.Empty<FeedbackLogEntry>();
            }
        }

        private static void WriteCache(FeedbackLogEntry[] entries) {
            try {
                if (!Directory.Exists(CacheFolder)) Directory.CreateDirectory(CacheFolder);

                var cache = new Cache {
                    downloadedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    entries = entries,
                };

                File.WriteAllText(GetCachePath(), JsonUtility.ToJson(cache));
            }
            catch (Exception e) {
                Debug.LogWarning($"{nameof(FeedbackLogLoader)}: can not write cached logs: {e.Message}.");
            }
        }

        private static string GetCachePath() {
            return Path.Combine(CacheFolder, CacheFile);
        }

        private static FeedbackLogEntry[] Parse(IList<IList<object>> values) {
            int rowCount = values?.Count ?? 0;
            if (rowCount == 0) return Array.Empty<FeedbackLogEntry>();

            int startRow = 0;
            var columns = GetHeaderColumns(values![0]);

            if (columns != null) startRow = 1;
            else columns = GetColumnsByCount(values[0].Count);

            var entries = new List<FeedbackLogEntry>(rowCount);

            for (int r = startRow; r < rowCount; r++) {
                var row = values[r];
                if (row == null || row.Count == 0) continue;

                var entry = new FeedbackLogEntry();
                bool isEmpty = true;

                for (int c = 0; c < row.Count && c < columns.Length; c++) {
                    string value = row[c]?.ToString();
                    if (string.IsNullOrEmpty(value)) continue;

                    isEmpty = false;
                    SetColumn(entry, columns[c], value);
                }

                if (isEmpty) continue;

                entry.ParseTime();
                entries.Add(entry);
            }

            return entries.ToArray();
        }

        /// <summary>
        /// Header row of the table, if the first row is one: a row is a header when it names
        /// at least the message column.
        /// </summary>
        private static string[] GetHeaderColumns(IList<object> row) {
            int count = row?.Count ?? 0;
            if (count == 0) return null;

            var columns = new string[count];
            bool hasMessage = false;

            for (int c = 0; c < count; c++) {
                string name = Normalize(row![c]?.ToString());

                columns[c] = name;
                hasMessage |= name == "message";
            }

            return hasMessage ? columns : null;
        }

        private static string[] GetColumnsByCount(int count) {
            return count <= LegacyColumnNames.Length ? LegacyColumnNames : ColumnNames;
        }

        private static void SetColumn(FeedbackLogEntry entry, string column, string value) {
            switch (column) {
                case "receivedat":
                case "date":
                case "time":
                    entry.receivedAt = value;
                    break;

                case "playerid":
                case "player":
                    entry.playerId = value;
                    break;

                case "sessionid":
                case "session":
                    entry.sessionId = value;
                    break;

                case "build":
                case "version":
                    entry.build = value;
                    break;

                case "platform":
                    entry.platform = value;
                    break;

                case "device":
                    entry.device = value;
                    break;

                case "timeutc":
                    entry.timeUtc = value;
                    break;

                case "type":
                    entry.type = value;
                    break;

                case "message":
                    entry.message = value;
                    break;

                case "stack":
                case "stacktrace":
                    entry.stack = value;
                    break;
            }
        }

        private static string Normalize(string value) {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant().Replace(" ", string.Empty).Replace("_", string.Empty);
        }
    }

}
