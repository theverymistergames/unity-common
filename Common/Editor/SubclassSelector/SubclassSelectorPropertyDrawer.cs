using System;
using System.Reflection;
using MisterGames.Common.Attributes;
using MisterGames.Common.Data;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			var baseType = GetFieldType(property);
			var type = GetManagedReferenceValueType(property);

			var subclassAttribute = (SubclassSelectorAttribute) attribute;
			if (!string.IsNullOrEmpty(subclassAttribute.explicitSerializedTypePath)) {

				string path = property.propertyPath;
				int lastDot = path.LastIndexOf('.');

				path = path.Remove(lastDot + 1, path.Length - 1 - lastDot);

				var serializedObject = property.serializedObject;
				var typeProperty = serializedObject.FindProperty($"{path}{subclassAttribute.explicitSerializedTypePath}");

				baseType = SerializedType.DeserializeType(typeProperty.FindPropertyRelative("_type").stringValue);
			}

			SubclassSelectorGUI.PropertyField(position, property, baseType, type, label);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, true);
		}

		private static Type GetFieldType(SerializedProperty property) {
			string typeName = property.managedReferenceFieldTypename;
			if (string.IsNullOrEmpty(typeName)) return null;

			int splitIndex = typeName.IndexOf(' ');
			return Assembly.Load(typeName[..splitIndex]).GetType(typeName[(splitIndex + 1)..]);
		}

		private static Type GetManagedReferenceValueType(SerializedProperty property) {
			string typeName = property.managedReferenceFullTypename;
			if (string.IsNullOrEmpty(typeName)) return null;

			int splitIndex = typeName.IndexOf(' ');
			return Assembly.Load(typeName[..splitIndex]).GetType(typeName[(splitIndex + 1)..]);
		}
	}
}
