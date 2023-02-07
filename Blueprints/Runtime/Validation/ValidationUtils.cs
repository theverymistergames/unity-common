using System;

namespace MisterGames.Blueprints.Validation {

    internal static class ValidationUtils {

        public static bool HasGenericInterface(Type subjectType, Type interfaceType, Type genericArgumentType) {
            var interfaces = subjectType.GetInterfaces();
            bool hasInterface = false;

            for (int i = 0; i < interfaces.Length; i++) {
                var x = interfaces[i];
                if (x.IsGenericType &&
                    x.GetGenericTypeDefinition() == interfaceType &&
                    x.GenericTypeArguments.Length == 1 &&
                    x.GenericTypeArguments[0] == genericArgumentType
                   ) {
                    hasInterface = true;
                    break;
                }
            }

            return hasInterface;
        }
    }

}
