using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalapaGoats.Sprites
{
    class SpritesetGame
    {
        public float camera_start_index = 720;
        public float camera_end_index = 0;
        public bool startDrawContol = false;
        int[] drawLimit;

        public List<Sprite>[] Layers; //Gestione layer grafici
        public List<Sprite>[][] DynamicLayers; //Gestione layer grafici, solo elementi dinamici
        public Matrix viewMatrix;
        public float y_offset;
        private Texture2D monoChrome;
        public Color additive_color = Color.Black;
        public Color subtract_color = Color.Blue;
        BlendState subtractiveBlend;
        
        private  int ok=0;


        public SpritesetGame(Matrix proj)
        {
            this.viewMatrix = proj;
            Layers = new List<Sprite>[4];
            DynamicLayers = new List<Sprite>[4][];
            for (int i = 0; i < Layers.Length; i++)
            { 
                Layers[i] = new List<Sprite>();
                DynamicLayers[i]=new List<Sprite>[2];
                for (int j = 0; j < 2; j++)
                    DynamicLayers[i][j] = new List<Sprite>();
            }
            monoChrome = ContentLoader.Load<Texture2D>("Hud/mono");
            subtractiveBlend = new BlendState();
            subtractiveBlend.AlphaBlendFunction = BlendFunction.ReverseSubtract;
            subtractiveBlend.AlphaDestinationBlend = subtractiveBlend.AlphaSourceBlend = Blend.One;
            subtractiveBlend.ColorBlendFunction = BlendFunction.ReverseSubtract;
            subtractiveBlend.ColorDestinationBlend = subtractiveBlend.ColorSourceBlend = Blend.One;

            drawLimit= new int[8];
        }

        public void Update()
        {
            int i, j;

            ////Ordinamento ListeLivello Spriteset
            //Sprite.SpriteComparer cp = new Sprite.SpriteComparer();

            for (i = 0; i < 4; i++)
                if (Layers[i].Count > 0)
                {
                    drawLimit[i + 4] = Math.Min(Layers[i].Count-1,drawLimit[i+4]);
                    for (j = drawLimit[i]; j <= drawLimit[i + 4]; j++)
                        Layers[i][j].Update();
                }

            foreach (var layer in DynamicLayers) //Aggiornamento layer dinamici
                foreach (var list in layer)
                    foreach (var sprite in list)
                        sprite.Update();

            i = 0; //mem Livello
            foreach (var layer in Layers) //Disegna solamente gli sprite dentro la cameraView
            {   
                j = drawLimit[i]; //mem indici disegno
                int j2 = drawLimit[i+4];
                
                ok=0;
                //sposto il limite inferiore
                while (ok != 2)
                {
                    if (j < layer.Count)
                    {
                        if (((layer[j].Position.Y - layer[j].center.Y) <= camera_start_index) && (ok == 0)) //Spostamento index iniziale
                        {
                            drawLimit[i] = j;
                            ok++;
                        }

                        else if (((layer[j].Position.Y - layer[j].center.Y + layer[j].Source_Rect.Height) < camera_end_index) && (ok == 1)) //Spostamento index finale
                        {
                            drawLimit[i + 4] = j;
                            ok++;
                        }
                    }
                    else
                    {
                        drawLimit[i + 4] = layer.Count - 1;
                        ok = 2;
                        break;
                    }

                    j += 1;
                    j2 += 1;
                }
                i += 1;    
            }
        }

        public void Draw()
        {
            viewMatrix = Matrix.CreateTranslation(new Vector3(0, (int)y_offset, 0));
            Sprite.batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, viewMatrix);

            for (int i = 0; i < 4; i++)
            {
                foreach (var sprite in DynamicLayers[i][0]) // Disegna tutti gli sprites nel layer dinamico dietro/prima al layer corrente
                    sprite.Draw();
                if (Layers[i].Count > 0)
                for (int j = drawLimit[i]; j <= drawLimit[i + 4]; j++)
                    (Layers.ElementAt(i))[j].Draw();
                
                foreach (var sprite in DynamicLayers[i][1]) // Disegna tutti gli sprites nel layer dinamico davanti/dopo il layer corrente
                    sprite.Draw();
            }

            Sprite.batch.End(); //Disegna la lista dei sprite alla chiamate dell'end
            Sprite.batch.Begin(SpriteSortMode.Immediate, subtractiveBlend);
            Sprite.batch.Draw(monoChrome, new Rectangle(0, 0, 1280, 720), subtract_color);
            Sprite.batch.End();
            Sprite.batch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            Sprite.batch.Draw(monoChrome, new Rectangle(0, 0, 1280, 720), additive_color);
            Sprite.batch.End();
        }


        internal void add(Sprite sprite, int layer)
        {
            Layers[layer].Add(sprite); //Caricamento della sprite sul layer statico, davanti a quello precedente
            sprite.layer = layer;
        }

        internal void addDynamic(Sprite sprite, int layer, bool before/* before = sprite disegnata Dietro o Davanti al layer corrente*/)
        {
            DynamicLayers[layer][before ? 0 : 1].Add(sprite); // Aggiungo uno sprite dinamico (che viene aggiornato e disegnato ad ogni frame)
            sprite.layer = layer;
            sprite.dynamic = true;
        }

        internal void Remove(Sprite sprite)
        {
            if (sprite.dynamic) //Rimozione Sprite Dynamic
            {
                DynamicLayers[sprite.layer][0].Remove(sprite);
                DynamicLayers[sprite.layer][1].Remove(sprite);
            }
            else //Rimozione Sprite
            {
                int index=Layers[sprite.layer].FindIndex(s=> s==sprite);
                if (drawLimit[sprite.layer] > index)
                    drawLimit[sprite.layer]--;
                if (drawLimit[sprite.layer+4]>index)
                    drawLimit[sprite.layer+4]--;
                    
                Layers[sprite.layer].Remove(sprite);
            }
        }
    }
}
