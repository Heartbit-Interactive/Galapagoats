using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalapaGoats.Sprites
{
    class AnimatedSprite : Sprite
    {
        static public Dictionary<string, Animation_Data> animation_dict = new Dictionary<string, Animation_Data>();

        string current_animation_name;
        Animation_Data current_anim;
        int current_frame_in_anim;
        private Frame _current_frame;
        private int frame_to_next_animation_frame;
        Frame current_frame { get { return _current_frame; } set { _current_frame = value; Source_Rect = value.Rect; } }        
        public string name;
        private bool animation_playing;


        public AnimatedSprite(Texture2D Text, Rectangle? Rect,string name) : base(Text,Rect)
        {
            this.name = name;
        }

        public void PlayAnimation(string animation_name,bool randomizeStartingFrame=false) //Partenza di un'animazione
        {
            var req_animation_name= string.Format("{0}_{1}",name,animation_name);
            animation_playing = true;
            if (req_animation_name != current_animation_name)
            {
                if (animation_dict.ContainsKey(req_animation_name))
                {
                    current_animation_name = req_animation_name;
                    current_anim = animation_dict[current_animation_name];
                    current_frame_in_anim = randomizeStartingFrame?Game1.rangen.Next(current_anim.frames.Count): 0;
                    current_frame = current_anim.frames[current_frame_in_anim];
                    frame_to_next_animation_frame = current_anim.game_frame_per_animation_frame;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Animation missing: {0}", req_animation_name));
                }
            }
        }
        public void StopAnimation() //Termina l'animazione
        {
            animation_playing = false;
        }

        public override void Update()
        {
            if (frame_to_next_animation_frame > 0)
                frame_to_next_animation_frame--;
            else if(current_anim!=null && animation_playing)
            {
                current_frame_in_anim = (current_frame_in_anim + 1) % current_anim.frames.Count;
                current_frame = current_anim.frames[current_frame_in_anim];
                frame_to_next_animation_frame = current_anim.game_frame_per_animation_frame; //reset the counter
            }
            base.Update();
        }        
    }

//----------------------------------------------------------------------------------------------------------

    class Animation_Data
    {
        public List<Frame> frames=new List<Frame>();
        public int fps;
        public int game_frame_per_animation_frame { get { return 60 / fps; } }
        
        public Animation_Data(int fps)
        {
            this.fps = fps;
        }

        internal void AddFrame(int frame_index, Frame new_frame)
        {
            frames.Add(new_frame);
            frames.Sort((fa, fb) => fa.frame_number.CompareTo(fb.frame_number));
        }
    }
}
