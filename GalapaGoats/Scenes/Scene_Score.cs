using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using GalapaGoats.Scene;
using GalapaGoats.Scenes;
using GalapaGoats.Sprites;
using GalapaGoats;

namespace GalapaGoats.Scene
{
    class ScoreScreen : GameScreen //Finestra dei punteggi
    {
        SoundEffectInstance coin;

        protected Texture2D _menu_front;
        protected Sprite[] _chars;
        protected Texture2D _gold_column, _gold_chest_front;
        protected ScoreScreenAction current_action;
        protected Queue<ScoreScreenAction> action_queue;
        
        List<Challenge> challenges;

        int challenge_count = 3;

        //Gestione Nuvole
        private Vector2 deltaN;
        private float xMin = -40;
        private float xMax = -0;
        private bool destra = false;

        int[] golz;
        int[] slow_golz;

        Player[] _players;
        public int winning_player;
        //SoundEffectInstance drums = Sounds.drum.CreateInstance();

        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScoreScreen(int total_prize/* erba rimasta da distribuire*/,Player[] players)
        {
            _players = players;
            _chars = new Sprite[Globals.max_players];

            TransitionOnTime = TimeSpan.FromSeconds(0.7);
            TransitionOffTime = TimeSpan.FromSeconds(0.7);
            this.total_prize = total_prize; //Math.Max(total_prize, 75);
            //drums.IsLooped = true;
            //drums.Volume = 0.7f;
            //drums.Play();

            if (Globals.media_mode == MediaMode.Manual)
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Pause();
                }
            }
            else
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    MediaPlayer.Stop();
                }
            }

            create_action_queue();
            AudioPlayer.StopSong();
            AudioPlayer.Play("End Music");

            deltaN = new Vector2();
        }
        

        private void create_action_queue() //Crea la lista di azioni da effettuare in automatico
        {
            winning_team = new List<int>();
            golz = new int[Globals.max_players];
            slow_golz = new int[Globals.max_players];

            create_challenges();

            action_queue = new Queue<ScoreScreenAction>();

            action_queue.Enqueue(new ScoreScreenAction(3, Globals.vocabs[33], null, delegate { assign_gold(0); }));
            action_queue.Enqueue(new ScoreScreenAction(2, new string[] { Globals.vocabs[34], Globals.vocabs[35] }, null,null));
            action_queue.Enqueue(new ScoreScreenAction(2, Globals.vocabs[36] + total_prize + Globals.vocabs[37], null));
            for (int i = 0; i < challenge_count; i++)
            {
                action_queue.Enqueue(new ScoreScreenAction(2, new string[]{challenges[i].description_1,challenges[i].prize + " " + Globals.vocabs[38]+challenges[i].description_2} , challenges[i],null));
                int counter = i;
                action_queue.Enqueue(new ScoreScreenAction(3, new string[] { challenges[i].description_1, challenges[i].prize + " " + Globals.vocabs[38] + challenges[i].description_2 }, challenges[i], delegate { assign_gold(counter + 1); }));
            }

            ScoreScreenAction last_action;
            if (Globals.DoubleMode[0] || Globals.DoubleMode[1] || Globals.DoubleMode[2] || Globals.DoubleMode[3])
            {   
                winning_player = calc_winner(); //Viene calcolato per capire il punteggio singolo e costruirci le colonne 

                winning_team = calc_team_winner();

                if (winning_team.Count > 0)
                {
                    float total_winners = 0;
                    foreach (int i in winning_team)
                        total_winners += Globals.DoubleMode[i % 4] ? 0.5f : 1;
                    if (total_winners > 1)
                    {
                        last_action = new ScoreScreenAction(5, new string[] { Globals.vocabs[39], Globals.vocabs[40] }, new Challenge(), null);//delegate { drums.Stop(); Sounds.drum_end.Play(); Sounds.applause.Play(); });
                        for (int i = 0; i < 8; i++)
                        {
                            Globals.VictoryStreak[i] = 0;
                        }
                    }
                    else
                    {
                        if (winning_team.Count == 1)
                        {
                            last_action = new ScoreScreenAction(5, Globals.vocabs[75] + Globals.playernames[winning_team[0]], new Challenge(), null);//delegate { drums.Stop(); Sounds.drum_end.Play(); Sounds.applause.Play(); });
                            last_action.header_color = Globals.colors[winning_team[0]];
                        }
                        else
                        {
                            last_action = new ScoreScreenAction(5, Globals.vocabs[75] + Globals.playernames[winning_team[1]], new Challenge(), null);//delegate { drums.Stop(); Sounds.drum_end.Play(); Sounds.applause.Play(); });
                            last_action.header_color = Globals.colors[winning_team[1]];
                        }
                    }
                    action_queue.Enqueue(last_action);
                }
            }
            else
            {
                winning_player = calc_winner();

                if (winning_player == 8)
                {
                    last_action = new ScoreScreenAction(5, new string[] { Globals.vocabs[39], Globals.vocabs[40] }, new Challenge(), null);//delegate { drums.Stop(); Sounds.drum_end.Play(); Sounds.applause.Play(); });
                    for (int i = 0; i < 8; i++)
                    {
                        Globals.VictoryStreak[i] = 0;
                    }
                }
                else
                {
                    last_action = new ScoreScreenAction(5, Globals.vocabs[41] + Globals.playernames[winning_player], new Challenge(), null);//delegate { drums.Stop(); Sounds.drum_end.Play(); Sounds.applause.Play(); });
                    for (int i = 0; i < 8; i++)
                    {
                        if (winning_player == i)
                        {
                            Globals.Victories[i]++;
                            Globals.VictoryStreak[i]++;
                        }
                        else
                            Globals.VictoryStreak[i] = 0;
                    }
                }
                last_action.header_color = Globals.colors[winning_player];

                action_queue.Enqueue(last_action);
            }
            dequeue_action();
        }

        internal void assign_gold(int phase) //Accumulo denaro vinto guadagnato e vinto nelle sfide
        {
            for (int i = 0; i < Globals.PlayerIndex.Count; i++)
            {
                int index = Globals.PlayerIndex[i];
                if (phase == 0)
                    golz[index] = Globals.Money[index];
                else if (challenges[phase - 1].winners.Contains(index))
                {
                    golz[index] += challenges[phase - 1].share;
                }
            }
        }

        private void create_challenges() //Crea bonus finale in maniera casuale
        {
            challenges = new List<Challenge>();
            List<int> ids = new List<int>();
            for (int i = 0; i < challenge_count; i++)
            {
                int id = Game1.rangen.Next(Challenge.NOfChallenges);
                while (ids.Contains(id))
                {
                    id = (id + 1) % Challenge.NOfChallenges;
                }
                challenges.Add(new Challenge(id, total_prize / challenge_count));
                ids.Add(id);
            }
        }
        

        private List<int> calc_team_winner() //Determina il giocatore vincente
        {
            int score = 0; int indexP1 = 0; int indexP2 = 0;
            
            //Globals.money = new int[Globals.player_index.Count];
            
            List<int> team_winners = new List<int>();
            for (int i = 0; i < Globals.PlayerIndex.Count; i++)
            {
                indexP1 = Globals.PlayerIndex[i];
                if (indexP1 >= 4)
                {
                    indexP2 = Globals.PlayerIndex[i]-4;
                    score = Globals.Money[indexP1] + Globals.Money[indexP2];
                    for (int j = 0; j < challenge_count; j++)
                    {
                        if (challenges[j].winners.Contains(indexP1))
                            score += challenges[j].share;
                        if (challenges[j].winners.Contains(indexP2))
                            score += challenges[j].share;
                    }

                    if (score > max_score)
                    {
                        max_score = score;
                        team_winners.Clear();
                        team_winners.Add(indexP1);
                        team_winners.Add(indexP2);                        
                    }
                    else if (score == max_score)
                    {
                        team_winners.Add(indexP1);
                        team_winners.Add(indexP2);
                    }
                }
                else if (!Globals.DoubleMode[Globals.PlayerIndex[i]%4]) //Il giocatore singolo deve contare come una squadra a se stante
                {
                    score = Globals.Money[indexP1];
                    for (int j = 0; j < challenge_count; j++)
                        if (challenges[j].winners.Contains(indexP1))
                            score += challenges[j].share;

                    if (score > max_score)
                    {
                        max_score = score;
                        team_winners.Clear();
                        team_winners.Add(indexP1);
                    }
                    else if (score == max_score)
                    {
                        team_winners.Add(indexP1);
                    }
                }
                score = 0;
            }
            return team_winners;
        }

        private int calc_winner() //Determina il giocatore vincente
        {
            //Globals.money = new int[Globals.player_index.Count];
            List<int> winners = new List<int>();
            for (int i = 0; i < Globals.PlayerIndex.Count; i++)
            {
                int index = Globals.PlayerIndex[i];
                int score = Globals.Money[index];
                for (int j = 0; j < challenge_count; j++)
                    if (challenges[j].winners.Contains(index))
                        score += challenges[j].share;

                if (score > max_score_single)
                {
                    max_score_single = score;
                    winners.Clear();
                    winners.Add(index);
                }
                else if (score == max_score_single)
                {
                    winners.Add(index);
                }
            }
            if (winners.Count > 1)
                return 8;
            else
                return winners[0];
        }

        public override void LoadContent()
        {
            base.LoadContent();

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            back = ScreenManager.Content.Load<Texture2D>("Hud\\menu_sfondo");
            _gold_chest_front = ScreenManager.Content.Load<Texture2D>("Hud\\cesso");
            _menu_front = ScreenManager.Content.Load<Texture2D>("Hud\\menu_fronte");
            _gold_column = ScreenManager.Content.Load<Texture2D>("Hud\\shit_column");
            var base_column_rect = new Rectangle(0, 0, _gold_column.Width, _gold_column.Height);

            nuvola_front = ContentLoader.Load<Texture2D>("Hud\\nuvola-1");
            nuvola_back = ContentLoader.Load<Texture2D>("Hud\\nuvola-2");
            onda_front = ContentLoader.Load<Texture2D>("Hud\\onda-1");
            onda_back = ContentLoader.Load<Texture2D>("Hud\\onda-2");


            _gold_column_rects = new Rectangle[Globals.max_players];
            
            foreach (int i in Globals.PlayerIndex)
            {
                _chars[i] = new Sprite(ScreenManager.Content.Load<Texture2D>("Tiles\\Inferno\\2"), Scene_Game.name_dictionaries[2][string.Format("SCORE_{0}_{1}",Scene_Game.colors_name[i%4],(i/4+1))].source_rectangle);                
                _chars[i].scale = Vector2.One * (Globals.max_players == 4 ? 1 : 1);
                int pos_index = Globals.max_players > 4 ? (i % 4) * 2 + i / 4 : i;
                _chars[i].Position = new Vector2(640 / Globals.max_players + 1280 / Globals.max_players * pos_index, 400);
                _chars[i].SpriteEffect = SpriteEffects.None;
                _gold_column_rects[i] = base_column_rect;
            }
        }
        Rectangle[] _gold_column_rects;

        //Gestione Input
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (current_action.can_skip)
            {
                for (int i = 0; i < Globals.PlayerIndex.Count; i++)
                {
                    if (Globals.PlayerIndex[i] < 4)
                    {
                        input.selected_index = Globals.PlayerIndex[i];

                        if (input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.A))
                        {
                            current_action.Skip();
                            break;
                        }
                    }
                }
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!coveredByOtherScreen)
            {
                    current_action.Update();

                if ( current_action.terminated)
                    dequeue_action();
                bool play_coins = false;
                for (int i = 0; i < slow_golz.Length; i++)
                {
                    if (slow_golz[i] != golz[i]) //Gestione incremento della pila di soldi
                    {
                        int delta = (golz[i] - slow_golz[i]);
                        slow_golz[i] += Mathf.Clamp(delta, 0, 3);
                        play_coins = true;
                    }
                    if (_chars[i] != null)
                        _chars[i].Update();
                }
                if (play_coins)
                {
                    if (coin == null || coin.State != SoundState.Playing)
                    {
                        if (coin == null || coin.State == SoundState.Stopped)
                        {
                            coin = AudioPlayer.Play("Count01",true);
                        }
                        else if (coin.State == SoundState.Paused)
                        {
                            coin.Resume();
                        }
                    }
                }
                else
                {
                    if (coin != null)
                        if (coin.State == SoundState.Playing)
                            coin.Pause();
                }
            }

            if (deltaN.X < xMin)
            {
                destra = true;
                deltaN.X += 0.2f;
            }
            else if (deltaN.X > xMax)
            {
                destra = false;
                deltaN.X -= 0.2f;
            }
            if (destra == true)
                deltaN.X += 0.2f;
            else
                deltaN.X -= 0.2f;
        }


        Challenge last_challenge=null;
        private void dequeue_action() //Rimuove un'azione dalla lista
        {
            if (action_queue.Count > 0)
            {
                current_action = action_queue.Dequeue();
                if (current_action.action != null)
                    current_action.action.Invoke();

                if (current_action.challenge != null)
                    last_challenge = current_action.challenge;

            }
            else
            {
                if (MediaPlayer.State == MediaState.Paused)
                    MediaPlayer.Resume();
                return_to_lobby();
            }

        }

        private void return_to_lobby() //Gestione chiusura finestra
        {
            ExitScreen();
            ScreenManager.AddScreen(new Scene_Victory(winning_player));   
        }

        Color gold_color = new Color(224,187,0,255);
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            ScreenManager.SpriteBatch.Draw(back, Vector2.Zero, Color.White);
            //ScreenManager.SpriteBatch.Draw(_menu_front, Vector2.Zero, Color.White);

            Draw_Waves();
            Draw_Clouds();

            for (int i = 0; i < Globals.PlayerIndex.Count; i++)
            {
                //if (Globals.player_index.Contains(i))
                //{

                //Disegno elementi scena
                var pos = _chars[Globals.PlayerIndex[i]].Position;
                if (Globals.PlayerIndex[i] > 3 || Globals.DoubleMode[i%4])
                {
                    int pos_index = (Globals.PlayerIndex[i] % 4) * 2 + Globals.PlayerIndex[i] / 4;
                    pos.X = (32+(640-32) / 8 + (1280-64) / 8 * pos_index);
                }
                _gold_column_rects[Globals.PlayerIndex[i]].Height = Math.Max((int)((slow_golz[Globals.PlayerIndex[i]] / (float)max_score_single) * 416), 1);
                _gold_column_rects[Globals.PlayerIndex[i]].Y = 428 - _gold_column_rects[Globals.PlayerIndex[i]].Height;
                Vector2 _gold_column_pos = new Vector2((int)(_chars[Globals.PlayerIndex[i]].Position.X - 32), 656 - _gold_column_rects[Globals.PlayerIndex[i]].Height + 32);

                pos.Y = (700 - _gold_column_rects[Globals.PlayerIndex[i]].Height);
                _chars[Globals.PlayerIndex[i]].Position = pos;
                _chars[Globals.PlayerIndex[i]].color = Color.White;
                Vector2 chest_center = new Vector2(_gold_chest_front.Width / 2, _gold_chest_front.Height);
                ScreenManager.SpriteBatch.Draw(_gold_column, _gold_column_pos, _gold_column_rects[Globals.PlayerIndex[i]], Color.White);
                _chars[Globals.PlayerIndex[i]].color =Color.White;
                _chars[Globals.PlayerIndex[i]].Draw();
                ScreenManager.SpriteBatch.Draw(_gold_chest_front, new Vector2(pos.X, 688), null, Color.White, 0, chest_center, 1, SpriteEffects.None, 0.5f);
                draw_string(slow_golz[Globals.PlayerIndex[i]].ToString(), new Vector2(_chars[Globals.PlayerIndex[i]].Position.X, 720 - 46), 1f, new Color(255, 216, 0, 255), 1);
                if (last_challenge != null)
                {
                    if (last_challenge.parameters != null)
                    {
                        int param = last_challenge.parameters[Globals.PlayerIndex[i]];
                        bool plural = param != 1;
                        draw_string(param.ToString() + " " + last_challenge.unit + (plural ? "S" : ""), new Vector2(pos.X, 196), Globals.DoubleMode[Globals.PlayerIndex[i] % 4] ? 0.8f : 1f, Color.White);
                    }
                }
                //}
                //else
                //{
                //    _chars[Globals.player_index[i]].color = semitransparent_color;
                //    _chars[Globals.player_index[i]].Draw();
                //}

            }
            if (current_action.header_text.Length == 1)
                draw_string(current_action.header_text[0] , new Vector2(640, 96), 4, current_action.header_color);
            else
            {
                draw_string(current_action.header_text[0], new Vector2(640, 96 - 24), 1.8f, current_action.header_color);
                draw_string(current_action.header_text[1], new Vector2(640, 96 + 24), 1.8f, current_action.header_color);            
            }
            ScreenManager.SpriteBatch.End();
        }

        private void Draw_Clouds()
        {
            deltaN.Y = 2 * (float)Math.Sin(0.5 * (double)deltaN.X);
            ScreenManager.SpriteBatch.Draw(nuvola_back, deltaN, new Color(220, 220, 220, 220));

            deltaN.Y = (float)Math.Cos(0.5 * (double)deltaN.X - Math.PI);
            ScreenManager.SpriteBatch.Draw(nuvola_front, new Vector2(-deltaN.X - 40, deltaN.Y), new Color(200, 200, 200, 200));
        }

        private void Draw_Waves()
        {
            deltaN.Y = 4 * (float)Math.Sin(0.5 * (double)deltaN.X);
            ScreenManager.SpriteBatch.Draw(onda_back, new Vector2(deltaN.X, 550 + deltaN.Y), new Color(220, 220, 220, 220));

            deltaN.Y = 3*(float)Math.Cos(0.5 * (double)deltaN.X - Math.PI);
            ScreenManager.SpriteBatch.Draw(onda_front, new Vector2(-deltaN.X - 40, 550 + deltaN.Y), new Color(200, 200, 200, 200));
        }

        Color semitransparent_color = new Color(96, 96, 96, 96);
        private int total_prize;
        private Texture2D back;
        private List<int> winning_team;
        private int max_score_single = int.MinValue;
        private int max_score = int.MinValue;
        private Texture2D nuvola_front;
        private Texture2D nuvola_back;
        private Texture2D onda_front;
        private Texture2D onda_back;
    }

