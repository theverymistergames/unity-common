# MisterGames Tick v0.2.0

Alternative for `MonoBehaviour.Update` to allow any object to subscribe to update of the specific player loop stage and
decrease amount of the `MonoBehaviour.Update` calls per frame.

This system has single `TimeSourcesRunner`, which receives `MonoBehaviour` update messages and 
calls update methods for its subscribers - objects of type that implements `IUpdate` interface.

## Player loop stage

Player loop stage represents 1) order of update during frame and 2) time scale mode.

```csharp
public enum PlayerLoopStage {
  PreUpdate,
  Update,
  UnscaledUpdate,
  LateUpdate,
  FixedUpdate,
}
```

`UnscaledUpdate` is an option for implementing in-game menu, this stage is not affcted by `Time.timeScale`. 

## Time source

`TimeSource` is a container for storing and updating `IUpdate` subscribers, implements interface `ITimeSource`:

```csharp
public interface ITimeSource {
  float DeltaTime { get; }
  float TimeScale { get; set; }
  bool IsPaused { get; set; }

  bool Subscribe(IUpdate sub);
  bool Unsubscribe(IUpdate sub);
}
```

`TimeSourcesRunner : MonoBehaviour` updates all time sources, one for each `PlayerLoopStage`. 
Time source for certain `PlayerLoopStage` can be received by calling static method `TimeSources.Get(PlayerLoopStage)`, 
that returns object of type `ITimeSource`. 

## Implementing alternative of `MonoBehaviour.Update`

```csharp
class SomeUpdatableMonoBehaviour : MonoBehaviour, IUpdate {
  
  [SerializeField] PlayerLoopStage stage;
  
  void OnEnable() {
    TimeSources.Get(stage).Subscribe(this);
  }

  void OnDisable() {
    TimeSources.Get(stage).Unsubscribe(this);
  }
  
  void IUpdate.OnUpdate(float dt) {
    // ...
  }
}
```

```csharp
class SomeMonoBehaviourWithUpdatableFields : MonoBehaviour {
  
  [SerializeField] PlayerLoopStage stage;
  [SerializeField] SomeSerializedUpdatable someSerializedUpdatable;
  
  IUpdate someNonSerializedUpdatable = new SomeUpdatable();
  
  void OnEnable() {
    TimeSources.Get(stage).Subscribe(someSerializedUpdatable);
    TimeSources.Get(stage).Subscribe(someNonSerializedUpdatable);
  }

  void OnDisable() {
    TimeSources.Get(stage).Unsubscribe(someSerializedUpdatable);
    TimeSources.Get(stage).Unsubscribe(someNonSerializedUpdatable);
  }
}
```

```csharp
class SomeUpdatable : IUpdate {
  
  void IUpdate.OnUpdate(float dt) {
    // ...
  }
}
```

## Assembly definitions
- `MisterGames.Tick`
- `MisterGames.Tick.Editor`
- `MisterGames.Tick.RuntimeTests`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
