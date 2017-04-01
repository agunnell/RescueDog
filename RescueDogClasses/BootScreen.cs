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
    class BootScreen : Screen
    {
        public ContentManager contentManager;   //we load our monogame logo into this content manager
        public Rectangle blackground;           //the background rectangle that covers the viewport
        public int displayTime = 120;           //how long the logo is displayed for, in frames @ 16ms / frame
        public int displayCounter = 0;          //counts the frames up to displayTime
        public Sprite monoLogo;                 //the monogame logo sprite thats drawn to the viewport
        Texture2D monoLogoTex;                  //the monogame texture that the monoLogo sprite uses
        public float fadeSpeed = 0.03f;         //how quickly the monogame logo fades in/out
        public enum DisplayState { Opening, Opened, Closing, Closed } //the various states the screen can be in
        public DisplayState displayState;       //the screen's current display state

        public BootScreen() { this.name = "Boot Screen"; }
        public override void LoadContent()
        {   //create a new content manager and load out monogame logo texture into it
            if (contentManager == null) { contentManager = new ContentManager(screenManager.game.Services, "Content"); }
            monoLogoTex = contentManager.Load<Texture2D>(@"MonoLogo"); //load the monogame logo texture
            monoLogo = new Sprite(screenManager, monoLogoTex, new Vector2(0, 0), new Point(256, 256), new Point(0, 0));
            monoLogo.alpha = 0.0f; //create the monogame logo sprite, but hide it for now - so it can nicely fade in
            displayState = DisplayState.Opening; //the initial screen state should be opening
            blackground = new Rectangle(0, 0, 0, 0); //we will match the rectangle to the viewport's dimensions in update
        }

        public override void UnloadContent() //unload content is called prior to a screen being removed
        {   //note that we force garbage collection (below) at a time when the user input/update/draw loops are not important
            contentManager.Unload();    //this unloads the monogame texture that we loaded
            contentManager.Dispose();   //this disposes the content manager, marking it for garbage collection
            GC.Collect(); //this forces garbage collector to run now, freeing up the memory used by contentManager
        }   //if we didn't force garbage collection now, in between screens, garbage collection may occur later during the game

        public override void Update(GameTime GameTime)
        {
            //match blackground width + height to viewport, done here so the user can resize the window
            blackground.Width = screenManager.game.GraphicsDevice.Viewport.Width;
            blackground.Height = screenManager.game.GraphicsDevice.Viewport.Height;
            //center logo to screen, again - the user may resize the window so we want the logo to stay centered
            monoLogo.position.X = screenManager.game.GraphicsDevice.Viewport.Width / 2;
            monoLogo.position.Y = screenManager.game.GraphicsDevice.Viewport.Height / 2;

            //manage the screen's display states
            if (displayState == DisplayState.Opening)
            {   //time this display state, then switch to logo
                displayCounter++;
                if (displayCounter >= displayTime / 2) //wait less before fading the logo in
                { displayCounter = 0; displayState = DisplayState.Opened; }
            }
            else if (displayState == DisplayState.Opened)
            {   //fade logo in
                if (monoLogo.alpha < 1.0f) { monoLogo.alpha += fadeSpeed; } else { monoLogo.alpha = 1.0f; }
                displayCounter++; //time this display state, then switch to closing
                if (displayCounter >= displayTime) { displayState = DisplayState.Closing; }
            }
            else if (displayState == DisplayState.Closing)
            {   //fade the logo out
                if (monoLogo.alpha > 0.0f) { monoLogo.alpha -= fadeSpeed; } else { monoLogo.alpha = 0.0f; displayState = DisplayState.Closed; }
            }
            else if (displayState == DisplayState.Closed)
            {   //logo has faded out, now we can load the game screens where our game takes place
                screenManager.ExitAndLoad(new GameScreen());    //add a game screen
                //screenManager.ExitAndLoad(new BootScreen());  //OR we can test garabge collection using this line
            }
        }

        public override void Draw(GameTime GameTime)
        {   //open the spritebatch, draw the background, draw the monogame logo, close the sprite batch
            screenManager.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, blackground, screenManager.game.colorBackground);
            monoLogo.Draw();
            screenManager.spriteBatch.End();
        }
    }
}