namespace MisterGames.Blueprints.Core2 {

    public ref struct LinkReader {

        private readonly RuntimeBlueprint2 _blueprint;
        private readonly int _first;
        private int _index;

        public LinkReader(RuntimeBlueprint2 blueprint, int index) {
            _blueprint = blueprint;
            _first = index;
            _index = -1;
        }

        public T Read<T>(T defaultValue = default) {
            return _blueprint.ReadLink(_index, defaultValue);
        }

        public bool MoveNext() {
            if (_index < 0) {
                _index = _first;
                return _index >= 0;
            }

            int next =_blueprint.GetNextLink(_index);
            if (next < 0) return false;

            _index = next;
            return true;
        }
    }

}
