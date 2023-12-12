# MisterGames Blueprints v0.3.1

A node-based visual scripting tool, implemented with structs, interfaces and generic methods to avoid boxing, without using reflection or code generation.

**Blueprint** is an alternative to the `MonoBehaviour` component script written in C#.
- Script logic is represented by nodes and connections between them;
- Script serialized fields are represented by a storage called `Blackboard`, where variables of different types can be stored. 

<img width="954" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/e61f4161-941f-4c7c-9c36-57c10692187e">

### Main features
- Blueprint **nodes**: implement custom nodes as a `struct` when possible to produce less garbage. `class` based implementation is also available 
- Blueprint **subgraphs**: blueprint asset can be used as a subgraph inside another blueprint, to create complex nodes
- Blueprint **references**: runtime graph can have links to another runtime graphs, using same interface as blueprint functions
- Blueprint **global storage**: in the runtime all the nodes from every running blueprint instance are stored in one place to improve memory management
- Blueprint **meta**: meta information for blueprint (node positions, ports, colors, etc.) is separated from data that will be used in the runtime
- Blueprint **compilation**: connections between nodes are optimized during compilation, so helper nodes such as "Subgraph", "Pipe" or "Go To" don't add any overhead at runtime  

Demonstration of the blueprint connections optimization process for subgraph and other special nodes:

