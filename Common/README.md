# MisterGames Common v0.5.1

## Features

### Attributes
- `[ReadOnly]`, `[BeginReadOnlyGroup]`, `[EndReadOnlyGroup]` for serializable properties and lists
- `[EmbeddedInspector]` to display any `UnityEngine.Object` inline (eg. SO)
- `[SubclassSelector]` to use with `[SerializeReference]` attribute, allows setting abstract class or interface implementation in the inspector

### Data
- `Blackboard` - multiple type objects storage with custom editor
- `ScriptableSingleton` - for implementing singleton scriptable objects for Editor or debug purposes 
- `ScriptableObjectStorage` - for accessing any `ScriptableObject` within runtime for Editor or debug purposes
- `[Serializable] Map<TKey, TValue>` - serializable dictionary with custom editor
- `[Serializable] Observable<TData>` - observable field with custom editor
- `[Serializable] Optional<TData>` - optional field with custom editor 
- `[Serializable] Pair<TDataA, TDataB>` - pair of types with custom editor
- `ObjectDataMap<TData>` - runtime dictionary for `UnityEngine.Object` as keys
- Pool `ObjectPool<T>`, runtime prefab pool `PrefabPool : MonoBehaviour`

### Editor-only related
- Toolbar extender - a tool to place custom contols around main toolbar
- Editor coroutines extensions

## Assembly definitions
- `MisterGames.Common`
- `MisterGames.Common.Editor`
- `MisterGames.Common.RuntimeTests`

## Dependencies
- [`Unity.EditorCoroutines.Editor`](https://docs.unity3d.com/Manual/com.unity.editorcoroutines.html)
