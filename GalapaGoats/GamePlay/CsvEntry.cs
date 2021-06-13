using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GalapaGoats
{
    class CsvEntry //Utilizzata x traduzione singola linea(creazione dizionario) dei csv
    {
        public string category = "";
        public string name = "";
        public string[] name_array;
        public Rectangle source_rectangle = new Rectangle();
        public Color color = new Color();
        public string[] category_array;
    }
}
