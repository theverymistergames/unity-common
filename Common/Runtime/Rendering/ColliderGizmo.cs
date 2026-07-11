using MisterGames.Common.Colors;
using UnityEngine;

namespace MisterGames.Common.Rendering {
    
    public sealed class ColliderGizmo : MonoBehaviour {
        
        [SerializeField] private Collider[] _colliders;
        [SerializeField] private Color _color;
        [SerializeField] private bool _solid = true;
        
#if UNITY_EDITOR
        private void Reset() {
            _colliders = gameObject.GetComponentsInChildren<Collider>();
            _color = ColorUtils.ColorFromHash(GetHashCode(), 0.7f, 0.7f).WithAlpha(0.5f);
        }

        private void OnDrawGizmos() {
            for (int i = 0; i < _colliders?.Length; i++) {
                var c = _colliders[i];
                if (c == null) continue;
                
                DebugExt.DrawCollider(c, _color, _solid);
            }
        }  
#endif
    }
    
}