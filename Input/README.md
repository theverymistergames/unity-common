﻿﻿﻿# MisterGames Input v0.5.0

## Input actions
- ```InputActionKey``` - single action with ```OnPress```, ```OnRelease``` and ```OnUse``` events.
  ```OnUse``` event is called by key activation strategy in the input action.
- ```InputActionVector2``` and ```InputActionAxis``` - continuous actions with ```OnChanged``` event.  

## Bindings
- enum ```KeyBinding```, enum ```AxisBinding```
- Static class ```GlobalInput``` methods can be used as extension:
  - ```bool GlobalInput.IsActive(this KeyBinding key)``` to see if ```KeyBinding``` is active during current frame   
  - ```Vector2 GlobalInput.GetValue(this AxisBinding axis)``` to see the value of ```AxisBinding``` during current frame   
- ```Key```/```KeyCombo``` - for ```InputActionKey``` actions
- ```Vector2```/```Vector2Key``` - for ```InputActionVector2``` actions
- ```AxisKey``` - for ```InputActionAxis``` actions

## Key activation
- To resolve key overlaps some of the key activation strategies can be selected
- If no strategy selected, ```OnUse``` event will not be invoked
- Create strategy via Create menu -> MisterGames -> Input -> Activation
- Strategy ```Press```: ```OnUse``` is invoked on key pressed 
- Strategy ```Release```: ```OnUse``` is invoked on key released
- Strategy ```Hold```: ```OnUse``` is invoked after key being held for some hold time
- Strategy ```Tap```: ```OnUse``` is invoked in activation time after key pressed - if no interruptions occured.
  Interruption can be caused by key overlap: 
  - ```InputActionKey``` `Action` with binding ```G```
  - ```InputActionKey``` `ComboAction` with binding ```Alt + G```
  - Tap strategy waits for the activation time after user pressed ```G```, and if during this time no combo action
    activated, then activate `Action`

## Setup
- Create ```InputAction``` via Create menu -> MisterGames -> Input -> Action
- Create bindings for the input action via Create menu -> MisterGames -> Input -> Bindings
- Create ```InputSet``` with input actions. Input set resolves key overlaps.
- Create ```InputScheme``` with input sets. Only one input scheme can be active at a time. 
- Create ```InputChannel``` with input schemes. Input channel can be used to select input scheme.
- Setup ```InputUpdater``` component on any game object in the scene and select input channels to update.
- Create key activation strategy for ```InputActionKey``` actions. Strategy invoke 

## Assembly definitions
- MisterGames.Input

## Dependencies
- MisterGames.Common
- Unity.InputSystem (embedded)

## Installation
- Add [MisterGames Common](https://gitlab.com/theverymistergames/common) package
- Top menu MisterGames -> Packages, add packages: 
  - [Input](https://gitlab.com/theverymistergames/input/)