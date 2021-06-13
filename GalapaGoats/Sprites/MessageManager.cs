using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalapaGoats
{
    public class MessageManager
    {
        public List<string> texts;
        public List<Vector2> InitPosition;
        public List<Vector2> EndPosition;
        public List<float> InitScale;
        public List<float> EndScale;
        public List<float> InitOpacity;
        public List<float> EndOpacity;
        public List<int> ActualTTL;
        public List<Color> InitColor;
        public List<Color> EndColor;

        private int TTL;
        public float Origin = 0;
        

        public MessageManager()
        {
            texts=new List<string>();
            InitPosition=new List<Vector2>();
            EndPosition = new List<Vector2>();
            InitScale = new List<float>();
            EndScale = new List<float>();
            InitOpacity = new List<float>();
            EndOpacity = new List<float>();
            ActualTTL = new List<int>();
            InitColor = new List<Color>();
            EndColor = new List<Color>();

            TTL = 60;
        }
 
        public void Update()
        {
            for (int i = 0; i < texts.Count; i++)
            {
                if (ActualTTL[i] <= 0)
                {
                    texts.RemoveAt(i);
                    InitPosition.RemoveAt(i);
                    EndPosition.RemoveAt(i);
                    InitScale.RemoveAt(i);
                    EndScale.RemoveAt(i);
                    InitOpacity.RemoveAt(i);
                    EndOpacity.RemoveAt(i);
                    ActualTTL.RemoveAt(i);
                    InitColor.RemoveAt(i);
                    EndColor.RemoveAt(i);
                }
                else
                    ActualTTL[i] -= 1;
            }
        }
 
        
        public void Draw(SpriteBatch batch)
        {
            for (int i = 0; i < texts.Count; i++)
            {
                var f = (TTL - ActualTTL[i]) / (float)TTL;
                var ActualPos=Vector2.Lerp(InitPosition[i],EndPosition[i], f);
                var ActualScale=MathHelper.Lerp(InitScale[i],EndScale[i],f);
                var ActualOpacity=MathHelper.Lerp(InitOpacity[i],EndOpacity[i],f);
                var color = Color.Lerp(InitColor[i], EndColor[i], f) * ActualOpacity;

                batch.DrawString(Game1.font, texts[i], ActualPos, color, 0f, new Vector2(Game1.font.MeasureString(texts[i]).X / 2, 0), ActualScale, SpriteEffects.None, 0);
            }
        }

    }
}
