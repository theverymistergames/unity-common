namespace MisterGames.Character.View {

    public readonly struct CameraStateKey {

        public readonly int hash;
        public readonly int token;
        public readonly int index;

        public CameraStateKey(int hash, int token, int index) {
            this.hash = hash;
            this.token = token;
            this.index = index;
        }
    }

}
