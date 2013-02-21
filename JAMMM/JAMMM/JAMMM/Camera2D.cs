using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JAMMM.Actors;

namespace JAMMM
{
    public class Camera2D : GameComponent
    {
        public struct CameraBounds
        {
            public float leftBound;
            public float rightBound;
            public float upperBound;
            public float lowerBound;
        };

        private const float EPSILON = 0.00001f;

        private const float slowZoom = 0.0005f;
        private const float medZoom = 0.0005f;

        public const float MIN_ZOOM = 0.5f;
        public const float MED_ZOOM = 0.75f;
        public const float MAX_ZOOM = 1.0f;

        public float ZoomRatioX
        {
            get { return zoomRatioX; }
        }
        private float zoomRatioX;

        public float ZoomRatioY
        {
            get { return ZoomRatioY; }
        }
        private float zoomRatioY;

        public const int ZOOM_BUFFER = 150;

        public Rectangle maxView = Rectangle.Empty;
        public Rectangle medView = Rectangle.Empty;
        public Rectangle minView = Rectangle.Empty;
        public Rectangle spawnView = Rectangle.Empty;

        protected float         zoom; // Camera Zoom
        public Matrix           transform; // Matrix Transform
        public Vector2          pos; // Camera Position
        protected float         rotation; // Camera Rotation

        private CameraBounds bounds;
        public CameraBounds Bounds
        {
            get { return bounds; }
        }

        private Vector2 viewCenter;
        
        private Rectangle view;
        public Rectangle View
        {
            get { return view; }
        }

        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;

                if (zoom < MIN_ZOOM)
                    zoom = MIN_ZOOM;
                else if (zoom > MAX_ZOOM)
                    zoom = MAX_ZOOM;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        public Vector2 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public Camera2D(Game game) : base(game)
        {
            zoom = 1.0f;
            rotation = 0.0f;
            pos = Vector2.Zero;
            zoomRatioX = 0.0f;
            zoomRatioY = 0.0f;
        }

        public void initialize()
        {
            // initialize the maximum view size
            maxView.X = (int)(-0.5f * Game.GraphicsDevice.Viewport.Width);
            maxView.Y = (int)(-0.5f * Game.GraphicsDevice.Viewport.Height);
            maxView.Width = (int)(2.0f * Game.GraphicsDevice.Viewport.Width);
            maxView.Height = (int)(2.0f * Game.GraphicsDevice.Viewport.Height);

            // initialize the medium view size
            medView.X = (int)(-0.25f * Game.GraphicsDevice.Viewport.Width);
            medView.Y = (int)(-0.25f * Game.GraphicsDevice.Viewport.Height);
            medView.Width = (int)(1.5f * Game.GraphicsDevice.Viewport.Width);
            medView.Height = (int)(1.5f * Game.GraphicsDevice.Viewport.Height);

            // initialize the minimum view size
            minView.X = 0;
            minView.Y = 0;
            minView.Width = (int)(Game.GraphicsDevice.Viewport.Width);
            minView.Height = (int)(Game.GraphicsDevice.Viewport.Height);

            // initialize the minimum view size
            spawnView.X = (int)(0.2 * Game.GraphicsDevice.Viewport.Width);
            spawnView.Y = (int)(0.2 * Game.GraphicsDevice.Viewport.Height);
            spawnView.Width = (int)(0.6 * Game.GraphicsDevice.Viewport.Width);
            spawnView.Height = (int)(0.6 * Game.GraphicsDevice.Viewport.Height);

            zoomRatioX = 1.0f / (2.0f * (Game.GraphicsDevice.Viewport.Width));
            zoomRatioY = 1.0f / (2.0f * (Game.GraphicsDevice.Viewport.Height));
        }

        /// <summary>
        /// Updates the camera bounds for culling usage as well
        /// as the view rectangle to see where things are on-screen.
        /// </summary>
        public void updateBounds()
        {
            bounds.leftBound = 0 - (1.0f - zoom) * Game.GraphicsDevice.Viewport.Width;
            bounds.rightBound = Game.GraphicsDevice.Viewport.Width + -(bounds.leftBound);
            bounds.upperBound = 0 - (1.0f - zoom) * Game.GraphicsDevice.Viewport.Height;
            bounds.lowerBound = Game.GraphicsDevice.Viewport.Height + -(bounds.upperBound);

            view.X = (int)bounds.leftBound;
            view.Y = (int)bounds.upperBound;
            view.Width = (int)(bounds.rightBound - bounds.leftBound);
            view.Height = (int)(bounds.lowerBound - bounds.upperBound);

            viewCenter.X = view.Center.X;
            viewCenter.Y = view.Center.Y;
        }

        public void resetZoom()
        {
            Zoom = 1.0f;
        }

        public void zoomOut(float amount)
        {
            Zoom -= amount;
        }

        public void zoomIn(float amount)
        {
            Zoom += amount;
        }

        public void move(Vector2 amount)
        {
            pos += amount;
        }

        /// <summary>
        /// Using the velocity component and the direction,
        /// find the corresponding wall which we are passing
        /// through and use its zoom ratio along the direction
        /// (x or y) to obtain the amount to change the overall
        /// zoom.
        /// </summary>
        public void zoomOutByVelocity(float changeInPosition, bool isXAxis)
        {
            float changeInZoom = 0.0f;

            changeInPosition *= 2.0f;

            if (isXAxis)
                changeInZoom = zoomRatioX * changeInPosition;
            else
                changeInZoom = zoomRatioY * changeInPosition;

            Zoom -= changeInZoom;
        }

        /// <summary>
        /// Checking whether an actor is within bounds for culling purposes.
        /// </summary>
        public bool isInBounds(Actor a)
        {
            if (view.Contains(a.getBufferedRectangleBounds(0)))
                return true;
            return false;
        }

