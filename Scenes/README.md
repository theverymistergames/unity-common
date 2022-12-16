# MisterGames Scenes v0.1.0

Scenes package offers loading scheme without `DontDestroyOnLoad`:

- `ScenesStorage` script (singleton SO): created automatically in `Data/Resources/` folder.
`ScenesStorage` contains root scene, start scene, and all scenes read-only list.SceneStorage
- `SceneLoader` script: at first loads root scene, then start scene (last opened scene in the Unity Editor)
- Scene shortcut script for fast scene selection

![Scene shortcut demo](https://raw.githubusercontent.com/theverymistergames/readmedata/master/unity-common/Scenes/scene-shortcut.mov)

## Usage
- SceneStorage - a singleton, that
- SceneLoader - a facade for SceneManager API. Also
```
    string sceneName = "SomeScene";
    bool makeActive = true;

    SceneLoader.LoadScene(sceneName, makeActive);
    SceneLoader.UnloadScene(sceneName);

    // or using UniTask
    await SceneLoader.LoadSceneAsync(sceneName, makeActive);
    await SceneLoader.UnloadSceneAsync(sceneName);
```

## Assembly definitions
- MisterGames.Scenes
- MisterGames.Scenes.Editor

## Dependencies
- MisterGames.Common
- MisterGames.Common.Editor
