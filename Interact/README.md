# MisterGames Interact v0.1.1

Package provides scripts to create interactive game objects.

- `Interactive` component: marks game object as interactive, has detection settings, provides API to subscribe to events for detection of the interactive object by interactive user
- `InteractiveUser` component: raycasts in the given direction with purpose of interactive object detection, provides API to subscribe to events related with detection of interactive objects
- `InteractStrategy` with input and distance settings determines detection settings
- Contains some interactive-related scripts, eg. `InteractiveCursor`, `InteractiveGrab`, `InteractiveDrawer`

https://user-images.githubusercontent.com/109593086/208623223-05d34504-08b2-41ab-a552-2dcf2eed3d8a.mp4

## Assembly definitions
- `MisterGames.Interact`
- `MisterGames.Interact.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Input`](https://github.com/theverymistergames/unity-common/tree/master/Input)
- [`MisterGames.Dbg`](https://github.com/theverymistergames/unity-common/tree/master/Dbg)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`MisterGames.Splines`](https://github.com/theverymistergames/unity-common/tree/master/Splines)
- [`MisterGames.Collisions`](https://github.com/theverymistergames/unity-common/tree/master/Collisions)
- [`Unity.Splines`](https://docs.unity3d.com/Packages/com.unity.splines@2.2/manual/index.html)
- [`Unity.Mathematics`](https://docs.unity3d.com/Packages/com.unity.mathematics@1.0/manual/index.html)
- [`Cysharp.UniTask`](https://github.com/Cysharp/UniTask)
