using System.Collections.Generic;
using MisterGames.Feedback;
using UnityEngine;

namespace MisterGames.Scenario.Events {

    [DefaultExecutionOrder(-100_000)]
    public sealed class EventBusFeedbackLogger : MonoBehaviour {

        [Tooltip("Domains to write. Empty means every domain.")]
        [SerializeField] private EventDomain[] _domains;
        [Tooltip("Write events raised without a domain, the ones a RaiseGlobal call sends.")]
        [SerializeField] private bool _writeGlobalEvents = true;
        [Tooltip("Time the same event is not written again for, in seconds. " +
                 "Zero writes every raise, which an event of a busy domain can turn into a flood.")]
        [SerializeField] [Min(0f)] private float _sameEventPeriodSec = 1f;
        [Tooltip("Data longer than this is cut.")]
        [SerializeField] [Min(0)] private int _maxDataLength = 200;

        private readonly Dictionary<EventReference, Entry> _entryMap = new();

        private struct Entry {
            public float lastTime;
            public int skipped;
        }

        private void OnEnable() {
            EventBus.Main.OnAnyEventRaised += OnEventRaised;
        }

        private void OnDisable() {
            EventBus.Main.OnAnyEventRaised -= OnEventRaised;
            _entryMap.Clear();
        }

        private void OnEventRaised(EventReference e) {
            if (!IsWritten(e)) return;

            float time = Time.realtimeSinceStartup;
            bool known = _entryMap.TryGetValue(e, out var entry);

            if (known && time - entry.lastTime < _sameEventPeriodSec) {
                entry.skipped++;
                _entryMap[e] = entry;
                return;
            }

            int skipped = known ? entry.skipped : 0;
            _entryMap[e] = new Entry { lastTime = time };

            FeedbackService.Log(GetMessage(e, skipped));
        }

        private string GetMessage(EventReference e, int skipped) {
            string name = e.GetName();
            string domain = e.EventDomain == null ? "global" : e.EventDomain.name;

            string message = $"Event {(string.IsNullOrEmpty(name) ? e.EventId.ToString() : name)} [{domain}]" +
                             $", count {EventBus.Main.GetCount(e)}";

            if (e.SubId != 0) message += $", sub {e.SubId}";
            if (skipped > 0) message += $", {skipped} raises skipped";

            return message;
        }

        private bool IsWritten(EventReference e) {
            if (e.EventDomain == null) return _writeGlobalEvents;
            if (_domains is not { Length: > 0 }) return true;

            for (int i = 0; i < _domains.Length; i++) {
                if (_domains[i] == e.EventDomain) return true;
            }

            return false;
        }
    }

}
