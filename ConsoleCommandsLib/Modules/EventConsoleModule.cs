using System;
using System.Collections.Generic;
using MisterGames.Dbg.Console.Attributes;
using MisterGames.Dbg.Console.Core;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.ConsoleCommandsLib.Modules {

    [Serializable]
    public sealed class EventConsoleModule : IConsoleModule, IConsoleSuggestionProvider {

        public ConsoleRunner ConsoleRunner { get; set; }

        private const string EventsCmd = "events";
        private const char Separator = '/';

        private EventConsoleModuleData _data;

        [ConsoleCommand(EventsCmd)]
        [ConsoleCommandHelp("raise event by name: <event>, <domain>/<event>, <group>/<event> or <domain>/<group>/<event>")]
        public void RaiseEvent(string eventPath) {
            if (!TryFindEvent(eventPath, out var e, out string fullPath)) return;

            e.Raise();
            ConsoleRunner.AppendLine($"Raised event {fullPath}, count {e.GetCount()}");
        }

        [ConsoleCommand(EventsCmd)]
        [ConsoleCommandHelp("set event count by name: <event>, <domain>/<event>, <group>/<event> or <domain>/<group>/<event>")]
        public void SetEventCount(string eventPath, int count) {
            if (!TryFindEvent(eventPath, out var e, out string fullPath)) return;

            e.SetCount(count);
            ConsoleRunner.AppendLine($"Set event {fullPath} count {e.GetCount()}");
        }

        [ConsoleCommand("events/list")]
        [ConsoleCommandHelp("print all events as <domain>/<group>/<event> with current count")]
        public void PrintEvents() {
            if (!ValidateData()) return;

            var domains = _data.EventDomains;
            ConsoleRunner.AppendLine("Events:");

            for (int i = 0; i < domains.Count; i++) {
                var domain = domains[i];
                if (domain == null) continue;

                var groups = GetEventGroups(domain);

                for (int j = 0; j < groups.Length; j++) {
                    var group = groups[j];
                    if (group.events == null) continue;

                    for (int k = 0; k < group.events.Length; k++) {
                        var entry = group.events[k];
                        int count = new EventReference(domain, entry.id).GetCount();

                        ConsoleRunner.AppendLine($" - {GetEventPath(domain.name, group.name, entry.name)} : {count}");
                    }
                }
            }
        }

        [ConsoleCommand("events/domains")]
        [ConsoleCommandHelp("print all event domains")]
        public void PrintDomains() {
            if (!ValidateData()) return;

            var domains = _data.EventDomains;
            ConsoleRunner.AppendLine("Event domains:");

            for (int i = 0; i < domains.Count; i++) {
                var domain = domains[i];
                if (domain == null) continue;

                ConsoleRunner.AppendLine($" - [{i}] {domain.name}");
            }
        }

        public void CollectSuggestions(string cmd, IReadOnlyList<string> args, ICollection<string> suggestions) {
            if (cmd != EventsCmd || args.Count != 1) return;

            if (_data == null) _data = EventConsoleModuleData.Load();
            if (_data == null) return;

            string query = args[0];
            var containsMatches = new List<string>();
            var domains = _data.EventDomains;

            for (int i = 0; i < domains.Count; i++) {
                var domain = domains[i];
                if (domain == null) continue;

                var groups = GetEventGroups(domain);

                for (int j = 0; j < groups.Length; j++) {
                    var group = groups[j];
                    if (group.events == null) continue;

                    for (int k = 0; k < group.events.Length; k++) {
                        var entry = group.events[k];
                        string fullPath = GetEventPath(domain.name, group.name, entry.name);

                        if (StartsWith(entry.name, query) ||
                            StartsWith($"{group.name}{Separator}{entry.name}", query) ||
                            StartsWith($"{domain.name}{Separator}{entry.name}", query) ||
                            StartsWith(fullPath, query))
                        {
                            suggestions.Add(fullPath);
                            continue;
                        }

                        if (fullPath.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            $"{domain.name}{Separator}{entry.name}".Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            containsMatches.Add(fullPath);
                        }
                    }
                }
            }

            for (int i = 0; i < containsMatches.Count; i++) {
                suggestions.Add(containsMatches[i]);
            }
        }

        private bool ValidateData() {
            if (_data == null) _data = EventConsoleModuleData.Load();
            if (_data != null) return true;

            ConsoleRunner.AppendLine($"{nameof(EventConsoleModule)}: {nameof(EventConsoleModuleData)} is not found in Resources/{EventConsoleModuleData.ResourcesPath}. " +
                                     "Use MisterGames/Tools/Collect EventDomains for debug console to create it.");
            return false;
        }

        private static EventGroupSnapshot[] GetEventGroups(EventDomain domain) {
            // EventDomain does not expose its groups publicly,
            // so the serialized data is read through json to get group and event names.
            return JsonUtility.FromJson<EventDomainSnapshot>(JsonUtility.ToJson(domain))._eventGroups
                   ?? Array.Empty<EventGroupSnapshot>();
        }

        private bool TryFindEvent(string eventPath, out EventReference e, out string fullPath) {
            e = default;
            fullPath = null;

            if (!ValidateData()) return false;

            string[] parts = eventPath.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length is 0 or > 3) {
                ConsoleRunner.AppendLine($"Invalid event path {eventPath}, expected <event>, <domain>/<event>, <group>/<event> or <domain>/<group>/<event>");
                return false;
            }

            string eventName = parts[^1];
            string domainName = parts.Length == 3 ? parts[0] : null;
            string groupName = parts.Length == 3 ? parts[1] : null;
            string domainOrGroupName = parts.Length == 2 ? parts[0] : null;

            var domains = _data.EventDomains;

            for (int i = 0; i < domains.Count; i++) {
                var domain = domains[i];
                if (domain == null) continue;
                if (domainName != null && !NameEquals(domain.name, domainName)) continue;

                var groups = GetEventGroups(domain);
                bool domainMatches = domainOrGroupName != null && NameEquals(domain.name, domainOrGroupName);

                for (int j = 0; j < groups.Length; j++) {
                    var group = groups[j];
                    if (group.events == null) continue;

                    if (groupName != null && !NameEquals(group.name, groupName)) continue;
                    if (domainOrGroupName != null && !domainMatches && !NameEquals(group.name, domainOrGroupName)) continue;

                    for (int k = 0; k < group.events.Length; k++) {
                        var entry = group.events[k];
                        if (!NameEquals(entry.name, eventName)) continue;

                        e = new EventReference(domain, entry.id);
                        fullPath = GetEventPath(domain.name, group.name, entry.name);
                        return true;
                    }
                }
            }

            ConsoleRunner.AppendLine($"Event {eventPath} is not found");
            return false;
        }

        private static bool NameEquals(string a, string b) {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetEventPath(string domainName, string groupName, string eventName) {
            // Group name can be empty: use short path <domain>/<event>, it is also supported in search.
            return string.IsNullOrEmpty(groupName)
                ? $"{domainName}{Separator}{eventName}"
                : $"{domainName}{Separator}{groupName}{Separator}{eventName}";
        }

        private static bool StartsWith(string value, string prefix) {
            return value != null && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        [Serializable]
        private sealed class EventDomainSnapshot {
            public EventGroupSnapshot[] _eventGroups;
        }

        [Serializable]
        private struct EventGroupSnapshot {
            public string name;
            public EventEntrySnapshot[] events;
        }

        [Serializable]
        private struct EventEntrySnapshot {
            public string name;
            public int id;
        }
    }

}
