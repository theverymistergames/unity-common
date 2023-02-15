using System;
using MisterGames.Common.Data;
using MisterGames.Common.Easing;
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
            string typeName = TypeNameFormatter.GetTypeName(property.type);

            var nameField = new BlackboardField { text = property.name, typeText = typeName };
            VisualElement valueField = null;

            if (property.type == typeof(bool)) {
                var boolField = new Toggle { value = blackboard.GetBool(hash) };
                boolField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = boolField;
            }

            if (property.type == typeof(float)) {
                var floatField = new FloatField { value = blackboard.GetFloat(hash) };
                floatField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = floatField;
            }

            if (property.type == typeof(int)) {
                var intField = new IntegerField { value = blackboard.GetInt(hash) };
                intField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = intField;
            }

            if (property.type == typeof(string)) {
                var textField = new TextField { value = blackboard.GetString(hash) };
                textField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = textField;
            }

            if (property.type == typeof(Vector2)) {
                var vector2Field = new Vector2Field { value = blackboard.GetVector2(hash) };
                vector2Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector2Field;
            }

            if (property.type == typeof(Vector3)) {
                var vector3Field = new Vector3Field { value = blackboard.GetVector3(hash) };
                vector3Field.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = vector3Field;
            }

            if (property.type == typeof(EasingCurve)) {
                var curve = blackboard.GetCurve(hash);

                var easingField = new EnumField(curve.easingType);
                var curveField = new CurveField { value = curve.curve };

                easingField.RegisterValueChangedCallback(evt => {
                    var value = new EasingCurve { easingType = (EasingType) evt.newValue };
                    value.SetCurveFromEasingType();

                    curveField.value = value.curve;

                    onValueChanged.Invoke(nameField.text, value);
                });

                curveField.RegisterValueChangedCallback(evt => {
                    var value = new EasingCurve();
                    if (Enum.TryParse<EasingType>(easingField.text, out var easingType)) value.easingType = easingType;
                    value.curve = evt.newValue;

                    onValueChanged.Invoke(nameField.text, value);
                });

                var easingCurveContainer = new VisualElement();
                easingCurveContainer.Add(easingField);
                easingCurveContainer.Add(curveField);

                valueField = easingCurveContainer;
            }

            if (property.type == typeof(ScriptableObject)) {
                var scriptableObjectField = new ObjectField {
                    value = blackboard.GetScriptableObject(hash),
                    objectType = typeof(ScriptableObject),
                    allowSceneObjects = false
                };
                scriptableObjectField.RegisterValueChangedCallback(evt => onValueChanged.Invoke(nameField.text, evt.newValue));
                valueField = scriptableObjectField;
            }

            if (property.type == typeof(GameObject)) {
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
