# MisterGames Blueprints 
>v0.3.1

A node-based visual scripting tool, implemented with structs, interfaces and generic methods to avoid boxing, without using reflection or code generation.

**Blueprint** is an alternative to the `MonoBehaviour` component script written in C#.
- Script logic is represented by nodes and connections between them;
- Script serialized fields are represented by a storage called `Blackboard`, where variables of different types can be stored. 

<img width="954" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/e61f4161-941f-4c7c-9c36-57c10692187e">

### Table of contents

- [Quick setup](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#quick-setup)
- [Main Features](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#main-features)
- [Core](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#core)
    - [Blueprint asset](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#1-blueprint-asset)
    - [Blueprint node](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#2-blueprint-node)
    - [Blueprint runner](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#3-blueprint-runner)
    - [Subgraphs](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#4-subgraphs)
    - [External subgraphs](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#5-external-subgraphs)
    - [Blackboard](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#6-blackboard)
- [Examples](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#examples)
    - [Implementing Finite State Machine using Blueprints](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#1-implementing-finite-state-machine-using-blueprints)
    - [Implementing door open/close using Blueprints](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#2-implementing-door-open-close-using-blueprints)
  
### Quick setup

1. Create scriptable object `BlueprintAsset`, edit blueprint with the Blueprint Editor.   
2. Add component `BlueprintRunner` to the game object to use the blueprint on the scene or inside a prefab
3. Setup scene references and serialized fields of the blueprint in `BlueprintRunner` using `Blackboard`

Any blueprint can be compiled and started in the Unity Editor within `BlueprintRunner` component without entering playmode, 
by pressing button "Compile & Start Blueprint" in the runner inspector.

https://github.com/theverymistergames/unity-common/assets/109593086/7b31fe4a-3d04-4fa9-85f3-d0098666f8c3

This blueprint will result in picked game object being disabled or enabled in a delay after runner starts. 

### Main features
- Blueprint **nodes**: implement custom nodes as a `struct` when possible to produce less garbage. `class` based implementation is also available 
- Blueprint **subgraphs**: blueprint asset can be used as a subgraph inside another blueprint, to create complex nodes
- Blueprint **references**: runtime graph can have links to another runtime graphs, using same interface as blueprint functions
- Blueprint **global storage**: in the runtime all the nodes from every running blueprint instance are stored in one place to improve memory management
- Blueprint **meta**: meta information for blueprint (node positions, ports, colors, etc.) is separated from data that will be used in the runtime
- Blueprint **editor**: using UI Toolkit to improve node rendering
- Blueprint **prototyping**: any blueprint can be launched within a blueprint runner without entering playmode 
- Blueprint **compilation**: connections between nodes are optimized during compilation, so helper nodes such as "Subgraph", "Pipe" or "Go To" don't add any overhead at runtime  

Demonstration of the blueprint connections optimization process for subgraph and other special nodes

![blueprint_link_inline](https://github.com/theverymistergames/unity-common/assets/109593086/e26f58a0-415a-47c6-a66c-e6cab836ecf5)

### Core

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
    BlueprintSources.IEnter<BlueprintNodeEnableGameObject>,
    BlueprintSources.ICloneable {}
```

Interface `IBlueprintNode` contains several methods, at least one method should be implemented: `CreatePorts(...)`. 
Other methods have default empty implementation.

```csharp
public interface IBlueprintNode {

    // Called in the editor to create node ports
    void CreatePorts(IBlueprintMeta meta, NodeId id);

    // Called in the editor to set default values into struct based nodes
    void OnSetDefaults(IBlueprintMeta meta, NodeId id) {}

    // Called in the editor when serialized fields of the node have been changed
    void OnValidate(IBlueprintMeta meta, NodeId id) {}

    // Called in the runtime at launch to initialize node internal stuff
    void OnInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {}

    // Called in the runtime when blueprint is about to be destroyed 
    void OnDeInitialize(IBlueprint blueprint, NodeToken token, NodeId root) {}
}
```

Special interfaces for nodes are:
- `IBlueprintEnter` for enter ports, to be able to react to enter port call
- `IBlueprintOutput<T>` and `IBluepintOutput` for output ports, so other nodes have interface to get output value
- `IBlueprintStartCallback` to react to `MonoBehaviour.Start` event at the component `BlueprintRunner`, where blueprint is launched
- `IBlueprintEnableCallback` to react to `MonoBehaviour.OnEnable` and `MonoBehaviour.OnDisable` events at the component `BlueprintRunner`, where blueprint is launched
- `IBlueprintConnectionCallback` to react to adding, deleting or changing connections of the node while blueprint is being edited
- `IBlueprintHashLink` to create hidden connections between hash-nodes with same hash
- `IBlueprintInternalLink` to create hidden internal links inside the node
- `IBlueprintCloneable` for `struct` based nodes to optimize node copy operation, can be added to node if it does not have fields serialized by reference.

#### 3. Blueprint runner

`BlueprintRunner` is a `MonoBehaviour` component, an enter point for any blueprint to launch on the specific scene. 
Each runner has overriden blackboard values for used blueprint assets. These values will be used at the runtime by blueprint runtime instance.

<img width="942" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/76b348de-00a7-471a-86ee-e7d9e8200211">

#### 4. Subgraphs

Blueprint can be used as a subgraph to create more nodes just within the Unity Editor.

Add node "Subgraph" to use blueprint asset as a subgraph, pick asset, and its external ports will be fetched. 
External ports are added into blueprint as nodes from "External" category.

<img width="951" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/4d3d20cf-6223-48e9-a239-09d542353ac0">

Here component `BlueprintRunner` at its `Start()` launches `Blueprint` asset (1), 
which has "Subgraph" node with `Blueprint_LogAsSubgraph` asset (2), 
which invokes log node (3) to call `Debug.Log()` with provided text.
Text string value is provided from the external input node `Text`, which is connected to blackboard property node (4) in `Blueprint`.

In the runtime blackboard property value is provided from `BlueprintRunner` blackboard (5).

#### 5. External subgraphs

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
See example [Implementing door open/close using Blueprints](https://github.com/theverymistergames/unity-common/blob/master/Blueprints/README.md#2-implementing-door-open-close-using-blueprints).

#### 6. Blackboard

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

### Examples

#### 1. Implementing Finite State Machine using Blueprints

FSM can be created in form of a Blueprint using fsm nodes: "Fsm State" and "Fsm Transition". 

<img width="545" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/eccc7db0-df03-43bb-9ae4-beea88d82e8a">

These nodes utilize several interfaces to be able to create states and transitions, and setup reactions for any state or transition: on enter/exit state, 
on start/finish transition. 

Let's create simple FSM to demonstrate states and transitions. To avoid too much connections between states we can add nodes "Go To" and "Go To (exit)",
which are connected during compilation, if labels are same.

<img width="770" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/32e81ca3-d98c-4199-9ee3-94baabfb4058">

When `BlueprintRunner` with this blueprint starts, `State 0` is entered first through "Start" => "Go To" nodes, and log "State 0 entered" is called. 
`State 0` has two debug transitions to `State 1` and `State 2`. 

Transition to `State 1` is active, so `State 1` is entered. `State 1` has two inactive transitions, so it remains entered. 

Real FSM has dynamically updated transitions. The following is an example how a character motion system can be implemented as FSM in Blueprints:
- `Blueprint_CharacterMotionFsm`: root blueprint, contains all states as connected subgraphs
- `Blueprint_CharacterMotionState_XXX`: subgraph blueprint for a motion state, contains transitions to possible states and reactions to apply state settings when needed

<img width="879" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/a6c01263-3e44-49e2-83d3-d5be25b86c0c">

<img width="879" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/64a26102-df98-4754-adb5-fc2baa2c05da">


#### 2. Implementing door open/close using Blueprints

Let's create a simple door mechanics using Blueprints. The goal is to be able to open/close the door by activating levers.

<img width="690" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/974fdbc9-314b-4df5-9ad6-ecba61be4a72">

There are 2 blueprint assets:
- `Blueprint_Door`, has external enter port "Toggle" for other blueprints to open or close door with tween animation
- `Blueprint_Lever`, also has external enter port "Toggle", and node "External Blueprint" with `Blueprint_Door` asset and reference to the door blueprint runner

When someone presses "Toggle" on lever (which can be replaced with actual interactive system to toggle levers), it triggers external port "Toggle" of the door blueprint and starts lever animation. Enter node "Toggle" inside door blueprint triggers door animation.

https://github.com/theverymistergames/unity-common/assets/109593086/82c9dcef-48a8-4ce7-89ef-0d750a7a63fa

## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`
- `MisterGames.Blueprints.RuntimeTests`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`MisterGames.Blackboards`](https://github.com/theverymistergames/unity-common/tree/master/Blackboards)
- [`Cysharp.UniTask`](https://github.com/Cysharp/UniTask)
