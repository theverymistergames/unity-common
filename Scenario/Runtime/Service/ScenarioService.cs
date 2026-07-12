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
        private readonly Dictionary<ScenarioEvent, int> _runActionMap = new();
        private readonly Dictionary<ScenarioEvent, CancellationTokenSource> _ctsMap = new();
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
                if (!CanStartScenarioEvent(evt, e)) continue;
                
                StartScenarioEvent(evt).Forget();
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

        private async UniTask StartScenarioEvent(ScenarioEvent scenarioEvent) {
            switch (scenarioEvent.actionMode) {
                case ScenarioEvent.ActionMode.FireAndForget: {
                    if (EnableLogs) Log($"Starting new scenario event [{scenarioEvent}]");
                    
                    EnqueueNewScenarioEvent(scenarioEvent);
                    var token = GetScenarioEventCancellationToken(scenarioEvent, cancelPrevious: false);
                    await scenarioEvent.startAction.Apply(context: null, token);
                    DequeueScenarioEvent(scenarioEvent);
                    break;
                }

                case ScenarioEvent.ActionMode.CancelPreviousAndStartNew: {
                    if (EnableLogs) {
                        Log(HasOngoingScenarioEvent(scenarioEvent)
                            ? $"Canceling ongoing scenario event and starting new [{scenarioEvent}] due to action mode {scenarioEvent.actionMode}" 
                            : $"Starting new scenario event [{scenarioEvent}]"
                        );
                    }
                    
                    EnqueueNewScenarioEvent(scenarioEvent);
                    var token = GetScenarioEventCancellationToken(scenarioEvent, cancelPrevious: true);
                    await scenarioEvent.startAction.Apply(context: null, token);
                    DequeueScenarioEvent(scenarioEvent);
                    break;
                }

                case ScenarioEvent.ActionMode.WaitPreviousThenStartNew: {
                    if (EnableLogs) {
                        Log(HasOngoingScenarioEvent(scenarioEvent)
                            ? $"Waiting ongoing scenario events then starting new [{scenarioEvent}] due to action mode {scenarioEvent.actionMode}" 
                            : $"Starting new scenario event [{scenarioEvent}]"
                        );
                    }

                    var globalToken = _cts.Token;
                    while (!globalToken.IsCancellationRequested && HasOngoingScenarioEvent(scenarioEvent)) {
                        await UniTask.Yield();
                    }
                    
                    EnqueueNewScenarioEvent(scenarioEvent);
                    var token = GetScenarioEventCancellationToken(scenarioEvent, cancelPrevious: false);
                    await scenarioEvent.startAction.Apply(context: null, token);
                    DequeueScenarioEvent(scenarioEvent);
                    break;
                }

                case ScenarioEvent.ActionMode.IgnoreNewIfRunningPrevious: {
                    if (HasOngoingScenarioEvent(scenarioEvent)) 
                    {
                        if (EnableLogs) Log($"Requested new scenario event [{scenarioEvent}], but previous is still running. " +
                                            $"Ignore due to action mode {scenarioEvent.actionMode}.");
                        return;
                    }
                    
                    if (EnableLogs) Log($"Starting new scenario event [{scenarioEvent}]");
                    
                    EnqueueNewScenarioEvent(scenarioEvent);
                    var token = GetScenarioEventCancellationToken(scenarioEvent, cancelPrevious: false);
                    await scenarioEvent.startAction.Apply(context: null, token);
                    DequeueScenarioEvent(scenarioEvent);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CancellationToken GetScenarioEventCancellationToken(ScenarioEvent scenarioEvent, bool cancelPrevious) {
            if (_ctsMap.TryGetValue(scenarioEvent, out var cts) && !cancelPrevious) return cts.Token;
            
            cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, new CancellationTokenSource().Token);
            _ctsMap[scenarioEvent] = cts;

            return cts.Token;
        }

        private void EnqueueNewScenarioEvent(ScenarioEvent scenarioEvent) {
            int nextCount = _runActionMap.GetValueOrDefault(scenarioEvent) + 1;
            
            _runActionMap[scenarioEvent] = nextCount;
        }

        private void DequeueScenarioEvent(ScenarioEvent scenarioEvent) {
            int nextCount = _runActionMap.GetValueOrDefault(scenarioEvent) - 1;

            if (nextCount > 0) _runActionMap[scenarioEvent] = nextCount;
            else _runActionMap.Remove(scenarioEvent);
        }
        
        private bool HasOngoingScenarioEvent(ScenarioEvent scenarioEvent) {
            return _runActionMap.TryGetValue(scenarioEvent, out int count) && count > 0;
        }
        
        private static bool CanStartScenarioEvent(ScenarioEvent scenarioEvent, EventReference e) {
            return scenarioEvent is { startAction: not null } && 
                   scenarioEvent.startEvent.EventDomain == e.EventDomain && 
                   scenarioEvent.startEvent.EventId == e.EventId && 
                   (scenarioEvent.subIdMode == ScenarioEvent.SubIdMode.IgnoreSubId || scenarioEvent.startEvent.SubId == e.SubId) && 
                   (!scenarioEvent.expectedRaiseCount.HasValue || scenarioEvent.expectedRaiseCount.Value.IsMatch(e.GetCount())) && 
                   (scenarioEvent.condition == null || scenarioEvent.condition.IsMatch(null));
        }

        [HideInCallstack]
        private static void Log(string message) {
            Debug.Log($"{nameof(ScenarioService).FormatColorOnlyForEditor(Color.white)}: f {Time.frameCount}, {message}");
        }
    }
    
}