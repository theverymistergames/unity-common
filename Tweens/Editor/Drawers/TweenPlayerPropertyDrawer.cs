using Cysharp.Threading.Tasks;
using MisterGames.Common.Editor.SerializedProperties;
using MisterGames.Common.Maths;
using MisterGames.Tweens;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    [CustomPropertyDrawer(typeof(TweenPlayer))]
    public class TweenPlayerPropertyDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var progressProperty = property.FindPropertyRelative("_progress");
            float oldProgress = progressProperty.floatValue;
            
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            
            float h = EditorGUI.GetPropertyHeight(property, includeChildren: true);
            position.height = h;
            EditorGUI.PropertyField(position, property, label, includeChildren: true);

            bool changed = EditorGUI.EndChangeCheck();
            
            EditorGUI.EndProperty();
            
            if (!property.isExpanded) return;

            float newProgress = progressProperty.floatValue;
            if (changed && !newProgress.IsNearlyEqual(oldProgress) && property.GetValue() is TweenPlayer tweenPlayer) {
                tweenPlayer.Progress = newProgress;
            }
            
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
