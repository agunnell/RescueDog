using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace RescueDog
{
    public class Camera2D
    {
        public static GraphicsDevice graphics;

        public Matrix matRotation = Matrix.CreateRotationZ(0.0f);
        public Matrix matZoom;
        public Vector3 translateCenter;
        public Vector3 translateBody;
        public Matrix view;

        public Vector2 delta;
        public float distance;

        public float currentZoom = 1.0f;
        public float targetZoom = 1.0f;
        public float zoomSpeed = 0.05f;

        public float speed = 5f; //how fast the camera moves
        public Vector2 currentPosition;
        public Vector2 targetPosition;

        public Matrix projection;
        Vector3 T; Point t;
        public Point ConvertScreenToWorld(int x, int y)
        {   //converts screen position to world position
            projection = Matrix.CreateOrthographicOffCenter(0f, graphics.Viewport.Width, graphics.Viewport.Height, 0f, 0f, 1f);
            T.X = x; T.Y = y; T.Z = 0;
            T = graphics.Viewport.Unproject(T, projection, view, Matrix.Identity);
            t.X = (int)T.X; t.Y = (int)T.Y; return t;
        }
        public Point ConvertWorldToScreen(int x, int y)
        {   //converts world position to screen position
            projection = Matrix.CreateOrthographicOffCenter(0f, graphics.Viewport.Width, graphics.Viewport.Height, 0f, 0f, 1f);
            T.X = x; T.Y = y; T.Z = 0;
            T = graphics.Viewport.Project(T, projection, view, Matrix.Identity);
            t.X = (int)T.X; t.Y = (int)T.Y; return t;
        }


        public void SetView()
        {
            translateCenter.X = (int)graphics.Viewport.Width / 2f;
            translateCenter.Y = (int)graphics.Viewport.Height / 2f;
            translateCenter.Z = 0;

            translateBody.X = -currentPosition.X;
            translateBody.Y = -currentPosition.Y;
            translateBody.Z = 0;

            matZoom = Matrix.CreateScale(currentZoom, currentZoom, 1); //allows camera to properly zoom
            view = Matrix.CreateTranslation(translateBody) *
                    matRotation *
                    matZoom *
                    Matrix.CreateTranslation(translateCenter);
        }


        public Camera2D(GraphicsDevice Graphics)
        {
            graphics = Graphics;
            view = Matrix.Identity;
            translateCenter.Z = 0; //these two values dont change on a 2D camera
            translateBody.Z = 0;
            currentPosition = Vector2.Zero; //initially the camera is at 0,0
            targetPosition = Vector2.Zero;
            SetView();
        }


        public void Update(GameTime gameTime)
        {   //move the camera to the target position, match the target zoom
            delta = targetPosition - currentPosition;
            distance = delta.Length();
            //if camera is very close to target, then snap it to target
            if (distance < 1) { currentPosition = targetPosition; }
            else //camera is not close and should move according to speed
            { currentPosition += delta * speed * (float)gameTime.ElapsedGameTime.TotalSeconds; }
            //round current position down to whole number - this prevents tearing/artifacting of sprites
            currentPosition.X = (int)currentPosition.X;
            currentPosition.Y = (int)currentPosition.Y;
            if (currentZoom != targetZoom)
            {   //gradually match the zoom
                if (currentZoom > targetZoom) { currentZoom -= zoomSpeed; } //zoom out
                if (currentZoom < targetZoom) { currentZoom += zoomSpeed; } //zoom in
                if (Math.Abs((currentZoom - targetZoom)) < 0.05f) { currentZoom = targetZoom; } //limit zoom
            }
            SetView();
        }
    }
}
