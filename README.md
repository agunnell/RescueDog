# RescueDog
A top-down 2D game, where you play a dog leading a human through a cave-like maze.  
This was created as an example project for the Monogame Getting Started Guide.

![](https://github.com/MrGrak/RescueDog/blob/master/rescueDogCapture1.gif)
![](https://github.com/MrGrak/RescueDog/blob/master/rescueDogCapture2.gif)


## Targets and Platforms
+ RescueDog runs on DirectX, OpenGL, and Windows 10 UWP.  
+ This is done using a shared codebase, with no platform specific code at all.
+ The only difference between the DirectX, OpenGL, and UWP versions is the graphics backend.
+ The graphics backend is entirely managed by MonoGame.

## Shared Class Files
+ The game's class files are located in the RescueDogClasses folder.
+ The DirectX, OpenGL, and UWP projects reference these classes.
+ The class files are platform and target agnostic.

## Duplicated Asset Files
+ The game's assets (images, fonts, sounds) are located in the Content folder.  
+ However, each project duplicates the game assets using it's own Content.mgcb.  
+ This design choice made it easier to properly build the game assets per platform.  

## ToDo
+ test game on Xbox One
+ various code refactoring