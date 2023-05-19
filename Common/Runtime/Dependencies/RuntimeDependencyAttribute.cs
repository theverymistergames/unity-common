using System;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class RuntimeDependencyAttribute : PropertyAttribute {

        public readonly Type type;

        public RuntimeDependencyAttribute(Type type) {
            this.type = type;
        }
    }

}
