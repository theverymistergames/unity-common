using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors
{

    [CreateAssetMenu(fileName = nameof(ActorData), menuName = "MisterGames/Actors/" + nameof(ActorData))]
    public sealed class ActorData : ScriptableObject
    {
        [SubclassSelector] [SerializeReference] private IActorData[] _data;

        public IReadOnlyList<IActorData> Data => _data;
        
        public T GetData<T>() where T : class, IActorData {
            for (int i = 0; i < _data.Length; i++) {
                if (_data[i] is T t) return t;
            }
            
            return default;
        }

#if UNITY_EDITOR
        internal event Action OnValidateCalled = delegate { };
        internal void NotifyValidateCalled() => OnValidateCalled.Invoke();
        
        private void OnValidate() {
            int count = _data.Length;
            
            for (int i = 0; i < count; i++)
            {
                _data[i]?.OnValidate(this);
            }

            OnValidateCalled.Invoke();
        }
#endif
    }
    
}