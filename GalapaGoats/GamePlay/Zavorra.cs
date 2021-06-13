using System;
using System.Collections.Generic;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GalapaGoats.Sprites;
using GalapaGoats.Scene;


namespace GalapaGoats
{
    class Zavorra : AnimatedSprite
    {
        public int index;
        public const int digestione_frames_max = 30;
        public const float weight_mass = 0.05f * 27 * 29;

        public float player_sberrata_impulse = 2.5f;
        private float player_sberrata_hit_impulse = 1.5f;
        public int sberrata_cooldown_max = 60;//frames
        public int sberrata_collision_duration = 20;//frames
        
        public int digestione_frames_current = 0;
        public int sberrata_cooldown_current = 0;//frames

        float x_acc;
        private Body sberrata_body;
        private float r_sberrata;
        int scia_size = 10;
        public List<Vector2> scia_positions;

        //private static Texture2D dollar;// = Content.Load<Texture2D>("Graphics\\HUD\\dollar");
        //static Vector2 dollar_delta = new Vector2(-8, -34);
        
        public Vector2 dir;
        public int sberrata_received_cooldown_current;
        public int sberrata_received_cooldown_max=20;
        public bool grounded;

        public event Action<Player, Vector2> onHitBySberrata;

        public Zavorra(int c_index, Body c_body,Texture2D tex, Rectangle source_rect,string animation_name)
            : base(tex, source_rect,animation_name)
        {
            this.index = c_index;

            body = c_body;
            this.body.UserData = this;
            this.body.BodyType = BodyType.Dynamic;
            this.body.Friction = 0.5f;
            //fromTextureFeetToBodyCenter = new Vector2(0,-16);
            this.center = new Vector2(this.Source_Rect.Width / 2, this.Source_Rect.Height / 2);
            this.updateCenter = false;
            var j = JointFactory.CreateAngleJoint(Scene_Game.world, Scene_Game.WorldAnchor, this.body);
            j.Softness = 0.990f;
            float nw = (this.Source_Rect.Width - 2);
            float nh = (this.Source_Rect.Height - 2);
            FixtureFactory.AttachRectangle(nw / 32f, nh / 32f, weight_mass / (nh * nw), Vector2.Zero, this.body);
            //Assegnamento Caratteristiche fisiche del player
            body.SleepingAllowed = false;
            body.CollisionCategories = Category.Cat1;

            dir = -Vector2.UnitY;
            SpriteEffect = dir.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            //Creazione body sberrata
            r_sberrata = 0.3f;
            sberrata_body = new Body(Scene_Game.world, body.Position + dir * (12 / 32f + r_sberrata));
            sberrata_body.BodyType = BodyType.Kinematic;
            FixtureFactory.AttachCircle(r_sberrata, 1, sberrata_body);
            //sberrata_body.IsSensor = true;
            sberrata_body.OnCollision += sberrata_body_OnCollision;
            sberrata_body.UserData = this;

            scia_positions = new List<Vector2>();
            
            PlayAnimation("MANGIA");
            
        }
       
        //Controllo collisioni della Sberrata
        bool sberrata_body_OnCollision(Fixture uno, Fixture due, Contact contact)
        {
            if (sberrata_active())
            {
                var fixtureB = uno.Body == sberrata_body ? due : uno;
                var fixtureA = uno.Body == sberrata_body ? uno : due;
                if (fixtureB.Body.UserData != null)
                {
                    if (fixtureB.Body.UserData is Player ) 
                    {
                        if ((fixtureB.Body.UserData as Player).index != (fixtureA.Body.UserData as Zavorra).index)//(fixtureB.Body.UserData != this)
                        {
                            var playerB = (fixtureB.Body.UserData as Player); //Gestione sberata con altre capre
                            if (playerB.sberrata_received_cooldown_current == 0)
                            {
                                AudioPlayer.Play(string.Format("Head{0:00}", Game1.rangen.Next(3) + 1));
                                fixtureB.Body.ApplyLinearImpulse(dir * player_sberrata_hit_impulse);
                                playerB.sberrata_received_cooldown_current = sberrata_received_cooldown_max;
                                Globals.scores[this.index].horns_given += 1;
                                Globals.scores[playerB.index].horns_gotten += 1;
                                if (playerB.onHitBySberrata != null)
                                    playerB.onHitBySberrata.Invoke(playerB,this, dir);
                            }
                        }
                        else
                        {
                            AudioPlayer.Play(string.Format("Head{0:00}", Game1.rangen.Next(3) + 1));
                            fixtureB.Body.ApplyLinearImpulse(dir * (player_sberrata_hit_impulse*2));
                        }
                    } 
                    else if( fixtureB.Body.UserData is Zavorra)
                    {
                        
                    }
                    else if (fixtureB.Body.UserData is Sprite) //Gestione sberrata con elementi world(alberi, cactus, etc...)
                    {
                        var playerB = (fixtureB.Body.UserData as Sprite);
                        Globals.scores[this.index].horns_objects += 1;
                        sberrata_cooldown_current = sberrata_cooldown_max - sberrata_collision_duration;
                            AudioPlayer.Play(string.Format("Head{0:00}",Game1.rangen.Next(3)+1));
                            fixtureB.Body.ApplyLinearImpulse(dir * player_sberrata_hit_impulse);
                    }
                }
            }
            
            return false;
        }

