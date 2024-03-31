using System;
using MisterGames.Blueprints.Meta;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MisterGames.Blueprints.Editor.View {
    
    public sealed class BlueprintGroupView : Group {

        public Action<int> OnPositionChanged = delegate {  };
        public readonly int id;

        public BlueprintGroupView(BlueprintMeta meta, int id) {
            this.id = id;
            viewDataKey = $"__{nameof(BlueprintGroupView)}_{id}";

            var group = meta.GroupStorage.GetGroup(id);
            title = group.name;
            SetPosition(new Rect(group.position, new Vector2(100, 100)));
        }
     
        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            OnPositionChanged.Invoke(id);
        }
    }
    
}