# MisterGames Dbg v0.2.0

## Features
- Developer Console
- Debug draw: line, ray, line array, circle, sphere, cylinder, capsule, text:

```
DbgRay.Create().From(start).Dir(dir).Color(Color.blue).Arrow(0.1f).Time(1f).Draw();
DbgPointer.Create().Position(start).Size(0.3f).Color(Color.yellow).Draw();
DbgCapsule.Create().From(start).To(end).Radius(radius).Color(Color.cyan).Draw();
DbgText.Create().Text(text).Position(start).Draw();
...
```

## Assembly definitions
- MisterGames.Dbg
- MisterGames.Dbg.Editor

## Dependencies
- MisterGames.Common
- MisterGames.Common.Editor
- MisterGames.Input
- Unity.TextMeshPro (embedded)

## Installation 
- Add [MisterGames Common](https://gitlab.com/theverymistergames/common) package
- Top menu MisterGames -> Packages, add packages: 
  - [Input](https://gitlab.com/theverymistergames/input/)
  - [Dbg](https://gitlab.com/theverymistergames/dbg/)