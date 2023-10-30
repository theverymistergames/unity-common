namespace MisterGames.Blueprints.Compile {

    public readonly struct RuntimeLink {

        public readonly BlueprintNode node;
        public readonly int port;

        public RuntimeLink(BlueprintNode node, int port) {
            this.node = node;
            this.port = port;
        }

        public void Call() {
            if (node is IBlueprintEnter enter) enter.OnEnterPort(port);
        }

        public R Get<R>(R defaultValue = default) => node switch {
            IBlueprintOutput<R> outputR => outputR.GetOutputPortValue(port),
            IBlueprintOutput output => output.GetOutputPortValue<R>(port),
            _ => defaultValue
        };
    }

}
