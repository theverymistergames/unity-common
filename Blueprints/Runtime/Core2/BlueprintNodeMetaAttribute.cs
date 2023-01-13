using System;

namespace MisterGames.Blueprints.Core2 {

    public sealed class BlueprintNodeMetaAttribute : Attribute {

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Color { get; set; } = BlueprintColors.Node.Default;

    }

}
