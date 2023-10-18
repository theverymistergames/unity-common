using MisterGames.Blueprints.Core2;

namespace Core {

    public class RuntimeBlueprintTests {

        public void AddNodes() {
            var factory = new BlueprintFactory();

            int sourceId = factory.GetOrCreateSource(typeof(BlueprintSourceTest0));
            var source = factory.GetSource(sourceId);

            int nodeId0 = source.AddNode();
            int nodeId1 = source.AddNode();

            long id0 = BlueprintNodeAddress.Pack(sourceId, nodeId0);
            long id1 = BlueprintNodeAddress.Pack(sourceId, nodeId1);

            var runtimeBlueprint = new RuntimeBlueprint2(factory, 2, 0, 0);

        }

    }

}
