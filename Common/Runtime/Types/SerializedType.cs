using System;
using UnityEngine;

namespace MisterGames.Common.Types {

    [Serializable]
    public sealed class SerializedType : IEquatable<SerializedType>, IEquatable<Type> {

        [SerializeField] private string _type;

        private SerializedType() { }

        public SerializedType(Type type) {
            _type = SerializeType(type);
        }

        public static implicit operator Type(SerializedType serializedType) {
            return DeserializeType(serializedType?._type);
        }

        public bool Equals(Type other) {
            return this == other;
        }

        public bool Equals(SerializedType other) {
            return this == other;
        }

        public override bool Equals(object obj) {
            if (obj == null) return string.IsNullOrWhiteSpace(_type);

            return ReferenceEquals(this, obj) ||
                   obj is SerializedType s && this == s ||
                   obj is Type t && this == t;
        }

        public override int GetHashCode() {
            var type = (Type) this;
            return type is not null ? type.GetHashCode() : 0;
        }

        public static bool operator ==(SerializedType serializedType, Type type) {
            return string.IsNullOrWhiteSpace(serializedType?._type) ? type is null : serializedType._type == SerializeType(type);
        }

        public static bool operator !=(SerializedType serializedType, Type type) {
            return !(serializedType == type);
        }

        public static bool operator ==(SerializedType serializedType0, SerializedType serializedType1) {
            string t0 = serializedType0?._type;
            string t1 = serializedType1?._type;
            return string.IsNullOrWhiteSpace(t0) && string.IsNullOrWhiteSpace(t1) || t0 == t1;
        }

        public static bool operator !=(SerializedType serializedType0, SerializedType serializedType1) {
            return !(serializedType0 == serializedType1);
        }

        public override string ToString() {
            return ((Type) this)?.ToString();
        }

        private const string TO_STRIP_START = ", Version";

        public static string SerializeType(Type type) {
            if (type == null) return null;

            string typeName = type.AssemblyQualifiedName;
            if (string.IsNullOrEmpty(typeName)) return null;

            for (
                int i = typeName.IndexOf(TO_STRIP_START, StringComparison.Ordinal);
                i >= 0;
                i = typeName.IndexOf(TO_STRIP_START, i, StringComparison.Ordinal)
            ) {
                typeName = StripTypeNameString(typeName, i);
            }

            return typeName;
        }

        public static Type DeserializeType(string serializedType) {
            if (string.IsNullOrEmpty(serializedType)) return null;
            if (!serializedType.Contains('\n')) return DeserializeTypeDefinition(serializedType);

            ReadOnlySpan<string> serializedTypes = serializedType.Split('\n');
            if (serializedTypes.Length == 0) return DeserializeTypeDefinition(serializedType);

            var t = DeserializeTypeDefinition(serializedTypes[0]);
            if (serializedTypes.Length == 1) return t;

            int pointer = 1;
            return DeserializeGenericTypeRecursively(t, serializedTypes, ref pointer);
        }

        private static string StripTypeNameString(string str, int startIndex) {
            int endIndex = startIndex;
            int commaCounter = 0;

            while (++endIndex < str.Length) {
                char c = str[endIndex];
                if (c is ']' || c is ',' && ++commaCounter >= 3) break;
            }

            return startIndex >= endIndex ? str : str.Remove(startIndex, endIndex - startIndex);
        }

        private static Type DeserializeTypeDefinition(string serializedTypeDefinition) {
            return string.IsNullOrEmpty(serializedTypeDefinition) ? null : Type.GetType(serializedTypeDefinition, false);
        }

        private static Type DeserializeGenericTypeRecursively(Type def, ReadOnlySpan<string> serializedTypes, ref int pointer) {
            var types = def.GetGenericArguments();

            for (int i = 0; i < types.Length; i++) {
                var t = DeserializeTypeDefinition(serializedTypes[pointer++]);
                if (t == null) continue;

                types[i] = t.IsGenericType ? DeserializeGenericTypeRecursively(t, serializedTypes, ref pointer) : t;
            }

            return def.MakeGenericType(types);
        }
    }

}
