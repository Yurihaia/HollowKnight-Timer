# HKTimer
Hollow Knight in-game timer mod

## Settings
The keybinds and various other settings can be edited in the `hktimer.json` in the same directory as your save files. They follow the unity keybind naming system.  
the defaults are
```json
{
    "pause": "[1]",
    "reset": "[0]",
    "reload_settings": "[5]",
    "set_start": "[7]",
    "set_end": "[8]",
    "load_triggers": "[9]",
    "save_triggers": "[6]",
    "timerAnchorX": 0.15,
    "timerAnchorY": 0.1
}
```
### timerAchorX/Y
The settings `timerAnchorX` and `timerAnchorY` can be used to change the position of the timer text.
The point specified should be the lower right corner of the text probably idk test it for yourself.
Values are % of the screen I think.

## Triggers
Currently, you are able to place a start and an end trigger that can start and pause the timer respectively.
They are marked by a green (start) or red (end) square floating in the air. Saving them (default keybind `num6`)
creates a file `hktimer_triggers.json` in your save folder that contains the positions.
You can copy this file to save different trigger presets.
Loading the file (default keybind `num9`) will read that same file. This is done automatically on game start as well

### Personal Best Display
The timer also shows a personal best that is tied to a set of waypoints.
These are saved in `hktimer_triggers.json` and measured in microseconds (1000 per millisecond).
The PB is loaded when you load the triggers, so if you need to edit/reset it just edit the file.
The PB will be wiped when you change the trigger positioning, and has to be saved using the save triggers button.

### Manual Trigger File Editing
TBD - Just look in the save file its fairly self explanatory