using System;
using System.Collections.Generic;
using MisterGames.Blueprints.Ports;
using MisterGames.Common.Data;
using UnityEngine;

namespace MisterGames.Blueprints.Core2 {

    [Serializable]
    public struct BlueprintNodeMeta {

        [SerializeField] private string _serializedNodeType;
        public Type nodeType {
            get => SerializedType.FromString(_serializedNodeType);
            set => _serializedNodeType = SerializedType.ToString(value);
        }

        public List<Port> ports;
        public Vector3 position;

    }

}
