using System;
using UnityEngine;
using Type = System.Type;

namespace MisterGames.Common.Data {

    [Serializable]
    public sealed class SerializedType : IEquatable<SerializedType>, IEquatable<Type> {

        [SerializeField] private string _type;

        private struct SerializedTypeData
        {
            public string typeName;
            public string genericTypeName;
            public bool isGeneric;
        }

        private SerializedType() { }

        public SerializedType(Type type) {
            _type = SerializeType(type);
        }

        public static implicit operator Type(SerializedType serializedType) {
            if (serializedType is null || 
                string.IsNullOrEmpty(serializedType._type) || 
                IsGeneric(serializedType._type)
            ) {
                return null;
            }

            return DeserializeType(serializedType._type);
        }

        public bool Equals(Type other) {
            return other is not null && (Type) this == other;
        }

        public bool Equals(SerializedType other) {
            return other is not null && (Type) this == (Type) other;
        }

        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) ||
                   obj is SerializedType otherSerializedType && Equals(otherSerializedType) ||
                   obj is Type otherType && Equals(otherType);
        }

        public override int GetHashCode() {
            var type = (Type) this;
            return type is not null ? type.GetHashCode() : 0;
        }

        public static bool operator ==(SerializedType serializedType, Type type) {
            return (Type) serializedType == type;
        }

        public static bool operator !=(SerializedType serializedType, Type type) {
            return !(serializedType == type);
        }

        public static bool operator ==(SerializedType serializedType0, SerializedType serializedType1) {
            return (Type) serializedType0 == (Type) serializedType1;
        }

        public static bool operator !=(SerializedType serializedType0, SerializedType serializedType1) {
            return !(serializedType0 == serializedType1);
        }

        public override string ToString() {
            var type = (Type) this;
            return type is not null ? type.ToString() : "<null>";
        }

        public static string SerializeType(Type type) {
            var data = new SerializedTypeData();
            if (type == null) return string.Empty;

            data.typeName = string.Empty;
            data.isGeneric = type.ContainsGenericParameters;

            if (data.isGeneric && type.IsGenericType) {
                data.typeName = ToShortTypeName(type.GetGenericTypeDefinition());
            }
            else {
                int num = data.isGeneric ? type.IsArray ? 1 : 0 : 0;
                data.typeName = num == 0 ? !data.isGeneric ? ToShortTypeName(type) : "T" : "T[]";
            }

            return ToString(data);
        }

        public static Type DeserializeType(string type) {
            try {
                return Type.GetType(SplitTypeString(type).typeName, true);
            }
            catch (TypeLoadException) {
                return null;
            }
        }

        private static string ToString(SerializedTypeData data) {
            return data.typeName + "#" + data.genericTypeName + "#" + (data.isGeneric ? "1" : "0");
        }

        private static string ToShortTypeName(Type type) {
            string assemblyQualifiedName = type.AssemblyQualifiedName;
            return string.IsNullOrEmpty(assemblyQualifiedName) 
                ? string.Empty 
                : StripAllFromTypeNameString(
                    StripAllFromTypeNameString(
                        StripAllFromTypeNameString(assemblyQualifiedName, ", Version"), 
                        ", Culture"
                    ), 
                    ", PublicKeyToken"
                );
        }
        
        private static string StripAllFromTypeNameString(string str, string toStrip) {
            for (int index = str.IndexOf(toStrip); index != -1; index = str.IndexOf(toStrip, index)) {
                str = StripTypeNameString(str, index);
            }
            return str;
        }

        private static string StripTypeNameString(string str, int index) {
            int index1 = index + 1;
            while (index1 < str.Length && str[index1] != ',' && str[index1] != ']') {
                ++index1;
            }
            return str.Remove(index, index1 - index);
        }
        
        private static bool IsGeneric(string serializedTypeString) {
            return !string.IsNullOrEmpty(serializedTypeString) && 
                   serializedTypeString[serializedTypeString.Length - 1] == '1';
        }
        
        private static SerializedTypeData SplitTypeString(string serializedTypeString) {
            bool isGeneric = !string.IsNullOrEmpty(serializedTypeString) 
                ? IsGeneric(serializedTypeString) 
                : throw new ArgumentException("Cannot parse serialized type string, it is empty.");
            
            string typeName = serializedTypeString.Substring(0, serializedTypeString.IndexOf('#'));
            int typeNameLength = typeName.Length;

            int genericTypeNameStartIndex = typeNameLength + 1;
            int genericTypeNameLength = serializedTypeString.IndexOf('#', typeNameLength + 1) - typeNameLength - 1;
            string genericTypeName = serializedTypeString.Substring(genericTypeNameStartIndex, genericTypeNameLength);
            
            return new SerializedTypeData {
                isGeneric = isGeneric,
                typeName = typeName,
                genericTypeName = genericTypeName
            };
        }
    }

}
