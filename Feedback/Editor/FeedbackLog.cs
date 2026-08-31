using System;
using System.Collections.Generic;
using System.Globalization;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// One row of the feedback table.
    /// </summary>
    [Serializable]
    public sealed class FeedbackLogEntry {

        public string receivedAt;
        public string playerId;
        public string sessionId;
        public string build;
        public string platform;
        public string device;
        public string timeUtc;
        public string type;
        public string message;
        public string stack;

        [NonSerialized] public DateTime time;

        public bool IsError => type is "Error" or "Exception" or "Assert";

        /// <summary>
        /// Time of the entry: the time it was written on the device, or the time the row was added
        /// by the web app if the first one is not readable.
        /// </summary>
        public void ParseTime() {
            time = ParseTime(timeUtc) ?? ParseTime(receivedAt) ?? default;
        }

        private static DateTime? ParseTime(string value) {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return DateTime.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces, out var result)
                ? result
                : null;
        }
    }

    /// <summary>
    /// Entries of one launch of the game.
    /// </summary>
    public sealed class FeedbackLogSession {

        public string sessionId;
        public string build;
        public string platform;
        public DateTime startTime;
        public DateTime endTime;
        public int errorCount;
        public readonly List<FeedbackLogEntry> entries = new();

        public TimeSpan Duration => endTime - startTime;
    }

    /// <summary>
    /// Sessions of one install of the game, newest first.
    /// </summary>
    public sealed class FeedbackLogPlayer {

        public string playerId;
        public string device;
        public string build;
        public DateTime lastTime;
        public int entryCount;
        public int errorCount;
        public readonly List<FeedbackLogSession> sessions = new();

        public string ShortId => string.IsNullOrEmpty(playerId)
            ? "unknown"
            : playerId.Length <= 8 ? playerId : playerId[..8];
    }

}
