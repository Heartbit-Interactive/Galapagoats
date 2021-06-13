using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace GalapaGoats.Scenes
{    
    class MotionLaw
    {
        public enum MotionLawType { LinearMotion, Attraction }
        public Body moving_body;

        public MotionLawType type = MotionLawType.LinearMotion;
        public Vector2 start_pos;
        public Vector2 end_pos;
        public float speed;
        private int sign;
        public Vector2 dir;
        private Body target_body;
        private float intensity;

        public MotionLaw(Body current_Body)
        {
            this.moving_body = current_Body;
        }
        public MotionLaw(Body current_Body, Vector2 start_position, Vector2 target_position, float speed)
        {
            this.start_pos = start_position;
            this.end_pos = target_position;
            this.speed = speed;
            this.moving_body = current_Body;

            dir = (end_pos - start_pos);
            dir = Vector2.Normalize(dir);
            sign = 1;
        }

        public void Update(float dt)
        {
            switch (type)
            {
                case MotionLawType.LinearMotion:
                    moving_body.LinearVelocity = dir * speed * sign;

                    var D = end_pos - start_pos;
                    var C = moving_body.Position - start_pos;
                    var d = Vector2.Dot(C, dir) / D.Length();

                    if (d > 1)
                        sign = -1;
                    else if (d < 0)
                        sign = 1;
                    break;
                case MotionLawType.Attraction:
                default: //Per gestione attrazione del vomito alle capre
                    var dist=(target_body.Position-moving_body.Position);                    
                    var force = intensity/Math.Max(1,dist.LengthSquared());
                    if(force>0.01f)
                    moving_body.ApplyForce(Vector2.Normalize(dist) * force);
                    break;
            }
        }


        internal static MotionLaw CreateAttraction(Body body1, Body body2, float intensity)
        {
            var result = new MotionLaw(body1);
            result.type = MotionLawType.Attraction;
            result.target_body = body2;
            result.intensity = intensity;
            return result;
        }
    }
}
