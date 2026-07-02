using System;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;

namespace MisterGames.Common.Types {

    [Serializable]
    public struct SerializedType : IEquatable<SerializedType> {

        [SerializeField] private string _type;

        private const string TO_STRIP_START = ", Version";
        private static readonly ConcurrentDictionary<string, Type> s_cache = new();
        
        public SerializedType(Type type) {
            _type = SerializeType(type);
        }

        public Type ToType() {
            return DeserializeType(_type);
        }

        public bool Equals(SerializedType other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            return obj is SerializedType s && this == s;
        }

        public override int GetHashCode() {
            return string.IsNullOrWhiteSpace(_type) ? 0 : _type.GetHashCode();
        }

        public static bool operator ==(SerializedType serializedType0, SerializedType serializedType1) {
            string t0 = serializedType0._type;
            string t1 = serializedType1._type;
            return string.IsNullOrWhiteSpace(t0) && string.IsNullOrWhiteSpace(t1) || t0 == t1;
        }

        public static bool operator !=(SerializedType serializedType0, SerializedType serializedType1) {
            return !(serializedType0 == serializedType1);
        }

        public override string ToString() {
            return ToType().ToString();
        }

        public static string SerializeType(Type type) {
            if (type == null) return null;

            string typeName = type.AssemblyQualifiedName;
            if (string.IsNullOrEmpty(typeName)) return null;

            int first = typeName.IndexOf(TO_STRIP_START, StringComparison.Ordinal);
            if (first < 0) return typeName;

            var sb = new StringBuilder(typeName.Length);
            int pos = 0;

            for (int i = first; i >= 0; i = typeName.IndexOf(TO_STRIP_START, pos, StringComparison.Ordinal)) {
                sb.Append(typeName, pos, i - pos);
                pos = FindStripEnd(typeName, i);
            }

            sb.Append(typeName, pos, typeName.Length - pos);
            return sb.ToString();
        }

        public static Type DeserializeType(string serializedType) {
            if (string.IsNullOrWhiteSpace(serializedType)) return null;
            if (s_cache.TryGetValue(serializedType, out var cached)) return cached;

            var result = DeserializeTypeInternal(serializedType);
            s_cache[serializedType] = result;
            return result;
        }

        private static Type DeserializeTypeInternal(string serializedType) {
            int nl = serializedType.IndexOf('\n');
            if (nl < 0) return DeserializeTypeDefinition(serializedType);

            var t = DeserializeTypeDefinition(serializedType[..nl]);
            int pointer = nl + 1;

            return DeserializeGenericTypeRecursively(t, serializedType, ref pointer);
        }

        private static int FindStripEnd(string str, int startIndex) {
            int endIndex = startIndex;
            int commaCounter = 0;

            while (++endIndex < str.Length) {
                char c = str[endIndex];
                if (c is ']' || c is ',' && ++commaCounter >= 3) break;
            }

            return endIndex;
        }

        private static Type DeserializeTypeDefinition(string serializedTypeDefinition) {
            return string.IsNullOrEmpty(serializedTypeDefinition) ? null : Type.GetType(serializedTypeDefinition, false);
        }

        private static Type DeserializeGenericTypeRecursively(Type def, string source, ref int pointer) {
            var types = def.GetGenericArguments();

            for (int i = 0; i < types.Length; i++) {
                int nl = source.IndexOf('\n', pointer);
                string line;

                if (nl < 0) {
                    line = source.Substring(pointer);
                    pointer = source.Length;
                } else {
                    line = source.Substring(pointer, nl - pointer);
                    pointer = nl + 1;
                }

                var t = DeserializeTypeDefinition(line);
                if (t == null) continue;

                types[i] = t.IsGenericType ? DeserializeGenericTypeRecursively(t, source, ref pointer) : t;
            }

            return def.MakeGenericType(types);
        }
    }

}
