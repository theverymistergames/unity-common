using System;
using System.Collections.Generic;
using System.Text;

namespace MisterGames.Common.Types {

    public static class TypeNameFormatter {

        private static readonly Dictionary<Type, string> NameOverrides = new Dictionary<Type, string> {
            [typeof(bool)] = "Boolean",
            [typeof(byte)] = "Byte",
            [typeof(sbyte)] = "SByte",
            [typeof(short)] = "Short",
            [typeof(ushort)] = "UShort",
            [typeof(int)] = "Int",
            [typeof(uint)] = "UInt",
            [typeof(long)] = "Long",
            [typeof(ulong)] = "ULong",
            [typeof(float)] = "Float",
            [typeof(double)] = "Double",
            [typeof(char)] = "Char",
            [typeof(string)] = "String",
        };

        public static string GetShortTypeName(Type type) {
            return FormatTypeName(type);
        }

        public static string GetFullTypeName(Type type) {
            return type == null ? "null" : $"{type.Namespace}.{FormatTypeName(type)}";
        }

        public static string GetFullTypeNamePathInBraces(Type type) {
            return type == null ? "null" : $"{FormatTypeName(type)}{(string.IsNullOrWhiteSpace(type.Namespace) ? string.Empty : $" ({type.Namespace})")}";
        }

        private static string FormatTypeName(Type type) {
            if (type == null) return "null";
            if (type.IsArray) return $"{FormatTypeName(type.GetElementType())}[]";

            if (type.IsGenericType) {
                var sb = new StringBuilder();
                sb.Append(type.GetGenericTypeDefinition().Name);
                sb.Append("<");

                var genericArguments = type.GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++) {
                    sb.Append(GetShortTypeName(genericArguments[i]));
                    if (i < genericArguments.Length - 1) sb.Append(", ");
                }

                sb.Append(">");
                return sb.ToString();
            }

            return NameOverrides.TryGetValue(type, out string typeName) ? typeName : type.Name;
        }
    }

}