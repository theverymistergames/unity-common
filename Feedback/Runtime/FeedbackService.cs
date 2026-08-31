using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Files;
using MisterGames.Common.Service;
using MisterGames.Common.Strings;
using UnityEngine;
using UnityEngine.Networking;

namespace MisterGames.Feedback {

    /// <summary>
    /// Collects feedback entries and sends them in batches into a Google Apps Script web app,
    /// which writes them into Google Sheets. Google API is never accessed directly,
    /// so no OAuth or service account key is shipped with the build.
    ///
    /// <see cref="AppendLog"/> can be called from any thread and only puts an entry into a queue.
    /// Queue is flushed into batch files inside Application.persistentDataPath/outbox, and a batch file
    /// is deleted only after the web app answered with { "ok": true }, so entries survive
    /// a crash, a process kill or a connection loss and are sent on the next launch.
    ///
    /// Request body is raw json, sent as text/plain to stay a CORS simple request:
    /// Apps Script does not answer CORS preflight, which matters for WebGL builds.
    ///
    /// {
    ///   "token": "...",                              // shared secret, must match the one in Apps Script
    ///   "batchId": "...", "sessionId": "...",        // batchId is unique, can be used for deduplication
    ///   "app": "...", "version": "...",
    ///   "platform": "...", "device": "...",
    ///   "createdAt": "2026-08-31T12:00:00.000Z",
    ///   "dropped": 0,                                // entries lost on queue overflow since the previous batch
    ///   "entries": [ { "time": "2026-08-31T12:00:00.000Z", "message": "..." } ]
    /// }
    ///
    /// Expected answer: { "ok": true } or { "ok": false, "error": "..." }.
    /// </summary>
    public sealed class FeedbackService : IFeedbackService, IDisposable {

        private const string OutboxFolder = "outbox";
        private const string BatchFileExtension = ".json";
        private const string BatchFileTimeFormat = "yyyyMMdd_HHmmss_fff";
        private const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string ContentType = "text/plain;charset=utf-8";
        private const int FileBufferSize = 4096;

        private readonly ConcurrentQueue<Entry> _entryQueue = new();
        private readonly object _fileLock = new();
        private readonly string _logPrefix = nameof(FeedbackService).FormatColorOnlyForEditor(Color.white);

        private FeedbackServiceConfig _config;
        private CancellationTokenSource _cts;
        private string _outboxPath;
        private string _sessionId;
        private string _app;
        private string _version;
        private string _platform;
        private string _device;
        private DateTime _lastFlushTimeUtc;
        private DateTime _nextSendTimeUtc;
        private float _retryPeriodSec;
        private int _droppedCount;
        private bool _isOutboxReady;
        private bool _isEnabled;
        private volatile bool _isLoggingInternal;

        // Copies of the settings used by AppendLog, which can be called from any thread.
        private int _maxEntriesInQueue;
        private int _maxMessageLength;

        private bool EnableLogs => _config == null || _config.EnableLogs;

        [Serializable]
        private sealed class Batch {
            public string token;
            public string batchId;
            public string sessionId;
            public string app;
            public string version;
            public string platform;
            public string device;
            public string createdAt;
            public int dropped;
            public Entry[] entries;
        }

        [Serializable]
        private sealed class Entry {
            public string time;
            public string message;
        }

        [Serializable]
        private sealed class Response {
            public bool ok;
            public string error;
        }

        /// <summary>
        /// Appends an entry into the send queue of the registered feedback service, if there is one.
        /// Can be called from any thread.
        /// </summary>
        public static void Log(string message) {
            Services.Get<IFeedbackService>()?.AppendLog(message);
        }

        public void Initialize(FeedbackServiceConfig config) {
            _config = config;

            if (config == null) {
                LogWarning("config is not set, feedback is not sent.");
                return;
            }

            if (Application.isEditor && !config.EnableInEditor) return;

            if (string.IsNullOrWhiteSpace(config.WebAppUrl) || string.IsNullOrWhiteSpace(config.Token)) {
                LogWarning($"web app url or token is not set in config [{config.name}], feedback is not sent.");
                return;
            }

            _isEnabled = true;
            _maxEntriesInQueue = config.MaxEntriesInQueue;
            _maxMessageLength = config.MaxMessageLength;

            _outboxPath = Path.Combine(Application.persistentDataPath, OutboxFolder);
            _sessionId = Guid.NewGuid().ToString("N");
            _app = Application.productName;
            _version = Application.version;
            _platform = Application.platform.ToString();
            _device = $"{SystemInfo.deviceModel} | {SystemInfo.operatingSystem} | {SystemInfo.processorType} | {SystemInfo.graphicsDeviceName}";

            _lastFlushTimeUtc = DateTime.UtcNow;
            _nextSendTimeUtc = DateTime.UtcNow;
            _retryPeriodSec = config.RetryPeriodMinSec;
            _isOutboxReady = false;
            _droppedCount = 0;

            AsyncExt.RecreateCts(ref _cts);

            Run(_cts.Token).Forget();
        }

        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);

