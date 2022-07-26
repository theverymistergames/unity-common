using System;

namespace MisterGames.Common.Data {
    
    public static class SerializedType {
        
        private struct SerializedTypeData
        {
            public string typeName;
            public string genericTypeName;
            public bool isGeneric;
        }
     
        public static Type FromString(string serializedTypeString) {
            return string.IsNullOrEmpty(serializedTypeString) || IsGeneric(serializedTypeString) 
                ? null 
                : Type.GetType(SplitTypeString(serializedTypeString).typeName, true);
        }
        
        public static string ToString(Type type)
        {
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
        
        private static string ToString(SerializedTypeData data) {
            return data.typeName + "#" + data.genericTypeName + "#" + (data.isGeneric ? "1" : "0");
        }

        private static string ToShortTypeName(Type type)
        {
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
        
        private static string StripAllFromTypeNameString(string str, string toStrip)
        {
            for (int index = str.IndexOf(toStrip); index != -1; index = str.IndexOf(toStrip, index)) {
                str = StripTypeNameString(str, index);
            }
            return str;
        }

        private static string StripTypeNameString(string str, int index)
        {
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