using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JAMMM
{
    public class HealthBar
    {
        /// <summary>
        /// The number of bars in this instance.
        /// </summary>
        private int numBars;

        /// <summary>
        /// The number of health points per bar.
        /// </summary>
        private int barToHealthRatio;

        /// <summary>
        /// The current health of this bar.
        /// </summary>
        private int health;

        /// <summary>
        /// An array of colors for each bar.
        /// </summary>
        private Color[] colors;

        /// <summary>
        /// A border to contain the bars.
        /// </summary>
        private RectangleOverlay border;

        /// <summary>
        /// The thickness of our border.
        /// </summary>
        private int borderThickness;

        /// <summary>
        /// The bar rectangles themselves.
        /// </summary>
        private RectangleOverlay[] bars;

        private int maxWidth;
        private int maxHeight;

        public int totalWidth;
        public int totalHeight;

        private Texture2D blinker;
        private Color blinkColor;

        public HealthBar(int num, int ratio, int width, int height, int borderThickness)
        {
            colors = new Color[num];
            border = new RectangleOverlay(Rectangle.Empty, Color.White);
            bars = new RectangleOverlay[num];

            health = 0;
            barToHealthRatio = ratio;
            numBars = num;

            totalWidth = width;
            totalHeight = height;

            for (int i = 0; i < num; ++i)
                colors[i] = Color.White;
            for (int i = 0; i < num; ++i)
                bars[i] = new RectangleOverlay(Rectangle.Empty, 
                    Color.White);

            border.setColor(Color.Red);
            border.setWidth(width);
            border.setHeight(height);

            maxWidth = width - 2 * borderThickness;
            maxHeight = height - 2 * borderThickness;

            this.borderThickness = borderThickness;

            for (int i = 0; i < numBars; ++i)
            {
                bars[i].setWidth(width - 2 * borderThickness);
                bars[i].setHeight(height - 2 * borderThickness);
            }
        }

        public void loadContent(GraphicsDevice g)
        {
            border.loadContent(g);

            blinker = new Texture2D(g, 1, 1);
            blinkColor = new Color(140, 140, 140, 120);
            blinker.SetData(new Color[] { blinkColor });

            for (int i = 0; i < numBars; ++i)
                bars[i].loadContent(g);

            border.makeWireFrame(borderThickness, g);
        }

        /// <summary>
        /// Update the position of this healthbar in the world.
        /// </summary>
        public void updatePosition(float positionX, float positionY)
        {
            border.setPosition(positionX, positionY);

            for (int i = 0; i < numBars; ++i)
            {
                bars[i].setPosition(positionX + borderThickness,
                                    positionY + borderThickness);
            }
        }

        /// <summary>
        /// Sets all of the health bar colors.
        /// </summary>
        public void setColors(List<Color> colors)
        {
            System.Diagnostics.Trace.Assert(colors.Count == numBars);

            for (int i = 0; i < numBars; ++i)
                bars[i].setColor(colors.ElementAt(i));
        }

        /// <summary>
        /// Sets the new health value and calls the 
        /// private update bars so that it will draw properly.
        /// </summary>
        public void updateHealth(int newHealth)
        {
            this.health = newHealth;

            updateBars();
        }

        /// <summary>
        /// Updates the parameters of each bar to match
        /// the desired health.
        /// </summary>
        private void updateBars()
        {
            int copyOfHealth = health;

            float currSize = 0.0f;
            float currRatio = 1.0f;

            for (int i = 0; i < numBars; ++i)
            {
                currRatio = (float)copyOfHealth / (float)barToHealthRatio;

                if (currRatio > 1.0f)
                    currRatio = 1.0f;
                else if (currRatio < 0.0f)
                    currRatio = 0.0f;

                currSize = currRatio * maxWidth;

                bars[i].setWidth((int)currSize);

                copyOfHealth -= 100;
            }
        }

        /// <summary>
        /// Draws all of the bars starting at the highest one first.
        /// </summary>
        public void draw(SpriteBatch batch, bool blinking)
        {
            border.draw(batch);

            for (int i = 0; i < numBars; ++i)
                bars[i].draw(batch);

            if (blinking)
                batch.Draw(blinker, border.Bounds, blinkColor);
        }
    }
}
