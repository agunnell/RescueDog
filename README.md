# RescueDog
A top-down 2D game, where you play a dog leading a human through a maze.  
This was created as an example project for the Monogame Getting Started Guide.

![](https://github.com/MrGrak/RescueDog/blob/master/rescueDogCapture1.gif)  
![](https://github.com/MrGrak/RescueDog/blob/master/rescueDogCapture2.gif)  

## Project Structure
The game's class files are located in the OpenGL project folder.  
The DirectX project references these game classes.  
The game classes are platform and target agnostic.

## ToDo
+ move game classes outside of OpenGL project into their own folder
+ setup all projects to reference classes from the game classes folder
+ port RescueDog to UWP, deploy and test on Xbox One
+ do some minor cleanup on the code, simplify it

