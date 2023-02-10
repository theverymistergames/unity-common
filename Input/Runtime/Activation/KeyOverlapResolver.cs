using System.Collections.Generic;
using System.Linq;
using MisterGames.Input.Actions;
using MisterGames.Input.Bindings;
using MisterGames.Input.Core;

namespace MisterGames.Input.Activation {
    
    internal sealed class KeyOverlapResolver {
        
        private readonly List<List<InputActionKey>> _groups = new List<List<InputActionKey>>();
        private readonly List<InputActionKey> _keyedActions = new List<InputActionKey>();
        private readonly Dictionary<KeyBinding, List<OverlapInfo>> _overlapMap = new Dictionary<KeyBinding, List<OverlapInfo>>();
        private Dictionary<KeyBinding, List<OverlapInfo>>.Enumerator _overlapEnumerator;

        private readonly struct OverlapInfo {
            public readonly int index;
            public readonly int keyCount;

            public OverlapInfo(int index, int keyCount) {
                this.index = index;
                this.keyCount = keyCount;
            }
        }

        public void RefillOverlapGroups(List<InputAction> actions) {
            Clear();
            FillKeyedActions(actions);
            FillOverlapMap();
            FillGroups();
        }

        public void Clear() {
            _keyedActions.Clear();
            _overlapMap.Clear();
            _groups.Clear();
        }
        
        public void ResolveOverlap() {
            for (int i = 0; i < _groups.Count; i++) {
                var group = _groups[i];
                bool needInterrupt = false;
                
                for (int j = 0; j < group.Count; j++) {
                    var action = group[j];
                    
                    if (needInterrupt) {
                        action.Interrupt();
                        continue;
                    }

                    needInterrupt = action.IsPressed;
                }
            }
        }

        private void FillKeyedActions(List<InputAction> actions) {
            foreach (var action in actions) {
                if (action is InputActionKey keyedAction) _keyedActions.Add(keyedAction);
            }
        }
        
        private void FillOverlapMap() {
            for (int a = 0; a < _keyedActions.Count; a++) {
                var action = _keyedActions[a];
                var bindings = action.Bindings;

                for (int b = 0; b < bindings.Length; b++) {
                    var binding = bindings[b];

                    if (binding is Key key) {
                        var info = new OverlapInfo(a, 1);

                        if (_overlapMap.ContainsKey(key.key)) _overlapMap[key.key].Add(info);
                        else _overlapMap[key.key] = new List<OverlapInfo> {info};

                        continue;
                    }

                    if (binding is KeyCombo keyCombo) {
                        var keys = keyCombo.keys;
                        int keyCount = keys.Length;

                        for (int k = 0; k < keys.Length; k++) {
                            var comboKey = keys[k];
                            var info = new OverlapInfo(a, keyCount);

                            if (_overlapMap.ContainsKey(comboKey)) _overlapMap[comboKey].Add(info);
                            else _overlapMap[comboKey] = new List<OverlapInfo> {info};
                        }
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
    }

}
