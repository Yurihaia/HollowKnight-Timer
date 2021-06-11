# HKTimer
Hollow Knight in-game timer mod

## Installation
To install the mod, grab the release ZIP from [the latest release](https://github.com/Yurihaia/HollowKnight-Timer/releases).
Extract all 3 `.dll` files into the `Mods` folder in your Hollow Knight installation.  
The modding API will say that `System.Runtime.Serialization.dll` failed to load but this is completely normal

## Settings
The keybinds and various other settings can be edited in the `hktimer.json` in the same directory as your save files. They follow the unity keybind naming system.  
the defaults are
```json
{
    "pause": "[1]",
    "reset": "[0]",
    "open_ui": "f1",
    "set_start": "[7]",
    "set_end": "[8]",
    "timerPosition": {
        "x": 1880.0,
        "y": 1020.0
    }
}
```
Note that `"[1]"` means numpad 1.  
See this image for a full list of acceptable inputs:
[Acceptable Inputs](./readme/Acceptable_Inputs.png)
### timerPosition
The setting `timerPosition` can be used to change the position of the timer text.
The point specified is the Middle-Right point of the main timer text rectangle.
Values is the pixels of the screen from the lower left corner.

## GUI
This mod comes with a badly programmed GUI that can be opened with the `open_ui` keybind.
In the ui you can use the up/down keys to navigate, and the left/right keys to change the options, and the menu accept key to activate buttons (like "Save Triggers", "Reload Settings", etc)

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