        private bool sberrata_active() //Controlla se una sberrata è in corso
        {
            return sberrata_cooldown_current + sberrata_collision_duration > sberrata_cooldown_max;
            
        }
        
        public override void Update()
        {
            if (sberrata_cooldown_current > 0) //Tempo di stanby tra una sberrata e l'altra
                sberrata_cooldown_current--;
            if (sberrata_received_cooldown_current > 0)
                sberrata_received_cooldown_current--;
            if (digestione_frames_current > 0)
                digestione_frames_current--;


            if (touching_ground()) //Cambio parametri fisici
            {//a Terra
                grounded = true;
                player_sberrata_impulse = 2.5f;
            }
            else
            {//in Aria
                grounded = false;
                player_sberrata_impulse = 1.5f;
                Globals.scores[index].time_flying += 1;
            }
            
            sberrata_body.Position = body.Position + dir * (18 / 32f + r_sberrata);
            if (body.LinearVelocity.X > 0.5f)
                SpriteEffect = SpriteEffects.None;
            else if (body.LinearVelocity.X < -0.5f)
                SpriteEffect = SpriteEffects.FlipHorizontally;
            //Gestione della scia
            if (sberrata_active())
            {
                scia_counter = (scia_counter + 1) % 2;
                if (scia_counter == 0)
                {
                    scia_positions.Add(this.Position);
                    if (scia_positions.Count > scia_size - 1)
                        scia_positions.RemoveAt(0);
                }
            }
            base.Update();
        }
        int scia_counter;
        //Rinascita
        //public void respawn(Vector2 position)
        //{
        //    alive = true;
        //    this.body.IgnoreGravity = false;
        //    body.Position = position;
        //    body.LinearVelocity = Vector2.Zero;
        //}
        
        
        //private void Die()
        //{
        //    //Score recording          
        //    int coins_lost = (int)(0.15f * money);
        //    money -= coins_lost;
            
        //    Vector2 position = this.body.Position;
        //    this.body.Position = -Vector2.UnitX * 10;
        //    this.body.IgnoreGravity = true;
        //}

        ////Gestione input single player
        //internal void HandleInput(InputHelper input)
        //{
        //    process_input(input, Buttons.A, new[]{Buttons.X,Buttons.B}, input.GamePadState.ThumbSticks.Left.X,input.GamePadState.ThumbSticks.Left.Y, Buttons.Y);
        //}
        
        //Controllo input
        bool jump_pressed = false;
        int frames_from_last_jump = 0;
        private bool _alive=true;
        private int spawn_max_time=180;
        public int spawn_timer;
        public bool drowning;

        public float tot_point = 0;
       //private void process_input(InputHelper input, Buttons jump_control, Buttons[] beeeh_control, float move_control, float vertical_control, Buttons sberrata_control)
        //{
        //    if (sberrata_cooldown_current + sberrata_collision_duration > sberrata_cooldown_max)
        //        move_control = 0;
        //    frames_from_last_jump++;

        //    process_sberrata(input, sberrata_control);

        //}

        
        //Controllo se il player è a terra
        private bool touching_ground()
        {
            bool grounded = false;
            ContactEdge list = body.ContactList;
            while (list != null)
            {
                if (list.Contact.IsTouching)
                {
                    Vector2 manifold_normal;
                    FixedArray2<Microsoft.Xna.Framework.Vector2> manifold_points;

                    list.Contact.GetWorldManifold(out manifold_normal, out manifold_points);
                    if (Math.Abs(manifold_normal.Y) > 0.8f)
                    {
                        grounded = true;
                        break;
                    }
                }
                list = list.Next;
            }
            return grounded;
        }

        public override void Draw()
        {
            if (sberrata_active())
                for (int i = 0; i < scia_positions.Count; i++)
                {
                    var rect=new Rectangle((int)scia_positions[i].X,(int)scia_positions[i].Y-16,32,32);
                    var a = (0.3f +0.7f*i / (float)scia_positions.Count);
                    var color = new Color(a, a, a, a);
                    if (dir.X < 0)
                        batch.Draw(texture, rect, Source_Rect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 1f);
                    else
                        batch.Draw(texture, rect, Source_Rect, color, 0, Vector2.Zero, SpriteEffects.None, 1f);
                }

            base.Draw();            
        }

    }
}
