# MisterGames Common v0.5.1

## Features

### Attributes
- `[ReadOnly]`, `[BeginReadOnlyGroup]`, `[EndReadOnlyGroup]` for serializable properties and arrays
- `[EmbeddedInspector]` to display any `UnityEngine.Object` inline (eg. ScriptableObject)
- `[SubclassSelector]` to use with `[SerializeReference]` attribute, allows setting
  abstract class or interface implementation in the inspector

### Data
- `ScriptableSingleton` - for implementing singleton scriptable objects for Editor or debug purposes
- `SerializedDictionary<TKey, TValue>`
- `Optional<TData>` - optional field with custom editor
- `Pair<TDataA, TDataB>` - pair of types with custom editor
- Pool `ObjectPool<T>`, runtime prefab pool `PrefabPool : MonoBehaviour`

### Editor-only related
- `ToolbarExtender` - a tool to place custom controls around main toolbar.
  Setup by calling `ToolbarExtender.OnLeftToolbarGUI(Action)`/`ToolbarExtender.OnRightToolbarGUI(Action)`

## Assembly definitions
- `MisterGames.Common`
- `MisterGames.Common.Editor`
- `MisterGames.Common.RuntimeTests`
