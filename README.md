# HKTimer
Hollow Knight in-game timer mod

## Installation
To install the mod, grab the release DLL from [the latest release](https://github.com/Yurihaia/HollowKnight-Timer/releases).
Put the DLL into the `Mods` folder in your Hollow Knight installation.

## Settings
The keybinds and various other settings can be edited in the `HKTimer.GlobalSettings.json` in the same directory as your save files. They follow the keybind naming system in the `InControl` unity asset. 
the defaults are
```json
{
  "keybinds": {
    "pause": "Pad1",
    "reset": "Pad0",
    "set_start": "Pad7",
    "set_end": "Pad8"
  },
  "timerPosition": {
    "x": 1880.0,
    "y": 1020.0
  },
  "textSize": 30,
  "showTimer": true,
  "trigger": "Scene"
}
```
### timerPosition
The setting `timerPosition` can be used to change the position of the timer text.
The point specified is the Middle-Right point of the main timer text rectangle.
Values is the pixels of the screen from the lower left corner.

## Triggers
Currently, you are able to place a start and an end trigger that can start and pause the timer respectively.
They are marked by a green (start) or red (end) square floating in the air. Saving them (default keybind `num6`)
creates a file `hktimer_triggers.json` in your save folder that contains the positions.
You can copy this file to save different trigger presets.
Loading the file (default keybind `num9`) will read that same file. This is done automatically on game start as well

### Trigger Types
The following trigger types are currently implemented
* Collision Trigger: the basic trigger that will activate when you walk into it
* Movement Trigger: a trigger that will activate when you do an action while you have control
   * Note that this trigger cannot be used as an end point
* Scene Trigger: a trigger that will activate when you enter the scene

### Personal Best Display
The timer also shows a personal best that is tied to a set of waypoints.
These are saved in `hktimer_triggers.json` and measured in microseconds (1000 per millisecond).
The PB is loaded when you load the triggers, so if you need to edit/reset it just edit the file.
The PB will be wiped when you change the trigger positioning, and has to be saved using the save triggers button.

### Manual Trigger File Editing
TBD - Just look in the code or ask me on discord cause this format got fairly complex but luckily manual 
editing is no longer super required