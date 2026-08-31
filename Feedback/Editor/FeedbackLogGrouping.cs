using System.Collections.Generic;

namespace MisterGames.Feedback.Editor {

    /// <summary>
    /// Turns the flat table into players with their sessions: players are sorted by the last activity,
    /// sessions of a player by their start time, both newest first, and entries inside a session
    /// keep the order they were written in.
    /// </summary>
    public static class FeedbackLogGrouping {

        private const string UnknownKey = "unknown";

        public static List<FeedbackLogPlayer> Build(IReadOnlyList<FeedbackLogEntry> entries) {
            var playerMap = new Dictionary<string, FeedbackLogPlayer>();
            var sessionMap = new Dictionary<(string player, string session), FeedbackLogSession>();
            var players = new List<FeedbackLogPlayer>();

            for (int i = 0; i < entries?.Count; i++) {
                var entry = entries[i];

                // Entries written before the player id was added to the payload are grouped by the device,
                // so old rows of the table are still readable.
                string playerKey = FirstNotEmpty(entry.playerId, entry.device, UnknownKey);
                string sessionKey = FirstNotEmpty(entry.sessionId, UnknownKey);

                if (!playerMap.TryGetValue(playerKey, out var player)) {
                    player = new FeedbackLogPlayer { playerId = playerKey };

                    playerMap[playerKey] = player;
                    players.Add(player);
                }

                if (!sessionMap.TryGetValue((playerKey, sessionKey), out var session)) {
                    session = new FeedbackLogSession {
                        sessionId = sessionKey,
                        startTime = entry.time,
                        endTime = entry.time,
                    };

                    sessionMap[(playerKey, sessionKey)] = session;
                    player.sessions.Add(session);
                }

                session.entries.Add(entry);
                session.build ??= NullIfEmpty(entry.build);
                session.platform ??= NullIfEmpty(entry.platform);

                if (entry.time < session.startTime) session.startTime = entry.time;
                if (entry.time > session.endTime) session.endTime = entry.time;

                player.device ??= NullIfEmpty(entry.device);
                player.build = NullIfEmpty(entry.build) ?? player.build;
                player.entryCount++;

                if (entry.time > player.lastTime) player.lastTime = entry.time;

                if (entry.IsError) {
                    session.errorCount++;
                    player.errorCount++;
                }
            }

            for (int i = 0; i < players.Count; i++) {
                var player = players[i];

                player.sessions.Sort((a, b) => b.startTime.CompareTo(a.startTime));

                for (int j = 0; j < player.sessions.Count; j++) {
                    player.sessions[j].entries.Sort((a, b) => a.time.CompareTo(b.time));
                }
            }

            players.Sort((a, b) => b.lastTime.CompareTo(a.lastTime));

            return players;
        }

        private static string FirstNotEmpty(params string[] values) {
            for (int i = 0; i < values.Length; i++) {
                if (!string.IsNullOrWhiteSpace(values[i])) return values[i];
            }

            return UnknownKey;
        }

        private static string NullIfEmpty(string value) {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }

}
