#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using System.Collections.Generic;
using System.Text;

namespace MisterGames.Common.Data {

    public static class TypeNameFormatter {

        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(float)] = "Float",
            [typeof(int)] = "Int",
            [typeof(long)] = "Long",
            [typeof(string)] = "String",
        };

        public static string GetTypeName(Type type) {
            if (type == null) return "null";
            if (type.IsArray) return $"{GetTypeName(type.GetElementType())}[]";

            if (type.IsGenericType) {
                var sb = new StringBuilder();
                sb.Append(type.GetGenericTypeDefinition().Name);
                sb.Append("<");

                var genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++) {
                    sb.Append(GetTypeName(genericArguments[i]));
                    if (i < genericArguments.Length - 1) sb.Append(", ");
                }

                sb.Append(">");
                return sb.ToString();
            }

            return NameOverrides.TryGetValue(type, out string typeName) ? typeName : type.Name;
        }
    }

}

#endif
