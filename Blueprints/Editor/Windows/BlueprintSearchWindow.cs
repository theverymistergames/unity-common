﻿using System;
 using System.Collections.Generic;
 using MisterGames.Blueprints.Core;
 using MisterGames.Blueprints.Editor;
 using MisterGames.Common.Editor.Reflect;
 using UnityEditor.Experimental.GraphView;
 using UnityEngine;

 namespace MisterGames.Fsm.Editor.Windows {

    internal struct NodeCreationData {
        public Vector2 position;
        public string name;
        public Type type;
    }

    internal class BlueprintSearchWindow : ScriptableObject, ISearchWindowProvider {

        internal Action<NodeCreationData> onNodeCreationRequest = delegate {  };

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
            return SearchTree.Create()
                .Add(SearchTree.Header("Create node"))
                .Add(CategoryTree.CreateSearchTree<BlueprintNode>(GetCategoryNodeMeta, NodeMeta.HasAttribute))
                .Build();
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            if (searchTreeEntry.userData is Type type) {
                var data = new NodeCreationData {
                    type = type,
                    position = context.screenMousePosition,
                    name = GetNodeName(type)
                };
                
                onNodeCreationRequest.Invoke(data);
                return true;
            }
            return false;
        }

        private static CategoryTree.Meta GetCategoryNodeMeta(Type type) {
            var data = NodeMeta.From(type);
            return new CategoryTree.Meta { name = data.name, category = data.category, };
        }

        private static string GetNodeName(Type type) {
            var data = NodeMeta.From(type);
            return data.name;
        }

    }

}