using System;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class FetchDependenciesAttribute : PropertyAttribute {

        public readonly string propertyPath;

        public FetchDependenciesAttribute(string propertyPath) {
            this.propertyPath = propertyPath;
        }

        public FetchDependenciesAttribute() {
            propertyPath = null;
        }
    }

}
