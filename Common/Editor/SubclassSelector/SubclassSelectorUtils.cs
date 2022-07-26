using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MisterGames.Common.Attributes;
using UnityEditor;

namespace MisterGames.Common.SubclassSelector {
	
	public static class SubclassSelectorUtils {
		
		public const string NullDisplayName = "<null>";

		public static object SetManagedReference(this SerializedProperty property, Type type) {
			object obj = type != null ? Activator.CreateInstance(type) : null;
			property.managedReferenceValue = obj;
			return obj;
		}

		public static Type GetType(string typeName) {
			int splitIndex = typeName.IndexOf(' ');
			var assembly = Assembly.Load(typeName.Substring(0,splitIndex));
			return assembly.GetType(typeName.Substring(splitIndex + 1));
		}
		
		public static AddTypeMenuAttribute GetAttribute(Type type) {
			return Attribute.GetCustomAttribute(type, typeof(AddTypeMenuAttribute)) as AddTypeMenuAttribute;
		}

		public static string[] GetSplittedTypePath (Type type) {
			var typeMenu = GetAttribute(type);
			if (typeMenu != null) 
			{
				return typeMenu.GetSplittedMenuName();
			}
			
			int splitIndex = type.FullName.LastIndexOf('.');
			if (splitIndex >= 0) 
			{
				return new[]
				{
					type.FullName.Substring(0,splitIndex), 
					type.FullName.Substring(splitIndex + 1),
				};
			}
			
			return new[] { type.Name };
		}

		public static IEnumerable<Type> OrderByType(IEnumerable<Type> source) {
			return source.OrderBy(type => type == null 
				? -999 
				: GetAttribute(type)?.Order ?? 0
			);
		}
	}
}
