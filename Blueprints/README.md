# MisterGames Blueprints v0.2.1

A tool for visual scripting without using reflection or code generation. 

## Core

### Blueprint Asset

`BlueprintAsset` basically is a scriptable object with some runtime and editor data. 

<img width="600" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/1c0c5ee0-54f0-4e30-b0c1-c676b68cb241">


The data is presented by:

1. Nodes
2. Links between node ports
3. Blackboard: exposed variables of a blueprint

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

#### Blueprint Node Ports

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

    public override Port[] CreatePorts() => new [] {
        Port.Enter("Enter"), // port 0
        Port.Exit("Exit")    // port 1
    };

    public void OnEnterPort(int port) {
        // Enter port 0 called
        if (port == 0) {
            
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
            Debug.Log(input);
            
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
            return $"Hello, {input}!";
        }
        
        return default;
    }
}
```

Added ports will be displayed in the node inspector.

<img width="207" alt="image" src="https://github.com/theverymistergames/unity-common/assets/109593086/001f5a43-92e0-4840-81b3-a9c6d4bac7b0">

## Built-in nodes

There is a category called `Exposed` in the node finder, these are built-in nodes to create a connection between blueprint and its runtime environment (ie. resolve links between nodes, links to the scene created with blackboard, links to the subgraphs and other). 

<img width="660" alt="image" src="https://user-images.githubusercontent.com/109593086/208861458-b021fc85-2bad-446d-beb4-09d7d023165f.png">

- Start node: has a single exit port, called on `MonoBehaviour.Start` call of the `BlueprintRunner`, or called on the corresponding port on a subgraph node when blueprint is used as a subgraph node parameter
- Finish node: has a single enter port, creates corresponding port on a subgraph node when blueprint is used as a subgraph node parameter
- Enter node: has a single exit port and port name parameter, called on the call of the corresponding port on a subgraph node when blueprint is used as a subgraph node parameter
- Exit node: has a single exit port and port name parameter, creates corresponding port on a subgraph node when blueprint is used as a subgraph node parameter
- Input node: has a single output port and port name parameter, returns a value of the corresponding input port on a subgraph node when blueprint is used as a subgraph node parameter
- Output node: has a single input port and port name parameter, returns specified value from the corresponding output port on a subgraph node when blueprint is used as a subgraph node parameter

- Subgraph node: has ports created by selected as a subgraph node parameter blueprint

<img width="348" alt="image" src="https://user-images.githubusercontent.com/109593086/208865456-c78d3d37-c397-48fd-b1f1-99f8ec5f4800.png">

Here is an example of a subgraph blueprint usage for moving specific object into position A if condition is met, or otherwise into position B:

Blueprint:

<img width="730" alt="image" src="https://user-images.githubusercontent.com/109593086/208869031-b75e6456-ade5-4202-a45f-8f7adf82a5f2.png">

Usage of the blueprint as a parameter in a subgraph node:

<img width="389" alt="image" src="https://user-images.githubusercontent.com/109593086/208869559-0e993aa5-20b2-48e9-a21c-90c8a602f152.png">

## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
