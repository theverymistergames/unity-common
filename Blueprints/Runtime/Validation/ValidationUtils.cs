#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Linq;

namespace MisterGames.Blueprints.Validation {

    internal static class ValidationUtils {

        public static Type GetGenericInterface(Type subjectType, Type interfaceType, Type genericArgumentType) {
            return subjectType
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == interfaceType &&
                    x.GenericTypeArguments.Length == 1 &&
                    x.GenericTypeArguments[0] == genericArgumentType
                );
        }
    }

}

#endif
