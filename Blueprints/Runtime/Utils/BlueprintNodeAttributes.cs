using System;

namespace MisterGames.Blueprints {

    public class BlueprintNodeAttribute : Attribute {

        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Color { get; set; } = BlueprintColors.Node.Default;

    }

}