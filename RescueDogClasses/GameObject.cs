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
    public class GameObject
    {
        //gameobjects are anything that can be animated and/or collided with
        //this includes: exit door, torch, animated fire, and a wall's spider hole (decoration)

        GameScreen gameScreen;
        public Sprite sprite;                   //gameobject's sprite
        public Rectangle collisionRec;          //used to check collisions with the object - usually different size than sprite size
        public Point collisionOffset;           //offsets the collision rec by this amount

        public List<Point> currentAnimation;    //list of frames used for obj's animation
        public int animationCounter = 0;        //tracks where the gameObj is in the currentAnimation
        public float timer = 0;                 //tracks how much time has elapsed since last tick
        public float animFPS = 80.0f;           //limits animation speed, higher is slower

        public enum Type { Exit, Torch, Fire, SpiderHole }  //there are only 4 types of game objects
        public Type type;   //represents this object's type

        public void UpdateCollisionRecPosition()
        {   //center collision rec to sprite, then offset it by collisionOffset
            collisionRec.X = (int)(sprite.position.X - collisionRec.Width / 2) + collisionOffset.X;
            collisionRec.Y = (int)(sprite.position.Y - collisionRec.Height / 2) + collisionOffset.Y;
        }


        public GameObject(GameScreen GameScreen, Vector2 Position, Type Type)
        {
            gameScreen = GameScreen;
            //create default gameobject values
            Point Cellsize = new Point(0, 0);
            Point CurrentFrame = new Point(0, 0);
            Point FrameOffset = new Point(0, 0);
            currentAnimation = new List<Point>();
            //change default values based on the passed object type parameters
            if (Type == Type.Exit)
            {
                Cellsize.X = 16 * 2; Cellsize.Y = 16 * 3; CurrentFrame.X = 4; CurrentFrame.Y = 0;
                currentAnimation.Add(CurrentFrame);
            }
            else if(Type == Type.Fire)
            {
                Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 7; CurrentFrame.Y = 0;
                currentAnimation.Add(CurrentFrame);
                currentAnimation.Add(new Point(7, 1)); //add the fire animation frames
                currentAnimation.Add(new Point(7, 2));
                animFPS = 160.0f;
            }
            else if (Type == Type.Torch)
            {
                Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 7; CurrentFrame.Y = 3;
                currentAnimation.Add(CurrentFrame);
            }
            else if (Type == Type.SpiderHole)
            {
                Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 8; CurrentFrame.Y = 3;
                currentAnimation.Add(CurrentFrame);
            }
            //setup the obj sprite
            sprite = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, Position, Cellsize, CurrentFrame);
            collisionRec = new Rectangle((int)Position.X, (int)Position.Y, Cellsize.X, Cellsize.Y);
            collisionOffset = new Point(0, 0);
        }


        public void Update(GameTime GameTime)
        {   //handle animation
            timer += (float)GameTime.ElapsedGameTime.TotalMilliseconds;
            if (timer >= animFPS)
            {   //reset timer, get next animation frame, increment animationCounter
                timer = 0;
                sprite.currentFrame = currentAnimation[animationCounter];
                animationCounter++;
                //reset animationCounter if we hit end of currentAnimation
                if (animationCounter >= currentAnimation.Count()) { animationCounter = 0; }
            }
        }


        public void Draw()
        {
            sprite.Draw();
            if (gameScreen.editor.drawDebug) //draw the collision rec if game is in debug mode
            { sprite.screenManager.spriteBatch.Draw(sprite.screenManager.game.dummyTexture, collisionRec, sprite.screenManager.game.colorDebugRed); } 
        }
    }
}