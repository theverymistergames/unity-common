using UnityEngine;

namespace MisterGames.Feedback.Perf {

    /// <summary>
    /// Settings of the <see cref="PerformanceLogService"/>.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PerformanceLogServiceConfig), menuName = "MisterGames/Feedback/" + nameof(PerformanceLogServiceConfig))]
    public sealed class PerformanceLogServiceConfig : ScriptableObject {

        [Header("Log")]
        [Tooltip("Period of writing the performance log, in seconds.")]
        [SerializeField] [Min(1f)] private float _logPeriodSec = 30f;
        [Tooltip("Write hardware and software info once on start.")]
        [SerializeField] private bool _logSystemInfoOnStart = true;
        [Tooltip("Send the same logs into the feedback service.")]
        [SerializeField] private bool _sendToFeedback = true;
        [Tooltip("Write logs into the console.")]
        [SerializeField] private bool _enableLogs = true;
        [Tooltip("Work while playing in the editor.")]
        [SerializeField] private bool _enableInEditor = true;

        [Header("Fps")]
        [Tooltip("Amount of the last frames the fps stats are calculated from. " +
                 "1% low is averaged over capacity/100 worst frames, 0.1% low over capacity/1000 worst frames.")]
        [SerializeField] [Min(10)] private int _fpsSamplesCapacity = 1024;
        [Tooltip("Reset collected fps samples after each log, so every log shows only the last period.")]
        [SerializeField] private bool _resetFpsSamplesOnLog;

        public float LogPeriodSec => _logPeriodSec;
        public bool LogSystemInfoOnStart => _logSystemInfoOnStart;
        public bool SendToFeedback => _sendToFeedback;
        public bool EnableLogs => _enableLogs;
        public bool EnableInEditor => _enableInEditor;

        public int FpsSamplesCapacity => _fpsSamplesCapacity;
        public bool ResetFpsSamplesOnLog => _resetFpsSamplesOnLog;
    }

}
