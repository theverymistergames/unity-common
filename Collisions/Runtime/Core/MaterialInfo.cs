namespace MisterGames.Collisions.Core {
    
    public readonly struct MaterialInfo {

        public static readonly MaterialInfo Empty = new();
        
        public readonly int materialId;
        public readonly float weight;
        
        public MaterialInfo(int materialId, float weight) {
            this.materialId = materialId;
            this.weight = weight;
        }
    }
    
}