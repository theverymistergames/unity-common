﻿# MisterGames Character v0.3.4

## Features
- Uses state machines for speed and pose
- Simulates ```Rigidbody``` physics with ```CharacterController```
- Advanced ground normal detection (for cases when standing on several surfaces)
- Crouch in jump to climb high shelves
- Based on my previous controller: [Asset store link](https://assetstore.unity.com/packages/templates/systems/mv-fps-controller-181699)

[Demo](https://gitlab.com/theverymistergames/readme-data/-/blob/master/character/character.mp4)

## Assembly definitions
- MisterGames.Character

## Dependencies
- MisterGames.Common
- MisterGames.Input
- MisterGames.Fsm
- MisterGames.Dbg
- MisterGames.View

## Installation
- Add [MisterGames Common](https://gitlab.com/theverymistergames/common/) package
- Top menu MisterGames -> Packages, add packages: 
  - [Input](https://gitlab.com/theverymistergames/input/)
  - [Fsm](https://gitlab.com/theverymistergames/fsm/)
  - [Dbg](https://gitlab.com/theverymistergames/dbg/)
  - [View](https://gitlab.com/theverymistergames/view/)
  - [Character](https://gitlab.com/theverymistergames/character/)