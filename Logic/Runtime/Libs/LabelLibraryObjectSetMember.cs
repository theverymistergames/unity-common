using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryObjectSetMember : MonoBehaviour, IActorComponent {
        
        [SerializeField] private LabelValue<HashSet<Object>> _groupLabel;
        [SerializeField] private Object[] _addToGroup;

        private void Awake() {
            if (!_groupLabel.TryGetData(out var data) || data == null) {
                _groupLabel.TrySetData(new HashSet<Object>(_addToGroup));
                return;
            }

            for (int i = 0; i < _addToGroup?.Length; i++) {
                if (_addToGroup[i] is { } obj) data.Add(obj);
            }
        }

        private void OnDestroy() {
            _groupLabel.ClearData();
        }
    }
    
}