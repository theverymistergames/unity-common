using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.Editor.SubclassSelector {
	
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer {
		
		private const int MaxTypePopupLineCount = 13;
		private static readonly Type UnityObjectType = typeof(Object);
		private static GUIContent NullDisplayName => new GUIContent(SubclassSelectorUtils.NullDisplayName);
		private static GUIContent IsNotManagedReferenceLabel => new GUIContent("The property type is not managed reference.");

		private readonly Dictionary<string, AdvancedTypePopup> _typePopups = new Dictionary<string, AdvancedTypePopup>();
		private readonly Dictionary<string, GUIContent> _typeNameCaches = new Dictionary<string, GUIContent>();
		
		private SerializedProperty _targetProperty;

		public override void OnGUI(Rect position,SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty(position, label, property);

			if (property.propertyType == SerializedPropertyType.ManagedReference) {
				var popupPosition = new Rect(position);
				popupPosition.width -= EditorGUIUtility.labelWidth;
				popupPosition.x += EditorGUIUtility.labelWidth;
				popupPosition.height = EditorGUIUtility.singleLineHeight;

				if (EditorGUI.DropdownButton(popupPosition,GetTypeName(property), FocusType.Keyboard)) {
					var popup = GetTypePopup(property);
					_targetProperty = property;
					popup.Show(popupPosition);
				}

				EditorGUI.PropertyField(position, property, label, true);
			} 
			else {
				EditorGUI.LabelField(position, label, IsNotManagedReferenceLabel);
			}

			EditorGUI.EndProperty();
		}

		private AdvancedTypePopup GetTypePopup(SerializedProperty property) {
			string managedReferenceFieldTypename = property.managedReferenceFieldTypename;
			if (_typePopups.TryGetValue(managedReferenceFieldTypename, out var result)) {
				return result;
			}
			
			var state = new AdvancedDropdownState();
			var baseType = SubclassSelectorUtils.GetType(managedReferenceFieldTypename);

			var types = TypeCache
				.GetTypesDerivedFrom(baseType)
				.Append(baseType)
				.Where(t =>
					(t.IsPublic || t.IsNestedPublic) &&
					!t.IsAbstract &&
					!t.IsGenericType &&
					!UnityObjectType.IsAssignableFrom(t) &&
					Attribute.IsDefined(t, typeof(SerializableAttribute))
				);
			
			var popup = new AdvancedTypePopup(types, MaxTypePopupLineCount, state);
			
			popup.OnItemSelected += item => {
				var type = item.Type;
				object obj = _targetProperty.SetManagedReference(type);
				_targetProperty.isExpanded = (obj != null);
				_targetProperty.serializedObject.ApplyModifiedProperties();
				_targetProperty.serializedObject.Update();
			};

			result = popup;
			_typePopups.Add(managedReferenceFieldTypename, result);
			return result;
		}

		private GUIContent GetTypeName(SerializedProperty property) 
		{
			string managedReferenceFullTypename = property.managedReferenceFullTypename;
			if (string.IsNullOrEmpty(managedReferenceFullTypename)) {
				return NullDisplayName;
			}
			
			if (_typeNameCaches.TryGetValue(managedReferenceFullTypename, out var cachedTypeName)) {
				return cachedTypeName;
			}

			var type = SubclassSelectorUtils.GetType(managedReferenceFullTypename);
			string typeName = null;

			var typeMenu = SubclassSelectorUtils.GetAttribute(type);
			if (typeMenu != null) {
				typeName = typeMenu.GetTypeNameWithoutPath();
				
				if (!string.IsNullOrWhiteSpace(typeName)) 
				{
					typeName = ObjectNames.NicifyVariableName(typeName);
				}
			}

			if (string.IsNullOrWhiteSpace(typeName)) {
				typeName = ObjectNames.NicifyVariableName(type.Name);
			}

			var result = new GUIContent(typeName);
			_typeNameCaches.Add(managedReferenceFullTypename, result);
			return result;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, true);
		}
	}
}
