# HKTimer
Hollow Knight in-game timer mod

## Keybinds
The keybinds can be edited in the `hktimer.json` in the same directory as your save files. They follow
the unity keybind naming system.  
the defaults are
```json
{
    "pause": "[1]",
    "reset": "[0]",
    "reload_settings": "[5]",
    "set_start": "[7]",
    "set_end": "[8]",
    "load_triggers": "[9]",
    "save_triggers": "[6]"
}
```

## Triggers
Current, you are able to place a start and an end trigger that can start and pause the timer respectively.
They are marked by a green (start) or red (end) square floating in the air. Saving them (default keybind `num6`)
creates a file `hktimer_triggers.json` in your save folder that contains the positions.
You can copy this file to save different trigger presets.
Loading the file (default keybind `num9`) will read that same file. This is done automatically on game start as well

