using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Data;
using MisterGames.Scenario.Events;

namespace MisterGames.Scenario.Service {
    
    public sealed class ScenarioService : IScenarioService, IEventListener, IDisposable {
        
        private readonly struct EventResponse : IEquatable<EventResponse> {
            
            public readonly IScenarioConfig scenarioConfig;
            public readonly int eventId;
            public readonly IActorAction action;
            
            public EventResponse(IScenarioConfig scenarioConfig, int eventId, IActorAction action) {
                this.scenarioConfig = scenarioConfig;
                this.eventId = eventId;
                this.action = action;
            }
            
            public bool Equals(EventResponse other) => Equals(scenarioConfig, other.scenarioConfig) && eventId == other.eventId;
            public override bool Equals(object obj) => obj is EventResponse other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(scenarioConfig, eventId);
            public static bool operator ==(EventResponse left, EventResponse right) => left.Equals(right);
            public static bool operator !=(EventResponse left, EventResponse right) => !left.Equals(right);
        }
        
        private readonly HashSet<IScenarioConfig> _addedScenarioConfigs = new();
        private readonly MultiValueDictionary<EventReference, EventResponse> _eventToResponseIdMap = new();
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
                _eventToResponseIdMap.GetValueAt(e, i).action?.Apply(null, _cts.Token).Forget();
            }
        }

        private void SubscribeToScenario(IScenarioConfig scenarioConfig) {
            for (int i = 0; i < scenarioConfig.ScenarioEvents.Count; i++) {
                var evt = scenarioConfig.ScenarioEvents[i];
                var response = new EventResponse(scenarioConfig, evt.startEvent.EventId, evt.startAction);
                
                _eventToResponseIdMap.AddValue(evt.startEvent, response);
                
                evt.startEvent.Subscribe(this);
            }
        }

        private void UnsubscribeFromScenario(IScenarioConfig scenarioConfig) {
            for (int i = 0; i < scenarioConfig.ScenarioEvents.Count; i++) {
                var evt = scenarioConfig.ScenarioEvents[i];
                var response = new EventResponse(scenarioConfig, evt.startEvent.EventId, evt.startAction);

                _eventToResponseIdMap.RemoveValue(evt.startEvent, response);
                
                evt.startEvent.Unsubscribe(this);
            }
        }
    }
    
}