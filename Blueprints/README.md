# MisterGames Blueprints v0.3.1

A node-based tool for visual scripting without using reflection or code generation. 

### Basics

#### 1. Blueprint 

`Blueprint` is an alternative to the `MonoBehaviour` component script written in C#.

- Script logic is represented by nodes and connections between them;
- Script serialized fields are represented by a storage called `Blackboard`, where variables of different types can be stored. 

#### 2. Blueprint node

Blueprint node is a serializable class or struct that implements node interface `IBlueprintNode` and has node attribute `[BlueprintNode(...)]`.
Each node can have ports to connect to the other nodes, and implement some special interfaces to support actions with ports.

<img width="389" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/a815c534-b70a-4ae7-86d7-756fcad6a995">

There are two categories of ports: 
- Data-based: input or output ports, with or without specific data type, to read values;
- Void-based: enter or exit ports, without data type, for void based calls (enter) and event-like subscriptions (exit).

#### 3. Blueprint asset

`Blueprint` can be saved into scriptable object called `BlueprintAsset`.

<img width="574" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/a189b869-ecda-4ec7-bb6f-08c44d36a9f8">

<img width="335" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/320bc0ae-ea35-407b-b0e2-35e6d7790f91">

This blueprint calls `Debug.Log()` with serialized text from the blackboard variable, when the `MonoBehaviour.Start` method is called in the runner.

#### 4. Blueprint runner

`BlueprintRunner` is a `MonoBehaviour` component, an enter point for any blueprint to launch on the specific scene. 
Each runner has overriden blackboard values for used blueprint assets. These values will be used at the runtime by blueprint runtime instance.

<img width="333" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/a2c90a49-8b90-4891-8161-19bff8abe971">

#### Blueprint node implementation

```csharp
// Node attribute adds the node type to the node browser in the Blueprint Editor. 
[Serializable]
[BlueprintNode(Name = "Log", Category = "Debug", Color = BlueprintColors.Node.Debug)]
public struct BlueprintNodeLog : IBlueprintNode, IBlueprintEnter {

  // Node can have serializable fields. 
  [SerializeField] private string _defaultText;

  // This method is called only in the Unity Editor to create node ports
  // that can be connected with other nodes in the Blueprint Editor.   
  public void CreatePorts(IBlueprintMeta meta, NodeId id) {
    meta.AddPort(id, Port.Enter(name: "Log"));
    meta.AddPort(id, Port.Input<string>(name: "Text"));
    meta.AddPort(id, Port.Exit(name: "After Log"));
  }

  // This method is called when some node invokes its port,
  // and that port is connected to the enter port 0 "Log" of this node.
  public void OnEnterPort(IBlueprint blueprint, NodeToken token, int port) {
    // Port 0 is the first enter port "Log" 
    if (port != 0) return;

    // Port 1 is input port "Text"
    string text = blueprint.Read<string>(token, port: 1, defaultValue: _defaultText);
    Debug.Log(text);

    // Port 2 is exit port "Exit"
    blueprint.Call(token, port: 2);
  }
}

// BlueprintSourceLog is a storage implementation for struct-based node BlueprintNodeLog.
// At the runtime single instance of this storage is created to store BlueprintNodeLog nodes
// for every instantiated blueprint.
//
// To support enter ports of the node, the source implements IBlueprintEnter interface with default implementation:
// BlueprintSources.IEnter<BlueprintNodeLog>. 
[Serializable]
public class BlueprintSourceLog :
  BlueprintSource<BlueprintNodeLog>,
  BlueprintSources.IEnter<BlueprintNodeLog> {}
```



## Core

### Blueprint Asset

`BlueprintAsset` is a scriptable object with some runtime and editor data. 

<img width="600" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/1c0c5ee0-54f0-4e30-b0c1-c676b68cb241">

The data is presented by:

1. Nodes
2. Links between node ports
3. Blackboard: exposed variables of a blueprint

`BlueprintAsset` can be launched from component `BlueprintRunner` or from Blueprint Subgraph node.

