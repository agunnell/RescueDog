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

using System.IO;
using System.Xml.Serialization;

namespace RescueDog
{
    [Serializable]
    public class SaveData { public List<LevelGenerator.Tile> tileList; }
    //we only need to save the tile list in order to rebuild the level upon a level load

    public class SaveLoadLevel
    {
        public GameScreen gameScreen;
        string saveFileName = "saveData.xml";   //the name of the save file
        public SaveLoadLevel(GameScreen GameScreen) { gameScreen = GameScreen; }

        public void SaveLevel()
        {   //create a saveData instance, create a new tile list in saveData
            SaveData saveData = new SaveData();
            saveData.tileList = new List<LevelGenerator.Tile>();
            //collect the tiles from levelGenerator and add them to saveData's tileList
            foreach (LevelGenerator.Tile tile in gameScreen.levelGenerator.tileList)
            { saveData.tileList.Add(tile); }
            //check to see if the save file exists, if it does, delete it
            if (File.Exists(saveFileName)) { File.Delete(saveFileName); }
            //create and open the file
            FileStream stream = File.Open(saveFileName, FileMode.OpenOrCreate);
            //convert the object to XML data and put it in the stream
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            serializer.Serialize(stream, saveData); //serialize the data to xml format
            stream.Close(); //always close the stream - never cross the streams
        }

        public void LoadLevel()
        {
            SaveData saveData; //create a saveData instance
            using (FileStream stream = new FileStream(saveFileName, FileMode.Open))
            {   //open the XML saveFile, deserialize XML data into saveData instance
                XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                saveData = (SaveData)serializer.Deserialize(stream);
            }   //saveData now contains the XML's saved tileList data
            //clear the old level data, pass the saveData tileList to level generator, then build the level
            //note that this creates new floors, walls, debris, exit + spawn points, and wall decorations
            //the level layout is kept the same, everything else is randomized
            gameScreen.level.ClearLevel();
            gameScreen.levelGenerator.tileList = saveData.tileList;
            gameScreen.levelGenerator.BuildFloors();
            gameScreen.levelGenerator.BuildWalls();
            gameScreen.levelGenerator.CompleteLevel();
            gameScreen.levelGenerator.PlaceWallDecorations();
            gameScreen.level.Sort();
        }
    }
}