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
    class Player : AnimatedSprite
    {
        public int money = 0;
        public int index;
        public float player_acc_x = 1;
        public float max_speed_y = 28;
        public float horz_damp = 0.9f;//0.8f;
        public float vert_damp = 1;
        public float player_jump_impulse = 1.0f;
        public float player_jump_decel = 0.8f/14;
        public float player_sberrata_impulse = 2.5f;
        private float player_sberrata_hit_impulse = 2f;
        private float standard_force_volo_caprone = -1.55f; //-1.75f;
        public float density = 0.16f;
        public int sberrata_cooldown_max = 60;//frames
        public int sberrata_collision_duration = 20;//frames
        public int sberrata_cooldown_current = 0;//frames
        public float player_friction = 0.5f;//frames
        public int jump_control_frames = 14;
        float x_acc;
        private Body sberrata_body;
        private float r_sberrata;
        int scia_size = 10;
        List<Vector2> scia_positions;
        int down_timer_max = 10;
        public int down_timer = 0;
        public bool over_the_top;
        public Vector2 death_pos;
        public Vector2? respawn_position;
        public int time_from_last_death;
        public Zavorra zavorra; //Memorizza i dati relativi al suo peso
        public int stordimento = 0;

        private static Texture2D dollar;// = Content.Load<Texture2D>("Graphics\\HUD\\dollar");
        static Vector2 dollar_delta = new Vector2(-8, -34);

        ParticleEngine particleEngine; //Sistema particellare

        public bool alive
        {
            get { return _alive; }
            set
            {
                if (_alive != value)
                {
                    _alive = value;
                    if (!_alive)
                    {
                        Globals.scores[index].deaths += 1;
                        if (Globals.scores[index].quickest_death > time_from_last_death)
                            Globals.scores[index].quickest_death = time_from_last_death;
                        time_from_last_death = 0;
                    }
                    spawn_timer = _alive ? 0 : spawn_max_time;
                    death_pos = Position;
                }
            }
        }

        public void dispose()
        {
            particleEngine.dispose();
        }

        public Vector2 dir;
        public int sberrata_received_cooldown_current;
        public int sberrata_received_cooldown_max=20;
        private bool grounded;
        public SpritesetGame spriteset;

        public Action<Player,Sprite, Vector2> onHitBySberrata;

        public Player(SpritesetGame spriteset,int c_index, Body c_body,Texture2D tex, Rectangle source_rect,string name)
            : base(tex, source_rect,name)
        {
            this.spriteset = spriteset;

            //Assegnamento Caratteristiche fisiche del player
            body = c_body;
            this.index = c_index;
            body.SleepingAllowed = false;
            body.CollisionCategories = Category.Cat1;

            body.BodyType = BodyType.Dynamic;
            var joint = JointFactory.CreateAngleJoint(Scene_Game.world, Scene_Game.WorldAnchor, body);
            FarseerPhysics.Factories.FixtureFactory.AttachRectangle(24 / 32f, 16 / 32f, density, Vector2.Zero, body); //Assegno una forma rettangolare al body
            body.Friction = player_friction;
            fromTextureFeetToBodyCenter = Vector2.UnitY * (-10);
            body.UserData = this;

            Vector2 dir = (c_index % 2 == 0 ? Vector2.UnitX : -Vector2.UnitX);
            SpriteEffect = dir.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            
            //Creazione body sberrata
            r_sberrata=0.3f;
            sberrata_body = new Body(Scene_Game.world, body.Position + dir * (12 / 32f + r_sberrata));
            sberrata_body.BodyType = BodyType.Kinematic;
            FixtureFactory.AttachCircle(r_sberrata, 1, sberrata_body);
            sberrata_body.IsSensor = true;
            sberrata_body.OnCollision += sberrata_body_OnCollision;
            PlayAnimation("CAMMINA");

            scia_positions = new List<Vector2>();
            Globals.scores[index].quickest_death = 180 * 60 * 1000;

            List<Sprite> Sprites_Particles = new List<Sprite>();
            Sprites_Particles.Add(new Sprite(ContentLoader.Load<Texture2D>("Tiles/Galapagos/2"), Scene_Game.name_dictionaries[2]["SCORREGGIA_FUMO1"].source_rectangle));
            Sprites_Particles.Add(new Sprite(ContentLoader.Load<Texture2D>("Tiles/Galapagos/2"), Scene_Game.name_dictionaries[2]["SCORREGGIA_FUMO2"].source_rectangle));
            particleEngine = new ParticleEngine( Sprites_Particles, new Vector2(400, 240));
        }

        //Controllo collisioni della Sberrata
        bool sberrata_body_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (sberrata_active())
            {
                if (fixtureB.Body.UserData != null)
                {
                    if (fixtureB.Body.UserData is Player)
                    {
                        if (fixtureB.Body.UserData != this)
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
            if (!alive)
            {
                spawn_timer--;
                return;
            }
            if (down_timer > 0)
                down_timer--;
            if (sberrata_cooldown_current > 0) //Tempo di stanby tra una sberrata e l'altra
                sberrata_cooldown_current--;
            if (sberrata_received_cooldown_current > 0)
                sberrata_received_cooldown_current--;
            if (stordimento > 0)
            {
                stordimento--;
                PlayAnimation("SB_SUBITA");
            }

            if (drowning)
                Globals.scores[index].underwater_frames++;
            else if (over_the_top)
                Globals.scores[index].over_the_clouds_frames++;

            if (touching_ground() && (frames_from_last_jump > jump_control_frames)) //Cambio parametri fisici
            {//a Terra
                grounded = true;
                horz_damp = 0.8f;
                player_acc_x = 1;
                player_sberrata_impulse = 2.5f;
            }
            else
            {//in Aria
                horz_damp = 0.95f;
                grounded = false;
                player_acc_x = 0.5f;
                player_sberrata_impulse = 1.5f;
                Globals.scores[index].time_flying += 1;
            }
            sberrata_body.Position = body.Position + dir * (12 / 32f + r_sberrata);

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
            
            if (zavorra != null)
            {
                zavorra.Update();
            }
            particleEngine.Update();

            base.Update();
        }
        int scia_counter;
        //Rinascita
        public void respawn(Vector2 position)
        {
            alive = true;
            this.body.IgnoreGravity = false;
            body.Position = position;
            body.LinearVelocity = Vector2.Zero;
        }
        
        
        private void Die()
        {
            //Score recording          
            int coins_lost = (int)(0.15f * money);
            money -= coins_lost;
            
            Vector2 position = this.body.Position;
            this.body.Position = -Vector2.UnitX * 10;
            this.body.IgnoreGravity = true;
        }

        //Gestione input single player
        internal void HandleInput(InputHelper input)
        {
            process_input(input, Buttons.A, Buttons.X, input.GamePadState.ThumbSticks.Left.X, input.GamePadState.ThumbSticks.Left.Y, Buttons.Y,new []{Buttons.X,Buttons.B});
        }

        //Gestione input Splitted player
        internal void HandleInputSplit(InputHelper input, bool alternative_input)
        {
            if (!alive)
                return;
            if (alternative_input)
            {
                process_input(input, Buttons.B, Buttons.Y, input.GamePadState.ThumbSticks.Right.Y,-input.GamePadState.ThumbSticks.Right.X, Buttons.X);
            }
            else
            {
                process_input(input, Buttons.DPadLeft, Buttons.DPadDown, -input.GamePadState.ThumbSticks.Left.Y,input.GamePadState.ThumbSticks.Left.X, Buttons.DPadRight);
            }
        }

        //Controllo input
        bool jump_pressed = false;
        int frames_from_last_jump = 0;
        private bool _alive=true;
        private int spawn_max_time=180;
        public int spawn_timer;
        public bool drowning;
        
        private void process_input(InputHelper input, Buttons jump_control, Buttons beeeh_control, float move_control, float vertical_control, Buttons sberrata_control, Buttons[] sberrataZavorra_controls=null)
        {   
            if ((sberrata_cooldown_current + sberrata_collision_duration > sberrata_cooldown_max) || (this.stordimento>0))
                move_control = 0;
            frames_from_last_jump++;
            jump_pressed = input.IsButtonPress(jump_control);

            if ((grounded) && (this.stordimento == 0))
            {
                if (input.IsNewButtonPress(jump_control))
                {
                    process_jump(vertical_control < -0.5);
                    grounded = false;
                }
            }
            if(grounded)
            {
                process_move(move_control);                
                if (Math.Abs(move_control) > 0.5)
                {
                    PlayAnimation("CAMMINA");
                    dir = (x_acc > 0 ? Vector2.UnitX : -Vector2.UnitX);
                    SpriteEffect = dir.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                }
                else
                {
                    StopAnimation();
                }
            }
            else
            {
                if (frames_from_last_jump < jump_control_frames && !jump_pressed)
                    body.ApplyLinearImpulse(player_jump_decel * Vector2.UnitY);

                process_move(move_control);
                //PlayAnimation("SALTA");

                if (Math.Abs(move_control) > 0.5)
                {
                    dir = (x_acc > 0 ? Vector2.UnitX : -Vector2.UnitX);
                    SpriteEffect = dir.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                }
            }

            if (this.stordimento > 0)
                return;

            if (input.IsNewButtonPress(beeeh_control))
                process_beeeh();

            process_sberrata(input, sberrata_control);
            if (zavorra!=null)
                process_sberrata_zavorra(input, sberrataZavorra_controls);

        }

        //Spostamenti
        private void process_move(float x_control)
        {

            x_acc = player_acc_x * x_control;
            
            Vector2 speed = new Vector2(
                x_acc + body.LinearVelocity.X * horz_damp,
                body.LinearVelocity.Y);

            speed.Y = Mathf.Clamp(speed.Y, -max_speed_y, max_speed_y);

            body.LinearVelocity = speed;
        }

        //Riproduzione suono Beeeh con conprensiva sberrata del caprone
        private void process_beeeh()
        {
            Globals.scores[index].bleats++;
            AudioPlayer.Play(string.Format("Goat{0:00}", Game1.rangen.Next(1, 9)));
            PlayAnimation("BEH");
        }



        //Salta
        private void process_jump(bool down)
        {
            PlayAnimation("SALTA");
            if (down)
            {
                down_timer = down_timer_max * 10;
                body.Position += 12 / 32f * Vector2.UnitY;
                AudioPlayer.Play("Jump");
                frames_from_last_jump = 0;
            }
            else
            {
                body.ApplyLinearImpulse(-player_jump_impulse * Vector2.UnitY);
                frames_from_last_jump = 0;
                AudioPlayer.Play("Jump");
            }
        }

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

        //Gestione incornata
        private void process_sberrata(InputHelper input, Buttons sberrata_control)
        {
            if (sberrata_cooldown_current == 0)
            {
                if (input.IsNewButtonPress(sberrata_control)) //Sberrata Player
                {
                    scia_positions.Clear();
                    AudioPlayer.Play("Slide");
                    PlayAnimation("SBERRATA");
                    body.ApplyLinearImpulse(dir * player_sberrata_impulse);
                    sberrata_cooldown_current = sberrata_cooldown_max;
                }
            }
        }

        private void process_sberrata_zavorra(InputHelper input, Buttons[] sberrata_control)
        {
            // Carica verso l'alto
            //if (zavorra.sberrata_cooldown_current == 0)
            //{
            //    if (input.IsNewButtonPress(sberrata_control[0])) //Sberrata Zavorra
            //    {
            //        zavorra.scia_positions.Clear();
            //        AudioPlayer.Play("Slide");
            //        zavorra.body.ApplyLinearImpulse(-Vector2.UnitY * 1f);
            //        zavorra.sberrata_cooldown_current = zavorra.sberrata_cooldown_max;
            //    }
            //}
            //Effetto palloncino
            if (input.IsButtonPress(sberrata_control[1]))
            {
                zavorra.PlayAnimation("VOLA");
                zavorra.scia_positions.Clear();
                //AudioPlayer.Play("Slide");
                zavorra.body.ApplyForce(new Vector2(0, standard_force_volo_caprone));

                particleEngine.is_palloncino = true;
                particleEngine.EmitterLocation = new Vector2(zavorra.body.Position.X * 32, zavorra.body.Position.Y * 32);
            }
            else 
            { 
                zavorra.PlayAnimation("MANGIA");
                particleEngine.is_palloncino = false;
            }
        }

        public override void Draw()
        {
            if (!alive)
                return;

            if (sberrata_active())
                for (int i = 0; i < scia_positions.Count; i++)
                {
                    var rect=new Rectangle((int)scia_positions[i].X-16,(int)scia_positions[i].Y-32,32,32);
                    var a = (0.3f +0.7f*i / (float)scia_positions.Count);
                    var color = new Color(a, a, a, a);
                    if (dir.X < 0)
                        batch.Draw(texture, rect, Source_Rect, color, 0, Vector2.Zero, SpriteEffects.FlipHorizontally, 1f);
                    else
                        batch.Draw(texture, rect, Source_Rect, color, 0, Vector2.Zero, SpriteEffects.None, 1f);
                }

            particleEngine.Draw(batch);

            base.Draw();
            
        }

        internal Vector2 death_indicator_pos()
        {
            if (respawn_position.HasValue)
                return Vector2.Lerp(respawn_position.Value, death_pos, spawn_timer / (float)spawn_max_time);
            return death_pos;
        }
    }
}
