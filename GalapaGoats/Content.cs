using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalapaGoats
{
    static class ContentLoader
    {
        public static Microsoft.Xna.Framework.Content.ContentManager content;
        public static T Load<T>(string filename)
        {
            return content.Load<T>(filename);
        }
    }
}
