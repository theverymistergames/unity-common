namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        private readonly IBlueprintFactoryStorage _factoryStorage;
        private readonly IBlueprintLinkStorage _linkStorage;

        public RuntimeBlueprint2(IBlueprintFactoryStorage factoryStorage, IBlueprintLinkStorage linkStorage) {
            _factoryStorage = factoryStorage;
            _linkStorage = linkStorage;
        }

        public ref T GetData<T>(long id) where T : struct {
            BlueprintNodeAddress.Parse(id, out int factoryId, out int nodeId);
            return ref _factoryStorage.GetFactory(factoryId).GetData<T>(nodeId);
        }

        public void Call(long id, int port) {
            _linkStorage.GetLinks(id, port, out int index, out int count);
            int end = index + count;

            for (int i = index; i < end; i++) {
                var link = _linkStorage.GetLink(i);
                var node = _factoryStorage.GetFactory(link.factoryId).Node;

                if (node is IBlueprintEnter2 enter) enter.OnEnterPort(link.port, this, link.GetNodeAddress());
            }
        }

        public T Read<T>(long id, int port, T defaultValue = default) {
            _linkStorage.GetLinks(id, port, out int index, out int count);
            if (count <= 0) return defaultValue;

            var link = _linkStorage.GetLink(index);
            var node = _factoryStorage.GetFactory(link.factoryId).Node;

            return node switch {
                IBlueprintOutput2<T> outputR => outputR.GetOutputPortValue(port, this, link.GetNodeAddress()),
                IBlueprintOutput2 output => output.GetOutputPortValue<T>(port, this, link.GetNodeAddress()),
                _ => defaultValue
            };
        }
    }

}
