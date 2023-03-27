using System;
using System.Reflection;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);
			SubclassSelectorGUI.PropertyField(position, property, GetFieldType(property), label, includeChildren: true);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return SubclassSelectorGUI.GetPropertyHeight(property, label, includeChildren: true);
		}

		private static Type GetFieldType(SerializedProperty property) {
			string typeName = property.managedReferenceFieldTypename;
			if (string.IsNullOrEmpty(typeName)) return null;

			int splitIndex = typeName.IndexOf(' ');
			return Assembly.Load(typeName[..splitIndex]).GetType(typeName[(splitIndex + 1)..]);
		}
	}
}
