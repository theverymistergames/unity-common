using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Actors;
using MisterGames.Actors.Actions;
using MisterGames.Common.Async;
using MisterGames.Common.Attributes;
using MisterGames.Common.Localization;
using MisterGames.Common.Service;
using MisterGames.Dialogues.Core;
using UnityEngine;

namespace MisterGames.Dialogues.Components {
    
    public sealed class DialogueEvents : MonoBehaviour, IActorComponent {

        [SerializeField] private DialogueEventData[] _events;
        
        [Serializable]
        private struct DialogueEventData {
            public LocalizationKey key;
            public DialogueEvent eventType;
            public bool awaitAction;
            [SubclassSelector]
            [SerializeReference] public IActorAction action;
        }

        private readonly Dictionary<int, Func<UniTask>> _subscribedEventsMap = new();
        
        private CancellationTokenSource _enableCts;
        private IActor _actor;

        void IActorComponent.OnAwake(IActor actor) {
            _actor = actor;
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
            
            if (!Services.TryGet(out IDialogueService service)) return;
            
            service.OnAnyDialogueEvent += OnDialogueEvent;
            
            SubscribeAwaitedEvents();
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
            
            if (!Services.TryGet(out IDialogueService service)) return;
            
            service.OnAnyDialogueEvent -= OnDialogueEvent;
            
            UnsubscribeAwaitedEvents();
        }

        private void OnDialogueEvent(LocalizationKey key, DialogueEvent eventType) {
            for (int i = 0; i < _events.Length; i++) {
                ref var data = ref _events[i];
                if (data.eventType != eventType || data.awaitAction || data.key != key) continue;
                
                data.action?.Apply(_actor, _enableCts.Token);
            }
        }

        private void SubscribeAwaitedEvents() {
            if (!Services.TryGet(out IDialogueService service)) return;
            
            for (int i = 0; i < _events.Length; i++) {
                ref var data = ref _events[i];
                if (!data.awaitAction || data.action == null || data.key.IsNull()) continue;

                var factory = CreateEventActionFactory(data.action);
                
                if (_subscribedEventsMap.TryAdd(i, factory)) {
                    service.AddDialogueEvent(data.key, data.eventType, factory);   
                }
            }
        }

        private void UnsubscribeAwaitedEvents() {
            if (!Services.TryGet(out IDialogueService service)) return;

            foreach ((int index, var factory) in _subscribedEventsMap) {
                ref var data = ref _events[index];
                service.RemoveDialogueEvent(data.key, data.eventType, factory);
            }
            
            _subscribedEventsMap.Clear();
        }

        private Func<UniTask> CreateEventActionFactory(IActorAction action) {
            return () => action.Apply(_actor, _enableCts.Token);
        }
    }
    
}