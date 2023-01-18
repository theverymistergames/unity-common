using System;
using System.Collections.Generic;
using System.Reflection;
using MisterGames.Blueprints.Core2;
using MisterGames.Common.Editor.Reflect;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.Core2 {

    public struct NodeCreationData {
        public Vector2 position;
        public Type type;
    }

    public sealed class BlueprintSearchWindow : ScriptableObject, ISearchWindowProvider {

        public Action<NodeCreationData> onNodeCreationRequest = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return SearchTree.Create()
                .Add(SearchTree.Header("Create node"))
                .Add(CategoryTree.CreateSearchTree<BlueprintNode>(GetCategoryNodeMeta, HasBlueprintNodeMetaAttribute))
                .Build();
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                var data = new NodeCreationData {
                    type = type,
                    position = context.screenMousePosition,
                };

                onNodeCreationRequest.Invoke(data);
                return true;
            }
            return false;
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
