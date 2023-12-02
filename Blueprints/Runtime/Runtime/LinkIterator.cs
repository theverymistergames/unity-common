namespace MisterGames.Blueprints.Runtime {

    public struct LinkIterator {

        private readonly RuntimeBlueprint _blueprint;
        private readonly NodeToken _token;
        private readonly int _port;
        private int _index;

        public LinkIterator(RuntimeBlueprint blueprint, NodeToken token, int port) {
            _blueprint = blueprint;
            _token = token;
            _port = port;
            _index = -1;
        }

        public void Call() {
            _blueprint.CallLink(_index, _token.caller);
        }

        public T Read<T>(T defaultValue = default) {
            return _blueprint.ReadLink(_index, _token.caller, defaultValue);
        }

        public bool MoveNext() {
            if (_index < 0) {
                _index = _blueprint.GetFirstLink(_token.node, _port);
                return _index >= 0;
            }

            int next = _blueprint.GetNextLink(_index);
            if (next < 0) return false;

            _index = next;
            return true;
        }
    }

}
