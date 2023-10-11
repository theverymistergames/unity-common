namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }

        private readonly IRuntimeBlueprintStorage _storage;
        private readonly IBlueprintFactoryStorage _factories;

        private RuntimeBlueprint2() { }

        public RuntimeBlueprint2(IRuntimeBlueprintStorage storage, IBlueprintFactoryStorage factories) {
            _factories = factories;
            _storage = storage;
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factories.GetFactory(factoryId).OnInitialize(this, id);
            }
        }

        public void DeInitialize() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factories.GetFactory(factoryId).OnDeInitialize(this, id);
            }
        }

        public void OnEnable() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetFactory(factoryId) is IBlueprintEnableDisable2 enableDisable) {
                    enableDisable.OnEnable(this, id);
                }
            }
        }

        public void OnDisable() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetFactory(factoryId) is IBlueprintEnableDisable2 enableDisable) {
                    enableDisable.OnDisable(this, id);
                }
            }
        }

        public void Start() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factories.GetFactory(factoryId) is IBlueprintStart2 start) {
                    start.OnStart(this, id);
                }
            }
        }

        public void GetLinks(long id, int port, out int index, out int count) {
            _storage.GetLinks(id, port, out index, out count);
        }

        public void Call(long id, int port) {
            _storage.GetLinks(id, port, out int index, out int count);
            int end = index + count;

            for (int i = index; i < end; i++) {
                var link = _storage.GetLink(i);
                BlueprintNodeAddress.Unpack(link.nodeId, out int factoryId, out _);

                if (_factories.GetFactory(factoryId) is not IBlueprintEnter2 enter) continue;

                enter.OnEnterPort(this, link.nodeId, link.port);
            }
        }

        public T Read<T>(long id, int port, T defaultValue = default) {
            _storage.GetLinks(id, port, out int index, out int count);
            return count > 0 ? Read(index, defaultValue) : defaultValue;
        }

        public T Read<T>(int linkIndex, T defaultValue = default) {
            var link = _storage.GetLink(linkIndex);
            BlueprintNodeAddress.Unpack(link.nodeId, out int factoryId, out _);

            return _factories.GetFactory(factoryId) switch {
                IBlueprintOutput2<T> outputT => outputT.GetOutputPortValue(this, link.nodeId, link.port),
                IBlueprintOutput2 output => output.GetOutputPortValue<T>(this, link.nodeId, link.port),
                _ => defaultValue
            };
        }
    }

}
