# MisterGames Scenes v0.1.1

## Scene loading

Package offers loading scheme without `DontDestroyOnLoad` usage by creating root scene and starting it first before Unity Editor's start scene.

- Root scene purpose is to hold initializers and global services
- Root scene can be set in the `ScenesStorage` (singleton scriptable object, auto-created at `Data/Resources` folder) 
- To disable root scene loading turn off the field `Enable Play Mode Start Scene Override` in the `ScenesStorage`.
- `SceneLoader` is a facade for `SceneManager` API that knows about root scene.

```csharp
    string sceneName = "SomeScene";
    bool makeActive = true;

    SceneLoader.LoadScene(sceneName, makeActive);
    SceneLoader.UnloadScene(sceneName);

    // using UniTask
    await SceneLoader.LoadSceneAsync(sceneName, makeActive);
    await SceneLoader.UnloadSceneAsync(sceneName);
```

## Scene shortcut

Allows to select a scene from a drowdown menu near the main toolbar. To display new created scene in this menu, `ScenesStorage` needs to be refreshed. 
Refresh can be performed via menu `MisterGames/Tools/Refresh Scenes Storage` or `Refresh Scenes` button in the inspector of `Data/Resources/ScenesStorage.asset`. 

https://user-images.githubusercontent.com/109593086/208106228-cf0a1c8c-96b6-4f9c-8481-041bee44b29e.mp4

## Assembly definitions
- `MisterGames.Scenes`
- `MisterGames.Scenes.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Tick`](https://github.com/theverymistergames/unity-common/tree/master/Tick)
- [`Cysharp.UniTask`](https://github.com/Cysharp/UniTask)
