using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM
{
    public class RectangleOverlay
    {
        SpriteBatch spriteBatch;
        Texture2D dummyTexture;
        Rectangle bounds;
        public Rectangle Bounds
        {
            get { return bounds; }
        }

        private bool isWireFrame;
        private Vector2 position;

        Color color;

        public RectangleOverlay(Rectangle rect, Color colori)
        {
            bounds = rect;
            color = colori;
        }

        public void setWidth(int width)
        {
            bounds.Width = width;
        }

        public int getWidth() { return bounds.Width; }

        public int getHeight() { return bounds.Height; }

        public void setHeight(int height)
        {
            bounds.Height = height;
        }

        public void setPosition(Vector2 position)
        {
            bounds.X = (int)position.X;
            bounds.Y = (int)position.Y;

            this.position = position;
        }

        public void setPosition(Point p)
        {
            bounds.X = p.X;
            bounds.Y = p.Y;

            position.X = p.X;
            position.Y = p.Y;
        }

        public void setPosition(int x, int y)
        {
            bounds.X = x;
            bounds.Y = y;

            position.X = x;
            position.Y = y;
        }

        public void setPosition(float x, float y)
        {
            bounds.X = (int)x;
            bounds.Y = (int)y;

            position.X = x;
            position.Y = y;
        }

        public int getX() { return this.bounds.X; }
        public int getY() { return this.bounds.Y; }

        public void setColor(Color c)
        {
            this.color = c;
        }

        public void setColor(byte r, byte g, byte b, byte a)
        {
            this.color.R = r;
            this.color.G = g;
            this.color.B = b;
            this.color.A = a;
        }

        public void loadContent(GraphicsDevice g)
        {
            spriteBatch = new SpriteBatch(g);
            dummyTexture = new Texture2D(g, 1, 1);
            dummyTexture.SetData(new Color[] { Color.White });
        }

        public void makeWireFrame(int thickness, GraphicsDevice g)
        {
            dummyTexture = new Texture2D(g, this.getWidth(), this.getHeight());

            Color[,] textureData = new Color[this.getWidth(), this.getHeight()];

            for (int i = 0; i < this.getHeight(); ++i)
            {
                for (int j = 0; j < this.getWidth(); ++j)
                {
                    // wire-frame border gets the right color
                    if (i <= thickness || i >= this.getHeight() - thickness - 1 ||
                        j <= thickness || j >= this.getWidth() - thickness - 1)
                        textureData[j, i] = this.color;
                    // inner invisible part
                    else
                        textureData[j, i] = Color.Transparent;
                }
            }

            Color[] textureData1D = new Color[this.getWidth() * this.getHeight()];

            int index = 0;
            for (int i = 0; i < this.getHeight(); ++i)
                for (int j = 0; j < this.getWidth(); ++j)
                    textureData1D[index++] = textureData[j, i];

            dummyTexture.SetData(textureData1D);

            this.isWireFrame = true;
        }

        public void draw(SpriteBatch batch)
        {
            if (!this.isWireFrame)
                batch.Draw(dummyTexture, bounds, color);
            else
                batch.Draw(dummyTexture, position, Color.White);
        }
    }
}
