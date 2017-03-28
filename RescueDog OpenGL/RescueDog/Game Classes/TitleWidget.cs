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
    public class TitleWidget
    {   //displays game logo, level information, won/lost information to player
        public Sprite logo;     //the game's lofo (Rescue Dog logo)
        public Sprite text;     //information text displayed below logo (level 1, won game, lost game, try again?)
        public Rectangle backgroundRec; //partially transparent background fill
        public float alpha = 0.0f;  //how transparent the background is (allows player to still see game)
        public int displayTime = 200; //how long the widget displays, in frames (16ms)
        public int frameCounter = 0; //counts up to displayTime

        public enum State { Level1, Level2, Level3, Level4, GameOver, WonGame, } //the states of the titleWidget
        public State state; //titleWidget only has one state at a time and must wait for that state to close

        public enum DisplayState { Opening, Opened, Closing, Closed } //the display states of titleWidget
        public DisplayState displayState;   //each state starts with opening, then progresses to closed
        
        public GameScreen gameScreen;
        public TitleWidget(GameScreen GameScreen)
        {
            gameScreen = GameScreen;
            backgroundRec = new Rectangle(0, 0, 0, 0);
            logo = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.titleSheet, new Vector2(0, 0), new Point(32 * 4, 32 * 3), new Point(0, 0));
            text = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.titleSheet, new Vector2(0, 0), new Point(32 * 4, 32 * 1), new Point(0, 4));
            logo.scale = 3.0f; text.scale = 3.0f;
            logo.CenterOrigin();
        }


        public void Open(State State)
        {   //widget only opens using this Open() method
            if (displayState == DisplayState.Closed) //widget can only open from a closed state
            {
                state = State;
                displayState = DisplayState.Opening;
                frameCounter = 0;
                
                //set text current frame based on widget state
                if (state == State.Level1) { text.currentFrame.Y = 4; text.currentFrame.X = 0; }
                else if (state == State.Level2) { text.currentFrame.Y = 5; text.currentFrame.X = 0; }
                else if (state == State.Level3) { text.currentFrame.Y = 6; text.currentFrame.X = 0; }
                else if (state == State.Level4) { text.currentFrame.Y = 7; text.currentFrame.X = 0; }
                else if (state == State.GameOver) { text.currentFrame.Y = 3; text.currentFrame.X = 0; }
                else if (state == State.WonGame) { text.currentFrame.Y = 3; text.currentFrame.X = 1; }
                text.CenterOrigin();
            }
        }


        public void HandleInput(InputHelper Input)
        {
            if (displayState == DisplayState.Opened)
            {   //hold widget open until user presses action key/button
                if (state == State.GameOver || state == State.WonGame)
                {
                    frameCounter = 0; //hold widget open             
                    if (Input.inputState == InputHelper.InputState.Action)
                    {   //action input closes the widget
                        displayState = DisplayState.Closing;
                        if (state == State.GameOver)
                        {
                            gameScreen.levelGenerator.GenerateLevel(gameScreen.levelNumber); //generate new level
                            gameScreen.screenManager.game.soundManager.wonGameIns.Play(); //play the wongame soundfx
                        } 
                        else if (state == State.WonGame) { gameScreen.screenManager.AddScreen(new WonScreen()); } //add a won screen
                    }
                }
            }
        }


        public void Update()
        {   //match bkg rec to viewport size
            backgroundRec.Width = gameScreen.screenManager.game.GraphicsDevice.Viewport.Width;
            backgroundRec.Height = gameScreen.screenManager.game.GraphicsDevice.Viewport.Height;
            //center logo to viewport
            logo.position.X = gameScreen.screenManager.game.GraphicsDevice.Viewport.Width / 2;
            logo.position.Y = (gameScreen.screenManager.game.GraphicsDevice.Viewport.Height / 2) + 10;
            //center text
            text.position.X = logo.position.X;
            text.position.Y = logo.position.Y + 230 + 0;
            //manage displayStates
            if (displayState == DisplayState.Opening) //fade in, switch to opened
            { if (alpha < 1.0f) { alpha += 0.05f; } else { alpha = 1.0f; displayState = DisplayState.Opened; } }
            else if (displayState == DisplayState.Opened) //count display frames, switch to closing
            { frameCounter++; if (frameCounter > displayTime) { displayState = DisplayState.Closing; } }
            else if (displayState == DisplayState.Closing) //fade out, switch to closed
            { if (alpha > 0.0f) { alpha -= 0.05f; } else { alpha = 0.0f; displayState = DisplayState.Closed; } }
            else if (displayState == DisplayState.Closed) { } //wait for open()
        }


        public void Draw()
        {
            gameScreen.screenManager.spriteBatch.Draw(
                gameScreen.screenManager.game.dummyTexture, 
                backgroundRec, 
                gameScreen.screenManager.game.colorBackground * alpha * 0.9f);
            logo.alpha = alpha;
            text.alpha = alpha;
            logo.Draw();
            text.Draw();
        }
    }
}
