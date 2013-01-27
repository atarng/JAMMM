using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Actors;

namespace JAMMM
{
    /// <summary>
    /// Animation uses a sprite sheet and some parameters to control
    /// the iteration through the sheet and some properties (like whether
    /// or not to loop). Actors can have animations. 
    /// 
    /// Looping defaults to false and frame duration defaults to 1/10th of a second.
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// The texture this animation is using.
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        private Texture2D texture;

        /// <summary>
        /// The width of each frame in this animation. 
        /// 
        /// <para>
        /// This assumes that every frame will 
        /// have the same width.
        /// </para>
        /// </summary>
        public int FrameWidth
        {
            get { return frameWidth; }
        }
        private int frameWidth;

        /// <summary>
        /// The height of each frame in this animation.
        /// 
        /// <para>
        /// This assumes that every frame will have
        /// the same height.
        /// </para>
        /// </summary>
        public int FrameHeight
        {
            get { return frameHeight; }
        }
        private int frameHeight;

        /// <summary>
        /// The current index of this frame in the overall
        /// animation.
        /// </summary>
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        private int frameIndex;

        /// <summary>
        /// Whether or not this animation is playing.
        /// </summary>
        public bool IsPlaying
        {
            get { return isPlaying; }
        }
        private bool isPlaying;

        /// <summary>
        /// The type of animation. Actors will use this
        /// information.
        /// </summary>
        public Actor.AnimationType AnimationType
        {
            get { return animationType; }
        }
        private Actor.AnimationType animationType;

        private Rectangle currentFrame;
        private int frameCount;
        private bool isLooping;
        private float frameDuration;
        private float frameTime;
        private Actor owner;

        public Animation(Actor owner, 
                         Actor.AnimationType type, 
                         Texture2D spriteSheet, 
                         int frameCount, 
                         bool isLooping = false, 
                         float frameTime = 0.1f)
        {
            this.owner         = owner;
            this.animationType = type;
            this.texture       = spriteSheet;
            this.frameDuration = frameTime;
            this.isLooping     = isLooping;
            this.frameTime     = 0.0f;
            this.frameCount    = frameCount;
            this.isPlaying     = false;
            this.frameIndex    = 0;
            this.frameWidth    = spriteSheet.Width / frameCount;
            this.frameHeight   = spriteSheet.Height;

            this.currentFrame.Width = this.frameWidth;
            this.currentFrame.Height = this.frameHeight;
            this.currentFrame.Y = 0;
        }

        public void play()
        {
            this.isPlaying = true;
        }

        public void stop()
        {
            this.isPlaying  = false;
            this.frameTime   = 0.0f;
            this.frameIndex = 0;
        }

        public void pause()
        {
            this.isPlaying = false;
        }

        private void updateFrameRectangle()
        {
            this.currentFrame.X = this.frameIndex * this.frameWidth;
        }

        public void update(GameTime gameTime)
        {
            if (!isPlaying)
                return;

            this.frameTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (this.frameTime >= this.frameDuration)
            {
                this.frameTime -= this.frameDuration;

                ++this.frameIndex;

                if (this.frameIndex == this.frameCount)
                {
                    if (this.isLooping)
                        this.frameIndex = 0;
                    else
                    {
                        stop();
                        owner.handleAnimationComplete(this.animationType);
                    }
                }

                updateFrameRectangle();
            }
        }

        public void draw(SpriteBatch   spriteBatch, 
                         Vector2       position,
                         Color         color,
                         SpriteEffects   effects = SpriteEffects.None,
                         float rotation   = 0.0f,
                         float scale      = 1.0f)
        {
            Vector2 origin = Vector2.Zero;

            origin.X = this.frameWidth / 2.0f;
            origin.Y = this.frameHeight / 2.0f;

           
            spriteBatch.Draw(this.texture, 
                             position, 
                             this.currentFrame, 
                             color, rotation, 
                             origin, scale, 
                             effects, 
                             0.0f);
        }
    }
}