            if (!_isEnabled) return;

            _isEnabled = false;

            // Entries left in the queue must reach the outbox to be sent on the next launch.
            WriteQueueIntoOutboxSync();
        }

        public void AppendLog(string message) {
            if (!_isEnabled || _isLoggingInternal || string.IsNullOrEmpty(message)) return;

            if (message.Length > _maxMessageLength) message = message[.._maxMessageLength];

            _entryQueue.Enqueue(new Entry {
                time = DateTime.UtcNow.ToString(TimeFormat, CultureInfo.InvariantCulture),
                message = message,
            });

            // Sending can be slower than logging: drop the oldest entries to keep memory bounded.
            while (_entryQueue.Count > _maxEntriesInQueue && _entryQueue.TryDequeue(out _)) {
                Interlocked.Increment(ref _droppedCount);
            }
        }

        private async UniTaskVoid Run(CancellationToken cancellationToken) {
            // Batches left from the previous session: crash, process kill or failed requests.
            await SendOutbox(cancellationToken);

            while (!cancellationToken.IsCancellationRequested) {
                await UniTask
                    .Delay(TimeSpan.FromSeconds(_config.TickPeriodSec), DelayType.Realtime, cancellationToken: cancellationToken)
                    .SuppressCancellationThrow();

                if (cancellationToken.IsCancellationRequested) break;

                var now = DateTime.UtcNow;
                int count = _entryQueue.Count;

                if (count >= _config.MaxEntriesInBatch ||
                    count > 0 && (now - _lastFlushTimeUtc).TotalSeconds >= _config.FlushPeriodSec)
                {
                    await WriteQueueIntoOutbox(cancellationToken);
                }

                if (DateTime.UtcNow >= _nextSendTimeUtc) await SendOutbox(cancellationToken);
            }
        }

        private async UniTask WriteQueueIntoOutbox(CancellationToken cancellationToken) {
            _lastFlushTimeUtc = DateTime.UtcNow;

            if (!TryEnsureOutboxFolder()) return;

            int maxEntriesInBatch = _config.MaxEntriesInBatch;
            var entries = new List<Entry>(maxEntriesInBatch);

            while (!cancellationToken.IsCancellationRequested && !_entryQueue.IsEmpty) {
                entries.Clear();

                while (entries.Count < maxEntriesInBatch && _entryQueue.TryDequeue(out var entry)) {
                    entries.Add(entry);
                }

                if (entries.Count == 0) break;

                var batch = CreateBatch(entries.ToArray());
                string path = GetBatchFilePath(batch);

                var result = await JsonExtensions.WriteJsonIntoFile(batch, path, FileBufferSize, _fileLock);

                if (result.status != JsonExtensions.Status.Success) {
                    Interlocked.Add(ref _droppedCount, batch.entries.Length);
                    LogWarning($"can not write batch into file {path}: {result.message}.");
                }
            }
        }

        private void WriteQueueIntoOutboxSync() {
            if (_entryQueue.IsEmpty || !TryEnsureOutboxFolder()) return;

            int maxEntriesInBatch = _config.MaxEntriesInBatch;
            var entries = new List<Entry>(maxEntriesInBatch);

            while (!_entryQueue.IsEmpty) {
                entries.Clear();

                while (entries.Count < maxEntriesInBatch && _entryQueue.TryDequeue(out var entry)) {
                    entries.Add(entry);
                }

                if (entries.Count == 0) break;

                var batch = CreateBatch(entries.ToArray());
                string path = GetBatchFilePath(batch);
                string json = JsonExtensions.SerializeJson(batch);

                try {
                    lock (_fileLock) {
                        File.WriteAllText(path, json, Encoding.UTF8);
                    }
                }
                catch (Exception e) {
                    LogWarning($"can not write batch into file {path} on dispose: {e.Message}.");
                }
            }
        }

        private async UniTask SendOutbox(CancellationToken cancellationToken) {
            string[] files = GetOutboxFiles();

            for (int i = 0; i < files.Length && !cancellationToken.IsCancellationRequested; i++) {
                string path = files[i];
                var readResult = await JsonExtensions.ReadJsonFromFile<Batch>(path, FileBufferSize, _fileLock);

                if (readResult.status != JsonExtensions.Status.Success || readResult.value?.entries == null) {
                    LogWarning($"removing invalid batch file {path}: {readResult.message}.");
                    JsonExtensions.DeleteFile(path, _fileLock);
                    continue;
                }

                var batch = readResult.value;

                // Token is applied on send and is never stored on disk.
                batch.token = _config.Token;

                if (!await SendBatch(batch, cancellationToken)) {
                    // Keep the file and retry later, batches are sent in order of creation.
                    _retryPeriodSec = Mathf.Min(_retryPeriodSec * 2f, _config.RetryPeriodMaxSec);
                    _nextSendTimeUtc = DateTime.UtcNow.AddSeconds(_retryPeriodSec);
                    return;
                }

                _retryPeriodSec = _config.RetryPeriodMinSec;
                _nextSendTimeUtc = DateTime.UtcNow;

                JsonExtensions.DeleteFile(path, _fileLock);
            }
        }

        private async UniTask<bool> SendBatch(Batch batch, CancellationToken cancellationToken) {
            byte[] body = Encoding.UTF8.GetBytes(JsonExtensions.SerializeJson(batch));

            using var request = new UnityWebRequest(_config.WebAppUrl, UnityWebRequest.kHttpVerbPOST);

            request.uploadHandler = new UploadHandlerRaw(body) { contentType = ContentType };
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = _config.RequestTimeoutSec;

            try {
                await request.SendWebRequest().WithCancellation(cancellationToken);
            }
            catch (OperationCanceledException) {
                return false;
            }
            catch (Exception e) {
                LogWarning($"batch {batch.batchId} is not sent: {e.Message}.");
                return false;
            }

            if (request.result != UnityWebRequest.Result.Success) {
                LogWarning($"batch {batch.batchId} is not sent: {request.result}, {request.error}.");
                return false;
            }

            string text = request.downloadHandler?.text;

            if (!IsOkResponse(text)) {
                LogWarning($"batch {batch.batchId} is not accepted, answer: {text}.");
                return false;
            }

            LogInfo($"batch {batch.batchId} with {batch.entries.Length} entries is sent.");
            return true;
        }

        private static bool IsOkResponse(string text) {
            if (string.IsNullOrEmpty(text)) return false;

            try {
                // Apps Script answers with html on internal errors, such answer is not a success.
                return JsonUtility.FromJson<Response>(text) is { ok: true };
            }
            catch (Exception) {
                return false;
            }
        }

        private Batch CreateBatch(Entry[] entries) {
            return new Batch {
                batchId = Guid.NewGuid().ToString("N"),
                sessionId = _sessionId,
                app = _app,
                version = _version,
                platform = _platform,
                device = _device,
                createdAt = DateTime.UtcNow.ToString(TimeFormat, CultureInfo.InvariantCulture),
                dropped = Interlocked.Exchange(ref _droppedCount, 0),
                entries = entries,
            };
        }

        private string GetBatchFilePath(Batch batch) {
            string time = DateTime.UtcNow.ToString(BatchFileTimeFormat, CultureInfo.InvariantCulture);
            return Path.Combine(_outboxPath, $"{time}_{batch.batchId}{BatchFileExtension}");
        }

        private string[] GetOutboxFiles() {
            try {
                if (!Directory.Exists(_outboxPath)) return Array.Empty<string>();

                string[] files = Directory.GetFiles(_outboxPath, $"*{BatchFileExtension}");

                // File names start with an utc timestamp, so ordinal sort keeps batches in order of creation.
                Array.Sort(files, StringComparer.Ordinal);

                int maxOutboxFiles = _config.MaxOutboxFiles;
                if (files.Length <= maxOutboxFiles) return files;

                int excess = files.Length - maxOutboxFiles;
                for (int i = 0; i < excess; i++) {
                    JsonExtensions.DeleteFile(files[i], _fileLock);
                }

                LogWarning($"outbox contains more than {maxOutboxFiles} batches, {excess} oldest batches are removed.");

                string[] rest = new string[maxOutboxFiles];
                Array.Copy(files, excess, rest, 0, maxOutboxFiles);

                return rest;
            }
            catch (Exception e) {
                LogWarning($"can not read outbox folder {_outboxPath}: {e.Message}.");
                return Array.Empty<string>();
            }
        }

        private bool TryEnsureOutboxFolder() {
            if (_isOutboxReady) return true;

            try {
                lock (_fileLock) {
                    if (!Directory.Exists(_outboxPath)) Directory.CreateDirectory(_outboxPath);
                }

                _isOutboxReady = true;
            }
            catch (Exception e) {
                LogWarning($"can not create outbox folder {_outboxPath}: {e.Message}.");
            }

            return _isOutboxReady;
        }

        private void LogInfo(string message) {
            if (EnableLogs) {
                // Own logs must not be appended back by a log listener that calls AppendLog.
                _isLoggingInternal = true;
                Debug.Log($"{_logPrefix}: f {Time.frameCount}, {message}");
                _isLoggingInternal = false;
            }
        }

        private void LogWarning(string message) {
            if (EnableLogs) {
                _isLoggingInternal = true;
                Debug.LogWarning($"{_logPrefix}: f {Time.frameCount}, {message}");
                _isLoggingInternal = false;
            }
        }
    }

}
