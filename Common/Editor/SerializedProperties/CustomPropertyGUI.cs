﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.SerializedProperties {

	public static class CustomPropertyGUI {

		private static Dictionary<Type, Type> TargetTypeToPropertyDrawerTypeMap;
		private static Dictionary<Type, PropertyDrawer> FieldTypeToPropertyDrawerInstanceMap;
		private static readonly string IgnoreScope = typeof(int).Module.ScopeName;

		public static void PropertyField(
			Rect position,
			SerializedProperty property,
			GUIContent label,
			FieldInfo fieldInfo,
			Attribute previousAttribute = null,
			bool includeChildren = false
		) {
			var nextAttr = GetNextAttribute(fieldInfo, previousAttribute);
			var propertyDrawer = GetCustomPropertyDrawerForProperty(fieldInfo, nextAttr);

			if (propertyDrawer == null) {
				EditorGUI.PropertyField(position, property, label, includeChildren);
				return;
			}

			propertyDrawer.OnGUI(position, property, label);
		}

		public static float GetPropertyHeight(
			SerializedProperty property,
			GUIContent label,
			FieldInfo fieldInfo,
			Attribute previousAttribute = null,
			bool includeChildren = false
		) {
			var nextAttr = GetNextAttribute(fieldInfo, previousAttribute);
			return GetCustomPropertyDrawerForProperty(fieldInfo, nextAttr)?.GetPropertyHeight(property, label)
			       ?? EditorGUI.GetPropertyHeight(property, label, includeChildren);
		}

		private static PropertyDrawer GetCustomPropertyDrawerForProperty(FieldInfo fieldInfo, Attribute attr) {
			FieldTypeToPropertyDrawerInstanceMap ??= new Dictionary<Type, PropertyDrawer>();

			while (true) {
				var drawerTargetType = attr?.GetType() ?? fieldInfo?.FieldType;
				if (drawerTargetType == null) return null;

				var propertyDrawerType = GetPropertyDrawerTypeByFieldType(drawerTargetType);

				if (FieldTypeToPropertyDrawerInstanceMap.TryGetValue(drawerTargetType, out var drawer)) {
					ActualizeFieldInfo(propertyDrawerType, drawer, fieldInfo, attr);
					return drawer;
				}


				drawer = InstantiatePropertyDrawer(propertyDrawerType);
				ActualizeFieldInfo(propertyDrawerType, drawer, fieldInfo, attr);

				// Failed to instantiate drawer at first attempt and attribute is not null:
				// trying to instantiate drawer with next attribute
				if (drawer == null && attr != null) {
					attr = GetNextAttribute(fieldInfo, attr);
					if (attr != null) continue;
				}

				if (drawer == null) return null;

				FieldTypeToPropertyDrawerInstanceMap[drawerTargetType] = drawer;
				return drawer;
			}
		}

		private static Attribute GetNextAttribute(FieldInfo fieldInfo, Attribute attribute) {
			if (attribute == null) return null;

			object[] attributes = fieldInfo
				.GetCustomAttributes(typeof(PropertyAttribute), inherit: false)
				.Where(a => a is not (SerializeField or SerializeReference))
				.ToArray();

			var attributeType = attribute.GetType();
			int nextAttributeIndex = -1;

			for (int i = 0; i < attributes.Length; i++) {
				object attr = attributes[i];
				if (attr.GetType() != attributeType) continue;

				nextAttributeIndex = i + 1;
				break;
			}

			return 0 < nextAttributeIndex && nextAttributeIndex < attributes.Length
				? (Attribute) attributes[nextAttributeIndex]
				: null;
		}

		private static PropertyDrawer InstantiatePropertyDrawer(Type drawerType) {
			if (drawerType == null) return null;

			try {
				return (PropertyDrawer) Activator.CreateInstance(drawerType);
			}
			catch (Exception) {
				return null;
			}
		}

		private static void ActualizeFieldInfo(
			Type drawerType,
			PropertyDrawer drawer,
			FieldInfo fieldInfo,
			Attribute insertAttribute
		) {
			try {
				// Reassign the attribute and fieldInfo fields in the drawer so it can access the argument values
				var fieldInfoField = drawerType.GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.NonPublic);
				fieldInfoField?.SetValue(drawer, fieldInfo);

				var attributeField = drawerType.GetField("m_Attribute", BindingFlags.Instance | BindingFlags.NonPublic);
				attributeField?.SetValue(drawer, insertAttribute);
			}
			catch (Exception) {
				// ignored
			}
		}

		private static Type GetPropertyDrawerTypeByFieldType(Type fieldType) {
			// Ignore .net types from mscorlib.dll
			if (fieldType.Module.ScopeName.Equals(IgnoreScope)) return null;

			FetchTargetTypeToPropertyDrawerTypeMap();

			// Of all property drawers in the assembly we need to find one that affects target type
			// or one of the base types of target type
			var t = fieldType;
			while (t != null) {
				if (!TargetTypeToPropertyDrawerTypeMap.TryGetValue(t, out var drawerType)) {
					t = t.BaseType;
					continue;
				}

				var attr = CustomAttributeData
					.GetCustomAttributes(drawerType)
					.FirstOrDefault(a => a.AttributeType == typeof(CustomPropertyDrawer))!;

				var drawerFieldType = attr.ConstructorArguments.FirstOrDefault().Value as Type;
				if (drawerFieldType == fieldType) return drawerType;

				bool useForChildren = attr.ConstructorArguments.LastOrDefault().Value is true;
				if (useForChildren) return drawerType;
			}

			return null;
		}

		private static void FetchTargetTypeToPropertyDrawerTypeMap() {
			if (TargetTypeToPropertyDrawerTypeMap != null) return;

			var propertyDrawerTypes = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t =>
					t is { IsClass: true, IsAbstract: false } &&
					typeof(PropertyDrawer).IsAssignableFrom(t) &&
					Attribute.IsDefined(t, typeof(CustomPropertyDrawer))
				)
				.ToArray();

			TargetTypeToPropertyDrawerTypeMap = new Dictionary<Type, Type>(propertyDrawerTypes.Length);

			for (int i = 0; i < propertyDrawerTypes.Length; i++) {
				var drawerType = propertyDrawerTypes[i];

				var attr = CustomAttributeData
					.GetCustomAttributes(drawerType)
					.FirstOrDefault(a => a.AttributeType == typeof(CustomPropertyDrawer));

				var targetType = attr?.ConstructorArguments.FirstOrDefault().Value as Type;
				if (targetType == null || TargetTypeToPropertyDrawerTypeMap.ContainsKey(targetType)) continue;

				TargetTypeToPropertyDrawerTypeMap.Add(targetType, drawerType);
			}
		}
	}

}
