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
    public abstract class Screen
    {   //represents a screen that is managed by screen manager
        public ScreenManager screenManager;
        public string name = "New";
        public Screen() { }
        public virtual void LoadContent() {}
        public virtual void UnloadContent() {}
        public virtual void HandleInput(InputHelper input, GameTime gameTime) { }
        public virtual void Update(GameTime gameTime) {}
        public virtual void Draw(GameTime gameTime) {}
    }
}