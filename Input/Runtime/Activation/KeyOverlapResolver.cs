using System.Collections.Generic;
using System.Linq;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;

namespace MisterGames.Input.Activation {
    
    internal class KeyOverlapResolver {
        
        private readonly List<List<InputActionKey>> _groups = new List<List<InputActionKey>>();
        private readonly List<InputActionKey> _keyedActions = new List<InputActionKey>();
        private readonly Dictionary<KeyBinding, List<OverlapInfo>> _overlapMap = new Dictionary<KeyBinding, List<OverlapInfo>>();
        private Dictionary<KeyBinding, List<OverlapInfo>>.Enumerator _overlapEnumerator;
        
        internal void RefillOverlapGroups(List<InputAction> actions) {
            Clear();
            FillKeyedActions(actions);
            FillOverlapMap();
            FillGroups();
        }

        internal void Clear() {
            _keyedActions.Clear();
            _overlapMap.Clear();
            _groups.Clear();
        }
        
        internal void ResolveOverlap() {
            for (int i = 0; i < _groups.Count; i++) {
                var group = _groups[i];
                bool needInterrupt = false;
                
                for (int j = 0; j < group.Count; j++) {
                    var action = group[j];
                    
                    if (needInterrupt) {
                        action.Interrupt();
                        continue;
                    }

                    needInterrupt = action.IsBindingActive();
                }
            }
        }

        private void FillKeyedActions(List<InputAction> actions) {
            foreach (var action in actions) {
                if (action is InputActionKey keyedAction) _keyedActions.Add(keyedAction);
            }
        }
        
        private void FillOverlapMap() {
            int count = _keyedActions.Count;
            for (int i = 0; i < count; i++) {
                var keyedAction = _keyedActions[i];
                var bindings = keyedAction.GetBindings();
                
                foreach (var binding in bindings) {
                    var keys = binding.GetBindings();
                    int keyCount = keys.Length;

                    foreach (var key in keys) {
                        var info = new OverlapInfo { index = i, keyCount = keyCount };

                        if (_overlapMap.ContainsKey(key)) _overlapMap[key].Add(info);
                        else _overlapMap[key] = new List<OverlapInfo> {info};
                    }
                }
            }
        }
        
        private void FillGroups() {
            _overlapEnumerator = _overlapMap.GetEnumerator();
            while (_overlapEnumerator.MoveNext()) {
                var infos = _overlapEnumerator.Current.Value;
                if (infos.Count < 2) continue;

                var sortedActions = infos
                    .OrderByDescending(info => info.keyCount)
                    .Select(info => info.index)
                    .Distinct()
                    .Select(i => _keyedActions[i])
                    .ToList();

                _groups.Add(sortedActions);
            }
        }
        
        private struct OverlapInfo {
            public int index;
            public int keyCount;
        }
    }

}
