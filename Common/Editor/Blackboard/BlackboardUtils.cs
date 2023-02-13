using System;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;

namespace MisterGames.Common.Editor {

    public static class BlackboardUtils {

        public static VisualElement CreateBlackboardPropertyView(
            Blackboard blackboard,
            BlackboardProperty property,
            Action<string, object> onValueChanged
        ) {
            int hash = Blackboard.StringToHash(property.name);
            var type = Blackboard.GetPropertyType(property);
            string typeName = TypeNameFormatter.GetTypeName(type);

            var nameField = new BlackboardField { text = property.name, typeText = typeName };
            VisualElement valueField = null;

            if (type == typeof(bool)) {
                var boolField = new Toggle { value = blackboard.GetBool(hash) };
                boolField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = boolField;
            }

            if (type == typeof(float)) {
                var floatField = new FloatField { value = blackboard.GetFloat(hash) };
                floatField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = floatField;
            }

            if (type == typeof(int)) {
                var intField = new IntegerField { value = blackboard.GetInt(hash) };
                intField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = intField;
            }

            if (type == typeof(string)) {
                var textField = new TextField { value = blackboard.GetString(hash) };
                textField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = textField;
            }

            if (type == typeof(Vector2)) {
                var vector2Field = new Vector2Field { value = blackboard.GetVector2(hash) };
                vector2Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector2Field;
            }

            if (type == typeof(Vector3)) {
                var vector3Field = new Vector3Field { value = blackboard.GetVector3(hash) };
                vector3Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector3Field;
            }

            if (type == typeof(ScriptableObject)) {
                var scriptableObjectField = new ObjectField {
                    value = blackboard.GetScriptableObject(hash),
                    objectType = typeof(ScriptableObject),
                    allowSceneObjects = false
                };
                scriptableObjectField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = scriptableObjectField;
            }

            if (type == typeof(GameObject)) {
                var gameObjectField = new ObjectField {
                    value = blackboard.GetGameObject(hash),
                    objectType = typeof(GameObject),
                    allowSceneObjects = false
                };
                gameObjectField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = gameObjectField;
            }

            var row = new BlackboardRow(nameField, valueField);
            
            var container = new VisualElement();
            container.Add(nameField);
            container.Add(row);
            return container;
        }
    }

}
