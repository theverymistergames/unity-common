# MisterGames Blueprints v0.2.1

A tool for visual scripting without using reflection or code generation. 

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

There is a category called `External` in the node finder, these are built-in nodes mostly for creation subgraphs. 

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


## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
