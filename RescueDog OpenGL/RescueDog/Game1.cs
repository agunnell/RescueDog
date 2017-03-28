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
    public class Game1 : Game
    {   //set this boolean to true prior to release - false = editor, true = no editor
        public Boolean release = false;
        public String version = "1.0";



        public GraphicsDeviceManager graphics;
        public ScreenManager screenManager;
        public SoundManager soundManager;

        public Texture2D spriteSheet; //sprite sheet for level + actors
        public Texture2D titleSheet; //sheet for logo + game feedback (game over, level 1, game won, etc...)
        public SpriteFont font; //the game's font, used by any screen
        public Texture2D dummyTexture; //texture used to draw rectangles

        public Color colorDarkGray = Color.FromNonPremultiplied(33, 33, 33, 256);
        public Color colorDebugRed = Color.FromNonPremultiplied(255, 0, 0, 100);
        public Color colorBackground = new Color(25, 23, 22);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "RescueDog v" + version;
            screenManager = new ScreenManager(this);
            soundManager = new SoundManager(this);

            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 540;

            Window.Position = new Point(200, 200);
            this.IsMouseVisible = false;
            Window.AllowUserResizing = true;
        }
        protected override void Initialize() { base.Initialize(); }
        protected override void LoadContent()
        {
            base.LoadContent();
            //we can support different languages by loading different title sheets
            titleSheet = Content.Load<Texture2D>(@"English");
            //titleSheet = Content.Load<Texture2D>(@"Japanese");

            spriteSheet = Content.Load<Texture2D>(@"SpriteSheet");
            font = Content.Load<SpriteFont>(@"FONT_Bark6pt");
            dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });

            screenManager.Initialize();
            soundManager.LoadContent();

            //if the game is in release mode, display a boot screen as the first screen, otherwise bypass it
            if (release) { screenManager.AddScreen(new BootScreen()); }
            else { screenManager.AddScreen(new GameScreen()); }
        }
        protected override void UnloadContent() { }
        protected override void Update(GameTime gameTime)
        {
            screenManager.Update(gameTime);
            soundManager.Update();
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) { screenManager.Draw(gameTime); base.Draw(gameTime); }
    }
}