![blueprint_link_inline](https://github.com/theverymistergames/unity-common/assets/109593086/e26f58a0-415a-47c6-a66c-e6cab836ecf5)

### Usage

1. Create scriptable object `BlueprintAsset`, edit blueprint with the Blueprint Editor.   
2. Add component `BlueprintRunner` to the game object to use the blueprint on the scene or inside a prefab
3. Setup scene references and serialized fields of the blueprint in `BlueprintRunner` using `Blackboard`

Any blueprint can be compiled and started in the Unity Editor on the `BlueprintRunner` component.

https://github.com/theverymistergames/unity-common/assets/109593086/7b31fe4a-3d04-4fa9-85f3-d0098666f8c3

This blueprint will result in picked game object being disabled or enabled in a delay after runner starts.

### Basics

#### 1. Blueprint asset

`BlueprintAsset` is a scriptable object that stores blueprint nodes, connections, and serialized properties in `Blackboard`.

<img width="944" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/71365110-6a6f-4213-9551-a6ef2876a60c">

This blueprint will enable or disable provided game object after delay.

#### 2. Blueprint node

Blueprint node is a serializable class or struct that implements node interface `IBlueprintNode` and has node attribute `[BlueprintNode(...)]`.
Each node can have serializable fields, ports to connect to the other nodes, and implement some special interfaces to support actions with ports.

<img width="325" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/b8f0afaf-ce8c-437d-a961-203eace43f38">

There are two categories of ports: 
- Data-based: input or output ports, with or without specific data type, to read values;
- Action-based: enter or exit ports, without data type, for void based calls (enter) and event-like subscriptions (exit).

Here is the implementation of the blueprint node "Enable GameObject":

```csharp
// Node attribute adds the node type to the node browser in the Blueprint Editor. 
[Serializable]
[BlueprintNode(Name = "Enable GameObject", Category = "GameObject", Color = BlueprintColors.Node.Actions)]
public struct BlueprintNodeEnableGameObject : IBlueprintNode, IBlueprintEnter {

    // Node can have serializable fields. 
    [SerializeField] private bool _isEnabled;

    // This method is called only in the Unity Editor to create node ports
    // that can be connected with other nodes in the Blueprint Editor.   
    public void CreatePorts(IBlueprintMeta meta, NodeId id) {
        meta.AddPort(id, Port.Enter("Apply"));
        meta.AddPort(id, Port.Input<GameObject>());
        meta.AddPort(id, Port.Input<bool>("Is Enabled"));
    }

    // This method is called when some node invokes its port,
    // and that port is connected to the enter port 0 "Apply" of this node.
    public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
        // Check if this is port 0 "Apply"
        if (port != 0) return;

        // Get target value to enable or disable from port 2 "Is Enabled",
        // default value is from serialized field _isEnabled
        bool isEnabled = blueprint.Read(token, port: 2, defaultValue: _isEnabled);

        // Get target game object to enable or disable from port 1 "GameObject",
        // default value is null, if no value provided, NullReferenceException will be thrown
        var gameObject = blueprint.Read<GameObject>(token, port: 1);

        // Result call of the node
        gameObject.SetActive(isEnabled);
    }
}

// BlueprintSourceEnableGameObject is a storage implementation for struct-based node BlueprintNodeEnableGameObject.
// At the runtime single instance of this storage is created to store BlueprintNodeLog nodes for every instantiated blueprint.
//
// To support enter ports of the node, the source implements IBlueprintEnter interface with default implementation:
// BlueprintSources.IEnter<BlueprintNodeEnableGameObject>. 
[Serializable]
public class BlueprintSourceEnableGameObject :
    BlueprintSource<BlueprintNodeEnableGameObject>,
    BlueprintSources.IEnter<BlueprintNodeEnableGameObject> {}
```

#### 3. Blackboard

Blackboard is a storage for blueprint variables. 
You can add a property with specified name and type and it will be displayed in the inspector like regular serialized field. 

Blackboard has implemented storages for: 
- General value types (`bool`, `float`, `int` etc.)
- Any type derived from `UnityEngine.Object` 
- Some other Unity types (`AnimationCurve`, `LayerMask`, `Color` etc)
- Any enum
- Any type that can be serialized by reference
- Array of any of types above. 

<img width="784" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/5e6c5d43-6f86-4070-b593-ae314ce361fc">

Default blackboard property values are set in the inspector of `BlueprintAsset`. When asset is picked in `BlueprintRunner`, a copy of the asset blackboard is created. Here you can override blackboard values and setup some scene references.

<img width="784" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/2126dfbe-c764-471d-a040-8687071936fb">

To get blackboard property value you can use Get Blackboard Property node. It has one dynamic output port, which fetches chosen blackboard property type.

<img width="484" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/f7a20634-82db-48cc-8970-dfbc62ced4ce">

#### 4. Blueprint runner

`BlueprintRunner` is a `MonoBehaviour` component, an enter point for any blueprint to launch on the specific scene. 
Each runner has overriden blackboard values for used blueprint assets. These values will be used at the runtime by blueprint runtime instance.

<img width="942" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/76b348de-00a7-471a-86ee-e7d9e8200211">

#### 5. Subgraphs

Blueprint can be used as a subgraph to create more nodes just within the Unity Editor.

Add node "Subgraph" to use blueprint asset as a subgraph, pick asset, and its external ports will be fetched. 
External ports are added into blueprint as nodes from "External" category.

<img width="951" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/4d3d20cf-6223-48e9-a239-09d542353ac0">

Here component `BlueprintRunner` at its `Start()` launches `Blueprint` asset (1), 
which has "Subgraph" node with `Blueprint_LogAsSubgraph` asset (2), 
which invokes log node (3) to call `Debug.Log()` with provided text.
Text string value is provided from the external input node `Text`, which is connected to blackboard property node (4) in `Blueprint`.

In the runtime blackboard property value is provided from `BlueprintRunner` blackboard (5).

#### 6. External subgraphs

Blueprint can have a link to some external `BlueprintRunner` and its root blueprint. 
Add node "External Blueprint", setup blackboard value for external runner, pick an asset that is used in that runner, and its external ports will be fetched.

<img width="951" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/9bae49ee-addf-4f20-9fff-82343607dfda">

Here component `BlueprintRunner` at its `Start()` launches `Blueprint` asset (1), 
which has "External Blueprint" node with `Blueprint_LogAsSubgraph` asset (2), 
which references an external runner via `Blackboard`. 
`Blackboard` on the original runner has field for an external runner (3), 
which points to the external runner (4).

In the runtime running instance of the external blueprint will be used. 
This allows to create connections between runners for complex behaviour.

#### 7. Blueprint compilation

## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`MisterGames.Blackboards`](https://github.com/theverymistergames/unity-common/tree/master/Blackboards)
