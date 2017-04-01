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
    public class Room
    {   //represents a room in a level
        public ScreenManager screenManager;
        public Rectangle rectangle;
        public Color color = Color.White;
        public enum Type { Spawn, Exit, None } //we aren't really using this information / type right now
        public Type type = Type.None;   //but we could use it to assign a purpose to a room, like a boss' lair
        public float zDepth = 0.01f;
        public Vector2 origin = new Vector2(0, 0);
        public Vector2 position = new Vector2(0, 0);
        public Room(ScreenManager ScreenManager) { screenManager = ScreenManager; rectangle = new Rectangle(0, 0, 0, 0); }

        public void Draw()
        {
            //screenManager.spriteBatch.Draw( screenManager.game.dummyTexture, rectangle, color * 0.25f);
            position.X = rectangle.Location.X;
            position.Y = rectangle.Location.Y;
            screenManager.spriteBatch.Draw( screenManager.game.dummyTexture,
                                            position,
                                            rectangle,
                                            color * 0.50f,
                                            0.0f,
                                            origin,
                                            1.0f,
                                            SpriteEffects.None,
                                            zDepth);
        }
    }
}
