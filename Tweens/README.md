# MisterGames Tweens v0.1.1

Package contains component `TweenRunner`, that has options to create tween-like actions with the inspector.
Tween here represents an instant or lasting action, eg. delay or smooth material color change with specific easing.

Each tween is implemented to match these rules of execution:
- A tween can be played forwards and backwards, depending on the value of the last set playing direction
- Each tween has progress - a value between 0 and 1 inclusive
- `Wind` call represents progress change up to 1
- `Rewind` call represents progress change down to 0
- `Play` call invokes progress change from the current progress value in the current direction (when playing forward - progress is increasing up to 1)

`ITween` is an interface for tween imlementations. Note that this package uses [`Cysharp.UniTask`](https://github.com/Cysharp/UniTask): 
play and pause are implemented as awaitable method `Play` with passed `CancellationToken` to be able to pause at the current progress of the tween. 

```
public interface ITween {
  void Initialize(MonoBehaviour owner);
  void DeInitialize();

  UniTask Play(CancellationToken token);

  void Wind();                    // set progress at 1
  void Rewind();                  // set progress at 0
  void Invert(bool isInverted);   // set progress change direction
  void ResetProgress();           // set progress at 0 without notification
}
```

## Built-in serialized tween types
- `InstantTween` - has an array of instant actions (`ITweenInstantAction`)
- `DelayTween` - just a delay with duration
- `ProgressTween` - has an array of progress actions (`ITweenProgressAction`), duration and `EasingType` by which tween progress is being changed
- `TweenSequence` - represents sequential executing array of tweens, also has `bool Loop` and `bool Yoyo` options
- `TweenParallel` - represents parallel executing array of tweens

## Tween actions
1. `ITweenInstantAction` is invoked from tween instances of type `InstanceTween`

```
public interface ITweenInstantAction {
  void Initialize(MonoBehaviour owner);
  void DeInitialize();

  void InvokeAction();
}
```

2. `ITweenProgressAction` is invoked on each frame from tween instances of type `ProgressTween`

```
public interface ITweenProgressAction {
  void Initialize(MonoBehaviour owner);
  void DeInitialize();

  void OnProgressUpdate(float progress);
}
```

## Built-in serialized tween action implementations

### Instant:
- `TweenInstantActionLog` - for debug
- `TweenInstantActionReparentTransform` - for reparenting specific game object to the new parent
- `TweenInstantActionActivateGameObject` - for activating specific game object

### Continuous:
- `TweenProgressActionLog` - for debug
- `TweenProgressActionTransform` - to change position, rotation or scale of the specific game object
- `TweenProgressActionMaterialField` - to change value of the field or color on the specific material 
- `TweenProgressActionComponentField_Reflection` - for debug or fast prototyping, to change value of some field of the `UnityEngine.Component` using reflection

## Editor tween controls

`TweenRunner` inspector has controls to play tweens while in play mode. Controls button click invokes actions `Play`, `Pause`, `Rewind`, `Wind` on `TweenRunner`. 
Play direction buttons invokes `Invert(bool isInverted)` correspoding. 

Here is an example of using tween runner with in-editor debug:

https://user-images.githubusercontent.com/109593086/208439998-9f7f06a3-d954-49d0-8b03-b0b630112032.mp4

User runs several tweens with different actions:
1. Move and rotate game object from start to end position
2. Start a loop of of the point light intencity and material custom emission intencity changing

![image](https://user-images.githubusercontent.com/109593086/208440699-82c95c55-bcff-4319-b662-960ba8fb4179.png)

Examples of tween actions:

![image](https://user-images.githubusercontent.com/109593086/208441057-1e1c8e92-cf5a-4d9f-b80a-7de187d70618.png)

## Assembly definitions
- `MisterGames.Tweens`
- `MisterGames.Tweens.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`Cysharp.UniTask`](https://github.com/Cysharp/UniTask)
