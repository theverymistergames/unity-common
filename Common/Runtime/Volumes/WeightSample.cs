namespace MisterGames.Common.Volumes {
    
    public readonly struct WeightSample {

        public readonly float weight;
        public readonly int volumeId;
        
        public WeightSample(float weight, int volumeId) {
            this.weight = weight;
            this.volumeId = volumeId;
        }
    }
    
}