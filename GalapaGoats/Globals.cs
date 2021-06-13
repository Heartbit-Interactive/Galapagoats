using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using System.Resources;
namespace GalapaGoats
{
    public enum MediaMode { Manual, Auto }
    public enum Language { English, German }
    public class Vocabs
    {
        readonly string[,] _data;

        public Vocabs()
        {
            var resourceManager = new ResourceManager ("GalapaGoatsOGL.Vocab", typeof(Vocabs).Assembly);
            _data = new string[78, 1];
            for (int i = 0; i < 78; i++)
                _data[i,0] = resourceManager.GetString(string.Format("Vocab_{0:00}", i));
        }

        public string this[int index]
        {
            get
            {
                return _data[index,(int)Globals.language];
            }
        }
    }
    static class Globals
    {
        public static Vocabs vocabs;
        public static Language language;
        public static MediaMode media_mode = MediaMode.Auto;
        public static MediaLibrary media = new MediaLibrary();
        public static PlayerScore[] scores = new PlayerScore[8];
        public static List<Vector2> timer_positions=new List<Vector2>();
        public static Color[] legend_data;
        public static int max_players=8;

        public static int coins_added = 0;
        public static string[] playernames = { "BLUE", "YELLOW", "RED", "DARK GREEN", "LIGHT BLUE", "ORANGE", "PINK", "GREEN", "WHITE" };
        public static Color[] colors ={
            new Color(127, 134, 170, 255),
            new Color(235, 227, 166, 255),
            new Color(185, 80, 80, 255),
            new Color(114, 169, 76, 255),
                        
            new Color(182, 214, 219, 255),
            new Color(211, 170, 46, 255),
            new Color(234, 159, 192, 255),
            new Color(111, 206, 64, 255),

            new Color(255, 255, 255, 255)};

        public static int[] Money;
        public static bool[] Connected = new bool[4];
        public static bool[] Joined = new bool[4];
        public static bool[] Ready = new bool[4];
        public static bool[] DoubleMode = new bool[5];
        public static int[] FavLevel = new int[4];

        public static int[] Victories = new int[8];
        public static int[] VictoryStreak = new int[8];
        public static List<int> RichestIndex = new List<int>();

        public static List<int> PlayerIndex;
        
        public static bool IsDemo;

        public static int LevelTotalPoint;
        public static int VomitTotalPoint;
        public static int arena=0;


        internal static int money_team(int i)
        {
            return Money[i] + Money[i + 4];
        }
    }

//-----------------------------------------------------


}
