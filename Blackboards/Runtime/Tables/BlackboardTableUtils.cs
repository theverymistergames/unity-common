using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MisterGames.Blackboards.Tables {

    public static class BlackboardTableUtils {

        private const string EDITOR = "editor";
        private static Type[] BlackboardTableTypeCache;

        public static bool IsSupportedElementType(Type t) {
            return GetBlackboardTableType(t) != null;
        }

        public static Type GetBlackboardTableType(Type t) {
            if (!IsValidElementType(t)) return null;

            var tableTypes = GetBlackboardTableTypes();
            if (FindBlackboardTableType(tableTypes, t) is {} direct) return direct;

            bool isArray = t.IsArray;
            if (isArray) t = t.GetElementType()!;

            t = typeof(UnityEngine.Object).IsAssignableFrom(t) ? typeof(UnityEngine.Object) :
                t.IsClass || t.IsInterface ? typeof(object) :
                t.IsEnum ? typeof(Enum) :
                t;

            if (isArray) t = t.MakeArrayType();
            return FindBlackboardTableType(tableTypes, t);
        }

        private static bool IsValidElementType(Type t) {
            if (t == null) return false;
            if (t.IsArray) t = t.GetElementType()!;

            return t.IsVisible && (t.IsPublic || t.IsNestedPublic) && !t.IsGenericType &&
                   t.FullName is not null && !t.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase);
        }

        private static Type FindBlackboardTableType(IEnumerable<Type> tableTypes, Type elementType) {
            return tableTypes.FirstOrDefault(t => t.GetCustomAttribute<BlackboardTableAttribute>(false)?.elementType == elementType);
        }

        private static Type[] GetBlackboardTableTypes() {
            BlackboardTableTypeCache ??=
#if UNITY_EDITOR
                UnityEditor.TypeCache.GetTypesDerivedFrom<IBlackboardTable>()
#else
                AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(assembly => !assembly.FullName.Contains(EDITOR, StringComparison.OrdinalIgnoreCase))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IBlackboardTable).IsAssignableFrom(t))
#endif
                .Where(t =>
                    typeof(IBlackboardTable).IsAssignableFrom(t) &&
                    (t.IsPublic || t.IsNestedPublic) &&
                    t.IsVisible &&
                    !t.IsAbstract &&
                    !t.IsGenericType &&
                    Attribute.IsDefined(t, typeof(BlackboardTableAttribute)) &&
                    Attribute.IsDefined(t, typeof(SerializableAttribute))
                )
                .ToArray();

            return BlackboardTableTypeCache;
        }
    }

}
