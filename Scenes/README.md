# MisterGames Scenes v0.1.1

## Scene loading

Package offers loading scheme without `DontDestroyOnLoad` usage by creating root scene and starting it first before Unity Editor's start scene.
Root scene can be set in `ScenesStorage` (singleton scriptable object, auto-created at `Data/Resources` folder). `SceneLoader` is a facade for `SceneManager` API.

```
    string sceneName = "SomeScene";
    bool makeActive = true;

    SceneLoader.LoadScene(sceneName, makeActive);
    SceneLoader.UnloadScene(sceneName);

    // using UniTask
    await SceneLoader.LoadSceneAsync(sceneName, makeActive);
    await SceneLoader.UnloadSceneAsync(sceneName);
```

## Scene shortcut

Allows to select scene from popup near main toolbar.

https://user-images.githubusercontent.com/109593086/208106228-cf0a1c8c-96b6-4f9c-8481-041bee44b29e.mp4

## Usage


## Assembly definitions
- `MisterGames.Scenes`
- `MisterGames.Scenes.Editor`

## Dependencies
- `MisterGames.Common`
- `MisterGames.Common.Editor`
- `MisterGames.Tick`
- `UniTask`
