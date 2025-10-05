using MisterGames.Common.Labels;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Logic.Libs {
    
    public sealed class LabelLibraryObjectReference : MonoBehaviour {
        
        [SerializeField] private Object _object;
        [SerializeField] private LabelValue<Object> _label;

        private void Awake() {
            _label.TrySetData(_object);
        }

        private void OnDestroy() {
            _label.ClearData();
        }
    }
    
}