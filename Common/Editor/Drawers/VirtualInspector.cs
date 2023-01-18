using System;
using UnityEditor;
using UnityEngine;

namespace MisterGames.Common.Editor.Drawers {

    public sealed class VirtualInspector : ScriptableObject {

        [SerializeReference] private object _data;

        public Action<SerializedProperty> OnGUI { get; private set; }

        public static VirtualInspector Create(object data, Action<SerializedProperty> onGUI = null) {
            var instance = CreateInstance<VirtualInspector>();

            instance.OnGUI = onGUI ?? SerializedPropertyExtensions.Draw;
            instance._data = data;

            return instance;
        }
    }

}
