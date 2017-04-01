using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace RescueDog
{
    public class Actor
    {
        GameScreen gameScreen;

        public Sprite sprite;               //the actor's sprite, actor's position is sprite.position
        public Vector2 newPosition;         //where the actor wants to move to this frame
        public Vector2 targetPosition;      //where the actor wants to move to overall
        public float movementSpeed;         //variable based on animation, affects new position
        public enum MoveDirection           //represents a direction - NSEW + DIAG
        { Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, None }
        public MoveDirection moveDirection = MoveDirection.None; //which direction the actor wants to go

        public enum State
        {
            Idle,       //standing stationary, idle animations
            Moving,     //moving NSEW + DIAG
            Landing,    //in between animation; moving > landing > idle
            Action,     //dog barking, human dying, spider jumping
        }
        public State animationState;    //represents the actor's animation state, updated based on animFPS
        public State inputState;        //represents the input's animation state, updated every tick
        public Boolean stateLocked;     //locks state to ensure animation completes before changing

        public List<Point> idleStill;          //single frame of static idle
        public List<Point> idleAnimation1;     //animated idle
        public List<Point> idleAnimation2;     //animated idle
        public List<Point> idleAnimation3;     //animated idle

        public List<Point> movingFrames;        //list of moving animation frames
        public List<Point> landingFrames;       //list of landing animation frames
        public List<Point> actionFrames;        //list of action animation frames (dog barking, spider jumping, human dying)
        public List<Point> currentAnimation;    //points to one of the animation lists above

        public int animationCounter = 0;        //tracks where the actor is in the currentAnimation
        public float timer = 0;                 //tracks how much time has elapsed since last tick
        public float animFPS = 80.0f;           //limits animation speed, higher is slower animation

        public int counter;                 //generic counter used by actor
        public int total;                   //generic total used by actor
        Random random = new Random();       //generates a random number
        public int animationChoice = 0;     //used to choose a random idle animation
        public SoundEffectInstance actionSoundFX;   //the sound effect played when the actor performs their action animation

        public Rectangle collisionRec;      //used to check collisions with the object - usually different size than sprite size
        public Point collisionOffset;       //offsets the collision rec by this amount
        public Point collisionPrevPos;      //where the collision rec was located before being projected
        public Point collisionNewPos;       //where the collision rec is located after being projected
        public Boolean collisionX;          //true means a collision occurred on the X axis
        public Boolean collisionY;          //true means a collision occurred on the Y axis
        public Boolean checkCollisions = true; //true enables collision checking

        public enum Type { Dog, Human, Spider } //there are only 3 types of actors in this game
        public Type type;   //represents which type of actor this actor is


        public Actor(GameScreen GameScreen, Vector2 Position, Type Type)
        {
            gameScreen = GameScreen;
            type = Type;
            //create default actor values
            Point Cellsize = new Point(0, 0);
            Point CurrentFrame = new Point(0, 0);
            Point FrameOffset = new Point(0, 0);
            collisionRec = new Rectangle(0, 0, 0, 0);
            collisionOffset = new Point(0, 0);
            collisionPrevPos = new Point(0, 0);
            collisionNewPos = new Point(0, 0);
            //create new animation lists
            idleStill = new List<Point>();
            idleAnimation1 = new List<Point>();
            idleAnimation2 = new List<Point>();
            idleAnimation3 = new List<Point>();
            movingFrames = new List<Point>();
            landingFrames = new List<Point>();
            actionFrames = new List<Point>();
            currentAnimation = new List<Point>();
            //create default sprite
            sprite = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, Position, Cellsize, CurrentFrame);
            movementSpeed = 0.0f;
            sprite.spriteEffect = SpriteEffects.None;
            animationState = State.Idle;
            inputState = State.Idle;
            stateLocked = false;
            currentAnimation = idleStill;
            targetPosition = new Vector2(Position.X, Position.Y);


            #region Define Dog

            if (Type == Type.Dog)
            {
                sprite.cellSize.X = 32 * 1; sprite.cellSize.Y = 32 * 1;
                sprite.currentFrame.X = 0; sprite.currentFrame.Y = 2;
                sprite.CenterOrigin();
                sprite.zOffset = 0;

                collisionRec.Width = 30; collisionRec.Height = 5;
                collisionOffset.X = 1; collisionOffset.Y = 12;

                //static idle animation
                idleStill.Add(new Point(0, 2));

                //idle animation 1
                idleAnimation1.Add(new Point(3, 3)); //tail wagging
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(3, 3)); //repeat
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(3, 3)); //repeat
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(3, 3)); //repeat
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(3, 3)); //repeat
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(3, 3)); //repeat
                idleAnimation1.Add(new Point(0, 2));
                idleAnimation1.Add(new Point(4, 3));
                idleAnimation1.Add(new Point(0, 2));

                //idle animation 2
                idleAnimation2.Add(new Point(5, 3)); //blink
                idleAnimation2.Add(new Point(6, 3)); //hold eyes closed
                idleAnimation2.Add(new Point(6, 3));
                idleAnimation2.Add(new Point(6, 3));
                idleAnimation2.Add(new Point(5, 3)); //open

                //idle animation 3
                idleAnimation3.Add(new Point(7, 2)); //lower head
                idleAnimation3.Add(new Point(6, 2));
                idleAnimation3.Add(new Point(5, 2)); //head down, tail wagging
                idleAnimation3.Add(new Point(7, 3));
                idleAnimation3.Add(new Point(5, 2)); //repeat
                idleAnimation3.Add(new Point(7, 3));
                idleAnimation3.Add(new Point(5, 2)); //repeat
                idleAnimation3.Add(new Point(7, 3));
                idleAnimation3.Add(new Point(5, 2)); //repeat
                idleAnimation3.Add(new Point(7, 3));
                idleAnimation3.Add(new Point(5, 2)); //repeat
                idleAnimation3.Add(new Point(7, 3));
                idleAnimation3.Add(new Point(6, 2)); //head up
                idleAnimation3.Add(new Point(7, 2));

                //setup walking animations
                movingFrames.Add(new Point(1, 2));
                movingFrames.Add(new Point(2, 2));
                movingFrames.Add(new Point(3, 2));
                movingFrames.Add(new Point(4, 2));
                movingFrames.Add(new Point(5, 2));

                //setup landing animations
                landingFrames.Add(new Point(6, 2));
                landingFrames.Add(new Point(7, 2));

                //setup action animations
                actionFrames.Add(new Point(0, 3));
                actionFrames.Add(new Point(1, 3));
                actionFrames.Add(new Point(2, 3));

                actionSoundFX = gameScreen.screenManager.game.soundManager.dogBarkIns;
            }

            #endregion


            #region Define Human

            else if (Type == Type.Human)
            {
                sprite.cellSize.X = 32 * 1; sprite.cellSize.Y = 32 * 2;
                sprite.currentFrame.X = 0; sprite.currentFrame.Y = 2;
                sprite.CenterOrigin();
                sprite.zOffset = 32 + 0;

                collisionRec.Width = 22; collisionRec.Height = 5;
                collisionOffset.X = 0; collisionOffset.Y = 40;

                //static idle animation
                idleStill.Add(new Point(0, 2));

                //idle animations 1, 2, 3
                idleAnimation1.Add(new Point(0, 2)); //blink
                idleAnimation1.Add(new Point(1, 2));
                idleAnimation1.Add(new Point(2, 2));
                idleAnimation1.Add(new Point(2, 2));
                idleAnimation1.Add(new Point(2, 2));
                idleAnimation1.Add(new Point(1, 2));
                idleAnimation2 = idleAnimation1;
                idleAnimation3 = idleStill;

                //setup walking animations
                movingFrames.Add(new Point(0, 3));
                movingFrames.Add(new Point(1, 3));
                movingFrames.Add(new Point(2, 3));
                movingFrames.Add(new Point(3, 3));
                movingFrames.Add(new Point(4, 3));
                movingFrames.Add(new Point(5, 3));
                movingFrames.Add(new Point(6, 3));
                movingFrames.Add(new Point(7, 3));

                //setup landing animations
                landingFrames.Add(new Point(0, 2));

                //setup death animations
                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(4, 2));
                actionFrames.Add(new Point(4, 2));

                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(4, 2));
                actionFrames.Add(new Point(4, 2));

                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(4, 2));
                actionFrames.Add(new Point(4, 2));

                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(3, 2));
                actionFrames.Add(new Point(4, 2));
                actionFrames.Add(new Point(4, 2));
                actionFrames.Add(new Point(5, 2));
                actionFrames.Add(new Point(5, 2));
                actionFrames.Add(new Point(6, 2));
                actionFrames.Add(new Point(7, 2));

                actionSoundFX = gameScreen.screenManager.game.soundManager.humanBitIns;
            }

            #endregion


            #region Define Spider

            else if (Type == Type.Spider)
            {
                sprite.cellSize.X = 16 * 1; sprite.cellSize.Y = 16 * 1;
                sprite.currentFrame.X = 11; sprite.currentFrame.Y = 2;
                sprite.CenterOrigin();
                sprite.zOffset = 0;

                collisionRec.Width = 16; collisionRec.Height = 8;
                collisionOffset.X = 1; collisionOffset.Y = 0;

                //static idle animation
                idleStill.Add(new Point(11, 2));
                idleAnimation1 = idleStill;
                idleAnimation2 = idleStill;
                idleAnimation3 = idleStill;

                //this causes the spider to rest after an attack
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));
                landingFrames.Add(new Point(11, 2));

                //moving animation
                movingFrames.Add(new Point(10, 2));
                movingFrames.Add(new Point(11, 2));
                movingFrames.Add(new Point(12, 2));
                movingFrames.Add(new Point(11, 2));

                actionFrames.Add(new Point(13, 2)); //jumping in air
                actionFrames.Add(new Point(13, 2));

                actionSoundFX = gameScreen.screenManager.game.soundManager.spiderJumpIns;
            }

            #endregion

        }


        public void Move()
        {   //move in direction + flip sprite if necessary
            if (moveDirection == MoveDirection.Down)
            { newPosition.Y += movementSpeed * 0.75f; }
            else if (moveDirection == MoveDirection.DownLeft)
            { newPosition.Y += movementSpeed * 0.75f; newPosition.X -= movementSpeed * 0.75f; sprite.spriteEffect = SpriteEffects.None; }
            else if (moveDirection == MoveDirection.DownRight)
            { newPosition.Y += movementSpeed * 0.75f; newPosition.X += movementSpeed * 0.75f; sprite.spriteEffect = SpriteEffects.FlipHorizontally; }
            else if (moveDirection == MoveDirection.Left)
            { newPosition.X -= movementSpeed; sprite.spriteEffect = SpriteEffects.None; }
            else if (moveDirection == MoveDirection.Right)
            { newPosition.X += movementSpeed; sprite.spriteEffect = SpriteEffects.FlipHorizontally; }
            else if (moveDirection == MoveDirection.Up)
            { newPosition.Y -= movementSpeed * 0.75f; }
            else if (moveDirection == MoveDirection.UpLeft)
            { newPosition.Y -= movementSpeed * 0.75f; newPosition.X -= movementSpeed * 0.75f; sprite.spriteEffect = SpriteEffects.None; }
            else if (moveDirection == MoveDirection.UpRight)
            { newPosition.Y -= movementSpeed * 0.75f; newPosition.X += movementSpeed * 0.75f; sprite.spriteEffect = SpriteEffects.FlipHorizontally; }
        }


        public void HandleInput(InputHelper.InputState InputState)
        {   //THIS DOES NOT ACCEPT screenManager.input - THIS ACCEPTS AN INPUT STATE
            //this allows AI to pass INPUT STATE as if AI were using a keyboard/controller
            //this also allows USER to pass INPUT STATE using a keyboard/controller
            //we map user input to an input state in inputHelper.cs
            moveDirection = MoveDirection.None; //assume no movement
            inputState = State.Idle; //assume idle input state

            //idle + action input
            if (InputState == InputHelper.InputState.Action) { inputState = State.Action; }
            //movement input
            else if (InputState == InputHelper.InputState.MovingDown) { inputState = State.Moving; moveDirection = MoveDirection.Down; }
            else if (InputState == InputHelper.InputState.MovingDownLeft) { inputState = State.Moving; moveDirection = MoveDirection.DownLeft; }
            else if (InputState == InputHelper.InputState.MovingDownRight) { inputState = State.Moving; moveDirection = MoveDirection.DownRight; }
            else if (InputState == InputHelper.InputState.MovingLeft) { inputState = State.Moving; moveDirection = MoveDirection.Left; }
            else if (InputState == InputHelper.InputState.MovingRight) { inputState = State.Moving; moveDirection = MoveDirection.Right; }
            else if (InputState == InputHelper.InputState.MovingUp) { inputState = State.Moving; moveDirection = MoveDirection.Up; }
            else if (InputState == InputHelper.InputState.MovingUpLeft) { inputState = State.Moving; moveDirection = MoveDirection.UpLeft; }
            else if (InputState == InputHelper.InputState.MovingUpRight) { inputState = State.Moving; moveDirection = MoveDirection.UpRight; }
        }


        public void Update(GameTime gameTime, Level level)
        {   
            newPosition.X = sprite.position.X;  //newPosition starts at the current position
            newPosition.Y = sprite.position.Y;  //then we change new position via Move() using actor's move direction


            #region Map InputState to Animation State

            //let input immediately overwrite any animation if input is action
            //this makes the game feel responsive - otherwise actors would wait for animation to finish
            if (inputState == State.Action)
            {   //if actor is already performing action, ignore user input
                if (animationState != State.Action) //otherwise, overwrite the current animation with actors actions
                { animationState = State.Action; animationCounter = 0; actionSoundFX.Play(); } //play the actor's action sound fx
            }
            else if (!stateLocked) //if the actor isn't in the middle of playing an animation
            {   
                if (inputState != animationState)
                {   //sync the animationState to the inputState
                    animationState = inputState;
                    animationCounter = 0; //animations must start from the beginning
                }
            }

            #endregion


            #region Check Animation State, Set Current Animation

            if (animationState == State.Moving)
            {
                if (type == Type.Dog)
                {   //based on where dog is in moving animation, change the movement speed
                    //this makes the dogs jump animation more realistic
                    if (animationCounter == 0) { movementSpeed = 1.0f; }
                    else if (animationCounter == 1) { movementSpeed = 2.0f; }
                    else if (animationCounter == 2) { movementSpeed = 2.0f; }
                    else if (animationCounter == 3) { movementSpeed = 2.0f; }
                    else if (animationCounter == 4) { movementSpeed = 1.0f; }
                }
                else if (type == Type.Human) { movementSpeed = 0.75f; } //human moves slower than dog, faster than spider
                else if (type == Type.Spider) { movementSpeed = 0.3f; } //spider moves the slowest

                Move();
                currentAnimation = movingFrames;
                stateLocked = true; //lock animation until completion
            }
            else if (animationState == State.Landing)
            {
                if (type == Type.Dog)
                {   //based on where dog is in landing animation, set movement speed
                    //this makes the dogs landing animation slow down instead of stopping abruptly or sliding too much
                    if (animationCounter == 0) { movementSpeed = 0.5f; }
                    else if (animationCounter == 1) { movementSpeed = 0.25f; }
                }
                else { movementSpeed = 0.0f; } //other actors don't have a landing animation, so just stop them
                Move();
                currentAnimation = landingFrames;
                stateLocked = true; //lock animation until completion
            }
            else if (animationState == State.Action)
            {   //spiders jump during their action, all other actor actions are stationary
                if (type == Type.Spider) { movementSpeed = 2.0f; Move(); }
                else { movementSpeed = 0.0f; }
                currentAnimation = actionFrames;
                stateLocked = true; //lock animation until completion
            }
            else if (animationState == State.Idle)
            {
                movementSpeed = 0.0f;
                //if actor is standing still, choose an idle animation to play
                if (currentAnimation == idleStill)
                {
                    if (random.Next(1000) > 990) //around a 1% chance to play an idle animation
                    {   //randomly choose between idle animations 1-3
                        animationChoice = random.Next(100);
                        if (animationChoice < 45) { currentAnimation = idleAnimation1; } //45% chance
                        else if (animationChoice < 85) { currentAnimation = idleAnimation2; } //45% chance
                        else { currentAnimation = idleAnimation3; } //10% chance
                        animationCounter = 0; //reset the animation to frame 0
                    }
                }
                stateLocked = false; //unlock state, animation can be interrupted
            }

            #endregion


            #region Play the Current Animation

            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; //get the elapsed gametime
            if (timer >= animFPS) //if the elapsed time is greater than our animation time
            {
                timer = 0; //reset the timer so we wait until the next animFPS tick
                sprite.currentFrame = currentAnimation[animationCounter]; //set the sprite's frame based on current animation
                animationCounter++; //increment the animation counter to the next frame

                if (animationCounter >= currentAnimation.Count())   //if actor is at end of current animation
                {
                    animationCounter = 0; //reset animation

                    if (animationState == State.Action)  //what does the actor do after their action animation is finished?
                    {
                        if (type == Type.Spider) //spider lands and waits for a bit before moving again
                        { animationState = State.Landing; currentAnimation = landingFrames; stateLocked = true; }
                        else if (type == Type.Dog) //dog immediately unlocks and returns to idle
                        { animationState = State.Idle; currentAnimation = idleStill; stateLocked = false; }
                        if (type == Type.Human) //human loops dead/action animation
                        {   //lock the human into action animation, set the animation counter to the next to last frame
                            animationState = State.Action; animationCounter = currentAnimation.Count() - 2; stateLocked = true;
                            gameScreen.titleWidget.Open(TitleWidget.State.GameOver); //open the game over graphic
                        }   //this causes the human to repeat the last 2 frames of their action animation, until revived
                    }
                    //if actor is moving, but input is idle, actor should 'land' then transition into idle
                    else if (animationState == State.Moving && inputState == State.Idle)
                    { animationState = State.Landing; currentAnimation = landingFrames; stateLocked = true; }
                    //otherwise, actor should return to idle state
                    else { animationState = State.Idle; currentAnimation = idleStill; stateLocked = false; }
                }
            }

            #endregion


            #region Perform Collision Checking

            if (checkCollisions)    //the X + Y axis are collision checked individually to allow actors to slide along walls
            {                       //otherwise their movement would stop entirely upon a wall collision
                collisionX = false; collisionY = false; //clear any previous collision data
                collisionPrevPos.X = collisionRec.X;    //capture collision rec's previous and new positions
                collisionPrevPos.Y = collisionRec.Y;
                collisionNewPos.X = (int)(newPosition.X - collisionRec.Width / 2) + collisionOffset.X;
                collisionNewPos.Y = (int)(newPosition.Y - collisionRec.Height / 2) + collisionOffset.Y;
                total = level.walls.Count;
                for (counter = 0; counter < total; counter++)
                {   //check each wall to see if the actor collides with it
                    //project collision rec on X axis, check collisions, unproject
                    collisionRec.X = collisionNewPos.X;
                    if (collisionRec.Intersects(level.walls[counter].collisionRec)) { collisionX = true; }
                    collisionRec.X = collisionPrevPos.X;
                    //project collision rec on Y axis, check collisions, unproject
                    collisionRec.Y = collisionNewPos.Y;
                    if (collisionRec.Intersects(level.walls[counter].collisionRec)) { collisionY = true; }
                    collisionRec.Y = collisionPrevPos.Y;
                }
                //allow actor movement per axis based on collision checks
                if (!collisionX) { sprite.position.X = newPosition.X; }
                if (!collisionY) { sprite.position.Y = newPosition.Y; }
            }
            else { sprite.position.X = newPosition.X; sprite.position.Y = newPosition.Y; } //allow clipping thru walls

            #endregion


            //match collision rec to sprite position
            collisionRec.X = (int)(sprite.position.X - collisionRec.Width / 2) + collisionOffset.X;
            collisionRec.Y = (int)(sprite.position.Y - collisionRec.Height / 2) + collisionOffset.Y;
            //sort onto a layer between 0.999990 to 0.000001 based on position.Y + zOffset
            sprite.zDepth = 0.999990f - ((sprite.position.Y + sprite.zOffset) * 0.000001f);
        }


        public void Draw()
        {
            sprite.Draw(); //always draw the actor's sprite
            if (gameScreen.editor.drawDebug) //draw the collision rec if game is in debug mode
            { sprite.screenManager.spriteBatch.Draw(sprite.screenManager.game.dummyTexture, collisionRec, sprite.screenManager.game.colorDebugRed); }
        }
    }
}