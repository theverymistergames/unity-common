using System;
using System.Collections;
using System.Collections.Generic;
using MisterGames.Common.Data;
using MisterGames.Common.Editor.Utils;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Blackboard = MisterGames.Common.Data.Blackboard;

namespace MisterGames.Common.Editor {

    public static class BlackboardUtils {

        public static IEqualityComparer GetEqualityComparer(Type type) {
            if (type == typeof(bool)) return EqualityComparer<bool>.Default;
            if (type == typeof(float)) return EqualityComparer<float>.Default;
            if (type == typeof(int)) return EqualityComparer<int>.Default;
            if (type == typeof(string)) return EqualityComparer<string>.Default;
            if (type == typeof(Vector2)) return EqualityComparer<Vector2>.Default;
            if (type == typeof(Vector3)) return EqualityComparer<Vector3>.Default;
            if (type == typeof(ScriptableObject)) return EqualityComparer<ScriptableObject>.Default;
            if (type == typeof(GameObject)) return EqualityComparer<GameObject>.Default;

            throw new NotSupportedException($"Blackboard field of type {type.Name} is not supported");
        }

        public static object BlackboardField(Type type, object value, string name) {
            if (type == typeof(bool)) return EditorGUILayout.Toggle(name, (bool) value);
            if (type == typeof(float)) return EditorGUILayout.FloatField(name, (float) value);
            if (type == typeof(int)) return EditorGUILayout.IntField(name, (int) value);
            if (type == typeof(string)) return EditorGUILayout.TextField(name, (string) value);
            if (type == typeof(Vector2)) return EditorGUILayout.Vector2Field(name, (Vector2) value);
            if (type == typeof(Vector3)) return EditorGUILayout.Vector3Field(name, (Vector3) value);
            if (type == typeof(ScriptableObject)) return EditorGUILayout.ObjectField(name, (ScriptableObject) value, typeof(ScriptableObject), false);
            if (type == typeof(GameObject)) return EditorGUILayout.ObjectField(name, (GameObject) value, typeof(GameObject), true);

            throw new NotSupportedException($"Blackboard field of type {type.Name} is not supported");
        }

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
