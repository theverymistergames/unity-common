# MisterGames Dbg v0.2.0

Package for debug and prototyping.

## Features

### Developer Console

Developer console allows to create console commands as methods with help of the attribute `[ConsoleCommand]`. Method has to be `void` and 
can have arguments of type that be represented by string to be able to be parsed when typing the console command into the console. 

https://user-images.githubusercontent.com/109593086/208451435-8582d3a8-2f01-4e42-81dd-8c66635b2252.mp4

Console command methods must be a part of a serializable class that implements `IConsoleModule` interface:

```
[Serializable]
class SomeConsoleModule : IConsoleModule {

  public ConsoleRunner ConsoleRunner { get; set; }

  [ConsoleCommand("someCommand")]
  public void SomeCommand(float arg) {
    // some work
    ConsoleRunner.AppendLine($"Invoked console command `someCommand` with arg = `{arg}`");
  }
}
```

`ConsoleRunner` is a `MonoBehaviour` and need to be placed on [root scene](https://github.com/theverymistergames/unity-common/tree/master/Scenes#scene-loading) or 
on some other persistent game object. 

`ConsoleRunner` has some methods to control or display something when executing commands:

```
// prints line in the end of the console
ConsoleRunner.AppendLine(string) 

// sets input in the input field of the console
ConsoleRunner.TypeIn(string)     

// runs console command presented as string
ConsoleRunner.RunCommand(string)

// current text of the input field of the console
string ConsoleRunner.CurrentInput
```

Built-in console modules:
- `HelpConsoleModule` to print console command help: help text can be injected with `[ConsoleCommandHelp("Help text")]` attribute on console command method, or 
otherwise if console command method does not have `[ConsoleCommandHelp]` attribute, help text is just a console command signature
- `TextConsoleModule` to manage font size and other properties of text and input fields in the console

Built-in plugins:
- `ConsoleCommandsHistory` for navigating through commands history by specific [input action](https://github.com/theverymistergames/unity-common/tree/master/Input#input-actions) 
- `ConsoleHotkeys` to assign input actions to the typing console commands into the input field of the console.

### Debug draw
- line, ray, line array, circle, sphere, cylinder, capsule, text

```
DbgRay.Create().From(start).Dir(dir).Color(Color.blue).Arrow(0.1f).Time(1f).Draw();

DbgPointer.Create().Position(start).Size(0.3f).Color(Color.yellow).Draw();

DbgCapsule.Create().From(start).To(end).Radius(radius).Color(Color.cyan).Draw();

DbgText.Create().Text(text).Position(start).Draw();
```

## Assembly definitions
- `MisterGames.Dbg`
- `MisterGames.Dbg.Editor`

## Dependencies
- [`MisterGames.Common`](https://github.com/theverymistergames/unity-common/tree/master/Common)
- [`MisterGames.Input`](https://github.com/theverymistergames/unity-common/tree/master/Input)
- [`Unity.TextMeshPro`](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html)
