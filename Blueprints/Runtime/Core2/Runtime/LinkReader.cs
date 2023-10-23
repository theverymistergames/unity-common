namespace MisterGames.Blueprints.Core2 {

    public ref struct LinkReader {

        private readonly RuntimeBlueprint2 _blueprint;
        private readonly IRuntimeLinkStorage _links;
        private readonly int _first;
        private int _index;

        public LinkReader(RuntimeBlueprint2 blueprint, IRuntimeLinkStorage links, NodeId id, int port) {
            _blueprint = blueprint;
            _links = links;
            _first = _links.GetFirstLink(id.source, id.node, port);
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

            int next =_links.GetNextLink(_index);
            if (next < 0) return false;

            _index = next;
            return true;
        }
    }

}
