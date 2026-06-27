using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Common.Strings;
using MisterGames.Scenario.Events;
using UnityEngine;

namespace MisterGames.Scenario.Service {
    
    public sealed class ScenarioService : IScenarioService, IEventListener, IDisposable {
        
        private const bool EnableLogs = true;
        
        private readonly HashSet<IScenarioConfig> _scenarioConfigsMap = new();
        private readonly List<ScenarioEvent> _scenarioEventsBuffer = new();
        private readonly MultiValueDictionary<EventReference, ScenarioEvent> _eventToResponseIdMap = new();
        private CancellationTokenSource _cts;

        public void Initialize() {
            AsyncExt.RecreateCts(ref _cts);
        }
        
        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            _scenarioEventsBuffer.Clear();
            foreach (var scenarioConfig in _scenarioConfigsMap) {
                scenarioConfig.CollectAllScenarioEvents(_scenarioEventsBuffer);
            }
            
            UnsubscribeFromScenario(_scenarioEventsBuffer);
            
            _scenarioConfigsMap.Clear();
            _eventToResponseIdMap.Clear();
        }

        public void AddScenario(IScenarioConfig scenarioConfig) {
            if (!_scenarioConfigsMap.Add(scenarioConfig)) return;
            
            if (EnableLogs) Log($"Add scenario config [{scenarioConfig}]");
            
            _scenarioEventsBuffer.Clear();
            scenarioConfig.CollectAllScenarioEvents(_scenarioEventsBuffer);
            SubscribeToScenarioEvents(_scenarioEventsBuffer);
        }

        public void RemoveScenario(IScenarioConfig scenarioConfig) {
            if (!_scenarioConfigsMap.Remove(scenarioConfig)) return;
            
            if (EnableLogs) Log($"Remove scenario config [{scenarioConfig}]");
            
            _scenarioEventsBuffer.Clear();
            scenarioConfig.CollectAllScenarioEvents(_scenarioEventsBuffer);
            UnsubscribeFromScenario(_scenarioEventsBuffer);
        }

        void IEventListener.OnEventRaised(EventReference e) {
            int count = _eventToResponseIdMap.GetCount(e);
            for (int i = 0; i < count; i++) {
                var evt = _eventToResponseIdMap.GetValueAt(e, i);
                if (CanStartScenarioEvent(evt, e)) StartScenarioEvent(evt);
            }
        }

        private void SubscribeToScenarioEvents(IReadOnlyList<ScenarioEvent> scenarioEvents) {
            for (int i = 0; i < scenarioEvents.Count; i++) {
                var evt = scenarioEvents[i];
                _eventToResponseIdMap.AddValue(evt.startEvent, evt);
                evt.startEvent.Subscribe(this);
            }
        }

        private void UnsubscribeFromScenario(IReadOnlyList<ScenarioEvent> scenarioEvents) {
            for (int i = 0; i < scenarioEvents.Count; i++) {
                var evt = scenarioEvents[i];
                _eventToResponseIdMap.RemoveValue(evt.startEvent, evt);
                evt.startEvent.Unsubscribe(this);
            }
        }

        private void StartScenarioEvent(ScenarioEvent scenarioEvent) {
            if (EnableLogs) Log($"Starting scenario event [{scenarioEvent}]");
            
            scenarioEvent.startAction.Apply(context: null, _cts.Token).Forget();
        }

        private static bool CanStartScenarioEvent(ScenarioEvent scenarioEvent, EventReference e) {
            return scenarioEvent is { startAction: not null } && 
                   scenarioEvent.startEvent.EventDomain == e.EventDomain && 
                   scenarioEvent.startEvent.EventId == e.EventId && 
                   (scenarioEvent.subIdMode == ScenarioEvent.Mode.IgnoreSubId || scenarioEvent.startEvent.SubId == e.SubId) && 
                   (!scenarioEvent.expectedRaiseCount.HasValue || scenarioEvent.expectedRaiseCount.Value.IsMatch(e.GetCount())) && 
                   (scenarioEvent.condition == null || scenarioEvent.condition.IsMatch(null));
                   
        }
        
        [HideInCallstack]
        private static void Log(string message) {
            Debug.Log($"{nameof(ScenarioService).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
    }
    
}