using System;
using System.Reflection;
using MisterGames.Common.Color;
using MisterGames.Common.Lists;
using UnityEngine;

namespace MisterGames.Blueprints.Editor {

    internal struct NodeMeta {
        
        public string name;
        public string category;
        public Color color;
        
        internal static NodeMeta From(Type type) {
            var attr = GetAttr(type);
            var color = ColorUtils.HexToColor(attr.Color);
            string name = attr.Name.IsEmpty() ? type.Name : attr.Name;

            return new NodeMeta { name = name, category = attr.Category, color = color };
        }
        
        internal static BlueprintNodeAttribute GetAttr(Type type) {
            return type.GetCustomAttribute<BlueprintNodeAttribute>(false);
        }
        
        internal static bool HasAttribute(Type type) {
            return NodeMeta.GetAttr(type) != null;
        }
    }

}