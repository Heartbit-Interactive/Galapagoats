using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace GalapaGoats
{
    static class AudioPlayer //Gestione dell'Audio
    {
        private static Song song;

        public static int master_volume { get { return _master_volume; } set { _master_volume = value; MediaPlayer.Volume = _master_volume / 100f; } }
        private static int _master_volume=100;

        private static List<string> playing_keys=new List<string>();
        private static List<int> playing_values = new List<int>();
        internal static SoundEffectInstance Play(string p)
        {
            var se = ContentLoader.Load<SoundEffect>(System.IO.Path.Combine("Audio/SE", p)).CreateInstance();
            se.Volume = _master_volume / 100f;
                se.Play();
            return se;
        }

        internal static SoundEffectInstance Play(string p, bool repeat)
        {
            var se = ContentLoader.Load<SoundEffect>(System.IO.Path.Combine("Audio/SE", p)).CreateInstance();
            se.Volume = _master_volume / 100f;
            se.IsLooped = repeat;
            se.Play();
            return se;
        }
        internal static void Play(string p1, int p2)
        {
            var counter=0;
            var index = playing_keys.IndexOf(p1);
            if (index >= 0)
            {
                counter = playing_values[index];
                if (counter > 0)
                    return;
                playing_values[index] = p2;
            }
            else
            {
                playing_keys.Add(p1);
                playing_values.Add(p2);
            }
            Play(p1);

        }

        public static void Update()
        {
            for (int i = 0; i < playing_values.Count; i++)
                if(playing_values[i]>0)
                    playing_values[i]--;
        }

        internal static void PlaySong(string p)
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }
            try
            {
                song = ContentLoader.Load<Song>(System.IO.Path.Combine("Audio/BGM", p));
                MediaPlayer.Volume = master_volume / 100f;
                MediaPlayer.Play(song);
                MediaPlayer.IsRepeating = true;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error Loading Song: " + p);
            }
        }


        internal static void StopSong()
        {
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }
        }
    }
}
