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
    public class Wall
    {
        //walls are not animated, but are collided with
        GameScreen gameScreen;

        public Sprite sprite;           //the wall's sprite
        public Rectangle collisionRec;  //used to check collisions with the object - usually different size than sprite size
        public Point collisionOffset;   //offsets the collision rec by this amount

        public enum WallType
        {   //the difference between interior and exterior walls can be difficult to understand
            North, South, East, West,   //face into the level
            NorthShort, SouthShort,     //face into the level
            InteriorNE, InteriorNW, InteriorSE, InteriorSW, //interior walls face into the level
            ExteriorNE, ExteriorNW, ExteriorSE, ExteriorSW, //exterior walls face out of the level
        }
        public WallType type;


        public void UpdateCollisionRecPosition()
        {   //center collision rec to sprite, then offset it by collisionOffset
            collisionRec.X = (int)(sprite.position.X - collisionRec.Width / 2) + collisionOffset.X;
            collisionRec.Y = (int)(sprite.position.Y - collisionRec.Height / 2) + collisionOffset.Y;
        }


        public Wall(GameScreen GameScreen, Vector2 Position, WallType Type)
        {
            gameScreen = GameScreen;
            Point Cellsize = new Point(0, 0);
            Point CurrentFrame = new Point(0, 0);
            Point FrameOffset = new Point(0, 0);
            Boolean flipHorizontal = false;
            type = Type;

            //set the cellsize + currentframe based on wall type
            if (Type == WallType.North) { Cellsize.X = 16 * 2; Cellsize.Y = 16 * 3; CurrentFrame.X = 2; CurrentFrame.Y = 0; }
            if (Type == WallType.South) { Cellsize.X = 16 * 2; Cellsize.Y = 16 * 1; CurrentFrame.X = 2; CurrentFrame.Y = 3; }
            if (Type == WallType.NorthShort) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 3; CurrentFrame.X = 4; CurrentFrame.Y = 0; }
            if (Type == WallType.SouthShort) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 4; CurrentFrame.Y = 3; }

            if (Type == WallType.East) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 2; CurrentFrame.X = 6; CurrentFrame.Y = 0; FrameOffset.Y = 16; }
            if (Type == WallType.West) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 2; CurrentFrame.X = 6; CurrentFrame.Y = 0; FrameOffset.Y = 16; flipHorizontal = true; }

            if (Type == WallType.InteriorNE) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 2; CurrentFrame.X = 6; CurrentFrame.Y = 0; }
            if (Type == WallType.InteriorNW) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 2; CurrentFrame.X = 6; CurrentFrame.Y = 0; flipHorizontal = true; }
            if (Type == WallType.InteriorSE) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 6; CurrentFrame.Y = 3; }
            if (Type == WallType.InteriorSW) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 6; CurrentFrame.Y = 3; flipHorizontal = true; }

            if (Type == WallType.ExteriorNE) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 3; CurrentFrame.X = 3; CurrentFrame.Y = 0; }
            if (Type == WallType.ExteriorNW) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 3; CurrentFrame.X = 3; CurrentFrame.Y = 0; flipHorizontal = true; }
            if (Type == WallType.ExteriorSE) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 3; CurrentFrame.Y = 3; }
            if (Type == WallType.ExteriorSW) { Cellsize.X = 16 * 1; Cellsize.Y = 16 * 1; CurrentFrame.X = 3; CurrentFrame.Y = 3; flipHorizontal = true; }

            sprite = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, Position, Cellsize, CurrentFrame);
            sprite.frameOffset = FrameOffset;
            if (flipHorizontal) { sprite.spriteEffect = SpriteEffects.FlipHorizontally; }

            collisionRec = new Rectangle((int)Position.X, (int)Position.Y, Cellsize.X, Cellsize.Y);
            collisionOffset = new Point(0, 0);
        }


        public void Draw()
        {
            sprite.Draw();
            if (gameScreen.editor.drawDebug) //draw the collision rec if game is in debug mode
            { sprite.screenManager.spriteBatch.Draw(sprite.screenManager.game.dummyTexture, collisionRec, sprite.screenManager.game.colorDebugRed); }
        }
    }
}