# MisterGames Blueprints v0.1.1

> :warning: Package needs refactoring to reduce heap allocations

A tool for visual scripting without using reflection or code generation. 

## Core

`BlueprintNode` is basic abstract class to implement node. It has one abstract member: method `CreatePorts`, which needs to be overrided in any blueprint node.

```
class BlueprintNodeImplementation : BlueprintNode {

  protected override IReadOnlyList<Port> CreatePorts() => new List<Port> {
    Port.Input<float>("A"),
    Port.Input<float>("A"),
    Port.Input<float>("B"),
    Port.Output<float>()
  };
  
  // ...
}
```

## Assembly definitions
- `MisterGames.Blueprints`
- `MisterGames.Blueprints.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
