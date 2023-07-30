namespace MisterGames.Character.View {

    public readonly struct CameraStateKey {

        public readonly int index;
        public readonly int hash;

        public CameraStateKey(int index, int hash) {
            this.index = index;
            this.hash = hash;
        }
    }

}
