using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#if !WINDOWS
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using GalapaGoats.Sprites;

namespace GalapaGoats.Scene
{
    class Scene_Victory : Scene_Lobby //Gestione della schermata di vittoria della selezione player dopo la prima partita
    {
        public Scene_Victory(int winning_player)
            : base(winning_player)
        { }

        protected override void Load_Character_Contents()
        {
            for (int i = 0; i < Globals.max_players; i++)
            {
                var name = string.Format("INDICATORE_{0}_{1}", Scene_Game.colors_name[i % 4], i / 4 + 1);
                _chars[i] = new Sprite(ScreenManager.Content.Load<Texture2D>("Tiles\\Inferno\\3"), Scene_Game.name_dictionaries[3][name].source_rectangle);
                _chars[i].scale = Vector2.One * (Globals.max_players == 4 ? scale_normal : scale_split);
                int pos_index = Globals.max_players > 4 ? (i % 4) * 2 + i / 4 : i;
                _chars[i].Position = new Vector2(640 / Globals.max_players + 1280 / Globals.max_players * pos_index, 400);
                _chars[i].SpriteEffect = i / 4 > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            _menu_front = ScreenManager.Content.Load<Texture2D>("Hud\\menu_fronte");
            base.Load_Character_Contents();
        }
        protected override void Draw_Custom_Text()
        {
            //if(winner<8)
            //    //draw_string( Globals.vocabs[41] + Globals.playernames[winner], new Vector2(640, 96), 4, Globals.colors[winner]);
            //    //draw_string(Globals.vocabs[42], new Vector2(640, 96), 4, Color.White);
            //else
            //    draw_string( Globals.vocabs[39], new Vector2(640, 96), 4, Globals.colors[winner]);
        }

    }


//-------------------------------------------------------------


    class Scene_Start : Scene_Lobby //Gestione schermata start(Selezione giocatori)
    {
        public Scene_Start()
            : base(0)
        { }

        protected override void Load_Character_Contents() //Caricamento Contenuti
        {
            for (int i = 0; i < Globals.max_players; i++)
            {
                var name=string.Format("OCCHI_CHIUSI_{0}_{1}", Scene_Game.colors_name[i % 4], i / 4 + 1);
                _chars[i] = new Sprite(ScreenManager.Content.Load<Texture2D>("Tiles\\Inferno\\3" ), Scene_Game.name_dictionaries[3][name].source_rectangle);
                _chars[i].scale = Vector2.One*(Globals.max_players==4?scale_normal:scale_split);
                int pos_index = Globals.max_players>4?(i % 4) * 2 + i / 4:i;
                _chars[i].Position = new Vector2(640 / Globals.max_players + 1280 / Globals.max_players * pos_index, 400);
                _chars[i].SpriteEffect = i / 4 > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            _menu_front = ScreenManager.Content.Load<Texture2D>("Hud\\menu_fronte");
            base.Load_Character_Contents();
        }

        protected override void Draw_Custom_Text()
        {
            //if (!Globals.is_demo)
            //    draw_string( Globals.vocabs[42], new Vector2(640, 90), 4);
            //else
            //{
            //    draw_string(Globals.vocabs[42], new Vector2(640, 76), 2);
            //    draw_string(Globals.vocabs[43], new Vector2(640, 122), 2);
            //}
        }
    }


//------------------------------------------------------------------------------------------------------------------------------


    class Scene_Lobby : GameScreen
    {
        protected Texture2D _menu_front, _pad, _pad_dual,_keyb,_keyb_dual;
        protected Sprite[] _chars;
        Vector2[] pos4 = new Vector2[4];


        Texture2D[] controls_back = {  ContentLoader.Load<Texture2D>("Hud\\Tutorial_01"), 
                                         ContentLoader.Load<Texture2D>("Hud\\Tutorial_02"), 
                                         ContentLoader.Load<Texture2D>("Hud\\Tutorial_03") };
#if WINDOWS
        Texture2D[] controls_back_W = {  ContentLoader.Load<Texture2D>("Hud\\Tutorial_PC_01"), 
                                         ContentLoader.Load<Texture2D>("Hud\\Tutorial_PC_02"), 
                                         ContentLoader.Load<Texture2D>("Hud\\Tutorial_PC_03") };

