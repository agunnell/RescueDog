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
    class WonScreen : Screen
    {
        //fades in background
        //displays text "thanks for playing!" with animated dog
        //text + dog fade out
        //screen exits, removing all other screens and loading boot screen, game resets

        public ContentManager contentManager;
        public Rectangle blackground;
        public float blackgroundAlpha;
        public int displayTime = 400; //in frames @ 16ms / frame
        public int displayCounter = 0;

        public Sprite thanksText;
        public Sprite dog;
        public List<Point> dogAnimation;
        public int animationCounter = 0;
        public float timer = 0;                 //tracks how much time has elapsed since last tick
        public float animFPS = 80.0f;           //limits animation speed, higher is slower

        public enum DisplayState { Opening, Opened, Closing, Closed }
        public DisplayState displayState;


        public WonScreen() { this.name = "Won Screen"; }


        public override void LoadContent()
        {
            contentManager = screenManager.game.Content;
            contentManager.RootDirectory = "Content";

            displayState = DisplayState.Opening;
            blackground = new Rectangle(0, 0, 0, 0);
            blackgroundAlpha = 0.0f;

            thanksText = new Sprite(screenManager, screenManager.game.titleSheet, new Vector2(0, 0), new Point(32 * 4, 32 * 2), new Point(1, 0));
            thanksText.scale = 3.0f;
            dog = new Sprite(screenManager, screenManager.game.spriteSheet, new Vector2(-500, -500), new Point(32, 32), new Point(0, 2));
            dog.scale = 3.0f;

            dogAnimation = new List<Point>();

            dogAnimation.Add(new Point(0, 2)); //standing still
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));

            dogAnimation.Add(new Point(3, 3)); //tail wagging
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(4, 3));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(3, 3)); //repeat
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(4, 3));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(3, 3)); //repeat
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(4, 3));
            dogAnimation.Add(new Point(0, 2));

            dogAnimation.Add(new Point(0, 2)); //standing still
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));
            dogAnimation.Add(new Point(0, 2));

            dogAnimation.Add(new Point(7, 2)); //lower head
            dogAnimation.Add(new Point(6, 2));
            dogAnimation.Add(new Point(5, 2)); //head down, tail wagging
            dogAnimation.Add(new Point(7, 3));
            dogAnimation.Add(new Point(5, 2)); //repeat
            dogAnimation.Add(new Point(7, 3));
            dogAnimation.Add(new Point(5, 2)); //repeat
            dogAnimation.Add(new Point(7, 3));
            dogAnimation.Add(new Point(5, 2)); //repeat
            dogAnimation.Add(new Point(7, 3));
            dogAnimation.Add(new Point(5, 2)); //repeat
            dogAnimation.Add(new Point(7, 3));
            dogAnimation.Add(new Point(6, 2)); //head up
            dogAnimation.Add(new Point(7, 2));
        }


        public override void Update(GameTime GameTime)
        {
            //match blackground width + height to viewport
            blackground.Width = screenManager.game.GraphicsDevice.Viewport.Width;
            blackground.Height = screenManager.game.GraphicsDevice.Viewport.Height;

            //center thanks text
            thanksText.position.X = (screenManager.game.GraphicsDevice.Viewport.Width / 2) - 0;
            thanksText.position.Y = (screenManager.game.GraphicsDevice.Viewport.Height / 2) + 155;

            //center dog
            dog.position.X = (screenManager.game.GraphicsDevice.Viewport.Width / 2) - 0;
            dog.position.Y = (screenManager.game.GraphicsDevice.Viewport.Height / 2) - 75;

            //handle display states
            if (displayState == DisplayState.Opening)
            {   //fade in background, once fully faded in switch to opened state
                if (blackgroundAlpha < 1.0f) { blackgroundAlpha += 0.05f; }
                else { blackgroundAlpha = 1.0f; displayState = DisplayState.Opened; }
                //match dog and thanks text alpha to background (fade in at same time)
                dog.alpha = blackgroundAlpha;
                thanksText.alpha = blackgroundAlpha;
            }
            else if (displayState == DisplayState.Opened)
            {   //count up to display time, then switch to closing state
                displayCounter++;
                if (displayCounter >= displayTime) { displayState = DisplayState.Closing; }

                timer += (float)GameTime.ElapsedGameTime.TotalMilliseconds;
                if (timer >= animFPS)
                {
                    timer = 0;
                    //animate dog using animation counter, loop
                    animationCounter++;
                    if (animationCounter >= dogAnimation.Count) { animationCounter = 0; }
                    dog.currentFrame = dogAnimation[animationCounter];
                }
            }
            else if (displayState == DisplayState.Closing)
            {   //fade out dog + text, once fully faded out switch to closed state
                if (dog.alpha > 0.0f) { dog.alpha -= 0.05f; } else { dog.alpha = 0.0f; displayState = DisplayState.Closed; }
                thanksText.alpha = dog.alpha;
            }
            else if (displayState == DisplayState.Closed)
            {   //reset the game, load title screen
                screenManager.ExitAndLoad(new BootScreen());
            }

            //mute all playing music
            screenManager.game.soundManager.playTrack1 = false;
            screenManager.game.soundManager.playTrack2 = false;
            screenManager.game.soundManager.playTrack3 = false;
        }


        public override void Draw(GameTime GameTime)
        {
            screenManager.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, blackground, screenManager.game.colorBackground * blackgroundAlpha);
            thanksText.Draw();
            dog.Draw();
            screenManager.spriteBatch.End();
        }
    }
}