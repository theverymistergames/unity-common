using System;

namespace MisterGames.Blueprints {

    public sealed class BlueprintNodeAttribute : Attribute {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Color { get; set; }
    }

}
