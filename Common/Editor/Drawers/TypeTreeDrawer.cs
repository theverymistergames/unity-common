using System;
using MisterGames.Common.Editor.Utils;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {
/*
    [CustomPropertyDrawer(typeof(TypeTreeAttribute))]
    public class TypeTreeDrawer : PropertyDrawer {

        private TypeSearchWindow _searchWindow;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var baseType = ((TypeTreeAttribute) attribute).baseType;
            
            if (_searchWindow == null) {
                _searchWindow = ScriptableObject.CreateInstance<TypeSearchWindow>();
                _searchWindow.OnTypePicked = pickedType => {
                    var instance = Activator.CreateInstance(pickedType);
                    property.SetValue(instance);
                };
            }

            var h = position.height;

            position.x += 100;
            position.y -= 1;
            position.height = 20;
            
            var target = property.GetValue();
            var targetType = target.GetType();
            
            if (GUI.Button(position, $": {targetType.Name}", "Label")) {
                _searchWindow.SetBaseType(baseType);
                var searchWindowPosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                SearchWindow.Open(new SearchWindowContext(searchWindowPosition), _searchWindow);    
            }

            position.x -= 100;
            position.y += 1;
            position.height = h;
            
            EditorGUI.PropertyField(position, property, label, true);
        }

    }
*/
}