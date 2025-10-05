using System.Collections.Generic;
using MisterGames.Actors;
using MisterGames.Common.Labels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryActorSetMember : MonoBehaviour, IActorComponent {
        
        [SerializeField] private LabelValue<HashSet<IActor>> _setLabel;
        [SerializeField] private Actor[] _addToGroup;

        private void Awake() {
            if (!_setLabel.TryGetData(out var data) || data == null) {
                _setLabel.TrySetData(new HashSet<IActor>(_addToGroup));
                return;
            }

            for (int i = 0; i < _addToGroup?.Length; i++) {
                if (_addToGroup[i] is { } obj) data.Add(obj);
            }
        }

        private void OnDestroy() {
            _setLabel.ClearData();
        }
    }
    
}