using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute { }
	
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface,
        Inherited = false
    )]
    public sealed class AddTypeMenuAttribute : Attribute {
        
        public static readonly char[] Separators = { '/' };
		
        public string MenuName { get; }
        public int Order { get; }

        public AddTypeMenuAttribute(string menuName, int order = 0) {
            MenuName = menuName;
            Order = order;
        }

        public string[] GetSplittedMenuName() {
            return string.IsNullOrWhiteSpace(MenuName)
                ? Array.Empty<string>() 
                : MenuName.Split(Separators,StringSplitOptions.RemoveEmptyEntries);
        }

        public string GetTypeNameWithoutPath() {
            string[] splittedDisplayName = GetSplittedMenuName();
            return splittedDisplayName.Length > 0 
                ? splittedDisplayName[splittedDisplayName.Length - 1] 
                : null;
        }
    }

}