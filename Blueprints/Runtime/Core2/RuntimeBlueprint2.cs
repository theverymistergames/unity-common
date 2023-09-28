namespace MisterGames.Blueprints.Core2 {

    public sealed class RuntimeBlueprint2 : IBlueprint {

        public IBlueprintHost2 Host { get; private set; }

        private readonly IBlueprintStorage _storage;
        private readonly IBlueprintFactoryStorage _factoryStorage;

        public RuntimeBlueprint2(IBlueprintStorage storage, IBlueprintFactoryStorage factoryStorage) {
            _factoryStorage = factoryStorage;
            _storage = storage;
        }

        public void Initialize(IBlueprintHost2 host) {
            Host = host;

            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factoryStorage.GetFactory(factoryId).Node.OnInitialize(this, id);
            }
        }

        public void DeInitialize() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                _factoryStorage.GetFactory(factoryId).Node.OnDeInitialize(this, id);
            }
        }

        public void OnEnable() {
            var nodes = _storage.Nodes;
            int count = nodes.Count;

            for (int i = 0; i < count; i++) {
                long id = nodes[i];
                BlueprintNodeAddress.Unpack(id, out int factoryId, out int _);

                if (_factoryStorage.GetFactory(factoryId).Node is IBlueprintEnableDisable2 enableDisable) {
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

                if (_factoryStorage.GetFactory(factoryId).Node is IBlueprintEnableDisable2 enableDisable) {
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

                if (_factoryStorage.GetFactory(factoryId).Node is IBlueprintStart2 start) {
                    start.OnStart(this, id);
                }
            }
        }

        public ref T GetData<T>(long id) where T : struct {
            BlueprintNodeAddress.Unpack(id, out int factoryId, out int nodeId);
            return ref _factoryStorage.GetFactory(factoryId).GetData<T>(nodeId);
        }

        public void GetLinks(long id, int port, out int index, out int count) {
            _storage.GetLinks(id, port, out index, out count);
        }

        public void Call(long id, int port) {
            _storage.GetLinks(id, port, out int index, out int count);
            int end = index + count;

            for (int i = index; i < end; i++) {
                var link = _storage.GetLink(i);
                var node = _factoryStorage.GetFactory(link.factoryId).Node;

                if (node is IBlueprintEnter2 enter) enter.OnEnterPort(this, link.GetNodeAddress(), link.port);
            }
        }

        public T Read<T>(long id, int port, T defaultValue = default) {
            _storage.GetLinks(id, port, out int index, out int count);
            return count > 0 ? Read(index, defaultValue) : defaultValue;
        }

        public T Read<T>(int linkIndex, T defaultValue = default) {
            var link = _storage.GetLink(linkIndex);
            var node = _factoryStorage.GetFactory(link.factoryId).Node;

            return node switch {
                IBlueprintOutput2<T> outputR => outputR.GetOutputPortValue(this, link.GetNodeAddress(), link.port),
                IBlueprintOutput2 output => output.GetOutputPortValue<T>(this, link.GetNodeAddress(), link.port),
                _ => defaultValue
            };
        }
    }

}
