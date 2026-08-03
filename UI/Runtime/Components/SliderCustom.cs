using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace MisterGames.UI.Components {
    
    public sealed class SliderCustom : Slider {
        
        [SerializeField] private bool _disableInternalInput = true;
        
        public override void OnMove(AxisEventData eventData) {
            if (_disableInternalInput) {
                switch (eventData.moveDir) {
                    case MoveDirection.Right:
                        Navigate(eventData, FindSelectableOnRight());
                        break;

                    case MoveDirection.Up:
                        Navigate(eventData, FindSelectableOnUp());
                        break;

                    case MoveDirection.Left:
                        Navigate(eventData, FindSelectableOnLeft());
                        break;

                    case MoveDirection.Down:
                        Navigate(eventData, FindSelectableOnDown());
                        break;
                }
                return;
            }
            
            base.OnMove(eventData);
        }
        
        private void Navigate(AxisEventData eventData, Selectable sel) {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;
        }
        
#if UNITY_EDITOR
        [CustomEditor(typeof(SliderCustom), true)]
        [CanEditMultipleObjects]
        private class SliderCustomEditor : SliderEditor {
        
            private const string CustomPropertiesLabel = "Custom Properties";
            
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
            
                GUILayout.Label(CustomPropertiesLabel, EditorStyles.boldLabel);

                serializedObject.Update();

                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_disableInternalInput)));

                serializedObject.ApplyModifiedProperties();
            }
        } 
#endif
    }
    
}