using System;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MisterGames.UI.Components {
    
    public sealed class ScrollbarCustom : Scrollbar, IPointerMoveHandler {

        [SerializeField] private bool _disableMoveInput = true;
        [SerializeField] private bool _selectOnHover = true;
        
        public override void OnMove(AxisEventData eventData) {
            if (_disableMoveInput) {
                switch (direction) {
                    case Direction.LeftToRight:
                    case Direction.RightToLeft:
                        if (eventData.moveDir is MoveDirection.Left or MoveDirection.Right) return;
                        break;
                
                    case Direction.BottomToTop:
                    case Direction.TopToBottom:
                        if (eventData.moveDir is MoveDirection.Up or MoveDirection.Down) return;
                        break;
                
                    default:
                        throw new ArgumentOutOfRangeException();
                }   
            }
            
            base.OnMove(eventData);
        }

        public void OnPointerMove(PointerEventData eventData) {
            if (_selectOnHover) Select();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(ScrollbarCustom), true)]
        [CanEditMultipleObjects]
        public class ScrollbarCustomEditor : ScrollbarEditor {
        
            private const string CustomPropertiesLabel = "Custom Properties";
            
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
            
                GUILayout.Label(CustomPropertiesLabel, EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_disableMoveInput)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(_selectOnHover)));
            }
        } 
#endif
    }
    
}