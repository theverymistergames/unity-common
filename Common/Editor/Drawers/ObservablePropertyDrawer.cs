using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(Observable<>))]
    public class ObservablePropertyDrawer : PropertyDrawer {
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var valueProperty = property.FindPropertyRelative("_value");
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, valueProperty, label);
            EditorGUI.EndProperty();

            var target = property.GetValue() as ObservableBase;
            target?.NotifyIfChanged();
        }

    }

}