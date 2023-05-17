using System;
using UnityEngine;

namespace MisterGames.Common.Dependencies {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FetchDependenciesAttribute : PropertyAttribute {

        public readonly string dependencyPropertyName;

        public FetchDependenciesAttribute(string dependencyPropertyName) {
            this.dependencyPropertyName = dependencyPropertyName;
        }

        public FetchDependenciesAttribute() {
            dependencyPropertyName = null;
        }
    }

}
