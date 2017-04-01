# Developer's Notes for RescueDog
These notes discuss various aspects of the game and it's development.

## The Codebase
+ Is tightly coupled spaghetti code.
+ Is also fairly straightforward and easy to understand.
+ Will eventually be refactored using techniques I've since adopted.
+ Is OOP + MVC, with a dash of functional programming (done incorrectly).

## Camera View Culling
+ We reduce the sprites we draw to only the sprites that can be seen.
+ This is based on the camera's view. Sprites outside the view are not drawn.
+ This draw state is managed using a .visible boolean.
+ Only actors that are .visible are updated and collision checked.

## Collision Checking
+ Collision checking is done using rectangles.
+ If two rectangles overlap, then they are understood to be colliding.
+ CC simply prevents rectangles from overlapping.
+ If a rectangle moves to a position where a collision occurs, the rec is pushed back.
+ Collisions are checked per axis, so rectangles can slide along each other.

## Level Generation Overview
+ Rooms are randomly scattered within a bounds.
+ Empty tiles surrounded by empty tiles are filled (fill islands).
+ Tiles are then randomly connected 50% of the time.
+ Any filled tiles surrounded by empty tiles are then removed (pillars).
+ The tiles are flood filled to determine connections.
+ the level is 'graded' to pass or fail, based on connections.
+ if the level fails to connect adequately, tiles are reconnected and reflooded.
+ this repeats until a passing level is generated.
+ islands are removed, disconnected paths are removed.
+ disconnected room are removed.
+ the floors and wall objects are created.
+ the spawn, exit, and wall decoration objects are created.
+ the level is zDepth sorted and the player is moved to spawn.
+ (i've since written much better/faster level generation algorithms, pls forgive)
