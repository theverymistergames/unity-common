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

        public override void OnScroll(PointerEventData data) {
            if (_disableInternalScrollInput) return;
            
            base.OnScroll(data);
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
            }
        } 
#endif
    }
    
}