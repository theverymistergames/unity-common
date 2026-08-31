using System;
using UnityEngine;

namespace MisterGames.Common.Attributes {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute {

        /// <summary>
        /// Show implementations from editor assemblies and namespaces, which are filtered out by default:
        /// they can not be serialized by a build, so only an editor only field can afford them.
        /// </summary>
        public readonly bool includeEditor;

        public SubclassSelectorAttribute(bool includeEditor = false) {
            this.includeEditor = includeEditor;
        }
    }

}
