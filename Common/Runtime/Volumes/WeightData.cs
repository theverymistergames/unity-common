namespace MisterGames.Common.Volumes {
    
    public readonly struct WeightData {
            
        public readonly float weight;
        public readonly int volumeId;
            
        public WeightData(float weight, int volumeId) {
            this.weight = weight;
            this.volumeId = volumeId;
        }
    }
    
}