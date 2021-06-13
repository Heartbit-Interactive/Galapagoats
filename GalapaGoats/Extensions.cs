using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalapaGoats
{
    public static class Extensions
    {
        public static int area(this Microsoft.Xna.Framework.Rectangle rect)
        {
            return rect.Width * rect.Height;
        }
    }
}
