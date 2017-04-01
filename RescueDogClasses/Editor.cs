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
    public class Editor
    {
        //editor displays dev info, allows user to click editor buttons
        //buttons allow user to playtest, set debug mode, save/load level, generate a new level, and roam the camera
        public GameScreen gameScreen;       //editor targets a game screen

        public Sprite cursorSprite;                     //we display a custom cursor sprite
        public Point cursorPointer = new Point(12, 3);  //the default cursor sprite - a hand pointing
        public Point cursorHand = new Point(13, 3);     //when we roam the camera via spacebar, the cursor becomes an open hand
        public Point cursorFist = new Point(14, 3);     //when we drag the camera, the cursor becomes a closed hand

        public Rectangle topRec;        //the top menu background rectangle
        public Text feedbackText;       //the top left update/draw timing text

        public Button buttonPlaytest;   //the top menu buttons
        public Button buttonDrawDebug;
        public Button buttonNewLevel;
        public Button buttonRoam;

        public List<Button> buttons;        //top menu buttons are placed in a buttons list
        public int counter;                 //generic counter used by editor
        public bool playTesting = false;    //true = hides editor, simulates release state
        public Boolean drawDebug = false;   //draw collision recs, room recs, bark rec, human line of sight


        public Editor(GameScreen GameScreen)
        {
            gameScreen = GameScreen;

            topRec = new Rectangle(0, 0, 0, 32);
            feedbackText = new Text(gameScreen.screenManager, "Init", new Vector2(6, 2));
            buttons = new List<Button>();

            int buttonHeight = 19; //the height of all the top menu buttons
            int buttonPadding = 6; //the padding between buttons (horizontally)
            buttonPlaytest = new Button(gameScreen.screenManager, "playtest (enter)", 
                new Point(75, 6), new Point(130, buttonHeight));
            buttonDrawDebug = new Button(gameScreen.screenManager, "debug", 
                new Point(buttonPlaytest.rectangle.X + buttonPlaytest.rectangle.Width + buttonPadding, 6), new Point(60, buttonHeight));
            buttonNewLevel = new Button(gameScreen.screenManager, "new",
                new Point(buttonDrawDebug.rectangle.X + buttonDrawDebug.rectangle.Width + buttonPadding, 6), new Point(60, buttonHeight));
            buttonRoam = new Button(gameScreen.screenManager, "roam",
                new Point(buttonNewLevel.rectangle.X + buttonNewLevel.rectangle.Width + buttonPadding, 6), new Point(60, buttonHeight));

            buttons.Add(buttonPlaytest);    //add all top menu buttons to the buttons list
            buttons.Add(buttonDrawDebug);   //this makes it easier to handle the buttons over and click states 
            buttons.Add(buttonNewLevel);    //this is done in handle input() below
            buttons.Add(buttonRoam);

            cursorSprite = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, new Vector2(0, 0), new Point(16, 16), cursorPointer);
        }


        public void HandleInput(InputHelper Input)
        {
            if (playTesting == false) //we are not simulating the game in release mode
            {   //set the cursor sprite to the cursor pointer frame
                cursorSprite.currentFrame = cursorPointer;
                //match cursor sprite to mouse cursor position
                cursorSprite.position.X = Input.cursorPosition.X + 6;
                cursorSprite.position.Y = Input.cursorPosition.Y + 6;
                //match the top gray rectangle to the viewport's width
                topRec.Width = gameScreen.screenManager.game.GraphicsDevice.Viewport.Width;


                #region Handle button input/update

                for (counter = 0; counter < buttons.Count; counter++)
                {   //loop through all buttons on the buttons list
                    if (buttons[counter].rectangle.Contains(Input.cursorPosition))
                    {   //set the current buttons over state if the cursor collides with it's background rec
                        buttons[counter].drawColor = buttons[counter].overColor; //set the button rec's draw color to overColor

                        if (Input.IsNewMouseButtonPress(MouseButtons.LeftButton))
                        {   //if user clicked on the button, set the buttons rec's draw color to downCOlor
                            buttons[counter].drawColor = buttons[counter].downColor;

                            //check to see which button user clicked on
                            if (buttons[counter] == buttonPlaytest)
                            {
                                playTesting = true; //hide editor, playTesting can be set false by enter key
                                drawDebug = false; buttonDrawDebug.selected = false;    //set debug and roam false
                                gameScreen.cameraRoam = false; buttonRoam.selected = false;
                            }
                            else if (buttons[counter] == buttonDrawDebug)
                            {   //toggle game.drawDebug true/false
                                if (drawDebug == false)
                                { drawDebug = true; buttonDrawDebug.selected = true; }
                                else { drawDebug = false; buttonDrawDebug.selected = false; }
                            }
                            else if (buttons[counter] == buttonNewLevel)
                            {   //create a new level and time it, cycling through the level difficulties
                                gameScreen.stopWatch.Reset(); gameScreen.stopWatch.Start();
                                gameScreen.levelNumber++; //test level progression
                                if (gameScreen.levelNumber > 4) { gameScreen.levelNumber = 1; } //cycle back to level 1
                                gameScreen.levelGenerator.GenerateLevel(gameScreen.levelNumber); //generate a new level
                                gameScreen.stopWatch.Stop(); gameScreen.updateTime = gameScreen.stopWatch.Elapsed;
                                System.Diagnostics.Debug.WriteLine(
                                    "generated " + gameScreen.levelGenerator.difficulty +
                                    " level in: " + gameScreen.updateTime.Milliseconds + "ms");
                            }
                            else if (buttons[counter] == buttonRoam)
                            {   //toggle gamescreen.cameraRoam true/false
                                if (gameScreen.cameraRoam == false)
                                { gameScreen.cameraRoam = true; buttonRoam.selected = true; }
                                else { gameScreen.cameraRoam = false; buttonRoam.selected = false; }
                            }
                        }
                    }
                    else { buttons[counter].drawColor = buttons[counter].upColor; } //set untouched buttons to upcolor
                }

                #endregion


                #region Handle keyboard input

                if (gameScreen.cameraRoam)
                {   //if the camera is allowed to roam
                    if (Input.IsKeyDown(Keys.Space))
                    {   //allow camera panning via photoshop spacebar slide drag
                        cursorSprite.currentFrame = cursorHand; //switch to open hand
                        if (Input.IsMouseButtonDown(MouseButtons.LeftButton))
                        {
                            cursorSprite.currentFrame = cursorFist; //switch to closed hand
                            if (Input.lastCursorPosition.X > Input.cursorPosition.X) //draw horizontally based on cursor movement
                            { gameScreen.camera.targetPosition.X += Math.Abs(Input.lastCursorPosition.X - Input.cursorPosition.X); }
                            else if (Input.lastCursorPosition.X < Input.cursorPosition.X)
                            { gameScreen.camera.targetPosition.X -= Math.Abs(Input.lastCursorPosition.X - Input.cursorPosition.X); }

                            if (Input.lastCursorPosition.Y > Input.cursorPosition.Y) //draw vertically based on cursor movement
                            { gameScreen.camera.targetPosition.Y += Math.Abs(Input.lastCursorPosition.Y - Input.cursorPosition.Y); }
                            else if (Input.lastCursorPosition.Y < Input.cursorPosition.Y)
                            { gameScreen.camera.targetPosition.Y -= Math.Abs(Input.lastCursorPosition.Y - Input.cursorPosition.Y); }
                        }
                    }
                    
                    if (Input.IsKeyDown(Keys.RightControl))
                    {   //photoshop zoom in/out using ctrl+ or ctrl-
                        if (Input.IsNewKeyPress(Keys.OemPlus)) { gameScreen.camera.targetZoom = gameScreen.camera.targetZoom * 1.25f; }
                        if (Input.IsNewKeyPress(Keys.OemMinus)) { gameScreen.camera.targetZoom = gameScreen.camera.targetZoom * 0.75f; }
                    }
                }

                #endregion


            }
        }


        public void Draw()
        {   //if we are simulating the game in release mode, hide editor
            if (playTesting == false)
            {   //else, draw editor
                gameScreen.screenManager.spriteBatch.Draw(
                    gameScreen.screenManager.game.dummyTexture,
                    topRec, gameScreen.screenManager.game.colorDarkGray);
                //draw the feedback (timing) text
                feedbackText.text = "u:" + gameScreen.updateTime.Milliseconds + "ms";
                feedbackText.text += "\nd:" + gameScreen.drawTime.Milliseconds + "ms";

                //we can display information about actors like so...
                /*
                feedbackText.text += "\n\ndog";
                feedbackText.text += "\ninput:" + gameScreen.dog.inputState;
                feedbackText.text += "\nanim:" + gameScreen.dog.animationState;
                feedbackText.text += "\nstatelocked:" + gameScreen.dog.stateLocked;
                feedbackText.text += "\n" + gameScreen.dog.moveDirection;
                
                feedbackText.text += "\n\nhuman";
                feedbackText.text += "\ninput:" + gameScreen.human.inputState;
                feedbackText.text += "\nanim:" + gameScreen.human.animationState;
                feedbackText.text += "\nstatelocked:" + gameScreen.human.stateLocked;
                feedbackText.text += "\n" + gameScreen.human.moveDirection;
                */

                feedbackText.Draw();

                //draw all the editor buttons + cursor
                for (counter = 0; counter < buttons.Count; counter++) { buttons[counter].Draw(); }
                cursorSprite.Draw();
            }
        }
    }
}