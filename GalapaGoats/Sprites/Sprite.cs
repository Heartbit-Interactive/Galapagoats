using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GalapaGoats.Sprites
{
    public class Sprite
    {
        public Texture2D texture;
        Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set // in tex coords
            {
                _position = value;
                if (body != null)
                    body.Position = (_position + fromTextureFeetToBodyCenter) / 32f;
            }
        }

        public FarseerPhysics.Dynamics.Body body;

        private Rectangle _source_Rect;
        private Vector2 _center;
        public Vector2 center { get { return _center; } set { _center = value; } }
        public Rectangle Source_Rect { 
            get { return _source_Rect; } 
            set { _source_Rect = value; 
                if(updateCenter)
                _center = new Vector2(_source_Rect.Width / 2, _source_Rect.Height); 
            } 
        }

        public static SpriteBatch batch;
        public SpriteEffects SpriteEffect = SpriteEffects.None; //Usato per il flip delle texture
        public Vector2 fromTextureFeetToBodyCenter;
        public bool Disposed;
        public Color color { get { return _color; } set { _color = value; draw_color = new Color(_color.ToVector4() * _alpha); } }
        Color draw_color = Color.White;
        public Vector2 scale=Vector2.One;
        private float _alpha=1;
        private Color _color=Color.White;
        private float _rotation;
        public int layer;
        public bool dynamic;
        public bool updateCenter = true;
        public float alpha { get { return _alpha; } set { _alpha = value; draw_color = new Color(_color.ToVector4() * _alpha); } }
        public int is_eaten = 0;
        public bool lockRotation = false;
        

        public Sprite()
        {
        }

        public Sprite(Texture2D Text, Rectangle? Rect)
        {
            texture = Text;
            if (Rect.HasValue)
                Source_Rect = Rect.Value;
            else
                Source_Rect = Text.Bounds;
            
        }

        public virtual void Update()
        {
            if (body == null)
                return;
            //spostamento del body per posizionarlo nella giusta posizione della texture(1 unità World Fisico = 32 Pixel dello schermo)
            _position = body.Position * 32 - fromTextureFeetToBodyCenter;
            if (lockRotation)
                return;
            _rotation = body.Rotation;
        }

        public virtual void Draw()
        {
            //batch.Draw(texture, Position, Source_Rect, Color.White, body != null ? body.Rotation : 0, _center, 1f, SpriteEffect, 0);
                batch.Draw(texture, _position, Source_Rect, draw_color, _rotation, _center, scale, SpriteEffect, 0);
            
        }

        internal void Dispose()
        {
            Disposed = true;
        }

        public int width { get { return Source_Rect.Width; } }


        public class SpriteComparer : IComparer<Sprite>
        {
            public int Compare(Sprite x, Sprite y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        // If x is null and y is null, they're 
                        // equal.  
                        return 0;
                    }
                    else
                    {
                        // If x is null and y is not null, y 
                        // is greater.  
                        return 1;
                    }
                }
                else
                {
                    // If x is not null... 
                    // 
                    if (y == null)
                    // ...and y is null, x is greater.
                    {
                        return -1;
                    }
                    else
                    {
                        // ...and y is not null, compare the  
                        // Position.Y of the two Sprites. 
                        // 
                        int retval = y.Position.Y.CompareTo(x.Position.Y);
                        return retval;

                    }
                }
            }
        }


    }
}
