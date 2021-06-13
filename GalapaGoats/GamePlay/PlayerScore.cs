using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalapaGoats
{
    struct PlayerScore
    {
        public int horns_given;
        public int deaths;
        public int horns_gotten;
        public int quickest_death;
        public int horns_objects;
        public int grass_blocks_eaten;
        public int vomit_blocks_eaten;
        public int underwater_frames;
        public int over_the_clouds_frames;
        public int bleats;
        public int time_flying;

        public static PlayerScore make_test_score()
        {
            var score = new PlayerScore();
            //score.horns_given = Game1.rangen.Next(60 * 200);
            //score.deaths = Game1.rangen.Next(60 * 200);
            //score.horns_gotten = Game1.rangen.Next(60 * 200);
            //score.time_second = Game1.rangen.Next(60 * 200);
            //score.horns_objects = Game1.rangen.Next(60 * 200);
            //score.grass_blocks_eaten = Game1.rangen.Next(200);
            //score.vomit_blocks_eaten = Game1.rangen.Next(200);
            //score.underwater_frames = Game1.rangen.Next(60 * 50);
            //score.over_the_clouds_frames = Game1.rangen.Next(60 * 50);
            //score.bleats = Game1.rangen.Next(50);
            //score.time_flying = Game1.rangen.Next(60 * 200);

            score.horns_given = 0;
            score.deaths = 0;
            score.horns_gotten = 0;
            score.quickest_death = 0;
            score.horns_objects = 0;
            score.grass_blocks_eaten = 0;
            score.vomit_blocks_eaten = 0;
            score.underwater_frames = 0;
            score.over_the_clouds_frames = 0;
            score.bleats = 0;
            score.time_flying = 0;

            return score;
        }
        public override string ToString()
        {
            return string.Format("colpisci:{0}\tmorti:{1}\tcolpito:{2}\ndtime:{3}\togget:{4}\terba:{5}\nvomit:{6}\tacqua:{7}\tnubi:{8}\nbeeh:{9}\tvolo:{10}", horns_given, deaths, horns_gotten, quickest_death, horns_objects, grass_blocks_eaten, vomit_blocks_eaten, underwater_frames, over_the_clouds_frames, bleats, time_flying);
        }
    }
}