### Blueprint Node

To create new type of node, create a class that implements abstract class `BlueprintNode`, add `[Serializable]` and `[BlueprintNodeMeta]` attributes to the class.

`BlueprintNode` is a plain class, which holds references to connected nodes and some abstract members to implement.
 
```csharp
[Serializable]
public abstract class BlueprintNode {

    protected internal RuntimePort[] Ports;

    public abstract Port[] CreatePorts();

    public virtual void OnInitialize(IBlueprintHost host) {}
    public virtual void OnDeInitialize() {}

    public virtual void OnValidate() {}
}
```

Note that node has `RuntimePort` struct array to store links to the connected nodes at runtime. This array is filled at the compilation stage of the `BlueprintRunner` during `Awake` call. The method `CreatePorts` returns an array of `Port` struct, which contains all the meta data about the port (name, direction, data type). This method is not used at runtime, only in the editor.

 `[BlueprintNodeMeta]` attribute must be used to add meta data to the created blueprint node, like name, category for the node finder, colors.

```csharp
[Serializable]
[BlueprintNodeMeta(Name = "Test", Category = "Test Nodes")]
public class BlueprintNodeTest : BlueprintNode {

    public override Port[] CreatePorts() {
        return Array.Empty<Port>();
    }
}
```

After compilation the node appears in the node finder and it can be added to the blueprint.

<img width="331" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/15ecc132-f6a2-4b8d-8a44-b4d5e2702f6b">

A node can have serialized fields, which are exposed in the node inspector.

```csharp
[Serializable]
[BlueprintNodeMeta(Name = "Test", Category = "Test Nodes")]
public class BlueprintNodeTest : BlueprintNode {

    [SerializeField] private string _parameter;

    public override Port[] CreatePorts() {
        return Array.Empty<Port>();
    }
}
```

<img width="213" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/2a837a74-4b05-46b0-baaf-4560370e8ef4">

#### Ports and links

To add connection between nodes, first you need to add ports. There are two types of ports:

1. Flow port (enter and exit)

Flow ports are just like void method calls without parameters.

- To add enter flow port, node needs implement `IBlueprintEnter` interface and have an enter port in array created in `CreatePorts` method
- To add exit flow port, just add exit port to array in `CreatePorts` method

Exit flow ports can be called within a node via `Ports` field of a `BlueprintNode` class.

```csharp
[Serializable]
[BlueprintNodeMeta(Name = "Test", Category = "Test Nodes")]
public class BlueprintNodeTest : BlueprintNode, IBlueprintEnter {

    [SerializeField] private string _parameter;

    public override Port[] CreatePorts() => new [] {
        Port.Enter("Enter"), // port 0
        Port.Exit("Exit")    // port 1
    };

    public void OnEnterPort(int port) {
        // Enter port 0 called
        if (port == 0) {
            Debug.Log($"Parameter is {_parameter}");

            // Call exit port 1
            Ports[1].Call();
        }
    }
}
```

In this implementation exit port (1) will be called inside the enter port (0) call.

2. Data port (input and output)

Data ports are used to pass some data. Data port stores `System.Type` in a string form to be able to validate connections between ports by type.

- To add input port, just add input port to array in `CreatePorts` method
- To add output port, add output port to array in `CreatePorts` method and make the node implement `IBlueprintOutput<T>` interface

```csharp
[Serializable]
[BlueprintNodeMeta(Name = "Test", Category = "Test Nodes")]
public class BlueprintNodeTest : BlueprintNode, IBlueprintEnter, IBlueprintOutput<string> {

    [SerializeField] private string _parameter;

    public override Port[] CreatePorts() => new [] {
        Port.Enter("Enter"),                   // port 0
        Port.Exit("Exit"),                     // port 1
        Port.Input<string>("Input String"),    // port 2
        Port.Output<string>("Output String"),  // port 3
    };

    public void OnEnterPort(int port) {
        // Enter port 0 called
        if (port == 0) {
            
            // Read input port 2
            string input = Ports[2].Get<string>();
            Debug.Log($"Input is {input}, parameter is {_parameter}");
            
            // Call exit port 1
            Ports[1].Call();
        }
    }

    public string GetOutputPortValue(int port) {
        // Someone trying to read port 3 of type string
        if (port == 3) {

            // Read input port 2
            string input = Ports[2].Get<string>();
            
            // Return string output
            return $"Input is {input}, parameter is {_parameter}";
        }
        
        return default;
    }
}
```