        /// <summary>
        /// Generic function taking a box itself instead
        /// of an Actor.
        /// </summary
        public bool isInBounds(Rectangle bounds)
        {
            if (view.Contains(bounds))
                return true;
            return false;
        }

        /// <summary>
        /// Returns the actor's distance from the center of the
        /// view.
        /// </summary>
        public float getDistanceFromViewCenter(Actor actor)
        {
            return Vector2.Subtract(actor.Position, viewCenter).Length();
        }

        /// <summary>
        /// Returns the maximum of 0.0f or the distance from an
        /// actor's position to the outer border of the camera
        /// view.
        /// </summary>
        public float getDistanceOutsideBorders(Actor actor)
        {
            Vector2 pos = actor.Position;

            // top or bottom
            if (pos.X >= bounds.leftBound &&
                pos.X <= bounds.rightBound)
            {
                // outside top border
                if (pos.Y < bounds.upperBound)
                    return bounds.upperBound - pos.Y;
                // outside bottom border
                else if (pos.Y > bounds.lowerBound)
                    return pos.Y - bounds.lowerBound;
                // in bounds
                else
                    return 0.0f;
            }
            // left or right
            else if (pos.Y >= bounds.upperBound &&
                pos.Y <= bounds.lowerBound)
            {
                // outside right border
                if (pos.X > bounds.rightBound)
                    return pos.X - bounds.rightBound;
                // outside left border
                else if (pos.X < bounds.leftBound)
                    return bounds.leftBound - pos.X;
                // in bounds
                else
                    return 0.0f;
            }
            // I don't know how it'd get here
            else return 0.0f;
        }

        #region INTERACTING_WITH_THE_LEVEL

        public void TryZoomOut(float gameTime, List<Penguin> players)
        {
            Actor furthestPlayer = null;

            Rectangle currView = View;

            float minDist = 500.0f,
                  minDistToLeft = 0.0f,
                  minDistToRight = 0.0f,
                  minDistToTop = 0.0f,
                  minDistToBottom = 0.0f;

            float distToLeft = 0.0f,
                  distToRight = 0.0f,
                  distToTop = 0.0f,
                  distToBottom = 0.0f,
                  minActorDist = 0.0f;

            float dPF = 0.0f;

            Vector2 dP = Vector2.Zero;

            Rectangle aBounds = Rectangle.Empty;

            foreach (Penguin p in players)
            {
                aBounds = p.getBufferedRectangleBounds(0);

                distToLeft = (float)Math.Abs(currView.Left - aBounds.Left);
                distToRight = (float)Math.Abs(currView.Right - aBounds.Right);
                distToTop = (float)Math.Abs(currView.Top - aBounds.Top);
                distToBottom = (float)Math.Abs(currView.Bottom - aBounds.Bottom);

                minActorDist = Math.Min(Math.Min(distToLeft, distToRight),
                                        Math.Min(distToTop, distToBottom));

                if (minActorDist < minDist)
                {
                    furthestPlayer = p;
                    dP = p.changeInPosition;
                    minDist = minActorDist;

                    minDistToLeft = distToLeft;
                    minDistToRight = distToRight;
                    minDistToTop = distToTop;
                    minDistToBottom = distToBottom;
                }
            }

            // this means we can zoom
            if (minDist <= Camera2D.ZOOM_BUFFER)
            {
                // crossing left boundary
                if (minDist == minDistToLeft)
                {
                    if (dP.X >= 0.0f) return;

                    dPF = Math.Abs(dP.X) + Camera2D.ZOOM_BUFFER - minDist;

                    zoomOutByVelocity(dPF, true);
                }
                // crossing right boundary
                else if (minDist == minDistToRight)
                {
                    if (dP.X <= 0.0f) return;

                    dPF = Math.Abs(dP.X) + Camera2D.ZOOM_BUFFER - minDist;

                    zoomOutByVelocity(dPF, true);
                }
                // crossing top boundary
                else if (minDist == minDistToTop)
                {
                    if (dP.Y >= 0.0f) return;

                    dPF = Math.Abs(dP.Y) + Camera2D.ZOOM_BUFFER - minDist;

                    zoomOutByVelocity(dPF, false);
                }
                // crossing bottom boundary
                else
                {
                    if (dP.Y <= 0.0f) return;

                    dPF = Math.Abs(dP.Y) + Camera2D.ZOOM_BUFFER - minDist;

                    zoomOutByVelocity(dPF, false);
                }
            }
        }

        public void TryZoomIn(float gameTime, List<Penguin> players)
        {
            bool allInBoundsMedium = true,
                 allInBoundsSmall = true;

            Rectangle aBounds = Rectangle.Empty;

            foreach (Penguin p in players)
            {
                aBounds = p.getBufferedRectangleBounds(0);

                if (!minView.Contains(aBounds))
                    allInBoundsSmall = false;
                if (!medView.Contains(aBounds))
                    allInBoundsMedium = false;
            }

            // control zoom speed
            if (allInBoundsSmall)
            {
                if (Zoom < 1.0f)
                {
                    zoomIn(slowZoom);
                }
            }
            else if (allInBoundsMedium)
            {
                if (Zoom < 0.75f)
                {
                    zoomIn(medZoom);
                }
            }
        }

        /// <summary>
        /// Get the transformation matrix for this camera to use in the 
        /// spriteBatch begin function.
        /// </summary>
        public Matrix getTransformation()
        {
            transform = 
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                    Matrix.CreateTranslation(new Vector3((Game.GraphicsDevice.Viewport.Width * 0.5f) - (pos.X * zoom),
                                                         (Game.GraphicsDevice.Viewport.Height * 0.5f) - (pos.Y * zoom),
                                                          0));
            return transform;
        }

        #endregion
    }
}
