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
    public class GameManager
    {
        //game manager handles aspects of the game that don't necessarily fit into other classes
        //for example, GM serves as the human and spiders AI, using the FollorHero() and HandleSpiders() methods
        //GM moves these actors using the MoveActor() method, which sets an actor's input state based on their target position
        //GM also handles the dog's bark event, via SetBark()

        public GameScreen gameScreen;
        public GameManager(GameScreen GameScreen) { gameScreen = GameScreen; }


        public void MoveActor(Actor actor)
        {   //if actor gets close enough to a specific axis, snap to that axis to prevent jittering
            if (Math.Abs(actor.targetPosition.X - actor.sprite.position.X) < 5)
            { actor.targetPosition.X = actor.sprite.position.X; }
            if (Math.Abs(actor.targetPosition.Y - actor.sprite.position.Y) < 5)
            { actor.targetPosition.Y = actor.sprite.position.Y; }

            //move actor towards target position
            if (actor.targetPosition.X > actor.sprite.position.X)
            {   //move right + DIAG
                if (actor.targetPosition.Y > actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingDownRight); }
                else if (actor.targetPosition.Y < actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingUpRight); }
                else { actor.HandleInput(InputHelper.InputState.MovingRight); }
            }
            else if (actor.targetPosition.X < actor.sprite.position.X)
            {   //move left + DiAG
                if (actor.targetPosition.Y > actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingDownLeft); }
                else if (actor.targetPosition.Y < actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingUpLeft); }
                else { actor.HandleInput(InputHelper.InputState.MovingLeft); }
            }
            else
            {   //move up or down
                if (actor.targetPosition.Y > actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingDown); }
                else if(actor.targetPosition.Y < actor.sprite.position.Y)
                { actor.HandleInput(InputHelper.InputState.MovingUp); }
            }
        }


        public void FollowHero()
        {   //human follows the dog
            gameScreen.human.targetPosition.X = gameScreen.dog.sprite.position.X;
            gameScreen.human.targetPosition.Y = gameScreen.dog.sprite.position.Y - 32;
            //BUT if exit is nearby, goto the exit instead
            if (Math.Abs(gameScreen.level.exitObj.sprite.position.X - gameScreen.human.sprite.position.X) < 40)
            {
                if (Math.Abs(gameScreen.level.exitObj.sprite.position.Y - gameScreen.human.sprite.position.Y) < 40)
                {
                    gameScreen.human.targetPosition.X = gameScreen.level.exitObj.sprite.position.X;
                    gameScreen.human.targetPosition.Y = gameScreen.level.exitObj.sprite.position.Y - 64;
                }
            }
            MoveActor(gameScreen.human);

            //if human gets close enough to dog, stop walking
            if (Math.Abs(gameScreen.human.targetPosition.X - gameScreen.human.sprite.position.X) < 25)
            {
                if (Math.Abs(gameScreen.human.targetPosition.Y - gameScreen.human.sprite.position.Y) < 25)
                { gameScreen.human.HandleInput(InputHelper.InputState.Idle); }
            }
        }


        public void SetBark()
        {   //center barkrec to dog's position if dog is barking
            if (gameScreen.dog.animationState == Actor.State.Action)
            {   //if dog is barking, set bark rec to dog position
                gameScreen.barkRec.X = (int)gameScreen.dog.sprite.position.X - 64;
                gameScreen.barkRec.Y = (int)gameScreen.dog.sprite.position.Y - 64;
                //place bark sprite near dog's mouth
                gameScreen.barkSprite.position.Y = (int)gameScreen.dog.sprite.position.Y - 4;
                gameScreen.barkSprite.spriteEffect = gameScreen.dog.sprite.spriteEffect;
                //place bark sprite left or right, depending on which way the dog is facing
                if (gameScreen.dog.sprite.spriteEffect == SpriteEffects.None)
                { gameScreen.barkSprite.position.X = (int)gameScreen.dog.sprite.position.X - 20; }
                else { gameScreen.barkSprite.position.X = (int)gameScreen.dog.sprite.position.X + 20; }
            }   
            else
            {   //else move bark rec + sprite offscreen (hide it)
                gameScreen.barkRec.X = 0; gameScreen.barkRec.Y = 0;
                gameScreen.barkSprite.position.X = 0; gameScreen.barkSprite.position.Y = 0;
            }
        }


        int total = 0;      //generic total used by GM
        int counter = 0;    //generic counter used by GM
        Actor spider;       //used to target a spider, saves a bit of codespace
        public void HandleSpiders(GameTime GameTime)
        {
            gameScreen.exclamationPoint.shouldDraw = false; //assume the human cannot see a spider
            gameScreen.screenManager.game.soundManager.playTrack3 = false; //assume there are no spiders on screen

            total = gameScreen.level.actors.Count;
            for (counter = 0; counter < total; counter++)
            {   //handle each spider on the level's actors list
                if (gameScreen.level.actors[counter].sprite.shouldDraw)
                {   //spider is on screen and should pursue human
                    spider = gameScreen.level.actors[counter];  //shorten the spider's object reference
                    spider.inputState = Actor.State.Idle;       //assume idle input
                    gameScreen.screenManager.game.soundManager.playTrack3 = true; //play the drum music track

                    //does this spider collide with the bark rec?
                    if (spider.collisionRec.Intersects(gameScreen.barkRec))
                    {   //spider should jump in opposite direction
                        spider.HandleInput(InputHelper.InputState.Action);
                        spider.stateLocked = true;
                        spider.animationState = Actor.State.Action;
                        spider.animationCounter = 0;
                        //push spider left, if dog is on right
                        if (gameScreen.dog.sprite.position.X > spider.sprite.position.X) { spider.targetPosition.X -= 100; }
                        else { spider.targetPosition.X += 100; } //or push spider the opposite way
                        //push spider up, if dog is lower than spider
                        if (gameScreen.dog.sprite.position.Y > spider.sprite.position.Y) { spider.targetPosition.Y -= 100; }
                        else { spider.targetPosition.Y += 100; } //or push spider the opposite way
                    } 
                    else if (spider.animationState != Actor.State.Action) //if spider isn't jumping
                    {   //target the human
                        spider.targetPosition.X = gameScreen.human.sprite.position.X;
                        spider.targetPosition.Y = gameScreen.human.collisionRec.Y + 2;
                    }

                    MoveActor(spider);

                    //does this spider collide with the human's line of sight?
                    if (spider.collisionRec.Intersects(gameScreen.humanLineOfSight))
                    {   //draw exclamation point sprite, but only if human isn't dying/dead
                        if (gameScreen.human.animationState != Actor.State.Action)
                        { gameScreen.exclamationPoint.shouldDraw = true; }
                    }

                    //if spiders gets close enough to human, ATTACK!
                    if (Math.Abs(gameScreen.human.sprite.position.X - spider.sprite.position.X) < 70)
                    {
                        if (Math.Abs(gameScreen.human.sprite.position.Y - spider.sprite.position.Y) < 70)
                        {
                            if (!spider.stateLocked) //wait until spider finishes animation
                            { spider.HandleInput(InputHelper.InputState.Action); }

                            if (spider.collisionRec.Intersects(gameScreen.human.collisionRec))
                            {   //spider has successfully bitten human, human dies
                                gameScreen.human.HandleInput(InputHelper.InputState.Action);
                                gameScreen.human.Update(GameTime, gameScreen.level);
                                //move spider outside of level bounds
                                spider.sprite.position.X = 0; spider.sprite.position.Y = 0;
                                //play lost game fanfare
                                gameScreen.screenManager.game.soundManager.lostGameIns.Play();
                            }
                        }
                    }
                    
                    //update the spider
                    gameScreen.level.actors[counter].Update(GameTime, gameScreen.level);
                }
            }
        }


        int frameCounter = 0;       //counts up to beat level pause amount
        int beatLevelPause = 60;    //how long game waits before next level, in frames @ 16ms / frame (about 1 second)
        public void CheckForExit()
        {
            gameScreen.screenManager.game.soundManager.playTrack2 = false; //assume track2 should be muted
            //if exit is somewhat closeby, play music track 2
            if (Math.Abs(gameScreen.level.exitObj.sprite.position.X - gameScreen.human.sprite.position.X) < 500)
            {
                if (Math.Abs(gameScreen.level.exitObj.sprite.position.Y - gameScreen.human.sprite.position.Y) < 500)
                { gameScreen.screenManager.game.soundManager.playTrack2 = true; }
            }
            //check to see if human is colliding with exit
            if (gameScreen.human.collisionRec.Intersects(gameScreen.level.exitObj.collisionRec))
            {   //hide the human
                gameScreen.human.sprite.shouldDraw = false;
                gameScreen.screenManager.game.soundManager.wonGameIns.Play(); //play the won game fanfare
                //wait for a moment to allow player to understand they've beaten the level
                frameCounter++;
                if (frameCounter > beatLevelPause)
                {
                    frameCounter = 0;
                    if (gameScreen.levelNumber < 4) //check the current level to see if number is less than 4
                    {   //increment the current level
                        gameScreen.levelNumber++;
                        //pop open the title widget to level number, notify player
                        if (gameScreen.levelNumber == 2)
                        { gameScreen.titleWidget.Open(TitleWidget.State.Level2); }
                        else if (gameScreen.levelNumber == 3)
                        { gameScreen.titleWidget.Open(TitleWidget.State.Level3); }
                        else if (gameScreen.levelNumber == 4)
                        { gameScreen.titleWidget.Open(TitleWidget.State.Level4); }
                        //load a new level based on level number
                        gameScreen.levelGenerator.GenerateLevel(gameScreen.levelNumber);
                        gameScreen.human.sprite.shouldDraw = true; //show the human again
                        //System.Diagnostics.Debug.WriteLine("created level " + gameScreen.levelNumber);
                    }
                    else //player has won the game
                    { gameScreen.titleWidget.Open(TitleWidget.State.WonGame); }
                }
            }
        }
    }
}
