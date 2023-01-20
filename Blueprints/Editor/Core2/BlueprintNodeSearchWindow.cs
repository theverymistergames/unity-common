using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Blueprints.Core2;
using MisterGames.Common.Editor.Reflect;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core2 {

    public sealed class BlueprintNodeSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<BlueprintsView.NodeCreationData> onNodeCreationRequest = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {

        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is not Type type) return false;

            var data = new BlueprintsView.NodeCreationData {
                node = Activator.CreateInstance(type) as BlueprintNode,
                position = context.screenMousePosition,
            };

            onNodeCreationRequest.Invoke(data);
            return true;
        }

        private static bool HasBlueprintNodeMetaAttribute(Type type) {
            return GetBlueprintNodeMetaAttribute(type) != null;
        }

        private static BlueprintNodeMetaAttribute GetBlueprintNodeMetaAttribute(Type type) {
            return type.GetCustomAttribute<BlueprintNodeMetaAttribute>(false);
        }

        private static CategoryTree.Meta GetCategoryNodeMeta(Type type) {
            var nodeMetaAttr = GetBlueprintNodeMetaAttribute(type);

            string name = string.IsNullOrEmpty(nodeMetaAttr.Name) ? type.Name : nodeMetaAttr.Name;
            string category = string.IsNullOrEmpty(nodeMetaAttr.Category) ? "" : nodeMetaAttr.Category;

            return new CategoryTree.Meta { name = name, category = category };
        }
    }

}
