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
    public enum MouseButtons { LeftButton, RightButton }
    public class InputHelper
    {
        public ScreenManager screenManager;

        public KeyboardState currentKeyboardState;
        public MouseState currentMouseState;

        public KeyboardState lastKeyboardState;
        public MouseState lastMouseState;
        public Point lastCursorPosition;
        public Point cursorPosition; //used to hold vector2 position as integer

        public GamePadState currentGamePadState;
        public GamePadState lastGamePadState;
        public float deadzone = 0.10f; //the amount of joystick movement classified as noise

        public enum InputState //maps input from different devices into a single input state
        {
            Idle,               //no input at all
            MovingUp,           //all the possible movement states
            MovingUpRight,      //wasd keys supported
            MovingRight,        //arrow keys supported
            MovingDownRight,    //gamepad supported
            MovingDown,
            MovingDownLeft,
            MovingLeft,
            MovingUpLeft,
            Action,             //spacebar/a button press
        }
        public InputState inputState = InputState.Idle;

        public InputHelper(ScreenManager ScreenManager)
        {
            screenManager = ScreenManager;
            currentKeyboardState = new KeyboardState();
            currentMouseState = new MouseState();
            lastKeyboardState = new KeyboardState();
            lastMouseState = new MouseState();
            cursorPosition = new Point(0, 0);
            lastCursorPosition = new Point(0, 0);
            currentGamePadState = new GamePadState();
            lastGamePadState = new GamePadState();
        }

        public void InspectGamePad(int playerNum)
        {   //this method is used to determine the capabilities of a gamepad/controller connected to the user's computer
            GamePadCapabilities gpc = GamePad.GetCapabilities(playerNum);
            System.Diagnostics.Debug.WriteLine("---------------------------------------");
            System.Diagnostics.Debug.WriteLine("inspecting gamepad " + playerNum);
            System.Diagnostics.Debug.WriteLine("\t type: " + gpc.GamePadType);
            System.Diagnostics.Debug.WriteLine("\t has left X joystick: " + gpc.HasLeftXThumbStick);
            System.Diagnostics.Debug.WriteLine("\t has left Y joystick: " + gpc.HasLeftYThumbStick);
            System.Diagnostics.Debug.WriteLine("\t has A button: " + gpc.HasAButton);
            System.Diagnostics.Debug.WriteLine("\t has start button: " + gpc.HasStartButton);
        }

        public void Update(GameTime gameTime)
        {
            lastKeyboardState = currentKeyboardState;
            lastMouseState = currentMouseState;
            lastGamePadState = currentGamePadState;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);
            //we can attempt to get input from other connected controllers, but we only need 1 input for this game
            //if(IsNewKeyPress(Keys.Enter)) { InspectGamePad(1); InspectGamePad(2); InspectGamePad(3); InspectGamePad(4); }

            //track cursor position
            lastCursorPosition = cursorPosition;
            cursorPosition.X = (int)currentMouseState.X; //convert cursor position to int
            cursorPosition.Y = (int)currentMouseState.Y; //we don't need cursor position as a float


            #region Set InputState based on keyboard / gamepad input

            inputState = InputState.Idle;

            //check for gamepad movement input (diagonals first)
            if (currentGamePadState.ThumbSticks.Left.X > deadzone && currentGamePadState.ThumbSticks.Left.Y > deadzone) { inputState = InputState.MovingUpRight; }
            else if (currentGamePadState.ThumbSticks.Left.X < -deadzone && currentGamePadState.ThumbSticks.Left.Y > deadzone) { inputState = InputState.MovingUpLeft; }
            else if (currentGamePadState.ThumbSticks.Left.X > deadzone && currentGamePadState.ThumbSticks.Left.Y < -deadzone) { inputState = InputState.MovingDownRight; }
            else if (currentGamePadState.ThumbSticks.Left.X < -deadzone && currentGamePadState.ThumbSticks.Left.Y < -deadzone) { inputState = InputState.MovingDownLeft; }
            else if (currentGamePadState.ThumbSticks.Left.X > deadzone) { inputState = InputState.MovingRight; }
            else if (currentGamePadState.ThumbSticks.Left.X < -deadzone) { inputState = InputState.MovingLeft; }
            else if (currentGamePadState.ThumbSticks.Left.Y > deadzone) { inputState = InputState.MovingUp; }
            else if (currentGamePadState.ThumbSticks.Left.Y < -deadzone) { inputState = InputState.MovingDown; }

            //check for arrow key movement input (diagonals first)
            if (currentKeyboardState.IsKeyDown(Keys.Up) && currentKeyboardState.IsKeyDown(Keys.Right)) { inputState = InputState.MovingUpRight; }
            else if (currentKeyboardState.IsKeyDown(Keys.Up) && currentKeyboardState.IsKeyDown(Keys.Left)) { inputState = InputState.MovingUpLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.Down) && currentKeyboardState.IsKeyDown(Keys.Right)) { inputState = InputState.MovingDownRight; }
            else if (currentKeyboardState.IsKeyDown(Keys.Down) && currentKeyboardState.IsKeyDown(Keys.Left)) { inputState = InputState.MovingDownLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.Up)) { inputState = InputState.MovingUp; }
            else if (currentKeyboardState.IsKeyDown(Keys.Down)) { inputState = InputState.MovingDown; }
            else if (currentKeyboardState.IsKeyDown(Keys.Left)) { inputState = InputState.MovingLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.Right)) { inputState = InputState.MovingRight; }

            //check for wasd key movement input (diagonals first)
            if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.D)) { inputState = InputState.MovingUpRight; }
            else if (currentKeyboardState.IsKeyDown(Keys.W) && currentKeyboardState.IsKeyDown(Keys.A)) { inputState = InputState.MovingUpLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.D)) { inputState = InputState.MovingDownRight; }
            else if (currentKeyboardState.IsKeyDown(Keys.S) && currentKeyboardState.IsKeyDown(Keys.A)) { inputState = InputState.MovingDownLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.W)) { inputState = InputState.MovingUp; }
            else if (currentKeyboardState.IsKeyDown(Keys.S)) { inputState = InputState.MovingDown; }
            else if (currentKeyboardState.IsKeyDown(Keys.A)) { inputState = InputState.MovingLeft; }
            else if (currentKeyboardState.IsKeyDown(Keys.D)) { inputState = InputState.MovingRight; }

            //check for action and pause input
            if (IsNewButtonPress(Buttons.A)) { inputState = InputState.Action; }
            if (IsNewKeyPress(Keys.Space)) { inputState = InputState.Action; }

            #endregion


        }

        //check for keyboard key presses and releases
        public bool IsNewKeyPress(Keys key)
        { return (currentKeyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key)); }
        public bool IsKeyDown(Keys key) { return (currentKeyboardState.IsKeyDown(key)); }
        public bool IsNewKeyRelease(Keys key)
        { return (lastKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyUp(key)); }

        //check for gamepad button presses and releases
        public bool IsNewButtonPress(Buttons button)
        { return (currentGamePadState.IsButtonDown(button) && lastGamePadState.IsButtonUp(button)); }
        public bool IsNewButtonRelease(Buttons button)
        { return (lastGamePadState.IsButtonDown(button) && currentGamePadState.IsButtonUp(button)); }

        //Check to see the mouse button was pressed
        public bool IsNewMouseButtonPress(MouseButtons button)
        {
            if (button == MouseButtons.LeftButton)
            { return (currentMouseState.LeftButton == ButtonState.Pressed && lastMouseState.LeftButton == ButtonState.Released); }
            else if (button == MouseButtons.RightButton)
            { return (currentMouseState.RightButton == ButtonState.Pressed && lastMouseState.RightButton == ButtonState.Released); }
            else { return false; }
        }

        //Check to see the mouse button was released
        public bool IsNewMouseButtonRelease(MouseButtons button)
        {
            if (button == MouseButtons.LeftButton)
            { return (lastMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released); }
            else if (button == MouseButtons.RightButton)
            { return (lastMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released); }
            else { return false; }
        }

        //Check to see if the mouse button is being held down
        public Boolean IsMouseButtonDown(MouseButtons button)
        {
            if (button == MouseButtons.LeftButton)
            { return (currentMouseState.LeftButton == ButtonState.Pressed); }
            else if (button == MouseButtons.RightButton)
            { return (currentMouseState.RightButton == ButtonState.Pressed); }
            else { return false; }
        }
    }
}
