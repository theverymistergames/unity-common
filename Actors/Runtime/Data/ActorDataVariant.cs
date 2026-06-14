using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors {
    
    [CreateAssetMenu(fileName = nameof(ActorDataVariant), menuName = "MisterGames/Actors/" + nameof(ActorDataVariant))]
    public sealed class ActorDataVariant : ActorData {
        
        [EmbeddedInspector]
        [SerializeField] private ActorData _parentConfig;

        private readonly List<IActorData> _dataBuffer = new();
        private readonly HashSet<Type> _overrideDataTypes = new();

        protected override IReadOnlyList<IActorData> GetResultDataArray() {
            if (_parentConfig == null) return GetLocalDataArray();
            
            _dataBuffer.Clear();
            _overrideDataTypes.Clear();
            
            var dataArray = GetLocalDataArray();
            for (int i = 0; i < dataArray.Count; i++) {
                if (dataArray[i] is not { } data) continue;
                
                _dataBuffer.Add(data);
                _overrideDataTypes.Add(data.GetType());
            }
            
            var parentDataArray = _parentConfig.GetDataArray();
            for (int i = 0; i < parentDataArray.Count; i++) {
                if (parentDataArray[i] is not { } data || _overrideDataTypes.Contains(data.GetType())) continue;
                
                _dataBuffer.Add(data);
            }

            return _dataBuffer;
        }
    }
    
}