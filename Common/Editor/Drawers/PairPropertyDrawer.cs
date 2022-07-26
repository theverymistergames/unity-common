using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
    
    [CustomPropertyDrawer(typeof(Pair<,>), true)]
    public class PairPropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var firstProperty = property.FindPropertyRelative("_first");
            var secondProperty = property.FindPropertyRelative("_second");
            
            position.width /= 2f;
            EditorGUI.PropertyField(position, firstProperty, label, true);
            
            position.x += position.width;
            EditorGUI.PropertyField(position, secondProperty, label, true);
        }
 
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
        
    }
}