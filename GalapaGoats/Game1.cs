using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using GalapaGoats.Scene;

using FarseerPhysics;
using FarseerPhysics.Dynamics;

using System.IO;

namespace GalapaGoats
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static InputHelper Input; //Whole input management
        public static Random rangen;
        public static SpriteFont font; 
        public static GraphicsDevice device; //Refer to display

        public GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {            
            graphics = new GraphicsDeviceManager(this); //Assegna lo schermo attuale come Graphic Device
            Content.RootDirectory = "Content"; //Percorso Contenuti
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            //this.IsFixedTimeStep=false;
            //graphics.IsFullScreen = true;
            //graphics.ApplyChanges();
        }
        protected override void Initialize()
        {            
            base.Initialize();            
        }
        protected override void LoadContent()
        {
            device = GraphicsDevice;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            GalapaGoats.Sprites.Sprite.batch = spriteBatch;

            ContentLoader.content = Content;
            rangen = new Random(DateTime.Now.Millisecond);
            Input = new InputHelper();
            font = Content.Load<SpriteFont>("Hud/font");
            Globals.vocabs = new Vocabs();
            ScreenManager manager = new ScreenManager(this);
            manager.Initialize();
            Components.Add(manager);
            Scene_Game.Load_CSV();
            GameScreen scene = new Scene_Start(); //Create and Add scene to SceneManager
            manager.AddScreen(scene);

            scene = new Scene_Logo(TimeSpan.FromSeconds(3)); //Starter Scene
            manager.AddScreen(scene);
        }
        protected override void Update(GameTime gameTime)
        {
            AudioPlayer.Update();
            base.Update(gameTime);
        }
    }
}
