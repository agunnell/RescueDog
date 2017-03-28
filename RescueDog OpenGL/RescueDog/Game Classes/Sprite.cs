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
    public class Sprite
    {
        public ScreenManager screenManager;
        public Texture2D texture;   //the texture used by sprite
        public Point cellSize;      //the size of the sprite in sprite sheet
        public Point currentFrame;  //the frame of the sprite in sprite sheet
        
        public Vector2 position;
        public Boolean shouldDraw = true;   //if false, the sprite will not draw
        public float alpha = 1.0f;          //controls the opacity of sprite
        public SpriteEffects spriteEffect = SpriteEffects.None; //used to flip the sprite horizontally or vertically
        public Rectangle drawRec = new Rectangle(0, 0, 0, 0); //what part of texture sprite wants to draw
        public Color drawColor = Color.White;   //used to tint the sprite a specific color, white = no tint
        public float scale = 1.0f;
        public float rotation = 0.0f;
        public Vector2 origin = new Vector2(0, 0);  //the pivot point/center point of sprite
        public float zDepth = 0.01f;    //the 'layer' this sprite is drawn on, based on Yposition
        public int zOffset = 0;         //offsets sorting value/layer + or - to ensure proper sort
        public Point frameOffset = new Point(0, 0); //offsets currentFrame by X,Y pixels

        public Sprite(ScreenManager ScreenManager, Texture2D Texture, Vector2 Position, Point Cellsize, Point CurrentFrame)
        {
            screenManager = ScreenManager;
            texture = Texture;
            position = Position;
            cellSize = Cellsize;
            currentFrame = CurrentFrame;
            CenterOrigin();
        }

        public void CenterOrigin() { origin.X = cellSize.X * 0.5f; origin.Y = cellSize.X * 0.5f; }

        public virtual void Draw()
        {
            if (shouldDraw)
            {   //set draw rec, draw sprite
                drawRec.Width = cellSize.X;
                drawRec.Height = cellSize.Y;
                drawRec.X = (cellSize.X * currentFrame.X) + frameOffset.X;
                drawRec.Y = (cellSize.Y * currentFrame.Y) + frameOffset.Y;
                screenManager.spriteBatch.Draw(    texture,
                                                   position,
                                                   drawRec,
                                                   drawColor * alpha,
                                                   rotation,
                                                   origin,
                                                   scale,
                                                   spriteEffect,
                                                   zDepth);
            }
        }
    }
}
