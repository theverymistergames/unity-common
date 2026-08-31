using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
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
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

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

            // A pasted url and stray spaces are the usual way an id goes wrong, and Google answers
            // such an id with a plain "not found", so they are cleaned up before the request.
            spreadsheetId = NormalizeSpreadsheetId(spreadsheetId);
            sheetName = sheetName.Trim();

            IList<IList<object>> values;
            SheetsService service;

            try {
                service = new SheetsService(new BaseClientService.Initializer {
                    HttpClientInitializer = GoogleCredential
                        .FromJson(credentialsJson)
                        .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly),
                });
            }
            catch (Exception e) {
                return Result.Error($"credentials are not valid: {e.Message}");
            }

            try {
                var request = service.Spreadsheets.Values.Get(spreadsheetId, sheetName + Range);
                var response = await request.ExecuteAsync(cancellationToken);

                values = response?.Values;
            }
            catch (Exception e) {
                return Result.Error(await Explain(service, spreadsheetId, sheetName, credentialsJson, e));
            }

            await UniTask.SwitchToMainThread();

            if (cancellationToken.IsCancellationRequested) return Result.Error("download is cancelled");

            var entries = Parse(values);
            WriteCache(entries);

            return Result.Success(entries);
        }

        /// <summary>
        /// Turns the error of a failed request into something that says what to do about it:
        /// the spreadsheet is asked about itself, and whether that works tells the id and the access
        /// apart from a wrong sheet name.
        /// </summary>
        private static async UniTask<string> Explain(
            SheetsService service,
            string spreadsheetId,
            string sheetName,
            string credentialsJson,
            Exception error)
        {
            string account = GetServiceAccountEmail(credentialsJson);

            try {
                var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();

                // The spreadsheet is readable, so it is the sheet name that does not fit.
                var titles = new List<string>();

                for (int i = 0; i < spreadsheet?.Sheets?.Count; i++) {
                    string title = spreadsheet.Sheets[i]?.Properties?.Title;
                    if (!string.IsNullOrEmpty(title)) titles.Add(title);
                }

                return titles.Contains(sheetName)
                    ? error.Message
                    : $"sheet [{sheetName}] is not found in [{spreadsheet?.Properties?.Title}]. " +
                      $"Sheets of the spreadsheet: {string.Join(", ", titles)}";
            }
            catch (Exception) {
                return $"spreadsheet [{spreadsheetId}] is not available: {error.Message}. " +
                       $"Check that the id is complete, it is {spreadsheetId.Length} characters long, " +
                       $"and that the spreadsheet is shared with {account}.";
            }
        }

        /// <summary>
        /// Id of the spreadsheet from what is set in the config: an id as it is, or the part
        /// of a pasted url between /d/ and the next slash.
        /// </summary>
        private static string NormalizeSpreadsheetId(string value) {
            value = value.Trim();

            const string marker = "/d/";
            int start = value.IndexOf(marker, StringComparison.Ordinal);

            if (start < 0) return value;

            start += marker.Length;

            int end = value.IndexOfAny(new[] { '/', '?', '#' }, start);

            return end < 0 ? value[start..] : value[start..end];
        }

        private static string GetServiceAccountEmail(string credentialsJson) {
            var match = Regex.Match(credentialsJson, "\"client_email\"\\s*:\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "the service account";
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
                    string value = ConvertCell(row[c]);
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
        /// Text of a cell. The json of the answer is read into objects, and a value that looks like a date
        /// arrives already turned into a DateTime, whose default text is written in the culture of the machine.
        /// Such a value is written back as the invariant time the rest of the analyzer reads.
        /// </summary>
        private static string ConvertCell(object value) {
            return value switch {
                null => null,
                // Time without a kind is the utc the game wrote, not a local time of the reading machine.
                DateTime { Kind: DateTimeKind.Unspecified } time =>
                    time.ToString(TimeFormat, CultureInfo.InvariantCulture),
                DateTime time => time.ToUniversalTime().ToString(TimeFormat, CultureInfo.InvariantCulture),
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString(),
            };
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
