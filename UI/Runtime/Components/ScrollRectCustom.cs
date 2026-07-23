using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class ScrollRectCustom : ScrollRect {

        [SerializeField] private bool _disableInternalScrollInput = true;
        [SerializeField] [Min(0f)] private float _fitEpsilon = 0.01f;
        
        private bool _isPinning;
        
        protected override void OnEnable() {
            base.OnEnable();
            Canvas.willRenderCanvases += PinContentIfNeeded;
        }

        protected override void OnDisable() {
            Canvas.willRenderCanvases -= PinContentIfNeeded;
            base.OnDisable();
        }
        
        public override void OnScroll(PointerEventData data) {
            if (_disableInternalScrollInput) return;
            
            base.OnScroll(data);
        }

        private RectTransform GetViewportRect() {
            return viewport != null ? viewport : transform as RectTransform;
        }

        private bool ContentFitsHorizontally() {
            return content != null &&
                   GetViewportRect() is { } viewportRect && 
                   content.rect.width <= viewportRect.rect.width + _fitEpsilon;
        }

        private bool ContentFitsVertically() {
            return content != null &&
                   GetViewportRect() is { } viewportRect && 
                   content.rect.height <= viewportRect.rect.height + _fitEpsilon;
        }

        protected override void SetContentAnchoredPosition(Vector2 position) {
            position = GetPinnedPosition(position);
            base.SetContentAnchoredPosition(position);
        }

        protected override void SetNormalizedPosition(float value, int axis) {
            if ((axis == 0 && ContentFitsHorizontally()) ||
                (axis == 1 && ContentFitsVertically()))
            {
                return;
            }

            base.SetNormalizedPosition(value, axis);
        }

        private void PinContentIfNeeded() {
            if (!isActiveAndEnabled || _isPinning || content == null) return;

            var current = content.anchoredPosition;
            var target = GetPinnedPosition(current);
            if (target == current) return;

            _isPinning = true;
            content.anchoredPosition = target;
            _isPinning = false;
        }

        private Vector2 GetPinnedPosition(Vector2 position) {
            if (ContentFitsHorizontally()) position.x = GetAnchorAlignedPosition(0);
            if (ContentFitsVertically()) position.y = GetAnchorAlignedPosition(1);

            return position;
        }
        
        private float GetAnchorAlignedPosition(int axis) {
            float pivot = content.pivot[axis];
            float anchorAtPivot = Mathf.Lerp(content.anchorMin[axis], content.anchorMax[axis], pivot);
            return GetViewportRect().rect.size[axis] * (pivot - anchorAtPivot);
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(ScrollRectCustom), true)]
        [CanEditMultipleObjects]
        private class ScrollRectCustomEditor : ScrollRectEditor {
        
            private const string CustomPropertiesLabel = "Custom Properties";
            
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
            
                GUILayout.Label(CustomPropertiesLabel, EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_disableInternalScrollInput)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_fitEpsilon)));
            }
        } 
#endif
    }
    
}