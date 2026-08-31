using UnityEngine;

namespace MisterGames.Feedback {

    /// <summary>
    /// Settings of the <see cref="FeedbackService"/>: where entries are sent and how they are batched.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(FeedbackServiceConfig), menuName = "MisterGames/Feedback/" + nameof(FeedbackServiceConfig))]
    public sealed class FeedbackServiceConfig : ScriptableObject {

        [Header("Connection")]
        [Tooltip("Google Apps Script web app url, deployed with access for anyone, ends with /exec. " +
                 "Feedback is not sent while the url or the token is empty.")]
        [SerializeField] private string _webAppUrl;
        [Tooltip("Shared secret checked on the Apps Script side.")]
        [SerializeField] private string _token;
        [Tooltip("Request timeout in seconds. Apps Script needs time to start up on the first request, " +
                 "so a small timeout can drop batches that would be accepted otherwise.")]
        [SerializeField] [Min(1)] private int _requestTimeoutSec = 60;

        [Header("Batching")]
        [Tooltip("Period of checking the queue and the outbox, in seconds.")]
        [SerializeField] [Min(0.1f)] private float _tickPeriodSec = 5f;
        [Tooltip("Queue is written into an outbox batch file at least this often, in seconds.")]
        [SerializeField] [Min(0.1f)] private float _flushPeriodSec = 30f;
        [Tooltip("Max entries in one batch. Queue is written into the outbox at once when it holds that many entries. " +
                 "Must not exceed the row limit of the receiving Apps Script, which cuts the rest of the batch silently.")]
        [SerializeField] [Min(1)] private int _maxEntriesInBatch = 50;
        [Tooltip("Max entries waiting in the queue. The oldest entries are dropped on overflow.")]
        [SerializeField] [Min(1)] private int _maxEntriesInQueue = 1000;
        [Tooltip("Entry message is truncated to this length.")]
        [SerializeField] [Min(1)] private int _maxMessageLength = 4000;

        [Header("Retry")]
        [Tooltip("Delay before the first retry of a failed batch, in seconds. It is doubled on each failure.")]
        [SerializeField] [Min(0f)] private float _retryPeriodMinSec = 10f;
        [Tooltip("Max delay between retries of a failed batch, in seconds.")]
        [SerializeField] [Min(0f)] private float _retryPeriodMaxSec = 300f;
        [Tooltip("Max batch files kept in the outbox. The oldest batches are removed on overflow.")]
        [SerializeField] [Min(1)] private int _maxOutboxFiles = 64;

        [Header("Unity logs")]
        [Tooltip("Forward Unity logs into the feedback. The handler is called from any thread and only puts " +
                 "an entry into the queue, so no work is done on the thread that printed the log.")]
        [SerializeField] private bool _forwardUnityLogs = true;
        [Tooltip("Forward Debug.LogError and LogErrorFormat.")]
        [SerializeField] private bool _forwardErrors = true;
        [Tooltip("Forward failed assertions.")]
        [SerializeField] private bool _forwardAsserts = true;
        [Tooltip("Forward unhandled exceptions.")]
        [SerializeField] private bool _forwardExceptions = true;
        [Tooltip("Amount of the same message in a row that is forwarded: an error printed every frame " +
                 "would fill the whole queue otherwise. One entry reports that the rest is skipped.")]
        [SerializeField] [Min(1)] private int _maxSameMessagesInRow = 5;

        [Header("Debug")]
        [Tooltip("Write logs of the feedback service itself into the console.")]
        [SerializeField] private bool _enableLogs = true;
        [Tooltip("Send feedback while playing in the editor.")]
        [SerializeField] private bool _enableInEditor = true;

        public string WebAppUrl => _webAppUrl;
        public string Token => _token;
        public int RequestTimeoutSec => _requestTimeoutSec;

        public float TickPeriodSec => _tickPeriodSec;
        public float FlushPeriodSec => _flushPeriodSec;
        public int MaxEntriesInBatch => _maxEntriesInBatch;
        public int MaxEntriesInQueue => _maxEntriesInQueue;
        public int MaxMessageLength => _maxMessageLength;

        public float RetryPeriodMinSec => _retryPeriodMinSec;
        public float RetryPeriodMaxSec => _retryPeriodMaxSec;
        public int MaxOutboxFiles => _maxOutboxFiles;

        public bool ForwardUnityLogs => _forwardUnityLogs;
        public bool ForwardErrors => _forwardErrors;
        public bool ForwardAsserts => _forwardAsserts;
        public bool ForwardExceptions => _forwardExceptions;
        public int MaxSameMessagesInRow => _maxSameMessagesInRow;

        public bool EnableLogs => _enableLogs;
        public bool EnableInEditor => _enableInEditor;

#if UNITY_EDITOR
        // Settings of the feedback log analyzer window. They are editor only on purpose: the credentials
        // are a service account key, and a field that does not exist in a build can not drag it into one.

        [Header("Analyzer (editor only)")]
        [Tooltip("Service account json with read access to the feedback spreadsheet, " +
                 "the same asset the GoogleSheetImporter uses. Never included into a build.")]
        [SerializeField] private UnityEngine.TextAsset _analyzerCredentials;
        [Tooltip("Id of the feedback spreadsheet, the part of its url between /d/ and /edit.")]
        [SerializeField] private string _analyzerSpreadsheetId;
        [Tooltip("Name of the tab the web app writes rows into.")]
        [SerializeField] private string _analyzerSheetName = "Logs";

        public UnityEngine.TextAsset AnalyzerCredentials => _analyzerCredentials;
        public string AnalyzerSpreadsheetId => _analyzerSpreadsheetId;
        public string AnalyzerSheetName => _analyzerSheetName;
#endif
    }

}
