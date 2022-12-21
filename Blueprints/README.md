# MisterGames Blueprints v0.1.1

> :warning: Package needs refactoring to reduce heap allocations

A tool for visual scripting without using reflection or code generation. 

## Core

### Blueprint

`Blueprint` basically is a scriptable object (has no dependencies on a separate scene) with some runtime and editor data.
Data is presented by:

1. A list of blueprint nodes

2. A blackboard to be able to create references to game objects on a scene

### Blueprint node

Blueprint node has in and out ports. `BlueprintNode` is basic abstract class to implement node, it has one abstract member: method `CreatePorts`.

```
class BlueprintNodeImplementation : BlueprintNode {

  protected override IReadOnlyList<Port> CreatePorts() {
    throw new NotImplementedException();
  }
}
```

Ports are to create connections between nodes. There are two types of ports:

1. Flow port (enter and exit)

When flow port called from inside the node by protected method `void BlueprintNode.Call(int port)`, node searches links to flow ports of other nodes and invokes method
`void IBlueprintEnter.Enter(int port)`, so to receive calls from enter port, node needs to implement interface `IBlueprintEnter`:

```
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

To create and call exit port:

```
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

2. Data port (input and output)

Input can be read by calling protected method `T BlueprintNode.Read<T>(int port, T default)`, this call invokes method `T IBlueprintGetter<out T>.Get(int port)` from a linked port of another node (a port must be an output of the same type). 

```
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
      float value = Read(port: 2, defaultValue: default);
      
      // Call port "Exit" when called port "Enter"
      Call(port: 1);
    }
  }
}
```

To has an output port, node must implement interface `IBlueprintGetter<out T>`. Node can has multiple output ports, 
so the method `T IBlueprintGetter<out T>.Get(int port)` has argument `int port` to check if the output port index is correct.

```
class BlueprintNodeImplementation : BlueprintNode, IBlueprintEnter, IBlueprintGetter<bool> {
  
  // a field to store output result, that is calculated on "Enter" call
  bool outputCache;
  
  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Enter("Enter"),              // index of port is 0
    Port.Exit("Exit"),                // index of port is 1
    Port.Input<float>("Input float"), // index of port is 2
    Port.Output<bool>("Output bool")  // index of port is 3
  };
  
  void IBlueprintEnter.Enter(int port) {
    if (port == 0) {
      // Read input value when called port "Enter" with default value. It will be used later.
      // Default value can be injected. Eg. with serialized field.
      float value = Read(port: 2, defaultValue: default);
      
      // Write a result for input value
      outputCache = value > 0f;
      
      // Call port "Exit" when called port "Enter"
      Call(port: 1);
    }
  }
  
  void IBlueprintGetter<bool>.Get(int port) {
    if (port == 3) {
      return outputCache;
    }
    
    return default;
  }
}
```

## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
