# MisterGames Collisions v0.1.1

Collision detection related scripts.

## Features
- Abstract `CollisionDetector : MonoBehaviour` to build custom collision detector components
- `CollisionUtils` to sort, filter and get specisic results from raycast operations

`CollisionDetector` implementations:
- `FrameRaycaster`
- `FrameSphereCaster`
- `FrameUiRaycaster`
- `FrameCollisionDetectorGroup`

## Assembly definitions
- `MisterGames.Collisions`
- `MisterGames.Collisions.RuntimeTests`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
