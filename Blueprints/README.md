# MisterGames Blueprints v0.1.1

> :warning: Package needs refactoring to reduce heap allocations and scriptable objects overuse.

A tool for visual scripting without using reflection or code generation. 

## Core

### Blueprint

`Blueprint` basically is a scriptable object (has no dependencies on a separate scene) with some runtime and editor data.
Data is presented by:

1. A list of blueprint nodes

2. A blackboard to be able to create references to game objects on a scene

<img width="833" alt="image" src="https://user-images.githubusercontent.com/109593086/208853464-567ce757-d215-46b5-ba61-3cddb0b17db1.png">

`Blueprint` can be started with `BlueprintRunner : MonoBehaviour`:

<img width="356" alt="image" src="https://user-images.githubusercontent.com/109593086/208853990-3cd43a0a-fd8e-478f-aca2-3eb0de86c549.png">

### Blueprint node implementation

To create new type of node, create a class that implements abstract class `BlueprintNode`.

`BlueprintNode` is also a scriptable object with data about in and out ports and action or data access implementation. 
 It is basic abstract class to implement node with one abstract member: method `CreatePorts`. 
 
 `[BlueprintNode]` attribute can be used to add meta data to the created blueprint node, like name, category for Node finder, colors.

```
[BlueprintNode(Name = "Implementation")]
class BlueprintNodeImplementation : BlueprintNode {

  protected override IReadOnlyList<Port> CreatePorts() {
    return Array.Empty<Port>(); 
  }
}
```

<img width="98" alt="image" src="https://user-images.githubusercontent.com/109593086/208855574-46b0187e-324b-4c63-9250-c807640d3e51.png">

#### Ports

Ports are to create connections between nodes. There are two types of ports:

1. Flow port (enter and exit)

When flow port called from inside the node by protected method `void BlueprintNode.Call(int port)`, node searches links to flow ports of other nodes and invokes method
`void IBlueprintEnter.Enter(int port)`. So, to receive calls from enter ports, node needs:
- to have a port created by `Port.Enter(...)` call
- to implement interface `IBlueprintEnter`

```
[BlueprintNode(Name = "Implementation")]
class BlueprintNodeImplementation : BlueprintNode, IBlueprintEnter {
  
  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Enter("Enter") // index of port is 0
  };
  
  void IBlueprintEnter.Enter(int port) {
    if (port == 0) {
      // called port "Enter"
    }
  }
}
```

<img width="105" alt="image" src="https://user-images.githubusercontent.com/109593086/208855989-4978059d-d668-4913-96a6-a57ae1b573be.png">

To create and call exit port the node needs to have port created by `Port.Exit(...)` call:

```
[BlueprintNode(Name = "Implementation")]
class BlueprintNodeImplementation : BlueprintNode, IBlueprintEnter {
  
  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Enter("Enter"),  // index of port is 0
    Port.Exit("Exit")     // index of port is 1
  };
  
  void IBlueprintEnter.Enter(int port) {
    if (port == 0) {
      // Call port "Exit" when called port "Enter"
      Call(port: 1);
    }
  }
}
```

<img width="109" alt="image" src="https://user-images.githubusercontent.com/109593086/208856168-de4f2cd9-d0b9-439f-b01d-9ac9fff1f39e.png">

2. Data port (input and output)

Input can be read by calling protected method `T BlueprintNode.Read<T>(int port, T default)`, this call invokes method `T IBlueprintGetter<out T>.Get(int port)` from a linked port of another node (a port must be an output of the same type to create a link). 

```
[BlueprintNode(Name = "Implementation")]
class BlueprintNodeImplementation : BlueprintNode, IBlueprintEnter {
  
  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Enter("Enter"),              // index of port is 0
    Port.Exit("Exit"),                // index of port is 1
    Port.Input<float>("Input float")  // index of port is 2
  };
  
  void IBlueprintEnter.Enter(int port) {
    if (port == 0) {
      // Read input value when called port "Enter" with default value. It will be used later.
      // Default value can be injected. Eg. with serialized field.
      float value = Read(port: 2, defaultValue: 0f);
      
      // Call port "Exit" when called port "Enter"
      Call(port: 1);
    }
  }
}
```

<img width="137" alt="image" src="https://user-images.githubusercontent.com/109593086/208857401-b7fad41f-4695-4e65-ace6-a41ca6a86e60.png">

To have an output port, the node must implement interface `IBlueprintGetter<out T>`. Node can have multiple output ports, 
so method `T IBlueprintGetter<out T>.Get(int port)` has argument `int port` to check if the output port index is correct.

```
[BlueprintNode(Name = "Implementation")]
class BlueprintNodeImplementation : BlueprintNode, IBlueprintEnter, IBlueprintGetter<bool> {
  
  // a field to store output result, that is calculated on "Enter" call
  bool outputCache;
  
  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Enter("Enter"),                           // index of port is 0
    Port.Exit("Exit"),                             // index of port is 1
    Port.Input<float>("Input float"),              // index of port is 2
    Port.Output<bool>("Output bool: input >= 0?")  // index of port is 3
  };
  
  void IBlueprintEnter.Enter(int port) {
    if (port == 0) {
      // Read input value when called port "Enter" with default value. It will be used later.
      // Default value can be injected. Eg. with serialized field.
      float value = Read(port: 2, defaultValue: 0f);
      
      // Write a result for input value
      outputCache = value >= 0f;
      
      // Call port "Exit" when called port "Enter"
      Call(port: 1);
    }
  }
  
  bool IBlueprintGetter<bool>.Get(int port) {
    if (port == 3) {
      return outputCache;
    }
    
    return default;
  }
}
```

<img width="205" alt="image" src="https://user-images.githubusercontent.com/109593086/208858330-6199d36a-a091-490a-9a25-80a83d1684fe.png">

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
