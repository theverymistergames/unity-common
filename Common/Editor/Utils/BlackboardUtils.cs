using System;
using MisterGames.Common.Data;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;

namespace MisterGames.Common.Editor.Utils {

    public static class BlackboardUtils {

        public static VisualElement CreateBlackboardPropertyView(BlackboardProperty data, Action<string, object> onValueChanged) {
            var type = Blackboard.GetPropertyType(data);
            string typeName = Blackboard.GetTypeName(type);
            
            var nameField = new BlackboardField { text = data.name, typeText = typeName };
            VisualElement valueField = null;

            if (type == typeof(bool)) {
                var boolField = new Toggle { value = Blackboard.GetPropertyValue<bool>(data) };
                boolField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = boolField;
            }

            if (type == typeof(float)) {
                var floatField = new FloatField { value = Blackboard.GetPropertyValue<float>(data) };
                floatField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = floatField;
            }

            if (type == typeof(int)) {
                var intField = new IntegerField { value = Blackboard.GetPropertyValue<int>(data) };
                intField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = intField;
            }

            if (type == typeof(string)) {
                var textField = new TextField { value = Blackboard.GetPropertyValue<string>(data) };
                textField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = textField;
            }

            if (type == typeof(Vector2)) {
                var vector2Field = new Vector2Field { value = Blackboard.GetPropertyValue<Vector2>(data) };
                vector2Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector2Field;
            }

            if (type == typeof(Vector3)) {
                var vector3Field = new Vector3Field { value = Blackboard.GetPropertyValue<Vector3>(data) };
                vector3Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector3Field;
            }
            
            if (type == typeof(ScriptableObject)) {
                var scriptableObjectField = new ObjectField {
                    value = Blackboard.GetPropertyValue<ScriptableObject>(data),
                    objectType = typeof(ScriptableObject),
                    allowSceneObjects = false
                };
                scriptableObjectField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = scriptableObjectField;
            }

            var row = new BlackboardRow(nameField, valueField);
            
            var container = new VisualElement();
            container.Add(nameField);
            container.Add(row);
            return container;
        }

    }

}