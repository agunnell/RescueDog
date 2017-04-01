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
    public class GameScreen : Screen
    {
        public ContentManager contentManager;   //the screen's content manager, but we don't load anything into it
        public Rectangle foregroundRec;         //fades out from initial screen load
        public float foregroundAlpha = 1.0f;    //foreground rec starts at 100% covering the game

        public Camera2D camera;
        public Rectangle cameraRec; //used to determine the viewing area of camera taking into account zoom/scale
        public Rectangle cullRec;   //used to cull unseen sprites/objs from drawing based on camerarec
        public Point cullBounds;    //determines the bottom right point of cullRec
        public bool cameraRoam;     //true = user can pan camera using spacebar + mouse drag, false = camera tracks dog hero

        public Actor dog;               //our hero, the dog
        public Rectangle barkRec;       //the dog's bark action collision rectangle
        public Sprite barkSprite;       //the dog's bark sprite 

        public Actor human;                 //the human
        public Rectangle humanLineOfSight;  //the human's line of sight collision rectangle
        public Sprite exclamationPoint;     //the exclamation point sprite that appears above the human's head

        public Level level;     //contains all the level data, changed each time a level is generated
        public LevelGenerator levelGenerator;   //procedurally generates cave dungeons/levels
        public int levelNumber = 1; //represents a level's difficulty: 1= easy, 2 = medium, 3 = hard, 4 = impossible

        public Stopwatch stopWatch = new Stopwatch(); //used to time various methods/code paths
        public TimeSpan updateTime;     //represents the time it takes to complete the Update() method
        public TimeSpan drawTime;       //represents the time it takes to complete the Draw() method

        public Editor editor;               //allows user developer input
        public TitleWidget titleWidget;     //displays game title, level info, won/lost game info
        public GameManager gameManager;     //handles various game logic related to dog, human, and spider actors
        public SoundManager soundManager;   //handles all music and soundfx playing/fading
        public GameTime gameTime;           //represents the gametime at the current frame


        public GameScreen() { this.name = "Game Screen"; }
        public override void LoadContent()
        {
            contentManager = screenManager.game.Content;
            contentManager.RootDirectory = "Content";
            soundManager = screenManager.game.soundManager;
            soundManager.PlayMusic(); //start playing all music tracks
            soundManager.musicTrack1Ins.Volume = 0.1f; //track1 should start a little audible
            soundManager.playTrack1 = true; //enable track1 fade in

            foregroundRec = new Rectangle(0, 0, 4096, 4906); //start foreground rec at massive size
            dog = new Actor(this, new Vector2(0, 0), Actor.Type.Dog); //create our hero dog instance
            //dog.checkCollisions = false; //we could enable clipping through walls like so
            barkRec = new Rectangle(0, 0, 128, 128); //bark rec's size doesn't change from 128x128
            barkSprite = new Sprite(screenManager, screenManager.game.spriteSheet, new Vector2(0, 0), new Point(16, 16), new Point(15, 3));

            human = new Actor(this, new Vector2(0, 0), Actor.Type.Human); //create our human instance
            humanLineOfSight = new Rectangle(0, 0, 128, 128); //human's line of sight rec size doesn't change from 128x128
            exclamationPoint = new Sprite(screenManager, screenManager.game.spriteSheet, new Vector2(0, 0), new Point(16, 16), new Point(15, 2));

            camera = new Camera2D(screenManager.game.GraphicsDevice);
            camera.currentPosition = new Vector2(0, 0);
            camera.targetPosition = new Vector2(0, 0);
            camera.targetZoom = 3.0f; //set the game's zoom
            camera.SetView();
            cameraRec = new Rectangle(-128, -128, 0, 0); //this matches what the camera can see
            cullRec = new Rectangle(0, 0, 0, 0); //this is a world position/size representation of cameraRec
            cameraRoam = false; //track the dog hero

            level = new Level(this);
            levelGenerator = new LevelGenerator(this);

            //generate an easy level 
            stopWatch.Reset(); stopWatch.Start();
            levelGenerator.GenerateLevel(levelNumber); //start on level 1
            stopWatch.Stop(); updateTime = stopWatch.Elapsed;
            //warp the camera to the dog's position
            camera.currentPosition = dog.sprite.position;
            camera.targetPosition = dog.sprite.position;

            if (screenManager.game.release == false)
            { System.Diagnostics.Debug.WriteLine("generator time: " + updateTime.Milliseconds + "ms"); }

            editor = new Editor(this);
            titleWidget = new TitleWidget(this);
            titleWidget.Open(TitleWidget.State.Level1);
            gameManager = new GameManager(this);
        }


        public override void HandleInput(InputHelper Input, GameTime GameTime)
        {   //pass input to dog and possibly editor
            dog.HandleInput(Input.inputState);
            gameManager.SetBark();
            gameManager.FollowHero(); //make the human follow the dog
            //human.HandleInput(Input.inputState, GameTime); //we could also control the human like so

            titleWidget.HandleInput(Input);
            if (screenManager.game.release == false)
            {
                editor.HandleInput(Input);
                if (Input.IsNewKeyPress(Keys.Enter)) { editor.playTesting = false; } //show editor
                //test beating a level, beating the game, losing the game
                if (Input.IsNewKeyPress(Keys.D1)) { titleWidget.Open(TitleWidget.State.Level1); }
                if (Input.IsNewKeyPress(Keys.D2)) { titleWidget.Open(TitleWidget.State.Level2); }
                if (Input.IsNewKeyPress(Keys.D3)) { titleWidget.Open(TitleWidget.State.Level3); }
                if (Input.IsNewKeyPress(Keys.D4)) { titleWidget.Open(TitleWidget.State.Level4); }
                if (Input.IsNewKeyPress(Keys.D5)) { titleWidget.Open(TitleWidget.State.GameOver); }
                if (Input.IsNewKeyPress(Keys.D6)) { titleWidget.Open(TitleWidget.State.WonGame); }
            }
        }


        public override void Update(GameTime GameTime)
        {
            gameTime = GameTime; //capture the current gametime
            stopWatch.Reset(); stopWatch.Start(); //being timing this Update() method

            //match viewport + fade out the initial foreground rec
            foregroundRec.Width = screenManager.game.GraphicsDevice.Viewport.Width;
            foregroundRec.Height = screenManager.game.GraphicsDevice.Viewport.Height;
            if (foregroundAlpha > 0.0) { foregroundAlpha -= 0.02f; } else { foregroundAlpha = 0.0f; }

            dog.Update(GameTime, level);
            human.Update(GameTime, level);
            //match human line of sight rec to human
            humanLineOfSight.X = (int)human.sprite.position.X - 64;
            humanLineOfSight.Y = (int)human.sprite.position.Y - 48;
            exclamationPoint.position.X = (int)human.sprite.position.X + 0;
            exclamationPoint.position.Y = (int)human.sprite.position.Y - 16;

            //if the camera isn't roaming, track the dog hero
            if (!cameraRoam) { camera.targetPosition = dog.sprite.position; camera.targetZoom = 3.0f; }
            camera.Update(GameTime);

            //Cull Floors, Debris, Walls, and GameObjs based on camera zoom + viewport size
            //match the viewport's width and height, keep camera rec at 0,0
            cameraRec.Width = screenManager.game.GraphicsDevice.Viewport.Width + 256;
            cameraRec.Height = screenManager.game.GraphicsDevice.Viewport.Height + 256;
            //place the cull rectangle at a world position that matches the camera rectangle's screen position
            cullRec.Location = camera.ConvertScreenToWorld(cameraRec.Location.X, cameraRec.Location.Y);
            //get the bounds of the camera view rec/viewport
            cullBounds = camera.ConvertScreenToWorld(cameraRec.Location.X + cameraRec.Width, cameraRec.Location.Y + cameraRec.Height);
            //set the cull rec's width and height based on cull bounds
            cullRec.Width = cullBounds.X - cullRec.Location.X;  //cullRec represents what the camera can see on screen currently
            cullRec.Height = cullBounds.Y - cullRec.Location.Y; //we use cullRec to hide sprites/objs that are not visible in the viewport
            //disable drawing of floors, walls, + gameobjs that do not intersect cullRec
            level.Cull(cullRec);    //this drastically reduces Draw() time
            level.Update(GameTime);

            gameManager.CheckForExit(); //check to see if human touches exit
            gameManager.HandleSpiders(GameTime); //handle AI for spiders
            titleWidget.Update(); //update any messages being displayed to the user 
            stopWatch.Stop(); updateTime = stopWatch.Elapsed;
        }


        public override void Draw(GameTime GameTime)
        {   //draw the game world through the camera.view
            stopWatch.Reset(); stopWatch.Start();
            screenManager.spriteBatch.Begin(
                        SpriteSortMode.BackToFront,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        camera.view
                        );
            //levelGenerator.Draw(); //this would draw the level generator's tile list - useful for debugging
            level.Draw(); //draw the floors, debris, walls, gameobjects, and actors
            dog.Draw();
            barkSprite.Draw();
            human.Draw();
            exclamationPoint.Draw();
            //draw cull rec - for testing/debug - should match camera rec
            //screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, cullRec, screenManager.game.colorDebugRed);

            if (editor.drawDebug) //if we're drawing debug info, draw the barkRec + human line of sight rec
            {
                screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, barkRec, screenManager.game.colorDebugRed);
                screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, humanLineOfSight, screenManager.game.colorDebugRed);
            }
            screenManager.spriteBatch.End();
            stopWatch.Stop(); drawTime = stopWatch.Elapsed;

            //reopen the sprite batch to draw the editor/cursor/title widget
            //we reopen the spritebatch and DO NOT pass it the camera.view
            //this draws these sprites/objects using screen positions, not world positions
            screenManager.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp);
            if (screenManager.game.release == false)
            {   //game is in dev mode, draw cursor + editor
                editor.Draw();
                //draw camera rec - for testing/debug - should match viewport
                //screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, cameraRec, screenManager.game.colorDebugRed); 
            }
            titleWidget.Draw();
            screenManager.spriteBatch.Draw(screenManager.game.dummyTexture, foregroundRec, screenManager.game.colorBackground * foregroundAlpha);
            screenManager.spriteBatch.End();
        }
    }
}