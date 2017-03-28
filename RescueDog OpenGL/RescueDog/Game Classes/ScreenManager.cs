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
    public class ScreenManager
    {   //manages screens - loads, updates, passes input, draws, and removes screens
        public Game1 game;
        public GameTime gameTime;
        public List<Screen> screens;
        public List<Screen> screensToUpdate;
        public SpriteBatch spriteBatch;
        public bool coveredByOtherScreen;
        public int transitionCount;
        public InputHelper input; //pass input to each screen

        public ScreenManager(Game1 Game)
        {
            game = Game;
            input = new InputHelper(this);
            screens = new List<Screen>();
            screensToUpdate = new List<Screen>();
        }

        public void Initialize() { spriteBatch = new SpriteBatch(game.GraphicsDevice); }
        public void UnloadContent() { foreach (Screen screen in screens) { screen.UnloadContent(); } }

        public void AddScreen(Screen screen)
        {
            screen.screenManager = this;
            screen.LoadContent();
            screens.Add(screen);
        }

        public void RemoveScreen(Screen screen)
        {
            screen.UnloadContent();
            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }
        public Screen[] GetScreens() { return screens.ToArray(); }

        public void ExitAndLoad(Screen screenToLoad)
        {   //remove every screen on screens list
            while (screens.Count > 0)
            {
                try
                {
                    screens[0].UnloadContent();
                    screensToUpdate.Remove(screens[0]);
                    screens.Remove(screens[0]);
                }
                catch { }
            }
            this.AddScreen(screenToLoad);
        }

        public void Update(GameTime GameTime)
        {
            gameTime = GameTime;    //capture the game's current time
            input.Update(gameTime); //read the keyboard and gamepad

            //make a copy of the master screen list, to avoid confusion if
            //the process of updating one screen adds or removes others
            screensToUpdate.Clear();
            foreach (Screen screen in screens) { screensToUpdate.Add(screen); }
            coveredByOtherScreen = false;

            //loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {   //remove the topmost screen from the waiting list
                Screen screen = screensToUpdate[screensToUpdate.Count - 1];
                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                if (coveredByOtherScreen == false) //targeting the top most screen
                {   //update & send input only to the top screen
                    screen.HandleInput(input, gameTime);
                    screen.Update(gameTime);
                    coveredByOtherScreen = true; //no update/input to screens below top
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(game.colorBackground); //draw the game's background color
            foreach (Screen screen in screens) { screen.Draw(gameTime); }
        }
    }
}