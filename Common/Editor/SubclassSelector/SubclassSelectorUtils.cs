using System;
using System.Reflection;
using UnityEditor;

namespace MisterGames.Common.Editor.SubclassSelector {
	
	public static class SubclassSelectorUtils {
		
		public const string NullDisplayName = "<null>";

		public static object SetManagedReference(SerializedProperty property, Type type) {
			object obj = type != null ? Activator.CreateInstance(type) : null;
			property.managedReferenceValue = obj;
			return obj;
		}

		public static Type GetType(string typeName) {
			int splitIndex = typeName.IndexOf(' ');
			var assembly = Assembly.Load(typeName[..splitIndex]);
			return assembly.GetType(typeName[(splitIndex + 1)..]);
		}

		public static string[] GetSplittedTypePath(Type type) {
			int splitIndex = type.FullName.LastIndexOf('.');
			if (splitIndex >= 0) 
			{
				return new[]
				{
					type.FullName[..splitIndex],
					type.FullName[(splitIndex + 1)..],
				};
			}
			
			return new[] { type.Name };
		}
	}
}
