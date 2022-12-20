# MisterGames ConsoleCommandsLib v0.1.1

Library of [developer console](https://github.com/theverymistergames/unity-common/tree/master/Dbg#developer-console) modules with console commands.

## Console modules

### `HeroConsoleModule`
- `hero/spawns` prints character spawn points on the current scene
- `hero/spawn <spawn_point_name>` performs respawn of the character at the spawn point with passed name
- `hero/spawni <spawn_point_index>` performs respawn of the character at the spawn point with passed index, index can be retrieved from `hero/spawns` command

### `SceneConsoleModule`
- `scenes/list` prints list of all scenes from `ScenesStorage`
- `scenes/load <scene_name>` performs load of the scene with passed name
- `scenes/loadi <scene_index>` performs load of the scene with passed index, index can be retrieved from `scenes/list` command
- `scenes/unload <scene_name>` performs unload of the scene with passed name
- `scenes/unloadi <scene_index>` performs unload of the scene with passed index, index can be retrieved from `scenes/list` command

## Assembly definitions
- `MisterGames.ConsoleCommandsLib`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`MisterGames.Dbg`](https://github.com/theverymistergames/unity-common/tree/master/Dbg)
- [`MisterGames.Scenes`](https://github.com/theverymistergames/unity-common/tree/master/Scenes)
- [`MisterGames.Character`](https://github.com/theverymistergames/unity-common/tree/master/Character)
