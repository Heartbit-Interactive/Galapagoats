#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using FarseerPhysics.DebugView;
using FarseerPhysics.Dynamics.Contacts;

#region Microsoft
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion
#region Farseer
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
#endregion
using GalapaGoats.Scenes;
using GalapaGoats.Sprites;
using System.Timers;
#endregion

namespace GalapaGoats.Scene
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Scene_Game : GameScreen //Scena principale di gioco
    {
        public class bloccoToRemove //Gestione blocchetti da rimuovere dopo l'animazione
        {
            public Sprite spriteR;
            public Body bodyR;
            public Body Caprone;
            public int TTLR;
            public bloccoToRemove(Sprite sprite,Body body,Body caprone,int TTL)
            {
                Caprone = caprone;
                spriteR = sprite;
                bodyR = body;
                TTLR = TTL;
            }
        }
        public List<bloccoToRemove> blocchiToRemove;
        //public Timer RemoveBlocchiTimer;


        public int TTLmax = 60;
        const float t0 = 6;
        const float t1 = 90f;
        const float t2 = 140f;
        const float t3 = 180f;
        public static World world; //Creo il mondo fisico         
        public static Body WorldAnchor;
        internal static Dictionary<Color, CsvEntry>[] color_dictionaries; //Gli elem vengono identificati tramite mappe di colore
        internal static Dictionary<string, CsvEntry>[] name_dictionaries; //Gli elem vengono identificati tramite nome

        internal static readonly string[] colors_name = new string[] { "BLU", "GIALLA", "ROSSA", "VERDE" };
        readonly Vector3[] additive_colors = new[] { new Vector3(0, 0, 8), new Vector3(16, 0, 0), new Vector3(0, 0, 0) };
        readonly Vector3[] gradient_colors = new[] { new Vector3(255, 255, 255), new Vector3(191, 128, 128), new Vector3(26, 26, 102) };
        private readonly Queue<Tuple<Sprite,Body>>[] listaBlocchettiCibo = new Queue<Tuple<Sprite, Body>>[4];
        readonly int[] section_widths = new int[] { 20, 20, 20, 20, 40, 80 };
        readonly Vector3[] sky_colors = new[] { new Vector3(0, 140, 191), new Vector3(191, 53, 106), new Vector3(0, 0, 56) };
        readonly Vector3[] subtract_colors = new[] { new Vector3(0, 0, 0), new Vector3(0, 16, 16), new Vector3(48, 48, 24) };
        Vector2 a; Vector2 b;
        private Sprite[][] clouds;
        private Sprite gradient;
        private Color gradientColor;
        private float h_f;
        private int h_i;
        private Sprite[] indicators=new Sprite[8];
        float l = 5;
        private int level_height;

        List<MotionLaw> motion_laws = new List<MotionLaw>(); //Lista dei body animati(nuvole, etc..)
        private Color normal_rope_color = new Color(197, 161, 87, 255);
        private bool paused;
        Player[] players;
        private Matrix projection; //Matrice di proiezione
        private int ropeLimit=25;
        Sprite[][] ropesSprites;
        private bool score_screen;
        private Color skyColor;
        private List<Vector2> spawn_points = new List<Vector2>();
        float spawn_range=5*32;
        SpritesetGame spriteset; //Gstione sprite da visualizzare(in ordine di immissione)
        int standard_fps_for_animations = 10;

        //Punteggi standard guadagnati
        public int standard_gold_Erba = 1;//5;
        public int standard_gold_Fiori = 2;//8;
        public int standard_gold_Blocchetto = 2;
        public int standard_gold_Golden = 20;
        public int standard_gold_Vomit = 5;//10;
        private int stordimento_time_Max = 120;

        private float star_alpha;
        private List<Vector2> star_pos;
        private List<Sprite> stars;
        float t = -6; // tempo di partita (la pausa non lo aumenta) parte da meno 6 che è il tempo di attesa prima che cominci a muoversi la telecamera
        private Color tintColor = Color.White;
        private DebugViewXNA view; //Creo la vista per i body
        private bool view_debug_data;
        private int vomit_factor = 15;//Percento
        private float vomit_impulse = 0.15f;//Percento
        private float w_f;
        private int w_i;
        private Sprite[][] waves;
        Zavorra[] weights = new Zavorra[4];
        bool[] weights_drowning = new bool[4];
        private Color winning_team_color = Color.IndianRed;
        private Body lineaBloccoAlto;
        private string scenario = "Galapagos";

        public MessageManager message_manager = new MessageManager();//Gestione scritte punteggi
        private bool isEndGameRequested;


        public Scene_Game() : base()
        {
            Globals.LevelTotalPoint = 0;
            Globals.VomitTotalPoint = 0;
            for (int i = 0; i < listaBlocchettiCibo.Length; i++)
                listaBlocchettiCibo[i] = new Queue<Tuple<Sprite, Body>>();

            //RemoveBlocchiTimer = new Timer(1000);
            blocchiToRemove = new List<bloccoToRemove>();
        }

        public override void LoadContent() //Organizza parametri primari
        {
            switch (Globals.arena)
            {
                case 0:
                    scenario = "Galapagos";
                    break;
                case 1:
                    scenario = "Inferno";
                    break;
                default:
                    scenario = "Galapagos";
                    break;
            }

            //Coordinate del world fisico
            w_i = Game1.device.PresentationParameters.BackBufferWidth / 32;
            h_i = Game1.device.PresentationParameters.BackBufferHeight / 32;
            w_f = Game1.device.PresentationParameters.BackBufferWidth / 32f;
            h_f = Game1.device.PresentationParameters.BackBufferHeight / 32f;

            //Load_CSV();
            CreateAnimations();

            base.LoadContent();
            
            world = new World(Vector2.UnitY * 9.8f*2); //Creazione mondo Fisico
            WorldAnchor = BodyFactory.CreateBody(world);
            WorldAnchor.Position = new Vector2(0, 0);
            WorldAnchor.BodyType = BodyType.Static;

            var sprite_transform = Matrix.Identity; //Le conversioni vengono gestite autonomamente
            spriteset = new SpritesetGame(sprite_transform); //Creo lo spriteset
            add_stars();
            Load_Level();

            view = new DebugViewXNA(world); //Creazione DebugView(per visualizzazione body)
            view.LoadContent(Game1.device, ContentLoader.content);
            view.AppendFlags(DebugViewFlags.CenterOfMass);
                                    
            l = w_i / 8;

            Globals.Money = new int[Globals.max_players];

            players = new Player[8];
            Globals.scores = new PlayerScore[8];
            Globals.PlayerIndex = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                if (Globals.Joined[index])
                {
                    add_player(index);
                    if (Globals.DoubleMode[i])
                    {
                        add_player(index + 4);
                    }
                }
            }
            Connect_Players(); //Creazione corda
            if (scenario == "Inferno")
            {
                AudioPlayer.PlaySong("HellGoat Music");
            }
            else
            {
                //To DO: Decommentare
                AudioPlayer.PlaySong("Gameplay Music");
            }
            add_waves(); //Creazione onde e nubi            

            add_player_indicators(); //Indicatori di posiozione fuori dallo schermo

            //Ordinamento ListeLivello Spriteset
            Sprite.SpriteComparer cp = new Sprite.SpriteComparer();
            for (int i = 0; i < 4; i++)
                spriteset.Layers[i].Sort(cp);
        }

        private void add_player_indicators() //Indicatori di posiozione fuori dallo schermo
        {
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player == null)
                    continue;
                string frame_name = String.Format("INDICATORE_{0}_{1}", colors_name[i % 4], i / 4 + 1);
                var indicator_sprite = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/3"), name_dictionaries[3][frame_name].source_rectangle);
                indicators[i] = indicator_sprite;
                spriteset.addDynamic(indicator_sprite, 3,false);
            }
        }

        private Body add_player(int playerIndex) //Aggiunge giocatori
        {
            var pos = Vector2.UnitX * (1 + w_i / 4 * (playerIndex%4)+w_i / 8 *(playerIndex/4)) + Vector2.UnitY * (h_i - 2);
            Body player_body = new Body(world, pos, 0); //Creazione body
            string frame_name = String.Format("CAPRA {0}-{1}_CAMMINA 1f", colors_name[playerIndex / 2], playerIndex % 2 + 1);
            string name = String.Format("CAPRA {0}-{1}", colors_name[playerIndex %4], playerIndex / 4 + 1);
            players[playerIndex] = new Player(spriteset,playerIndex, player_body, (ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/2")), name_dictionaries[2][frame_name].source_rectangle, name);
            players[playerIndex].onHitBySberrata += onHitBySberrata; //Associa l'handle della sberrata ricevuta ai players
            //players[playerIndex].zavorra.onHitBySberrata += onHitBySberrata; 
            spriteset.addDynamic(players[playerIndex], 2,false); //Caricamento Texture2D
            Globals.PlayerIndex.Add(playerIndex);
            return player_body;
        }

        private void onHitBySberrata(Player giocatoreColpito,Sprite giocatoreCheHaColpito, Vector2 dir) //Creazione del vomito della capra dopo aver subito la sberrata
        {
            if ((giocatoreColpito.index == (giocatoreCheHaColpito as Player).index + 4) || (giocatoreColpito.index + 4 == (giocatoreCheHaColpito as Player).index))
                return;
            //Gestione punti Vinti/Persi            
            int punti_persi=calcolaPuntiPersi(giocatoreColpito);
            int numero_vomitini = punti_persi / standard_gold_Vomit;
            Globals.VomitTotalPoint += punti_persi; //Memorizzo punti tolti al giocatore che subisce una sberrata
            AddPlayerScore(giocatoreColpito, -punti_persi, false, false);
            AudioPlayer.Play("Vomit");
            giocatoreColpito.PlayAnimation("SB_SUBITA");
            giocatoreColpito.stordimento = stordimento_time_Max;
            //Creazione degli nuovi elementi(Vomito)
            for (int i = 0; i < numero_vomitini; i++)
            {
                Vomita(giocatoreColpito, dir,giocatoreCheHaColpito);
            }
        }

        private void Vomita(Player giocatoreColpito, Vector2 dir,Sprite giocatoreCheHaColpito=null)
        {
            //if ((giocatoreColpito.index == (giocatoreCheHaColpito as Player).index + 4) || (giocatoreColpito.index + 4 == (giocatoreCheHaColpito as Player).index))
            //    return;
            Vector2 vomit_dir = Vector2.Transform(-dir, Matrix.CreateRotationZ(MathHelper.ToRadians(Math.Sign(dir.X) * 60 + Game1.rangen.Next(-10, 11))));
            var starting_position = giocatoreColpito.Position / 32f + vomit_dir * 0.75f;
            var body = new Body(world, starting_position, 0, null);
            body.BodyType = BodyType.Dynamic;
            var sprite = new Sprite(ContentLoader.Load<Texture2D>("Tiles/" + scenario + "/2"), name_dictionaries[2]["VOMITO_TOP"].source_rectangle);
            sprite.body = body;
            sprite.center = new Vector2(sprite.Source_Rect.Width, sprite.Source_Rect.Height) / 2;

            var sprite2 = new AnimatedSprite(ContentLoader.Load<Texture2D>("Tiles/" + scenario + "/2"), null, "VOMITO");
            sprite2.PlayAnimation("FUMO",true);
            sprite2.body = body;
            sprite2.fromTextureFeetToBodyCenter = Vector2.UnitY * 8;
            sprite2.lockRotation = true;
            sprite2.color = Globals.colors[(giocatoreCheHaColpito as Player).index];

            FixtureFactory.AttachRectangle(sprite.Source_Rect.Width / 32f, sprite.Source_Rect.Height / 32f, 0.05f, Vector2.Zero, body);
            body.CollisionCategories = Category.Cat10;

            body.CollidesWith = Category.All ^ Category.Cat10;
            body.Friction = 5f;
            body.OnCollision += body_vomit_OnCollision;
            body.ApplyLinearImpulse(vomit_dir * vomit_impulse);
            body.UserData = new object[] { sprite,sprite2, t + 1f }; //sprite per la rimozione , t+2 per avere due secondi in cui i giocatori non possono raccogliere il vomito

            //Aggiungi un attrazione gravitazionale tra vomito sparato e giocatore che mi ha colpito
            if(giocatoreCheHaColpito!=null)
            motion_laws.Add(MotionLaw.CreateAttraction(body,giocatoreCheHaColpito.body,1.2f));

            spriteset.addDynamic(sprite, 1, false); //Mem. sprite dinamico
            spriteset.addDynamic(sprite2, 1, false); //Mem. sprite dinamico
        }

        private int calcolaPuntiPersi(Player giocatoreColpito)
        {
            var punti_vomitati_temp = (int)(Globals.Money[giocatoreColpito.index] * vomit_factor / 100f);
            var numero_vomitini = punti_vomitati_temp / standard_gold_Vomit;
            return numero_vomitini * standard_gold_Vomit;
        }


        private void add_stars() //Aggiunge stelle allo afondo
        {
            stars = new List<Sprite>();
            star_pos = new List<Vector2>();
            var rect = name_dictionaries[0]["CIELO_STELLE"].source_rectangle;
            for (int y = rect.Height; y < h_f * 32 + rect.Height * 2; y += rect.Height)
            {
                for (int x = rect.Width / 2; x < 1280; x += rect.Width)
                {
                    var star_sprite = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/0"), rect);
                    star_sprite.Position = new Vector2(x, y);
                    star_sprite.alpha = 0;
                    star_pos.Add(new Vector2(x, y));
                    stars.Add(star_sprite);
                    spriteset.addDynamic(star_sprite, 0,true);
                }
            }
            string frame_name = "GRADIENT_1";
            gradient = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/0"), name_dictionaries[0][frame_name].source_rectangle);
            gradient.Position = new Vector2(w_f/2*32, h_f*32);
            gradient.scale = new Vector2(w_f * 32f / gradient.width, 1);
            gradient.alpha = 0.75f;
            spriteset.addDynamic(gradient, 0,true);
        }

        private void add_waves() //Gestione Onde e Nuvole
        {
            //Img doppie su 3 livelli
            waves = new Sprite[3][];
            for (int i = 0; i < 3; i++)
            {
                waves[i] = new Sprite[2];
                string frame_name = String.Format("ONDA_{0}", i + 1);
                for (int j = 0; j < 2; j++)
                {
                    waves[i][j] = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/3"), name_dictionaries[3][frame_name].source_rectangle);
                    waves[i][j].Position = new Vector2(waves[i][j].center.X + waves[i][j].width * j, waves[i][j].center.Y + h_i * 32);
                    spriteset.addDynamic(waves[i][j], 3,false);
                }
            }            
            clouds = new Sprite[3][];
            for (int i = 0; i < 3; i++)
            {
                clouds[i] = new Sprite[2];
                string frame_name = String.Format("NUBI_{0}", i + 1);
                for (int j = 0; j < 2; j++)
                {
                    clouds[i][j] = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/3"), name_dictionaries[3][frame_name].source_rectangle);
                    clouds[i][j].Position = new Vector2(clouds[i][j].center.X + clouds[i][j].width * j, clouds[i][j].center.Y - 10);
                    spriteset.addDynamic(clouds[i][j], 3,false);
                }
            }
        }

        private void CreateAnimations() //Crea e memorizza animazioni
        {
            foreach (var layer_dictionary in name_dictionaries)
            {
                foreach (var frame_in_dict in layer_dictionary.Values)
                { 
                    var matches=Regex.Matches(frame_in_dict.name,@"\d+f");
                    if (matches.Count <= 0) continue;
                    var frame_index = int.Parse(matches[0].Value.TrimEnd('f'))-1;
                    var animation_name = frame_in_dict.name.Replace(matches[0].Value,"").Trim();
                    int fps = standard_fps_for_animations;
                    if (animation_name.Contains("MANGIA"))
                        fps = 2;
                    else if (animation_name.StartsWith("VOMITO"))
                        fps =10;

                    if(!AnimatedSprite.animation_dict.ContainsKey(animation_name))
                        AnimatedSprite.animation_dict[animation_name]=new Animation_Data(fps);

                    var new_frame=new Frame {Rect = frame_in_dict.source_rectangle, frame_number = frame_index};
                    AnimatedSprite.animation_dict[animation_name].AddFrame(frame_index,new_frame );
                }
            }
        }

        private void Load_Level() //Creazione del level tramite mappe di colore
        {
            level_height = 0;
            string file_name = "Levels/"+scenario+"/{0}-{1}";
            var level_texts = new Texture2D[6][];
            Color[][][] textures_data = new Color[level_texts.Length][][];
            for (int level_id = 0; level_id < level_texts.Length; level_id++)
            {
                level_texts[level_id] = new Texture2D[4];
                textures_data[level_id] = new Color[4][];
                for (int layer_id = 0; layer_id < 4; layer_id++)
                {
                    try
                    {
                        level_texts[level_id][layer_id] = ContentLoader.Load<Texture2D>(string.Format(file_name, level_id + 1, layer_id));
                        textures_data[level_id][layer_id] = new Color[level_texts[level_id][layer_id].Width * level_texts[level_id][layer_id].Height];
                        level_texts[level_id][layer_id].GetData(textures_data[level_id][layer_id]);
                    }
                    catch { }
                }
            }
            for (int yi = 0; yi < level_texts.Length; yi++)
            {
                int section_width = section_widths[yi];
                int section_height = level_texts[yi][0].Height;
                level_height += yi > 0 ? section_height : 0;
                for (int xi = 0; xi < 80 / section_width; xi++) //Plase to insert the puzzle part
                {
                    int number_of_sections_in_file = level_texts[yi][1].Width / section_width;
                    int selected_part_rand = Game1.rangen.Next(number_of_sections_in_file);
                    for (int layer_id = 0; layer_id < 4; layer_id++)
                    {
                        var selected_part = layer_id == 0 ? xi : selected_part_rand;
                        int xs = xi * section_width;
                        int ys = -yi * section_height;
                        InsertLevelPart(layer_id, selected_part, section_width, xs, ys, level_texts[yi], textures_data[yi]);
                    }
                }
            }
            spawn_points.Sort((p1, p2) => p1.Y.CompareTo(p2.Y));

            //Creazione linea blocco in alto
            lineaBloccoAlto = new Body(world, new Vector2(0,0) / 32f);
            lineaBloccoAlto.BodyType = BodyType.Kinematic;
            FixtureFactory.AttachEdge(new Vector2(-100, 0) / 32f, new Vector2(1460, 0) / 32f, lineaBloccoAlto);
            lineaBloccoAlto.UserData = "LineaBlocco";
            lineaBloccoAlto.OnCollision += body_Linea_OnCollision;
        }

        

        private void InsertLevelPart(int layer_id,int selecter_part,int section_width, int xs, int ys, Texture2D[] level_texts, Color[][] texture_data) //Inserisce gli elementi da disegnare nel livello
        {
            var data_x_start = section_width * selecter_part;
            var data_y_start = 0;
                if(texture_data[layer_id]!=null)
                for (int dy = 0; dy < level_texts[0].Height; dy++) //scorre la texture
                {
                    var terra = new List<Sprite>();
                    int dx = 0;
                    var last_entry = new CsvEntry();
                    for (dx = 0; dx < section_width; dx++)
                    {
                        var col = texture_data[layer_id][(data_y_start+ dy) * level_texts[layer_id].Width + data_x_start+ dx];
                        if (col.A == 0)
                        {
                            chiudi_terra(terra, (xs + dx), (ys + dy), last_entry); //trovato pixel trasparente
                            terra.Clear();
                            last_entry = new CsvEntry();
                            continue;
                        }
                        if (color_dictionaries[layer_id].ContainsKey(col))
                        {
                            var entry = color_dictionaries[layer_id][col];

                            if (entry.category != last_entry.category)
                            {
                                chiudi_terra(terra, (xs + dx), (ys + dy), last_entry); //cambia categoria
                                terra.Clear();
                            }
                            bool moving = false;
                            var sprite = new Sprite(ContentLoader.Load<Texture2D>(string.Format("Tiles/"+scenario+"/{0}", layer_id)), entry.source_rectangle);
                            Body body;
                            switch (entry.category_array[0]) //Gestisce gli elementi del livello in  base alla categoria
                            {
                                case "UCCELLO":
                                    body = new Body(world, new Vector2(((xs + dx) * 16 + entry.source_rectangle.Width / 2) / 32f, ((ys + dy) * 16 + entry.source_rectangle.Height / 2) / 32f));
                                    body.BodyType = BodyType.Kinematic;
                                    FixtureFactory.AttachRectangle(entry.source_rectangle.Width / 3 * 1 / 32f, entry.source_rectangle.Height / 32f, 1, Vector2.Zero, body);
                                    sprite = new AnimatedSprite(ContentLoader.Load<Texture2D>(string.Format("Tiles/"+scenario+"/1", layer_id)), entry.source_rectangle,entry.category);
                                    sprite.fromTextureFeetToBodyCenter = new Vector2(0, -entry.source_rectangle.Height / 2f);
                                    sprite.body = body;
                                    body.UserData = sprite;
                                    body.Friction = 0.5f;
                                    var delta = new Vector2(0, (entry.category_array[1] == "UP" ? 1 : -1) * int.Parse(entry.category_array[2]) * 16 / 32f);
                                    var speed = 2 * 16 / 32f;
                                    var target_position = body.Position + delta;
                                    motion_laws.Add(new MotionLaw(body, body.Position, target_position, speed));
                                    (sprite as AnimatedSprite).PlayAnimation("CORPO");
                                    moving = true;
                                    break;
                                case "BASE": terra.Add(sprite); break;
                                case "TARTARUGA":
                                case "PIANTA":
                                case "CACTUS":
                                    body = new Body(world, new Vector2(((xs + dx) * 16 + entry.source_rectangle.Width / 2) / 32f, ((ys + dy) * 16 + entry.source_rectangle.Height / 2) / 32f));
                                    body.BodyType = BodyType.Dynamic;
                                    FixtureFactory.AttachRectangle(entry.source_rectangle.Width / 32f, entry.source_rectangle.Height / 32f, 0.2f, Vector2.Zero, body);
                                    sprite.fromTextureFeetToBodyCenter = Vector2.Zero;
                                    sprite.body = body;
                                    sprite.center = new Vector2(entry.source_rectangle.Width, entry.source_rectangle.Height) / 2;
                                    if (entry.source_rectangle.area() <= 16 * 16)
                                        body.OnCollision += body_blocchetto_OnCollision;
                                    body.UserData = sprite;
                                    body.Friction = 0.5f;
                                    moving = true;
                                    break;
                                case "GOLDEN":
                                case "ERBA":
                                case "FIORI":
                                    var isgolden = entry.category_array[0] == "GOLDEN";
                                    var pos = new Vector2(((xs + dx) * 16 + entry.source_rectangle.Width / 2) / 32f,
                                        ((ys + dy) * 16 + (isgolden ? 16 : entry.source_rectangle.Height / 2)) / 32f);
                                    body = new Body(world, pos, 0, sprite);
                                    body.BodyType = BodyType.Static;
                                    FixtureFactory.AttachCircle(isgolden ? 0.75f : 0.2f, 0.1f, body);
                                    sprite.body = body;
                                    sprite.fromTextureFeetToBodyCenter = new Vector2(0, -entry.source_rectangle.Height / 2f);
                                    body.Friction = 0;
                                    switch (entry.category_array[0])
                                    {
                                        // Assegnamento Collision Handler in base al tipo
                                        case "GOLDEN": body.OnCollision += body_golden_OnCollision; Globals.LevelTotalPoint += standard_gold_Golden; break;
                                        case "ERBA": body.OnCollision += body_erba_OnCollision; Globals.LevelTotalPoint += standard_gold_Erba; break;
                                        case "FIORI": body.OnCollision += body_fiori_OnCollision; Globals.LevelTotalPoint += standard_gold_Fiori; break;
                                    }
                                    break;
                                case "SPAWN": //Spawn point
                                    spawn_points.Add(new Vector2(xs + dx + 0.5f, ys + dy + 1) * 16f);
                                    break;
                            }
                            sprite.Position = new Vector2((xs + dx) * 16, (ys + dy) * 16) + sprite.center;
                            if (moving)
                                spriteset.addDynamic(sprite, layer_id, false);
                            else
                                spriteset.add(sprite, layer_id);
                            last_entry = entry;
                        }
                    }
                    chiudi_terra(terra, (xs + dx), (ys + dy), last_entry); // finita la riga
                }
            
        }
        //Collisione con il terreno
        private bool body_Linea_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData == null || !(fixtureB.Body.UserData is Player || fixtureB.Body.UserData is Zavorra))
                return false;
            return true;
        }

        //Gestione Collisioni fiori
        private bool body_fiori_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData == null || !(fixtureB.Body.UserData is Player || fixtureB.Body.UserData is Zavorra))
                return false;
            
            AudioPlayer.Play(string.Format("Gnam{0:00}", Game1.rangen.Next(3) + 1), 5);

            RemoveErba(fixtureA.Body.UserData as Sprite, fixtureA.Body);
            AddPlayerScore(fixtureB.Body.UserData, standard_gold_Fiori, true, false);
            return false;
        }

        //Gestione Collisioni Golden Grass
        private bool body_golden_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData == null || !(fixtureB.Body.UserData is Player))
                return false;
            
            AudioPlayer.Play(string.Format("Gnam{0:00}", Game1.rangen.Next(3) + 1), 5);

            RemoveErba(fixtureA.Body.UserData as Sprite, fixtureA.Body);
            AddPlayerScore(fixtureB.Body.UserData, standard_gold_Golden,false,false);

            if (fixtureB.Body.UserData != null && fixtureB.Body.UserData is Player)
                requestEndGame(); //Si passa alla schermata dei punteggi
            return false;
        }

        //Gestione Collisioni vomito
        bool body_vomit_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData == null || !(fixtureB.Body.UserData is Player || fixtureB.Body.UserData is Zavorra))
                return true;
            var data = fixtureA.Body.UserData as object[]; 
            
            if (t > (float)data[2]) //Per i primi 2 sec dalla creazione, il vomito creato non può essere mangiato
            {
                AudioPlayer.Play(string.Format("Gulp{0:00}", Game1.rangen.Next(2) + 1), 5);
                var fumo_sprite = data[1] as AnimatedSprite;
                if (!fumo_sprite.Disposed)
                {
                    spriteset.Remove(fumo_sprite);
                    fumo_sprite.Dispose();
                }
                var sprite=data[0] as Sprite;
                //elimino le leggi di movimento corrispondenti al body mangiato
                motion_laws.RemoveAll((ml) => ml.moving_body == sprite.body);
                RemoveErba(sprite, fixtureA.Body);
                AddPlayerScore(fixtureB.Body.UserData, standard_gold_Vomit,false,true);
                Globals.VomitTotalPoint -= standard_gold_Vomit;
                return false;
            }
            else
            {
                return false;
            }
        }

        //Gestione Collisioni erba
        bool body_erba_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.Body.UserData == null || !(fixtureB.Body.UserData is Player || fixtureB.Body.UserData is Zavorra))
                return false;
            AudioPlayer.Play(string.Format("Gnam{0:00}", Game1.rangen.Next(3) + 1), 5);
            RemoveErba(fixtureA.Body.UserData as Sprite, fixtureA.Body);
            AddPlayerScore(fixtureB.Body.UserData, standard_gold_Erba,true,false);
            return false;
        }

        //Gestione Collisioni Blocchi scenario(Solo Zavorra)
        bool body_blocchetto_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (!(fixtureB.Body.UserData is Zavorra))
                return true;

            AudioPlayer.Play(string.Format("Gnam{0:00}", 4), 5);
            var zavorra = (fixtureB.Body.UserData as Zavorra);
            var sprite=fixtureA.Body.UserData as Sprite;
            if (zavorra.digestione_frames_current > 0)
            {

                listaBlocchettiCibo[zavorra.index].Enqueue(new Tuple<Sprite, Body>(sprite, fixtureA.Body));
                return true;
            }

            ProcessEatBlocchetto(sprite,fixtureA.Body, zavorra);
            return false;
        }

        private void ProcessEatBlocchetto(Sprite sprite, Body body, Zavorra zavorra)
        {
            blocchiToRemove.Add(new bloccoToRemove(sprite, body, zavorra.body, TTLmax));
            zavorra.digestione_frames_current = Zavorra.digestione_frames_max;
            body.CollidesWith = Category.None;
            //fixtureA.Body.BodyType = BodyType.Kinematic;
        }

        private void RemoveBlocco(Sprite sprite, Body body)
        {
            if (!(sprite.Disposed))
            {
                spriteset.Remove(sprite);
                world.RemoveBody(body);
                sprite.Dispose();
            }
        }

        

        //Memorizza punti guadaganti
        private void AddPlayerScore(object ob, int earn,bool is_grass=false,bool is_vomit=false)
        {
            int index=-1;
            if (ob is Player)
                index=(ob as Player).index;
            else
                index=(ob as Zavorra).index;
                
            Globals.Money[index] += earn;
            if (is_grass)
                Globals.scores[index].grass_blocks_eaten += 1;
            if (is_vomit)
                Globals.scores[index].vomit_blocks_eaten += 1;

            if (ob is Zavorra)
                (ob as Zavorra).tot_point += earn;

            AddPointText(ob, earn);
        }

        private void AddPointText(object ob, int earn) //Aggiunge testo da visualizzare al MessageManager
        {
            int index=-1;

            if (earn==1)
                return;
            if (ob is Zavorra)
            {
                message_manager.texts.Add("+");
                message_manager.InitPosition.Add(new Vector2((ob as Zavorra).Position.X, (ob as Zavorra).Position.Y - 30));
                message_manager.EndPosition.Add(new Vector2((ob as Zavorra).Position.X, (ob as Zavorra).Position.Y - 90));
                message_manager.InitColor.Add(Color.Yellow);
                message_manager.EndColor.Add(Color.WhiteSmoke);
                message_manager.InitScale.Add(0.5f);
                message_manager.EndScale.Add(1.5f);
                message_manager.InitOpacity.Add(1f);
                message_manager.EndOpacity.Add(0f);
                message_manager.ActualTTL.Add(60);

                index = (ob as Zavorra).index;
            }

            if (index == -1)
            {
                if (earn > 0)
                {
                    message_manager.texts.Add("+" + earn.ToString());
                    message_manager.InitPosition.Add(new Vector2((ob as Player).Position.X, (ob as Player).Position.Y - 30));
                    message_manager.EndPosition.Add(new Vector2((ob as Player).Position.X, (ob as Player).Position.Y - 90));
                    message_manager.InitColor.Add(Color.Yellow);
                    message_manager.EndColor.Add(Color.WhiteSmoke);
                    //message_manager.Origin = -spriteset.y_offset;
                }
                else
                {
                    message_manager.texts.Add("-" + earn.ToString());
                    message_manager.InitPosition.Add(new Vector2((ob as Player).Position.X, (ob as Player).Position.Y + 30));
                    message_manager.EndPosition.Add(new Vector2((ob as Player).Position.X, (ob as Player).Position.Y + 90));
                    message_manager.InitColor.Add(Color.Red);
                    message_manager.EndColor.Add(Color.IndianRed);
                }
                message_manager.InitScale.Add(0.5f);
                message_manager.EndScale.Add(1.5f);
                message_manager.InitOpacity.Add(1f);
                message_manager.EndOpacity.Add(0f);
                message_manager.ActualTTL.Add(60);
            }
            else
            {
                message_manager.texts.Add("+" + earn.ToString());
                message_manager.InitPosition.Add(new Vector2(players[(ob as Zavorra).index].Position.X, players[(ob as Zavorra).index].Position.Y - 30));
                message_manager.EndPosition.Add(new Vector2(players[(ob as Zavorra).index].Position.X, players[(ob as Zavorra).index].Position.Y - 90));
                message_manager.InitColor.Add(Color.Yellow);
                message_manager.EndColor.Add(Color.WhiteSmoke);
                message_manager.InitScale.Add(0.5f);
                message_manager.EndScale.Add(1.5f);
                message_manager.InitOpacity.Add(1f);
                message_manager.EndOpacity.Add(0f);
                message_manager.ActualTTL.Add(60);
            }

            
        }

        //Elimina l'erba al contatto
        private void RemoveErba(Sprite sprite,Body body)
        {
            if (sprite.Disposed) return;
            spriteset.Remove(sprite);
            world.RemoveBody(body);
            sprite.Dispose();
        }

        //Fine del gioco
        private void process_end_game()
        {
            int taken_point = 0;
            for (int i = 0; i < Globals.Money.Count(); i++)
            {
                taken_point += Globals.Money[i];
            }

            ScreenManager.AddScreen(new ScoreScreen(Globals.VomitTotalPoint, players));
            ScreenManager.RemoveScreen(this);
            this.Dispose();
        }

        private void requestEndGame()
        {
            isEndGameRequested = true;
        }

        //Permette di chiudere le piattaforme da disegnare
        private void chiudi_terra(List<Sprite> terra, int x_mappa, int y_mappa, CsvEntry last_entry)
        {
            var l_mappa = terra.Count();
            if (l_mappa == 0)
                return;
            var x=(x_mappa*16)/32f;
            var y=(y_mappa*16)/32f;
            var l=(l_mappa*16)/32f;

            var body = new Body(world, new Vector2(x - l / 2, y), 0); //Creazione piano
            FixtureFactory.AttachEdge(-l / 2 * Vector2.UnitX, l / 2 * Vector2.UnitX, body); //Assegno forma lineare
            if (last_entry.category_array.Length >= 3 && (last_entry.category_array[1] == "DX" || last_entry.category_array[1] == "SX"))
            {
                body.BodyType = BodyType.Kinematic;
                var delta = new Vector2((last_entry.category_array[1] == "DX" ? 1 : -1) * int.Parse(last_entry.category_array[2]) * 16 / 32f, 0);
                var speed = 2 * 16 / 32f;
                var target_position = body.Position + delta;

                var nv2 = new Vector2(x - l / 2, y) * 32; //coordinate sprite (pixel)
                foreach (var sprite in terra)
                {
                    sprite.body = body;
                    sprite.fromTextureFeetToBodyCenter = nv2 - sprite.Position;
                }
                body.Friction = 0.5f;
                motion_laws.Add(new MotionLaw(body, body.Position, target_position, speed));
            }
            body.OnCollision += plane_OnCollision;
        }

        public static void Load_CSV() //Legge e crea strutture dati per i file CSV
        {
            const string file_name = "Content/Data/Galapagoat, tiles - {0}.csv";
            color_dictionaries = new Dictionary<Color, CsvEntry>[4];
            name_dictionaries = new Dictionary<string, CsvEntry>[4];
            for (int layer_id = 0; layer_id < 4; layer_id++)
            {
                color_dictionaries[layer_id] = new Dictionary<Color, CsvEntry>();
                name_dictionaries[layer_id] = new Dictionary<string, CsvEntry>();
                var prefix = "";
                using (var file = File.OpenRead(string.Format(file_name, layer_id))) //Apro il file CSV in lettura
                {
                    using (var sr = new StreamReader(file))
                    {
                        var csv = new CSV(sr); //In csv ci finsce tutto il contenuto del file passato
                        for (int y = 1; y < csv.sizey; y++)
                        {
                            if (csv.data[y][0].Length != 0)
                            {
                                prefix = csv.data[y][0];
                                continue;
                            }
                            var entry = new CsvEntry();
                            entry.category = prefix;
                            entry.category_array = entry.category.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            entry.name = string.Format("{0}_{1}", prefix, csv.data[y][1]);
                            entry.name_array= entry.name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            entry.source_rectangle = new Rectangle(int.Parse(csv.data[y][2]), int.Parse(csv.data[y][3]), int.Parse(csv.data[y][4]), int.Parse(csv.data[y][5]));
                            if (csv.data[y][6].Length != 0 /* Color defined */)
                            {
                                entry.color = new Color(int.Parse(csv.data[y][6]), int.Parse(csv.data[y][7]), int.Parse(csv.data[y][8]));
                                color_dictionaries[layer_id][entry.color] = entry;
                            }
                            name_dictionaries[layer_id][entry.name] = entry;
                        }
                    }
                }
            }
        }

        private void Connect_Players() //Lega 2 giocatori tramite una corda fisica o ad un peso
        {
            string name = String.Format("ROPE_SINGLE");
            ropesSprites= new Sprite[4][];
            for (int i = 0; i < players.Length / 2; i++)
            {
                if (players[i] != null)
                    if (players[i + 4] != null) //Connette i 2 players
                    {
                        a = players[i].body.Position;
                        b = players[4 + i].body.Position;

                        var joint = JointFactory.CreateRopeJoint(world, players[i].body, players[4 + i].body, new Vector2(0, 0), new Vector2(0, 0));
                        joint.CollideConnected = true;
                        CreateRopeSprite(name, i);
                    }
                    else //Connette il player ad un peso
                    {
                        a = players[i].body.Position;
                        b = players[i].body.Position+Vector2.UnitX*4;

                        add_weight(i);
                        players[i].zavorra = weights[i];
                        CreateRopeSprite(name, i);
                    }
            }
        }

        private void add_weight(int i) //Aggiunge una zavorra(solo Single PLayer)
        {
            string anim_name=String.Format("PESO_{0}",i+1);
            string frame_name = anim_name+"_STAND 1f";
            weights[i] = new Zavorra(i, new Body(world, b, 0), ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/2"), name_dictionaries[2][frame_name].source_rectangle,anim_name);
            var joint = JointFactory.CreateRopeJoint(world, players[i].body, weights[i].body, new Vector2(0, 0), new Vector2(0, -0.4f));
            joint.MaxLength = 4.8f;
            joint.CollideConnected = true;
            spriteset.addDynamic(weights[i], 2, false);
            
        }

        private void CreateRopeSprite(string name, int i) //Disegna la corda e l'aggiunge allo spriteset
        {
            ropesSprites[i] = new Sprite[ropeLimit];
            for (int j = 0; j < ropeLimit; j++) //Creazione sprite per la corda
            {
                Vector2 current_point = Vector2.Zero;
                ropesSprites[i][j] = new Sprite(ContentLoader.Load<Texture2D>("Tiles/"+scenario+"/3"), name_dictionaries[3][name].source_rectangle);
                ropesSprites[i][j].color = new Color(197, 161, 87, 255);
                spriteset.addDynamic(ropesSprites[i][j], 3,false);
            }
        }

        private void UpdateRope() //Aggiornamento gestione corda per disegno colore corda
        {
            bool changed = false;
            if (t > 0) //Calcolo del team/singolo vincente
            {
                int max_gold = int.MinValue;
                var last_richest = Globals.RichestIndex;
                Globals.RichestIndex = new List<int>() ;
                for (int i = 0; i < 4; i++)
                {
                    if (Globals.money_team(i) > max_gold)
                    {
                        Globals.RichestIndex.Clear();
                        max_gold = Globals.money_team(i);
                        Globals.RichestIndex.Add(i);
                    }
                    else if (Globals.money_team(i) == max_gold)
                    {
                        Globals.RichestIndex.Add(i);
                    }
                }
                if(Globals.RichestIndex.Count!=last_richest.Count)
                    changed=true;
                else
                {
                    for (int i = 0; i < Globals.RichestIndex.Count; i++)
                    {
                        if (Globals.RichestIndex[i] != last_richest[i])
                        {
                            changed = true;
                            break;
                        }
                    }
                }
            }
            for (int i = 0; i < players.Length / 2; i++)
            {
                if (players[i] == null)
                    continue;
                a = players[i].body.Position;
                if (!Globals.DoubleMode[i])
                    b = weights[i].body.Position - Vector2.Transform(Vector2.UnitY * 0.2f, Matrix.CreateRotationZ(weights[i].body.Rotation));
                else
                    b = players[4 + i].body.Position;
                var g = b - a;
                var l2 = l * 1.015f - (float)(Math.Sqrt(g.X * g.X + g.Y * g.Y) - Math.Abs(g.X));
                var h = (float)Math.Sqrt(l2 * l2 / 4 - g.X * g.X / 4);

                Vector2 current_point;
                for (int j = 0; j < ropeLimit; j++) //Creazione sprite per la corda
                {
                    float x = j / (float)ropeLimit;
                    current_point.X = MathHelper.Lerp(a.X, b.X, x);
                    current_point.Y = a.Y + h * (float)(Math.Sin(x * Math.PI)) + g.Y * x;
                    ropesSprites[i][j].Position = current_point * 32;
                }
                if (changed) //Colore la corda del vinvitore in maniera differente
                {
                    var color = Globals.RichestIndex.Contains(i) ? winning_team_color : normal_rope_color;
                    for (int j = 0; j < ropeLimit; j++) //Ricolora
                        ropesSprites[i][j].color = color;
                }
            }
        }

        //Controlla la collisione con il piano
        bool plane_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (fixtureB.UserData is Player)
                if ((fixtureB.UserData as Player).down_timer > 0)
                    return false;
            if ((fixtureB.Body.Position.Y + 8/32f) > fixtureA.Body.Position.Y) //La collisione viene accettata solamente se il body della capra è totalmente sopra un altro body
            {
                return false;
            }
            return true;
        }
        
        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        //Gestione dell'input
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if(input.IsNewKeyPress(Keys.D0)) //Premendo 0, attivo e disattivo la DebugView
                view_debug_data = !view_debug_data;


            if (input.IsNewKeyPress(Keys.D2)) //Premendo Invio si passa direttamente alla ScoreScene.
            {
                Vomita(players[0], players[0].dir, players[0]);
            }
            //if (input.IsNewKeyPress(Keys.Enter)) //Premendo Invio si passa direttamente alla ScoreScene.
            //{
            //    GoToScore();
            //    return;
            //}
            if (!paused)
            {
                for (int i = 0; i < 4; i++)
                {
                    var lower_edge = -spriteset.y_offset + h_f * 32+16;
                    if (Globals.Joined[i])
                    {
                        if (players[i].alive)
                        {
                            bool exit = process_input(input, i);
                            if (exit)
                                break;
                            check_exit_screen(input, i, lower_edge);
                        }
                        else
                        {
                            update_death(i);
                        }
                        update_indicators(i);
                    }
                }
            }
            base.HandleInput(input, gameTime);
        }

        //private void GoToScore() //Passa alla schermata dei punti
        //{
        //    int taken_point = 0;
        //    for (int i = 0; i < Globals.Money.Count(); i++)
        //    {
        //        taken_point += Globals.Money[i];
        //    }

        //    ScreenManager.AddScreen(new ScoreScreen((int) (Globals.VomitTotalPoint), players));
        //    ScreenManager.RemoveScreen(this);
        //    return;
        //}

        private void update_indicators(int i) //Aggiorna indicatori di posizione fuori dallo schermo
        {
            if (players[i].alive)
            {
                bool clamping_required;
                indicators[i].Position = ClampToScreen(players[i].Position, out clamping_required);
                indicators[i].alpha = clamping_required ? 1f : 0f;
                players[i].time_from_last_death += 1;
                if (Globals.DoubleMode[i])
                {
                    players[i+4].time_from_last_death += 1;
                    indicators[i + 4].Position = ClampToScreen(players[i+4].Position, out clamping_required);
                    indicators[i + 4].alpha = clamping_required ? 1f : 0f;
                }
            }
            else
            {
                indicators[i].Position = players[i].death_indicator_pos();
                indicators[i].alpha = 1;
                if (Globals.DoubleMode[i])
                {
                    indicators[i + 4].Position = players[i + 4].death_indicator_pos();
                    indicators[i + 4].alpha = 1;
                }
            }
        }

        private void check_exit_screen(InputHelper input, int i, float lower_edge) //Controllo dell'uscita fuori dallo schermo
        {
            if (Globals.DoubleMode[i])
            {                
                players[i].HandleInputSplit(input, false);
                players[i + 4].HandleInputSplit(input, true);
                if (players[i].Position.Y > lower_edge)
                {
                    if (!players[i].drowning)
                        AudioPlayer.Play("Splash", 60);
                    else
                        AudioPlayer.Play("Drown", 87);
                    players[i].drowning = true;
                }
                else
                {
                    players[i].drowning = false;
                }
                if (players[i + 4].Position.Y > lower_edge)
                {
                    if (!players[i + 4].drowning)
                        AudioPlayer.Play("Splash", 60);
                    else
                        AudioPlayer.Play("Drown", 87);
                    players[i+4].drowning = true;
                }
                else
                    players[i + 4].drowning = false;

                if (players[i].drowning && players[i + 4].drowning)
                {
                    players[i].alive = false;
                    players[i].drowning = false;
                    players[i + 4].alive = false;
                    players[i+4].drowning = false;
                }
                players[i].over_the_top = players[i].Position.Y < -spriteset.y_offset + 64;
                players[i + 4].over_the_top = players[i + 4].Position.Y < -spriteset.y_offset + 64;
            }
            else
            {
                players[i].HandleInput(input);
                if (players[i].Position.Y > lower_edge)
                {
                    AudioPlayer.Play("Splash", 60);
                    AudioPlayer.Play("Drown", 87);
                    players[i].drowning = false;
                    players[i].alive = false;
                }
                if(weights[i].Position.Y > lower_edge)
                {
                    if (!weights_drowning[i])
                    {
                        AudioPlayer.Play("Splash", 60);
                        weights_drowning[i] = true;
                    }
                }
                players[i].over_the_top = players[i].Position.Y < -spriteset.y_offset+64;
            }
        }

        private void update_death(int i) //Aggiorna punti di ReSpawn
        {
            if (Globals.DoubleMode[i])
            {
                var p1=players[i];
                var p2=players[i+4];
                if (p1.respawn_position == null)
                    p1.respawn_position = GetSpawn();
                if (p2.respawn_position == null && p1.respawn_position.HasValue)
                    p2.respawn_position = p1.respawn_position + Vector2.UnitX * 32;
                if (p1.respawn_position.HasValue && p2.respawn_position.HasValue)
                    if (p1.spawn_timer <= 0 && p2.spawn_timer <= 0)
                        doRespawn(i);
            }
            else
            {
                if (players[i].respawn_position == null)
                    players[i].respawn_position = GetSpawn();
                if (players[i].respawn_position.HasValue)
                    if (players[i].spawn_timer <= 0)
                        doRespawn(i);
            }
        }

        private Vector2 ClampToScreen(Vector2 pos, out bool clamping_required)
        {
            Vector2 newv = new Vector2(Mathf.Clamp(pos.X, 16, (w_f*2-1) * 16),Mathf.Clamp(pos.Y, -spriteset.y_offset+48, h_f * 32-spriteset.y_offset));
            clamping_required = (pos.X != newv.X || pos.Y != newv.Y);
            return newv;
        }

        private bool process_input(InputHelper input, int i) //Controllo input
        {

            input.selected_index = i;
            bool exit = false;
#if WINDOWS
            if (InputHelper.keyboard_index == i)
            {
                input.keyboard_split_mode = Globals.DoubleMode[i];

                if (input.IsNewButtonPress(Buttons.Back))
                {
                    ScreenManager.AddScreen(new Scene_Pause(i, this));
                    exit = true;
                }
            }
            else
#endif
            {
                if (input.IsNewButtonPress(Buttons.Start))
                {
                    ScreenManager.AddScreen(new Scene_Pause(i, this));
                    exit = true;
                }
            }
            return exit;
        }
        internal void doRespawn(int i)
        {
            if (Globals.DoubleMode[i])
            {
                var pos1 = players[i].respawn_position;
                var pos2 = players[i+4].respawn_position;
                if (pos1.HasValue && pos2.HasValue)
                {
                    players[i].Position = pos1.Value;
                    players[i].alive = true;
                    players[i].respawn_position = null;

                    var punti_persi=calcolaPuntiPersi(players[i]);
                    AddPlayerScore(players[i], -punti_persi);

                    Globals.VomitTotalPoint += punti_persi;

                    players[i + 4].Position = pos2.Value + Vector2.UnitX * 48;
                    players[i + 4].alive = true;
                    players[i + 4].respawn_position = null;
                    AddPlayerScore(players[i + 4], -calcolaPuntiPersi(players[i + 4]));
                    Globals.VomitTotalPoint += punti_persi;
                }
            }
            else
            {

                var pos = players[i].respawn_position;
                if (pos.HasValue)
                {
                    players[i].Position = pos.Value;
                    weights[i].Position = pos.Value + Vector2.UnitX * 48;
                    players[i].alive = true;
                    players[i].respawn_position = null;
                    var punti_persi = calcolaPuntiPersi(players[i]);
                    AddPlayerScore(players[i], -punti_persi);
                    Globals.VomitTotalPoint += punti_persi;
                }
            }
        }

        private Vector2? GetSpawn() //Controllo dei Spawn point
        {
            var lower_bound=h_f*32f-spriteset.y_offset;
            var higher_bound=-spriteset.y_offset;
            var available_spawns = spawn_points.Where((v) => v.Y > higher_bound && v.Y < lower_bound && !hasPlayersInRange(v)).ToList();           
            if(available_spawns.Count>0)
                return available_spawns[0];
            else 
                return null;
        }

        private bool hasPlayersInRange(Vector2 v)
        {
            return players.Any((p) => {
                return (p != null &&
                    ((p.respawn_position.HasValue ? p.respawn_position.Value:p.Position) - v).Length() < spawn_range);
            });
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            if (paused)
                return;         

            var slowmo = 1;
            var dt = 1 / (60f * slowmo);
            foreach (var mp in motion_laws)
                mp.Update(dt);
            world.Step(dt); //Equivale alla chiamata dell'update, ma per il world

            UpdateMangiaBlocchetti();
            UpdateRope();
            spriteset.Update();
            message_manager.Update();
            UpdateCamera(gameTime);
            if (isEndGameRequested)
            {
                isEndGameRequested = false;
                process_end_game();
            }
        }

        private void UpdateMangiaBlocchetti() //Permette al caprone di mangiare oggetti dello scenario
        {
            foreach (var zavorra in weights)
            {
                if(zavorra==null)
                    continue;
                var i = zavorra.index;

                while (zavorra.digestione_frames_current == 0 && listaBlocchettiCibo[i].Count > 0)
                {
                    var spriteBody = listaBlocchettiCibo[i].Dequeue();
                    if (spriteBody.Item1.Disposed)
                        continue;
                    if (blocchiToRemove.Any((b) => b.bodyR == spriteBody.Item2))
                        continue;
                    if ((spriteBody.Item1.Position - zavorra.Position).Length() / 32f < 1.2f)
                    {
                        ProcessEatBlocchetto(spriteBody.Item1, spriteBody.Item2, zavorra);
                        break;
                    }
                }
            }

            List<int> indexToRemove = new List<int>();
            for (int i = 0; i < blocchiToRemove.Count();i++ )
            {
                blocchiToRemove[i].bodyR.Position = Vector2.Lerp(blocchiToRemove[i].bodyR.Position, blocchiToRemove[i].Caprone.Position, (TTLmax - blocchiToRemove[i].TTLR) / (float)TTLmax);
                blocchiToRemove[i].TTLR--;
                if (blocchiToRemove[i].TTLR <= 0)
                {
                    indexToRemove.Add(i);
                }
            }
            for (int i = 0; i < indexToRemove.Count(); i++)
            {
                AddPlayerScore(blocchiToRemove[i].Caprone.UserData, standard_gold_Blocchetto, true, false);
                RemoveBlocco(blocchiToRemove[i].spriteR, blocchiToRemove[i].bodyR);
                blocchiToRemove.RemoveAt(i);
            }
        }

        private void UpdateCamera(GameTime time) //Permette lo spostamento/stop della camera verso l'alto
        {
#if DEBUG
            int mult = Game1.Input.IsKeyPress(Keys.Add)?50:1;
            
#else   
            int mult = 1;
#endif
             t+=(float)time.ElapsedGameTime.TotalSeconds*mult;

            if (t < t1)
            {
                skyColor = new Color(Vector3.Lerp(sky_colors[0],sky_colors[1], t / t1) / 255f);
                gradient.color = new Color(Vector3.Lerp(gradient_colors[0],gradient_colors[1] , t / t1) / 255f);
                spriteset.subtract_color = new Color(Vector3.Lerp(subtract_colors[0], subtract_colors[1], t / t1) / 255f);
                spriteset.additive_color = new Color(Vector3.Lerp(additive_colors[0], additive_colors[1], t / t1) / 255f);
            }
            else if(t<t3)
            {
                var f=(t - t1) / (t3 - t1);
                skyColor = new Color(Vector3.Lerp(sky_colors[1],sky_colors[2] , f) / 255f);
                gradient.color = new Color(Vector3.Lerp(gradient_colors[1], gradient_colors[2], f) / 255f);
                spriteset.subtract_color = new Color(Vector3.Lerp(subtract_colors[1], subtract_colors[2], f) / 255f);
                spriteset.additive_color = new Color(Vector3.Lerp(additive_colors[1], additive_colors[2], f) / 255f);

                if (t > t2)
                {
                    star_alpha = MathHelper.Lerp(0, 1f, (t - t2) / (t3 - t2));
                    //gradient.alpha = MathHelper.Lerp(0.6f, 0.25f, (t - t2) / (t3 - t2));
                }
            }

            gradient.Position = new Vector2(w_f / 2 * 32, h_f * 32 - (int)spriteset.y_offset);
            float y_speed = level_height * 16/t3;
            if (t>0 && t<t3)
            {   //La telecamera inizia a muoversi
                //Inposta i limiti della vista per sapere cosa disegnare
                spriteset.y_offset = MathHelper.Lerp(0, level_height * 16, t / t3);
                spriteset.startDrawContol = true;
                spriteset.camera_start_index = -spriteset.y_offset + 720;
                spriteset.camera_end_index = -spriteset.y_offset;

                lineaBloccoAlto.Position = new Vector2(lineaBloccoAlto.Position.X, - (spriteset.y_offset+70) / 32);
            }

            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].Position = star_pos[i]-spriteset.y_offset*Vector2.UnitY;
                stars[i].alpha = star_alpha;
            }

            for (int i = 0; i < 3; i++)
            {

                int delta_x = -(int)(20 * (1 + Math.Cos((t * 0.05f * y_speed + i / 2f) * MathHelper.Pi)));
                int delta_y = (i==0?12:i ==1 ? 20 : 12) - (int)(4 * (1 + Math.Cos((t * 0.125f * y_speed + i / 2f) * MathHelper.Pi)));
                for (int j = 0; j < 2; j++)
                {
                    waves[i][j].Position = new Vector2(waves[i][j].center.X + waves[i][j].width * j + delta_x,
                        h_f * 32 + 60 * (Math.Max(0,-t) / t0) - (int)spriteset.y_offset + delta_y);
                }
                
                delta_x = -(int)(20 * (1 + Math.Cos((t * 0.3f  + i / 2f) * MathHelper.Pi)));
                delta_y = - (int)(8 * (1 + Math.Cos((t * 0.2f +i / 2f+0.25f) * MathHelper.Pi)));
                for (int j = 0; j < 2; j++)
                {
                    clouds[i][j].Position = new Vector2(clouds[i][j].center.X + clouds[i][j].width * j +delta_x,
                        clouds[i][j].center.Y - (int)spriteset.y_offset+delta_y);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Game1.device.Clear(skyColor);
            projection = Matrix.CreateOrthographicOffCenter(0, w_f, h_f - (int)spriteset.y_offset / 32f, -(int)spriteset.y_offset / 32f, -10, 10); //faccio scorrere la matrice di proiezione per spostare in alto lo schermo


            //Sprite.color=tintColor;
            spriteset.Draw(); //Disegno tutti gli sprite
            if(view_debug_data)
            view.RenderDebugData(ref projection); //Render della vista del mondo
            base.Draw(gameTime); 
            DrawScores();
        }

        private void DrawScores()
        {
#if DEBUG
            //Sprite.batch.Begin();
            //for (int i = 0; i < 8; i++)
            //{
            //    if (!Globals.Joined[i % 4])
            //        continue;
            //    float posx = ((w_f * 32f - 24) / 8) * i + 12;
            //    float posy = 12;
            //    Sprite.batch.DrawString(view._font, Globals.scores[i].ToString(), new Vector2(posx - 1, posy - 1), Color.Black);
            //    Sprite.batch.DrawString(view._font, Globals.scores[i].ToString(), new Vector2(posx - 1, posy + 1), Color.Black);
            //    Sprite.batch.DrawString(view._font, Globals.scores[i].ToString(), new Vector2(posx + 1, posy - 1), Color.Black);
            //    Sprite.batch.DrawString(view._font, Globals.scores[i].ToString(), new Vector2(posx + 1, posy + 1), Color.Black);
            //    Sprite.batch.DrawString(view._font, Globals.scores[i].ToString(), new Vector2(posx, posy), Color.White);
            //}
            //Sprite.batch.End();
#endif
            //Disegno numeri
            Sprite.batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, spriteset.viewMatrix);
            message_manager.Draw(Sprite.batch);
            Sprite.batch.End();
        }

        internal void unpause()
        {
            paused = false;
        }

        internal void pause()
        {
            paused = true;
        }

        public void Dispose()
        {
            foreach (var player in players)
            {
                if (player != null)
                    player.dispose();
            }
        }
    }
}
