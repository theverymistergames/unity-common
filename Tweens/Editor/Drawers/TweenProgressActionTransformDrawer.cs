using MisterGames.Tweens.Actions;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Tweens.Editor.Drawers {

    [CustomPropertyDrawer(typeof(TweenProgressActionTransform))]
    public class TweenProgressActionTransformDrawer : PropertyDrawer {

        // todo: not called if property object is [SerializeReference][SubclassSelector]
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property, label, true);

            if (!property.isExpanded) return;

            var transformAction = (TweenProgressActionTransform) property.managedReferenceValue;

            position.height = 18f;

            position.y -= 40f;
            if (GUI.Button(position, "Setup start value")) transformAction.WriteCurrentValueAsStartValue();

            position.y -= 40f;
            if (GUI.Button(position, "Setup end value")) transformAction.WriteCurrentValueAsEndValue();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property) + (property.isExpanded ? 80f : 0f);
        }
    }

}
