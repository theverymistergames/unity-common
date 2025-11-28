using System;
using System.Collections.Generic;
using MisterGames.Common.Save;
using UnityEngine;

namespace MisterGames.Scenario.Events {
    
    [DefaultExecutionOrder(-10000)]
    public sealed class EventBusLauncher : MonoBehaviour, ISaveable {

        [SerializeField] private string _id;

        [Serializable]
        private struct EventEntry {
            public EventReference eventReference;
            public int count;
        }
        
        private readonly List<EventEntry> _eventsListEmpty = new();
        private readonly List<EventEntry> _eventsListSaveable = new();
        
        private void OnEnable() {
            SaveSystem.Main.Register(this);
        }

        private void OnDisable() {
            SaveSystem.Main.Unregister(this);
        }

        private void OnDestroy() {
            EventBus.Main.Dispose();
        }

        public void OnLoadData(ISaveSystem saveSystem) {
            saveSystem.Pop(SaveStorage.CurrentSave, _id, _eventsListEmpty, out var eventList);

            var raisedEventsMap = EventBus.Main.RaisedEvents;
            
            for (int i = 0; i < eventList.Count; i++) {
                var eventEntry = eventList[i];
                raisedEventsMap[eventEntry.eventReference] = eventEntry.count;
            }
        }

        public void OnSaveData(ISaveSystem saveSystem) {
            _eventsListSaveable.Clear();
            var raisedEventsMap = EventBus.Main.RaisedEvents;
            
            foreach ((var e, int count) in raisedEventsMap) {
                if (!e.EventDomain.IsSerializable(e.EventId)) continue;
                
                _eventsListSaveable.Add(new EventEntry { eventReference = e, count = count });
            }
            
            saveSystem.Push(SaveStorage.CurrentSave, _id, _eventsListSaveable);
        }
    }

}
