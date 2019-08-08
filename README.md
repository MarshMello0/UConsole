# UConsole
A in game unity console for debugging and much more
![Image of UConsole](https://raw.githubusercontent.com/MarshMello0/UConsole/master/Images/uconsole.PNG)

## How to install UConsole to your Unity Project!

1. First head over to the [releases](https://github.com/MarshMello0/UConsole/releases) and download the .unitypackage file
3. Open the .unitypackage and import all of the files.
4. Drag the UConsole-Canvas prefab into your first scene.

Then you should be all set up and ready to start using it.

## Features

- Create and Remove Custom commands in Runtime
- Small out of the way design
- Simple drag and drop into your first scene
- No extra requirements, uses basic unity items.



## Commands

You can get the instance with 
```cs
UConsole.instance
```

You can create a new command with ``UConsole.instance.AddCommand(UCommand);``. It requires a UCommand.

You can add events when the command is entered by using ``UCommand.callbacks.Add(<method(string[] args)>)``

You can remove commands with ``UConsole.instance.RemoveCommand(string command);``