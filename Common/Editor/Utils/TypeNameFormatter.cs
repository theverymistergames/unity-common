using System;
using System.Collections.Generic;

namespace MisterGames.Common.Editor.Utils {

    public static class TypeNameFormatter {

        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(float)] = "Float",
            [typeof(int)] = "Int",
            [typeof(long)] = "Long",
            [typeof(string)] = "String",
        };

        public static string GetTypeName(Type type) {
            return type == null ? "<null>" : NameOverrides.TryGetValue(type, out string typeName) ? typeName : type.Name;
        }
    }

}