        Texture2D credits_tip = ContentLoader.Load<Texture2D>("Hud\\Credits_PC");
        Texture2D controls_tip = ContentLoader.Load<Texture2D>("Hud\\Controls_PC");

        Texture2D nuvola_front = ContentLoader.Load<Texture2D>("Hud\\nuvola-1");
        Texture2D nuvola_back = ContentLoader.Load<Texture2D>("Hud\\nuvola-2");

        Texture2D onda_front = ContentLoader.Load<Texture2D>("Hud\\onda-1");
        Texture2D onda_back = ContentLoader.Load<Texture2D>("Hud\\onda-2");
#else
        
        Texture2D credits_tip =  ContentLoader.Load<Texture2D>("Graphics\\HUD\\Credits");
        Texture2D controls_tip =  ContentLoader.Load<Texture2D>("Graphics\\HUD\\Controls");
#endif
        Vector2 control_start = new Vector2(32 + 8, 32);
        Vector2[] column_start = new Vector2[]{
            new Vector2(196*1,18),
            new Vector2(196*1+168*1,18),
            new Vector2(216*2+168*1,18),
            new Vector2(216*2+162*2,18),
            new Vector2(216*3+168*2,18),
            new Vector2(216*3+168*3,18)                };
        Vector2 line_height = new Vector2(0, 36);
        Vector2 credits_start = new Vector2((1280 - 64) / 2, 18 + 32);

        Vector2 line_height_credits = new Vector2(0, 24);
        Color semi_light = new Color(210, 210, 210, 210);
        Color almost_visible = new Color(64, 64, 64, 64);
        Color[] column_colors = new Color[] { Color.White, Color.White, Color.White };
        int winner = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Scene_Lobby(int winner)
        {
            this.winner = winner;
            TransitionOnTime = TimeSpan.FromSeconds(0.7);
            TransitionOffTime = TimeSpan.FromSeconds(0.7);
        }
        
        public override void LoadContent() 
        {
            base.LoadContent();
            _pad =  ContentLoader.Load<Texture2D>("Hud\\GamePad");
            _pad_dual = ContentLoader.Load<Texture2D>("Hud\\GamePad2");

            _keyb = ContentLoader.Load<Texture2D>("Hud\\Keyboard_W");
            _keyb_dual = ContentLoader.Load<Texture2D>("Hud\\Keyboard_2");
            
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            _chars = new Sprite[Globals.max_players];
            Load_Character_Contents();
            for (int i = 0; i < 4; i++)
            {
                pos4[i] = new Vector2(24+(640-24) / 4f+ (1280-48) / 4f * i, 328);
            }
            

            // play the first track from the album
            if (Globals.media_mode == MediaMode.Auto)
            {
                //To DO: Decommentare x musica
                AudioPlayer.PlaySong("Menu Music");
            }

            delta = new Vector2();

        }
        private Texture2D back;
        protected virtual void Load_Character_Contents() //Caricamento background
        { 
            back = ScreenManager.Content.Load<Texture2D>("Hud\\menu_sfondo");
    }
        const int ready_time = 180;
        int timer = ready_time;

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            base.HandleInput(input, gameTime);
            Update_Lobby_State(input);
            Update_Ready_Timer();
        }