//--------------------------------------------------------------------------------------------

    public class ScoreScreenAction //Struttura dati per la lista di azioni 
    {
        int _duration_in_frames;
        int remaining_frames;
        public Color header_color = Color.White;
        public Challenge challenge;
        public string[] header_text;
        public bool can_skip=true;
        public  Action action;
        public bool terminated { get { return remaining_frames <= 0; } }
        float duration { set { _duration_in_frames = (int)(value * 60); remaining_frames = _duration_in_frames; } get { return _duration_in_frames / 60f; } }

        public ScoreScreenAction(float duration, string header_text, Challenge challenge)
        {
            this.duration = duration;
            this.challenge = challenge;
            this.header_text = new string[] { header_text };
            this.action = null;
        }
        public ScoreScreenAction(float duration, string header_text, Challenge challenge, Action action)
        {
            this.duration = duration;
            this.challenge = challenge;
            this.header_text = new string[]{header_text};
            this.action = action;
        }
        public ScoreScreenAction(float duration, string[] header_text, Challenge challenge, Action action)
        {
            this.duration = duration;
            this.challenge = challenge;
            this.header_text = header_text;
            this.action = action;
        }
        public void Update()
        {
            this.remaining_frames--;
        }
        public void Skip()
        {
            this.remaining_frames = 1;
        }

    }

