using System;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintNodeMetaAttribute : Attribute {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Color { get; set; }
    }

}
