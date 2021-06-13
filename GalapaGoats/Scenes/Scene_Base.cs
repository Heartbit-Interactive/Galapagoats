using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalapaGoats.Scene
{
    public class Scene_Base : GameScreen
    {
        protected Microsoft.Xna.Framework.Graphics.GraphicsDevice GraphicsDevice;

        public Scene_Base(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            // TODO: Complete member initialization
            this.GraphicsDevice = device;
        }
    }
}
