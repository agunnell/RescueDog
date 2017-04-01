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
    public class Level
    {
        GameScreen gameScreen;
        public List<Sprite> floors;
        public List<Sprite> debris;
        public List<Wall> walls;
        public List<GameObject> gameobjects;
        public List<Room> rooms;
        public List<Actor> actors;
        public GameObject exitObj;
        public int counter;
        public int total;


        public void Sort() //sorts floors, debris, walls, gameobjects, actors to a zDepth layer
        {   //set all floors to zDepth 0.999999f
            total = floors.Count; for (counter = 0; counter < total; counter++) { floors[counter].zDepth = 0.999999f; }
            //set all debris to zDepth 0.999998f
            total = debris.Count; for (counter = 0; counter < total; counter++) { debris[counter].zDepth = 0.999998f; }
            //sort walls onto layers 0.999990 to 0.000001 based on position.Y + zOffset
            total = walls.Count;
            for (counter = 0; counter < total; counter++)
            { walls[counter].sprite.zDepth = 0.999990f - ((walls[counter].sprite.position.Y + walls[counter].sprite.zOffset) * 0.000001f); }
            //sort gameobjs onto layers 0.999990 to 0.000001 based on position.Y + zOffset
            total = gameobjects.Count;
            for (counter = 0; counter < total; counter++)
            { gameobjects[counter].sprite.zDepth = 0.999990f - ((gameobjects[counter].sprite.position.Y + gameobjects[counter].sprite.zOffset) * 0.000001f); }
        }


        public Level(GameScreen GameScreen) { gameScreen = GameScreen; ClearLevel(); }


        public void ClearLevel()
        {   //clear all the level data
            floors = new List<Sprite>();
            debris = new List<Sprite>();
            walls = new List<Wall>();
            gameobjects = new List<GameObject>();
            rooms = new List<Room>();
            actors = new List<Actor>();
        }


        public void Cull(Rectangle rectangle)
        {   //set draw boolean for any floors, debris, walls, gameobjects, and actors contained within rectangle
            total = floors.Count;
            for (counter = 0; counter < total; counter++)
            {
                if (rectangle.Contains(floors[counter].position))
                { floors[counter].shouldDraw = true; } else { floors[counter].shouldDraw = false; }
            }
            total = debris.Count;
            for (counter = 0; counter < total; counter++)
            {
                if (rectangle.Contains(debris[counter].position))
                { debris[counter].shouldDraw = true; }
                else { debris[counter].shouldDraw = false; }
            }
            total = walls.Count;
            for (counter = 0; counter < total; counter++)
            {
                if (rectangle.Contains(walls[counter].sprite.position))
                { walls[counter].sprite.shouldDraw = true; }
                else { walls[counter].sprite.shouldDraw = false; }
            }
            total = gameobjects.Count;
            for (counter = 0; counter < total; counter++)
            {
                if (rectangle.Contains(gameobjects[counter].sprite.position))
                { gameobjects[counter].sprite.shouldDraw = true; }
                else { gameobjects[counter].sprite.shouldDraw = false; }
            }
            total = actors.Count;
            for (counter = 0; counter < total; counter++)
            {
                if (rectangle.Contains(actors[counter].sprite.position))
                { actors[counter].sprite.shouldDraw = true; }
                else { actors[counter].sprite.shouldDraw = false; }
            }
        }


        public void Update(GameTime GameTime)
        {   //update (animate) the level's gameobjects
            total = gameobjects.Count;
            for (counter = 0; counter < total; counter++) { gameobjects[counter].Update(GameTime); }
        }


        public void Draw()
        {   //draw the floors, debris, walls, gameobjects, and actors
            total = floors.Count;
            for (counter = 0; counter < total; counter++) { floors[counter].Draw(); }
            total = debris.Count;
            for (counter = 0; counter < total; counter++) { debris[counter].Draw(); }
            total = walls.Count;
            for (counter = 0; counter < total; counter++) { walls[counter].Draw(); }
            total = gameobjects.Count;
            for (counter = 0; counter < total; counter++) { gameobjects[counter].Draw(); }
            total = actors.Count;
            for (counter = 0; counter < total; counter++) { actors[counter].Draw(); }
            if (gameScreen.editor.drawDebug)
            {   //only draw room recs if game is in debug mode
                total = rooms.Count;
                for (counter = 0; counter < total; counter++) { rooms[counter].Draw(); }
            }
        }
    }
}