//-----------------------------------------------------------------------------------

    public class Challenge //Gestione dei bonus finali
    {
       public const int NOfChallenges = 11;

        int id;
        public int prize;
        public string description_1;
        public string description_2;
        public string unit;
        public int[] parameters;
        bool higher_wins = true;
        public List<int> winners=new List<int>();

        public Challenge()
        {
        }
        public Challenge(int id,int prize)
        {
            this.id = id;
            this.prize = prize;
            this.parameters = new int[Globals.max_players];

//CALCOLO DEI PUNTI DEI PLAYER IN BASE ALL'ANDAMENTO DELLA BATTAGLIA  
            switch (id)
            {
                case 0:
                    description_1 = Globals.vocabs[0];
                    description_2 = Globals.vocabs[1];
                    unit = Globals.vocabs[2];
                    break;
                case 1:
                    description_1 = Globals.vocabs[3];
                    description_2 = Globals.vocabs[4];
                    unit = Globals.vocabs[5];
                    break;
                case 2:
                    description_1 = Globals.vocabs[6];
                    description_2 = Globals.vocabs[7];
                    unit = Globals.vocabs[8];
                    break;
                case 3:
                    description_1 = Globals.vocabs[9];
                    description_2 = Globals.vocabs[10];
                    unit = Globals.vocabs[11];
                    break;
                case 4:
                    description_1 = Globals.vocabs[12];
                    description_2 = Globals.vocabs[13];
                    unit = Globals.vocabs[14];
                    break;
                case 5:
                    description_1 = Globals.vocabs[15];
                    description_2 = Globals.vocabs[16];
                    unit = Globals.vocabs[17];
                    break;
                case 6:
                    description_1 = Globals.vocabs[18];
                    description_2 = Globals.vocabs[19];
                    unit = Globals.vocabs[20];
                    break;
                case 7:
                    description_1 = Globals.vocabs[21];
                    description_2 = Globals.vocabs[22];
                    unit = Globals.vocabs[23];
                    break;
                case 8:
                    description_1 = Globals.vocabs[24];
                    description_2 = Globals.vocabs[25];
                    unit = Globals.vocabs[26];
                    break;
                case 9:
                    description_1 = Globals.vocabs[27];
                    description_2 = Globals.vocabs[28];
                    unit = Globals.vocabs[29];
                    break;
                case 10:
                    description_1 = Globals.vocabs[30];
                    description_2 = Globals.vocabs[31];
                    unit = Globals.vocabs[32];
                    break;
                default:
                    break;
            }

            for (int i = 0; i < Globals.max_players; i++)
                parameters[i] = higher_wins ? int.MinValue : int.MaxValue;
          
            for (int i = 0; i < Globals.PlayerIndex.Count; i++)
            {
                    int index = Globals.PlayerIndex[i];
                    switch (id)
                    {
                        case 0:
                            parameters[index] = Globals.scores[index].horns_given; // capre colpite con cornata
                            break;
                        case 1:
                            parameters[index] = Globals.scores[index].deaths; //numero di suicidi
                            break;
                        case 2:
                            parameters[index] = Globals.scores[index].horns_gotten; // numero di incornate subite
                            break;
                        case 3:
                            parameters[index] = Globals.scores[index].quickest_death/60;   //  tempo passato secondo in classifica
                            higher_wins = false;
                            break;
                        case 4:
                            parameters[index] = Globals.scores[index].horns_objects; // oggetti colpiti
                            break;
                        case 5:
                            parameters[index] = Globals.scores[index].grass_blocks_eaten;  // blocchi erba mangiata
                            break;
                        case 6:
                            parameters[index] = Globals.scores[index].vomit_blocks_eaten; // mangiatore di vomito
                            break;
                        case 7:
                            parameters[index] = Globals.scores[index].underwater_frames/60;  //più tempo sottacqua
                            break;
                        case 8:
                            parameters[index] = Globals.scores[index].over_the_clouds_frames/60;  // più tempo sopra le nuvole
                            break;
                        case 9:
                            parameters[index] = Globals.scores[index].bleats;   //chi bela di più
                            break;
                        case 10:
                            parameters[index] = Globals.scores[index].time_flying / 60;  //tempo in volo
                            break;
                        default:
                            parameters[index] = 0;
                            break;
                    }


            }
            int winning_score=int.MinValue;
            if (higher_wins)
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] > winning_score)
                    {
                        winning_score = parameters[i];
                        winners.Clear();
                        winners.Add(i);
                    }
                    else if (parameters[i] == winning_score)
                    {
                        winners.Add(i);
                    }
                }
            else
            {
                winning_score = int.MaxValue;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i] < winning_score)
                    {
                        winning_score = parameters[i];
                        winners.Clear();
                        winners.Add(i);
                    }
                    else if (parameters[i] == winning_score)
                    {
                        winners.Add(i);
                    }
                }
            }
        }

        public int share { get { return prize / winners.Count; } }
    }    
}
