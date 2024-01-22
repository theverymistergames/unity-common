using Cysharp.Threading.Tasks;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Tweens;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(TweenPlayer))]
    public class TweenPlayerPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            float h = EditorGUI.GetPropertyHeight(property, includeChildren: true);
            position.height = h;
            EditorGUI.PropertyField(position, property, label, includeChildren: true);

            EditorGUI.EndProperty();

            if (!property.isExpanded) return;

            position.y += h + EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;
            position.x += 14f;
            position.width = (position.width - 14f) * 0.5f;

            if (GUI.Button(position, "Play")) {
                (property.GetValue() as TweenPlayer)?.Play().Forget();
            }

            position.x += position.width;

            if (GUI.Button(position, "Stop")) {
                (property.GetValue() as TweenPlayer)?.Stop();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float h = EditorGUI.GetPropertyHeight(property, true);

            if (property.isExpanded) {
                h += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return h;
        }
    }

}
