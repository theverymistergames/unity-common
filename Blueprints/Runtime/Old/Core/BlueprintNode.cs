using System;
using MisterGames.Blueprints.Compile;

namespace MisterGames.Blueprints {

    [Serializable]
    public abstract class BlueprintNode {

        protected internal RuntimePort[] Ports;

        public abstract Port[] CreatePorts();
        public virtual void OnInitialize(IBlueprintHost host) {}
        public virtual void OnDeInitialize() {}
        public virtual void OnValidate() {}
    }

}
