namespace MisterGames.Character.View {

    public readonly struct WeightedValue<T> {
        
        public readonly float weight;
        public readonly T value;
        
        public WeightedValue(float weight, T value) {
            this.weight = weight;
            this.value = value;
        }
    }
    
}
