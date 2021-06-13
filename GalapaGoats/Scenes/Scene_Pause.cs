using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace GalapaGoats.Scene
{
    class Scene_Pause:GameScreen
    {
        int caller_index;
        public Scene_Pause(int caller_index, Scene_Game caller)
            : base()
        {
            this.caller_index = caller_index;
            this.caller_screen = caller;
            caller_screen.pause();
            this.IsPopup = true;
            mono = ContentLoader.Load<Texture2D>("Hud/mono");
            if (MediaPlayer.State == MediaState.Playing) 
                MediaPlayer.Pause();
        }
        Rectangle screen_rect = new Rectangle(0, 0, 1280, 720);
        Color screen_color = new Color(0,0,0,168);
        Vector2 center = new Vector2(640, 360);
        private Scene_Game caller_screen;
        private Texture2D mono;
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            ScreenManager.SpriteBatch.Draw(mono, screen_rect, screen_color);
#if WINDOWS
            if(caller_index==InputHelper.keyboard_index)
            {
                draw_string("PRESS ESC TO RESUME", center - Vector2.UnitY * 96, 2, Globals.colors[caller_index]);
                draw_string("PRESS ENTER TO RETURN TO THE LOBBY ", center + Vector2.UnitY * 0, 2, Globals.colors[caller_index]);
                draw_string("PRESS ALT+F4 TO CLOSE GAME ", center + Vector2.UnitY * 96, 2, Globals.colors[caller_index]);
            }
            else
#endif
            {
                draw_string("PRESS START TO RESUME", center - Vector2.UnitY * 96, 2, Globals.colors[caller_index]);
                draw_string("PRESS BACK TO RETURN TO THE LOBBY ", center + Vector2.UnitY * 32, 2, Globals.colors[caller_index]);
            }
            ScreenManager.SpriteBatch.End();
            base.Draw(gameTime);
        }
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            input.selected_index = caller_index;

#if WINDOWS

            if (caller_index == InputHelper.keyboard_index)
            {
                if (input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.Start))
                {
                    this.ExitScreen();
                    caller_screen.ExitScreen();
                    ScreenManager.AddScreen(new Scene_Lobby(0));
                }
                if (input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.Back))
                {
                    this.ExitScreen();
                    if (MediaPlayer.State == MediaState.Paused)
                        MediaPlayer.Resume();
                    caller_screen.unpause();
                }
            }
            else
#endif
            {
                if (input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.Back))
                {
                    this.ExitScreen();
                    caller_screen.ExitScreen();
                    ScreenManager.AddScreen(new Scene_Start());
                }
                if (input.IsNewButtonPress(Microsoft.Xna.Framework.Input.Buttons.Start))
                {
                    this.ExitScreen();
                    if (MediaPlayer.State == MediaState.Paused)
                        MediaPlayer.Resume();
                    caller_screen.unpause();
                }
            }
            base.HandleInput(input, gameTime);
        }
    }
}