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
    public class Button
    {
        ScreenManager screenManager;
        public Rectangle rectangle; //the buttons background rectangle
        public Text text;           //the buttons foreground text

        public Color upColor = new Color(44,44,44);
        public Color overColor = new Color(66, 66, 66);
        public Color downColor = new Color(100, 100, 100);
        public Color drawColor;     //points to one of the above colors
        public Color selectedColor; //points to one of the above colors
        public bool selected = false;

        public enum ButtonState { Up, Over, Down } //up = normal, over = mouse over, down = mouse click
        public ButtonState buttonState = ButtonState.Up;

        public Button(ScreenManager ScreenManager, string Text, Point Position, Point Size)
        {
            screenManager = ScreenManager;
            rectangle = new Rectangle(Position, Size);
            text = new Text(screenManager, Text, new Vector2(0, 0));
            CenterText();
            drawColor = upColor;
            selectedColor = downColor;
        }

        public void CenterText()
        {
            float textWidth = screenManager.game.font.MeasureString(text.text).X; //measure string
            text.position.X = (rectangle.Location.X + rectangle.Width / 2) - (textWidth / 2); //center text to button
            //if the textWidth is odd, it blurs, so add 0.5f to any odd sized text
            if (textWidth % 2 != 0) { text.position.X += 0.5f; } //if textWidth is odd offset by 1/2 pixel to keep it sharp/on whole number
            text.position.Y = rectangle.Location.Y + 1; //center text vertically
        }

        public void Draw()
        {
            if (selected) //if selected, draw rec using the selected color, else draw rec using the draw color
            { screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, rectangle, selectedColor); }
            else { screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, rectangle, drawColor); }
            text.Draw();
        }
    }
}