        private void Update_Ready_Timer()
        {
            if (!all_ready)
            {
                if (timer > 0)
                {
                    timer = -1;
                }
            }
            else
            {
                if (timer < 0)
                {
                    timer = ready_time;
                }
                else if (timer > 0)
                {
                    timer--;
                }
                else
                {
                    Globals.arena = arena;
                    start_game();
                    timer = ready_time;
                    Globals.Ready = new bool[4];
                }
            }
        }
        int title_mode = 0;
        int arena = 0;
        private void Update_Lobby_State(InputHelper input)
        {
#if WINDOWS
            input.keyboard_split_mode = false;
                if (input.IsNewKeyPress(Keys.F1))
                {
                    if (title_mode == 0)
                        title_mode = 1;
                    else
                        title_mode = 0;
                }
                else if (input.IsNewKeyPress(Keys.F2))
                {
                    if (title_mode == 0)
                        title_mode = 2;
                    else
                        title_mode = 0;
                }
                else if (input.IsNewKeyPress(Keys.F3))
                {
                    if (title_mode == 0)
                        title_mode = -1;
                    else
                        title_mode = 0;
                }
#endif
            for (int i = 0; i < 4; i++)
            {
                input.selected_index = i;
                Globals.Connected[i] = input.Connected_Pads[i]; //Controllo dei pad connessi
#if !WINDOWS
                if (input.IsNewButtonPress(Buttons.RightShoulder))
                {
                    if (title_mode == 0)
                        title_mode = -1;
                    else
                        title_mode = 0;
                }
                else if (input.IsNewButtonPress(Buttons.LeftShoulder))
                {
                    if (title_mode == 0)
                        title_mode = 1;
                    else
                        title_mode = 0;
                }
#endif
                if (Globals.Connected[i])
                {
                    if (Globals.Joined[i])
                    {
                        if ((input.IsNewButtonPress(Buttons.LeftThumbstickLeft) || (input.IsNewKeyPress(Keys.Left))) && (i == winner))
                        {
                            arena = (arena+1)%2;
                        }
                        else if ((input.IsNewButtonPress(Buttons.LeftThumbstickRight) || (input.IsNewKeyPress(Keys.Right))) && (i == winner))
                        {
                            arena = Math.Abs((arena -1) % 2);
                        }

                        if (Globals.Ready[i])
                        {
                            if (input.IsNewButtonPress(Buttons.Back) || input.IsNewButtonPress(Buttons.B))
                            {
                                Globals.Ready[i] = false;
                            }
                        }
                        else //!ready[i]
                        {
                            if (input.IsNewButtonPress(Buttons.Y))
                            {
                                Globals.DoubleMode[i] = !Globals.DoubleMode[i];
                            }

                            if (input.IsNewButtonPress(Buttons.Back) || input.IsNewButtonPress(Buttons.B))
                            {
                                Globals.Joined[i] = false;
                                for (int j = i; j < i + 5; j += 4)
                                {
                                    var name = string.Format("OCCHI_CHIUSI_{0}_{1}", Scene_Game.colors_name[j % 4], j / 4 + 1);
                                    _chars[j].Source_Rect = Scene_Game.name_dictionaries[3][name].source_rectangle;
                                }
                            }
                            else if (input.IsNewButtonPress(Buttons.A))
                            {
                                Globals.Ready[i] = true;
                            }
                        }
                    }
                    else
                    {
                        if (input.IsNewButtonPress(Buttons.Start))
                        {
                            Globals.Joined[i] = true;
                            for (int j = i; j < i+5; j+=4)
                            {
                                var name = string.Format("INDICATORE_{0}_{1}", Scene_Game.colors_name[j % 4], j / 4 + 1);
                                _chars[j].Source_Rect = Scene_Game.name_dictionaries[3][name].source_rectangle;
                            }
                        }
                    }
                }
                else
                {
                    Globals.Joined[i] = false;
                    Globals.Ready[i] = false;
                }
            }
        }

        int demo_timer = 120;
        protected int scale_split = 2;
        protected int scale_normal = 3;

        //Gestione Nuvole
        private Vector2 delta;
        private float xMin=-40;
        private float xMax=-0;
        private bool destra=false;

        //SpritesetGame spriteset;
        /// <summary>
        /// Updates the menu.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            Game1.rangen.Next(2);
            if (demo_timer >= 0)
            {
                if(demo_timer==0)
#if !WINDOWS
                    Globals.is_demo = Guide.IsTrialMode;
#elif DEMO
                    Globals.is_demo = true;
#else
                    Globals.IsDemo = false;
#endif
                demo_timer--;
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (!coveredByOtherScreen)
            {
                for (int i = 0; i < Globals.max_players; i++)
                {
                    _chars[i].Update();
                }
            }


            if (delta.X < xMin)
            {
                destra = true;
                delta.X+=0.2f;
            }
            else if (delta.X > xMax)
            {
                destra = false;
                delta.X -= 0.2f;
            }
            if (destra == true)
                delta.X += 0.2f;
            else
                delta.X -= 0.2f;
        }


        private void start_game() //Creazione della schermata di gioco
        {
            ScreenManager.AddScreen(new Scene_Game());
            ExitScreen();
        }

