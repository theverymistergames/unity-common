using System;
using System.Collections.Generic;
using System.Linq;
using MisterGames.Common.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MisterGames.Common.SubclassSelector {
	
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer {
		
		private const int MaxTypePopupLineCount = 13;
		private static readonly Type UnityObjectType = typeof(Object);
		private static readonly GUIContent NullDisplayName = new GUIContent(SubclassSelectorUtils.NullDisplayName);
		private static readonly GUIContent IsNotManagedReferenceLabel = new GUIContent("The property type is not managed reference.");

		private readonly Dictionary<string, TypePopupCache> _typePopups = new Dictionary<string, TypePopupCache>();
		private readonly Dictionary<string, GUIContent> _typeNameCaches = new Dictionary<string, GUIContent>();
		
		private SerializedProperty _targetProperty;
		
		private readonly struct TypePopupCache {
			public readonly AdvancedTypePopup typePopup;
			public readonly AdvancedDropdownState state;
		
			public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state) {
				this.typePopup = typePopup;
				this.state = state;
			}
		}

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
					popup.typePopup.Show(popupPosition);
				}

				EditorGUI.PropertyField(position, property, label, true);
			} 
			else {
				EditorGUI.LabelField(position, label, IsNotManagedReferenceLabel);
			}

			EditorGUI.EndProperty();
		}

		private TypePopupCache GetTypePopup(SerializedProperty property) {
			string managedReferenceFieldTypename = property.managedReferenceFieldTypename;
			if (_typePopups.TryGetValue(managedReferenceFieldTypename, out var result)) {
				return result;
			}
			
			var state = new AdvancedDropdownState();
			var baseType = SubclassSelectorUtils.GetType(managedReferenceFieldTypename);

			var types = TypeCache
				.GetTypesDerivedFrom(baseType)
				.Append(baseType)
				.Where(p => 
					(p.IsPublic || p.IsNestedPublic) && 
					!p.IsAbstract && 
					!p.IsGenericType && 
					!UnityObjectType.IsAssignableFrom(p) && 
					Attribute.IsDefined(p, typeof(SerializableAttribute))
				);
			
			var popup = new AdvancedTypePopup(types, MaxTypePopupLineCount, state);
			
			popup.OnItemSelected += item => {
				var type = item.Type;
				object obj = _targetProperty.SetManagedReference(type);
				_targetProperty.isExpanded = (obj != null);
				_targetProperty.serializedObject.ApplyModifiedProperties();
				_targetProperty.serializedObject.Update();
			};

			result = new TypePopupCache(popup, state);
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
