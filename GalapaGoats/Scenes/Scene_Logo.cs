using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GalapaGoats.Scene
{
    public class Scene_Logo : GameScreen
    {
        //private Rectangle _destination;
        private TimeSpan _duration;
        private Texture2D _farseerLogoTexture;

        public Scene_Logo(TimeSpan duration):base()
        {
            _duration = duration;
            TransitionOffTime = TimeSpan.FromSeconds(2.0);
        }

        public override void LoadContent()
        {
            _farseerLogoTexture = ContentLoader.Load<Texture2D>("logo");
        }

        public override void UnloadContent()
        {
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.GamePadState.IsButtonDown(Buttons.A | Buttons.Start | Buttons.Back))
            {
                _duration = TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            _duration -= gameTime.ElapsedGameTime;
            if (_duration <= TimeSpan.Zero)
            {
                ExitScreen();
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.White);
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.Draw(_farseerLogoTexture, Vector2.Zero, Color.White);
            ScreenManager.SpriteBatch.End();
        }
    }
}