        /// <summary>
        /// Draws the menu.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            ScreenManager.SpriteBatch.Draw(back, Vector2.Zero, Color.White);
            Draw_Waves();
            //ScreenManager.SpriteBatch.Draw(_menu_front, Vector2.Zero, Color.White);
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos_1, pos; 
                Draw_chars(i, out pos_1, out pos);
            }
#if WINDOWS
            draw_string(Globals.vocabs[44], new Vector2(640,702), 1);
#endif
            for (int i = 0; i < 4; i++)
            {
#if WINDOWS
                if (InputHelper.keyboard_index == i)
                {
                    if (!Globals.Joined[i])
                    {
                        draw_string(Globals.vocabs[45], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[46], pos4[i] + Vector2.UnitY * 288, 1);
                    }
                    else if (!Globals.Ready[i])
                    {
                        if (Globals.FavLevel[i] > 0)
                            draw_string("< " + Globals.vocabs[76 + arena] + Globals.FavLevel[i] + " >", pos4[i] + Vector2.UnitY * 256, 1);
                        else
                            draw_string("< "+Globals.vocabs[76 + arena]+" >", pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[49], pos4[i] + Vector2.UnitY * 288, 1);
                        draw_string(Globals.DoubleMode[i] ? Globals.vocabs[50]  : Globals.vocabs[51], pos4[i] + Vector2.UnitY * 320, 1);
                    }
                    else if (timer > 0)
                    {
                        draw_string(Globals.vocabs[52], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string((timer / 60 + 1).ToString() + " "+ Globals.vocabs[53], pos4[i] + Vector2.UnitY * 288, 1);
                    }
                    else
                    {
                        draw_string(Globals.vocabs[54], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[55], pos4[i] + Vector2.UnitY * 288, 1);
                        draw_string(Globals.vocabs[56], pos4[i] + Vector2.UnitY * 320, 1);
                    }
                }
                else
#endif
                {
                    if (!Globals.Connected[i])
                    {
                        draw_string(Globals.vocabs[57], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[58], pos4[i] + Vector2.UnitY * 288, 1);
                    }
                    else if (!Globals.Joined[i])
                    {
                        draw_string(Globals.vocabs[59], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[46], pos4[i] + Vector2.UnitY * 288, 1);
                    }
                    else if (!Globals.Ready[i])
                    {
                        if (Globals.FavLevel[i] > 0)
                            draw_string("< " + Globals.vocabs[76 + arena] + " " + Globals.FavLevel[i] + " >", pos4[i] + Vector2.UnitY * 256, 1);
                        else
                            draw_string("< " + Globals.vocabs[76 + arena] + " >", pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[60], pos4[i] + Vector2.UnitY * 288, 1);
                        draw_string((Globals.DoubleMode[i] ?Globals.vocabs[61] :Globals.vocabs[62]), pos4[i] + Vector2.UnitY * 320, 1);
                    }
                    else if (timer > 0)
                    {
                        draw_string(Globals.vocabs[52], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string((timer / 60 + 1).ToString() + Globals.vocabs[63]+" "+Globals.vocabs[53], pos4[i] + Vector2.UnitY * 288, 1);
                    }
                    else
                    {
                        draw_string(Globals.vocabs[54], pos4[i] + Vector2.UnitY * 256, 1);
                        draw_string(Globals.vocabs[55], pos4[i] + Vector2.UnitY * 288, 1);
                        draw_string(Globals.vocabs[56], pos4[i] + Vector2.UnitY * 320, 1);
                    }
                }
            }
            switch (title_mode)
            {
                case -1:
                    Draw_Clouds();
                    Draw_credits();
                    break;
                case 0:
                    Sprite.batch.Draw(controls_tip, control_start + new Vector2(16, 16), semi_light);
                    Sprite.batch.Draw(credits_tip, control_start + new Vector2(1280 - control_start.X / 2 - 64 - credits_tip.Width - 16, 16), semi_light);
                    Draw_Custom_Text();
                    break;
                case 1:
                    Draw_Clouds();
                    Draw_controls();
                    break;
                case 2:
                    Draw_Clouds();
                    Draw_controls_keyb();
                    break;
            }
            
            ////spriteset.Draw();
            //Draw_Clouds();

            ScreenManager.SpriteBatch.End();
        }

        private void Draw_Clouds()
        {
            delta.Y = 2*(float)Math.Sin(0.5*(double)delta.X);
            ScreenManager.SpriteBatch.Draw(nuvola_back, delta, new Color(220, 220, 220, 220));
            
            delta.Y = (float)Math.Cos(0.5*(double)delta.X-Math.PI);
            ScreenManager.SpriteBatch.Draw(nuvola_front, new Vector2(-delta.X-40,delta.Y), new Color(200,200,200,200));
        }

        private void Draw_Waves()
        {
            delta.Y = 4 * (float)Math.Sin(0.5 * (double)delta.X);
            ScreenManager.SpriteBatch.Draw(onda_back, new Vector2(delta.X, 550 + delta.Y), Color.White);

            delta.Y = 3*(float)Math.Cos(0.5 * (double)delta.X - Math.PI);
            ScreenManager.SpriteBatch.Draw(onda_front, new Vector2(-delta.X - 40, 550 + delta.Y), new Color(200, 200, 200, 200));
        }

        private void Draw_credits() //Disegno dei credits
        {
            draw_string(Globals.vocabs[64], credits_start, 1, semi_light);

            draw_string(Globals.vocabs[65], credits_start + line_height_credits, 1, semi_light);

            draw_string(Globals.vocabs[66], credits_start + line_height_credits * 2, 1, semi_light);

            draw_string(Globals.vocabs[67], credits_start + line_height_credits * 3, 1, semi_light);
        }

        private void Draw_controls() //Disegno dei controlli
        {

            bool split_any = false;
            bool split_all = true;
            for (int i = 0; i < 4; i++)
                if (Globals.Joined[i] && InputHelper.keyboard_index!=i)
                {
                    if (Globals.DoubleMode[i])
                    {
                        split_any = true;
                    }
                    else
                    {
                        split_all = false;
                    }
                }
            split_all &= split_any;

            column_colors[0] = split_all ? almost_visible : semi_light;
            column_colors[1] = split_any ? semi_light : almost_visible;
            column_colors[2] = column_colors[1];

            Sprite.batch.Draw(controls_back[0], control_start, column_colors[0]);
            Sprite.batch.Draw(controls_back[1], control_start + Vector2.UnitX * controls_back[0].Width, column_colors[1]);
            Sprite.batch.Draw(controls_back[2], control_start + Vector2.UnitX * (controls_back[0].Width + controls_back[1].Width), column_colors[2]);
            int j = 0;
            foreach (int i in new int[] { 0, 2, 4 })
            {
                j = i / 2;
                draw_string(Globals.vocabs[68], control_start + column_start[i], 1, column_colors[j]);
                draw_string(i == 0 ? Globals.vocabs[69] : "", control_start + column_start[i] + line_height, 1, column_colors[j]);
                draw_string(Globals.vocabs[70], control_start + column_start[i] + line_height * 2, 1, column_colors[j]);
            }
            foreach (int i in new int[] { 1, 3, 5 })
            {
                j = i / 2;
                draw_string(Globals.vocabs[71], control_start + column_start[i], 1, column_colors[j]);
                draw_string(Globals.vocabs[72], control_start + column_start[i] + line_height, 1, column_colors[j]);
                draw_string(Globals.vocabs[73], control_start + column_start[i] + line_height * 2, 1, column_colors[j]);
            }
        }
        private void Draw_controls_keyb() //Disegno dei controlli tastiera
        {

            bool split_any = false;
            bool split_all = true;
                    if (Globals.DoubleMode[InputHelper.keyboard_index])
                    {
                        split_any = true;
                    }
                    else
                    {
                        split_all = false;
                    }
            split_all &= split_any;

            column_colors[0] = split_all ? almost_visible : semi_light;
            column_colors[1] = split_any ? semi_light : almost_visible;
            column_colors[2] = column_colors[1];

            Sprite.batch.Draw(controls_back_W[0], control_start, column_colors[0]);
            Sprite.batch.Draw(controls_back_W[1], control_start + Vector2.UnitX * controls_back[0].Width, column_colors[1]);
            Sprite.batch.Draw(controls_back_W[2], control_start + Vector2.UnitX * (controls_back[0].Width + controls_back[1].Width), column_colors[2]);
            int j = 0;
            foreach (int i in new int[] { 0, 2, 4 })
            {
                j = i / 2;
                draw_string(Globals.vocabs[68], control_start + column_start[i], 1, column_colors[j]);
                draw_string(i == 0 ? Globals.vocabs[69]: "", control_start + column_start[i] + line_height, 1, column_colors[j]);
                draw_string(Globals.vocabs[70], control_start + column_start[i] + line_height * 2, 1, column_colors[j]);
            }
            foreach (int i in new int[] { 1, 3, 5 })
            {
                j = i / 2;
                draw_string(Globals.vocabs[71], control_start + column_start[i], 1, column_colors[j]);
                draw_string(Globals.vocabs[72], control_start + column_start[i] + line_height, 1, column_colors[j]);
                draw_string(Globals.vocabs[73], control_start + column_start[i] + line_height * 2, 1, column_colors[j]);
            }
        }

        void Draw_chars(int i, out Vector2 pos_1, out Vector2 pos) //Disegno delle facce dei caratteri
        {
            if (Globals.DoubleMode[i])
            {
                _chars[i].scale = scale_split * Vector2.One;
                _chars[i + 4].scale = scale_split * Vector2.One;

                pos_1 = _chars[i + 4].Position;
                pos = pos_1;

                pos.X = pos4[i].X +78 ;

                if(Globals.Connected[i])               
                {
                    if (Globals.Joined[i] && Globals.Victories[i + 4] > 0)
                    {
                        draw_string(Globals.vocabs[74]+" " + Globals.Victories[i+4], pos - Vector2.UnitY * (112 * _chars[i].scale.Y + 6), 1, Globals.colors[i+4]);
                    }
                }
                else
                {
                    pos.Y += (float)(_chars[i+4].scale.Y / 4 * Math.Sin(Math.PI * 2 * (pos.X / 80f - x_wave)));
                }

                _chars[i + 4].Position = pos;
                _chars[i + 4].alpha = Globals.Connected[i] ? 1 : 0.33f;
                _chars[i + 4].Draw();
                pos_1.X = pos.X;
                _chars[i + 4].Position = pos_1;
            }
            else
            {
                _chars[i].scale = Vector2.One*scale_normal;
                _chars[i + 4].alpha = 0;
            }

            pos_1 = _chars[i].Position;
            pos = pos_1;

            pos.X = pos4[i].X - (Globals.DoubleMode[i] ? 78 : 0);


            if (Globals.Connected[i])
            {
                pos.Y += -2;
                if (Globals.Joined[i] && Globals.Victories[i] > 0)
                {
                    draw_string(Globals.vocabs[74]+" " + Globals.Victories[i], pos+Vector2.One*8, 1, Globals.colors[i]);
                }
            }
            else
            {
                pos.Y += (float)(_chars[i].scale.Y / 4 * Math.Sin(Math.PI * 2 * (pos.X / 80f - x_wave)));
            }


            _chars[i].Position = pos;
            _chars[i].alpha = Globals.Connected[i] ? 1 : 0.33f;
            _chars[i].Draw();

            pos_1.X = pos.X;
            _chars[i].Position = pos_1;
            if (Globals.Joined[i])
                if (!Globals.DoubleMode[i])
                {
#if WINDOWS
                    if(InputHelper.keyboard_index==i)
                        Sprite.batch.Draw(_keyb, pos4[i] + Vector2.UnitY * 168, null, Color.White, 0, Vector2.One*16, 6, SpriteEffects.None, 0.5f);
                    else
#endif
                        Sprite.batch.Draw(_pad, pos4[i] + Vector2.UnitY * 168, null, Color.White, 0, Vector2.One * 16, 8, SpriteEffects.None, 0.5f);
                }
                else
#if WINDOWS
                    if (InputHelper.keyboard_index == i)
                        Sprite.batch.Draw(_keyb_dual, pos4[i] + Vector2.UnitY * 168, null, Color.White, 0, Vector2.One * 16, 6, SpriteEffects.None, 0.5f);
                    else
#endif
                        Sprite.batch.Draw(_pad_dual, pos4[i] + Vector2.UnitY * (146), null, Color.White, 0, Vector2.One * 16, 8, SpriteEffects.None, 0.5f);
        }

        

        protected virtual void Draw_Custom_Text()
        {
        }

        public bool all_ready 
        {
            get
            {
                int ready_count=0, joined_count=0;
                for (int i = 0; i < 4; i++)
                {
                    if (Globals.Connected[i])
                        if (Globals.Joined[i])
                        {
                            if (Globals.Ready[i])
                                ready_count++;
                            joined_count++;
                        }
                }
                return ready_count > 0 && ready_count == joined_count ;
            }
        }
    }
}