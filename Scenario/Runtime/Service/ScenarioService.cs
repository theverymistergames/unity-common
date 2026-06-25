using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;

namespace MisterGames.Scenario.Service {
    
    public sealed class ScenarioService : IScenarioService, IEventListener, IDisposable {
        
        private readonly HashSet<IScenarioConfig> _addedScenarioConfigs = new();
        private readonly MultiValueDictionary<EventReference, ScenarioEvent> _eventToResponseIdMap = new();
        private CancellationTokenSource _cts;

        public void Initialize() {
            AsyncExt.RecreateCts(ref _cts);
        }
        
        public void Dispose() {
            AsyncExt.DisposeCts(ref _cts);
            
            foreach (var scenarioConfig in _addedScenarioConfigs) {
                UnsubscribeFromScenario(scenarioConfig);
            }
            
            _addedScenarioConfigs.Clear();
            _eventToResponseIdMap.Clear();
        }

        public void AddScenario(IScenarioConfig scenarioConfig) {
            if (!_addedScenarioConfigs.Add(scenarioConfig)) return;
            
            SubscribeToScenario(scenarioConfig);
        }

        public void RemoveScenario(IScenarioConfig scenarioConfig) {
            if (!_addedScenarioConfigs.Remove(scenarioConfig)) return;
            
            UnsubscribeFromScenario(scenarioConfig);
        }

        void IEventListener.OnEventRaised(EventReference e) {
            int count = _eventToResponseIdMap.GetCount(e);
            for (int i = 0; i < count; i++) {
                var evt = _eventToResponseIdMap.GetValueAt(e, i);
                if (!CanStartScenarioEvent(evt, e)) continue;

                evt.startAction.Apply(context: null, _cts.Token).Forget();
            }
        }

        private void SubscribeToScenario(IScenarioConfig scenarioConfig) {
            for (int i = 0; i < scenarioConfig.ScenarioEvents.Count; i++) {
                var evt = scenarioConfig.ScenarioEvents[i];
                _eventToResponseIdMap.AddValue(evt.startEvent, evt);
                
                evt.startEvent.Subscribe(this);
            }
        }

        private void UnsubscribeFromScenario(IScenarioConfig scenarioConfig) {
            for (int i = 0; i < scenarioConfig.ScenarioEvents.Count; i++) {
                var evt = scenarioConfig.ScenarioEvents[i];
                _eventToResponseIdMap.RemoveValue(evt.startEvent, evt);
                
                evt.startEvent.Unsubscribe(this);
            }
        }

        private static bool CanStartScenarioEvent(ScenarioEvent scenarioEvent, EventReference e) {
            return scenarioEvent is { startAction: not null } && 
                   scenarioEvent.startEvent.EventDomain == e.EventDomain && scenarioEvent.startEvent.EventId == e.EventId && 
                   (scenarioEvent.subIdMode == ScenarioEvent.Mode.IgnoreSubId || scenarioEvent.startEvent.SubId == e.SubId) && 
                   (!scenarioEvent.expectedRaiseCount.HasValue || scenarioEvent.expectedRaiseCount.Value.IsMatch(e.GetCount()));
                   
        }
    }
    
}