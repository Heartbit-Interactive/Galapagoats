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


namespace GalapaGoats
{
    class Player : Sprite
    {
        public static List<int> richest_index = new List<int>();
        public PlayerScore score;
        public int money = 0;
        public int index;
        public float player_acc_x = 3;
        public float max_speed_x = 14;
        public float max_speed_y = 28;
        public float horz_damp = 0.7f;
        public float vert_damp = 1f;
        public float player_jump_impulse = 0.25f;
        private static Texture2D dollar;// = Content.Load<Texture2D>("Graphics\\HUD\\dollar");
        static Vector2 dollar_delta = new Vector2(-8, -34);
        public bool alive = true;
        public int dir = 0;

        public Player(int c_index, Body c_body)
            : base(ContentLoader.Load<Texture2D>("goat1"), new Rectangle(0, 0, 32, 32))
        {
            
            body = c_body;
            this.index = c_index;

            body.SleepingAllowed = false;
            body.CollisionCategories = Category.Cat1;
        }
        public override void Update()
        {
            if (!alive)
                return;
            base.Update();
        }
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

        internal void HandleInputSplit(InputHelper input, bool alternative_input)
        {
            Vector2 speed;

            if (!alive)
                return;
            if (alternative_input)
            {
                if (touching_ground())
                {
                    if (input.IsNewButtonPress(Buttons.B))
                    {
                        body.ApplyLinearImpulse(-player_jump_impulse * Vector2.UnitY);
                    }

                    speed = new Vector2(
                    player_acc_x * input.GamePadState.ThumbSticks.Right.Y + body.LinearVelocity.X * horz_damp,
                    body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
                }
                else
                {
                    score.time_flying++;

                    speed = new Vector2(
                    body.LinearVelocity.X * 0.98f,
                    body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
                }

                if (input.IsNewButtonPress(Buttons.X))
                {

                }

                speed.Y = Mathf.Clamp(speed.Y, -max_speed_y, max_speed_y);

                body.LinearVelocity = speed;

                UpdateShootInputSplit(input, alternative_input);
            }
            else
            {
                if (touching_ground())
                {
                    if (input.IsNewButtonPress(Buttons.DPadLeft))
                    {
                        body.ApplyLinearImpulse(-player_jump_impulse * Vector2.UnitY);
                    }

                    speed = new Vector2(
                    player_acc_x * (-input.GamePadState.ThumbSticks.Left.Y) + body.LinearVelocity.X * horz_damp,
                    body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
                }
                else
                {
                    score.time_flying++;

                    speed = new Vector2(
                    body.LinearVelocity.X * 0.98f,
                    body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
                }

                if (input.IsNewButtonPress(Buttons.DPadRight))
                {

                }

                speed.Y = Mathf.Clamp(speed.Y, -max_speed_y, max_speed_y);

                body.LinearVelocity = speed;

                UpdateShootInputSplit(input, alternative_input);
            }
        }

        internal void HandleInput(InputHelper input)
        {
            Vector2 speed;
            
            if (touching_ground())
            {
                if (input.IsNewButtonPress(Buttons.A) || input.IsNewButtonPress(Buttons.LeftTrigger))
                {
                    body.ApplyLinearImpulse(-player_jump_impulse * Vector2.UnitY);
                }

                speed = new Vector2(
                    player_acc_x * input.GamePadState.ThumbSticks.Left.X + body.LinearVelocity.X * horz_damp,
                    body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
            }
            else
            {
                score.time_flying++;

                speed = new Vector2(
                body.LinearVelocity.X * 0.98f,
                body.LinearVelocity.Y * (body.LinearVelocity.Y > 0 ? vert_damp : 1));
            }

            if (input.IsNewButtonPress(Buttons.Y))
            {

            }

            speed.Y = Mathf.Clamp(speed.Y, -max_speed_y, max_speed_y);

            body.LinearVelocity = speed;

            UpdateShootInput(input);
            input.selected_index = 0;
        }

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

        private void UpdateShootInputSplit(InputHelper input, bool alt)
        {
            if (alt)
            {
            }
            else
            {
            }
        }

        private void UpdateShootInput(InputHelper input)
        {
            
        }

        public override void Draw()
        {
            if (!alive)
                return;

            if (richest_index.Contains(this.index))
            {
                batch.Draw(dollar, Position + dollar_delta, Color.White);
            }

            base.Draw();
        }
    }
}
