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
    public class LevelGenerator
    {
        //level generator creates a level based on presets controlled by level difficulty
        //level generation occurs in GenerateLevel() - all other class methods are called in GenerateLevel()
        //there are several methods, but they are kept short and readable - except for GrowConnected()
        //GrowConnected() could be refactored to be cleaner / easier to understand - but it's well commented
        //BuildWalls() can be difficult to understand as well - just know it builds walls around tiles/floors

        //level variables - these are set in GenerateLevel() based on difficulty
        public static int levelWidth = 60;      //dungeon width in tiles/cellsize
        public static int levelHeight = 30;     //dungeon height in tiles/cellsize
        public int maxRoomWidth = 9;            //the maximum room width/height
        public int maxRoomHeight = 9;
        public int minRoomWidth = 3;            //the minimum room width/height
        public int minRoomHeight = 3;
        public int createRoomAttempts = 200;    //how many times LG attempts to place a room inside of tile grid

        public enum Difficulty { Easy, Medium, Hard, Impossible }
        public Difficulty difficulty = Difficulty.Easy;

        //level constants
        public int cellsize = 32;           //in pixels
        public int startPosition = 256;     //tilegrid is built at 256x256 cellsizes away from 0,0
        public int minimumSize = levelWidth * levelHeight / 3;  //dungeons smaller than minimumSize will be fixed or remade

        GameScreen gameScreen;
        Level level;
        int counterA;   //generic counter
        int counterB;   //generic counter
        Stopwatch stopWatch = new Stopwatch();
        //TimeSpan timeSpan;            //used to time various methods / overall progress
        Random random = new Random();   //quickly generates a random number            
        public List<Tile> tileList;     //holds all tiles created

        [Serializable]
        public class Tile
        {   //the tile class that tileList is built from
            public Rectangle rectangle;
            public int index = 0;
            public bool filled = false;
            public bool connected = false;
            public Color color = Color.Black;
            //NSEW neighbors
            public int neighborNindex = -1; //-1 means this tile  
            public int neighborSindex = -1; //doesn't have a neighbor
            public int neighborEindex = -1; //in that position
            public int neighborWindex = -1;
            //DIAG neighbors
            public int neighborNEindex = -1;
            public int neighborNWindex = -1;
            public int neighborSEindex = -1;
            public int neighborSWindex = -1;
        }

        //used to check if a tile has proper neighbors (used for debugging neighbors)
        public void CheckTile(int index)
        {
            tileList[index].color = Color.White;
            //set tile's N neighbors
            try { tileList[tileList[index].neighborNindex].color = Color.Red; } catch { }
            try { tileList[tileList[index].neighborNEindex].color = Color.Red; } catch { }
            try { tileList[tileList[index].neighborNWindex].color = Color.Red; } catch { }
            //set tile's EW neighbors
            try { tileList[tileList[index].neighborEindex].color = Color.Green; } catch { }
            try { tileList[tileList[index].neighborWindex].color = Color.Green; } catch { }
            //set tile's S neighbors
            try { tileList[tileList[index].neighborSindex].color = Color.Blue; } catch { }
            try { tileList[tileList[index].neighborSEindex].color = Color.Blue; } catch { }
            try { tileList[tileList[index].neighborSWindex].color = Color.Blue; } catch { }

            /*
            int column = (tileList[index].rectangle.X - cellsize * 256) / cellsize;
            int row = (tileList[index].rectangle.Y - cellsize * 256) / cellsize;
            System.Diagnostics.Debug.WriteLine("tile X, Y: " + column + ", " + row);
            */
        }












        public LevelGenerator( GameScreen GameScreen) { gameScreen = GameScreen; level = gameScreen.level; }


        public void BuildTileList()
        {   //creates a list of tiles of cellsize, at GridSize
            int index = 0;
            int totalTiles = levelWidth * levelHeight;
            for (counterA = 0; counterA < levelWidth; counterA++)
            {
                for (counterB = 0; counterB < levelHeight; counterB++)
                {
                    Tile tile = new Tile();
                    tile.rectangle = new Rectangle(new Point(0, 0), new Point(cellsize, cellsize));
                    tile.rectangle.X = (cellsize * startPosition) + (cellsize * counterA);
                    tile.rectangle.Y = (cellsize * startPosition) + (cellsize * counterB);
                    tile.filled = false;
                    tile.index = index;
                    index++;

                    //always add E/W neighbors
                    if (tile.index - levelHeight >= 0) { tile.neighborWindex = tile.index - levelHeight; } //W
                    if (tile.index + levelHeight >= 0) { tile.neighborEindex = tile.index + levelHeight; } //E

                    if (counterB != 0) //dont add north neighbors if tile is on top row
                    {
                        if (tile.index - 1 >= 0) { tile.neighborNindex = tile.index - 1; } //N
                        if (tile.index - 1 - levelHeight >= 0) { tile.neighborNWindex = tile.index - 1 - levelHeight; } //NW
                        if (tile.index - 1 + levelHeight >= 0) { tile.neighborNEindex = tile.index - 1 + levelHeight; } //NE
                    }
                    if (counterB != levelHeight - 1) //dont add south neighbors if tile is on bottom row
                    {
                        if (tile.index + 1 >= 0) { tile.neighborSindex = tile.index + 1; } //S
                        if (tile.index + 1 - levelHeight >= 0) { tile.neighborSWindex = tile.index + 1 - levelHeight; } //SW
                        if (tile.index + 1 + levelHeight >= 0) { tile.neighborSEindex = tile.index + 1 + levelHeight; } //SE
                    }

                    //if the tile's neighbor exceeds the total number of tiles, then set that neighbor to be empty (-1)
                    if (tile.neighborNindex >= totalTiles) { tile.neighborNindex = -1; }
                    if (tile.neighborNEindex >= totalTiles) { tile.neighborNEindex = -1; }
                    if (tile.neighborNWindex >= totalTiles) { tile.neighborNWindex = -1; }
                    if (tile.neighborEindex >= totalTiles) { tile.neighborEindex = -1; }
                    if (tile.neighborWindex >= totalTiles) { tile.neighborWindex = -1; }
                    if (tile.neighborSindex >= totalTiles) { tile.neighborSindex = -1; }
                    if (tile.neighborSEindex >= totalTiles) { tile.neighborSEindex = -1; }
                    if (tile.neighborSWindex >= totalTiles) { tile.neighborSWindex = -1; }

                    tileList.Add(tile);
                }
            }
        }

        
        public void CreateRoom()
        {
            Room newRoom = new Room(gameScreen.screenManager); //creates room at 0,0

            //randomly assign a width and height of room between min/max
            newRoom.rectangle.Width = random.Next(minRoomWidth, maxRoomWidth);
            newRoom.rectangle.Height = random.Next(minRoomHeight, maxRoomHeight);

            //rooms are odd multiple tiles (1, 3, 5, 7, 9, etc..)
            if (newRoom.rectangle.Width % 2 == 0) { newRoom.rectangle.Width += 1; }
            if (newRoom.rectangle.Height % 2 == 0) { newRoom.rectangle.Height += 1; }
            //multiply room size by cellsize to get proper pixel size
            newRoom.rectangle.Width = newRoom.rectangle.Width * cellsize;
            newRoom.rectangle.Height = newRoom.rectangle.Height * cellsize;

            //pick a random tile position
            int randomX = random.Next(0, levelWidth);
            int randomY = random.Next(0, levelHeight);

            //rooms only fall on odd numbered rows + columns
            if (randomX % 2 == 0) { } else { randomX -= 1; }
            if (randomY % 2 == 0) { } else { randomY -= 1; }
            //place room at random odd tile position
            newRoom.rectangle.X = (startPosition * cellsize) + randomX * cellsize;
            newRoom.rectangle.Y = (startPosition * cellsize) + randomY * cellsize;

            //check to see if we should discard the room
            bool discard = false; //assume the room fits into the level

            //does room fit into the level completely? if not, discard it
            if (newRoom.rectangle.X + newRoom.rectangle.Width > (startPosition * cellsize) + (levelWidth * cellsize))
            { discard = true; }
            if (newRoom.rectangle.Y + newRoom.rectangle.Height > (startPosition * cellsize) + (levelHeight * cellsize))
            { discard = true; }

            //if the room intersects a room that already exists, discard it
            for (counterA = 0; counterA < level.rooms.Count; counterA++)
            { if (level.rooms[counterA].rectangle.Intersects(newRoom.rectangle)) { discard = true; } }
            
            if (!discard) { level.rooms.Add(newRoom); } //if the room passes discard tests, keep it
        }


        public void TransferRooms()
        {   //check to see if any tiles intersect any rooms, set filled to true
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                for (counterB = 0; counterB < level.rooms.Count; counterB++)
                {
                    if (tileList[counterA].rectangle.Intersects(level.rooms[counterB].rectangle))
                    { tileList[counterA].filled = true; }
                }
            }
        }

        
        public bool TileHasNeighbors(int tileIndex)
        {   //if the tile has atleast one filled neighbor, return true
            if (tileList[tileIndex].neighborNindex != -1 && tileList[tileList[tileIndex].neighborNindex].filled) { return true; }
            if (tileList[tileIndex].neighborNEindex != -1 && tileList[tileList[tileIndex].neighborNEindex].filled) { return true; }
            if (tileList[tileIndex].neighborNWindex != -1 && tileList[tileList[tileIndex].neighborNWindex].filled) { return true; }
            if (tileList[tileIndex].neighborEindex != -1 && tileList[tileList[tileIndex].neighborEindex].filled) { return true; }
            if (tileList[tileIndex].neighborWindex != -1 && tileList[tileList[tileIndex].neighborWindex].filled) { return true; }
            if (tileList[tileIndex].neighborSindex != -1 && tileList[tileList[tileIndex].neighborSindex].filled) { return true; }
            if (tileList[tileIndex].neighborSEindex != -1 && tileList[tileList[tileIndex].neighborSEindex].filled) { return true; }
            if (tileList[tileIndex].neighborSWindex != -1 && tileList[tileList[tileIndex].neighborSWindex].filled) { return true; }
            return false;
        } 


        public bool AllNeighborsFilled(int tileIndex)
        {   //if any neighboring tile is unfilled, return false
            if (tileList[tileIndex].neighborNindex != -1 && !tileList[tileList[tileIndex].neighborNindex].filled) { return false; }
            if (tileList[tileIndex].neighborNEindex != -1 && !tileList[tileList[tileIndex].neighborNEindex].filled) { return false; }
            if (tileList[tileIndex].neighborNWindex != -1 && !tileList[tileList[tileIndex].neighborNWindex].filled) { return false; }
            if (tileList[tileIndex].neighborEindex != -1 && !tileList[tileList[tileIndex].neighborEindex].filled) { return false; }
            if (tileList[tileIndex].neighborWindex != -1 && !tileList[tileList[tileIndex].neighborWindex].filled) { return false; }
            if (tileList[tileIndex].neighborSindex != -1 && !tileList[tileList[tileIndex].neighborSindex].filled) { return false; }
            if (tileList[tileIndex].neighborSEindex != -1 && !tileList[tileList[tileIndex].neighborSEindex].filled) { return false; }
            if (tileList[tileIndex].neighborSWindex != -1 && !tileList[tileList[tileIndex].neighborSWindex].filled) { return false; }
            return true;
        }


        public void FillIslands()
        {   //locate an empty tile surrounded by empty tiles
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (!tileList[counterA].filled)
                {
                    if (!TileHasNeighbors(counterA))
                    {   //if the tile doesn't have any filled neighbors, fill it
                        tileList[counterA].filled = true;
                    }
                }
            }
        }


        public void ConnectTiles(int percentageChance)
        {
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (!tileList[counterA].filled)
                {
                    //does this tile have both N + S neighbors? then randomly fill
                    if (tileList[counterA].neighborNindex != -1 && tileList[counterA].neighborSindex != -1) //valid north & south
                    {   //are these tiles filled?
                        if (tileList[tileList[counterA].neighborNindex].filled && tileList[tileList[counterA].neighborSindex].filled)
                        { if (random.Next(100) > percentageChance) { tileList[counterA].filled = true; } }
                    }

                    //does this tile have both E + W neighbors? then randomly fill
                    if (tileList[counterA].neighborEindex != -1 && tileList[counterA].neighborWindex != -1) //valid north & south
                    {   //are these tiles filled?
                        if (tileList[tileList[counterA].neighborEindex].filled && tileList[tileList[counterA].neighborWindex].filled)
                        { if (random.Next(100) > percentageChance) { tileList[counterA].filled = true; } }
                    }
                }
            }
        }


        public void GrowConnected()
        {   //this is the messiest/hackiest method in LG
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].connected)
                {
                    //grow a connected tile to any nearby tile that has a filled neighbor, then exit method

                    #region Grow North

                    if (tileList[counterA].neighborNindex != -1) //check for valid N neighbor
                    {
                        if (!tileList[tileList[counterA].neighborNindex].filled) //if the N neighbor isn't filled
                        {
                            if (tileList[tileList[counterA].neighborNindex].neighborNindex != -1) //check for valid NN neighbor
                            {
                                if (tileList[tileList[tileList[counterA].neighborNindex].neighborNindex].filled) //if NN neighbor is filled
                                {
                                    if (!tileList[tileList[tileList[counterA].neighborNindex].neighborNindex].connected) //if NN neighbor is not connected
                                    { tileList[tileList[counterA].neighborNindex].filled = true; return; } //fill N neighbor
                                }
                            }
                        }
                    }

                    #endregion


                    #region Grow South

                    if (tileList[counterA].neighborSindex != -1) //check for valid S neighbor
                    {
                        if (!tileList[tileList[counterA].neighborSindex].filled) //if the S neighbor isn't filled
                        {
                            if (tileList[tileList[counterA].neighborSindex].neighborSindex != -1) //check for valid SS neighbor
                            {
                                if (tileList[tileList[tileList[counterA].neighborSindex].neighborSindex].filled) //if SS neighbor is filled
                                {
                                    if (!tileList[tileList[tileList[counterA].neighborSindex].neighborSindex].connected) //if SS neighbor is not connected
                                    { tileList[tileList[counterA].neighborSindex].filled = true; return; } //fill S neighbor
                                }
                            }
                        }
                    }

                    #endregion


                    #region Grow East

                    if (tileList[counterA].neighborEindex != -1) //check for valid E neighbor
                    {
                        if (!tileList[tileList[counterA].neighborEindex].filled) //if the E neighbor isn't filled
                        {
                            if (tileList[tileList[counterA].neighborEindex].neighborEindex != -1) //check for valid EE neighbor
                            {
                                if (tileList[tileList[tileList[counterA].neighborEindex].neighborEindex].filled) //if EE neighbor is filled
                                {
                                    if (!tileList[tileList[tileList[counterA].neighborEindex].neighborEindex].connected) //if EE neighbor is not connected
                                    { tileList[tileList[counterA].neighborEindex].filled = true; return; } //fill E neighbor
                                }
                            }
                        }
                    }

                    #endregion


                    #region Grow West

                    if (tileList[counterA].neighborWindex != -1) //check for valid W neighbor
                    {
                        if (!tileList[tileList[counterA].neighborWindex].filled) //if the W neighbor isn't filled
                        {
                            if (tileList[tileList[counterA].neighborWindex].neighborWindex != -1) //check for valid WW neighbor
                            {
                                if (tileList[tileList[tileList[counterA].neighborWindex].neighborWindex].filled) //if WW neighbor is filled
                                {
                                    if (!tileList[tileList[tileList[counterA].neighborWindex].neighborWindex].connected) //if WW neighbor is not connected
                                    { tileList[tileList[counterA].neighborWindex].filled = true; return; } //fill W neighbor
                                }
                            }
                        }
                    }

                    #endregion

                }
            }
        }


        public void RemoveIslands()
        {
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].filled)
                {
                    if (!TileHasNeighbors(counterA))
                    {   //if the tile doesn't have any filled neighbors, unfill it
                        tileList[counterA].filled = false;
                    }
                }
            }
        }


        public void RemovePillars()
        {
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (!tileList[counterA].filled)
                {   //if an unfilled tile is surrounded on all sides by filled tiles, then fill it
                    if (AllNeighborsFilled(counterA)) { tileList[counterA].filled = true; }
                }
            }
        }


        public void FloodFill(Tile tile)
        {
            if (tile.neighborSindex != -1 && tileList[tile.neighborSindex].filled && !tileList[tile.neighborSindex].connected) //valid filled S neighbor
            { tileList[tile.neighborSindex].connected = true; FloodFill(tileList[tile.neighborSindex]); } //connect, continue flood

            if (tile.neighborWindex != -1 && tileList[tile.neighborWindex].filled && !tileList[tile.neighborWindex].connected) //valid filled W neighbor
            { tileList[tile.neighborWindex].connected = true; FloodFill(tileList[tile.neighborWindex]); } //connect, continue flood

            if (tile.neighborNindex != -1 && tileList[tile.neighborNindex].filled && !tileList[tile.neighborNindex].connected) //valid filled N neighbor
            { tileList[tile.neighborNindex].connected = true; FloodFill(tileList[tile.neighborNindex]); } //connect, continue flood

            if (tile.neighborEindex != -1 && tileList[tile.neighborEindex].filled && !tileList[tile.neighborEindex].connected) //valid filled E neighbor
            { tileList[tile.neighborEindex].connected = true; FloodFill(tileList[tile.neighborEindex]); } //connect, continue flood
        }


        public void FloodTiles()
        {   //set all tiles to disconnected
            for (counterA = 0; counterA < tileList.Count; counterA++) { tileList[counterA].connected = false; }
            //locate a tile surrounded by neighbors to start the fill in (start fill in a room)
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].filled)
                {   //if an unfilled tile is surrounded on all sides by filled tiles, then start flood here
                    if (AllNeighborsFilled(counterA))
                    {
                        FloodFill(tileList[counterA]);
                        counterA = tileList.Count; //end checking
                    }
                }
            }
        }


        public void RemoveDisconnected()
        {   //remove disconnected dungeon sections
            for (counterA = 0; counterA < tileList.Count; counterA++)
            { if (!tileList[counterA].connected) { tileList[counterA].filled = false; } }
        }


        public void ColorTiles()
        {   //color filled tiles a dark gray, connected tiles a lighter gray 
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].filled) { tileList[counterA].color = new Color(33, 33, 33); }
                if (tileList[counterA].connected) { tileList[counterA].color = new Color(128, 128, 128); }
            }
        }


        public bool GradeLevel()
        {   //check to see if there are 'enough' connected tiles
            int totalConnected = 0;
            for (counterA = 0; counterA < tileList.Count; counterA++)
            { if (tileList[counterA].connected) { totalConnected++; } }
            //if our level is smaller than the minimum size required, level has failed
            if (totalConnected < minimumSize) { return false; }
            return true; //otherwise, level passes inspection
        }


        public void KeepConnectedRooms()
        {
            bool keepRec = false;
            for (counterB = 0; counterB < level.rooms.Count; counterB++)
            {   //check each rec to see if it touches a connected tile, if it doesn't - remove it
                keepRec = false; //assume rec is not connected
                for (counterA = 0; counterA < tileList.Count; counterA++)
                {
                    if (tileList[counterA].connected)
                    {
                        if (level.rooms[counterB].rectangle.Intersects(tileList[counterA].rectangle))
                        { keepRec = true; }
                    }
                }
                if (!keepRec) { level.rooms.RemoveAt(counterB); counterB--; }
            }
        }






        public void GenerateLevel(int levelNumber)
        {   //set the difficulty based on the level number
            if (levelNumber == 1) { difficulty = Difficulty.Easy; }
            else if (levelNumber == 2) { difficulty = Difficulty.Medium; }
            else if (levelNumber == 3) { difficulty = Difficulty.Hard; }
            else if (levelNumber >= 4) { difficulty = Difficulty.Impossible; }


            #region Set Difficulty

            //these are essentially dungeon PRESETS

            if (difficulty == Difficulty.Easy)
            {   //small - many massive rooms connected with few mazes
                levelWidth = 40; levelHeight = 40;
                maxRoomWidth = 11; maxRoomHeight = 11;
                minRoomWidth = 5; minRoomHeight = 5;
                createRoomAttempts = 100;
            }
            else if (difficulty == Difficulty.Medium)
            {   //medium - many large rooms with some mazes
                levelWidth = 40; levelHeight = 40;
                maxRoomWidth = 5; maxRoomHeight = 5;
                minRoomWidth = 3; minRoomHeight = 3;
                createRoomAttempts = 50;
            }
            else if (difficulty == Difficulty.Hard)
            {   //few very small rooms, mostly mazes
                levelWidth = 60; levelHeight = 60;
                maxRoomWidth = 5; maxRoomHeight = 5;
                minRoomWidth = 3; minRoomHeight = 3;
                createRoomAttempts = 100;
            }
            else if (difficulty == Difficulty.Impossible)
            {   //massively complex maze
                levelWidth = 80; levelHeight = 80;
                maxRoomWidth = 3; maxRoomHeight = 3;
                minRoomWidth = 1; minRoomHeight = 1;
                createRoomAttempts = 100;
            }

            #endregion




            //remove any previous level data
            //System.Diagnostics.Debug.WriteLine("----------------------------------");
            //stopWatch.Reset(); stopWatch.Start();
            level.ClearLevel();
            tileList = new List<Tile>();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("clear level: " + timeSpan.Milliseconds + "ms");

            //create the tile list, which also sets the tile's neighbors
            //stopWatch.Reset(); stopWatch.Start();
            BuildTileList();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("build tiles: " + timeSpan.Milliseconds + "ms");




            //Check tile's neighbors (debugging tile neighbors)
            
            //CheckTile(0); //top left
            //CheckTile(levelHeight / 2); //left
            //CheckTile(levelHeight - 1); //bottom left

            //CheckTile(levelHeight * levelWidth / 2); //top center
            //CheckTile((levelHeight * levelWidth / 2) + levelHeight / 2); //center
            //CheckTile(levelHeight * ((levelWidth / 2) + 1) - 1);//bottom center

            //CheckTile(levelHeight * (levelWidth - 1)); //top right
            //CheckTile(levelHeight * (levelWidth - 1) + (levelHeight / 2)); //right
            //CheckTile(levelHeight * levelWidth - 1); //bottom right





            //randomly create rectangle rooms
            //stopWatch.Reset(); stopWatch.Start();
            for (counterB = 0; counterB < createRoomAttempts; counterB++) { CreateRoom(); }
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("create rooms: " + timeSpan.Milliseconds + "ms");

            //check to see which tiles intersect rooms
            //stopWatch.Reset(); stopWatch.Start();
            TransferRooms();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("transfer rooms: " + timeSpan.Milliseconds + "ms");

            //locate and fill island tiles (tiles without neighbors)
            //stopWatch.Reset(); stopWatch.Start();
            FillIslands();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("fill islands: " + timeSpan.Milliseconds + "ms");

            //connect tiles with N+S or E+W neighbors
            //stopWatch.Reset(); stopWatch.Start();
            ConnectTiles(50); //connect tiles 50% of the time
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("connect tiles: " + timeSpan.Milliseconds + "ms");

            //fill tiles that are surrounded by neighbors
            //stopWatch.Reset(); stopWatch.Start();
            RemovePillars();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("remove pillars: " + timeSpan.Milliseconds + "ms");

            //fill the tiles to determine connections
            //stopWatch.Reset(); stopWatch.Start();
            FloodTiles();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("flooding tiles: " + timeSpan.Milliseconds + "ms");



            //stopWatch.Reset(); stopWatch.Start();
            if (difficulty == Difficulty.Impossible)
            {   //this makes impossible levels slightly more connected, increasing complexity
                GrowConnected(); 
                ConnectTiles(90); //connect tiles 10% of the time
                FillIslands();
                RemovePillars();
                FloodTiles();
            }
            if (GradeLevel() == false) //if the level does not have enough connections
            {
                for (int i = 0; i < 10; i++)
                {   //try to fix the level 10 times
                    GrowConnected(); FloodTiles();
                    if (GradeLevel()) { i = 10; } //if level passes, exit loop
                }
            }
            if (GradeLevel() == false)
            {   //if we didn't fix the level, then just regenerate the level
                GenerateLevel(gameScreen.levelNumber); //this rarely happens
                return;
            }
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("Growing: " + timeSpan.Milliseconds + "ms");

            //our level has been generated and passed quality tests
            //clean up tiles

            //remove tiles that have no neighbors
            //stopWatch.Reset(); stopWatch.Start();
            RemoveIslands();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("remove islands: " + timeSpan.Milliseconds + "ms");

            //remove unconnected parts
            //stopWatch.Reset(); stopWatch.Start();
            RemoveDisconnected();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("remove disconnected: " + timeSpan.Milliseconds + "ms");

            //set filled tiles to proper color
            //stopWatch.Reset(); stopWatch.Start();
            ColorTiles();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("color tiles: " + timeSpan.Milliseconds + "ms");

            //remove room rectangles that don't collide with connected tiles
            //stopWatch.Reset(); stopWatch.Start();
            KeepConnectedRooms();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("cleaning rooms: " + timeSpan.Milliseconds + "ms");

            //build level floors + walls
            
            //for each tile, create a floor tile at that location
            //stopWatch.Reset(); stopWatch.Start();
            BuildFloors();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("build floors: " + timeSpan.Milliseconds + "ms");

            //for each floortile, create walls around them
            //stopWatch.Reset(); stopWatch.Start();
            BuildWalls();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("build walls: " + timeSpan.Milliseconds + "ms");

            //place exit, spawn, floor debris, and spiders
            //stopWatch.Reset(); stopWatch.Start();
            CompleteLevel();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("level completed: " + timeSpan.Milliseconds + "ms");

            //place torches, spider holes
            //stopWatch.Reset(); stopWatch.Start();
            PlaceWallDecorations();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("placed wall decorations: " + timeSpan.Milliseconds + "ms");

            //sort the level
            //stopWatch.Reset(); stopWatch.Start();
            level.Sort();
            //stopWatch.Stop(); timeSpan = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine("level sort: " + timeSpan.Milliseconds + "ms");

            GC.Collect(); //cleanup any unreachable data
            //we force garbage collection now because it's a good time to do so
            //we don't want garbage collection occurring during the game
            //and we just marked some of the previous level data for garbage collection
            //so it's best to collect that garbage now while the game's input/update/draw loops are unimportant
        }







        public void BuildFloors()
        {   //for each connected tile, build a floor tile at it's location
            int rotator = 0; //used to incrementally rotate floortile, much faster than generating a random number
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].connected)
                {
                    Sprite floorTile = new Sprite(gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, new Vector2(0, 0), new Point(32, 32), new Point(0, 0));
                    floorTile.position.X = tileList[counterA].rectangle.X + 16;
                    floorTile.position.Y = tileList[counterA].rectangle.Y + 16;
                    //if (rotator == 0) { } //do nothing
                    if (rotator == 1) { floorTile.spriteEffect = SpriteEffects.FlipHorizontally; }
                    else if (rotator == 2) { floorTile.spriteEffect = SpriteEffects.FlipVertically; rotator = -1; }
                    rotator++;
                    level.floors.Add(floorTile);
                }
            }
        }


        public void BuildWalls()
        {   //booleans to track a tiles valid neighbors (makes neighbor checking code shorter/easier to read)
            Boolean N = false;
            Boolean NE = false;
            Boolean E = false;
            Boolean SE = false;
            Boolean S = false;
            Boolean SW = false;
            Boolean W = false;
            Boolean NW = false;

            for (counterA = 0; counterA < tileList.Count; counterA++)
            {  //reset neighbor booleans
                N = false;
                NE = false;
                E = false;
                SE = false;
                S = false;
                SW = false;
                W = false;
                NW = false;

                //gather the NSEW neighbor booleans
                if (tileList[counterA].neighborNindex != -1) //valid N neighbor
                { if (tileList[tileList[counterA].neighborNindex].filled) { N = true; } }
                if (tileList[counterA].neighborSindex != -1) //valid S neighbor
                { if (tileList[tileList[counterA].neighborSindex].filled) { S = true; } }
                if (tileList[counterA].neighborEindex != -1) //valid E neighbor
                { if (tileList[tileList[counterA].neighborEindex].filled) { E = true; } }
                if (tileList[counterA].neighborWindex != -1) //valid W neighbor
                { if (tileList[tileList[counterA].neighborWindex].filled) { W = true; } }
                //gather the DIAG neighbor booleans
                if (tileList[counterA].neighborNEindex != -1) //valid NE neighbor
                { if (tileList[tileList[counterA].neighborNEindex].filled) { NE = true; } }
                if (tileList[counterA].neighborNWindex != -1) //valid NW neighbor
                { if (tileList[tileList[counterA].neighborNWindex].filled) { NW = true; } }
                if (tileList[counterA].neighborSEindex != -1) //valid SE neighbor
                { if (tileList[tileList[counterA].neighborSEindex].filled) { SE = true; } }
                if (tileList[counterA].neighborSWindex != -1) //valid SW neighbor
                { if (tileList[tileList[counterA].neighborSWindex].filled) { SW = true; } }

                //build walls based on the tile's neighbors
                if (tileList[counterA].filled) //build walls around filled tiles
                {   

                    #region Place NSEW walls

                    if (!N & !NW & !NE) //place north wall
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.North);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 16;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y - 16;
                        wall.collisionOffset.Y += 7;
                        wall.collisionRec.Height -= 3;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!S & !SW & !SE) //place south wall
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.South);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 16;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y + 32 + 8;
                        wall.collisionOffset.Y -= 0;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!W & !NW & !SW) //place west wall
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.West);
                        wall.sprite.position.X = tileList[counterA].rectangle.X - 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!E & !NE & !SE) //place east wall
                    {
                        Wall wall = new Wall(gameScreen,new Vector2(0, 0), Wall.WallType.East);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 32 + 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }

                    #endregion


                    #region Place Interior Corners

                    if (!N & !NW & !W)
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.InteriorNW);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 0 - 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y - 8 - 16;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!N & !NE & !E)
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.InteriorNE);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 32 + 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y - 8 - 16;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!S & !SE & !E)
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.InteriorSE);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 32 + 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y + 32;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }
                    if (!S & !SW & !W)
                    {
                        Wall wall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.InteriorSW);
                        wall.sprite.position.X = tileList[counterA].rectangle.X + 0 - 8;
                        wall.sprite.position.Y = tileList[counterA].rectangle.Y + 32;
                        wall.collisionOffset.Y += 8;
                        wall.UpdateCollisionRecPosition();
                        level.walls.Add(wall);
                    }

                    #endregion

                }
                else //build walls around unfilled tiles
                {

                    #region Vertical Pillar Walls

                    if (E & SE & S & SW & W) //vertical pillar south
                    {
                        //left corner
                        Wall lCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNE);
                        lCorner.sprite.position.X = tileList[counterA].rectangle.X + 8;
                        lCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        lCorner.collisionOffset.Y += 15;
                        lCorner.collisionRec.Height -= 2;
                        lCorner.UpdateCollisionRecPosition();
                        level.walls.Add(lCorner);

                        //right corner wall
                        Wall rCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNW);
                        rCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        rCorner.collisionOffset.Y += 15;
                        rCorner.collisionRec.Height -= 2;
                        rCorner.UpdateCollisionRecPosition();
                        level.walls.Add(rCorner);
                    }
                    if (E & NE & N & NW & W) //vertical pillar north
                    {
                        //left corner
                        Wall lCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSE);
                        lCorner.sprite.position.X = tileList[counterA].rectangle.X + 8;
                        lCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        lCorner.collisionOffset.Y += 8;
                        lCorner.collisionRec.Height -= 0;
                        lCorner.UpdateCollisionRecPosition();
                        level.walls.Add(lCorner);

                        //right corner
                        Wall rCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSW);
                        rCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        rCorner.collisionOffset.Y += 8;
                        rCorner.collisionRec.Height -= 0;
                        rCorner.UpdateCollisionRecPosition();
                        level.walls.Add(rCorner);

                        //left wall
                        Wall lWall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.East);
                        lWall.sprite.position.X = tileList[counterA].rectangle.X + 8;
                        lWall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        lWall.collisionOffset.Y += 8;
                        lWall.collisionRec.Height -= 0;
                        lWall.UpdateCollisionRecPosition();
                        level.walls.Add(lWall);

                        //right wall
                        Wall rWall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.West);
                        rWall.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rWall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        rWall.collisionOffset.Y += 8;
                        rWall.collisionRec.Height -= 0;
                        rWall.UpdateCollisionRecPosition();
                        level.walls.Add(rWall);
                    }

                    #endregion


                    #region Horizontal Pillar Walls

                    if(N & NE & E & SE & S) //horizontal pillar right
                    {
                        //top right corner
                        Wall rTopCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSW);
                        rTopCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rTopCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        rTopCorner.collisionOffset.Y += 8;
                        rTopCorner.collisionRec.Height -= 2;
                        rTopCorner.UpdateCollisionRecPosition();
                        rTopCorner.sprite.zOffset += 10;
                        level.walls.Add(rTopCorner);

                        //short south wall
                        Wall shortSouth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.SouthShort);
                        shortSouth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        shortSouth.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        shortSouth.collisionOffset.Y += 8;
                        shortSouth.collisionRec.Height -= 2;
                        shortSouth.UpdateCollisionRecPosition();
                        level.walls.Add(shortSouth);

                        //bottom right corner
                        Wall rBotCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNW);
                        rBotCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rBotCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        rBotCorner.collisionOffset.Y += 15;
                        rBotCorner.collisionRec.Height -= 2;
                        rBotCorner.UpdateCollisionRecPosition();
                        level.walls.Add(rBotCorner);

                        //short north wall
                        Wall shortNorth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.NorthShort);
                        shortNorth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        shortNorth.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        shortNorth.collisionOffset.Y += 15;
                        shortNorth.collisionRec.Height -= 2;
                        shortNorth.UpdateCollisionRecPosition();
                        level.walls.Add(shortNorth);
                    }

                    if (N & NW & W & SW & S) //horizontal pillar left
                    {
                        //top left corner
                        Wall lTopCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSE);
                        lTopCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        lTopCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        lTopCorner.collisionOffset.Y += 8;
                        lTopCorner.collisionRec.Height -= 2;
                        lTopCorner.UpdateCollisionRecPosition();
                        lTopCorner.sprite.zOffset += 10;
                        level.walls.Add(lTopCorner);

                        //short south wall
                        Wall shortSouth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.SouthShort);
                        shortSouth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        shortSouth.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        shortSouth.collisionOffset.Y += 8;
                        shortSouth.collisionRec.Height -= 2;
                        shortSouth.UpdateCollisionRecPosition();
                        level.walls.Add(shortSouth);

                        //bottom left corner
                        Wall lBotCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNE);
                        lBotCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        lBotCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        lBotCorner.collisionOffset.Y += 15;
                        lBotCorner.collisionRec.Height -= 2;
                        lBotCorner.UpdateCollisionRecPosition();
                        level.walls.Add(lBotCorner);

                        //short north wall
                        Wall shortNorth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.NorthShort);
                        shortNorth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        shortNorth.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        shortNorth.collisionOffset.Y += 15;
                        shortNorth.collisionRec.Height -= 2;
                        shortNorth.UpdateCollisionRecPosition();
                        level.walls.Add(shortNorth);
                    }

                    #endregion


                    #region Upper Exterior Corners

                    if (NE & N & NW & W & SW & !E & !S) //left upper corner
                    {
                        //top left corner
                        Wall lTopCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSE);
                        lTopCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        lTopCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        lTopCorner.collisionOffset.Y += 8;
                        lTopCorner.UpdateCollisionRecPosition();
                        lTopCorner.sprite.zOffset += 10;
                        level.walls.Add(lTopCorner);

                        //short south wall
                        Wall shortSouth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.SouthShort);
                        shortSouth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        shortSouth.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        shortSouth.collisionOffset.Y += 8;
                        shortSouth.UpdateCollisionRecPosition();
                        level.walls.Add(shortSouth);

                        //left wall
                        Wall lWall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.East);
                        lWall.sprite.position.X = tileList[counterA].rectangle.X + 8;
                        lWall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        lWall.collisionOffset.Y += 8;
                        lWall.collisionRec.Height -= 0;
                        lWall.UpdateCollisionRecPosition();
                        level.walls.Add(lWall);
                    }
                    if (NW & N & NE & E & SE & !W & !S) //right upper corner
                    {
                        //top right corner
                        Wall rTopCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorSW);
                        rTopCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rTopCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        rTopCorner.collisionOffset.Y += 8;
                        rTopCorner.UpdateCollisionRecPosition();
                        rTopCorner.sprite.zOffset += 10;
                        level.walls.Add(rTopCorner);

                        //short south wall
                        Wall shortSouth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.SouthShort);
                        shortSouth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        shortSouth.sprite.position.Y = tileList[counterA].rectangle.Y + 0;
                        shortSouth.collisionOffset.Y += 8;
                        shortSouth.UpdateCollisionRecPosition();
                        level.walls.Add(shortSouth);

                        //right wall
                        Wall rWall = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.West);
                        rWall.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rWall.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        rWall.collisionOffset.Y += 8;
                        rWall.collisionRec.Height -= 0;
                        rWall.UpdateCollisionRecPosition();
                        level.walls.Add(rWall);
                    }

                    #endregion


                    #region Lower Exterior Corners

                    if (NE & E & SE & S & SW & !W & !N) //bottom right corner
                    {
                        //bottom right corner
                        Wall rBotCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNW);
                        rBotCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        rBotCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        rBotCorner.collisionOffset.Y += 15;
                        rBotCorner.collisionRec.Height -= 2;
                        rBotCorner.UpdateCollisionRecPosition();
                        level.walls.Add(rBotCorner);

                        //short north wall
                        Wall shortNorth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.NorthShort);
                        shortNorth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        shortNorth.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        shortNorth.collisionOffset.Y += 15;
                        shortNorth.collisionRec.Height -= 2;
                        shortNorth.UpdateCollisionRecPosition();
                        level.walls.Add(shortNorth);
                    }
                    if (NW & W & SW & S & SE & !E & !N) //bottom left corner
                    {
                        //bottom left corner
                        Wall lBotCorner = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.ExteriorNE);
                        lBotCorner.sprite.position.X = tileList[counterA].rectangle.X + 8 + 0;
                        lBotCorner.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        lBotCorner.collisionOffset.Y += 15;
                        lBotCorner.collisionRec.Height -= 2;
                        lBotCorner.UpdateCollisionRecPosition();
                        level.walls.Add(lBotCorner);

                        //short north wall
                        Wall shortNorth = new Wall(gameScreen, new Vector2(0, 0), Wall.WallType.NorthShort);
                        shortNorth.sprite.position.X = tileList[counterA].rectangle.X + 8 + 16;
                        shortNorth.sprite.position.Y = tileList[counterA].rectangle.Y + 8;
                        shortNorth.collisionOffset.Y += 15;
                        shortNorth.collisionRec.Height -= 2;
                        shortNorth.UpdateCollisionRecPosition();
                        level.walls.Add(shortNorth);
                    }

                    #endregion

                }
            }
        }


        public void CompleteLevel()
        {   //place exit, spawn, floor debris, and spiders
            List<Tile> possibleExits = new List<Tile>();
            List<Tile> possibleSpawns = new List<Tile>();
            List<Tile> connectedTiles = new List<Tile>();

            int topRow = startPosition * cellsize;
            int bottomRow = (startPosition * cellsize) + ((levelHeight - 2) * cellsize);
            //loop through all the tiles, checking their Y position
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                if (tileList[counterA].connected)
                {   //if a connected tile is on the top row, it could be an exit... if it's on the bottom row, it could be a spawn
                    connectedTiles.Add(tileList[counterA]);
                    if (tileList[counterA].rectangle.Location.Y == topRow) { possibleExits.Add(tileList[counterA]); }
                    else if (tileList[counterA].rectangle.Location.Y == bottomRow) { possibleSpawns.Add(tileList[counterA]); }

                    PlaceDebris(tileList[counterA]); //if the tile is connected, maybe add some debris to it
                    PlaceSpiders(tileList[counterA]); //if the tile is connected, maybe add a spider to it
                }
            }
            //System.Diagnostics.Debug.WriteLine("possible exits: " + possibleExits.Count);
            //System.Diagnostics.Debug.WriteLine("possible spawns: " + possibleSpawns.Count);
            //it is possible that no good spawns or exits were created in the level
            //this case is handled below


            #region Setup Exit

            //first connected tile will never have a north neighbor, so it's suitable for the exit obj
            Tile selectedExit = connectedTiles[0]; 
            if(difficulty != Difficulty.Impossible) //impossible difficulty always sets exit to first connected tile
            {
                if (possibleExits.Count > 0) //if we have good exit tiles, then randomly choose one
                { selectedExit = possibleExits[random.Next(possibleExits.Count)]; }
            }
            //build an exit at this tile location
            level.exitObj = new GameObject(gameScreen, new Vector2(0, 0), GameObject.Type.Exit);
            level.exitObj.sprite.position.X = selectedExit.rectangle.X + 16;
            level.exitObj.sprite.position.Y = selectedExit.rectangle.Y;
            level.exitObj.UpdateCollisionRecPosition();
            level.gameobjects.Add(level.exitObj); //ensures the exit is drawn

            #endregion


            #region Setup Spawn

            //get the last connected tile, suitable for spawn
            Tile selectedSpawn = connectedTiles[connectedTiles.Count - 1];
            if (difficulty != Difficulty.Impossible) //impossible difficulty always sets spawn to last connected tile
            {
                if (possibleSpawns.Count > 0) //if we have good spawnpoints, randomly choose a spawn tile
                { selectedSpawn = possibleSpawns[random.Next(possibleSpawns.Count)]; }
            }

            //place dog at spawn
            gameScreen.dog.sprite.position.X = selectedSpawn.rectangle.X + 16;
            gameScreen.dog.sprite.position.Y = selectedSpawn.rectangle.Y + 16 + 0;
            gameScreen.dog.newPosition.X = selectedSpawn.rectangle.X + 16;
            gameScreen.dog.newPosition.Y = selectedSpawn.rectangle.Y + 16 + 6;
            //place human at spawn
            gameScreen.human.sprite.position = gameScreen.dog.sprite.position;
            gameScreen.human.sprite.position.Y -= 32;
            gameScreen.human.newPosition = gameScreen.human.sprite.position;
            //bring human back to life if he died
            gameScreen.human.inputState = Actor.State.Idle;
            gameScreen.human.animationState = Actor.State.Idle;
            gameScreen.human.stateLocked = false;
            gameScreen.human.animationCounter = 0;
            gameScreen.human.currentAnimation = gameScreen.human.idleStill;

            #endregion


            #region Prevent Spawn Camping by Spiders

            for (counterA = 0; counterA < level.actors.Count; counterA++)
            {   //hide any spiders (actors) that are nearby spawn - prevents spiders from immediately attacking human upon spawn
                if (Math.Abs(level.actors[counterA].sprite.position.X - gameScreen.dog.sprite.position.X) < 300)
                {
                    if (Math.Abs(level.actors[counterA].sprite.position.Y - gameScreen.dog.sprite.position.Y) < 300)
                    {
                        level.actors[counterA].sprite.position.X = 0;
                        level.actors[counterA].sprite.position.Y = 0;
                    }
                }
            }

            #endregion

        }


        public void PlaceDebris(Tile Tile)
        {
            if (random.Next(100) > 50) //50% chance to place debris at this tile
            {
                Sprite debris = new Sprite(
                    gameScreen.screenManager, gameScreen.screenManager.game.spriteSheet, 
                    new Vector2(
                        Tile.rectangle.X + random.Next(16), 
                        Tile.rectangle.Y + random.Next(16)), 
                    new Point(16, 16), 
                    new Point(0, 2));

                int currentFrame = random.Next(8);
                //System.Diagnostics.Debug.WriteLine("random number: " + currentFrame);
                //randomly choose a debris sprite frame
                if (currentFrame == 0) { debris.currentFrame.X = 0; debris.currentFrame.Y = 2; }
                else if (currentFrame == 1) { debris.currentFrame.X = 0; debris.currentFrame.Y = 3; }
                else if (currentFrame == 2) { debris.currentFrame.X = 1; debris.currentFrame.Y = 2; }
                else if (currentFrame == 3) { debris.currentFrame.X = 1; debris.currentFrame.Y = 3; }
                else if (currentFrame == 4) { debris.currentFrame.X = 2; debris.currentFrame.Y = 0; }
                else if (currentFrame == 5) { debris.currentFrame.X = 2; debris.currentFrame.Y = 1; }
                else if (currentFrame == 6) { debris.currentFrame.X = 2; debris.currentFrame.Y = 2; }
                else if (currentFrame == 7) { debris.currentFrame.X = 2; debris.currentFrame.Y = 3; }
                //randomly flip sprite horizonatally, further increasing debris variation
                if (random.Next(100) > 50) { debris.spriteEffect = SpriteEffects.FlipHorizontally; }
                level.debris.Add(debris);
            }
        }


        public void PlaceSpiders(Tile Tile)
        {
            if (random.Next(100) > 97) //2% chance
            {   //create an idle spider at the tile's location
                Actor spider = new Actor(gameScreen, new Vector2(Tile.rectangle.X + 16, Tile.rectangle.Y + 20), Actor.Type.Spider);
                spider.inputState = Actor.State.Idle;
                level.actors.Add(spider);
            }
        }


        public void PlaceWallDecorations()
        {   //wall decorations are only placed on NORTH WALLS
            for (counterA = 0; counterA < level.walls.Count; counterA++)
            {   //loop through all the wall objects and find a NORTH WALL
                if (level.walls[counterA].type == Wall.WallType.North)
                {
                    if (random.Next(100) > 85) //15% chance
                    {   //make a fire object, which is animated
                        GameObject fire = new GameObject(gameScreen, level.walls[counterA].sprite.position, GameObject.Type.Fire);
                        fire.sprite.position.Y += 10;
                        fire.UpdateCollisionRecPosition();
                        if (random.Next(100) > 50) { fire.sprite.spriteEffect = SpriteEffects.FlipHorizontally; }
                        //make a torch object, which isn't animated
                        GameObject torch = new GameObject(gameScreen, level.walls[counterA].sprite.position, GameObject.Type.Torch);
                        torch.sprite.position.Y += 10 + 15;
                        torch.UpdateCollisionRecPosition();
                        torch.sprite.zOffset -= 16; //sort torch below fire

                        //if this gameobject doesn't touch the exit object, then add it to gameobjects list
                        //this prevents placement of torches + fire gameobjects ontop of exit objects (looks bad)
                        if (!fire.collisionRec.Intersects(level.exitObj.collisionRec))
                        { level.gameobjects.Add(fire); level.gameobjects.Add(torch); }
                        fire.collisionRec.Width = 0;    //actors can't collide with
                        torch.collisionRec.Width = 0;   //these two objects
                    }
                    else
                    {
                        if (random.Next(100) > 85) //15% chance
                        {   //make a spider hole wall decoration
                            GameObject spiderHole = new GameObject(gameScreen, level.walls[counterA].sprite.position, GameObject.Type.SpiderHole);
                            spiderHole.sprite.position.Y += 12 + 12;
                            spiderHole.UpdateCollisionRecPosition();

                            //if this gameobject doesn't touch the exit object, then add it to gameobjects list
                            if (!spiderHole.collisionRec.Intersects(level.exitObj.collisionRec)) { level.gameobjects.Add(spiderHole); }
                            spiderHole.collisionRec.Width = 0; //actor can't collide with a spider hole
                            spiderHole.sprite.zOffset -= 8; //sort spider hole behind dog/human/spider but ontop of NorthWall
                        }
                    }
                }
            }
        }






        public void Draw()
        {   //draw the tile list
            for (counterA = 0; counterA < tileList.Count; counterA++)
            {
                gameScreen.screenManager.spriteBatch.Draw(
                    gameScreen.screenManager.game.dummyTexture, 
                    tileList[counterA].rectangle,
                    tileList[counterA].color * 1.0f);
            }
        }
    }
}