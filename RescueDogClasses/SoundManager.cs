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
    public class SoundManager
    {   //loads, plays, fades between music tracks and soundFX
        Game1 game;

        //load all the sounds used in the game
        SoundEffect musicTrack1;
        SoundEffect musicTrack2;
        SoundEffect musicTrack3;
        //actor action fx
        SoundEffect dogBark;
        SoundEffect spiderJump;
        SoundEffect humanBit;
        //won/lost fx
        SoundEffect lostGame;
        SoundEffect wonGame;

        //create sound instances for each sound as well
        public SoundEffectInstance musicTrack1Ins;
        public SoundEffectInstance musicTrack2Ins;
        public SoundEffectInstance musicTrack3Ins;
        public SoundEffectInstance dogBarkIns;
        public SoundEffectInstance spiderJumpIns;
        public SoundEffectInstance humanBitIns;
        public SoundEffectInstance lostGameIns;
        public SoundEffectInstance wonGameIns;

        //booleans control music track fade in/out
        public bool playTrack1 = false;
        public bool playTrack2 = false;
        public bool playTrack3 = false;
        float fadeSpeed = 0.004f; //very slowly fade a music track in/out

        public SoundManager(Game1 Game) { game = Game; }
        public void LoadContent()
        {   //load + loop each music track, set volume to 0
            musicTrack1 = game.Content.Load<SoundEffect>("IotaPart1");
            musicTrack1Ins = musicTrack1.CreateInstance();
            musicTrack1Ins.IsLooped = true;
            musicTrack1Ins.Volume = 0.001f;

            musicTrack2 = game.Content.Load<SoundEffect>("IotaPart2");
            musicTrack2Ins = musicTrack2.CreateInstance();
            musicTrack2Ins.IsLooped = true;
            musicTrack2Ins.Volume = 0.001f;

            musicTrack3 = game.Content.Load<SoundEffect>("IotaPart3");
            musicTrack3Ins = musicTrack3.CreateInstance();
            musicTrack3Ins.IsLooped = true;
            musicTrack3Ins.Volume = 0.001f;

            //load the soundfx
            dogBark = game.Content.Load<SoundEffect>("Bark");
            dogBarkIns = dogBark.CreateInstance();

            spiderJump = game.Content.Load<SoundEffect>("SpiderJump");
            spiderJumpIns = spiderJump.CreateInstance();

            humanBit = game.Content.Load<SoundEffect>("HumanBit");
            humanBitIns = humanBit.CreateInstance();

            lostGame = game.Content.Load<SoundEffect>("LostGame");
            lostGameIns = lostGame.CreateInstance();

            wonGame = game.Content.Load<SoundEffect>("WonGame");
            wonGameIns = wonGame.CreateInstance();
        }

        public void FadeTrack(bool TrackState, SoundEffectInstance MusicTrack)
        {
            if (TrackState) //the music track should be heard
            { if (MusicTrack.Volume < 0.99f) { MusicTrack.Volume += fadeSpeed; } }
            else //the music track should not be heard
            { if (MusicTrack.Volume > 0.01f) { MusicTrack.Volume -= fadeSpeed; } }
        }

        public void PlayMusic()
        {   //stop any playing music, then start playing all music
            if (musicTrack1Ins.State == SoundState.Playing) { musicTrack1Ins.Stop(); }
            if (musicTrack2Ins.State == SoundState.Playing) { musicTrack2Ins.Stop(); }
            if (musicTrack3Ins.State == SoundState.Playing) { musicTrack3Ins.Stop(); }
            musicTrack1Ins.Play();
            musicTrack2Ins.Play();
            musicTrack3Ins.Play();
        }

        public void Update()
        {   //pass boolean as fade control with corresponding music track
            FadeTrack(playTrack1, musicTrack1Ins);
            FadeTrack(playTrack2, musicTrack2Ins);
            FadeTrack(playTrack3, musicTrack3Ins);
        }
    }
}