Added ports will be displayed in the node inspector.

<img width="391" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/cec86e7a-9d46-409d-a981-d22c16b47a2a">

### Blueprint Runner

`BlueprintAsset` can be launched from `BlueprintRunner : MonoBehaviour`. At runtime `BlueprintRunner` creates a runtime copy of a `BlueprintAsset` with overrided values, which can be set up via inspector.

<img width="944" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/4ff04126-63f8-47f4-95f8-2167e8dd3311">

### Blueprint Subraph

`BlueprintAsset` can also be launched from a built-in subgraph blueprint node, which takes a `BlueprintAsset` as a serialized field and exposes all its ports, that are intended to be exposed. 

<img width="493" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/67d97b52-554e-477c-9c77-3621f865dbb3">

Let's add a new exposed port to the previous `BlueprintAsset` example:

<img width="624" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/f52ee6d9-a245-46ab-b843-f9acf3d353b2">

Subgraph node fetches new exposed ports:

<img width="508" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/c52ce285-bbcd-49d2-913f-9650c5fdc8c1">

Now if we set up a `BlueprintRunner` with `BlueprintAsset` with one or more subgraphs, inspector of `BlueprintRunner` contains all nested blackboards:

<img width="401" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/f2baaaac-6579-4acb-8a8b-d7d633c0af02">

Subgraph node cannot hold a reference to the host `BlueprintAsset` and cannot contain reference loops to prevent recursion. 

## External nodes

There is a category called `External` in the node finder, these are built-in nodes mostly for subgraph creation. 

<img width="746" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/160e4d7b-3e27-4f27-a8c6-9dcc2159d0b2">

- Start node: has a single exit port, called on `MonoBehaviour.Start` of the `BlueprintRunner`, or called by exposed port titled `On Start` on a subgraph node when blueprint is used as a subgraph
- Subgraph node: displays exposed ports of a subgraph blueprint asset

Other external nodes with serialized port name only create exposed port when `BlueprintAsset` is used in subgraph node.

- Enter node: has a single exit port
- Exit node: has a single enter port
- Input node: has a single output port
- Output node: has a single input port

Input and output nodes has dynamic data ports to be able to fetch any connected data type.

<img width="567" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/e3439506-a4ba-47ce-8607-6222aaf872d5">

This `BlueprintAsset` is displayed as following when used as subgraph:

<img width="646" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/55db8bc2-76c2-40a8-8081-40b61f9eaf49">

### Blackboard

Blackboard is a storage for blueprint variables. You can add a property with specified name and type. 
Blackboard has storages for main value types (`bool`, `float`, `int` etc.), some Unity value types (`AnimationCurve`, `LayerMask`, `Color` etc), any enum, and for any managed type, also it is possible to create an array of any of those types. 

Blackboard Editor provides inspector for all properties.

<img width="784" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/5e6c5d43-6f86-4070-b593-ae314ce361fc">

Default blackboard property values are set in the inspector of `BlueprintAsset`. When asset is picked in `BlueprintRunner`, a copy of the asset blackboard is created. Here you can override blackboard values and setup some scene references.

<img width="784" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/2126dfbe-c764-471d-a040-8687071936fb">

To get blackboard property value you can use Get Blackboard Property node. It has one dynamic output port, which fetches chosen blackboard property type.

<img width="484" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/f7a20634-82db-48cc-8970-dfbc62ced4ce">


## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`MisterGames.Blackboards`](https://github.com/theverymistergames/unity-common/tree/master/Blackboards)
