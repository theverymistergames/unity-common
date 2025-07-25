﻿#if DEVELOPMENT_BUILD || UNITY_EDITOR

using System;
using System.Linq;

namespace MisterGames.Blueprints.Validation {

    internal static class ValidationUtils {

        public static Type GetGenericInterface(Type subjectType, Type interfaceType, params Type[] genericArguments) {
            if (subjectType == null || interfaceType == null) return null;

            return subjectType
                .GetInterfaces()
                .FirstOrDefault(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == interfaceType &&
                    Equals(x.GenericTypeArguments, genericArguments)
                );
        }

        private static bool Equals(Type[] a, Type[] b) {
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) return false;
            }

            return true;
        }
    }

}

#endif
